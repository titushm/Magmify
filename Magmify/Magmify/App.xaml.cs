using System.Runtime.Versioning;
using System.Windows;
using Magmify.Models;
using Magmify.Services;

namespace Magmify;

[SupportedOSPlatform("windows7.0")]
public partial class App {
	protected override async void OnStartup(StartupEventArgs e) {
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