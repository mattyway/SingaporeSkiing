using System;
using System.IO;

namespace SingaporeSkiing
{
	internal class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("No map file specified");
				return 1;
			}

			var filename = args[0];

			bool didParse = false;

			try
			{
				using (var fileStream = File.OpenText(filename))
				{
					didParse = TryParseFile(fileStream);
				}
			}
			catch (IOException e)
			{
				Console.WriteLine($"Failed to open map file '{filename}': '{e.Message}");
			}

			if (!didParse)
			{
				Console.WriteLine("Failed to parse file");
			}

			return 0;
		}

		private static bool TryParseFile(StreamReader streamReader)
		{
			try
			{
				ParseFile(streamReader);
			}
			catch (IOException e)
			{
				Console.WriteLine($"Error occurred reading file: '{e.Message}'");
				return false;
			}
			catch (ParseException e)
			{
				Console.WriteLine($"Parse error: '{e.Message}'");
				return false;
			}

			return true;
		}

		private static void ParseFile(StreamReader streamReader)
		{
			long lineNumber = 1;
			var sizeLine = streamReader.ReadLine();

			if (string.IsNullOrWhiteSpace(sizeLine))
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

			if (!long.TryParse(sizeParts[0], out gridWidth))
			{
				throw new ParseException($"Unable to parse grid width ({sizeParts[0]})");
			}

			if (!long.TryParse(sizeParts[1], out gridHeight))
			{
				throw new ParseException($"Unable to parse grid height ({sizeParts[1]})");
			}

			long expectedLineNumber = gridHeight + 1;

			long[] altitudeBuffer = new long[gridWidth*gridHeight];

			long highestAltitude = -1;
			long highestAltitudeCount = 0;

			while (!streamReader.EndOfStream && lineNumber < expectedLineNumber)
			{
				lineNumber++;

				var line = streamReader.ReadLine();

				if (string.IsNullOrWhiteSpace(line))
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

					if (!long.TryParse(linePart, out altitude))
					{
						throw new ParseException($"Unable to parse value {gridX} on line {lineNumber} ({linePart})");
					}

					altitudeBuffer[(gridY*gridWidth) + gridX] = altitude;

					if (altitude > highestAltitude)
					{
						highestAltitude = altitude;
						highestAltitudeCount = 1;
					}
					else if (altitude == highestAltitude)
					{
						highestAltitudeCount++;
					}
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

			if (highestAltitude != -1 && highestAltitudeCount > 0)
			{
				Console.WriteLine($"Found a total of {highestAltitudeCount} peaks at altitude {highestAltitude}");
			}
			else
			{
				Console.WriteLine("Failed to find highest altitude");
			}
		}

		private class ParseException : Exception
		{
			public ParseException(string message) : base(message)
			{
			}
		}
	}
}