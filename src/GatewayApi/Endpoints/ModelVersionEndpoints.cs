using Contracts.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Endpoints;

public static class ModelVersionEndpoints
{
    public static void MapModelVersionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/model-versions")
            .WithTags("Model Versions")
            .WithOpenApi();

        // GET /api/model-versions/{id} - Get version by ID
        group.MapGet("/{id:int}", async (
            [FromServices] UmlmmDbContext db,
            [FromRoute] int id) =>
        {
            var version = await db.ModelVersions
                .Include(v => v.Model)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (version == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Model version not found",
                    Detail = $"Model version with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var dto = new ModelVersionDto
            {
                Id = version.Id,
                ModelId = version.ModelId,
                ModelName = version.Model.Name,
                VersionName = version.VersionName,
                Description = version.Description,
                DownloadUrl = version.DownloadUrl,
                FileSizeBytes = version.FileSizeBytes,
                CreatedAt = version.CreatedAt,
                UpdatedAt = version.UpdatedAt
            };

            return Results.Ok(dto);
        })
        .WithName("GetModelVersionById")
        .WithSummary("Get model version by ID")
        .Produces<ModelVersionDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // GET /api/model-versions/by-model/{modelId} - List versions by model
        group.MapGet("/by-model/{modelId:int}", async (
            [FromServices] UmlmmDbContext db,
            [FromRoute] int modelId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var modelExists = await db.Models.AnyAsync(m => m.Id == modelId);
            if (!modelExists)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Model not found",
                    Detail = $"Model with ID {modelId} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var query = db.ModelVersions
                .Include(v => v.Model)
                .Where(v => v.ModelId == modelId);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new ModelVersionDto
                {
                    Id = v.Id,
                    ModelId = v.ModelId,
                    ModelName = v.Model.Name,
                    VersionName = v.VersionName,
                    Description = v.Description,
                    DownloadUrl = v.DownloadUrl,
                    FileSizeBytes = v.FileSizeBytes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<ModelVersionDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        })
        .WithName("GetModelVersionsByModel")
        .WithSummary("List versions for a specific model")
        .Produces<PagedResult<ModelVersionDto>>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
