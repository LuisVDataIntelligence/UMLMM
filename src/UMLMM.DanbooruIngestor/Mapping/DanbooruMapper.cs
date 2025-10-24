using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UMLMM.Domain.Entities;
using UMLMM.DanbooruIngestor.Danbooru;

namespace UMLMM.DanbooruIngestor.Mapping;

public class DanbooruMapper
{
    private const string SourceId = "danbooru";

    public Image MapToImage(DanbooruPostDto post)
    {
        var metadata = new
        {
            post.Score,
            post.UploaderId,
            post.Source,
            post.ImageWidth,
            post.ImageHeight,
            post.Md5,
            CreatedAt = post.CreatedAt
        };

        var image = new Image
        {
            SourceId = SourceId,
            ExternalId = post.Id.ToString(),
            Sha256 = ComputeSha256FromMd5(post.Md5),
            PreviewUrl = post.PreviewFileUrl,
            OriginalUrl = post.FileUrl ?? post.LargeFileUrl,
            Rating = MapRating(post.Rating),
            Metadata = JsonSerializer.Serialize(metadata),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return image;
    }

    public List<(string Name, string? Category)> ExtractTags(DanbooruPostDto post)
    {
        var tags = new List<(string Name, string? Category)>();

        if (!string.IsNullOrWhiteSpace(post.TagStringGeneral))
        {
            tags.AddRange(post.TagStringGeneral.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => (t.Trim(), (string?)"general")));
        }

        if (!string.IsNullOrWhiteSpace(post.TagStringCharacter))
        {
            tags.AddRange(post.TagStringCharacter.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => (t.Trim(), (string?)"character")));
        }

        if (!string.IsNullOrWhiteSpace(post.TagStringCopyright))
        {
            tags.AddRange(post.TagStringCopyright.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => (t.Trim(), (string?)"copyright")));
        }

        if (!string.IsNullOrWhiteSpace(post.TagStringArtist))
        {
            tags.AddRange(post.TagStringArtist.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => (t.Trim(), (string?)"artist")));
        }

        if (!string.IsNullOrWhiteSpace(post.TagStringMeta))
        {
            tags.AddRange(post.TagStringMeta.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => (t.Trim(), (string?)"meta")));
        }

        return tags.Where(t => !string.IsNullOrWhiteSpace(t.Name)).ToList();
    }

    private static string? ComputeSha256FromMd5(string? md5)
    {
        if (string.IsNullOrWhiteSpace(md5))
            return null;

        // For now, we store MD5 as a placeholder for SHA256
        // In production, you would download and hash the actual file
        // This is just to demonstrate the field usage
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes($"md5:{md5}");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string? MapRating(string? rating)
    {
        return rating?.ToLowerInvariant() switch
        {
            "g" => "general",
            "s" => "sensitive",
            "q" => "questionable",
            "e" => "explicit",
            _ => rating
        };
    }
}
