using System;
using System.Collections.Generic;

namespace SingaporeSkiing
{
	internal class Map
	{
		private MapData _mapData;

		private MapNode[] _nodes;

		public Map(MapData mapData)
		{
			_mapData = mapData;

			_nodes = new MapNode[_mapData.Altitudes.Length];

			BuildNodes();

			BuildLinks();

			BuildPaths();

			FindPath();
		}

		private void BuildNodes()
		{
			for (long y = 0; y < _mapData.Height; y++)
			{
				for (long x = 0; x < _mapData.Width; x++)
				{
					var index = y*_mapData.Width + x;
					var node = new MapNode(x, y, _mapData.Altitudes[index]);
					_nodes[index] = node;
				}
			}
		}

		private void BuildLinks()
		{
			for (long y = 0; y < _mapData.Height; y++)
			{
				for (long x = 0; x < _mapData.Width; x++)
				{
					MapNode node = GetNode(x, y);

					MapNode[] neighbourNodes = new MapNode[4];

					if (x > 0)
					{
						var westNode = GetNode(x - 1, y);
						if (westNode.Altitude < node.Altitude)
						{
							neighbourNodes[0] = westNode;
						}
					}

					if (x < _mapData.Width - 1)
					{
						var eastNode = GetNode(x + 1, y);
						if (eastNode.Altitude < node.Altitude)
						{
							neighbourNodes[1] = eastNode;
						}
					}

					if (y > 0)
					{
						var northNode = GetNode(x, y - 1);
						if (northNode.Altitude < node.Altitude)
						{
							neighbourNodes[2] = northNode;
						}
					}

					if (y < _mapData.Height - 1)
					{
						var southNode = GetNode(x, y + 1);
						if (southNode.Altitude < node.Altitude)
						{
							neighbourNodes[3] = southNode;
						}
					}

					for (int i = 0; i < neighbourNodes.Length; i++)
					{
						var neighbourNode = neighbourNodes[i];
						if (neighbourNode == null)
						{
							continue;
						}

						var link = new Link()
						{
							Descent = node.Altitude - neighbourNode.Altitude,
							ToNode = neighbourNode
						};
						node.Links.Add(link);
					}
				}
			}
		}

		private void BuildPaths()
		{
			for (long y = 0; y < _mapData.Height; y++)
			{
				for (long x = 0; x < _mapData.Width; x++)
				{
					MapNode node = GetNode(x, y);

					if (!node.PathBuilt)
					{
						node.BuildPath();
					}
				}
			}
		}

		private void FindPath()
		{
			MapNode bestNode = null;

			for (long y = _mapData.Height - 1; y >= 0; y--)
			{
				for (long x = _mapData.Width - 1; x >= 0; x--)
				{
					MapNode node = GetNode(x, y);

					if (bestNode == null || node.Path > bestNode.Path)
					{
						bestNode = node;
					}
				}
			}

			if (bestNode != null)
			{
				Console.WriteLine($"Best path has {bestNode.Path.Steps} steps with a descent of {bestNode.Path.Descent}");
			}
			else
			{
				Console.WriteLine("Failed to find best path");
			}
		}

		private MapNode GetNode(long x, long y)
		{
			if (x < 0 || x >= _mapData.Width)
			{
				throw new ArgumentException("x");
			}

			if (y < 0 || y >= +_mapData.Height)
			{
				throw new ArgumentException("y");
			}

			return _nodes[y*_mapData.Width + x];
		}
	}

	internal class MapNode
	{
		public long X { get; }
		public long Y { get; }
		public long Altitude { get; }

		public bool PathBuilt
		{
			get { return Path != null; }
		}

		public Path Path { get; set; }

		public List<Link> Links { get; }

		public MapNode(long x, long y, long altitude)
		{
			X = x;
			Y = y;
			Altitude = altitude;
			Links = new List<Link>();
		}

		public void BuildPath()
		{
			Link bestLink = null;
			foreach (var link in Links)
			{
				if (!link.ToNode.PathBuilt)
				{
					link.ToNode.BuildPath();
				}

				if (bestLink == null || link.ToNode.Path > bestLink.ToNode.Path)
				{
					bestLink = link;
				}
			}

			if (bestLink == null)
			{
				Path = new Path(1, 0);
			}
			else
			{
				var steps = bestLink.ToNode.Path.Steps + 1;
				var descent = bestLink.Descent + bestLink.ToNode.Path.Descent;
				Path = new Path(steps, descent);
			}
		}
	}

	internal class Path : IComparable<Path>
	{
		public long Steps { get; }
		public long Descent { get; }

		public Path(long steps, long descent)
		{
			Steps = steps;
			Descent = descent;
		}

		public int CompareTo(Path other)
		{
			if (other == null)
				return 1;

			if (Steps == other.Steps)
			{
				if (Descent == other.Descent)
				{
					return 0;
				}

				if (Descent > other.Descent)
				{
					return 1;
				}

				return -1;
			}

			if (Steps > other.Steps)
			{
				return 1;
			}

			return -1;
		}

		public static bool operator <(Path p1, Path p2)
		{
			return p1.CompareTo(p2) < 0;
		}

		public static bool operator >(Path p1, Path p2)
		{
			return p1.CompareTo(p2) > 0;
		}
	}

	internal class Link
	{
		public long Descent { get; set; }
		public MapNode ToNode { get; set; }
	}
}