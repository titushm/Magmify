using System.Windows.Input;

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
	public bool Ctrl { get; init; }
	public bool Alt { get; init; }
	public bool Shift { get; init; }
	public bool Win { get; init; }

	public override string ToString() {
		var mods = new List<string>();
		if (Ctrl) mods.Add("Ctrl");
		if (Alt) mods.Add("Alt");
		if (Shift) mods.Add("Shift");
		if (Win) mods.Add("Win");
		return string.Join("+", mods);
	}
}