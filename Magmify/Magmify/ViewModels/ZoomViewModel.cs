using System.ComponentModel;
using Magmify.Models;

namespace Magmify.ViewModels;

public class ZoomViewModel : INotifyPropertyChanged {
	public int KeyMode {
		get => Config.KeyMode;
		set {
			Config.KeyMode = value;
			OnPropertyChanged(nameof(KeyMode));
		}
	}

	public float ZoomMultiplier {
		get => Config.ZoomMultiplier;
		set {
			Config.ZoomMultiplier = value;
			OnPropertyChanged(nameof(ZoomMultiplier));
		}
	}

	public int ZoomSensitivity {
		get => Config.ZoomSensitivity;
		set {
			Config.ZoomSensitivity = value;
			OnPropertyChanged(nameof(ZoomSensitivity));
		}
	}

	public bool ScrollZoom {
		get => Config.ScrollZoom;
		set {
			Config.ScrollZoom = value;
			OnPropertyChanged(nameof(ScrollZoom));
		}
	}

	public bool HandVisibility {
		get => Config.HandVisibility;
		set {
			Config.HandVisibility = value;
			OnPropertyChanged(nameof(HandVisibility));
		}
	}

	public bool HudVisibility {
		get => Config.HudVisibility;
		set {
			Config.HudVisibility = value;
			OnPropertyChanged(nameof(HudVisibility));
		}
	}

	public int ZoomAnimation {
		get => Config.ZoomAnimation;
		set {
			Config.ZoomAnimation = value;
			OnPropertyChanged(nameof(ZoomAnimation));
		}
	}

	private Keybinding? _zoomKeybinding;

	public Keybinding? ZoomKeybinding {
		get => _zoomKeybinding;
		set {
			if (_zoomKeybinding != value) {
				_zoomKeybinding = value;
				OnPropertyChanged(nameof(ZoomKeybinding));
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged(string propName)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}