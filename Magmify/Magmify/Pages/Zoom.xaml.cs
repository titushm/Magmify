using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Magmify.Models;
using Magmify.Services;
using Magmify.ViewModels;

namespace Magmify.Pages;

/// <summary>
/// Interaction logic for Zoom.xaml
/// </summary>
public partial class Zoom : Page {
	private int _secretClickCounter;
	private ZoomViewModel ViewModel => (ZoomViewModel)DataContext;

	public Zoom() {
		InitializeComponent();
	}

	private void Keybinding_OnPreviewKeyDown(object sender, KeyEventArgs e) {
		e.Handled = true;
		Key actualKey = e.Key == Key.System ? e.SystemKey : e.Key;
		int vk = KeyInterop.VirtualKeyFromKey(actualKey);
		
		if (actualKey == Key.Escape || KeyService.IsModifierKey(vk)) {
			ViewModel.ZoomKeybinding = null;
			StatusLabel.Visibility = Config.ZoomKeybinding != null ? Visibility.Visible : Visibility.Hidden;
			return;
		}

		Keybinding keybinding = new Keybinding {
			VKey = vk,
			Modifiers = KeyService.Modifiers.Clone()
		};

		ViewModel.ZoomKeybinding = keybinding;
		StatusLabel.Visibility = ViewModel.ZoomKeybinding != Config.ZoomKeybinding ? Visibility.Visible : Visibility.Hidden;
	}

	private void SaveButton_Click(object sender, RoutedEventArgs e) {
		Config.ZoomKeybinding = ViewModel.ZoomKeybinding;
		KeyService.Instance.RegisterKeybinding(Config.ZoomKeybinding);
		StatusLabel.Visibility = Visibility.Hidden;
		_secretClickCounter++;
		if (_secretClickCounter >= 30) {
			SaveButton.SetResourceReference(ContentProperty, "Button.Save.Secret");
		}
	}

	private void Zoom_OnLoaded(object sender, RoutedEventArgs e) {
		ViewModel.ZoomKeybinding = Config.ZoomKeybinding;
	}
}