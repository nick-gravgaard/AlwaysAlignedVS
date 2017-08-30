using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// TODO: Rewrite this so it's based off my much nicer implementation at:
// https://github.com/nickgravgaard/ElasticNotepad/blob/master/src/main/scala/elasticTabstops.scala

namespace AlwaysAligned
{
	public class Cell
	{
		public int IndexFromBOF;
		public int Length;

		public Cell()
		{
			Length = 0;
		}

		public Cell(int indexFromBOF, int length)
		{
			IndexFromBOF = indexFromBOF;
			Length = length;
		}
	}

	public class Line
	{
		public SortedDictionary<int, Cell> Cells;
		public bool EndsInCR;

		public Line()
        {
			Cells = new SortedDictionary<int, Cell>();
			EndsInCR = false;
		}

		public Line(SortedDictionary<int, Cell> cells)
		{
			Cells = cells;
			EndsInCR = false;
		}
	}


	public class ElasticTabstopsConverter
	{
		public static bool CellExists(List<Line> lines, int lineNum, int cellNum)
		{
			return (lineNum < lines.Count) && (cellNum < lines[lineNum].Cells.Count);
		}
		public static bool CellExists(List<List<int>> list, int lineNum, int cellNum)
		{
			return (lineNum < list.Count) && (cellNum < list[lineNum].Count);
		}


		private static int CalcFixedCellSize(int textLen, int tabSize)
		{
			if (tabSize > 0)
			{
				return ((int)Math.Ceiling((textLen + 2.0) / tabSize)) * tabSize;
			}
			return tabSize;
		}


		public static List<Line> GetLines(string text, int tabSize)
		{
			var lines = new List<Line>();
			var line = new Line();
			lines.Add(line);
			var inText = false;
			var previousCharIsSpace = false;
			var textLength = 0;
			var pos = 0;
			var startPos = 0;
			var startCharNum = 0;
			for (var charNum = 0; charNum < text.Length; charNum++)
			{
				var currentChar = text[charNum];
				switch (currentChar)
				{
					case '\r':
					{
						line.EndsInCR = true;
						break;
					}
					case '\n':
					{
						if (inText)
						{
							line.Cells.Add(startPos, new Cell(startCharNum, textLength));
						}
						line = new Line();
						lines.Add(line);
						pos = 0;
						inText = false;
						previousCharIsSpace = false;
						break;
					}
					case '\t':
					{
						if (inText)
						{
							if (previousCharIsSpace)
							{
								line.Cells.Add(startPos, new Cell(startCharNum, textLength - 1));
							}
							else
							{
								line.Cells.Add(startPos, new Cell(startCharNum, textLength));
							}
							inText = false;
						}
						previousCharIsSpace = false;
						int expand = tabSize - (pos % tabSize);
						pos += expand;
						break;
					}
					case ' ':
					{
						if (previousCharIsSpace && inText)
						{
							line.Cells.Add(startPos, new Cell(startCharNum, textLength - 1));
							inText = false;
						}
						previousCharIsSpace = true;
						textLength++;
						pos++;
						break;
					}
					default:
					{
						if (!inText)
						{
							startPos = pos;
							startCharNum = charNum;
							textLength = 0;
						}
						inText = true;
						previousCharIsSpace = false;
						textLength++;
						pos++;
						break;
					}
				}
			}
			if (inText)
			{
				line.Cells.Add(startPos, new Cell(startCharNum, textLength));
			}
			return lines;
		}


		enum AtPosResults
		{
			PastEndOfLine,
			CellStart,
			CellMiddle,
			Space,
		};

		static AtPosResults CellExistsAtPos(int position, SortedDictionary<int, Cell> cells)
		{
			if (cells.Count == 0) return AtPosResults.PastEndOfLine;

			if (cells.ContainsKey(position)) return AtPosResults.CellStart;

			foreach (KeyValuePair<int, Cell> cell in cells)
			{
				if (cell.Key > position)
				{
					return AtPosResults.Space;
				}

				if (position >= cell.Key && position <= cell.Key + cell.Value.Length + 2)
				{
					return AtPosResults.CellMiddle;
				}
			}
			return AtPosResults.PastEndOfLine;
		}


		static void InsertEmptyCells(ref List<Line> lines, int pos, int firstBlockLineNum, int lastBlockLineNum)
		{
			for (var blockLineNum = firstBlockLineNum; blockLineNum <= lastBlockLineNum; blockLineNum++)
			{
				lines[blockLineNum].Cells.Add(pos, new Cell());
			}
		}


