using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace AlwaysAligned
{
	[Export(typeof(IWpfTextViewCreationListener))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	public class UpdateTabCreationListener : IWpfTextViewCreationListener
	{
		private class UpdateTabListener
		{
			readonly IWpfTextView _textView;
			readonly ITextDataModel _dataModel;

			private INormalizedTextChangeCollection _lastChange;

			public UpdateTabListener(IWpfTextView textView)
			{
				_textView = textView;
				_dataModel = _textView.TextViewModel.DataModel;

				_textView.Closed += OnClosed;
				_dataModel.ContentTypeChanged += ChangedHighPriority;

				_textView.TextBuffer.PostChanged += OnBufferPostChanged;
				_textView.TextBuffer.Changed += OnBufferChanged;

				_textView.Caret.PositionChanged += CaretPositionChanged;

				AlwaysAlignedConfigurationService.Instance.ConfigurationChanged += InstanceConfigurationSaved;

			}

			private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
			{
				_lastChange = e.Changes;
			}

			void CaretPositionChanged(object sender, CaretPositionChangedEventArgs args)
			{
				if (!AlwaysAlignedConfigurationService.Instance.Config.Enabled) return;

				if (_lastChange == null) return;

				if (!_textView.Caret.InVirtualSpace)
				{
					_lastChange = null;
					return;
				}

				var rtCount = Math.Max(_textView.Caret.Position.VirtualSpaces / _textView.FormattedLineSource.TabSize, 0);
				var rtEdit = _textView.TextBuffer.CreateEdit();
				rtEdit.Insert(_textView.Caret.ContainingTextViewLine.Start.Position, new string('\t', rtCount));
				rtEdit.Apply();
				var line = _textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(_textView.Caret.Position.BufferPosition);
				_textView.Caret.MoveTo(line.End);

			}

			void InstanceConfigurationSaved(object sender, ConfigurationSavedEventArgs e)
			{
				if (!e.HasChanges) return;

				var tabStopsManager = ElasticTabstopsSizeManager.Get(_textView);
				tabStopsManager.InvalidateChanges();

				RefreshView();
			}

			void ChangedHighPriority(object sender, TextDataModelContentTypeChangedEventArgs e)
			{
				if (!e.AfterContentType.IsOfType("code"))
				{
					// we are no longer a "code" content type so unhook all the events so we can be garbage collected
					OnClosed(null, null);
				}
			}

			private void OnBufferPostChanged(object sender, EventArgs e)
			{
				if (!AlwaysAlignedConfigurationService.Instance.Config.Enabled) return;

				var tabStopsManager = ElasticTabstopsSizeManager.Get(_textView);
				if (tabStopsManager != null)
				{
					tabStopsManager.InvalidateChanges(_lastChange);
					RefreshView();
				}
			}

			private void RefreshView()
			{
				int oldTabSize = _textView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
				_textView.Options.SetOptionValue(DefaultOptions.TabSizeOptionId, oldTabSize + 1);
				_textView.Options.SetOptionValue(DefaultOptions.TabSizeOptionId, oldTabSize);
			}

			private void OnClosed(object sender, EventArgs e)
			{
				_textView.Closed -= OnClosed;
				_textView.TextBuffer.PostChanged -= OnBufferPostChanged;
				_dataModel.ContentTypeChanged -= ChangedHighPriority;
				AlwaysAlignedConfigurationService.Instance.ConfigurationChanged -= InstanceConfigurationSaved;

			}
		}

		public void TextViewCreated(IWpfTextView textView)
		{
			new UpdateTabListener(textView);
		}
	}
}
