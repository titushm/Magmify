using System.IO;
using System.Runtime.Versioning;
using Wpf.Ui.Controls;

namespace Magmify.Services;

[SupportedOSPlatform("windows7.0")]
public sealed class Logger {
	private static Logger? _instance;
	private static readonly object InstanceLock = new();

	private Logger() {
		Ensure();
		Clear();
	}

	public static Logger Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new Logger();
				}

				return _instance;
			}
		}
	}
	
	private void Ensure() {
		if (!Directory.Exists(Constants.AppDirectory)) Directory.CreateDirectory(Constants.AppDirectory);
		if (!File.Exists(Constants.LogPath)) File.Create(Constants.LogPath).Close();
	}

	public void Write(string message) {
		Ensure();
		try {
			string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
			File.AppendAllText(Constants.LogPath, logMessage);
		} catch (Exception ex) {
			MessageBox msgBox = new() {
				Title = "Log Error",
				Content = "Error writing to log: " + ex
			};

			msgBox.ShowDialogAsync();
		}
	}

	private void Clear() {
		try {
			if (File.Exists(Constants.LogPath)) {
				File.Delete(Constants.LogPath);
			}
		} catch (Exception ex) {
			Write("Error clearing log: " + ex);
		}
	}
}