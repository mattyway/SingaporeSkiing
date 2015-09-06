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

			MapVisualiser.ExportImage(mapData, filename);

			Map map = new Map(mapData);
			map.BuildNodes();
			map.BuildLinks();
			map.BuildPaths();

			var bestPath = map.FindPath();

			if (bestPath != null)
			{
				Console.WriteLine($"Best path has {bestPath.Steps} steps with a descent of {bestPath.Descent}");
			}
			else
			{
				Console.WriteLine("Failed to find best path");
			}

			return 0;
		}
	}
}