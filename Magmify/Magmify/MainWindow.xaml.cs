using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows;
using Magmify.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using Wpf.Ui.Tray.Controls;
using Config = Magmify.Models.Config;

namespace Magmify;

[SupportedOSPlatform("windows7.0")]
public partial class MainWindow {
	public MainWindow() {
		InitializeComponent();
	}

	private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
		MainNavigationView.Navigate(typeof(Pages.Zoom));
		if (Config.StartHidden) {
			WindowState = WindowState.Minimized;
			Dispatcher.BeginInvoke(new Action(Hide), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
		}
	}

	private void MainWindow_OnClosing(object? sender, CancelEventArgs e) {
		if (Config.CloseToTray) {
			e.Cancel = true;
			Hide();
			if (!Config.TrayTipShown) {
				new ToastContentBuilder()
					.AddText("Magmify is still running in the system tray.")
					.AddText("You can reopen it by clicking the tray icon.")
					.SetToastDuration(ToastDuration.Short)
					.SetToastScenario(ToastScenario.Default)
					.Show();
				Config.TrayTipShown = true;
			}
			return;
		}
		Helper.CleanupAndExit();
	}

	private void Exit_OnClick(object sender, RoutedEventArgs e) {
		Helper.CleanupAndExit();
	}

	private void NotifyIcon_OnLeftClick(NotifyIcon sender, RoutedEventArgs e) {
		Show();
		Activate();
		WindowState = WindowState.Normal;
	}
}