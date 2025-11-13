using Contracts.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Endpoints;

public static class ModelEndpoints
{
    public static void MapModelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/models")
            .WithTags("Models")
            .WithOpenApi();

        // GET /api/models - List/search models with pagination
        group.MapGet("/", async (
            [FromServices] UmlmmDbContext db,
            [FromQuery] string? search,
            [FromQuery] string? type,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = db.Models
                .Include(m => m.Versions)
                .Include(m => m.ModelTags)
                    .ThenInclude(mt => mt.Tag)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => 
                    m.Name.Contains(search) || 
                    (m.Description != null && m.Description.Contains(search)));
            }

            // Apply type filter
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(m => m.Type == type);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ModelDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Type = m.Type,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    VersionCount = m.Versions.Count,
                    Tags = m.ModelTags.Select(mt => mt.Tag.Name).ToList()
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<ModelDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        })
        .WithName("GetModels")
        .WithSummary("List and search models with pagination")
        .Produces<PagedResult<ModelDto>>(StatusCodes.Status200OK);

        // GET /api/models/{id} - Get model by ID with version summaries
        group.MapGet("/{id:int}", async (
            [FromServices] UmlmmDbContext db,
            [FromRoute] int id) =>
        {
            var model = await db.Models
                .Include(m => m.Versions)
                .Include(m => m.ModelTags)
                    .ThenInclude(mt => mt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Model not found",
                    Detail = $"Model with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var modelDetail = new ModelDetailDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Type = model.Type,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                VersionCount = model.Versions.Count,
                Tags = model.ModelTags.Select(mt => mt.Tag.Name).ToList(),
                Versions = model.Versions
                    .OrderByDescending(v => v.CreatedAt)
                    .Select(v => new ModelVersionSummaryDto
                    {
                        Id = v.Id,
                        VersionName = v.VersionName,
                        CreatedAt = v.CreatedAt
                    })
                    .ToList()
            };

            return Results.Ok(modelDetail);
        })
        .WithName("GetModelById")
        .WithSummary("Get model by ID with version summaries")
        .Produces<ModelDetailDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
