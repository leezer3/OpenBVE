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

		internal AbstractHandle ParseHandle(Block block, TrainBase train, bool isPower)
		{
			HandleMinimum = (int)(block.ReadSingle() * 100);
			HandleMaximum = (int)(block.ReadSingle() * 100);
			HandleStep = (int)(block.ReadSingle() * 100);
			HandleStartingPosition = (int)(block.ReadSingle() * 100);
			// some handles seem to have extra numbers here, I believe ignored by the MSTS parser
			Block notchDescriptions = block.GetSubBlock(KujuTokenID.NumNotches);
			ParseNotchDescriptionBlock(notchDescriptions, isPower);
			return new VariableHandle(train, isPower, NotchDescriptions);
		}
		
		private void ParseNotchDescriptionBlock(Block block, bool isPower)
		{
			int numNotches = block.ReadInt32();
			if (numNotches < 2)
			{
				// percentage based notch
				NotchDescriptions = null;
				return;
			}

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
						notchDescription = isPower ? "P" + i : "B" + i;
					}

				}
				NotchDescriptions[i] = new Tuple<double, string>(notchPower, notchDescription);
			}

		}
	}
}
