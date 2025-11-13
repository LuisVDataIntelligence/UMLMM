using GatewayApi.Endpoints;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "UMLMM Gateway API",
        Version = "v1",
        Description = "Gateway API for Unified Model/Media Metadata - Core endpoints for Models, Versions, Tags, Images, and Runs"
    });
});

// Configure DbContext with In-Memory database
builder.Services.AddDbContext<UmlmmDbContext>(options =>
    options.UseInMemoryDatabase("UmlmmDb"));

var app = builder.Build();

// Seed database with sample data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UmlmmDbContext>();
    SeedDatabase(db);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "UMLMM Gateway API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();

// Map API endpoints
app.MapModelEndpoints();
app.MapModelVersionEndpoints();
app.MapTagEndpoints();
app.MapImageEndpoints();
app.MapRunEndpoints();

app.Run();

// Seed method
static void SeedDatabase(UmlmmDbContext db)
{
    // Ensure database is created and check if already seeded
    db.Database.EnsureCreated();
    if (db.Models.Any()) return; // Already seeded

    var now = DateTime.UtcNow;

    // Create tags
    var tags = new[]
    {
        new Infrastructure.Entities.Tag { Name = "Stable Diffusion", CreatedAt = now },
        new Infrastructure.Entities.Tag { Name = "Anime", CreatedAt = now },
        new Infrastructure.Entities.Tag { Name = "Realistic", CreatedAt = now },
        new Infrastructure.Entities.Tag { Name = "LLM", CreatedAt = now }
    };
    db.Tags.AddRange(tags);
    db.SaveChanges();

    // Create models
    var models = new[]
    {
        new Infrastructure.Entities.Model
        {
            Name = "Stable Diffusion XL",
            Description = "Latest SDXL base model",
            Type = "Checkpoint",
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now.AddDays(-5)
        },
        new Infrastructure.Entities.Model
        {
            Name = "Anime Diffusion",
            Description = "Anime-focused generative model",
            Type = "LoRA",
            CreatedAt = now.AddDays(-20),
            UpdatedAt = now.AddDays(-3)
        }
    };
    db.Models.AddRange(models);
    db.SaveChanges();

    // Create model-tag associations
    db.ModelTags.AddRange(
        new Infrastructure.Entities.ModelTag { ModelId = models[0].Id, TagId = tags[0].Id, AssignedAt = now },
        new Infrastructure.Entities.ModelTag { ModelId = models[0].Id, TagId = tags[2].Id, AssignedAt = now },
        new Infrastructure.Entities.ModelTag { ModelId = models[1].Id, TagId = tags[0].Id, AssignedAt = now },
        new Infrastructure.Entities.ModelTag { ModelId = models[1].Id, TagId = tags[1].Id, AssignedAt = now }
    );
    db.SaveChanges();

    // Create versions
    var versions = new[]
    {
        new Infrastructure.Entities.ModelVersion
        {
            ModelId = models[0].Id,
            VersionName = "1.0",
            Description = "Initial release",
            DownloadUrl = "https://example.com/sdxl-1.0.safetensors",
            FileSizeBytes = 6_900_000_000,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now.AddDays(-30)
        },
        new Infrastructure.Entities.ModelVersion
        {
            ModelId = models[0].Id,
            VersionName = "1.1",
            Description = "Improved quality",
            DownloadUrl = "https://example.com/sdxl-1.1.safetensors",
            FileSizeBytes = 6_900_000_000,
            CreatedAt = now.AddDays(-5),
            UpdatedAt = now.AddDays(-5)
        },
        new Infrastructure.Entities.ModelVersion
        {
            ModelId = models[1].Id,
            VersionName = "2.0",
            Description = "Enhanced anime style",
            DownloadUrl = "https://example.com/anime-2.0.safetensors",
            FileSizeBytes = 144_000_000,
            CreatedAt = now.AddDays(-3),
            UpdatedAt = now.AddDays(-3)
        }
    };
    db.ModelVersions.AddRange(versions);
    db.SaveChanges();

    // Create images
    var images = new[]
    {
        new Infrastructure.Entities.Image
        {
            ModelVersionId = versions[0].Id,
            Url = "https://example.com/images/sample1.png",
            Hash = "abc123def456",
            Width = 1024,
            Height = 1024,
            Rating = 5,
            CreatedAt = now.AddDays(-29)
        },
        new Infrastructure.Entities.Image
        {
            ModelVersionId = versions[1].Id,
            Url = "https://example.com/images/sample2.png",
            Hash = "def456ghi789",
            Width = 1024,
            Height = 1024,
            Rating = 4,
            CreatedAt = now.AddDays(-4)
        },
        new Infrastructure.Entities.Image
        {
            ModelVersionId = versions[2].Id,
            Url = "https://example.com/images/anime1.png",
            Hash = "ghi789jkl012",
            Width = 512,
            Height = 768,
            Rating = 5,
            CreatedAt = now.AddDays(-2)
        }
    };
    db.Images.AddRange(images);
    db.SaveChanges();

    // Create runs
    var runs = new[]
    {
        new Infrastructure.Entities.Run
        {
            WorkflowName = "CivitAI Sync",
            Status = "Completed",
            StartedAt = now.AddHours(-2),
            CompletedAt = now.AddHours(-1),
            ResultData = "{\"modelsProcessed\": 150}"
        },
        new Infrastructure.Entities.Run
        {
            WorkflowName = "Danbooru Sync",
            Status = "Running",
            StartedAt = now.AddMinutes(-30),
            CompletedAt = null
        },
        new Infrastructure.Entities.Run
        {
            WorkflowName = "ComfyUI Workflow",
            Status = "Failed",
            StartedAt = now.AddHours(-5),
            CompletedAt = now.AddHours(-4),
            ErrorMessage = "Connection timeout"
        }
    };
    db.Runs.AddRange(runs);
    db.SaveChanges();
}

// Make the Program class accessible to tests
public partial class Program { }
