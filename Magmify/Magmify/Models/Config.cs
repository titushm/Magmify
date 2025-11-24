using System.Diagnostics;
using Magmify.Services;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace Magmify.Models;

public static class Config {
	public static bool RunOnStartup {
		get {
			using TaskService ts = new TaskService();
			Task? t = ts.FindTask(Info.AppStartupName);
			return t != null;
		}
		set {
			using TaskService ts = new TaskService();
			if (value) {
				TaskDefinition td = ts.NewTask();
				td.RegistrationInfo.Description = "Launch Magmify elevated at logon";

				td.Triggers.Add(new LogonTrigger());
				td.Actions.Add(new ExecAction(Process.GetCurrentProcess().MainModule.FileName, null, null));
				td.Principal.RunLevel = TaskRunLevel.Highest;
				ts.RootFolder.RegisterTaskDefinition(
					Info.AppStartupName,
					td,
					TaskCreation.CreateOrUpdate,
					null,
					null,
					TaskLogonType.InteractiveToken);
			} else {
				ts.RootFolder.DeleteTask(Info.AppStartupName, false);
			}
		}
	}

	public static bool IsDarkTheme {
		get => ConfigService.Instance.Get("IsDarkTheme", false);
		set => ConfigService.Instance.Set("IsDarkTheme", value);
	}

	public static bool CloseToTray {
		get => ConfigService.Instance.Get("CloseToTray", false);
		set => ConfigService.Instance.Set("CloseToTray", value);
	}

	public static bool StartHidden {
		get => ConfigService.Instance.Get("StartHidden", false);
		set => ConfigService.Instance.Set("StartHidden", value);
	}

	public static int KeyMode {
		get => ConfigService.Instance.Get("KeyMode", 0);
		set => ConfigService.Instance.Set("KeyMode", value);
	}

	public static float ZoomMultiplier {
		get => ConfigService.Instance.Get("ZoomMultiplier", 2.0f);
		set => ConfigService.Instance.Set("ZoomMultiplier", value);
	}

	public static int ZoomSensitivity {
		get => ConfigService.Instance.Get("ZoomSensitivity", 50);
		set => ConfigService.Instance.Set("ZoomSensitivity", value);
	}

	public static bool ScrollZoom {
		get => ConfigService.Instance.Get("ScrollZoom", true);
		set => ConfigService.Instance.Set("ScrollZoom", value);
	}

	public static bool HandVisibility {
		get => ConfigService.Instance.Get("HandVisibility", true);
		set => ConfigService.Instance.Set("HandVisibility", value);
	}

	public static bool HudVisibility {
		get => ConfigService.Instance.Get("HudVisibility", true);
		set => ConfigService.Instance.Set("HudVisibility", value);
	}

	public static int ZoomAnimation {
		get => ConfigService.Instance.Get("ZoomAnimation", 1);
		set => ConfigService.Instance.Set("ZoomAnimation", value);
	}

	public static Keybinding? ZoomKeybinding {
		get => ConfigService.Instance.Get<Keybinding>("ZoomKeybinding");
		set => ConfigService.Instance.Set("ZoomKeybinding", value);
	}

	public static string Language {
		get => ConfigService.Instance.Get<string>("Language", "en");
		set => ConfigService.Instance.Set("Language", value);
	}
	
	public static Version LastNotifiedVersion {
		get => ConfigService.Instance.Get("LastNotifiedVersion", new Version(0, 0, 0));
		set => ConfigService.Instance.Set("LastNotifiedVersion", value);
	}
}