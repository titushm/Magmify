using System.Windows;

namespace Magmify.Services;

public class ResourceSwitcher {
	private static ResourceSwitcher? _instance;
	private static readonly object InstanceLock = new();

	public static ResourceSwitcher Instance {
		get {
			lock (InstanceLock) {
				if (_instance == null) {
					_instance = new ResourceSwitcher();
				}

				return _instance;
			}
		}
	}

	public List<string> OppositeResources(string resource, List<string> resources) {
		resources.RemoveAll(x => x == resource);
		return resources;
	}

	private void RemoveResourceDictionaries(List<string> resources) {
		var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
		var dictionariesToRemove = mergedDictionaries
			.Where(dictionary => resources.Contains(dictionary.Source.ToString()))
			.ToList();

		foreach (var dictionary in dictionariesToRemove) {
			mergedDictionaries.Remove(dictionary);
		}
	}

	private void AddResourceDictionaries(List<string> resources) {
		foreach (var resource in resources) {
			var resourceDictionary = new ResourceDictionary {
				Source = new Uri(resource, UriKind.RelativeOrAbsolute)
			};
			Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
		}
	}

	public void SwitchResources(List<string> resourcesToRemove, List<string> resourcesToAdd) {
		RemoveResourceDictionaries(resourcesToRemove);
		AddResourceDictionaries(resourcesToAdd);
	}
}