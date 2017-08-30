using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace AlwaysAligned
{
	class MiscGui
	{
		internal abstract class EnvDTEConstants
		{
			public const string vsWindowKindOutput = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";
		}

		public static void WriteOutput(string outputText)
		{
			IVsOutputWindow outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

			Guid guidGeneral = Microsoft.VisualStudio.VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
			IVsOutputWindowPane pane;
			int hr = outputWindow.CreatePane(guidGeneral, "General", 1, 0);
			hr = outputWindow.GetPane(guidGeneral, out pane);
			pane.Activate();
			pane.OutputString(outputText);
			pane.Activate();

			DTE dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
			Window win = dte.Windows.Item(EnvDTEConstants.vsWindowKindOutput);
			win.Visible = true;
		}

		public static void ShowModal(string title, string caption, OLEMSGBUTTON msgbtn, OLEMSGDEFBUTTON msgdefbtn, OLEMSGICON msgicon)
		{
			ServiceProvider serviceProvider = new ServiceProvider(((DTE)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(DTE))) as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
			IVsUIShell uiShell = serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;

			var id = Guid.Empty;
			int result;
			uiShell.ShowMessageBox(
				0, ref id,
				title, caption,
				string.Empty, 0,
				msgbtn, msgdefbtn, msgicon,
				0, out result);
		}
	}
}
