using System;
using System.Collections.Generic;
using System.Linq;

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
		}

		public void BuildNodes()
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

		public void BuildLinks()
		{
			for (long y = 0; y < _mapData.Height; y++)
			{
				for (long x = 0; x < _mapData.Width; x++)
				{
					MapNode node = GetNode(x, y);

					if (x > 0)
					{
						var westNode = GetNode(x - 1, y);
						if (westNode.Altitude < node.Altitude)
						{
							node.NeighbourNodes.Add(westNode);
						}
					}

					if (x < _mapData.Width - 1)
					{
						var eastNode = GetNode(x + 1, y);
						if (eastNode.Altitude < node.Altitude)
						{
							node.NeighbourNodes.Add(eastNode);
						}
					}

					if (y > 0)
					{
						var northNode = GetNode(x, y - 1);
						if (northNode.Altitude < node.Altitude)
						{
							node.NeighbourNodes.Add(northNode);
						}
					}

					if (y < _mapData.Height - 1)
					{
						var southNode = GetNode(x, y + 1);
						if (southNode.Altitude < node.Altitude)
						{
							node.NeighbourNodes.Add(southNode);
						}
					}
				}
			}
		}

		public void BuildPaths()
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

		public Path FindPath()
		{
			MapNode bestNode = null;

			for (long y = 0; y < _mapData.Height; y++)
			{
				for (long x = 0; x < _mapData.Width; x++)
				{
					MapNode node = GetNode(x, y);

					if (bestNode == null || node.Path > bestNode.Path)
					{
						bestNode = node;
					}
				}
			}

			return bestNode?.Path;
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

		public List<MapNode> NeighbourNodes { get; }

		public MapNode(long x, long y, long altitude)
		{
			X = x;
			Y = y;
			Altitude = altitude;
			NeighbourNodes = new List<MapNode>();
		}

		public void BuildPath()
		{
			Path bestPath = null;
			foreach (var neighbourNode in NeighbourNodes)
			{
				if (!neighbourNode.PathBuilt)
				{
					neighbourNode.BuildPath();
				}

				var testPath = new Path(this, neighbourNode);
				if (bestPath == null || testPath > bestPath)
				{
					bestPath = testPath;
				}
			}

			if (bestPath == null)
			{
				Path = new Path(this);
			}
			else
			{
				Path = bestPath;
			}
		}
	}

	internal class Path : IComparable<Path>
	{
		public long Steps
		{
			get { return _nodes.Count; }
		}

		public long Descent { get; }

		public IReadOnlyList<MapNode> Nodes
		{
			get { return _nodes; }
		}

		private readonly List<MapNode> _nodes;

		public Path(MapNode mapNode)
		{
			_nodes = new List<MapNode>();
			_nodes.Add(mapNode);
			Descent = 0;
		}

		public Path(MapNode fromNode, MapNode toNode) : this(fromNode)
		{
			_nodes.AddRange(toNode.Path.Nodes);
			Descent = _nodes.First().Altitude - _nodes.Last().Altitude;
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
}