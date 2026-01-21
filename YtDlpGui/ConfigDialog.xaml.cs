using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YtDlpGui;

/// <summary>
/// Interaction logic for ConfigDialog.xaml
/// </summary>
public partial class ConfigDialog : Window
{
	public string YtDlpPath { get; }
	public string DownloadDir { get; }

	public ConfigDialog()
	{
		InitializeComponent();
		YtDlpPath = "";
		DownloadDir = "";
	}
}
