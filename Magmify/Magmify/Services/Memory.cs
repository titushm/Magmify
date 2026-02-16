using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Magmify.Services;

public class Memory {
	private static Memory? _instance;
	private static readonly object InstanceLock = new();
	private static Process? _process;
	private static IntPtr _processHandle;

	[DllImport("kernel32.dll")]
	static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("kernel32.dll")]
	static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize,
		out int lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize,
		out int lpNumberOfBytesWritten);

	private Memory() {
		bool success = Attach();
		if (!success) {
			Logger.Instance.Write("Failed to attach to Minecraft process.");
		}
	}
	
	public static Memory Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new Memory();
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
			Logger.Instance.Write($"Failed to open process: {_process?.ProcessName}");
		}

		return true;
	}

	private IntPtr GetModuleBaseAddress(string moduleName) {
		if (_process == null) return IntPtr.Zero;

		foreach (ProcessModule module in _process.Modules) {
			if (module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase)) {
				return module.BaseAddress;
			}
		}

		return IntPtr.Zero;
	}

	public T ReadMemory<T>(IntPtr address) where T : unmanaged {
		int size = Marshal.SizeOf<T>();
		byte[] buffer = new byte[size];
		ReadProcessMemory(_processHandle, address, buffer, size, out int bytesRead);
		if (bytesRead == 0) Logger.Instance.Write($"Failed to read bytes of {typeof(T).Name}");
		T value = Marshal.PtrToStructure<T>(Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0));
		return value;
	}

	public void WriteMemory<T>(IntPtr address, T value) where T : unmanaged {
		int size = Marshal.SizeOf<T>();
		byte[] buffer = new byte[size];
		Marshal.StructureToPtr(value, Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0), false);
		WriteProcessMemory(_processHandle, address, buffer, size, out int bytesWritten);
		if (bytesWritten == 0) Logger.Instance.Write($"Failed to write bytes of {typeof(T).Name}");
	}
	
	public IntPtr ResolvePointer(string moduleName, List<int> offsets) {
		IntPtr baseAddr = GetModuleBaseAddress(moduleName);
		if (baseAddr == IntPtr.Zero) return IntPtr.Zero;

		IntPtr address = baseAddr + offsets[0];
		offsets.RemoveAt(0);

		foreach (int off in offsets) {
			IntPtr ptr = ReadMemory<IntPtr>(address);
			if (ptr == 0) return IntPtr.Zero;
			address = ptr + off;
		}

		return address;
	}

}