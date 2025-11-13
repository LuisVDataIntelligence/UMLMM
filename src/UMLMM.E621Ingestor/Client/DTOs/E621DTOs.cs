using System.Text.Json.Serialization;

namespace UMLMM.E621Ingestor.Client.DTOs;

public class E621PostResponse
{
    [JsonPropertyName("posts")]
    public List<E621Post> Posts { get; set; } = new();
}

public class E621Post
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("rating")]
    public string Rating { get; set; } = string.Empty;
    
    [JsonPropertyName("tags")]
    public E621Tags Tags { get; set; } = new();
    
    [JsonPropertyName("file")]
    public E621File File { get; set; } = new();
    
    [JsonPropertyName("preview")]
    public E621Preview? Preview { get; set; }
    
    [JsonPropertyName("sample")]
    public E621Sample? Sample { get; set; }
}

public class E621Tags
{
    [JsonPropertyName("general")]
    public List<string> General { get; set; } = new();
    
    [JsonPropertyName("species")]
    public List<string> Species { get; set; } = new();
    
    [JsonPropertyName("character")]
    public List<string> Character { get; set; } = new();
    
    [JsonPropertyName("copyright")]
    public List<string> Copyright { get; set; } = new();
    
    [JsonPropertyName("artist")]
    public List<string> Artist { get; set; } = new();
    
    [JsonPropertyName("invalid")]
    public List<string> Invalid { get; set; } = new();
    
    [JsonPropertyName("lore")]
    public List<string> Lore { get; set; } = new();
    
    [JsonPropertyName("meta")]
    public List<string> Meta { get; set; } = new();
}

public class E621File
{
    [JsonPropertyName("width")]
    public int? Width { get; set; }
    
    [JsonPropertyName("height")]
    public int? Height { get; set; }
    
    [JsonPropertyName("ext")]
    public string? Ext { get; set; }
    
    [JsonPropertyName("size")]
    public long? Size { get; set; }
    
    [JsonPropertyName("md5")]
    public string? Md5 { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class E621Preview
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class E621Sample
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [JsonPropertyName("width")]
    public int? Width { get; set; }
    
    [JsonPropertyName("height")]
    public int? Height { get; set; }
}
