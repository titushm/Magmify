using System.IO;
using System.Runtime.Versioning;
using Magmify.Models;
using Tomlyn;

namespace Magmify.Services;

[SupportedOSPlatform("windows7.0")]
public static class Database {
	private static readonly object FileLock = new();
	private static AppDatabaseModel? _cache;
	
	private static void Ensure() {
		if (!Directory.Exists(Constants.AppDirectory)) Directory.CreateDirectory(Constants.AppDirectory);
		if (!File.Exists(Constants.DatabasePath)) {
			File.Create(Constants.DatabasePath).Close();
			var toml = Toml.FromModel(AppDatabaseModel.Default);
			File.WriteAllText(Constants.DatabasePath, toml);
		}
	}

	private static void RecreateDatabase() {
		Logger.Instance.Write("Database is corrupted. Recreating...");
		_cache = AppDatabaseModel.Default;
		SaveDatabase(_cache);
	}
	
	
	private static void LoadDatabase() {
		Ensure();
		lock (FileLock) {
			var toml = File.ReadAllText(Constants.DatabasePath);
			
			try {
				_cache = Toml.ToModel<AppDatabaseModel>(toml);
			} catch {
				RecreateDatabase();
				return;
			}
			
			var validator = new AppDatabaseModelValidator();
			var result = validator.Validate(_cache);
			if (!result.IsValid) {
				Logger.Instance.Write("Database validation failed: " + string.Join(", ", result.Errors));
				RecreateDatabase();
			}
		}
	}
	
	public static AppDatabaseModel GetDatabase() {
		if (_cache != null) return _cache;
		LoadDatabase();
		return _cache!;
	}
	
	
	public static void SaveDatabase(AppDatabaseModel database) {
		Ensure();
		lock (FileLock) {
			var toml = Toml.FromModel(database);
			File.WriteAllText(Constants.DatabasePath, toml);
		}
	}
	
	public static void SetConfigProperty<T>(string property, T value) {
		var database = GetDatabase();
		database.config.GetType().GetProperty(property)?.SetValue(database.config, value);
		SaveDatabase(database);
	}
	
	public static void SetVersionPointers(Version version, Pointers pointers) {
		var database = GetDatabase();
		database.pointers[version.ToString()] = pointers;
		SaveDatabase(database);
	}
}