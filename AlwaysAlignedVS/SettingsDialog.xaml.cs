using System;
using System.Windows;
using System.Windows.Input;

namespace AlwaysAligned
{
	/// <summary>
	/// Interaction logic for SettingsDialog.xaml
	/// </summary>
	public partial class SettingsDialog
	{
		private bool postInit = false;
		private AlwaysAlignedConfiguration config;

		public SettingsDialog()
		{
			config = (AlwaysAlignedConfiguration)AlwaysAlignedConfigurationService.Instance.GetConfiguration().Clone();
			InitializeComponent();
			postInit = true;
		}

		public AlwaysAlignedConfiguration Configuration
		{
			get
			{
				return config;
			}
		}

		private void widgetChanged(object sender, EventArgs e)
		{
			if (postInit)
			{
				saveButton.IsEnabled = true;
			}
		}

		private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			int result;
			if (!int.TryParse(e.Text, out result))
			{
				e.Handled = true;
			}
		}

		private void saveButton_Click(object sender, RoutedEventArgs e)
		{
			AlwaysAlignedConfigurationService.Instance.Save(Configuration);
			this.Close();
		}

		private void dontSaveButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
