using System.Security.Cryptography;
using System.Text;
using UMLMM.Core.Domain.Entities;
using UMLMM.Core.Domain.Enums;
using UMLMM.E621Ingestor.Client.DTOs;

namespace UMLMM.E621Ingestor.Mapping;

public interface IE621Mapper
{
    Post MapToPost(E621Post apiPost, int sourceId);
    List<Tag> ExtractTags(E621Post apiPost);
    Image MapToImage(E621Post apiPost);
    Rating MapRating(string rating);
}

public class E621Mapper : IE621Mapper
{
    public Post MapToPost(E621Post apiPost, int sourceId)
    {
        return new Post
        {
            SourceId = sourceId,
            ExternalId = apiPost.Id.ToString(),
            Description = apiPost.Description,
            Rating = MapRating(apiPost.Rating),
            ExternalCreatedAt = apiPost.CreatedAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public List<Tag> ExtractTags(E621Post apiPost)
    {
        var tags = new List<Tag>();
        var allTags = new List<(string name, string category)>();

        allTags.AddRange(apiPost.Tags.General.Select(t => (t, "general")));
        allTags.AddRange(apiPost.Tags.Species.Select(t => (t, "species")));
        allTags.AddRange(apiPost.Tags.Character.Select(t => (t, "character")));
        allTags.AddRange(apiPost.Tags.Copyright.Select(t => (t, "copyright")));
        allTags.AddRange(apiPost.Tags.Artist.Select(t => (t, "artist")));
        allTags.AddRange(apiPost.Tags.Lore.Select(t => (t, "lore")));
        allTags.AddRange(apiPost.Tags.Meta.Select(t => (t, "meta")));

        foreach (var (name, category) in allTags)
        {
            tags.Add(new Tag
            {
                Name = name,
                Category = category,
                CreatedAt = DateTime.UtcNow
            });
        }

        return tags;
    }

    public Image MapToImage(E621Post apiPost)
    {
        var image = new Image
        {
            Url = apiPost.File.Url,
            SampleUrl = apiPost.Sample?.Url,
            Width = apiPost.File.Width,
            Height = apiPost.File.Height,
            FileSize = apiPost.File.Size,
            FileExtension = apiPost.File.Ext,
            CreatedAt = DateTime.UtcNow
        };

        // Convert MD5 to SHA256 format if available (e621 provides MD5)
        // Note: e621 API provides MD5, not SHA256. We store it as-is
        // In a real implementation, you might want to download and compute SHA256
        if (!string.IsNullOrWhiteSpace(apiPost.File.Md5))
        {
            // For now, we'll use MD5 as the hash identifier
            // Ideally, we'd download the file and compute SHA256
            image.Sha256 = apiPost.File.Md5;
        }

        return image;
    }

    public Rating MapRating(string rating)
    {
        return rating.ToLowerInvariant() switch
        {
            "s" or "safe" => Rating.Safe,
            "q" or "questionable" => Rating.Questionable,
            "e" or "explicit" => Rating.Explicit,
            _ => Rating.Safe
        };
    }
}
