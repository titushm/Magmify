using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Input;
using Magmify.Models;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

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
	
	public void CheckForUpdates() {
		try {
			HttpClient client = new();
			client.DefaultRequestHeaders.UserAgent.ParseAdd(Info.AppName);
			GithubRelease? response = client.GetFromJsonAsync<GithubRelease>(Info.RepoUrl + "/releases/latest").Result;
			if (response == null) return;
			Version latestVersion = new(response.Tag_Name.TrimStart('v'));
			if (latestVersion == Config.LastNotifiedVersion) return;
			Version currentVersion = new(Info.AppVersion);
			if (latestVersion > currentVersion) {
				MessageBox msgBox = new() {
					Title = "Update Available",
					Content = $"A new version of {Info.AppName} is available!\n\n" +
					          $"Current version: {Info.AppVersion}\n" +
					          $"Latest version: {latestVersion}\n\n" +
					          "Would you like to download the latest version?",
					IsPrimaryButtonEnabled = true,
					PrimaryButtonText = "Download",
					IsSecondaryButtonEnabled = true,
					SecondaryButtonText = "Skip Update"
				};
				MessageBoxResult result = msgBox.ShowDialogAsync().Result;
				if (result == MessageBoxResult.Primary) {
					Process.Start(new ProcessStartInfo {
						FileName = response.Assets[0].Browser_Download_Url,
						UseShellExecute = true
					});
				} else if (result == MessageBoxResult.Secondary) {
					Config.LastNotifiedVersion = latestVersion;
				}
			}
		} catch {
			LogService.Instance.Write("Update check failed.");
		}
	}

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