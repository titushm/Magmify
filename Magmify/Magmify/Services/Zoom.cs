using System.Runtime.Versioning;
using Magmify.Models;

namespace Magmify.Services;

[SupportedOSPlatform("windows7.0")]
public static class Zoom {
	public static Pointers? Pointers;
	public static MemoryState? InitialState;
	public static float AdjustedFov;
	private static CancellationTokenSource? _zoomCts;
	private static float? _fovOverride;
	
	public static void Initialise() {
		Version? version = Minecraft.Instance.GetVersion();
		if (version == null) {
			Logger.Instance.Write("Failed to initialize Zoom service: Minecraft version is null");
			return;
		}
		Database.GetDatabase().pointers.TryGetValue(version.ToString(), out Pointers);
		Helper.OnUpdateStatusLabel?.Invoke(null, EventArgs.Empty);
	}
	
	private static Addresses? ResolveAddresses() {
		if (Pointers == null) {
			Logger.Instance.Write("Failed to resolve addresses: pointers not initialized");
			return null;
		}

		IntPtr Resolve(string name, List<int> offsets) {
			try {
				IntPtr ptr = Memory.Instance.ResolvePointer(Constants.ModuleName, offsets);
				if (ptr == IntPtr.Zero) {
					Logger.Instance.Write( $"Failed to resolve {name} pointer. Module: {Constants.ModuleName}; Offsets: {string.Join(", ", offsets)}");
				}
				return ptr;
			} catch (Exception ex) {
				Logger.Instance.Write($"Exception while resolving {name}: {ex.Message}");
				return IntPtr.Zero;
			}
		}

		var fovPointer = Resolve("FOV", Pointers.fov.ToList());
		var cameraSensitivityPointer = Resolve("Camera Sensitivity", Pointers.camera_sensitivity.ToList());
		var handVisibilityPointer = Resolve("Hand Visibility", Pointers.hand_visibility.ToList());
		var hudVisibilityPointer = Resolve("HUD Visibility", Pointers.hud_visibility.ToList());

		if (fovPointer == IntPtr.Zero || cameraSensitivityPointer == IntPtr.Zero || handVisibilityPointer == IntPtr.Zero || hudVisibilityPointer == IntPtr.Zero) {
			Logger.Instance.Write("Address resolution failed");
			return null;
		}
		
		return new Addresses(fovPointer, cameraSensitivityPointer, handVisibilityPointer, hudVisibilityPointer);
	}

	private static bool Ensure() {
		return Memory.Instance.Attach() && Pointers != null;
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
				Memory.Instance.WriteMemory(address, targetValue);
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
						Memory.Instance.WriteMemory(address, value); 
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
		IntPtr fovAddress = Memory.Instance.ResolvePointer(Constants.ModuleName, Pointers!.fov.ToList());
		float fov = _fovOverride ?? Memory.Instance.ReadMemory<float>(fovAddress);
		float adjustment = Config.ScrollStep * steps;
		float targetFov = Math.Clamp(fov + adjustment, 30, 110);
		AdjustedFov = targetFov;
		Memory.Instance.WriteMemory(fovAddress, targetFov);
	}

	public static void SetZoom(bool enable) {
		if (!Ensure()) return;
		Addresses? _addresses = ResolveAddresses();
		if (_addresses == null) return;
		CancelZoomAnimation();
		
		if (enable) {
			InitialState ??= new MemoryState(
				_fovOverride ?? Memory.Instance.ReadMemory<float>(_addresses.FovAddress),
				Memory.Instance.ReadMemory<float>(_addresses.CameraSensitivityAddress),
				Memory.Instance.ReadMemory<bool>(_addresses.HandVisibilityAddress),
				Memory.Instance.ReadMemory<bool>(_addresses.HudVisibilityAddress)
			);
			
			if (!Config.ScrollAdjustLock) AdjustedFov = 0;
	
			Memory.Instance.WriteMemory(_addresses.HandVisibilityAddress, Config.HideHand);
			Memory.Instance.WriteMemory(_addresses.HudVisibilityAddress, Config.HideHud);
			Memory.Instance.WriteMemory(_addresses.CameraSensitivityAddress, Config.CameraSensitivity / 100f);

		} else {
			if (InitialState == null) return;
			
			Memory.Instance.WriteMemory(_addresses.HandVisibilityAddress, InitialState.HandVisibility);
			Memory.Instance.WriteMemory(_addresses.HudVisibilityAddress, InitialState.HudVisibility);
			Memory.Instance.WriteMemory(_addresses.CameraSensitivityAddress, InitialState.CameraSensitivity);

			CancelZoomAnimation();
			
		}

		if (Config.HideHud || Config.HideHand) {
			KeyInjector.SendKeypress(0x70); // Force HUD/hand refresh (F1)
			KeyInjector.SendKeypress(0x70); // Toggle back
		}

		float targetFov = enable
			? Config.ScrollAdjust && Config.ScrollAdjustLock
				? AdjustedFov
				: Config.ZoomFov
			: InitialState.Fov;
		
		float currentCurrentFov = _fovOverride ?? Memory.Instance.ReadMemory<float>(_addresses.FovAddress);
		StartAnimation(_addresses.FovAddress, 
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