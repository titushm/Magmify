using System.Diagnostics;
using Magmify.Services;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace Magmify.Models;

public class GithubRelease {
	public string Tag_Name { get; set; }
	public List<GithubAsset> Assets { get; set; }
}

public class GithubAsset {
	public string Name { get; set; }
	public string Browser_Download_Url { get; set; }
	public long Size { get; set; }
}