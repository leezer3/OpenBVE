using System.Globalization;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace CsvRwRouteParser
{
	internal class Crack
	{
		internal readonly int PrimaryRail;
		internal readonly int SecondaryRail;
		internal readonly int Type;
		
		internal Crack(int primaryRail, int secondaryRail, int type)
		{
			PrimaryRail = primaryRail;
			SecondaryRail = secondaryRail;
			Type = type;
		}

		internal void Create(int CurrentRail, Transformation RailTransformation, Vector3 pos, Block CurrentBlock, Block NextBlock, StructureData Structure, double StartingDistance, double EndingDistance, string FileName)
		{
			if (PrimaryRail != CurrentRail)
			{
				return;
			}
			CultureInfo Culture = CultureInfo.InvariantCulture;
			int p = PrimaryRail;
			double px0 = p > 0 ? CurrentBlock.Rails[p].RailStart.X : 0.0;
			double px1 = p > 0 ? NextBlock.Rails[p].RailEnd.X : 0.0;
			int s = SecondaryRail;
			if (s < 0 || !CurrentBlock.Rails.ContainsKey(s) || !CurrentBlock.Rails[s].RailStarted)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is out of range in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
			}
			else
			{
				double sx0 = CurrentBlock.Rails[s].RailStart.X;
				double sx1 = NextBlock.Rails[s].RailEnd.X;
				double d0 = sx0 - px0;
				double d1 = sx1 - px1;
				if (d0 < 0.0)
				{
					if (!Structure.CrackL.ContainsKey(Type))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex references a CrackL not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						StaticObject Crack = (StaticObject) Structure.CrackL[Type].Transform(d0, d1);
						Plugin.CurrentHost.CreateStaticObject(Crack, pos, RailTransformation, Transformation.NullTransformation, 0, new WorldProperties(0, StartingDistance, StartingDistance, EndingDistance));
					}
				}
				else if (d0 > 0.0)
				{
					if (!Structure.CrackR.ContainsKey(Type))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "CrackStructureIndex references a CrackR not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						StaticObject Crack = (StaticObject) Structure.CrackR[Type].Transform(d0, d1);
						Plugin.CurrentHost.CreateStaticObject(Crack, pos, RailTransformation, Transformation.NullTransformation, 0, new WorldProperties(0, StartingDistance, StartingDistance, EndingDistance));
					}
				}
			}
		}
	}
}
