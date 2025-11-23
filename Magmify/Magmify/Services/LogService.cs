using System.IO;
using Wpf.Ui.Controls;
using Magmify.Models;

namespace Magmify.Services;

public sealed class LogService {
	private static LogService? _instance;
	private static readonly object InstanceLock = new();

	private LogService() {
		HelperService.EnsureAppDirectory();
		Clear();
	}

	public static LogService Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new LogService();
				}

				return _instance;
			}
		}
	}

	public void Write(string message) {
		try {
			string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
			File.AppendAllText(Info.LogPath, logMessage);
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
			if (File.Exists(Info.LogPath)) {
				File.Delete(Info.LogPath);
			}
		} catch (Exception ex) {
			Write("Error clearing log: " + ex);
		}
	}
}