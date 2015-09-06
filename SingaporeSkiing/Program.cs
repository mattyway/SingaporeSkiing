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

			Console.WriteLine("Reading file");

			didParse = MapReader.TryParseFile(filename, out mapData);

			if (!didParse || mapData == null)
			{
				Console.WriteLine("Failed to parse file");
				return 2;
			}

			Console.WriteLine("Creating Map");
			Map map = new Map(mapData);

			Console.WriteLine("Building nodes");
			map.BuildNodes();

			Console.WriteLine("Building links");
			map.BuildLinks();

			Console.WriteLine("Building paths");
			map.BuildPaths();

			Console.WriteLine("Finding best path");
			Console.WriteLine();
			var bestPath = map.FindPath();

			if (bestPath != null)
			{
				Console.WriteLine($"Best path has {bestPath.Steps} steps with a descent of {bestPath.Descent}");

				Console.WriteLine($"Starts at {bestPath.Nodes.First().X},{bestPath.Nodes.First().Y} and follows these directions:");

				string[] directions = new string[bestPath.Nodes.Count - 1];
				for (int idx = 1; idx < bestPath.Nodes.Count; idx++)
				{
					var toNode = bestPath.Nodes[idx];
					var fromNode = bestPath.Nodes[idx - 1];

					string direction = null;
					if (toNode.Y > fromNode.Y)
					{
						direction = "North";
					}
					else if (toNode.Y < fromNode.Y)
					{
						direction = "South";
					}
					else if (toNode.X > fromNode.X)
					{
						direction = "East";
					}
					else if (toNode.X < fromNode.X)
					{
						direction = "West";
					}

					directions[idx - 1] = direction;
				}

				Console.WriteLine($"{string.Join("\n", directions)}");
			}
			else
			{
				Console.WriteLine("Failed to find best path");
			}

			return 0;
		}
	}
}