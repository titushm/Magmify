namespace Magmify.Models;

public class GithubRelease {
	[System.Text.Json.Serialization.JsonPropertyName("tag_name")]
	public string tag_name { get; set; } = string.Empty;

	[System.Text.Json.Serialization.JsonPropertyName("assets")]
	public List<ReleaseAsset>? assets { get; set; }
}

public class ReleaseAsset {
	[System.Text.Json.Serialization.JsonPropertyName("browser_download_url")]
	public string browser_download_url { get; set; } = string.Empty;

	[System.Text.Json.Serialization.JsonPropertyName("name")]
	public string? name { get; set; }
}