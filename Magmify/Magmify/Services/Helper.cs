using System.Diagnostics;
using System.Windows;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Versioning;
using Windows.Media.Capture;
using Magmify.Models;
using Wpf.Ui.Appearance;

namespace Magmify.Services;

[SupportedOSPlatform("windows7.0")]
public static class Helper {
	public static EventHandler? OnUpdateStatusLabel;

	public static async Task CheckForUpdatesAsync() {
		try {
			using HttpClient client = new();
			client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.AppName);

			var options = new System.Text.Json.JsonSerializerOptions {
				PropertyNameCaseInsensitive = true
			};

			GithubRelease? response = await client.GetFromJsonAsync<GithubRelease>(Constants.RepoUrl + "/releases/latest", options);
			if (response == null) return;

			Version latestVersion = new(response.tag_name.TrimStart('v'));
			if (latestVersion == Config.LastNotifiedVersion) return;

			Version currentVersion = new(Constants.AppVersion);
			if (latestVersion > currentVersion) {
				Wpf.Ui.Controls.MessageBox msgBox = new() {
					Title = "Update Available",
					Content = $"A new version of {Constants.AppName} is available!\n\n" +
					          $"Current version: {Constants.AppVersion}\n" +
					          $"Latest version: {latestVersion}\n\n" +
					          "Would you like to download the latest version?",
					IsPrimaryButtonEnabled = true,
					PrimaryButtonText = "Download",
					IsSecondaryButtonEnabled = true,
					SecondaryButtonText = "Skip Update"
				};

				Wpf.Ui.Controls.MessageBoxResult result = await msgBox.ShowDialogAsync();
				if (result == Wpf.Ui.Controls.MessageBoxResult.Primary) {
					string? downloadUrl = response.assets?.FirstOrDefault()?.browser_download_url;
					if (!string.IsNullOrEmpty(downloadUrl)) {
						Process.Start(new ProcessStartInfo {
							FileName = downloadUrl,
							UseShellExecute = true
						});
					} else {
						Logger.Instance.Write("No downloadable asset found in release.");
					}
				} else if (result == Wpf.Ui.Controls.MessageBoxResult.Secondary) {
					Config.LastNotifiedVersion = latestVersion;
				}
			}
		} catch (Exception e) {
			Logger.Instance.Write("Update check failed: " + e.Message);
		}
	}

	public static void ApplyLanguage(string cultureCode) {
		try {
			string newLanguage = $"Resources/Strings/{cultureCode}.xaml";
			List<string> oldResources = ResourceSwitcher.Instance
				.OppositeResources(newLanguage, Constants.SupportedLanguages.Keys
					.Select(code => $"Resources/Strings/{code}.xaml").ToList());
			ResourceSwitcher.Instance.SwitchResources(oldResources, [newLanguage]);
		} catch (Exception ex) {
			Logger.Instance.Write(
				$"Error setting language to {cultureCode}. Exception: {ex.Message}\n{ex.StackTrace}");
		}
		OnUpdateStatusLabel?.Invoke(null, EventArgs.Empty);
	}

	public static void ApplyTheme(bool isDark) {
		try {
			ApplicationTheme newTheme = isDark ? ApplicationTheme.Dark : ApplicationTheme.Light;
			List<string> oldResources = ResourceSwitcher.Instance
				.OppositeResources(Constants.SupportedThemes[newTheme], Constants.SupportedThemes.Values.ToList());

			ResourceSwitcher.Instance.SwitchResources(
				[
					..oldResources, Constants.IconsResource
				], // Icons must be added after the theme resource as they use its colors
				[Constants.SupportedThemes[newTheme], Constants.IconsResource]
			);

			ApplicationThemeManager.Apply(newTheme);
		} catch (Exception ex) {
			Logger.Instance.Write(
				$"Error setting theme to {(isDark ? "dark" : "light")} theme. Exception: {ex.Message}\n{ex.StackTrace}");
		}
	}

	public static void ShowMessageBox(string title, string content) {
		Wpf.Ui.Controls.MessageBox msgBox = new() {
			Title = title,
			Content = content
		};
		msgBox.ShowDialogAsync();
	}
	
	public static void CleanupAndExit() {
		Zoom.Dispose();
		HookHandler.Dispose();
		Application.Current.Shutdown();
	}
	
	public static void UnhandledExceptionHandler(object? sender, UnhandledExceptionEventArgs args) {
		Exception? ex = args.ExceptionObject as Exception;
		if (ex == null) {
			Logger.Instance.Write($"Unhandled non-Exception object thrown: {args.ExceptionObject}");
			ShowMessageBox("Unhandled Error", "An unhandled non-exception object was thrown. Check logs.");
			return;
		}

		Logger.Instance.Write($"Unhandled exception (AppDomain): {ex.Message}\n{ex.StackTrace}");
		ShowMessageBox("Unhandled Error", "An unhandled error occurred. Please check the log file for more details.");
		Application.Current.Shutdown();
	}

	public static void DispatcherUnhandledExceptionHandler(object? sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs args) {
		if (args.Exception != null) {
			Logger.Instance.Write(
				$"Unhandled exception (Dispatcher): {args.Exception.Message}\n{args.Exception.StackTrace}");
			ShowMessageBox("Unhandled UI Error",
				"An unhandled UI error occurred. Please check the log file for more details.");
			args.Handled = true;
		}
		Application.Current.Shutdown();
	}

	public static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args) {
		Logger.Instance.Write($"Unobserved Task exception: {args.Exception.Message}\n{args.Exception.StackTrace}");
		ShowMessageBox("Background Error", "A background task threw an exception. Please check the log file.");
		args.SetObserved();
		Application.Current.Shutdown();
	}
}