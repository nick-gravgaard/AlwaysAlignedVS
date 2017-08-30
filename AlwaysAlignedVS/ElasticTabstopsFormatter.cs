using Microsoft.VisualStudio.Text.Formatting;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.TextFormatting;

namespace AlwaysAligned
{
	/// <summary>
	/// Provides text formatting properties.
	/// </summary>
	internal class ElasticTabstopsFormatter : TextFormattingParagraphProperties
	{
		private readonly double[] _tabSizes;

		/// <summary>
		/// Creates an instance of ElasticTabstopsFormatter
		/// </summary>
		internal ElasticTabstopsFormatter(
			TextFormattingRunProperties textProperties,
			IFormattedLineSource formattedLineSource, double[] tabSizes)
			: base(textProperties, formattedLineSource.ColumnWidth * formattedLineSource.TabSize)
		{
			_tabSizes = tabSizes;
		}

		/// <summary>
		/// Gets a collection of tab definitions.
		/// </summary>
		public override IList<TextTabProperties> Tabs
		{
			get
			{
				var tabList = _tabSizes.Select((ts, i) => new TextTabProperties(TextTabAlignment.Left, ts, 0, 0)).ToList();
				return tabList;
			}
		}
	}
}
