using Contracts.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Endpoints;

public static class ImageEndpoints
{
    public static void MapImageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/images")
            .WithTags("Images")
            .WithOpenApi();

        // GET /api/images - List/search images with pagination
        group.MapGet("/", async (
            [FromServices] UmlmmDbContext db,
            [FromQuery] int? modelVersionId,
            [FromQuery] string? hash,
            [FromQuery] int? rating,
            [FromQuery] int? minRating,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = db.Images.AsQueryable();

            // Apply filters
            if (modelVersionId.HasValue)
            {
                query = query.Where(i => i.ModelVersionId == modelVersionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(hash))
            {
                query = query.Where(i => i.Hash == hash);
            }

            if (rating.HasValue)
            {
                query = query.Where(i => i.Rating == rating.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(i => i.Rating >= minRating.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new ImageDto
                {
                    Id = i.Id,
                    ModelVersionId = i.ModelVersionId,
                    ModelId = i.ModelId,
                    Url = i.Url,
                    Hash = i.Hash,
                    Width = i.Width,
                    Height = i.Height,
                    Rating = i.Rating,
                    Metadata = i.Metadata,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<ImageDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        })
        .WithName("GetImages")
        .WithSummary("List and search images by model version, hash, or rating")
        .Produces<PagedResult<ImageDto>>(StatusCodes.Status200OK);

        // GET /api/images/{id} - Get image by ID
        group.MapGet("/{id:int}", async (
            [FromServices] UmlmmDbContext db,
            [FromRoute] int id) =>
        {
            var image = await db.Images
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Image not found",
                    Detail = $"Image with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var dto = new ImageDto
            {
                Id = image.Id,
                ModelVersionId = image.ModelVersionId,
                ModelId = image.ModelId,
                Url = image.Url,
                Hash = image.Hash,
                Width = image.Width,
                Height = image.Height,
                Rating = image.Rating,
                Metadata = image.Metadata,
                CreatedAt = image.CreatedAt
            };

            return Results.Ok(dto);
        })
        .WithName("GetImageById")
        .WithSummary("Get image by ID")
        .Produces<ImageDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
