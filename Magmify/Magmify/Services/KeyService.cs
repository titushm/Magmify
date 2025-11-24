using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Magmify.Models;

namespace Magmify.Services;

public unsafe partial class KeyService {
	private static KeyService? _instance;
	private static readonly object InstanceLock = new();
	private static IntPtr _hookId = IntPtr.Zero;
	private static readonly LowLevelKeyboardProc Proc = HookCallback;
	private const int WmKeydown = 0x0100;
	private const int WmKeyup = 0x0101;
	private const int WmSysKeydown = 0x0104;
	private const int WmSysKeyup = 0x0105;
	private const int WhKeyboardLl = 13;
	private static bool _isHotkeyDown;
	public static readonly Modifiers Modifiers = new();
	public static Keybinding? CurrentKeybinding = new();

	private KeyService() {
		_hookId = SetHook(Proc);
	}

	public static KeyService Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new KeyService();
				}

				return _instance;
			}
		}
	}

	public void RegisterKeybinding(Keybinding? keybinding) {
		CurrentKeybinding = keybinding;
	}
	
	public void UnregisterKeybinding() {
		CurrentKeybinding = null;
		_isHotkeyDown = false;
	}
	
	private static bool IsHotkeyPressed(int vkCode) {
		if (CurrentKeybinding == null) return false;
		return vkCode == CurrentKeybinding.VKey && Modifiers.Match(CurrentKeybinding.Modifiers);
	}

	private static IntPtr SetHook(LowLevelKeyboardProc proc) {
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule? curModule = curProcess.MainModule;
		if (curModule == null) {
			throw new InvalidOperationException("Unable to retrieve the current process module.");
		}

		return SetWindowsHookEx(WhKeyboardLl, proc, GetModuleHandle(curModule.ModuleName), 0);
	}

	public static bool IsModifierKey(int vkCode) {
		return
			vkCode == 0x10 || // VK_SHIFT (generic)
			vkCode == 0xA0 || // Left Shift
			vkCode == 0xA1 || // Right Shift
			vkCode == 0x11 || // VK_CONTROL (generic)
			vkCode == 0xA2 || // Left Ctrl
			vkCode == 0xA3 || // Right Ctrl
			vkCode == 0x12 || // VK_MENU (generic Alt)
			vkCode == 0xA4 || // Left Alt (LMenu)
			vkCode == 0xA5 || // Right Alt (RMenu)
			vkCode == 0x5B || // Left Windows
			vkCode == 0x5C; // Right Windows
	}

	private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
		int vkCode = Marshal.ReadInt32(lParam);
		if (nCode < 0) {
			return CallNextHookEx(_hookId, nCode, wParam, lParam);
		}

		int msg = wParam.ToInt32();
		bool keyDown = msg is WmKeydown or WmSysKeydown;
		if (IsModifierKey(vkCode)) {
			Modifiers.SetFromVCode(vkCode, keyDown);
		}

		if (!IsHotkeyPressed(vkCode)) {
			return CallNextHookEx(_hookId, nCode, wParam, lParam);
		}

		bool keyUp = msg is WmKeyup or WmSysKeyup;

		switch (Config.KeyMode) {
			case 0: // Hold
				if (keyDown && !_isHotkeyDown) {
					_isHotkeyDown = true;
					MessageBox.Show("Zoom ON (Hold)");
				} else if (keyUp && _isHotkeyDown) {
					_isHotkeyDown = false;
					MessageBox.Show("Zoom OFF (Hold)");
				}

				break;

			case 1: // Toggle
				if (keyUp) {
					_isHotkeyDown = !_isHotkeyDown;
					if (_isHotkeyDown) {
						MessageBox.Show("Zoom ON (Toggle)");
					} else {
						MessageBox.Show("Zoom OFF (Toggle)");
					}
				}

				break;
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