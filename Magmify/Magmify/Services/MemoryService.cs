using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Magmify.Services;

public unsafe class MemoryService {
	private static MemoryService? _instance;
	private static readonly object InstanceLock = new();
	private Process? _process = null;
	private IntPtr _processHandle;

	[DllImport("kernel32.dll")]
	static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("kernel32.dll")]
	static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize,
		out int lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize,
		out int lpNumberOfBytesWritten);

	public static MemoryService Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new MemoryService();
				}

				return _instance;
			}
		}
	}

	public bool Attach() {
		try {
			_process?.Refresh();
			if (_process == null || _process.HasExited) {
				var processes = Process.GetProcessesByName("Minecraft.Windows");
				if (processes.Length < 1) return false;
				_process = processes[0];
				_processHandle = OpenProcess(0x1F0FFF, false, _process.Id);
			}
		} catch {
			LogService.Instance.Write($"Failed to open process: {_process?.ProcessName}");
		}

		return true;
	}

	public byte[] ReadMemory(IntPtr address, int size) {
		byte[] buffer = new byte[size];
		ReadProcessMemory(_processHandle, address, buffer, size, out _);
		return buffer;
	}

	public void WriteMemory(IntPtr address, byte[] data) {
		WriteProcessMemory(_processHandle, address, data, data.Length, out _);
	}

	public IntPtr GetPointerAddress(IntPtr baseAddress, int[] offsets) {
		IntPtr currentAddress = baseAddress;
		byte[] buffer = new byte[IntPtr.Size];

		foreach (var t in offsets) {
			ReadProcessMemory(_processHandle, currentAddress, buffer, buffer.Length, out _);
			currentAddress = (IntPtr)(BitConverter.ToInt64(buffer, 0) + t);
		}

		return currentAddress;
	}
}