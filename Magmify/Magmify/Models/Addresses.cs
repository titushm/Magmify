namespace Magmify.Models;

public class Addresses(IntPtr fovAddress, IntPtr cameraSensitivityAddress, IntPtr handVisibilityAddress, IntPtr hudVisibilityAddress, IntPtr selectedSlotAddress) {
	public IntPtr FovAddress { get; set; } = fovAddress;
	public IntPtr CameraSensitivityAddress { get; set; } = cameraSensitivityAddress;
	public IntPtr HandVisibilityAddress { get; set; } = handVisibilityAddress;
	public IntPtr HudVisibilityAddress { get; set; } = hudVisibilityAddress;
	public IntPtr SelectedSlotAddress { get; set; } = selectedSlotAddress;
}