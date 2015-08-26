using System;
using System.IO;

namespace SingaporeSkiing
{
	internal class MapReader
	{
		public static bool TryParseFile(string filename, out MapData mapData)
		{
			try
			{
				using (var fileStream = File.OpenText(filename))
				{
					return MapReader.TryParse(fileStream, out mapData);
				}
			}
			catch (IOException e)
			{
				Console.WriteLine($"Failed to open map file '{filename}': '{e.Message}");
			}

			mapData = null;
			return false;
		}

		public static bool TryParse(StreamReader streamReader, out MapData mapData)
		{
			try
			{
				mapData = Parse(streamReader);
				return true;
			}
			catch (IOException e)
			{
				Console.WriteLine($"Error occurred reading file: '{e.Message}'");
			}
			catch (ParseException e)
			{
				Console.WriteLine($"Parse error: '{e.Message}'");
			}

			mapData = null;
			return false;
		}

		public static MapData Parse(StreamReader streamReader)
		{
			long lineNumber = 1;
			var sizeLine = streamReader.ReadLine();

			if (String.IsNullOrWhiteSpace(sizeLine))
			{
				throw new ParseException($"Line {lineNumber} is empty");
			}

			var sizeParts = sizeLine.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			if (sizeParts.Length != 2)
			{
				throw new ParseException("Unable to parse width and height of map");
			}

			long gridWidth;
			long gridHeight;

			if (!Int64.TryParse(sizeParts[0], out gridWidth))
			{
				throw new ParseException($"Unable to parse grid width ({sizeParts[0]})");
			}

			if (!Int64.TryParse(sizeParts[1], out gridHeight))
			{
				throw new ParseException($"Unable to parse grid height ({sizeParts[1]})");
			}

			long expectedLineNumber = gridHeight + 1;

			long[] altitudeBuffer = new long[gridWidth*gridHeight];

			while (!streamReader.EndOfStream && lineNumber < expectedLineNumber)
			{
				lineNumber++;

				var line = streamReader.ReadLine();

				if (String.IsNullOrWhiteSpace(line))
				{
					throw new ParseException($"Line {lineNumber} is empty");
				}

				var lineParts = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

				if (lineParts.Length != gridWidth)
				{
					throw new ParseException($"Line {lineNumber} does not have {gridWidth} altitude values");
				}

				// Subtract two because grid data starts on line 2
				long gridY = lineNumber - 2;

				for (int gridX = 0; gridX < lineParts.Length; gridX++)
				{
					var linePart = lineParts[gridX];

					long altitude;

					if (!Int64.TryParse(linePart, out altitude))
					{
						throw new ParseException($"Unable to parse value {gridX} on line {lineNumber} ({linePart})");
					}

					altitudeBuffer[(gridY*gridWidth) + gridX] = altitude;
				}
			}

			if (lineNumber != expectedLineNumber)
			{
				throw new ParseException("Failed to read expected number of lines");
			}

			if (!streamReader.EndOfStream)
			{
				throw new ParseException("Unexpected lines at end of file");
			}

			return new MapData(gridWidth, gridHeight, altitudeBuffer);
		}

		private class ParseException : Exception
		{
			public ParseException(string message) : base(message)
			{
			}
		}
	}
}