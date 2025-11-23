using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Magmify.Models;

namespace Magmify.Pages {
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : Page {
		public Settings() {
			InitializeComponent();
		}

		private void OpenLog_OnClick(object sender, RoutedEventArgs e) {
			Process.Start("explorer.exe", Info.LogPath);
		}
	}
}