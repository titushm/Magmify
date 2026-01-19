using System.Diagnostics;
using Magmify.Models;

namespace Magmify.Services;

public enum EasingType {
	EaseInOut,
	Elastic,
	EaseInOutQuart,
	None
}

public static class Animation {
	private static float EaseInOut(float start, float end, float t) {
		t = Math.Clamp(t, 0f, 1f);
		t = t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
		return start + (end - start) * t;
	}

	private static float Elastic(float start, float end, float t) {
		t = Math.Clamp(t, 0f, 1f);
		if (t == 0f) return start;
		if (t == 1f) return end;
		float range = end - start;
		const double decay = 10.0;
		const double period = 0.5;
		double s = period / (2 * Math.PI) * Math.Asin(1.0);
		double oscillation = Math.Pow(2.0, -decay * t) * Math.Sin((t - s) * (2.0 * Math.PI) / period);
		double value = 1 + oscillation;
		return start + range * (float)value;
	}

	private static float EaseInOutQuart(float start, float end, float t) {
		t = Math.Clamp(t, 0f, 1f);
		t = t < 0.5f ? 8f * t * t * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 4) / 2f;
		return start + (end - start) * t;
	}
	
	private static float Linear(float start, float end, float t) {
		t = Math.Clamp(t, 0f, 1f);
		return start + (end - start) * t;
	}

	private static float Ease(EasingType type, float start, float end, float t) {
		return type switch {
			EasingType.EaseInOut => EaseInOut(start, end, t),
			EasingType.Elastic => Elastic(start, end, t),
			EasingType.EaseInOutQuart => EaseInOutQuart(start, end, t),
			_ => throw new ArgumentOutOfRangeException(nameof(type), "Unknown easing type"),
		};
	}

	public static EasingType FromIndex(int index) {
		return index switch {
			0 => EasingType.EaseInOut,
			1 => EasingType.Elastic,
			2 => EasingType.EaseInOutQuart,
			3 => EasingType.None,
			_ => EasingType.EaseInOut,
		};
	}

	
	public static async Task AnimateAsync(EasingType type, float start, float end, TimeSpan duration, Action<float> onUpdate, CancellationToken ct = default) {
		if (duration.TotalMilliseconds <= 0) {
			onUpdate.Invoke(end);
			return;
		}

		double delay = Constants.AnimationFameTimes[Config.ZoomAnimationFpsIndex];
		var sw = Stopwatch.StartNew();

		while (sw.Elapsed < duration) {
			ct.ThrowIfCancellationRequested();

			float t = (float)(sw.Elapsed.TotalMilliseconds / duration.TotalMilliseconds);
			float value = Ease(type, start, end, t);
			onUpdate?.Invoke(value);

			await Task.Delay(TimeSpan.FromMilliseconds(delay), ct).ConfigureAwait(false);
		}

		onUpdate?.Invoke(end);
	}

}