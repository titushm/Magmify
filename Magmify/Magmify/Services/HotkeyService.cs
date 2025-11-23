using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Magmify.Models;

namespace Magmify.Services;

public unsafe partial class HotkeyService {
	private static HotkeyService? _instance;
	private static readonly object InstanceLock = new();
	private static IntPtr _hookId = IntPtr.Zero;
	private static readonly LowLevelKeyboardProc Proc = HookCallback;
	private const int WmKeydown = 0x0100;
	private const int WhKeyboardLl = 13;
	private static Keybinding? _keybinding;

	private HotkeyService() {
		_hookId = SetHook(Proc);
	}

	public static HotkeyService Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new HotkeyService();
				}

				return _instance;
			}
		}
	}

	public void Register(Keybinding? keybinding) {
		_keybinding = keybinding;
	}

	public void Unregister() {
		_keybinding = null;
		UnhookWindowsHookEx(_hookId);
	}

	private static IntPtr SetHook(LowLevelKeyboardProc proc) {
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule? curModule = curProcess.MainModule;
		if (curModule == null) {
			throw new InvalidOperationException("Unable to retrieve the current process module.");
		}

		return SetWindowsHookEx(WhKeyboardLl, proc, GetModuleHandle(curModule.ModuleName), 0);
	}


	private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
		if (nCode < 0 || wParam != WmKeydown || _keybinding == null)
			return CallNextHookEx(_hookId, nCode, wParam, lParam);

		int vkCode = Marshal.ReadInt32(lParam);
		bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
		bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
		bool alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
		bool win = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
		if (vkCode == _keybinding.VKey && ctrl == _keybinding.Modifiers.Ctrl && shift == _keybinding.Modifiers.Shift && alt == _keybinding.Modifiers.Alt && win == _keybinding.Modifiers.Win) {
			MessageBox.Show("Hotkey pressed!");
		}

		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}

	// DLL Imports
	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook,
		LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);
}