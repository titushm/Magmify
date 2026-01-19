using System.Net.Http;
using System.Runtime.Versioning;
using Magmify.Models;

namespace Magmify.Services;

[SupportedOSPlatform("windows7.0")]
public class Registry {
	private static readonly HttpClient HttpClient = new();
		
	private static async Task<Pointers?> GetVersionPointers(Version version) {
		var response = await HttpClient.GetAsync(Constants.RegistryRawUrl + $"/versions/{version}");
		if (!response.IsSuccessStatusCode) {
			Logger.Instance.Write("Failed to fetch pointers from registry: " + response.StatusCode);
			return null;
		}
		var content = await response.Content.ReadAsStringAsync();
		var pointers = System.Text.Json.JsonSerializer.Deserialize<Pointers>(content);
		return pointers;
	}
	
	public static async Task UpdatePointers() {
		var version = Minecraft.Instance.GetVersion() ?? new Version(0, 0, 0);
		var database = Database.GetDatabase();
		if (database.pointers.TryGetValue(version.ToString(), out _)) {
			Logger.Instance.Write($"Pointers for version {version} already exist in database");
			return;
		}

		try {
			var pointers = await GetVersionPointers(version);
			
			if (pointers == null) {
				Logger.Instance.Write($"No pointers found for version {version}");
				return;
			}
		
			Database.SetVersionPointers(version, pointers);
			Logger.Instance.Write($"Fetched pointers for version {version} from registry");
		} catch (Exception e) {
			Logger.Instance.Write($"Failed to fetch pointers for version {version} from registry: {e.Message}");
		}
	}
}