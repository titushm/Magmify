namespace Magmify.Models;

public class MemoryState(float fov, float cameraSensitivity, bool handVisibility, bool hudVisibility, int initialSlot) {
	public float Fov { get; set; } = fov;
	public float CameraSensitivity { get; set; } = cameraSensitivity;
	public bool HandVisibility { get; set; } = handVisibility;
	public bool HudVisibility { get; set; } = hudVisibility;
	public int InitialSlot { get; set; } = initialSlot;
}