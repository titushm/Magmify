using System.Windows.Input;
using Magmify.Services;

namespace Magmify.Models;

public class Keybinding {
	public int VKey { get; init; }
	public Modifiers Modifiers { get; init; } = new();

	public override string ToString() {
		var mods = Modifiers.ToString();
		Key key = KeyInterop.KeyFromVirtualKey(VKey);

		if (!string.IsNullOrEmpty(mods)) {
			return $"{mods}+{key.ToString()}";
		}

		return key.ToString();
	}
}

public class Modifiers {
	// Make these public so they are serializable by System.Text.Json
	public bool Ctrl { get; set; }
	public bool Alt { get; set; }
	public bool Shift { get; set; }
	public bool Win { get; set; }

	public override string ToString() {
		var mods = new List<string>();
		if (Ctrl) mods.Add("Ctrl");
		if (Alt) mods.Add("Alt");
		if (Shift) mods.Add("Shift");
		if (Win) mods.Add("Win");
		return string.Join("+", mods);
	}

	public void SetFromVCode(int vkCode, bool keyDown) {
		switch (vkCode) {
			case 0x11: // VK_CONTROL (generic)
			case 0xA2: // LCtrl
			case 0xA3: // RCtrl
				Ctrl = keyDown;
				break;

			case 0x10: // VK_SHIFT (generic)
			case 0xA0: // LShift
			case 0xA1: // RShift
				Shift = keyDown;
				break;

			case 0x12: // VK_MENU (generic Alt)
			case 0xA4: // LAlt (LMenu)
			case 0xA5: // RAlt (RMenu)
				Alt = keyDown;
				break;

			// Windows keys
			case 0x5B: // LWin
			case 0x5C: // RWin
				Win = keyDown;
				break;
		}
	}

	public bool Match(Modifiers other) {
		return Ctrl == other.Ctrl && Alt == other.Alt && Shift == other.Shift && Win == other.Win;
	}

	public Modifiers Clone() {
		return new Modifiers {
			Ctrl = this.Ctrl,
			Alt = this.Alt,
			Shift = this.Shift,
			Win = this.Win
		};
	}
}