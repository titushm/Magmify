using System.Diagnostics;
using Magmify.Models;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace Magmify.Services;

public class ZoomService {
	private static ZoomService? _instance;
	private static readonly object InstanceLock = new();
	private static bool? _isMinecraftRunningCache;

	private ZoomService() {
		Task.Run(() => {
			using var session = new TraceEventSession("MCBEWatcherSession");
			session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);

			session.Source.Kernel.ProcessStart += data => {
				if (string.Equals(data.ImageFileName, Info.MinecraftProcessName+ ".exe", StringComparison.OrdinalIgnoreCase)) {
					_isMinecraftRunningCache = true;
				}
			};

			session.Source.Kernel.ProcessStop += data => {
				if (string.Equals(data.ImageFileName, Info.MinecraftProcessName + ".exe", StringComparison.OrdinalIgnoreCase)) {
					_isMinecraftRunningCache = false;
				}
			};

			session.Source.Process();
		});
	}
	
	public static ZoomService Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new ZoomService();
				}

				return _instance;
			}
		}
	}

	public bool IsMinecraftRunning {
		get {
			if (_isMinecraftRunningCache != null) {
				return (bool)_isMinecraftRunningCache;
			}

			bool isMcRunning = Process.GetProcessesByName(Info.MinecraftProcessName).Any();
			_isMinecraftRunningCache = isMcRunning;
			return isMcRunning;
		}
	}
	
}