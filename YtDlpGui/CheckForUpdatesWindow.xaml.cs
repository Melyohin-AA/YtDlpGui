using System.Windows;

namespace YtDlpGui;

/// <summary>
/// Interaction logic for CheckForUpdatesDialog.xaml
/// </summary>
public partial class CheckForUpdatesWindow : Window
{
	private const string NewerVersionReleasedMessage = "A newer version of yt-dlp is out!";
	private const string LocalVersionIsLatestMessage = "Your local yt-dlp version is the latest.";
	private const string LocalVersionUpdatedMessage = "yt-dlp has been successfully updated.";

	private readonly YtDlpManager ytDlpManager;
	private Version localVersion;
	private ReleaseInfo latestRelease;

	public CheckForUpdatesWindow(YtDlpManager ytDlpManager)
	{
		InitializeComponent();
		this.ytDlpManager = ytDlpManager;
		GetVersions().ConfigureAwait(false);
	}

	private async Task GetVersions()
	{
		Task<Version> localVersionTask = GetLocalVersion();
		Task<ReleaseInfo> latestReleaseTask = FetchLatestRelease();
		await Task.WhenAll(localVersionTask, latestReleaseTask);
		localVersion = await localVersionTask;
		latestRelease = await latestReleaseTask;
		bool canUpdate = localVersion.CompareTo(latestRelease.Version) < 0;
		statusLabel.Content = canUpdate ? NewerVersionReleasedMessage : LocalVersionIsLatestMessage;
		if (canUpdate)
			updateButton.IsEnabled = true;
	}

	private async Task<Version> GetLocalVersion()
	{
		Version localVersion = await ytDlpManager.GetLocalVersion();
		localVersionTextBox.Text = localVersion.ToString();
		return localVersion;
	}

	private async Task<ReleaseInfo> FetchLatestRelease()
	{
		ReleaseInfo latestRelease = await ytDlpManager.FetchLatestRelease();
		latestReleaseTextBox.Text = latestRelease.Version.ToString();
		return latestRelease;
	}

	private async Task Update()
	{
		IsEnabled = false;
		await ytDlpManager.Update(localVersion, latestRelease, "yt-dlp.exe");
		updateButton.IsEnabled = false;
		localVersion = await ytDlpManager.GetLocalVersion();
		localVersionTextBox.Text = localVersion.ToString();
		if (localVersion.CompareTo(latestRelease.Version) != 0)
			throw new Exception();
		IsEnabled = true;
		statusLabel.Content = LocalVersionUpdatedMessage;
	}

	private async Task DU()
	{
		IsEnabled = false;
		await ytDlpManager.Update();
		localVersion = await ytDlpManager.GetLocalVersion();
		localVersionTextBox.Text = localVersion.ToString();
		IsEnabled = true;
	}

	// Events

	private void UpdateButton_Click(object sender, RoutedEventArgs e)
	{
		Update().ConfigureAwait(false);
	}

	private void DUButton_Click(object sender, RoutedEventArgs e)
	{
		DU().ConfigureAwait(false);
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
