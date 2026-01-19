using System.ComponentModel;
using System.Runtime.Versioning;
using Magmify.Models;

namespace Magmify.ViewModels;

[SupportedOSPlatform("windows7.0")]
public class ZoomViewModel : INotifyPropertyChanged {
	public int KeyMode {
		get => Config.KeyModeIndex;
		set {
			Config.KeyModeIndex = value;
			OnPropertyChanged(nameof(KeyMode));
		}
	}

	public float ZoomFov {
		get => Config.ZoomFov;
		set {
			Config.ZoomFov = value;
			OnPropertyChanged(nameof(ZoomFov));
		}
	}

	public int CameraSensitivity {
		get => Config.CameraSensitivity;
		set {
			Config.CameraSensitivity = value;
			OnPropertyChanged(nameof(CameraSensitivity));
		}
	}

	public bool ScrollAdjust {
		get => Config.ScrollAdjust;
		set {
			Config.ScrollAdjust = value;
			OnPropertyChanged(nameof(ScrollAdjust));
		}
	}

	public bool HideHand {
		get => Config.HideHand;
		set {
			Config.HideHand = value;
			OnPropertyChanged(nameof(HideHand));
		}
	}

	public bool HideHud {
		get => Config.HideHud;
		set {
			Config.HideHud = value;
			OnPropertyChanged(nameof(HideHud));
		}
	}

	public int ZoomAnimation {
		get => Config.ZoomAnimationIndex;
		set {
			Config.ZoomAnimationIndex = value;
			OnPropertyChanged(nameof(ZoomAnimation));
		}
	}
	
	public int ScrollStep {
		get => Config.ScrollStep;
		set {
			Config.ScrollStep = value;
			OnPropertyChanged(nameof(ScrollStep));
		}
	}

	public bool ScrollAdjustLock {
		get => Config.ScrollAdjustLock;
		set {
			Config.ScrollAdjustLock = value;
			OnPropertyChanged(nameof(ScrollAdjustLock));
		}
	}
	
	public int ZoomAnimationFpsIndex {
		get => Config.ZoomAnimationFpsIndex;
		set {
			Config.ZoomAnimationFpsIndex = value;
			OnPropertyChanged(nameof(ZoomAnimationFpsIndex));
		}
	}
	
	public int ZoomAnimationDuration {
		get => Config.ZoomAnimationDuration;
		set {
			Config.ZoomAnimationDuration = value;
			OnPropertyChanged(nameof(ZoomAnimationDuration));
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