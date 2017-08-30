using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media.TextFormatting;

namespace AlwaysAligned
{
	/// <summary>
	/// Creates ElasticTabstopsFormatter classes
	/// to be used when lines on the view are being formatted.
	/// </summary>
	[Export(typeof(ITextParagraphPropertiesFactoryService))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	internal class ElasticTabstopsProvider : ITextParagraphPropertiesFactoryService
	{
		[Import]
		private ITextBufferToViewMapService _textBufferToViewMapService = null;

		/// <summary>
		/// Creates an ElasticTabstopsFormatters for
		/// the provided configuration.
		/// </summary>
		public TextParagraphProperties Create(IFormattedLineSource formattedLineSource, TextFormattingRunProperties textProperties, 
			IMappingSpan line, IMappingPoint lineStart, int lineSegment)
		{
			if (!AlwaysAlignedConfigurationService.Instance.GetConfiguration().Enabled)
			{
				return new TextFormattingParagraphProperties(textProperties, formattedLineSource.ColumnWidth * formattedLineSource.TabSize);
			}

			IWpfTextView textView = _textBufferToViewMapService.GetViewByFormattedLineSource(formattedLineSource);
			//View is not initialized yet
			if (textView == null)
			{
				return new TextFormattingParagraphProperties(textProperties, formattedLineSource.ColumnWidth * formattedLineSource.TabSize);
			}
			var manager = ElasticTabstopsSizeManager.Get(textView);

			ITextSnapshot textSnapshot = formattedLineSource.SourceTextSnapshot;
			ITextBuffer textBuffer = textSnapshot.TextBuffer;			

			var normalizedspancoll = line.GetSpans(textBuffer);
			ITextSnapshotLine currentLine = textSnapshot.GetLineFromPosition(normalizedspancoll.First().Start.Position);
			
			//Get tab offset calculated by ElasticTabstopsSizeManager
			double[] tabOffsets = manager.GetTabOffsets(currentLine);

			return new ElasticTabstopsFormatter(textProperties, formattedLineSource, tabOffsets);
		}
	}
}
