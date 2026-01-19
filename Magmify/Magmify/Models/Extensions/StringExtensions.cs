namespace Magmify.Models.Extensions;

public static class StringExtensions {
	public static Version ToVersionOrNull(this string? versionString) {
	

		if (Version.TryParse(versionString, out var version)) {
			return version;
		}

		return new Version(0, 0);
	}
	
}