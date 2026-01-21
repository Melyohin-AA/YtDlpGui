using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace YtDlpGui;

public partial class YtDlpManager
{
	private readonly HttpClient http;

	public Config Config { get; }
	public string FilePath { get; }

	public YtDlpManager(Config config)
	{
		http = new HttpClient();
		Config = config;
		FilePath = Path.Combine(config.YtDlpPath, "yt-dlp.exe");
	}

	public async Task<Version> GetLocalVersion()
	{
		Process process = Process.Start(new ProcessStartInfo {
			FileName = FilePath,
			Arguments = "--version",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true,
		})!;
		await process.WaitForExitAsync();
		string output = await process.StandardOutput.ReadToEndAsync();
		return Version.Parse(output.Trim());
	}

	public async Task DownloadVideo(string sourceUrl,
		VideoDownloadingTarget target = VideoDownloadingTarget.VideoAudio,
		string? cookieFilePath = null,
		byte playlistDigitNumber = 0,
		string? sections = null,
		bool writeThumbnail = false,
		bool writeSubs = false,
		bool ejs = false)
	{
		var args = new StringBuilder();
		args.Append(target switch {
			VideoDownloadingTarget.VideoAudio => "-f bestvideo*+bestaudio/best",
			VideoDownloadingTarget.VideoOnly => "-f bestvideo*/best",
			VideoDownloadingTarget.AudioOnly => "-f bestaudio/best -x",
			VideoDownloadingTarget.Storyboard => "-f sb0",
			_ => throw new Exception(),
		});
		if (ejs)
			args.Append(" --js-runtimes node");
		if (cookieFilePath != null)
			args.Append($" --cookies \"{cookieFilePath}\"");
		args.Append((playlistDigitNumber == 0) ?
			" -o \"dl/%(title)s [%(id)s].%(ext)s\"" :
			" --yes-playlist -o \"dl/%(playlist)s [%(playlist_id)s]/" +
			$"%(playlist_index)0{playlistDigitNumber}d. %(title)s [%(id)s].%(ext)s\"");
		if (writeThumbnail)
			args.Append(" --write-thumbnail");
		if (writeSubs)
			args.Append(" --write-subs");
		if (sections != null)
			args.Append($" --download-sections \"{sections}\"");
		args.Append($" \"{sourceUrl}\"");
		Process process = StartWindowedProcess(args.ToString());
		await process.WaitForExitAsync();
	}

	public async Task CheckFormats(string sourceUrl, string? cookieFilePath = null, bool ejs = false)
	{
		var args = new StringBuilder();
		if (ejs)
			args.Append("--js-runtimes node");
		if (cookieFilePath != null)
			args.Append($" --cookies \"{cookieFilePath}\"");
		args.Append($" -F \"{sourceUrl}\"");
		Process process = StartWindowedProcess(args.ToString());
		await process.WaitForExitAsync();
	}

	public static string SanitizeSourceUrl(string sourceUrl)
	{
		int i = sourceUrl.IndexOf('?');
		if (i == -1)
			return sourceUrl;
		Match videoId = VideoIdRegex().Match(sourceUrl, i);
		return videoId.Success ? sourceUrl.Remove(i + 1) + videoId.Value : sourceUrl;
	}

	[GeneratedRegex(@"v=[\w_\-]+", RegexOptions.Compiled)]
	private static partial Regex VideoIdRegex();

	public async Task Update()
	{
		Process process = StartWindowedProcess("-U");
		await process.WaitForExitAsync();
	}

	private Process StartWindowedProcess(string args)
	{
		args = $"/k {FilePath} {args}";
		return Process.Start(new ProcessStartInfo {
			FileName = "cmd.exe",
			Arguments = args,
			UseShellExecute = false,
		})!;
	}

	public async Task<ReleaseInfo> FetchLatestRelease()
	{
		var request = new HttpRequestMessage(HttpMethod.Get,
			$"https://api.github.com/repos/{Config.Owner}/{Config.Repo}/releases/latest");
		if (!request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json"))
			throw new Exception();
		if (!request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28"))
			throw new Exception();
		request.Headers.UserAgent.ParseAdd("YtDlpGui/1.0.0");
		HttpResponseMessage response = await http.SendAsync(request);
		if (response.StatusCode != HttpStatusCode.OK)
			throw new Exception();
		string content = await response.Content.ReadAsStringAsync();
		return ReleaseInfo.Parse(content);
	}

	public async Task Update(Version currentVersion, ReleaseInfo release, string assetName)
	{
		File.Move(FilePath, GetOldFilePath(currentVersion));
		HttpResponseMessage response = await http.GetAsync(release.DownloadUrls[assetName]);
		if (response.StatusCode != HttpStatusCode.OK)
			throw new Exception();
		Stream content = await response.Content.ReadAsStreamAsync();
		using var file = new FileStream(FilePath, FileMode.CreateNew);
		await content.CopyToAsync(file);
	}

	private string GetOldFilePath(Version oldVersion)
	{
		return Path.Combine(Config.YtDlpPath, $"yt-dlp-{oldVersion}.exe");
	}
}
