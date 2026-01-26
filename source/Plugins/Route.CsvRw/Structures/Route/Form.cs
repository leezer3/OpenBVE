using System.Globalization;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace CsvRwRouteParser
{
	internal class Form
	{
		internal Form(int primaryRail, int secondaryRail, int formType, int roofType, StructureData structure, string fileName)
		{
			PrimaryRail = primaryRail;
			SecondaryRail = secondaryRail;
			FormType = formType;
			RoofType = roofType;
			Structure = structure;
			FileName = fileName;
		}
		/// <summary>The platform face rail</summary>
		internal readonly int PrimaryRail;
		/// <summary>The rail for the rear transformation</summary>
		internal readonly int SecondaryRail;
		/// <summary>The index of the FormType to use</summary>
		internal readonly int FormType;
		/// <summary>The index of the RoofType to use</summary>
		internal readonly int RoofType;
		/// <summary>The file name of the expression</summary>
		internal readonly string FileName;
		/*
		 * Magic number constants used by BVE2 /4
		 */
		internal const int SecondaryRailStub = 0;
		internal const int SecondaryRailL = -1;
		internal const int SecondaryRailR = -2;

		internal readonly StructureData Structure;
		private readonly CultureInfo Culture = CultureInfo.InvariantCulture;
		internal void CreatePrimaryRail(Block currentBlock, Block nextBlock, Vector3 pos, Transformation railTransformation, double startingDistance, double endingDistance)
		{
			
			if (SecondaryRail == Form.SecondaryRailStub)
			{
				if (!Structure.FormL.ContainsKey(FormType))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
				}
				else
				{
					Structure.FormL[FormType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
					if (RoofType > 0)
					{
						if (!Structure.RoofL.ContainsKey(RoofType))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
						}
						else
						{
							Structure.RoofL[RoofType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
						}
					}
				}
			}
			else if (SecondaryRail == Form.SecondaryRailL)
			{
				if (!Structure.FormL.ContainsKey(FormType))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
				}
				else
				{
					Structure.FormL[FormType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
				}

				if (!Structure.FormCL.ContainsKey(FormType))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
				}
				else
				{
					Plugin.CurrentHost.CreateStaticObject((StaticObject) Structure.FormCL[FormType], pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
				}

				if (RoofType > 0)
				{
					if (!Structure.RoofL.ContainsKey(RoofType))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						Structure.RoofL[RoofType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
					}

					if (!Structure.RoofCL.ContainsKey(RoofType))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						Plugin.CurrentHost.CreateStaticObject((StaticObject) Structure.RoofCL[RoofType], pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
					}
				}
			}
			else if (SecondaryRail == Form.SecondaryRailR)
			{
				if (!Structure.FormR.ContainsKey(FormType))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
				}
				else
				{
					Structure.FormR[FormType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
				}

				if (!Structure.FormCR.ContainsKey(FormType))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
				}
				else
				{
					Plugin.CurrentHost.CreateStaticObject((StaticObject) Structure.FormCR[FormType], pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
				}

				if (RoofType > 0)
				{
					if (!Structure.RoofR.ContainsKey(RoofType))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						Structure.RoofR[RoofType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
					}

					if (!Structure.RoofCR.ContainsKey(RoofType))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						Plugin.CurrentHost.CreateStaticObject((StaticObject) Structure.RoofCR[RoofType], pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
					}
				}
			}
			else if (SecondaryRail > 0)
			{
				double px0 = PrimaryRail > 0 ? currentBlock.Rails[PrimaryRail].RailStart.X : 0.0;
				double px1 = PrimaryRail > 0 ? nextBlock.Rails[PrimaryRail].RailEnd.X : 0.0;
				if (SecondaryRail < 0 || !currentBlock.Rails.ContainsKey(SecondaryRail) || !currentBlock.Rails[SecondaryRail].RailStarted)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RailIndex2 is out of range in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName);
				}
				else
				{
					double d0 = currentBlock.Rails[SecondaryRail].RailStart.X - px0;
					double d1 = nextBlock.Rails[SecondaryRail].RailEnd.X - px1;
					if (d0 < 0.0)
					{
						if (!Structure.FormL.ContainsKey(FormType))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
						}
						else
						{
							Structure.FormL[FormType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
						}

						if (!Structure.FormCL.ContainsKey(FormType))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
						}
						else
						{
							StaticObject formC = (StaticObject) Structure.FormCL[FormType].Transform(d0, d1);
							Plugin.CurrentHost.CreateStaticObject(formC, pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
						}

						if (RoofType > 0)
						{
							if (!Structure.RoofL.ContainsKey(RoofType))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
							}
							else
							{
								Structure.RoofL[RoofType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
							}

							if (!Structure.RoofCL.ContainsKey(RoofType))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCL not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
							}
							else
							{
								StaticObject roofC = (StaticObject) Structure.RoofCL[RoofType].Transform(d0, d1);
								Plugin.CurrentHost.CreateStaticObject(roofC, pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
							}
						}
					}
					else if (d0 > 0.0)
					{
						if (!Structure.FormR.ContainsKey(FormType))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
						}
						else
						{
							Structure.FormR[FormType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
						}

						if (!Structure.FormCR.ContainsKey(FormType))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormCR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
						}
						else
						{
							StaticObject formC = (StaticObject) Structure.FormCR[FormType].Transform(d0, d1);
							Plugin.CurrentHost.CreateStaticObject(formC, pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
						}

						if (RoofType > 0)
						{
							if (!Structure.RoofR.ContainsKey(RoofType))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
							}
							else
							{
								Structure.RoofR[RoofType].CreateObject(pos, railTransformation, startingDistance, endingDistance, startingDistance);
							}

							if (!Structure.RoofCR.ContainsKey(RoofType))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofCR not loaded in Track.Form at track position " + startingDistance.ToString(Culture) + " in file " + FileName + ".");
							}
							else
							{
								StaticObject roofC = (StaticObject) Structure.RoofCR[RoofType].Transform(d0, d1);
								Plugin.CurrentHost.CreateStaticObject(roofC, pos, railTransformation, Transformation.NullTransformation, 0.0, startingDistance, endingDistance, startingDistance);
							}
						}
					}
				}
			}

		}

		internal void CreateSecondaryRail(Block currentBlock, Vector3 pos, Transformation RailTransformation, double StartingDistance, double EndingDistance)
		{
			double px = 0.0;
			if (currentBlock.Rails.ContainsKey(PrimaryRail))
			{
				px = PrimaryRail > 0 ? currentBlock.Rails[PrimaryRail].RailStart.X : 0.0;
			}
			double d = px - currentBlock.Rails[SecondaryRail].RailStart.X;
			if (d < 0.0)
			{
				if (!Structure.FormL.ContainsKey(FormType))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
				}
				else
				{
					Structure.FormL[FormType].CreateObject(pos, RailTransformation, StartingDistance, EndingDistance, StartingDistance);
				}

				if (RoofType > 0)
				{
					if (!Structure.RoofL.ContainsKey(RoofType))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						Structure.RoofL[RoofType].CreateObject(pos, RailTransformation, StartingDistance, EndingDistance, StartingDistance);
					}
				}
			}
			else
			{
				if (!Structure.FormR.ContainsKey(FormType))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "FormStructureIndex references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
				}
				else
				{
					Structure.FormR[FormType].CreateObject(pos, RailTransformation, StartingDistance, EndingDistance, StartingDistance);
				}

				if (RoofType > 0)
				{
					if (!Structure.RoofR.ContainsKey(RoofType))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "RoofStructureIndex references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
					}
					else
					{
						Structure.RoofR[RoofType].CreateObject(pos, RailTransformation, Transformation.NullTransformation, StartingDistance, EndingDistance, StartingDistance);
					}
				}
			}
		}
	}
}
