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

					if (bestNode == null)
					{
						bestNode = node;
					}
					else if (node.Path.Steps == bestNode.Path.Steps && node.Path.Descent > bestNode.Path.Descent)
					{
						bestNode = node;
					}
					else if (node.Path.Steps > bestNode.Path.Steps)
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
		public long X { get; private set; }
		public long Y { get; private set; }
		public long Altitude { get; private set; }

		public bool PathBuilt
		{
			get { return Path != null; }
		}

		public Path Path { get; set; }

		public List<Link> Links { get; set; }

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

				if (bestLink == null)
				{
					bestLink = link;
				}
				else if (bestLink.ToNode.Path.Steps == link.ToNode.Path.Steps && link.ToNode.Path.Descent > bestLink.ToNode.Path.Descent)
				{
					bestLink = link;
				}
				else if (link.ToNode.Path.Steps > bestLink.ToNode.Path.Steps)
				{
					bestLink = link;
				}
			}

			if (bestLink == null)
			{
				Path = new Path()
				{
					Descent = 0,
					Steps = 1
				};
			}
			else
			{
				Path = new Path()
				{
					Descent = bestLink.Descent + bestLink.ToNode.Path.Descent,
					Steps = bestLink.ToNode.Path.Steps + 1
				};
			}
		}
	}

	internal class Path
	{
		public long Steps { get; set; }
		public long Descent { get; set; }
	}

	internal class Link
	{
		public long Descent { get; set; }
		public MapNode ToNode { get; set; }
	}
}