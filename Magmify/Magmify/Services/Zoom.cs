using System.Runtime.Versioning;
using Magmify.Models;

namespace Magmify.Services;

[SupportedOSPlatform("windows7.0")]
public static class Zoom {
	public static Pointers? Pointers;
	public static MemoryState? InitialState;
	public static float AdjustedFov;
	private static CancellationTokenSource? _zoomCts;
	private static Addresses? _addressCache;
	private static Memory? _memory;
	private static float? _fovOverride;
	
	public static void Initialise() {
		Version? version = Minecraft.Instance.GetVersion();
		if (version == null) {
			Logger.Instance.Write("Failed to initialize Zoom service: Minecraft version is null");
			return;
		}
		Database.GetDatabase().pointers.TryGetValue(version.ToString(), out Pointers);
		Helper.OnUpdateStatusLabel?.Invoke(null, EventArgs.Empty);
		_memory = new Memory(InvalidMemoryHandler);
	}
	
	private static void InvalidMemoryHandler() {
		Logger.Instance.Write("Memory was invalid, resetting address cache");
		_addressCache = null;
	}
	
	private static void ResolveAddresses() {
	if (_addressCache != null) return;

	if (_memory == null || Pointers == null) {
		Logger.Instance.Write("Failed to resolve addresses: memory or pointers not initialized");
		return;
	}

	IntPtr Resolve(string name, List<int> offsets) {
		try {
			IntPtr ptr = _memory!.ResolvePointer(Constants.ModuleName, offsets);
			if (ptr == IntPtr.Zero) {
				var msg = $"Failed to resolve {name} pointer. Module: {Constants.ModuleName}; Offsets: {string.Join(", ", offsets)}";
				Logger.Instance.Write(msg);
				Helper.ShowMessageBox("Invalid Pointer", msg);
			}
			return ptr;
		} catch (Exception ex) {
			var msg = $"Exception while resolving {name}: {ex.Message}";
			Logger.Instance.Write(msg);
			return IntPtr.Zero;
		}
	}

	var fovPointer = Resolve("FOV", Pointers.fov.ToList());
	var cameraSensitivityPointer = Resolve("Camera Sensitivity", Pointers.camera_sensitivity.ToList());
	var handVisibilityPointer = Resolve("Hand Visibility", Pointers.hand_visibility.ToList());
	var hudVisibilityPointer = Resolve("HUD Visibility", Pointers.hud_visibility.ToList());
	var selectedSlotPointer = Resolve("Selected Slot", Pointers.selected_slot.ToList());

	if (fovPointer == IntPtr.Zero || cameraSensitivityPointer == IntPtr.Zero || handVisibilityPointer == IntPtr.Zero || hudVisibilityPointer == IntPtr.Zero || selectedSlotPointer == IntPtr.Zero) {
		Logger.Instance.Write("Address resolution failed; address cache not created");
		return;
	}

	_addressCache = new Addresses(fovPointer, cameraSensitivityPointer, handVisibilityPointer, hudVisibilityPointer, selectedSlotPointer);
	Logger.Instance.Write("Address cache resolved successfully");
}

	private static bool Ensure() {
		return _memory!.Attach() && Pointers != null;
	}
	
	private static void CancelZoomAnimation() {
		try {
			_zoomCts?.Cancel();
			_zoomCts?.Dispose();
		} catch { /* ignore */ }
		_zoomCts = null;
	}
	
	private static void StartAnimation(IntPtr address, float startingValue, float targetValue, EasingType easingType) {
		if (easingType == EasingType.None) {
			try {
				_memory!.WriteMemory(address, targetValue);
			} catch (Exception ex) {
				Logger.Instance.Write($"Zoom write error: {ex}");
			}
			return;
		}
   
		_zoomCts = new CancellationTokenSource();
		var token = _zoomCts.Token;
   
		_ = Task.Run(async () => {
			try {
				await Animation.AnimateAsync(easingType, startingValue, targetValue, TimeSpan.FromMilliseconds(Config.ZoomAnimationDuration), value => {
					try {
						_fovOverride = value;
						_memory!.WriteMemory(address, value); 
					}
					catch { /* ignore */ }
				}, token).ConfigureAwait(false);
         
				_fovOverride = null;
         
			} catch (OperationCanceledException) {
				// Animation cancelled
				// Ideally we DON'T snap to target here if we are just reversing direction
				_fovOverride = null; 
         
			} catch (Exception ex) {
				Logger.Instance.Write($"Zoom animation error: {ex}");
				_fovOverride = null;
			}
		}, token);
	}

	public static void AdjustZoom(int steps) {
		if (!Ensure()) return;
		IntPtr fovAddress = _memory!.ResolvePointer(Constants.ModuleName, Pointers!.fov.ToList());
		float fov = _fovOverride ?? _memory.ReadMemory<float>(fovAddress);
		float adjustment = Config.ScrollStep * steps;
		float targetFov = Math.Clamp(fov + adjustment, 30, 110);
		AdjustedFov = targetFov;
		_memory.WriteMemory(fovAddress, targetFov);
	}

	public static void SetZoom(bool enable) {
		if (!Ensure()) return;
		if (_addressCache == null) ResolveAddresses();
		
		CancelZoomAnimation();
		
		if (enable) {
			InitialState ??= new MemoryState(
				_fovOverride ?? _memory!.ReadMemory<float>(_addressCache!.FovAddress),
				_memory!.ReadMemory<float>(_addressCache!.CameraSensitivityAddress),
				_memory.ReadMemory<bool>(_addressCache.HandVisibilityAddress),
				_memory.ReadMemory<bool>(_addressCache.HudVisibilityAddress),
				_memory.ReadMemory<int>(_addressCache.SelectedSlotAddress)
			);
			
			if (!Config.ScrollAdjustLock) AdjustedFov = 0;
	
			_memory!.WriteMemory(_addressCache!.HandVisibilityAddress, Config.HideHand);
			_memory.WriteMemory(_addressCache.HudVisibilityAddress, Config.HideHud);
			_memory.WriteMemory(_addressCache.CameraSensitivityAddress, Config.CameraSensitivity / 100f);

		} else {
			if (InitialState == null) return;
			
			_memory!.WriteMemory(_addressCache!.HandVisibilityAddress, InitialState.HandVisibility);
			_memory.WriteMemory(_addressCache.HudVisibilityAddress, InitialState.HudVisibility);
			_memory.WriteMemory(_addressCache.CameraSensitivityAddress, InitialState.CameraSensitivity);

			CancelZoomAnimation();
			
		}

		if (Config.HideHud) {
			int currentSlot = _memory.ReadMemory<int>(_addressCache.SelectedSlotAddress);
			try { KeyInjector.SendKeypress(0x30 + currentSlot + 1); } catch { /* ignore */ }
		}

		float targetFov = enable
			? Config.ScrollAdjust && Config.ScrollAdjustLock
				? AdjustedFov
				: Config.ZoomFov
			: InitialState.Fov;
		
		float currentCurrentFov = _fovOverride ?? _memory.ReadMemory<float>(_addressCache.FovAddress);
		StartAnimation(_addressCache.FovAddress, 
			currentCurrentFov,
			targetFov, 
			Animation.FromIndex(Config.ZoomAnimationIndex)
		); 
	}

	public static void Dispose() {
		CancelZoomAnimation();
		_zoomCts = null;
		SetZoom(false);
	}
}