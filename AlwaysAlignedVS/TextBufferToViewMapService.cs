using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace AlwaysAligned
{
	public class TextViewLoadedEventArgs : EventArgs
	{
		public TextViewLoadedEventArgs (IWpfTextView textView)
		{
			TextView = textView;
		}
		public IWpfTextView TextView { get; private set; }
	}
	public interface ITextBufferToViewMapService
	{
		IWpfTextView[] GetAllViews(ITextBuffer textBuffer);
		IWpfTextView GetViewByFormattedLineSource(IFormattedLineSource formattedLineSource);
	}

	[Export(typeof(ITextBufferToViewMapService))]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	[ContentType("text")]
	[Name("FakeMarginProvider")]
	[Export(typeof(IWpfTextViewMarginProvider))]
	[Order(Before = PredefinedMarginNames.VerticalScrollBarContainer)]
	[MarginContainer(PredefinedMarginNames.RightControl)]
	public class TextBufferToViewMapService : 
		ITextBufferToViewMapService, IWpfTextViewMarginProvider
	{
		/// <summary>
		/// Fake margin that keeps _wpfTextViewList list up to date
		/// </summary>
		private class FakeWpfTextViewMargin : FrameworkElement, IWpfTextViewMargin
		{
			private readonly TextBufferToViewMapService _textBufferToViewMapService;
			private readonly IWpfTextView _wpfTextView;

			public FakeWpfTextViewMargin(TextBufferToViewMapService textBufferToViewMapService, IWpfTextView iWpfTextView)
			{
				_textBufferToViewMapService = textBufferToViewMapService;
				_wpfTextView = iWpfTextView;
				_textBufferToViewMapService._wpfTextViewList.Add(_wpfTextView);
			}
			public FrameworkElement VisualElement
			{
				get { return this; }
			}

			public bool Enabled
			{
				get { return false; }
			}

			public ITextViewMargin GetTextViewMargin(string marginName)
			{
				return this;
			}

			public double MarginSize
			{
				get { return 0; }
			}

			public void Dispose()
			{
				_textBufferToViewMapService._wpfTextViewList.Remove(_wpfTextView);
			}
		}

		private readonly IList<IWpfTextView> _wpfTextViewList = new List<IWpfTextView>();

		public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
		{
			ElasticTabstopsSizeManager.Create(wpfTextViewHost.TextView);
			return new FakeWpfTextViewMargin(this, wpfTextViewHost.TextView);
		}

		/// <summary>
		/// Get all views for a given ITextBuffer
		/// </summary>
		public IWpfTextView[] GetAllViews(ITextBuffer textBuffer)
		{
			return _wpfTextViewList.Where(view => view.TextBuffer == textBuffer).ToArray();
		}
		
		/// <summary>
		/// Return view that contains given IFormattedLineSource
		/// </summary>
		public IWpfTextView GetViewByFormattedLineSource(IFormattedLineSource formattedLineSource)
		{
			return _wpfTextViewList.FirstOrDefault(view => view.FormattedLineSource == formattedLineSource);
		}
	}
}
