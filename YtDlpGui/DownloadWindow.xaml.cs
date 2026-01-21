using System.Windows;
using Microsoft.Win32;

namespace YtDlpGui
{
	/// <summary>
	/// Interaction logic for DownloadWindow.xaml
	/// </summary>
	public partial class DownloadWindow : Window
	{
		private readonly YtDlpManager ytDlpManager;

		public DownloadWindow(YtDlpManager ytDlpManager)
		{
			InitializeComponent();
			this.ytDlpManager = ytDlpManager;
		}

		private async Task Download()
		{
			IsEnabled = false;
			await ytDlpManager.DownloadVideo(sourceUrlTextBox.Text,
				target: (VideoDownloadingTarget)targetComboBox.SelectedIndex,
				cookieFilePath: (useCookiesCheckBox.IsChecked == true) ? cookieFileTextBox.Text : null,
				playlistDigitNumber: (isPlaylistCheckBox.IsChecked == true) ? (byte)digitNumberSlider.Value : (byte)0,
				sections: (downloadSectionsCheckBox.IsChecked == true) ? downloadSectionsTextBox.Text : null,
				writeThumbnail: writeThumbnailCheckBox.IsChecked == true,
				writeSubs: writeSubsCheckBox.IsChecked == true,
				ejs: ejsCheckBox.IsChecked == true);
			IsEnabled = true;
		}

		private async Task CheckFormats()
		{
			IsEnabled = false;
			await ytDlpManager.CheckFormats(sourceUrlTextBox.Text,
				cookieFilePath: (useCookiesCheckBox.IsChecked == true) ? cookieFileTextBox.Text : null,
				ejs: ejsCheckBox.IsChecked == true);
			IsEnabled = true;
		}

		private void SanitizeSourceUrl()
		{
			sourceUrlTextBox.Text = YtDlpManager.SanitizeSourceUrl(sourceUrlTextBox.Text);
		}

		// Events

		private void SelectCookieFileButton_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog { Filter = "Cookie files (*.txt)|*.txt" };
			if (dialog.ShowDialog() == false)
				return;
			cookieFileTextBox.Text = dialog.FileName;
		}

		private void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
			SanitizeSourceUrl();
			Download().ConfigureAwait(false);
		}

		private void CheckFormatsButton_Click(object sender, RoutedEventArgs e)
		{
			SanitizeSourceUrl();
			CheckFormats().ConfigureAwait(false);
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
	}
}
