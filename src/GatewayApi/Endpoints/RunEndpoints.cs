using Contracts.DTOs;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Endpoints;

public static class RunEndpoints
{
    public static void MapRunEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/runs")
            .WithTags("Runs")
            .WithOpenApi();

        // GET /api/runs - List latest runs with pagination
        group.MapGet("/", async (
            [FromServices] UmlmmDbContext db,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = db.Runs.AsQueryable();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.StartedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RunDto
                {
                    Id = r.Id,
                    WorkflowName = r.WorkflowName,
                    Status = r.Status,
                    StartedAt = r.StartedAt,
                    CompletedAt = r.CompletedAt,
                    ResultData = r.ResultData,
                    ErrorMessage = r.ErrorMessage
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<RunDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        })
        .WithName("GetRuns")
        .WithSummary("List latest runs with optional status filter")
        .Produces<PagedResult<RunDto>>(StatusCodes.Status200OK);

        // GET /api/runs/{id} - Get run by ID
        group.MapGet("/{id:int}", async (
            [FromServices] UmlmmDbContext db,
            [FromRoute] int id) =>
        {
            var run = await db.Runs
                .FirstOrDefaultAsync(r => r.Id == id);

            if (run == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Run not found",
                    Detail = $"Run with ID {id} was not found.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var dto = new RunDto
            {
                Id = run.Id,
                WorkflowName = run.WorkflowName,
                Status = run.Status,
                StartedAt = run.StartedAt,
                CompletedAt = run.CompletedAt,
                ResultData = run.ResultData,
                ErrorMessage = run.ErrorMessage
            };

            return Results.Ok(dto);
        })
        .WithName("GetRunById")
        .WithSummary("Get run by ID")
        .Produces<RunDto>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}
