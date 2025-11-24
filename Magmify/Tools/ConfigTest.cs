using System;
using System.IO;
using Magmify.Models;
using Magmify.Services;

class Program {
    static void Main() {
        var kb = new Keybinding { VKey = 0x53, Modifiers = new Modifiers { Ctrl = true, Alt = false, Shift = false, Win = false } };
        ConfigService.Instance.Set("ZoomKeybinding", kb);
        string path = Magmify.Info.ConfigPath;
        Console.WriteLine("Wrote to: " + path);
        Console.WriteLine(File.ReadAllText(path));
    }
}

