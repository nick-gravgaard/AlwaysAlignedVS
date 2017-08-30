using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AlwaysAligned
{
	/// <summary>
	/// Interaction logic for Logo.xaml
	/// </summary>
	public partial class Logo : UserControl
	{
		DispatcherTimer dispatcherTimer = new DispatcherTimer();
		private int preAnimDelay = 6;
		private readonly int numFrames = 14;
		private int currentFrameNum = 0;
		private int logoHeight;

		public Logo()
		{
			InitializeComponent();

			logoHeight = (int)spritesheet.Source.Height / numFrames;

			dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(250);
			dispatcherTimer.Start();

			this.DataContext = this; // so we can bind LogoHeight
		}

		public double LogoHeight
		{
			get { return logoHeight; }
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			if (preAnimDelay > 0)
			{
				preAnimDelay--;
				return;
			}
			currentFrameNum++;
			if (currentFrameNum >= numFrames - 1)
			{
				dispatcherTimer.Stop();
			}
			scrollViewer.ScrollToVerticalOffset(logoHeight * currentFrameNum);
		}

	}
}
