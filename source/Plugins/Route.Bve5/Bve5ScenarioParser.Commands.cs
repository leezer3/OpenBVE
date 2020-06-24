using System;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	partial class Parser
	{
		/// <summary>Parses a legacy command contained within a BVE5 scenario map</summary>
		/// <param name="command">The command to parse</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		/// <param name="PreviewOnly">Whether this is a preview only</param>
		internal void ParseLegacyCommand(string command, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength, bool PreviewOnly)
		{
			switch (command)
			{
				//Legacy commands
				case "curve":
				{
					double radius = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out radius))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "Radius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						radius = 0.0;
					}
					double cant = 0.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out cant))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "CantInMillimeters is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						cant = 0.0;
					}
					else
					{
						cant *= 0.001;
					}
					if (Data.SignedCant)
					{
						if (radius != 0.0)
						{
							cant *= (double) Math.Sign(radius);
						}
					}
					else
					{
						cant = Math.Abs(cant)*(double) Math.Sign(radius);
					}
					Data.Blocks[BlockIndex].CurrentTrackState.CurveRadius = radius;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCant = cant;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCantTangent = 0.0;
				}
					break;
				case "fog":
					if (!PreviewOnly)
					{
						double start = 0.0, end = 0.0;
						int r = 128, g = 128, b = 128;
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out start))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "StartingDistance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							start = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out end))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "EndingDistance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							end = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out r))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							r = 128;
						}
						else if (r < 0 | r > 255)
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							r = r < 0 ? 0 : 255;
						}
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out g))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							g = 128;
						}
						else if (g < 0 | g > 255)
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							g = g < 0 ? 0 : 255;
						}
						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[4], out b))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							b = 128;
						}
						else if (b < 0 | b > 255)
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							b = b < 0 ? 0 : 255;
						}
						if (start < end)
						{
							Data.Blocks[BlockIndex].Fog.Start = (float) start;
							Data.Blocks[BlockIndex].Fog.End = (float) end;
						}
						else
						{
							Data.Blocks[BlockIndex].Fog.Start = CurrentRoute.NoFogStart;
							Data.Blocks[BlockIndex].Fog.End = CurrentRoute.NoFogEnd;
						}
						Data.Blocks[BlockIndex].Fog.Color = new Color24((byte) r, (byte) g, (byte) b);
						Data.Blocks[BlockIndex].FogDefined = true;
					}
					break;
				case "pitch":
					double p = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out p))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "ValueInPermille is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						p = 0.0;
					}
					Data.Blocks[BlockIndex].Pitch = 0.001 * p;
					break;
			}
		}

		static void SetCurve(string command, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			switch (command)
			{
				case "begintransition":
					Data.Blocks[BlockIndex].CurrentTrackState.BeginInterpolation = true;
					break;
				case "begincircular":
					double radius = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 &&
					    !NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out radius))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "Radius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						radius = 0.0;
					}
					double cant = 0.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out cant))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "CantInMillimeters is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						cant = 0.0;
					}
					else
					{
						cant *= 0.001;
					}
					if (Data.SignedCant)
					{
						if (radius != 0.0)
						{
							cant *= (double) Math.Sign(radius);
						}
					}
					else
					{
						cant = Math.Abs(cant)*(double) Math.Sign(radius);
					}
					Data.Blocks[BlockIndex].CurrentTrackState.CurveRadius = radius;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCant = cant;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCantTangent = 0.0;
					break;
				case "end":
					Data.Blocks[BlockIndex].CurrentTrackState.CurveRadius = 0.0;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCant = 0.0;
					Data.Blocks[BlockIndex].CurrentTrackState.CurveCantTangent = 0.0;
					break;
			}
			
		}

		/// <summary>Changes the currently displayed background object</summary>
		/// <param name="key">The key (Must be change(??)</param>
		/// <param name="Arguments"></param>
		/// <param name="Data"></param>
		/// <param name="BlockIndex"></param>
		/// <param name="UnitOfLength"></param>
		static void SetBackground(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			if (key.ToLowerInvariant() != "change")
			{
				//Key must be change
				return;
			}
			if (Arguments.Length != 1)
			{
				//Should only be a single argument
				return;
			}
			Arguments[0] = Arguments[0].RemoveEnclosingQuotes();
			//Find the key in our object list (A background is just a generic object)
			int sttype = FindStructureIndex(Arguments[0], Data);
			int BackgroundIdx = -1;
			if (Data.Backgrounds == null)
			{
				Data.Backgrounds = new Background[0];
			}
			else
			{
				for (int i = 0; i > Data.Backgrounds.Length; i++)
				{
					if (Data.Backgrounds[i].Key == Arguments[0])
					{
						BackgroundIdx = i;
						break;
					}
				}
			}
			if (BackgroundIdx == -1 && sttype != -1)
			{
				//The object is loaded but not in the backgrounds list
				BackgroundIdx = Data.Backgrounds.Length;
				Array.Resize(ref Data.Backgrounds, BackgroundIdx +1);
				if (Data.Structure.Objects[sttype] != null)
				{
					Data.Backgrounds[BackgroundIdx] = new Background(Arguments[0], new BackgroundObject((StaticObject) Data.Structure.Objects[sttype]));
				}
				else
				{
					//The background object is null (presume the load has failed?, just chuck in a new static one for the minute)
					Data.Backgrounds[BackgroundIdx] = new Background(Arguments[0], new StaticBackground(null, 6, false));
				}
			}
			if (BackgroundIdx != -1)
			{
				Data.Blocks[BlockIndex].Background = BackgroundIdx;
			}
			else
			{
				//Failed to load the background
			}
		}

		/// <summary>Changes the current adhesion value</summary>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		static void SetAdhesion(string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			switch (Arguments.Length)
			{
				case 1:
					double c = 0;
					if (!NumberFormats.TryParseDoubleVb6(Arguments[0], out c))
					{
						//Invalid number
						return;
					}
					//Presumably this is just the adhesion coefficent at 0km/h
					Data.Blocks[BlockIndex].AdhesionMultiplier = (int) (c * 100 / 0.26) / 100.0;
					break;
				case 3:
					double c0 = 0, c1 = 0, c2 = 0;
					NumberFormats.TryParseDoubleVb6(Arguments[0], out c0);
					NumberFormats.TryParseDoubleVb6(Arguments[1], out c1);
					NumberFormats.TryParseDoubleVb6(Arguments[2], out c2);
					if (c1 == 0 && c0 != 0 && c2 != 0)
					{
						if (c0 == 0.35 && c2 == 0.01)
						{
							//Default adhesion value of 100
							Data.Blocks[BlockIndex].AdhesionMultiplier = 1;
							return;
						}
						int ca = (int)(c0 * 100 / 0.26);
						double cb = 1.0 / (300 * (ca / 100.0 * 0.259999990463257));

						if (Math.Round(cb, 8) == c2)
						{
							//BVE2 / 4 converted value, so let's use that
							Data.Blocks[BlockIndex].AdhesionMultiplier = ca / 100.0;
							return;
						}
						//They don't match.....
						//Use an average of the two to get something reasonable
						//TODO: Implement adhesion based upon speed formulae
						Data.Blocks[BlockIndex].AdhesionMultiplier = (ca + cb) / 2 / 100;
						return;
					}

					break;
				default:
					//Must be 1 or 3
					return;
			}
		}

		/// <summary>Places a structure in the world</summary>
		/// <param name="key">The structure key</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		internal void PutStructure(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength, bool Type2)
		{
			//Find the key in our object list
			int sttype = FindStructureIndex(key, Data);
			string railkey = Arguments[0].RemoveEnclosingQuotes();
			int idx = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
			if (idx == -1)
			{
				//Our rail index was not found in the array, so we must create it with a starting position of 0,0
				//As commands may come in any order, the position may be modified later on....
				SecondaryTrack(railkey, new string[]{"0","0"}, ref Data, BlockIndex, UnitOfLength);
				idx = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
				if (idx == -1)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Rail key is invalid in Structure.Put");
					return;
				}
			}
			if (sttype == -1)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Structure key is invalid in Structure.Put");
				return;
			}

			double x = 0.0, y = 0.0, z = 0.0;
			double yaw = 0.0, pitch = 0.0, roll = 0.0;
			int Type = 0;
				/*
				 * Unhelpfully, there are two types of structure command
				 * If our argument length is 3, then it's type 1, otherwise type 2
				 * 
				 * TYPE 1 .Put0:
				 * 0 = Rail key
				 * 1 = 0- Flat object 1- Follows gradient 2- Follows cant 3- Follows gradient & cant
				 * 2 = Object length, used for rotations (NOT IMPLEMENTED AT PRESENT)
				 * 
				 * TYPE 2 .Put:
				 * 0 = Rail key
				 * 1 = X
				 * 2 = Y
				 * 3 = Z
				 * 4 = Pitch (??)
				 * 5 = Yaw (??)
				 * 6 = Roll
				 * 7 = 0- Flat object 1- Follows gradient 2- Follows gradient & cant
				 * 8 = Object length
				 */
			if (Type2)
			{
				if (!NumberFormats.TryParseIntVb6(Arguments[1], out Type) && (Type < 0 || Type > 3))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Object rotation type is invalid in Structure.Put0");
					Type = 0;
				}
			}
			else
			{
				if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
				{
					//Interface.AddMessage(Interface.MessageType.Error, false,"X is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					x = 0.0;
				}
				if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
				{
					//Interface.AddMessage(Interface.MessageType.Error, false,"Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					y = 0.0;
				}
				if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out z))
				{
					//Interface.AddMessage(Interface.MessageType.Error, false,"Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					z = 0.0;
				}
				//BVETS documentation states that yaw and pitch are the opposite way around......
				//Not sure whether this is our bug or that of BVETS at the minute
				if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out pitch))
				{
					//Interface.AddMessage(Interface.MessageType.Error, false,"Yaw is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					pitch = 0.0;
				}
				if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out yaw))
				{
					//Interface.AddMessage(Interface.MessageType.Error, false,"Pitch is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					yaw = 0.0;
				}
				if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
				{
					//Interface.AddMessage(Interface.MessageType.Error, false,"Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					roll = 0.0;
				}
				if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[7], out Type))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Object rotation type is invalid in Structure.Put");
					Type = 0;
				}
				if (Type < 0 || Type > 3)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Object rotation type is invalid in Structure.Put");
					Type = 0;
				}
			}
			int n;
			if (idx >= Data.Blocks[BlockIndex].RailFreeObj.Length)
			{
				Array.Resize(ref Data.Blocks[BlockIndex].RailFreeObj, idx + 1);
			}
			if (Data.Blocks[BlockIndex].RailFreeObj[idx] == null)
			{
				Data.Blocks[BlockIndex].RailFreeObj[idx] = new Object[1];
				n = 0;
			}
			else
			{
				n = Data.Blocks[BlockIndex].RailFreeObj[idx].Length;
				Array.Resize(ref Data.Blocks[BlockIndex].RailFreeObj[idx], n + 1);
			}
			switch (Type)
			{
				case 0:
					//Object is horizontal
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].X = x;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Y = y;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Z = z;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].BaseTransformation = RailTransformationTypes.Flat;
					break;
				case 1:
					//Follows gradient
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].X = x;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Y = y;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Z = z;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].BaseTransformation = RailTransformationTypes.FollowsPitch;
					break;
				case 2:
					//Follows cant
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].X = x;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Y = y;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Z = z;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll * 0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].BaseTransformation = RailTransformationTypes.FollowsCant;
					break;
				case 3:
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].X = x;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Y = y;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Z = z;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw*0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch*0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll*0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].BaseTransformation = RailTransformationTypes.FollowsBoth;
					break;
			}
		}

		internal void PutStructureBetween(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			int sttype = FindStructureIndex(key, Data);
			if (sttype == -1)
			{
				//Object not loaded
				return;
			}
			string railkey = Arguments[0].Trim().RemoveEnclosingQuotes();
			int idx1 = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
			if (idx1 == -1)
			{
				//Our rail index was not found in the array, so we must create it with a starting position of 0,0
				//As commands may come in any order, the position may be modified later on....
				SecondaryTrack(railkey, new string[] { "0", "0" }, ref Data, BlockIndex, UnitOfLength);
				idx1 = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
			}

			railkey = Arguments[1].Trim().RemoveEnclosingQuotes();
			int idx2 = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
			if (idx2 == -1)
			{
				//Our rail index was not found in the array, so we must create it with a starting position of 0,0
				//As commands may come in any order, the position may be modified later on....
				SecondaryTrack(railkey, new string[] { "0", "0" }, ref Data, BlockIndex, UnitOfLength);
				idx2 = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
			}
			double length = 0.0;
			if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out length))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "CrackStructureIndex is invalid in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				length = 0.0;
			}
			else
			{
				if (idx1 < 0)
				{
					//Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex1 is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				}
				else if (idx2 < 0)
				{
					//Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex2 is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				}
				else if (idx1 == idx2)
				{
					//Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex1 is expected to be unequal to Index2 in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				}
				else
				{
					if (idx1 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx1].RailStart)
					{
						//Interface.AddMessage(Interface.MessageType.Warning, false, "RailIndex1 " + idx1 + " could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					}
					if (idx2 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx2].RailStart)
					{
						//Interface.AddMessage(Interface.MessageType.Warning, false, "RailIndex2 " + idx2 + " could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
					}
					int n = Data.Blocks[BlockIndex].Crack.Length;
					Array.Resize(ref Data.Blocks[BlockIndex].Crack, n + 1);
					Data.Blocks[BlockIndex].Crack[n].PrimaryRail = idx1;
					Data.Blocks[BlockIndex].Crack[n].SecondaryRail = idx2;
					Data.Blocks[BlockIndex].Crack[n].Type = sttype;
				}
			}
		}

		/// <summary>Finds a station from the list, and places it within the world</summary>
		/// <param name="key">The station key</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		internal void PutStation(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			for (int i = 0; i < CurrentRoute.Stations.Length; i++)
			{
				if (CurrentRoute.Stations[i].Key == key)
				{
					Data.Blocks[BlockIndex].Station = i;
					int doors = 0;
					//Add station stop
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out doors))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "BackwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						doors = 0;
					}
					double backw = 5.0, forw = 5.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out backw))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "BackwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						backw = 5.0;
					}
					else if (backw <= 0.0)
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "BackwardTolerance is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						backw = 5.0;
					}
					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out forw))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "ForwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						forw = 5.0;
					}
					else if (forw <= 0.0)
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "ForwardTolerance is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						forw = 5.0;
					}

					CurrentRoute.Stations[i].OpenLeftDoors = doors < 0.0;
					CurrentRoute.Stations[i].OpenRightDoors = doors > 0.0;

					int n = Data.Blocks[BlockIndex].Stop.Length;
					Array.Resize(ref Data.Blocks[BlockIndex].Stop, n + 1);
					Data.Blocks[BlockIndex].Stop[n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].Stop[n].Station = i;
					Data.Blocks[BlockIndex].Stop[n].Direction = 0;
					Data.Blocks[BlockIndex].Stop[n].ForwardTolerance = forw;
					Data.Blocks[BlockIndex].Stop[n].BackwardTolerance = backw;
					Data.Blocks[BlockIndex].Stop[n].Cars = 0;
					return;
				}
			}
			//TODO: Add error message stating that the station's key has not been found
		}

		/// <summary>Starts a repeating object</summary>
		/// <param name="key">The repeating object's key</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		internal void StartRepeater(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength, bool Type2)
		{
			int n = 0, sttype = -1, type = 0, idx;
			double span = 0, interval = 0;
			double x = 0.0, y = 0.0, z = 0.0;
			double yaw = 0.0, pitch = 0.0, roll = 0.0;
			if (Data.Blocks[BlockIndex].Repeaters == null)
			{
				Data.Blocks[BlockIndex].Repeaters = new Repeater[1];
			}
			else
			{
				n = Data.Blocks[BlockIndex].Repeaters.Length;
				bool Found = false;
				for (int i = 0; i < n; i++)
				{
					if (Data.Blocks[BlockIndex].Repeaters[i].Name == key)
					{
						//If our repeater already exists, we need to modify it rather than spawning another....
						n = i;
						Found = true;
					}

				}
				if (!Found)
				{
					//Otherwise, reset our control variable and increase the array length
					n = Data.Blocks[BlockIndex].Repeaters.Length;
					Array.Resize(ref Data.Blocks[BlockIndex].Repeaters, n + 1);
					Data.Blocks[BlockIndex].Repeaters[n] = new Repeater();
				}
			}


			if (Type2 == false)
			{
				if (Arguments.Length >= 10)
				{
					//Find the rail key
					string railkey = Arguments[0].Trim().RemoveEnclosingQuotes();
					idx = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
					if (idx == -1)
					{
						//Our rail index was not found in the array, so we must create it with a starting position of 0,0
						//As commands may come in any order, the position may be modified later on....
						SecondaryTrack(railkey, new string[] { "0", "0" }, ref Data, BlockIndex, UnitOfLength);
						idx = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
					}
					//Structure key
					string structurekey = Arguments[10].Trim().RemoveEnclosingQuotes();
					//Find the structure type
					sttype = FindStructureIndex(structurekey, Data);
					if (sttype == -1)
					{
						//TODO: Add error message
						return;
					}

					if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"X is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						x = 0.0;
					}
					if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						y = 0.0;
					}
					if (!NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out z))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						z = 0.0;
					}
					//BVETS documentation states that yaw and pitch are the opposite way around......
					//Not sure whether this is our bug or that of BVETS at the minute
					if (!NumberFormats.TryParseDoubleVb6(Arguments[4], out pitch))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Yaw is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						pitch = 0.0;
					}
					if (!NumberFormats.TryParseDoubleVb6(Arguments[5], out yaw))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Pitch is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						yaw = 0.0;
					}
					if (!NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						roll = 0.0;
					}
					if (!NumberFormats.TryParseIntVb6(Arguments[7], out type))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						type = 0;
					}
					if (!NumberFormats.TryParseDoubleVb6(Arguments[8], out span))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						span = 0.0;
					}
					if (!NumberFormats.TryParseDoubleVb6(Arguments[9], out interval))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						interval = 0.0;
					}
				}
				else
				{
					//TODO: Add error message that we have an incomplete repeater definition (Missing structure key.....)
					return;
				}
			}
			else
			{
				//Parse the repeater data
				if (Arguments.Length > 4)
				{
					//Find the rail key
					string railkey = Arguments[0].Trim().RemoveEnclosingQuotes();
					idx = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
					if (idx == -1)
					{
						//Our rail index was not found in the array, so we must create it with a starting position of 0,0
						//As commands may come in any order, the position may be modified later on....
						SecondaryTrack(railkey, new string[] {"0", "0"}, ref Data, BlockIndex, UnitOfLength);
						idx = FindRailIndex(railkey.Trim(), Data.Blocks[BlockIndex].Rail);
					}

					//Arguments 1 is whether this is a ground based object or a rail based object
					NumberFormats.TryParseIntVb6(Arguments[1], out type);
					//Arguments 2 is the span (Length of object for rotations- Only supports 25m legacy block lengths at present)
					NumberFormats.TryParseDoubleVb6(Arguments[2], out span);
					//Arguments 3 is the repetiton distance, again only supports legacy 25m block lengths
					NumberFormats.TryParseDoubleVb6(Arguments[3], out interval);

					//Structure key
					string structurekey = Arguments[4].Trim().RemoveEnclosingQuotes();
					//Find the structure type
					sttype = FindStructureIndex(structurekey, Data);
					if (sttype == -1)
					{
						//TODO: Add error message
						return;
					}
					//TODO: Multiple repeating objects (cycle) are not handled
				}
				else
				{
					//TODO: Add error message that we have an incomplete repeater definition (Missing structure key.....)
					return;
				}
			}
			if (interval == 24.99)
			{
				/*
				 * The BVETS convertor generates an interval value of 24.99
				 * Treat this as one complete block
				 */
				interval = 25;
			}
			Data.Blocks[BlockIndex].Repeaters[n].RailIndex = idx;
			Data.Blocks[BlockIndex].Repeaters[n].Name = key;
			Data.Blocks[BlockIndex].Repeaters[n].Type = type;
			Data.Blocks[BlockIndex].Repeaters[n].StructureTypes = new int[1];
			Data.Blocks[BlockIndex].Repeaters[n].StructureTypes[0] = sttype;
			Data.Blocks[BlockIndex].Repeaters[n].Span = span;
			Data.Blocks[BlockIndex].Repeaters[n].RepetitionInterval = interval;
			Data.Blocks[BlockIndex].Repeaters[n].TrackPosition = Data.TrackPosition;

			Data.Blocks[BlockIndex].Repeaters[n].X = x;
			Data.Blocks[BlockIndex].Repeaters[n].Y = y;
			Data.Blocks[BlockIndex].Repeaters[n].Z = z;
			Data.Blocks[BlockIndex].Repeaters[n].Yaw = yaw;
			Data.Blocks[BlockIndex].Repeaters[n].Pitch = pitch;
			Data.Blocks[BlockIndex].Repeaters[n].Roll = roll;
		}

		/// <summary>Ends a repeating object</summary>
		/// <param name="key">The repeating object's key</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		static void EndRepeater(string key, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			if (Data.Blocks[BlockIndex].Repeaters == null || Data.Blocks[BlockIndex].Repeaters.Length == 0)
			{
				//No current repeaters
				//TODO: Add error message stating that there are no current defined repeaters
				return;
			}
			bool RepeaterFound = false;
			for (int i = 0; i < Data.Blocks[BlockIndex].Repeaters.Length; i++)
			{
				if (Data.Blocks[BlockIndex].Repeaters[i].Name == key)
				{
					//Shift elements in the array & resize
					for (int a = i; a < Data.Blocks[BlockIndex].Repeaters.Length - 1; a++)
					{
						Data.Blocks[BlockIndex].Repeaters[a] = Data.Blocks[BlockIndex].Repeaters[a + 1];
					}
					Array.Resize(ref Data.Blocks[BlockIndex].Repeaters, Data.Blocks[BlockIndex].Repeaters.Length - 1);
					RepeaterFound = true;
					break;
				}
			}
			if (!RepeaterFound)
			{
				//TODO: Add error message stating that the repeater's key has not been found
			}


		}

		/// <summary>Sets the speed limit</summary>
		/// <param name="Limit">The speed limit to set in km/h</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		internal void StartSpeedLimit(double Limit, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			int n = Data.Blocks[BlockIndex].Limit.Length;
			Array.Resize(ref Data.Blocks[BlockIndex].Limit, n + 1);
			Data.Blocks[BlockIndex].Limit[n].TrackPosition = Data.TrackPosition;
			Data.Blocks[BlockIndex].Limit[n].Speed = Limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed*Limit;
			//NOT USED BY BVE5
			Data.Blocks[BlockIndex].Limit[n].Direction = 0;
			Data.Blocks[BlockIndex].Limit[n].Cource = 0;

			//TODO: This only handles ending one enclosing set of limits, really ought to handle multiples......
			//Set previous speed limit, required for ending the limit
			Data.Blocks[BlockIndex].Limit[n].LastSpeed = 0;
			if (n > 0)
			{
				//We have multiple speed limits in this block, so set to the previous speed limit in the block
				Data.Blocks[BlockIndex].Limit[n].LastSpeed = Data.Blocks[BlockIndex].Limit[n - 1].Speed;
			}
			else if (BlockIndex > 0)
			{
				//This is not the first block, so check to see if a previous speed limit has been defined
				if (Data.Blocks[BlockIndex - 1].Limit.Length > 0)
				{
					//Limit defined in the last block, so use this
					Data.Blocks[BlockIndex].Limit[n].LastSpeed =
						Data.Blocks[BlockIndex - 1].Limit[Data.Blocks[BlockIndex - 1].Limit.Length - 1].Speed;
				}
			}
		}


		/// <summary>Ends the current speed limit</summary>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		internal void EndSpeedLimit(ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			int n = Data.Blocks[BlockIndex].Limit.Length;
			Array.Resize(ref Data.Blocks[BlockIndex].Limit, n + 1);
			Data.Blocks[BlockIndex].Limit[n].TrackPosition = Data.TrackPosition;
			Data.Blocks[BlockIndex].Limit[n].Speed = Data.Blocks[BlockIndex].Limit[n - 1].Speed;
			//NOT USED BY BVE5
			Data.Blocks[BlockIndex].Limit[n].Direction = 0;
			Data.Blocks[BlockIndex].Limit[n].Cource = 0;

			//TODO: This only handles ending one enclosing set of limits, really ought to handle multiples......
			//Set previous speed limit, required for ending the limit
			Data.Blocks[BlockIndex].Limit[n].LastSpeed = 0;

		}

		/// <summary>Starts a new signalling section</summary>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="CurrentSection">The zero-based index of the current section (updated via 'ref')</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		static void StartSection(string[] Arguments, ref RouteData Data, int BlockIndex, ref int CurrentSection, double[] UnitOfLength)
		{
			if (Arguments.Length == 0)
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"At least one argument is required in " + Command + "at line " + Expressions[j].Line.ToString(Culture) +", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			else
			{
				int[] aspects = new int[Arguments.Length];
				for (int i = 0; i < Arguments.Length; i++)
				{
					if (!NumberFormats.TryParseIntVb6(Arguments[i], out aspects[i]))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Aspect" + i.ToString(Culture) + " is invalid in " + Command + "at line " +Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " +Expressions[j].File);
						aspects[i] = -1;
					}
					else if (aspects[i] < 0)
					{
						//Interface.AddMessage(Interface.MessageType.Error, false,"Aspect" + i.ToString(Culture) + " is expected to be non-negative in " + Command + "at line " +Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " +Expressions[j].File);
						aspects[i] = -1;
					}
				}
				int n = Data.Blocks[BlockIndex].Section.Length;
				Array.Resize(ref Data.Blocks[BlockIndex].Section, n + 1);
				Data.Blocks[BlockIndex].Section[n] = new Section(Data.TrackPosition, aspects, -1, SectionType.IndexBased);
				/*
					if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal)
					{
						if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed)
						{
							Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = CurrentStation;
							DepartureSignalUsed = true;
						}
					}
					 */
				CurrentSection++;
			}

		}

		/// <summary>Places an in-game signal</summary>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="CurrentSection">The zero-based index of the current section (updated via 'ref')</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		internal void PlaceSignal(string key, string[] Arguments, ref RouteData Data, int BlockIndex, int CurrentSection, double[] UnitOfLength)
		{
			int[] aspects = new int[0];
			int comp = -1;
			bool SignalFound = false;
			for (int i = 0; i < Data.CompatibilitySignalData.Length; i++)
			{
				if (key == Data.CompatibilitySignalData[i].Key)
				{
					aspects = Data.CompatibilitySignalData[i].Numbers;
					comp = i;
					SignalFound = true;
					break;
				}
			}
			if (!SignalFound)
			{
				//TODO: Add appropriate error message that our signal has not been loaded
				return;
			}
			
			double x = 0.0, y = 0.0;
			if (Arguments.Length >= 3 && Arguments[2].Length > 0 &&
			    !NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				x = 0.0;
			}
			if (Arguments.Length >= 4 && Arguments[3].Length > 0 &&
			    !NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				y = 0.0;
			}
			double yaw = 0.0, pitch = 0.0, roll = 0.0;
			if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out yaw))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				yaw = 0.0;
			}
			if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out pitch))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Pitch is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				pitch = 0.0;
			}
			if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out roll))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				roll = 0.0;
			}


			int n = Data.Blocks[BlockIndex].Section.Length;
			Array.Resize(ref Data.Blocks[BlockIndex].Section, n + 1);
			Data.Blocks[BlockIndex].Section[n] = new Section(Data.TrackPosition, aspects, -1, SectionType.ValueBased, x == 0.0);
			/*
			if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal)
			{
				if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed)
				{
					Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = CurrentStation;
					DepartureSignalUsed = true;
				}
			}
			*/
			CurrentSection++;
			n = Data.Blocks[BlockIndex].Signal.Length;
			Array.Resize(ref Data.Blocks[BlockIndex].Signal, n + 1);
			Data.Blocks[BlockIndex].Signal[n] = new Signal(Data.TrackPosition, CurrentSection, Data.CompatibilitySignalData[comp], new Vector2(x, y < 0.0 ? 4.8 : y),0.0174532925199433*yaw, 0.0174532925199433*pitch, 0.0174532925199433*roll, x != 0.0,  x != 0.0 & y < 0.0);
		}

		/// <summary>Plays the points sound for the current block</summary>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		static void PlayJointSound(string[] Arguments, ref RouteData Data, int BlockIndex)
		{
			//TODO: It appears that a variety of point noises may be set, this only supports a single at the minute
			Data.Blocks[BlockIndex].JointNoise = true;
		}

		/// <summary>Changes the cab brightness value for the current block</summary>
		/// <param name="Immediate">Whether this should be updated immediately or interpolated</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		static void ChangeBrightness(bool Immediate, string[] Arguments, ref RouteData Data, int BlockIndex)
		{
			float b;
			if (NumberFormats.TryParseFloatVb6(Arguments[0], out b))
			{
				if (Immediate)
				{
					//Immediately changes the brightness mode (?? Check behaviour against BVE5, possibly just a spelling change ??)
					int n = Data.Blocks[BlockIndex].Brightness.Length;
					Array.Resize(ref Data.Blocks[BlockIndex].Brightness, n + 2);
					Data.Blocks[BlockIndex].Brightness[n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].Brightness[n].Value = Data.LastBrightness;
					Data.Blocks[BlockIndex].Brightness[n + 1].TrackPosition = Data.TrackPosition + 1;
					Data.Blocks[BlockIndex].Brightness[n + 1].Value = b;
				}
				else
				{
					//Default brightness mode as per previous
					int n = Data.Blocks[BlockIndex].Brightness.Length;
					Array.Resize(ref Data.Blocks[BlockIndex].Brightness, n + 1);
					Data.Blocks[BlockIndex].Brightness[n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].Brightness[n].Value = b;
					
				}
				Data.LastBrightness = b;
				return;
			}
			//Add error message NaN
		}

		/// <summary>Changes the current track run sound</summary>
		/// <param name="RunSoundIndex">The index of the new run sound from this point onwards</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		static void ChangeRunSound(int RunSoundIndex, ref RouteData Data, int BlockIndex)
		{
			int idx = -1;
			for (int i = 0; i < Data.Blocks[BlockIndex].RunSounds.Length; i++)
			{
				if (Data.Blocks[BlockIndex].RunSounds[i].TrackPosition == Data.TrackPosition)
				{
					idx = i;
					break;
				}
			}
			if (idx == -1)
			{
				idx = Data.Blocks[BlockIndex].RunSounds.Length;
				Array.Resize(ref Data.Blocks[BlockIndex].RunSounds, idx + 1);
				Data.Blocks[BlockIndex].RunSounds[idx] = new TrackSound(Data.TrackPosition, RunSoundIndex, Data.Blocks[BlockIndex].RunSounds[idx - 1].FlangeSoundIndex);
				
			}
			else
			{
				Data.Blocks[BlockIndex].RunSounds[idx].RunSoundIndex = RunSoundIndex;
			}		
		}

		/// <summary>Changes the current flange sound</summary>
		/// <param name="FlangeSoundIndex">The index of the new flange sound from this point onwards</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		internal void ChangeFlangeSound(int FlangeSoundIndex, ref RouteData Data, int BlockIndex)
		{
			int idx = -1;
			for (int i = 0; i < Data.Blocks[BlockIndex].RunSounds.Length; i++)
			{
				if (Data.Blocks[BlockIndex].RunSounds[i].TrackPosition == Data.TrackPosition)
				{
					idx = i;
					break;
				}
			}
			if (idx == -1)
			{
				idx = Data.Blocks[BlockIndex].RunSounds.Length;
				Array.Resize(ref Data.Blocks[BlockIndex].RunSounds, idx + 1);
				Data.Blocks[BlockIndex].RunSounds[idx] = new TrackSound(Data.TrackPosition, Data.Blocks[BlockIndex].RunSounds[idx - 1].RunSoundIndex, FlangeSoundIndex);
			}
			else
			{
				Data.Blocks[BlockIndex].RunSounds[idx].FlangeSoundIndex = FlangeSoundIndex;
			}
		}

		/// <summary>Sets the route ambient light</summary>
		/// <param name="Arguments">The command arguments</param>
		internal void SetAmbientLight(string[] Arguments)
		{
			float r = 1, g = 1, b = 1;
			if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[0], out r))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is invalid in Light.Ambient at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			else if (r < 0 | r > 1)
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is required to be within the range from 0 to 1 in Light.Ambient at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				r = r < 0 ? 0 : 1;
			}
			if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[1], out g))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is invalid in Light.Ambient at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			else if (g < 0 | g > 1)
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is required to be within the range from 0 to 1 in Light.Ambient at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				g = g < 0 ? 0 : 1;
			}
			if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[2], out b))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is invalid in Light.Ambient at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			else if (b < 0 | b > 1)
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is required to be within the range from 0 to 1 in Light.Ambient at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				b = b < 0 ? 0 : 1;
			}
			CurrentRoute.Atmosphere.AmbientLightColor = new Color24((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
		}

		/// <summary>Sets the route diffuse light</summary>
		/// <param name="Arguments">The command arguments</param>
		internal void SetDiffuseLight(string[] Arguments)
		{
			float r = 1, g = 1, b = 1;
			if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[0], out r))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is invalid in Light.Diffuse at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			else if (r < 0 | r > 1)
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is required to be within the range from 0 to 1 in Light.Diffuse at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				r = r < 0 ? 0 : 1;
			}
			if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[1], out g))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is invalid in Light.Diffuse at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			else if (g < 0 | g > 1)
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is required to be within the range from 0 to 1 in Light.Diffuse at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				g = g < 0 ? 0 : 1;
			}
			if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[2], out b))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is invalid in Light.Diffuse at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			else if (b < 0 | b > 1)
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "BlueValue is required to be within the range from 0 to 1 in Light.Diffuse at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				b = b < 0 ? 0 : 1;
			}
			CurrentRoute.Atmosphere.DiffuseLightColor = new Color24((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
		}

		/// <summary>Sets the direction of the route diffuse light</summary>
		/// <param name="Arguments">The command arguments</param>
		internal void SetLightDirection(string[] Arguments)
		{
			double theta = 60.0, phi = -26.565051177078;
			if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out theta))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Theta is invalid in Light.Direction at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out phi))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Phi is invalid in Light.Direction at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
			}
			theta *= 0.0174532925199433;
			phi *= 0.0174532925199433;
			double dx = Math.Cos(theta) * Math.Sin(phi);
			double dy = -Math.Sin(theta);
			double dz = Math.Cos(theta) * Math.Cos(phi);
			CurrentRoute.Atmosphere.LightPosition = new Vector3((float)-dx, (float)-dy, (float)-dz);
		}

		internal void SecondaryTrack(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			//First, convert the key to the track's index
			int idx = FindRailIndex(key, Data.Blocks[BlockIndex].Rail);
			if (idx == -1)
			{
				//We haven't found the rail in our list, so we need to set the variable to the last member of the array & resize
				idx = Data.Blocks[BlockIndex].Rail.Length;
				Array.Resize(ref Data.Blocks[BlockIndex].Rail, idx + 1);
				Data.Blocks[BlockIndex].Rail[idx].Key = key;
				Data.Blocks[BlockIndex].Rail[idx].HorizontalRadius = 0;
				Data.Blocks[BlockIndex].Rail[idx].VerticalRadius = 0;
			}
			
			if (Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed)
			{
				Data.Blocks[BlockIndex].Rail[idx].RailEnd = true;
			}
			{
				Data.Blocks[BlockIndex].Rail[idx].RailStart = true;
				Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed = true;
				if (Arguments.Length >= 1)
				{
					if (Arguments[0].Length > 0)
					{
						double x;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							x = 0.0;
						}
						Data.Blocks[BlockIndex].Rail[idx].RailStartX = x;
					}
					if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd)
					{
						Data.Blocks[BlockIndex].Rail[idx].RailEndX = Data.Blocks[BlockIndex].Rail[idx].RailStartX;
					}
				}
				if (Arguments.Length >= 2)
				{
					if (Arguments[1].Length > 0)
					{
						double y;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							y = 0.0;
						}
						Data.Blocks[BlockIndex].Rail[idx].RailStartY = y;
					}
					if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd)
					{
						Data.Blocks[BlockIndex].Rail[idx].RailEndY = Data.Blocks[BlockIndex].Rail[idx].RailStartY;
					}
				}
				if (Arguments.Length >= 3)
				{
					if (Arguments[2].Length > 0)
					{
						double HorizontalRadius;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[2], UnitOfLength, out HorizontalRadius))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "HorizontalRadius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							HorizontalRadius = 0.0;
						}
						Data.Blocks[BlockIndex].Rail[idx].HorizontalRadius = HorizontalRadius;
					}
				}
				if (Arguments.Length >= 4)
				{
					if (Arguments[3].Length > 0)
					{
						double VerticalRadius;
						if (!NumberFormats.TryParseDoubleVb6(Arguments[3], UnitOfLength, out VerticalRadius))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "HorizontalRadius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							VerticalRadius = 0.0;
						}
						Data.Blocks[BlockIndex].Rail[idx].VerticalRadius = VerticalRadius;
					}
				}
				if (Data.Blocks[BlockIndex].RailType.Length <= idx)
				{
					Array.Resize(ref Data.Blocks[BlockIndex].RailType, idx + 1);
				}
				
			}

		}

		internal void InterpolateSecondaryTrack(string key, double Distance, ref RouteData Data, int BlockIndex, double[] UnitOfLength, bool Vertical)
		{
			int idx = FindRailIndex(key, Data.Blocks[BlockIndex].Rail);
			if (idx == -1)
			{
				//We haven't found the rail in our list, so we need to set the variable to the last member of the array & resize
				idx = Data.Blocks[BlockIndex].Rail.Length;
				Array.Resize(ref Data.Blocks[BlockIndex].Rail, idx + 1);
				Data.Blocks[BlockIndex].Rail[idx].Key = key;
			}
			//NOTE: No interpolation at the minute, straight move.....

			if (Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed)
			{
				Data.Blocks[BlockIndex].Rail[idx].RailEnd = true;
			}
			if (Vertical)
			{
				//Vertically interpolate the height of the track
				Data.Blocks[BlockIndex].Rail[idx].RailStart = true;
				Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed = true;
				Data.Blocks[BlockIndex].Rail[idx].RailStartY = Distance;
				if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd)
				{
					Data.Blocks[BlockIndex].Rail[idx].RailEndY = Data.Blocks[BlockIndex].Rail[idx].RailStartY;
				}
			}
			else
			{
				//Horizontally interpolate the height of the track
				Data.Blocks[BlockIndex].Rail[idx].RailStart = true;
				Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed = true;
				Data.Blocks[BlockIndex].Rail[idx].RailStartX = Distance;
				if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd)
				{
					Data.Blocks[BlockIndex].Rail[idx].RailEndX = Data.Blocks[BlockIndex].Rail[idx].RailStartX;
				}
			}
		}

	}
}
