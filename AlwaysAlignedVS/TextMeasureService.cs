using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace AlwaysAligned
{
	public class TextMeasureService
	{
		private readonly Dictionary<char, double> _symbolWidthCache = new Dictionary<char, double>();
		private IWpfTextView _textView;
		private TextRunProperties _defaultTextRunProperties;

		private TextMeasureService()
		{
		}

		public static TextMeasureService Create(IWpfTextView textView)
		{
			TextMeasureService tms = GetOrCreateProperty(textView);

			//font was changed, refresh cache
			if (tms._defaultTextRunProperties != textView.FormattedLineSource.DefaultTextProperties)
			{
				textView.Properties.RemoveProperty(typeof(TextMeasureService));
				tms = GetOrCreateProperty(textView);
			}

			return tms;
		}

		private static TextMeasureService GetOrCreateProperty(IWpfTextView textView)
		{
			TextMeasureService tms = textView.Properties.GetOrCreateSingletonProperty(() => new TextMeasureService
			{
				_textView = textView,
				_defaultTextRunProperties = textView.FormattedLineSource.DefaultTextProperties
			});
			return tms;
		}

		public double GetWidth(SnapshotSpan span)
		{
			double widthSum = 0;
			for (int i = 0; i < span.Length; i++)
			{
				var curPoint = span.Start + i;
				widthSum += GetWidthFromCache(curPoint);
			}
			return widthSum;
		}

		public double GetCharWidth(char ch) // TODO: can this be made private?
		{
			if (_symbolWidthCache.ContainsKey(ch))
			{
				return _symbolWidthCache[ch];
			}
			//emSize = emSize > 0 ? emSize : _textView.FormattedLineSource.DefaultTextProperties.FontRenderingEmSize;
			var formattedText = new FormattedText(
				ch.ToString(CultureInfo.InvariantCulture),
				_textView.FormattedLineSource.DefaultTextProperties.CultureInfo,
				FlowDirection.LeftToRight,
				_textView.FormattedLineSource.DefaultTextProperties.Typeface,
				_textView.FormattedLineSource.DefaultTextProperties.FontRenderingEmSize,
				_textView.FormattedLineSource.DefaultTextProperties.ForegroundBrush,
				_textView.FormattedLineSource.DefaultTextProperties.NumberSubstitution,
				TextFormattingMode.Display);

			_symbolWidthCache[ch] = formattedText.WidthIncludingTrailingWhitespace;

			return formattedText.WidthIncludingTrailingWhitespace;
		}

		private double GetWidthFromCache(SnapshotPoint point)
		{
			var ch = point.GetChar();
			return GetCharWidth(ch);
		}
	}
}
