using System.ComponentModel;
using System.Windows;
using Magmify.Models;
using Magmify.Services;
using Wpf.Ui.Controls;
using Wpf.Ui.Tray.Controls;

namespace Magmify {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : FluentWindow {
		public MainWindow() {
			InitializeComponent();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
			LogService.Instance.Write("Application started.");
			HelperService.Instance.CheckForUpdates();
			HelperService.Instance.ApplyTheme(Config.IsDarkTheme);
			HelperService.Instance.ApplyLanguage(Config.Language);
			KeyService.Instance.RegisterKeybinding(Config.ZoomKeybinding);

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
			}
		}

		private void Exit_OnClick(object sender, RoutedEventArgs e) {
			Application.Current.Shutdown();
		}

		private void NotifyIcon_OnLeftClick(NotifyIcon sender, RoutedEventArgs e) {
			Show();
			Activate();
			WindowState = WindowState.Normal;
		}
	}
}