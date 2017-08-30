using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Reflection;

namespace AlwaysAligned
{
	/// <summary>
	/// Interaction logic for About.xaml
	/// </summary>
	public partial class About : Window
	{
		public About()
		{
			InitializeComponent();

			string informationalVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
			var fieldsParagraph = new Paragraph();
			fieldsParagraph.Inlines.Add(new Run("Version: "));
			fieldsParagraph.Inlines.Add(new Bold(new Run(informationalVersion)));

			AboutFlowDoc.Blocks.InsertBefore(AboutFlowDoc.Blocks.FirstBlock, fieldsParagraph);
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

	}
}
