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
		if (e.Key == Key.Escape) {
			ViewModel.ZoomKeybinding = null;

			StatusLabel.Visibility = Config.ZoomKeybinding != null ? Visibility.Visible : Visibility.Hidden;
			return;
		}

		int vk = KeyInterop.VirtualKeyFromKey(e.Key);
		Modifiers modifiers = new Modifiers {
			Ctrl = false,
			Alt = false,
			Shift = false,
			Win = false
		};

		foreach (var group in Info.KeybindingModifiers) {
			Key[] keys = {
				KeyInterop.KeyFromVirtualKey(group.Value[0]),
				KeyInterop.KeyFromVirtualKey(group.Value[1])
			};
			bool isPressed = Keyboard.IsKeyDown(keys[0]) || Keyboard.IsKeyDown(keys[1]);

			PropertyInfo? prop = typeof(Modifiers).GetProperty(group.Key);
			prop?.SetValue(modifiers, isPressed);

			if (keys[0] == e.Key || keys[1] == e.Key) {
				vk = 0;
			}
		}

		Keybinding keybinding = new Keybinding {
			VKey = vk,
			Modifiers = modifiers
		};

		ViewModel.ZoomKeybinding = vk != 0 ? keybinding : null;
		StatusLabel.Visibility =
			ViewModel.ZoomKeybinding != Config.ZoomKeybinding ? Visibility.Visible : Visibility.Hidden;
	}

	private void SaveButton_Click(object sender, RoutedEventArgs e) {
		Config.ZoomKeybinding = ViewModel.ZoomKeybinding;
		HotkeyService.Instance.Register(Config.ZoomKeybinding);
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