using System;
using OpenBve.Formats.MsTs;
using TrainManager.Handles;
using TrainManager.Trains;

namespace Train.MsTs
{
	internal partial class WagonParser
	{
		internal int HandleMinimum;

		internal int HandleMaximum;

		internal int HandleStep;

		internal int HandleStartingPosition;

		internal Tuple<double, string>[] NotchDescriptions;

		internal AbstractHandle ParseHandle(Block block, TrainBase train)
		{
			HandleMinimum = (int)(block.ReadSingle() * 100);
			HandleMaximum = (int)(block.ReadSingle() * 100);
			HandleStep = (int)(block.ReadSingle() * 100);
			HandleStartingPosition = (int)(block.ReadSingle() * 100);
			// some handles seem to have extra numbers here, I believe ignored by the MSTS parser
			Block notchDescriptions = block.GetSubBlock(KujuTokenID.NumNotches);
			ParseNotchDescriptionBlock(notchDescriptions);
			return new VariableHandle(train, NotchDescriptions);
		}
		
		private void ParseNotchDescriptionBlock(Block block)
		{
			int numNotches = block.ReadInt32();
			NotchDescriptions = new Tuple<double, string>[numNotches];
			for (int i = 0; i < numNotches; i++)
			{
				Block descriptionBlock = block.ReadSubBlock(KujuTokenID.Notch);
				double notchPower = descriptionBlock.ReadSingle();
				descriptionBlock.ReadSingle(); // ??
				string notchDescription = descriptionBlock.ReadString();
				if (notchDescription.Equals("dummy", StringComparison.InvariantCultureIgnoreCase))
				{
					if (i == 0)
					{
						notchDescription = "N";
					}
					else
					{
						notchDescription = "P" + i;
					}
						
				}
				NotchDescriptions[i] = new Tuple<double, string>(notchPower, notchDescription);
			}
		}
	}
}
