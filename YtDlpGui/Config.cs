using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YtDlpGui;

public class Config
{
	public const string DefaultOwner = "yt-dlp", DefaultRepo = "yt-dlp";

	public string YtDlpPath { get; }
	public string Owner { get; }
	public string Repo { get; }

	public Config(string ytDlpPath, string owner = DefaultOwner, string repo = DefaultRepo)
	{
		YtDlpPath = ytDlpPath;
		Owner = owner;
		Repo = repo;
	}

	public async Task Save(string path)
	{
		string content = new JObject {
			{ "ytDlpPath", YtDlpPath },
			{ "owner", Owner },
			{ "repo", Repo },
		}.ToString(Formatting.None);
		await File.WriteAllTextAsync(path, content);
	}

	public static async Task<(Config?, string?)> Load(string path)
	{
		try
		{
			if (!File.Exists(path))
				return (null, "Config file not found!");
			string content = await File.ReadAllTextAsync(path);
			JObject jobj = JObject.Parse(content);
			string? ytDlpPath = (string?)(JValue?)jobj["ytDlpPath"];
			if ((ytDlpPath == null) || !File.Exists(ytDlpPath))
				return (null, "Wrong value of 'ytDlpPath'!");
			string owner = (string?)(JValue?)jobj["owner"] ?? DefaultOwner;
			string repo = (string?)(JValue?)jobj["repo"] ?? DefaultRepo;
			return (new Config(ytDlpPath, owner, repo), null);
		}
		catch (Exception ex)
		{
			return (null, ex.ToString());
		}
	}
}
