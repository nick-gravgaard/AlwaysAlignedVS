using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;

namespace AlwaysAligned
{
	class CommandFilter : IOleCommandTarget
	{
		public CommandFilter(IWpfTextView view)
		{
			//changeMenuText();
		}

		internal IOleCommandTarget Next { get; set; }

		public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (pguidCmdGroup == GuidList.guidAlwaysAlignedCmdSet)
			{
				switch (nCmdID)
				{
					case PkgCmdIDList.cmdidSettings:
						_btnConfigure_Click();
						return VSConstants.S_OK;
					case PkgCmdIDList.cmdidConvertToSpaces:
						_btnAlignSpaces_Click();
						return VSConstants.S_OK;
					case PkgCmdIDList.cmdidConvertToElasticTabstops:
						_btnAlignTabsElastic_Click();
						return VSConstants.S_OK;
					case PkgCmdIDList.cmdidAbout:
						_btnInfoDialog_Click();
						return VSConstants.S_OK;
					default:
						break;
				}
			}

			return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
		}

		public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
		{
			if (pguidCmdGroup == GuidList.guidAlwaysAlignedCmdSet)
			{
				switch (prgCmds[0].cmdID)
				{
					case PkgCmdIDList.cmdidSettings:
						prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
						break;
					case PkgCmdIDList.cmdidConvertToSpaces:
						prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
						break;
					case PkgCmdIDList.cmdidConvertToElasticTabstops:
						prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
						break;
					case PkgCmdIDList.cmdidAbout:
						prgCmds[0].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
						break;
					default:
						break;
				}
				return VSConstants.S_OK;
			}

			return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
		}

		private void _btnAlignSpaces_Click()
		{
			if (AppInfo.appObject.ActiveDocument == null) return;

			var doc = (TextDocument)AppInfo.appObject.ActiveDocument.Object("TextDocument");
			if (doc == null) return;

			string text = doc.StartPoint.CreateEditPoint().GetText(doc.EndPoint);
			string convertedText = ElasticTabstopsConverter.ToSpaces(text, doc.TabSize);
			doc.ReplaceText(text, convertedText);
		}

		private void _btnAlignTabsElastic_Click()
		{
			if (AppInfo.appObject.ActiveDocument == null) return;

			var doc = (TextDocument)AppInfo.appObject.ActiveDocument.Object("TextDocument");
			if (doc == null) return;

			string text = doc.StartPoint.CreateEditPoint().GetText(doc.EndPoint);
			string convertedText = ElasticTabstopsConverter.ToElasticTabstops(text, doc.TabSize);
			doc.ReplaceText(text, convertedText);
		}

		private void _btnConfigure_Click()
		{
			var sd = new SettingsDialog
			{
				WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
				ShowInTaskbar = false
			};
			sd.ShowDialog();
		}

		private void _btnInfoDialog_Click()
		{
			var sd = new About
			{
				WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
				ShowInTaskbar = false
			};
			sd.ShowDialog();
		}

	}
}
