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
		internal readonly string FileName;
		
		internal Crack(int primaryRail, int secondaryRail, int type, string fileName)
		{
			PrimaryRail = primaryRail;
			SecondaryRail = secondaryRail;
			Type = type;
			FileName = fileName;
		}

		internal void Create(int CurrentRail, Transformation RailTransformation, Vector3 pos, Block CurrentBlock, Block NextBlock, StructureData Structure, double StartingDistance, double EndingDistance)
		{
			if (PrimaryRail != CurrentRail)
			{
				return;
			}
			CultureInfo Culture = CultureInfo.InvariantCulture;
			double px0 = PrimaryRail > 0 ? CurrentBlock.Rails[PrimaryRail].RailStart.X : 0.0;
			double px1 = PrimaryRail > 0 ? NextBlock.Rails[PrimaryRail].RailEnd.X : 0.0;
			if (SecondaryRail < 0 || !CurrentBlock.Rails.ContainsKey(SecondaryRail) || !CurrentBlock.Rails[SecondaryRail].RailStarted)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is out of range in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
			}
			else
			{
				double sx0 = CurrentBlock.Rails[SecondaryRail].RailStart.X;
				double sx1 = NextBlock.Rails[SecondaryRail].RailEnd.X;
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
						Plugin.CurrentHost.CreateStaticObject(Crack, pos, RailTransformation, Transformation.NullTransformation, 0.0, StartingDistance, EndingDistance, StartingDistance);
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
						Plugin.CurrentHost.CreateStaticObject(Crack, pos, RailTransformation, Transformation.NullTransformation, 0.0, StartingDistance, EndingDistance, StartingDistance);
					}
				}
			}
		}
	}
}
