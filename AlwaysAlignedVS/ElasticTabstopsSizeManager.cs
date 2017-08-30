using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlwaysAligned
{
	/// <summary>
	/// Represents class that calculates elastic tabstops for TextBuffer
	/// </summary>
	internal class ElasticTabstopsSizeManager
	{
		#region Inner Definitions

		private enum CalculateDirection
		{
			Down,
			Up,
			DownUp
		}

		/// <summary>
		/// Keep Elastic Tabstop data for a line
		/// </summary>
		private class ElasticTabstopsLine
		{
			/// <summary>
			/// Gets or sets Elastic Columns in the line
			/// </summary>
			internal ElasticTabstopsColumn[] ElasticColumns { get; set; }

			/// <summary>
			/// Returns elastic column information if exists, or null otherwise
			/// </summary>
			internal ElasticTabstopsColumn GetColumnOrDefault(int colNum)
			{
				if (ElasticColumns.Length > colNum)
					return ElasticColumns[colNum];
				return null;
			}

			/// <summary>
			/// Returns true if given column is the last column in the line, 
			/// false otherwise
			/// </summary>
			internal bool IsLastColumnInLine(int colNum)
			{
				return colNum + 1 == ElasticColumns.Length;
			}

			/// <summary>
			/// Returns true if current line offsets changed regarding to given line
			/// false otherwise
			/// </summary>
			internal bool ChangedRegardingTo(ElasticTabstopsLine oldLine)
			{
				if (ElasticColumns == oldLine.ElasticColumns)
					return false;

				if (ElasticColumns.Length != oldLine.ElasticColumns.Length)
					return true;

				return ElasticColumns.Where((etc, i) => etc.ChangedRegardingTo(oldLine.ElasticColumns[i])).Any();
			}
		}

		/// <summary>
		/// Keeps data about ElasticColumn
		/// </summary>
		private class ElasticTabstopsColumn
		{
			/// <summary>
			/// Gets or sets start position of this column in the TextBuffer
			/// </summary>
			internal int Start { get; set; }
			
			/// <summary>
			/// Gets or sets text length in the line
			/// </summary>
			internal int ColumnTextLength { get; set; }
			/// <summary>
			/// Gets or sets Tab Offset of the line
			/// </summary>
			internal ColumnSizeInfo TabOffset { get; set; }

			/// <summary>
			/// Returns true if column contains changed, false otherwise
			/// </summary>
			internal bool ChangedRegardingTo(ElasticTabstopsColumn elasticTabstopsColumn)
			{
				if (this == elasticTabstopsColumn)
					return false;

				if (ColumnTextLength != elasticTabstopsColumn.ColumnTextLength)
					return true;

				return TabOffset.ChangedRegardingTo(elasticTabstopsColumn.TabOffset);
			}
		}

		/// <summary>
		/// Keep data about tab ofset
		/// </summary>
		private class ColumnSizeInfo
		{
			/// <summary>
			/// Real tab offset of the column
			/// </summary>
			internal double TabOffset { get; set; }
			/// <summary>
			/// Column real width
			/// </summary>
			internal double ColumnWidth { get; set; }

			/// <summary>
			/// returns true if column size contains change, false otherwise
			/// </summary>
			/// <param name="columnSizeInfo"></param>
			/// <returns></returns>
			internal bool ChangedRegardingTo(ColumnSizeInfo columnSizeInfo)
			{
				return Math.Abs(TabOffset - columnSizeInfo.TabOffset) > 1.0E-10 || Math.Abs(ColumnWidth - columnSizeInfo.ColumnWidth) > 1.0E-10;
			}
		}

		#endregion

		private List<ElasticTabstopsLine> _elasticTabstopsLinesCache;
		private readonly IWpfTextView _textView;

		private double _minCellWidth;
		private double _paddingWidth;
		private readonly TextMeasureService _textMeasureService;
		/// <summary>
		/// Initialize new instance of ElasticTabstopsSizeManager for IWpfTextView 
		/// </summary>
		private ElasticTabstopsSizeManager(IWpfTextView textView)
		{
			_textView = textView;
			_textMeasureService = TextMeasureService.Create(_textView);

			InvalidateTabstops();
		}

		/// <summary>
		/// Get tab offsets for line
		/// </summary>
		internal double[] GetTabOffsets(ITextSnapshotLine line)
		{
			//VS is trying to Format textbuffer with old version,
			//not actual for AlwaysAligned
			if (line.LineNumber >= _elasticTabstopsLinesCache.Count)
			{
				return new double[0];
			}
			//returns tab offsets from cache
			var tabOffsets = _elasticTabstopsLinesCache[line.LineNumber].ElasticColumns.TakeWhile(etc => etc.TabOffset != null).Select(etc => etc.TabOffset.TabOffset).ToArray();
			return tabOffsets;
		}

		/// <summary>
		/// Invalidates tabs cache depending given changes, and return changed line numbers
		/// </summary>
		internal void InvalidateChanges()
		{
			InvalidateTabstops();
		}

		/// <summary>
		/// Invalidates tabs cache depending given changes, and return changed line numbers
		/// </summary>
		/// <param name="changes">changed made</param>
		internal void InvalidateChanges(INormalizedTextChangeCollection changes)
		{
			if (!changes.Any()) return;

			#region Old

			var firstChange = changes.First();

			var start = Math.Min(firstChange.OldSpan.Start, firstChange.NewSpan.Start);
			var end = Math.Max(firstChange.OldSpan.End, firstChange.NewSpan.End);

			foreach (var change in changes)
			{
				var lineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(change.NewPosition);

				if (change.LineCountDelta > 0)
				{
					_elasticTabstopsLinesCache.InsertRange(lineNumber,
														   Enumerable.Range(0, change.LineCountDelta).Select(
															   c => new ElasticTabstopsLine()));
				}
				else if (change.LineCountDelta < 0)
				{
					_elasticTabstopsLinesCache.RemoveRange(lineNumber, -change.LineCountDelta);
				}

				start = Math.Min(start, Math.Min(change.OldSpan.Start, change.NewSpan.Start));
				end = Math.Max(end, Math.Max(change.OldSpan.End, change.NewSpan.End));
			}

			var topLine = _textView.TextSnapshot.GetLineFromPosition(start);
			var topLineNumber = topLine.LineNumber;

			if (changes.IncludesLineChanges && topLineNumber != 0)
			{
				topLineNumber--;
				topLine = _textView.TextSnapshot.GetLineFromLineNumber(topLineNumber);
			}

			while (topLineNumber > 0
				&& topLine.Start != topLine.End)
			{
				topLineNumber--;
				topLine = _textView.TextSnapshot.GetLineFromLineNumber(topLineNumber);
			}

			end = Math.Min(end, Math.Max(0, _textView.TextSnapshot.Length - 1));

			var bottomLine = _textView.TextSnapshot.GetLineFromPosition(end);
			var bottomLineNumber = bottomLine.LineNumber;

			if (changes.IncludesLineChanges && bottomLineNumber < _textView.TextSnapshot.LineCount - 1)
			{
				bottomLineNumber++;
				bottomLine = _textView.TextSnapshot.GetLineFromLineNumber(bottomLineNumber);
			}


			while (bottomLineNumber < _textView.TextSnapshot.LineCount - 1
				&& bottomLine.Start != bottomLine.End)
			{
				bottomLineNumber++;
				bottomLine = _textView.TextSnapshot.GetLineFromLineNumber(bottomLineNumber);
			}

			#endregion

			//InvalidateChanges();
			for (var i = topLineNumber; i <= bottomLineNumber; i++)
			{
				_elasticTabstopsLinesCache[i] = new ElasticTabstopsLine();
			}

			for (var i = topLineNumber; i <= bottomLineNumber; i++)
			{
				var line = _textView.TextSnapshot.GetLineFromLineNumber(i);
				CalculateTabOffsets(line, CalculateDirection.Down, false);
			}

		}

		/// <summary>
		/// Invalidates tabstops cache for textBuffer
		/// </summary>
		private void InvalidateTabstops()
		{
			_minCellWidth = AlwaysAlignedConfigurationService.Instance.GetConfiguration().MinimumCellWidth;
			_paddingWidth = AlwaysAlignedConfigurationService.Instance.GetConfiguration().CellPadding;

			ITextSnapshot textSnapshot = _textView.TextSnapshot;
			//Create empty ElasticTabstopsLine for each line
			_elasticTabstopsLinesCache = Enumerable.Range(0, textSnapshot.LineCount).Select(i => new ElasticTabstopsLine()).ToList();

			//Build _elasticTabstopsLinesCache by calculating from top to down
			foreach (var line in textSnapshot.Lines)
			{
				CalculateTabOffsets(line, CalculateDirection.Down, false);
			}
		}

		/// <summary>
		/// Calculate tab offsets for line in a given direction
		/// </summary>
		private void CalculateTabOffsets(ITextSnapshotLine line, CalculateDirection direction, bool forceInvalidate)
		{
			//Calculates tab offset for a given line for the given direction
			ElasticTabstopsLine elasticLine = GetElasticTabstopsLine(line, forceInvalidate);

			for (int colNumber = 0; colNumber < elasticLine.ElasticColumns.Length; colNumber++)
			{
				ElasticTabstopsColumn column = elasticLine.ElasticColumns[colNumber];

				//Tab offset is allready calculated during other line calculation
				if (!forceInvalidate && column.TabOffset != null)
					continue;

				//Assign the same ColumnTabOffset to all columns in the same block
				ColumnSizeInfo colTabOffset = new ColumnSizeInfo
												{
													TabOffset = CalculateInitialTabOffset(elasticLine, colNumber),
													ColumnWidth = CalculateInitialWidth(elasticLine, colNumber)
												};

				column.TabOffset = colTabOffset;

				switch (direction)
				{
					case CalculateDirection.Up:
						CalculateTabOffsetUp(line, colNumber, colTabOffset);
						break;
					case CalculateDirection.Down:
						CalculateTabOffsetDown(line, colNumber, colTabOffset);
						break;
					case CalculateDirection.DownUp:
						CalculateTabOffsetDown(line, colNumber, colTabOffset);
						CalculateTabOffsetUp(line, colNumber, colTabOffset);
						break;
					default:
						throw new ArgumentException("direction");
				}
			}
		}

		/// <summary>
		/// Calculate tab offsets by going down
		/// </summary>
		private void CalculateTabOffsetDown(ITextSnapshotLine curLine, int colNumber, ColumnSizeInfo colTabOffset)
		{
			int curLineNumber = curLine.LineNumber + 1;

			ITextSnapshot textSnapshot = _textView.TextSnapshot;

			ElasticTabstopsLine elasticLine = _elasticTabstopsLinesCache[curLine.LineNumber];
			bool isLastColumnInLine = elasticLine.IsLastColumnInLine(colNumber);

			while ( curLineNumber < textSnapshot.LineCount)
			{
				ITextSnapshotLine downLine = textSnapshot.GetLineFromLineNumber(curLineNumber);
				ElasticTabstopsLine downElasticLine = GetElasticTabstopsLine(downLine);

				if (downElasticLine.IsLastColumnInLine(colNumber) != isLastColumnInLine)
				{
					break;
				}

				ElasticTabstopsColumn downColumn = downElasticLine.GetColumnOrDefault(colNumber);

				if (downColumn == null)
				{
					break;
				}

				downColumn.TabOffset = colTabOffset;
				ShrinkTabOffset(downElasticLine, colNumber);
				curLineNumber++;
			}
		}

		/// <summary>
		/// Calculate tab offsets by going up
		/// </summary>
		private void CalculateTabOffsetUp(ITextSnapshotLine curLine, int colNumber, ColumnSizeInfo colTabOffset)
		{
			int curLineNumber = curLine.LineNumber - 1;

			ITextSnapshot textSnapshot = _textView.TextSnapshot;

			ElasticTabstopsLine elasticLine = _elasticTabstopsLinesCache[curLine.LineNumber];
			bool isLastColumnInLine = elasticLine.IsLastColumnInLine(colNumber);

			while (curLineNumber >= 0)
			{
				ITextSnapshotLine upLine = textSnapshot.GetLineFromLineNumber(curLineNumber);
				ElasticTabstopsLine upElasticLine = GetElasticTabstopsLine(upLine);

				if (upElasticLine.IsLastColumnInLine(colNumber) != isLastColumnInLine)
				{
					break;
				}

				ElasticTabstopsColumn upColumn = upElasticLine.GetColumnOrDefault(colNumber);

				if (upColumn == null)
				{
					break;
				}

				upColumn.TabOffset = colTabOffset;
				ShrinkTabOffset(upElasticLine, colNumber);
				curLineNumber--;
			}
		}

		/// <summary>
		/// Fix tab offset for a column if needed
		/// </summary>
		private void ShrinkTabOffset(ElasticTabstopsLine tabLine, int colNumber)
		{
			ElasticTabstopsColumn colTabOffset = tabLine.ElasticColumns[colNumber];

			double width = CalculateInitialWidth(tabLine, colNumber);

			if (colTabOffset.TabOffset.ColumnWidth < width)
			{
				colTabOffset.TabOffset.ColumnWidth = width;
				colTabOffset.TabOffset.TabOffset = CalculateInitialTabOffset(tabLine, colNumber);
			}
		}

		/// <summary>
		/// Calculates column width for a specific column in specific line
		/// </summary>
		private double CalculateInitialWidth(ElasticTabstopsLine elasticLine, int colNumber)
		{
			ITextSnapshot textSnapshot = _textView.TextSnapshot;
			ElasticTabstopsColumn column = elasticLine.ElasticColumns[colNumber];
			Span span = new Span(column.Start, column.ColumnTextLength);
			if (span.Start > textSnapshot.Length || span.End > textSnapshot.Length)
			{
				return 0;
			}

			SnapshotSpan columnSpan = new SnapshotSpan(textSnapshot, span);

			double columnWidth = _textMeasureService.GetWidth(columnSpan);

			return Math.Max(columnWidth, _minCellWidth);
		}

		/// <summary>
		/// Calulates tab offset depending column widthes before current column
		/// This method assume that column widthes before current column are calculated allready
		/// </summary>
		private double CalculateInitialTabOffset(ElasticTabstopsLine tabLine, int colNumber)
		{
			return tabLine.ElasticColumns.Take(colNumber).Sum(ct => ct.TabOffset.ColumnWidth) + colNumber * _paddingWidth;
		}

		/// <summary>
		/// Returns ElasticTabstopsLine with initialized ElasticColumns
		/// </summary>
		private ElasticTabstopsLine GetElasticTabstopsLine(ITextSnapshotLine line, bool forceInvalidateColumns = false)
		{
			ElasticTabstopsLine elasticTabstopsLine = _elasticTabstopsLinesCache[line.LineNumber];
			if (elasticTabstopsLine.ElasticColumns == null || forceInvalidateColumns)
			{
				string lineText = line.GetText();
				string[] tabSplits = lineText.Split('\t');
				elasticTabstopsLine.ElasticColumns = new ElasticTabstopsColumn[tabSplits.Length];
				int curPosInLine = line.Start.Position;
				for (int i = 0; i < tabSplits.Length; i++)
				{
					string ts = tabSplits[i];
					ElasticTabstopsColumn column = new ElasticTabstopsColumn {ColumnTextLength = ts.Length, Start = curPosInLine};
					//skeep tab
					curPosInLine += ts.Length + 1;
					elasticTabstopsLine.ElasticColumns[i] = column;
				}
			}
			return elasticTabstopsLine;
		}

		internal static ElasticTabstopsSizeManager Get(IWpfTextView textView)
		{
			if (textView == null)
				return null;
			ElasticTabstopsSizeManager outManager;
			textView.Properties.TryGetProperty(typeof(ElasticTabstopsSizeManager), out outManager);
			return outManager;
		}

		internal static ElasticTabstopsSizeManager Create(IWpfTextView textView)
		{
			if (textView == null)
				return null;
			return textView.Properties.GetOrCreateSingletonProperty(() => new ElasticTabstopsSizeManager(textView));
		}
	}
}
