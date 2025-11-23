using System.Management;
using System.Windows;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace Magmify.Services;

public class ZoomService {
	private static ZoomService? _instance;
	private static readonly object InstanceLock = new();

	public void StartWatching() {
		// Task.Run(() => {
		// 	using (var session = new TraceEventSession("MyProcessSession")) {
		// 		session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);
		//
		// 		session.Source.Kernel.ProcessStart += data => {
		// 			if (string.Equals(data.ImageFileName, "notepad.exe", StringComparison.OrdinalIgnoreCase)) {
		// 				MessageBox.Show("Notepad started!");
		// 			}
		// 		};
		//
		// 		session.Source.Process();
		// 	}
		// });
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
}