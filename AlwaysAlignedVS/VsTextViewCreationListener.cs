using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace AlwaysAligned
{
	[Export(typeof(IVsTextViewCreationListener))]
	[ContentType("code")]
	[TextViewRole(PredefinedTextViewRoles.Editable)]
	class VsTextViewCreationListener : IVsTextViewCreationListener
	{
		[Import]
		IVsEditorAdaptersFactoryService AdaptersFactory = null;
		
		public void VsTextViewCreated(IVsTextView textViewAdapter)
		{
			var wpfTextView = AdaptersFactory.GetWpfTextView(textViewAdapter);
			if (wpfTextView == null)
			{
				Debug.Fail("Unable to get IWpfTextView from text view adapter");
				return;
			}

			CommandFilter filter = new CommandFilter(wpfTextView);

			IOleCommandTarget next;
			if (ErrorHandler.Succeeded(textViewAdapter.AddCommandFilter(filter, out next)))
			{
				filter.Next = next;
			}
		}
	}
}
