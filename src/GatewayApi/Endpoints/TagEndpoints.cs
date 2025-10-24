using Contracts.DTOs;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Endpoints;

public static class TagEndpoints
{
    public static void MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags")
            .WithTags("Tags")
            .WithOpenApi();

        // GET /api/tags - List/search tags
        group.MapGet("/", async (
            [FromServices] UmlmmDbContext db,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = db.Tags
                .Include(t => t.ModelTags)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    CreatedAt = t.CreatedAt,
                    ModelCount = t.ModelTags.Count
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<TagDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        })
        .WithName("GetTags")
        .WithSummary("List and search tags")
        .Produces<PagedResult<TagDto>>(StatusCodes.Status200OK);

        // POST /api/tags/assign - Assign tag to model
        group.MapPost("/assign", async (
            [FromServices] UmlmmDbContext db,
            [FromBody] AssignTagRequest request) =>
        {
            if (request.ModelId <= 0 || request.TagId <= 0)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid request",
                    Detail = "ModelId and TagId must be positive integers.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var modelExists = await db.Models.AnyAsync(m => m.Id == request.ModelId);
            if (!modelExists)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Model not found",
                    Detail = $"Model with ID {request.ModelId} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var tagExists = await db.Tags.AnyAsync(t => t.Id == request.TagId);
            if (!tagExists)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Tag not found",
                    Detail = $"Tag with ID {request.TagId} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var existingAssignment = await db.ModelTags
                .AnyAsync(mt => mt.ModelId == request.ModelId && mt.TagId == request.TagId);

            if (existingAssignment)
            {
                return Results.Conflict(new ProblemDetails
                {
                    Title = "Tag already assigned",
                    Detail = $"Tag {request.TagId} is already assigned to model {request.ModelId}.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            var modelTag = new ModelTag
            {
                ModelId = request.ModelId,
                TagId = request.TagId,
                AssignedAt = DateTime.UtcNow
            };

            db.ModelTags.Add(modelTag);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("AssignTagToModel")
        .WithSummary("Assign a tag to a model")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        // DELETE /api/tags/remove - Remove tag from model
        group.MapDelete("/remove", async (
            [FromServices] UmlmmDbContext db,
            [FromBody] RemoveTagRequest request) =>
        {
            if (request.ModelId <= 0 || request.TagId <= 0)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid request",
                    Detail = "ModelId and TagId must be positive integers.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var modelTag = await db.ModelTags
                .FirstOrDefaultAsync(mt => mt.ModelId == request.ModelId && mt.TagId == request.TagId);

            if (modelTag == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Tag assignment not found",
                    Detail = $"Tag {request.TagId} is not assigned to model {request.ModelId}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            db.ModelTags.Remove(modelTag);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("RemoveTagFromModel")
        .WithSummary("Remove a tag from a model")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
