using System;
using OpenBveApi.Runtime;
using OpenBve.RouteManager;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Updates all signal sections</summary>
		internal static void UpdateAllSections()
		{
			if (CurrentRoute.Sections.Length != 0)
			{
				CurrentRoute.Sections[CurrentRoute.Sections.Length - 1].Update(CurrentRoute.SecondsSinceMidnight);
			}
		}

		/// <summary>Updates the plugin to inform about sections.</summary>
		/// <param name="train">The train.</param>
		internal static void UpdatePluginSections(TrainManager.Train train)
		{
			if (train.Plugin != null)
			{
				SignalData[] data = new SignalData[16];
				int count = 0;
				int start = train.CurrentSectionIndex >= 0 ? train.CurrentSectionIndex : 0;
				for (int i = start; i < CurrentRoute.Sections.Length; i++)
				{
					SignalData signal = CurrentRoute.Sections[i].GetPluginSignal(train);
					if (data.Length == count)
					{
						Array.Resize<SignalData>(ref data, data.Length << 1);
					}
					data[count] = signal;
					count++;
					if (signal.Aspect == 0 | count == 16)
					{
						break;
					}
				}
				Array.Resize<SignalData>(ref data, count);
				train.Plugin.UpdateSignals(data);
			}
		}
	}
}
