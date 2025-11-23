using Wpf.Ui.Appearance;

namespace Magmify.Models;

public static class Info {
	public static string AppName => "Magmify";
	public static string AppVersion => "1.0.0";
	public static string AppStartupName => "Magmify Startup";
	public static string AppDirectory =>
		System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "titushm",
			"Magmify");

	public static readonly string ConfigPath = System.IO.Path.Combine(AppDirectory, "config.json");
	public static readonly string LogPath = System.IO.Path.Combine(AppDirectory, "log.txt");
	public const string StringsDir = "Resources/Strings/";
	public const string IconsResource = "Resources/Assets/Icons.xaml";

	public static readonly Dictionary<string, List<int>> KeybindingModifiers = new() {
		{ "Shift", [0xA0, 0xA1] },
		{ "Ctrl", [0xA2, 0xA3] },
		{ "Alt", [0xA4, 0xA5] },
		{ "Win", [0x5B, 0x5C] }
	};

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
}