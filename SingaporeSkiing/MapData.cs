namespace SingaporeSkiing
{
	class MapData
	{
		public long Width { get; private set; }
		public long Height { get; private set; }
		public long[] Altitudes { get; private set; }

		public MapData(long width, long height, long[] altitudes)
		{
			Width = width;
			Height = height;
			Altitudes = altitudes;
		}
	}
}
