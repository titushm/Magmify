using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows;
using Magmify.Services;

namespace Magmify.ViewModels;

[SupportedOSPlatform("windows7.0")]
public class MainViewModel : INotifyPropertyChanged {
	public MainViewModel() {
		
		Helper.OnUpdateStatusLabel += (_, _) => {
			if (Application.Current?.Dispatcher != null) {
				Application.Current.Dispatcher.BeginInvoke(new Action(() => OnPropertyChanged(nameof(StatusLabel))));
			} else {
				OnPropertyChanged(nameof(StatusLabel));
			}
		};
		OnPropertyChanged(nameof(StatusLabel));
	}


	public string StatusLabel {
		get {
			Version? version = Minecraft.Instance.GetVersion();
			if (version == null) return (Application.Current?.Resources["Label.MinecraftNotFound"] as string)!;
			if (Zoom.Pointers == null) return (Application.Current?.Resources["Label.UnsupportedVersion"] as string)!;
			return (Minecraft.Instance.IsRunning ? Application.Current.Resources["Label.Connected"] as string: Application.Current.Resources["Label.Waiting"] as string)!;
		}
	}

	public string MinecraftVersion {
		get {
			string version = Minecraft.Instance.GetVersion()?.ToString() ?? "Not Found";
			return version;
		}
	}


	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}