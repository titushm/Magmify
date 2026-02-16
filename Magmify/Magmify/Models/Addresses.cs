namespace Magmify.Models;

public class Addresses(IntPtr fovAddress, IntPtr cameraSensitivityAddress, IntPtr handVisibilityAddress, IntPtr hudVisibilityAddress) {
	public IntPtr FovAddress { get; set; } = fovAddress;
	public IntPtr CameraSensitivityAddress { get; set; } = cameraSensitivityAddress;
	public IntPtr HandVisibilityAddress { get; set; } = handVisibilityAddress;
	public IntPtr HudVisibilityAddress { get; set; } = hudVisibilityAddress;
}