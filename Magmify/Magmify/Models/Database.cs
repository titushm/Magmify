using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;
using FluentValidation;
using Magmify.Services;

namespace Magmify.Models;

public class AppDatabaseModel {
	public static AppDatabaseModel Default => new() {
		config = ConfigModel.Default,
		pointers = new Dictionary<string, Pointers>()
	};

	public ConfigModel config { get; set; }
	public Dictionary<string, Pointers> pointers { get; set; }
}

public class AppDatabaseModelValidator : AbstractValidator<AppDatabaseModel> {
	public AppDatabaseModelValidator() {
		RuleFor(x => x.config)
			.NotNull()
			.WithMessage("config must not be null.")
			.SetValidator(new ConfigModelValidator());

		RuleFor(x => x.pointers)
			.NotNull()
			.WithMessage("pointers must not be null.");
	}
}

public class ConfigModel {
	public static ConfigModel Default => new() {
		is_dark_theme = true,
		close_to_tray = false,
		tray_tip_shown = false,
		start_hidden = false,
		scroll_adjust = true,
		scroll_adjust_lock = false,
		scroll_step = 10,
		hide_hand = false,
		hide_hud = false,
		zoom_fov = 30.0f,
		camera_sensitivity = 50,
		key_mode_index = 0,
		zoom_animation_index = 1,
		zoom_animation_fps_index = 1,
		zoom_animation_duration = 300,
		language = "en",
		last_notified_version = "0.0.0",
		minecraft_install_path = Constants.MinecraftInstallPath,
		zoom_keybinding = ZoomKeybindingModel.Default
	};

	public bool is_dark_theme { get; set; }
	public bool close_to_tray { get; set; }
	public bool tray_tip_shown { get; set; }
	public bool start_hidden { get; set; }
	public bool scroll_adjust { get; set; }
	public bool scroll_adjust_lock { get; set; }
	public int scroll_step { get; set; }
	public bool hide_hand { get; set; }
	public bool hide_hud { get; set; }
	public float zoom_fov { get; set; }
	public int camera_sensitivity { get; set; }
	public int key_mode_index { get; set; }
	public int zoom_animation_index { get; set; }
	public int zoom_animation_fps_index { get; set; }
	public int zoom_animation_duration { get; set; }
	public string language { get; set; }
	public string last_notified_version { get; set; }
	public string minecraft_install_path { get; set; }
	public ZoomKeybindingModel? zoom_keybinding { get; set; }
}

[SupportedOSPlatform("windows7.0")]
public class ConfigModelValidator : AbstractValidator<ConfigModel> {
	public ConfigModelValidator() {
		RuleFor(x => x.is_dark_theme).NotNull();
		RuleFor(x => x.close_to_tray).NotNull();
		RuleFor(x => x.tray_tip_shown).NotNull();
		RuleFor(x => x.start_hidden).NotNull();
		RuleFor(x => x.scroll_adjust).NotNull();
		RuleFor(x => x.scroll_adjust_lock).NotNull();
		RuleFor(x => x.scroll_step).NotNull();
		RuleFor(x => x.hide_hand).NotNull();
		RuleFor(x => x.hide_hud).NotNull();

		RuleFor(x => x.zoom_fov)
			.NotNull()
			.InclusiveBetween(30, 110);

		RuleFor(x => x.camera_sensitivity)
			.NotNull()
			.InclusiveBetween(0, 100);

		RuleFor(x => x.key_mode_index)
			.NotNull()
			.InclusiveBetween(0, 1);

		RuleFor(x => x.zoom_animation_index)
			.NotNull()
			.InclusiveBetween(0, 3);
		
		RuleFor(x => x.zoom_animation_fps_index)
			.NotNull()
			.InclusiveBetween(0, 5);
		
		RuleFor(x => x.zoom_animation_duration)
			.NotNull()
			.InclusiveBetween(50, 500);

		RuleFor(x => x.language)
			.NotEmpty()
			.Must(s => Constants.SupportedLanguages.Keys.Contains(s))
			.WithMessage("Language code is not supported.");

		RuleFor(x => x.last_notified_version).NotEmpty();
		RuleFor(x => x.minecraft_install_path)
			.NotEmpty()
			.Must(Directory.Exists)
			.Must(Minecraft.Instance.IsValidInstallation)
			.WithMessage("Minecraft install path does not exist.");

		RuleFor(x => x.zoom_keybinding)
			.SetValidator(new ZoomKeybindingModelValidator());
	}
	
}

public class ZoomKeybindingModel {
	public static ZoomKeybindingModel Default => new() {
		key = 0,
		ctrl = false,
		alt = false,
		shift = false,
		win = false
	};

	public int key { get; set; }
	public bool ctrl { get; set; }
	public bool alt { get; set; }
	public bool shift { get; set; }
	public bool win { get; set; }
}

public class ZoomKeybindingModelValidator : AbstractValidator<ZoomKeybindingModel> {
	public ZoomKeybindingModelValidator() {
		RuleFor(x => x.key).NotNull();
		RuleFor(x => x.ctrl).NotNull();
		RuleFor(x => x.alt).NotNull();
		RuleFor(x => x.shift).NotNull();
		RuleFor(x => x.win).NotNull();
	}
}