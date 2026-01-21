using System.Windows;

namespace YtDlpGui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private const string ConfigPath = "config.json";

	private Config config;
	private YtDlpManager ytDlpManager;

	public MainWindow()
	{
		InitializeComponent();
		config = new Config("");
		ytDlpManager = new YtDlpManager(config);
		//LoadConfig().ConfigureAwait(false);
	}

	private async Task LoadConfig()
	{
		(Config? config, string? error) = await Config.Load(ConfigPath);
		if (config != null)
		{
			this.config = config;
			return;
		}
		string message = "Failed to read config";
		if (error != null)
			message += ":\n" + error;
		MessageBox.Show(message);
		await Configure();
	}

	private async Task Configure()
	{
		IsEnabled = false;
		var dialog = new ConfigDialog();
		if (dialog.ShowDialog() == true)
		{
			try
			{
				config = new Config(dialog.YtDlpPath, dialog.DownloadDir);
				await config.Save(ConfigPath);
				ytDlpManager = new YtDlpManager(config);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		IsEnabled = true;
	}

	// Events

	private void CheckForUpdatesMenuItem_Click(object sender, RoutedEventArgs e)
	{
		new CheckForUpdatesWindow(ytDlpManager).ShowDialog();
	}

	private void DownloadMenuItem_Click(object sender, RoutedEventArgs e)
	{
		new DownloadWindow(ytDlpManager).ShowDialog();
	}

	private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
	{
		MessageBox.Show(
			$"YtDlpGui v{GetType().Assembly.GetName().Version} for Windows x64\nCopyright © 2026 Artem Melekhin",
			"About YtDlpGui");
	}
}
