using System.IO;
using Magmify.Models;
using Wpf.Ui.Appearance;

namespace Magmify.Services;

public class HelperService {
	private static HelperService? _instance;
	private static readonly object InstanceLock = new();


	public static HelperService Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new HelperService();
				}

				return _instance;
			}
		}
	}
	// public static void RegisterZoomHotkey() {
	// 	var keybinding = Config.ZoomKeybinding;
	// 	if (keybinding == null) {
	// 		HotkeyManager.Current.Remove("Zoom");
	// 		return;
	// 	}
	//
	// 	Key key = KeyInterop.KeyFromVirtualKey(keybinding.VKey);
	// 	Modifiers modifiers = keybinding.Modifiers;
	// 	ModifierKeys keys = ModifierKeys.None;
	// 	if (modifiers.Ctrl) keys |= ModifierKeys.Control;
	// 	if (modifiers.Alt) keys |= ModifierKeys.Alt;
	// 	if (modifiers.Shift) keys |= ModifierKeys.Shift;
	// 	if (modifiers.Win) keys |= ModifierKeys.Windows; // Probably a better way to do this. idc
	//
	// 	HotkeyManager.Current.AddOrReplace(
	// 		"Zoom",
	// 		key,
	// 		keys,
	// 		(sender, e) => {
	// 			// Perform the hotkey action here
	// 			// MessageBox.Show("Hotkey pressed!");
	// 			e.Handled = false; // <-- this allows the key press to go through
	// 		}); //TODO: NHotkey cant do non blocking... Create custom solution later
	// 	//TODO: HANDLE GLOBAL APPLICATYION EXCEPTIONS SOMEHOW
	// }


	public void ApplyLanguage(string cultureCode) {
		try {
			string newLanguage = $"Resources/Strings/{cultureCode}.xaml";
			List<string> oldResources = ResourceSwitcherService.Instance
				.OppositeResources(newLanguage, Info.SupportedLanguages.Keys
					.Select(code => $"Resources/Strings/{code}.xaml").ToList());
			ResourceSwitcherService.Instance.SwitchResources(oldResources, [newLanguage]);
		} catch (Exception ex) {
			LogService.Instance.Write(
				$"Error setting language to {cultureCode}. Exception: {ex.Message}\n{ex.StackTrace}");
		}
	}


	public void ApplyTheme(bool isDark) {
		try {
			ApplicationTheme newTheme = isDark ? ApplicationTheme.Dark : ApplicationTheme.Light;

			List<string> oldResources = ResourceSwitcherService.Instance
				.OppositeResources(Info.SupportedThemes[newTheme], Info.SupportedThemes.Values.ToList());

			ResourceSwitcherService.Instance.SwitchResources(
				[
					..oldResources, Info.IconsResource
				], // Icons must be added after the theme resource as they use its colors
				[Info.SupportedThemes[newTheme], Info.IconsResource]
			);

			ApplicationThemeManager.Apply(newTheme);
		} catch (Exception ex) {
			LogService.Instance.Write(
				$"Error setting theme to {(isDark ? "dark" : "light")} theme. Exception: {ex.Message}\n{ex.StackTrace}");
		}
	}

	public static void EnsureAppDirectory() {
		if (!Directory.Exists(Info.AppDirectory)) {
			Directory.CreateDirectory(Info.AppDirectory);
		}
	}
}