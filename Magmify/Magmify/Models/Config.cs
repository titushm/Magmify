using System.Diagnostics;
using System.Runtime.Versioning;
using Magmify.Models.Extensions;
using Magmify.Services;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace Magmify.Models;

[SupportedOSPlatform("windows7.0")]
public static class Config {
	public static bool RunOnStartup {
		get {
			using TaskService ts = new TaskService();
			Task? t = ts.FindTask(Constants.AppStartupName);
			return t != null;
		}
		set {
			using TaskService ts = new TaskService();
			if (value) {
				TaskDefinition td = ts.NewTask();
				td.RegistrationInfo.Description = "Launch Magmify elevated at logon";

				td.Triggers.Add(new LogonTrigger());

				if (Process.GetCurrentProcess().MainModule == null) { // Sanity check
					Logger.Instance.Write("Failed to get current process main module for startup task.");
					return;
				}

				td.Actions.Add(new ExecAction(Process.GetCurrentProcess().MainModule!.FileName));
				td.Principal.RunLevel = TaskRunLevel.Highest;
				ts.RootFolder.RegisterTaskDefinition(
					Constants.AppStartupName,
					td,
					TaskCreation.CreateOrUpdate,
					null,
					null,
					TaskLogonType.InteractiveToken);
			} else {
				ts.RootFolder.DeleteTask(Constants.AppStartupName, false);
			}
		}
	}

	public static bool IsDarkTheme {
		get => Database.GetDatabase().config.is_dark_theme;
		set => Database.SetConfigProperty("is_dark_theme", value);
	}

	public static bool CloseToTray {
		get => Database.GetDatabase().config.close_to_tray;
		set {
			Database.SetConfigProperty("close_to_tray", value);
			if (!value) return;
			TrayTipShown = false; // If enabling close to tray, reset tray tip shown so it can show again
		}
	}
	
	public static bool TrayTipShown {
		get => Database.GetDatabase().config.tray_tip_shown;
		set => Database.SetConfigProperty("tray_tip_shown", value);
	}

	public static bool StartHidden {
		get => Database.GetDatabase().config.start_hidden;
		set => Database.SetConfigProperty("start_hidden", value);
	}

	public static int KeyModeIndex {
		get => Database.GetDatabase().config.key_mode_index;
		set => Database.SetConfigProperty("key_mode_index", value);
	}

	public static float ZoomFov {
		get => Database.GetDatabase().config.zoom_fov;
		set => Database.SetConfigProperty("zoom_fov", value);
	}

	public static int CameraSensitivity {
		get => Database.GetDatabase().config.camera_sensitivity;
		set => Database.SetConfigProperty("camera_sensitivity", value);
	}

	public static bool ScrollAdjust {
		get => Database.GetDatabase().config.scroll_adjust;
		set {
			Database.SetConfigProperty("scroll_adjust", value);
			if (ZoomKeybinding != null && Minecraft.Instance.IsRunning) HookHandler.SetMouseHookEnabled(value); // Only enable mouse hook if a keybinding is set
		}
	}

	public static bool HideHand {
		get => Database.GetDatabase().config.hide_hand;
		set => Database.SetConfigProperty("hide_hand", value);
	}

	public static bool HideHud {
		get => Database.GetDatabase().config.hide_hud;
		set => Database.SetConfigProperty("hide_hud", value);
	}

	public static int ZoomAnimationIndex {
		get => Database.GetDatabase().config.zoom_animation_index;
		set => Database.SetConfigProperty("zoom_animation_index", value);
	}

	public static Keybinding? ZoomKeybinding {
		get => Database.GetDatabase().config.zoom_keybinding?.ToKeybinding();
		set {
			Database.SetConfigProperty("zoom_keybinding", value?.ToZoomKeybindingModel());
			HookHandler.SetKeyboardHookEnabled(value != null && Minecraft.Instance.IsRunning); // Enable or disable keyboard/mouse hook based on whether a keybinding is set
			if (ScrollAdjust) HookHandler.SetMouseHookEnabled(value != null);
			HookHandler.CurrentKeybinding = value; // Update current keybinding
		}
	}

	public static string Language {
		get => Database.GetDatabase().config.language;
		set => Database.SetConfigProperty("language", value);
	}

	public static Version LastNotifiedVersion {
		get => Database.GetDatabase().config.last_notified_version.ToVersionOrNull();
		set => Database.SetConfigProperty("last_notified_version", value.ToString());
	}

	public static string MinecraftInstallPath {
		get => Database.GetDatabase().config.minecraft_install_path;
		set => Database.SetConfigProperty("minecraft_install_path", value);
	}
	
	public static int ScrollStep {
		get => Database.GetDatabase().config.scroll_step;
		set => Database.SetConfigProperty("scroll_step", value);
	}

	public static bool ScrollAdjustLock {
		get => Database.GetDatabase().config.scroll_adjust_lock;
		set {
			Database.SetConfigProperty("scroll_adjust_lock", value);
			Zoom.AdjustedFov = ZoomFov; // Reset adjusted FOV to zoom FOV when changing lock setting
		}
	}
	
	public static int ZoomAnimationFpsIndex {
		get => Database.GetDatabase().config.zoom_animation_fps_index;
		set => Database.SetConfigProperty("zoom_animation_fps_index", value);
	}
		
	public static int ZoomAnimationDuration {
		get => Database.GetDatabase().config.zoom_animation_duration;
		set => Database.SetConfigProperty("zoom_animation_duration", value);
	}
}