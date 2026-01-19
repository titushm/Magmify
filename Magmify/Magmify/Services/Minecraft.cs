using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Xml.Linq;
using Magmify.Models;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace Magmify.Services;

[SupportedOSPlatform("windows7.0")]
public sealed class Minecraft {
	private static Minecraft? _instance;
	private static readonly object InstanceLock = new();
	private static bool? _isMinecraftRunningCache;
	public int? CurrentProcessId;

	private Minecraft() {
		if (!IsValidInstallation(Constants.MinecraftInstallPath)) {
			Helper.ShowMessageBox("Minecraft Not Found",
				"Minecraft Bedrock Edition installation not found. Please set the correct installation path in settings.");
		}

		Task.Run(() => {
			using var session = new TraceEventSession("MCBEWatcherSession");
			session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);

			session.Source.Kernel.ProcessStart += data => {
				if (string.Equals(data.ImageFileName, Constants.MinecraftProcessName + ".exe", StringComparison.OrdinalIgnoreCase)) {
					CurrentProcessId = data.ProcessID;
					UpdateRunningCache(true);
					Application.Current.Dispatcher.Invoke(() => {
						HookHandler.SetKeyboardHookEnabled(Config.ZoomKeybinding != null);
						HookHandler.SetMouseHookEnabled(Config.ScrollAdjust);
					});
				}
			};

			session.Source.Kernel.ProcessStop += data => {
				if (string.Equals(data.ImageFileName, Constants.MinecraftProcessName + ".exe", StringComparison.OrdinalIgnoreCase)) {
					CurrentProcessId = null;
					UpdateRunningCache(false);
					Application.Current.Dispatcher.Invoke(() => {
						HookHandler.SetKeyboardHookEnabled(false);
						HookHandler.SetMouseHookEnabled(false);
					});
				}
			};

			session.Source.Process();
		});

		if (IsRunning) {
			CurrentProcessId = Process.GetProcessesByName(Constants.MinecraftProcessName).First().Id;
		}
	}

	public static Minecraft Instance {
		get {
			lock (InstanceLock) {
				return _instance ??= new Minecraft();
			}
		}
	}

	private void UpdateRunningCache(bool value) {
		if (_isMinecraftRunningCache != null && _isMinecraftRunningCache.Value == value) return;
		_isMinecraftRunningCache = value;
		Helper.OnUpdateStatusLabel!.Invoke(this, EventArgs.Empty);
	}

	public bool IsRunning {
		get {
			if (_isMinecraftRunningCache != null) {
				return (bool)_isMinecraftRunningCache;
			}

			bool isMcRunning = Process.GetProcessesByName(Constants.MinecraftProcessName).Any();
			_isMinecraftRunningCache = isMcRunning;
			return isMcRunning;
		}
	}

	public bool IsValidInstallation(string path) {
		Version? version = GetVersion(path);
		return version != null;
	}

	public Version? GetVersion(string? path = null) {
		try {
			if (path == null) {
				path = Config.MinecraftInstallPath;
			}

			string manifestPath = Path.Combine(
				path,
				"AppxManifest.xml"
			);

			XDocument xml = XDocument.Load(manifestPath);
			XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
			var identityElement = xml.Root!.Element(ns + "Identity")!; // These are not always null, but they will throw if the file is malformed so it's fine.
			string versionString = identityElement.Attribute("Version")!.Value; // ^^^
			return new Version(versionString);
		} catch {
			return null;
		}
	}
}