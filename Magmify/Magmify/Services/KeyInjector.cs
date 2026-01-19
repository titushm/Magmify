using System.Runtime.InteropServices;

namespace Magmify.Services;

public static class KeyInjector {
	[DllImport("user32.dll")]
	static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	const uint WmKeydown = 0x0100;
	
	const uint WmKeyup = 0x0101;

	public static void SendKeypress(int vkCode) {
		IntPtr hwnd = HookHandler.GetForegroundWindow();
		if (hwnd == IntPtr.Zero) return;
		PostMessage(hwnd, WmKeydown, vkCode, IntPtr.Zero);
		PostMessage(hwnd, WmKeyup, vkCode, IntPtr.Zero);
	}
}