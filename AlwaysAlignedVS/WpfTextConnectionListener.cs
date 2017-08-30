using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;

namespace AlwaysAligned
{
	[Export(typeof(IWpfTextViewConnectionListener))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	internal class WpfTextConnectionListener : IWpfTextViewConnectionListener
	{
		[Import]
		ITextDocumentFactoryService _textDocumentFactoryService = null;

		public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
		{
			ITextDocument textDocument;
			if (_textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out textDocument))
			{
				textDocument.FileActionOccurred += delegate(object sender, TextDocumentFileActionEventArgs e)
				{
					if (e.FileActionType != FileActionTypes.ContentSavedToDisk) return;

					ITextDocument document = (ITextDocument)sender;
					ITextBuffer buffer = document.TextBuffer;
					string filePath = e.FilePath;

					FixBufferOnSave(buffer, textView.FormattedLineSource.TabSize, filePath);
				};
			}
			foreach (var buffer in subjectBuffers)
			{
				IEditorOptions options = textView.Properties.GetProperty<IEditorOptions>(typeof(IEditorOptions));
				int tabSize = options.GetTabSize();
				FixBufferOnLoad(buffer, tabSize);
			}

			if(textDocument != null)
			{
				textDocument.UpdateDirtyState(false, DateTime.Now);
			}
		}

		public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
		{

		}

		/// <summary>
		/// Called on buffer load
		/// </summary>
		private static void FixBufferOnLoad(ITextBuffer buffer, int tabSize)
		{
			if (!AlwaysAlignedConfigurationService.Instance.GetConfiguration().ConvertOnLoadSave) return;

			string str = buffer.CurrentSnapshot.GetText();
			if (str.Contains("\t"))
			{
				MiscGui.WriteOutput("Always Aligned is set to convert files from using spaces when loading, but at least one tab was found. The file will be left alone.");
			}
			else
			{
				string convertedText = ElasticTabstopsConverter.ToElasticTabstops(str, tabSize);
				ITextEdit tb = buffer.CreateEdit();
				tb.Replace(new Span(0, buffer.CurrentSnapshot.Length), convertedText);
				tb.Apply();
			}
		}

		/// <summary>
		/// Called on buffer save
		/// </summary>
		private void FixBufferOnSave(ITextBuffer buffer, int tabSize, string filePath)
		{
			if (!AlwaysAlignedConfigurationService.Instance.GetConfiguration().ConvertOnLoadSave) return;

			using (StreamWriter streamWriter = new StreamWriter(filePath))
			{
				string text = buffer.CurrentSnapshot.GetText();
				string convertedText = ElasticTabstopsConverter.ToSpaces(text, tabSize);
				streamWriter.Write(convertedText);
			}
		}


	}
}
