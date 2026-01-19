using System.Runtime.Versioning;
using System.Windows;
using Magmify.Models;
using Magmify.Services;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Magmify;

[SupportedOSPlatform("windows7.0")]
public partial class App {
	private static Mutex _mutex;
	protected override async void OnStartup(StartupEventArgs e) {
		const string appName = "Global\\" + Constants.AppGuid;
            
		bool createdNew;
		_mutex = new Mutex(true, appName, out createdNew);

		if (!createdNew) {
			new ToastContentBuilder()
				.AddText("Magmify is already running.")
				.AddText("Only one instance of Magmify can run at a time.")
				.SetToastDuration(ToastDuration.Short)
				.SetToastScenario(ToastScenario.Default)
				.Show();
			Current.Shutdown(); 
			return;
		}

		base.OnStartup(e);
		Logger.Instance.Write("Application started.");
		Helper.CheckForUpdatesAsync();
		await Registry.UpdatePointers();
		Zoom.Initialise(); // Preload Zoom service
		Helper.ApplyTheme(Config.IsDarkTheme);
		Helper.ApplyLanguage(Config.Language);
		HookHandler.Initialise();
		HookHandler.CurrentKeybinding = Config.ZoomKeybinding;
		
		DispatcherUnhandledException += Helper.DispatcherUnhandledExceptionHandler;
		AppDomain.CurrentDomain.UnhandledException += Helper.UnhandledExceptionHandler;
		TaskScheduler.UnobservedTaskException += Helper.TaskScheduler_UnobservedTaskException;
	}
}