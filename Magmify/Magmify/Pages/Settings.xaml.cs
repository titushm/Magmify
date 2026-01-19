using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using Magmify.Services;
using Magmify.ViewModels;

namespace Magmify.Pages;

[SupportedOSPlatform("windows7.0")]
public partial class Settings {
	public Settings() {
		InitializeComponent();
	}

	private void OpenLog_OnClick(object sender, RoutedEventArgs e) {
		Process.Start("explorer.exe", Constants.LogPath);
	}
		
	private void ResetPointers_OnClick(object sender, RoutedEventArgs e) {
		try {
			var database = Database.GetDatabase();
			database.pointers.Clear();
			Database.SaveDatabase(database);
		} catch (Exception ex) {
			Helper.ShowMessageBox("Error Resetting Pointers",
				"An error occurred while resetting pointers");
			Logger.Instance.Write($"Error resetting pointers: {ex.Message}\n{ex.StackTrace}");
		}
		Helper.ShowMessageBox("Pointers Reset","All pointers have been reset. You will need to restart the application for changes to take effect.");
	}

	private void PickLocation_OnClick(object sender, RoutedEventArgs e) {
		Microsoft.Win32.OpenFolderDialog openFolderDialog = new();
		if (openFolderDialog.ShowDialog() == true) {
			string selectedPath = openFolderDialog.FolderName;
			if (!File.Exists(selectedPath + "\\appxmanifest.xml")) {
				Helper.ShowMessageBox("Invalid Minecraft Install Path",
					"Ensure that the directory has an appxmanifest.xml");
				return;
			}
			SettingsViewModel? viewModel = DataContext as SettingsViewModel;
			viewModel!.MinecraftInstallLocation = selectedPath;
		}
	}
}