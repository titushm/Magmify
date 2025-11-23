using System.ComponentModel;
using Magmify.Models;
using Magmify.Services;

namespace Magmify.ViewModels {
	public class SettingsViewModel : INotifyPropertyChanged {
		public bool IsDarkTheme {
			get => Config.IsDarkTheme;
			set {
				Config.IsDarkTheme = value;
				OnPropertyChanged(nameof(IsDarkTheme));
				HelperService.Instance.ApplyTheme(value);
			}
		}

		public string AppVersion {
			get => Info.AppVersion;
		}

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

		public Dictionary<string, string> Languages => Info.SupportedLanguages;

		private string _selectedLanguage = Config.Language;

		public string SelectedLanguage {
			get => _selectedLanguage;
			set {
				if (_selectedLanguage == value) return;
				_selectedLanguage = value;

				Config.Language = value;
				OnPropertyChanged(nameof(SelectedLanguage));

				HelperService.Instance.ApplyLanguage(value);
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged(string propName)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
	}
}