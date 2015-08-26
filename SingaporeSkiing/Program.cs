using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

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
			MapData mapData = null;

			try
			{
				using (var fileStream = File.OpenText(filename))
				{
					didParse = TryParseFile(fileStream, out mapData);
				}
			}
			catch (IOException e)
			{
				Console.WriteLine($"Failed to open map file '{filename}': '{e.Message}");
			}

			if (!didParse || mapData == null)
			{
				Console.WriteLine("Failed to parse file");
				return 2;
			}

			long highestAltitude = -1;
			long highestAltitudeCount = 0;

			foreach (var altitude in mapData.Altitudes)
			{
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

			if (highestAltitude != -1 && highestAltitudeCount > 0)
			{
				Console.WriteLine($"Found a total of {highestAltitudeCount} peaks at altitude {highestAltitude}");
			}
			else
			{
				Console.WriteLine("Failed to find highest altitude");
			}

			ExportImage(mapData, highestAltitude, filename);

			return 0;
		}

		private static bool TryParseFile(StreamReader streamReader, out MapData mapData)
		{
			mapData = null;
			try
			{
				mapData = ParseFile(streamReader);
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

		private static MapData ParseFile(StreamReader streamReader)
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

		private static void ExportImage(MapData mapData, long highestAltitude, string filename)
		{
			byte[] pixelBuffer = new byte[mapData.Width*mapData.Height*3];
			for (long i = 0; i < pixelBuffer.Length; i += 3)
			{
				long altitudeIdx = i/3;

				double normalisedAltitude = Math.Min(Math.Max(mapData.Altitudes[altitudeIdx]/(double) highestAltitude, 0f), 1f);
				byte pixelValue = (byte) (normalisedAltitude*0xFF);

				pixelBuffer[i + 0] = pixelValue;
				pixelBuffer[i + 1] = pixelValue;
				pixelBuffer[i + 2] = pixelValue;
			}

			GCHandle pixelBufferHandle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);

			using (
				Image image = new Bitmap((int) mapData.Width, (int) mapData.Height, (int) (mapData.Width*3),
					PixelFormat.Format24bppRgb, pixelBufferHandle.AddrOfPinnedObject()))
			{
				var imageName = filename + ".png";
				image.Save(imageName);
			}

			pixelBufferHandle.Free();
		}
	}
}