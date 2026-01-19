using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Magmify.Models;
using Magmify.Models.Extensions;
using Microsoft.Extensions.Logging;

namespace Magmify.Services;

enum KeyType {
	WmKeydown = 0x100,
	WmKeyup = 0x101,
	WmSyskeydown = 0x104,
	WmSyskeyup = 0x105,
	WmMousewheel = 0x020A
}

enum HookType {
	WhKeyboardLl = 13,
	WhMouseLl = 14
}

[StructLayout(LayoutKind.Sequential)]
public struct Point {
	public int x;
	public int y;
}

[StructLayout(LayoutKind.Sequential)]
public struct Msllhookstruct {
	public Point pt;
	public uint mouseData;
	public uint flags;
	public uint time;
	public IntPtr dwExtraInfo;
}

[SupportedOSPlatform("windows7.0")]
public partial class HookHandler {
	private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
	private static IntPtr _mouseHookId;
	private static readonly HookProc MouseProc = MouseHookCallback;
	private static IntPtr _keyboardHookId;
	private static readonly HookProc KeyboardProc = KeyboardHookCallback;
	private static bool _isZoomed;
	public static readonly Modifiers Modifiers = new();
	public static Keybinding? CurrentKeybinding;
	private static readonly HashSet<int> DownKeys = [];
	private static int _lastScrollEventEpoch;
	private static int _scrollBucket;

	public static void Initialise() {
		SetKeyboardHookEnabled(Config.ZoomKeybinding != null && Minecraft.Instance.IsRunning);
		SetMouseHookEnabled(Config.ScrollAdjust && Minecraft.Instance.IsRunning);
	}

	public static void SetMouseHookEnabled(bool enabled) {
		if (enabled) {
			_mouseHookId = SetHook(MouseProc, (int)HookType.WhMouseLl);
			return;
		}
		UnhookWindowsHookEx(_mouseHookId);
	}

	public static void SetKeyboardHookEnabled(bool enabled) {
		if (enabled) {
			_keyboardHookId = SetHook(KeyboardProc, (int)HookType.WhKeyboardLl);
			return;
		}
		UnhookWindowsHookEx(_keyboardHookId);
	}

	private static bool IsMinecraftFocused() {
		IntPtr hwnd = GetForegroundWindow();
		if (hwnd == IntPtr.Zero) return false;
		GetWindowThreadProcessId(hwnd, out uint pid);
		return pid == Minecraft.Instance.CurrentProcessId;
	}

	private static IntPtr SetHook(HookProc proc, int hookType) {
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule? curModule = curProcess.MainModule;
		if (curModule == null)
		{
			throw new InvalidOperationException("Unable to retrieve the current process module.");
		}

		IntPtr hModule = GetModuleHandle(curModule.ModuleName);
		IntPtr hookId = SetWindowsHookEx(hookType, proc, hModule, 0);

		return hookId;
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

	private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
		if (!IsMinecraftFocused()) return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
		int vkCode = Marshal.ReadInt32(lParam);
		if (nCode < 0) return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);

		int msg = wParam.ToInt32();
		bool keyDown = msg is (int)KeyType.WmKeydown or (int)KeyType.WmSyskeydown;
		bool keyUp = msg is (int)KeyType.WmKeyup or (int)KeyType.WmSyskeyup;
		if (IsModifierKey(vkCode)) {
			switch (vkCode) {
				case 0x10: // VK_SHIFT
				case 0xA0: // Left Shift
				case 0xA1: // Right Shift
					Modifiers.Shift = keyDown;
					break;
				case 0x11: // VK_CONTROL
				case 0xA2: // Left Ctrl
				case 0xA3: // Right Ctrl
					Modifiers.Ctrl = keyDown;
					break;
				case 0x12: // VK_MENU
				case 0xA4: // Left Alt
				case 0xA5: // Right Alt
					Modifiers.Alt = keyDown;
					break;
				case 0x5B: // Left Windows
				case 0x5C: // Right Windows
					Modifiers.Win = keyDown;
					break;
			}
		}

		bool initialDown = false;
		if (keyDown) {
			initialDown = DownKeys.Add(vkCode);
		}

		if (CurrentKeybinding == null || vkCode != CurrentKeybinding.VKey) {
			if (keyUp) DownKeys.Remove(vkCode);
			return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
		}

		var required = CurrentKeybinding.Modifiers;
		bool requiresAnyModifier = required.Ctrl || required.Alt || required.Shift || required.Win;
		if (requiresAnyModifier && !Modifiers.MatchModifiers(required)) {
			if (keyUp) DownKeys.Remove(vkCode);
			return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
		}

		switch (Config.KeyModeIndex) {
			case 0: // Hold
				if (keyDown && initialDown) {
					if (!_isZoomed) {
						_isZoomed = true;
						Zoom.SetZoom(true);
					}
				} else if (keyUp) {
					if (_isZoomed) {
						_isZoomed = false;
						Zoom.SetZoom(false);
					}
				}

				break;

			case 1: // Toggle
				if (keyDown && initialDown) {
					_isZoomed = !_isZoomed;
					Zoom.SetZoom(_isZoomed);
				}

				break;
		}

		if (keyUp) DownKeys.Remove(vkCode);
		return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
	}

	private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
		if (!IsMinecraftFocused()) return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
		if (nCode < 0) return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
		int eventType = wParam.ToInt32();
		if (eventType != (int)KeyType.WmMousewheel || !_isZoomed || Zoom.Pointers == null)
			return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);

		var mouseData = Marshal.PtrToStructure<Msllhookstruct>(lParam);
		short scrollDelta = (short)((mouseData.mouseData >> 16) & 0xFFFF);
		KeyInjector.SendKeypress(0x30 + Zoom.InitialState!.InitialSlot + 1); // Force refresh by re-selecting current slot (0x30 is 0 Key. Zero-indexed so add 1)
		_scrollBucket += scrollDelta > 0 ? -1 : 1; // Accumulate small scrolls
		float timeSinceLastEvent = Environment.TickCount - _lastScrollEventEpoch;
		if (timeSinceLastEvent < 50) return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
		if (timeSinceLastEvent > 100) _scrollBucket = scrollDelta > 0 ? -1 : 1;
		_lastScrollEventEpoch = Environment.TickCount;
		Zoom.AdjustZoom(_scrollBucket);
		_scrollBucket = 0;
		return 1;
	}
	
	public static void Dispose() {
		UnhookWindowsHookEx(_keyboardHookId);
		UnhookWindowsHookEx(_mouseHookId);
	}

	// DLL Imports
	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);
}