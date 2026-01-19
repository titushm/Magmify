using System.ComponentModel;
using System.Runtime.Versioning;
using Magmify.Services;
using Config = Magmify.Models.Config;

namespace Magmify.ViewModels;

[SupportedOSPlatform("windows7.0")]
public class SettingsViewModel : INotifyPropertyChanged {
	public bool IsDarkTheme {
		get => Config.IsDarkTheme;
		set {
			Config.IsDarkTheme = value;
			OnPropertyChanged(nameof(IsDarkTheme));
			Helper.ApplyTheme(value);
		}
	}

	public static string AppVersion => Constants.AppVersion;

	public bool RunOnStartup {
		get => Config.RunOnStartup;
		set {
			Config.RunOnStartup = value;
			OnPropertyChanged(nameof(RunOnStartup));
		}
	}

	public bool CloseToTray {
		get => Config.CloseToTray;
		set {
			Config.CloseToTray = value;
			OnPropertyChanged(nameof(CloseToTray));
		}
	}

	public bool StartHidden {
		get => Config.StartHidden;
		set {
			Config.StartHidden = value;
			OnPropertyChanged(nameof(StartHidden));
		}
	}

	public string MinecraftInstallLocation {
		get => Config.MinecraftInstallPath;
		set {
			Config.MinecraftInstallPath = value;
			OnPropertyChanged(nameof(MinecraftInstallLocation));
		}
	}

	public Dictionary<string, string> Languages => Constants.SupportedLanguages;

	private string _selectedLanguage = Config.Language;

	public string SelectedLanguage {
		get => _selectedLanguage;
		set {
			if (_selectedLanguage == value) return;
			_selectedLanguage = value;

			Config.Language = value;
			OnPropertyChanged(nameof(SelectedLanguage));

			Helper.ApplyLanguage(value);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged(string propName)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}