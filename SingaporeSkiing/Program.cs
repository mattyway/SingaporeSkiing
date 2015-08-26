using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

			didParse = MapReader.TryParseFile(filename, out mapData);

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

		private static void ExportImage(MapData mapData, long highestAltitude, string filename)
		{
			byte[] pixelBuffer = new byte[mapData.Width*mapData.Height*3];
			for (long i = 0; i < pixelBuffer.Length; i += 3)
			{
				long altitudeIdx = i/3;

				double normalisedAltitude = Math.Min(Math.Max(mapData.Altitudes[altitudeIdx]/(double) highestAltitude, 0f), 1f);

				var pixel = CalculateColour(normalisedAltitude);

				pixelBuffer[i + 0] = pixel.B;
				pixelBuffer[i + 1] = pixel.G;
				pixelBuffer[i + 2] = pixel.R;
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

		static Color CalculateColour (double offset)
		{
			var colours = new[] {
				new Tuple<double, Color>(0, Color.FromArgb(255, 0, 0, 255)),
				new Tuple<double, Color>(0.5f, Color.FromArgb(255, 0, 255, 0)),
				new Tuple<double, Color>(1, Color.FromArgb(255, 255, 0, 0)),
			};

			Tuple<double, Color> before = colours.OrderBy(c => c.Item1).First();
			Tuple<double, Color> after = colours.OrderByDescending(c => c.Item1).First();

			foreach (var gradientStop in colours) {
				if (gradientStop.Item1 < offset && gradientStop.Item1 > before.Item1) {
					before = gradientStop;
				}

				if (gradientStop.Item1 > offset && gradientStop.Item1 < after.Item1) {
					after = gradientStop;
				}
			}

			return Color.FromArgb(
				(byte) (((offset - before.Item1)*(after.Item2.A - before.Item2.A)/(after.Item1 - before.Item1) + before.Item2.A)),
				(byte) (((offset - before.Item1)*(after.Item2.R - before.Item2.R)/(after.Item1 - before.Item1) + before.Item2.R)),
				(byte) (((offset - before.Item1)*(after.Item2.G - before.Item2.G)/(after.Item1 - before.Item1) + before.Item2.G)),
				(byte) (((offset - before.Item1)*(after.Item2.B - before.Item2.B)/(after.Item1 - before.Item1) + before.Item2.B)));
		}
	}
}