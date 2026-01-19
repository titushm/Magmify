namespace Magmify.Models.Extensions;

public static class KeybindingExtensions {
	public static Keybinding ToKeybinding(this ZoomKeybindingModel source) {

		return new Keybinding {
			VKey = source.key,
			Modifiers = new() {
				Alt = source.alt,
				Ctrl = source.ctrl,
				Shift = source.shift,
				Win = source.win
			}
		};
	}
	
	public static ZoomKeybindingModel ToZoomKeybindingModel(this Keybinding source) {
		return new ZoomKeybindingModel {
			key = source.VKey,
			ctrl = source.Modifiers.Ctrl,
			alt = source.Modifiers.Alt,
			shift = source.Modifiers.Shift,
			win = source.Modifiers.Win
		};
	}
	
	public static bool MatchModifiers(this Modifiers source, Modifiers target) {
		return source.Ctrl == target.Ctrl && source.Alt == target.Alt && source.Shift == target.Shift && source.Win == target.Win;
	}
	
	public static Modifiers Clone(this Modifiers source) {
		return new Modifiers {
			Ctrl = source.Ctrl,
			Alt = source.Alt,
			Shift = source.Shift,
			Win = source.Win
		};
	}
}

