using System.IO;
using System.Text.Json;
using Magmify.Models;

namespace Magmify.Services;

public sealed class ConfigService {
	private static ConfigService? _instance;
	private static readonly object InstanceLock = new();
	private static readonly object FileLock = new();
	private static Dictionary<string, JsonElement>? _cache;
	private readonly JsonSerializerOptions _jsonOptions = new() {
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
	};

	private ConfigService() {
		HelperService.EnsureAppDirectory();
	}

	public static ConfigService Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new ConfigService();
				}

				return _instance;
			}
		}
	}

	private void Load() {
		lock (FileLock) {
			if (_cache != null) return;

			if (!File.Exists(Info.ConfigPath)) {
				_cache = new Dictionary<string, JsonElement>();
				return;
			}

			string json = File.ReadAllText(Info.ConfigPath);
			_cache = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ??
			         new Dictionary<string, JsonElement>();
		}
	}

	private void Save() {
		lock (FileLock) {
			string json = JsonSerializer.Serialize(_cache, _jsonOptions);

			File.WriteAllText(Info.ConfigPath, json);
		}
	}

	public T Get<T>(string key, T defaultValue = default!) {
		Load();
		if (_cache!.TryGetValue(key, out JsonElement elem)) {
			try {
				return elem.Deserialize<T>(_jsonOptions)!;
			} catch {
				return defaultValue;
			}
		}

		return defaultValue;
	}

	public void Set<T>(string key, T value) {
		Load();
		_cache![key] = JsonSerializer.SerializeToElement(value, _jsonOptions);
		Save();
	}
}