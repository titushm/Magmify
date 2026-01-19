using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;
using Magmify.Models;
using Magmify.Models.Extensions;
using Magmify.Services;
using Magmify.ViewModels;
using Config = Magmify.Models.Config;

namespace Magmify.Pages;

[SupportedOSPlatform("windows7.0")]
public partial class Zoom {
	private int _secretClickCounter;
	private ZoomViewModel ViewModel => (ZoomViewModel)DataContext;

	public Zoom() {
		InitializeComponent();
	}

	private void Keybinding_OnPreviewKeyDown(object sender, KeyEventArgs e) {
		e.Handled = true;
		Key actualKey = e.Key == Key.System ? e.SystemKey : e.Key;
		int vk = KeyInterop.VirtualKeyFromKey(actualKey);
		
		if (actualKey == Key.Escape || HookHandler.IsModifierKey(vk)) {
			ViewModel.ZoomKeybinding = null;
			StatusLabel.Visibility = Config.ZoomKeybinding != null ? Visibility.Visible : Visibility.Hidden;
			return;
		}

		Keybinding keybinding = new Keybinding {
			VKey = vk,
			Modifiers = HookHandler.Modifiers.Clone() // Make a copy of the current modifier state
		};

		ViewModel.ZoomKeybinding = keybinding;
		StatusLabel.Visibility = ViewModel.ZoomKeybinding != Config.ZoomKeybinding ? Visibility.Visible : Visibility.Hidden;
	}

	private void SaveButton_Click(object sender, RoutedEventArgs e) {
		Config.ZoomKeybinding = ViewModel.ZoomKeybinding;
		StatusLabel.Visibility = Visibility.Hidden;
		_secretClickCounter++;
		if (_secretClickCounter >= 30) { //Shh
			SaveButton.SetResourceReference(ContentProperty, "Button.Save.Secret");
		}
	}

	private void Zoom_OnLoaded(object sender, RoutedEventArgs e) {
		ViewModel.ZoomKeybinding = Config.ZoomKeybinding;
	}
}