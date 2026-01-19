using System.Windows;
using System.Windows.Input;
using Key = System.Windows.Input.Key;

namespace Magmify.Models;

public class Keybinding {
	public int VKey { get; init; }
	public required Modifiers Modifiers { get; init; }
	
	public override string ToString() {
		string keybindingString = "";
		if (Modifiers.Ctrl) keybindingString += "Ctrl+";
		if (Modifiers.Alt) keybindingString += "Alt+";
		if (Modifiers.Shift) keybindingString += "Shift+";
		if (Modifiers.Win) keybindingString += "Win+";
		Key key = KeyInterop.KeyFromVirtualKey(VKey);
		if (key == Key.None) {
			var notSet = Application.Current?.Resources["Label.NotSet"] as string;
			return notSet!;
		}
		
		keybindingString += key.ToString();
		return keybindingString;
	}
}

public class Modifiers {
	public bool Ctrl { get; set; }
	public bool Alt { get; set; }
	public bool Shift { get; set; }
	public bool Win { get; set; }
}