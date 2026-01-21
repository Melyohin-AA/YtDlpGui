using Newtonsoft.Json.Linq;

namespace YtDlpGui;

public struct ReleaseInfo
{
	public Version Version { get; }
	public IReadOnlyDictionary<string, string> DownloadUrls { get; }

	public ReleaseInfo(Version version, IReadOnlyDictionary<string, string> downloadUrls)
	{
		Version = version;
		DownloadUrls = downloadUrls;
	}

	public static ReleaseInfo Parse(string str)
	{
		JObject root = JObject.Parse(str);
		string strVersion = (string?)(JValue?)root["tag_name"] ?? "0";
		Version version = Version.Parse(strVersion);
		var assets = (JArray?)root["assets"] ?? new JArray();
		Dictionary<string, string> downloadUrls = assets.Cast<JObject>().
			ToDictionary(a => (string?)a["name"] ?? "", a => (string?)a["browser_download_url"] ?? "");
		return new ReleaseInfo(version, downloadUrls);
	}
}
