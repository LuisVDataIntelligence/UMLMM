using System.Text.Json.Serialization;

namespace UMLMM.DanbooruIngestor.Danbooru;

public class DanbooruPostDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = default!;
    
    [JsonPropertyName("uploader_id")]
    public long UploaderId { get; set; }
    
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    [JsonPropertyName("md5")]
    public string? Md5 { get; set; }
    
    [JsonPropertyName("rating")]
    public string? Rating { get; set; }
    
    [JsonPropertyName("image_width")]
    public int ImageWidth { get; set; }
    
    [JsonPropertyName("image_height")]
    public int ImageHeight { get; set; }
    
    [JsonPropertyName("tag_string")]
    public string TagString { get; set; } = default!;
    
    [JsonPropertyName("tag_string_general")]
    public string? TagStringGeneral { get; set; }
    
    [JsonPropertyName("tag_string_character")]
    public string? TagStringCharacter { get; set; }
    
    [JsonPropertyName("tag_string_copyright")]
    public string? TagStringCopyright { get; set; }
    
    [JsonPropertyName("tag_string_artist")]
    public string? TagStringArtist { get; set; }
    
    [JsonPropertyName("tag_string_meta")]
    public string? TagStringMeta { get; set; }
    
    [JsonPropertyName("file_url")]
    public string? FileUrl { get; set; }
    
    [JsonPropertyName("large_file_url")]
    public string? LargeFileUrl { get; set; }
    
    [JsonPropertyName("preview_file_url")]
    public string? PreviewFileUrl { get; set; }
}
