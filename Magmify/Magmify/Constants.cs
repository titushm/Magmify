using System.Runtime.Versioning;
using Wpf.Ui.Appearance;

namespace Magmify;

[SupportedOSPlatform("windows7.0")]
public static class Constants {
	public const string AppName = "Magmify";
	public const string AppGuid = "43f4cd6f-4faf-409e-95c4-5aaa6fb67cb1";
	public const string AppVersion = "1.0.1";
	public const string AppStartupName = "Magmify Startup";
	public const string RepoUrl = "https://api.github.com/repos/titushm/Magmify";
	public const string MinecraftProcessName = "Minecraft.Windows";
	public const string ModuleName = "Minecraft.Windows.exe";
	public const string MinecraftInstallPath = "C:\\XboxGames\\Minecraft for Windows\\Content";
	public static readonly string AppDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "titushm",
		"Magmify");

	public const string RegistryRawUrl = "https://raw.githubusercontent.com/titushm/magmify-registry/refs/heads/main";
	
	public static readonly string DatabasePath = System.IO.Path.Combine(AppDirectory, "db.toml");
	public static readonly string LogPath = System.IO.Path.Combine(AppDirectory, "log.txt");
	public const string IconsResource = "Resources/Assets/Icons.xaml";

	public static readonly Dictionary<string, string> SupportedLanguages = new() {
		{ "en", "English" },
		{ "es", "Español" },
		{ "fr", "Français" },
		{ "de", "Deutsch" },
		{ "zh", "中文" },
		{ "ja", "日本語" },
		{ "ru", "Русский" }
	};

	public static readonly Dictionary<ApplicationTheme, string> SupportedThemes = new() {
		{ ApplicationTheme.Light, "Resources/Themes/Light.xaml" },
		{ ApplicationTheme.Dark, "Resources/Themes/Dark.xaml" }
	};
	
	public static readonly double[] AnimationFameTimes = new[] {
		1000.0 / 15.0,
		1000.0 / 30.0,
		1000.0 / 60.0,
		1000.0 / 120.0,
		1000.0 / 165.0,
		1000.0 / 240.0
	};
}