		public static String ToElasticTabstops(String text, int tabSize = 4)
		{
			var lines = GetLines(text, tabSize);
			var maxCells = lines.Aggregate(0, (current, line) => Math.Max(line.Cells.Count, current));

			for (var lineNum = 0; lineNum < lines.Count; lineNum++)
			{
				var line = lines[lineNum];
				foreach (KeyValuePair<int, Cell> cell in line.Cells)
				{
					int position = cell.Key;
					for (var i = lineNum + 1; i <= lines.Count - 1; i++)
					{
						var atPosResult = CellExistsAtPos(position, lines[i].Cells);
						if (atPosResult == AtPosResults.CellStart)
						{
							continue;
						}
						if (atPosResult == AtPosResults.Space)
						{
							lines[i].Cells.Add(position, new Cell());
							continue;
						}
						else
						{
							break;
						}
					}
				}
			}

			var maxPos = 0;
			foreach (var line in lines)
			{
				var cells = line.Cells;
				if (cells.Count > 0)
				{
					var lastCell = cells.Last();
					var lineEnd = lastCell.Key + lastCell.Value.Length;
					maxPos = Math.Max(maxPos, lineEnd);
				}
			}

			for (var pos = 0; pos < maxPos; pos += tabSize)
			{
				var startingNewBlock = true;
				var firstBlockLineNum = 0;
				var lastBlockLineNum = 0;
				var allSpaces = true;

				for (var lineNum = 0; lineNum < lines.Count; lineNum++)
				{
					var line = lines[lineNum];

					var atPosResult = CellExistsAtPos(pos, line.Cells);
					if (atPosResult == AtPosResults.Space || atPosResult == AtPosResults.CellMiddle)
					{
						if (atPosResult != AtPosResults.Space)
						{
							allSpaces = false;
						}
						if (startingNewBlock)
						{
							firstBlockLineNum = lineNum;
							startingNewBlock = false;
						}
						lastBlockLineNum = lineNum;
					}
					else
					{
						if (!startingNewBlock)
						{
							if (allSpaces)
							{
								InsertEmptyCells(ref lines, pos, firstBlockLineNum, lastBlockLineNum);
							}
							startingNewBlock = true;
							allSpaces = true;
						}
					}
				}
				if (!startingNewBlock && allSpaces)
				{
					InsertEmptyCells(ref lines, pos, firstBlockLineNum, lastBlockLineNum);
				}
			}

			var builder = new StringBuilder(text.Length);
			var lastLine = lines.Last();
			foreach (var line in lines)
			{
				if (line.Cells.Count > 0)
				{
					var lastCell = line.Cells.Last();
					foreach (var cell in line.Cells)
					{
						if (cell.Value.Length > 0)
						{
							builder.Append(text.Substring(cell.Value.IndexFromBOF, cell.Value.Length));
						}
						if (!cell.Equals(lastCell))
						{
							builder.Append('\t');
						}
					}
				}
				if (!line.Equals(lastLine))
				{
					if (line.EndsInCR)
					{
						builder.Append('\r');
					}
					builder.Append('\n');
				}
			}
			return builder.ToString();
		}


		public static String ToSpaces(String text, int tabSize = 4)
		{
			var textLines = text.Split('\n');
			IList<IList<String>> lines = textLines.Select(textLine => textLine.Split('\t')).Cast<IList<string>>().ToList();

			List<List<int>> sizes = new List<List<int>>();
			foreach (var line in lines)
			{
				List<int> sizesLine = new List<int>();
				foreach (var cell in line)
				{
					sizesLine.Add(CalcFixedCellSize(cell.Length, tabSize));
				}
				sizes.Add(sizesLine);
			}

			var maxCells = lines.Aggregate(0, (current, line) => Math.Max(current, line.Count));
			var nofLines = lines.Count;

			for (int cellNum = 0; cellNum < maxCells; cellNum++)
			{
				var startingNewBlock = true;
				int startRange = 0;
				int endRange = 0;
				int maxWidth = 0;

				for (int lineNum = 0; lineNum < nofLines; lineNum++)
				{
					if (CellExists(sizes, lineNum, cellNum) && CellExists(sizes, lineNum, cellNum + 1))
					{
						if (startingNewBlock)
						{
							startRange = lineNum;
							startingNewBlock = false;
						}
						maxWidth = Math.Max(maxWidth, sizes[lineNum][cellNum]);
						endRange = lineNum;
					}
					else
					{

						if (!startingNewBlock)
						{
							for (var blockcellNum = startRange; blockcellNum <= endRange; blockcellNum++)
							{
								sizes[blockcellNum][cellNum] = maxWidth;
							}
							startingNewBlock = true;
							maxWidth = 0;
						}
					}
				}

				if (!startingNewBlock)
				{
					for (int blockcellNum = startRange; blockcellNum <= endRange; blockcellNum++)
					{
						sizes[blockcellNum][cellNum] = maxWidth;
					}
				}
			}

			// build final string
			IList<string> newText = new List<string>();
			for (var lineNum = 0; lineNum < nofLines; lineNum++)
			{
				string newLine = "";
				for (var cellNum = 0; cellNum < lines[lineNum].Count; cellNum++)
				{
					newLine += lines[lineNum][cellNum];
					if (cellNum != lines[lineNum].Count - 1)
					{
						var nofSpaces = sizes[lineNum][cellNum] - lines[lineNum][cellNum].Length;
						newLine += new string(' ', nofSpaces);
					}
				}
				newText.Add(newLine);
			}
			return String.Join("\n", newText);
		}

	}
}
