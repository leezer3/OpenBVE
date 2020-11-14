using System.Collections.Generic;

namespace MechanikRouteParser
{
	internal class RouteData
	{
		internal List<Block> Blocks = new List<Block>();

		internal int FindBlock(double TrackPosition)
		{
			for (int i = 0; i < this.Blocks.Count; i++)
			{
				if (this.Blocks[i].StartingTrackPosition == TrackPosition)
				{
					return i;
				}
			}
			this.Blocks.Add(new Block(TrackPosition));
			return this.Blocks.Count - 1;
		}

		internal void CreateMissingBlocks()
		{
			for (int i = 0; i < this.Blocks.Count -1; i++)
			{
				while (true)
				{
					if (this.Blocks[i + 1].StartingTrackPosition - this.Blocks[i].StartingTrackPosition > 25)
					{
						this.Blocks.Insert(i + 1, new Block(this.Blocks[i].StartingTrackPosition + 25));
						i++;
					}
					else
					{
						break;
					}
				}
			}
		}
	}
}
