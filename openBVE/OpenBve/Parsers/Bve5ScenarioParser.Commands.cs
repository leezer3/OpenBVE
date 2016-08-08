using System;
using OpenBveApi.Colors;

namespace OpenBve
{
	partial class Bve5ScenarioParser
	{
		/// <summary>Parses a legacy command contained within a BVE5 scenario map</summary>
		/// <param name="command">The command to parse</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		/// <param name="PreviewOnly">Whether this is a preview only</param>
		static void ParseLegacyCommand(string command, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength, bool PreviewOnly)
		{
			switch (command)
			{
				//Legacy commands
				case "curve":
				{
					double radius = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], UnitOfLength, out radius))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "Radius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						radius = 0.0;
					}
					double cant = 0.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out cant))
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
						if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out start))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "StartingDistance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							start = 0.0;
						}
						if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out end))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "EndingDistance is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							end = 0.0;
						}
						if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out r))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							r = 128;
						}
						else if (r < 0 | r > 255)
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "RedValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							r = r < 0 ? 0 : 255;
						}
						if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseIntVb6(Arguments[3], out g))
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							g = 128;
						}
						else if (g < 0 | g > 255)
						{
							//Interface.AddMessage(Interface.MessageType.Error, false, "GreenValue is required to be within the range from 0 to 255 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
							g = g < 0 ? 0 : 255;
						}
						if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseIntVb6(Arguments[4], out b))
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
							Data.Blocks[BlockIndex].Fog.Start = Game.NoFogStart;
							Data.Blocks[BlockIndex].Fog.End = Game.NoFogEnd;
						}
						Data.Blocks[BlockIndex].Fog.Color = new Color24((byte) r, (byte) g, (byte) b);
						Data.Blocks[BlockIndex].FogDefined = true;
					}
					break;
				case "pitch":
					double p = 0.0;
					if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out p))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "ValueInPermille is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						p = 0.0;
					}
					Data.Blocks[BlockIndex].Pitch = 0.001 * p;
					break;
			}
		}

		/// <summary>Places a structure in the world</summary>
		/// <param name="key">The structure key</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		static void PutStructure(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			//Find the key in our object list
			int sttype = FindStructureIndex(key, Data);
			if (sttype == -1)
			{
				//TODO: Add error message
				return;
			}
			double x = 0.0, y = 0.0, z = 0.0;
			double yaw = 0.0, pitch = 0.0, roll = 0.0;
			int Type = 0;
			if (Arguments.Length >= 2 && Arguments[1].Length > 0 &&
			    !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"X is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				x = 0.0;
			}
			if (Arguments.Length >= 3 && Arguments[2].Length > 0 &&
			    !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				y = 0.0;
			}
			if (Arguments.Length >= 4 && Arguments[3].Length > 0 &&
			    !Interface.TryParseDoubleVb6(Arguments[3], UnitOfLength, out z))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				z = 0.0;
			}
			//BVETS documentation states that yaw and pitch are the opposite way around......
			//Not sure whether this is our bug or that of BVETS at the minute
			if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[4], out pitch))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Yaw is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				pitch = 0.0;
			}
			if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[5], out yaw))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Pitch is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				yaw = 0.0;
			}
			if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[6], out roll))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				roll = 0.0;
			}
			if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !Interface.TryParseIntVb6(Arguments[7], out Type))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " +Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				//roll = 0.0;
			}
			int n;
			switch (Type)
			{
				case 0:
					//Object is horizontal, for the minute we'll call this the equivilant of a GroundFreeObj
					//TODO: We can attach an object to a rail, but still place it horizontal==> Need to find the distance of the rail in question.....
					n = Data.Blocks[BlockIndex].GroundFreeObj.Length;
					Array.Resize<Object>(ref Data.Blocks[BlockIndex].GroundFreeObj, n + 1);
					Data.Blocks[BlockIndex].GroundFreeObj[n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].GroundFreeObj[n].Type = sttype;
					Data.Blocks[BlockIndex].GroundFreeObj[n].X = x;
					Data.Blocks[BlockIndex].GroundFreeObj[n].Y = y;
					Data.Blocks[BlockIndex].GroundFreeObj[n].Z = z;
					Data.Blocks[BlockIndex].GroundFreeObj[n].Yaw = yaw*0.0174532925199433;
					Data.Blocks[BlockIndex].GroundFreeObj[n].Pitch = pitch*0.0174532925199433;
					Data.Blocks[BlockIndex].GroundFreeObj[n].Roll = roll*0.0174532925199433;
					break;
				case 1:
					//Object follows the gradient of it's attached rail (or rail 0 if not specificied)
					int idx = 0;
					if (idx >= Data.Blocks[BlockIndex].RailFreeObj.Length)
					{
						Array.Resize<Object[]>(ref Data.Blocks[BlockIndex].RailFreeObj, idx + 1);
					}
					if (Data.Blocks[BlockIndex].RailFreeObj[idx] == null)
					{
						Data.Blocks[BlockIndex].RailFreeObj[idx] = new Object[1];
						n = 0;
					}
					else
					{
						n = Data.Blocks[BlockIndex].RailFreeObj[idx].Length;
						Array.Resize<Object>(ref Data.Blocks[BlockIndex].RailFreeObj[idx], n + 1);
					}
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].X = x;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Y = y;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Z = z;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw*0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch*0.0174532925199433;
					Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll*0.0174532925199433;
					break;
			}
		}

		/// <summary>Finds a station from the list, and places it within the world</summary>
		/// <param name="key">The station key</param>
		/// <param name="Arguments">The command arguments</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		/// <param name="UnitOfLength">The current unit of length</param>
		static void PutStation(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			for (int i = 0; i < Game.Stations.Length; i++)
			{
				if (Game.Stations[i].Key == key)
				{
					Data.Blocks[BlockIndex].Station = i;
					int doors = 0;
					//Add station stop
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out doors))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "BackwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						doors = 0;
					}
					double backw = 5.0, forw = 5.0;
					if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out backw))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "BackwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						backw = 5.0;
					}
					else if (backw <= 0.0)
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "BackwardTolerance is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						backw = 5.0;
					}
					if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out forw))
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "ForwardTolerance is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						forw = 5.0;
					}
					else if (forw <= 0.0)
					{
						//Interface.AddMessage(Interface.MessageType.Error, false, "ForwardTolerance is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						forw = 5.0;
					}

					Game.Stations[i].OpenLeftDoors = doors < 0.0;
					Game.Stations[i].OpenRightDoors = doors > 0.0;

					int n = Data.Blocks[BlockIndex].Stop.Length;
					Array.Resize<Stop>(ref Data.Blocks[BlockIndex].Stop, n + 1);
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
		static void StartRepeater(string key, string[] Arguments, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			/*
			 * 
			 * WARNING: THIS ONLY SUPPORTS THE SECOND TYPE OF REPEATERS DEFINED AT THE MINUTE.
			 * TYPE 1 ARE LIKELY TO BE BROKEN!!!!
			 * 
			 */
			int n = 0, sttype = -1, type = 0;
			double span = 0, interval = 0;
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
					Array.Resize<Repeater>(ref Data.Blocks[BlockIndex].Repeaters, n + 1);
					Data.Blocks[BlockIndex].Repeaters[n] = new Repeater();
				}
			}

			//Parse the repeater data
			if (Arguments.Length > 4)
			{
				//Arguments 0 is the rail key, not supported ATM

				//Arguments 1 is whether this is a ground based object or a rail based object
				Interface.TryParseIntVb6(Arguments[1], out type);
				//Arguments 2 is the span (Length of object for rotations- Only supports 25m legacy block lengths at present)
				Interface.TryParseDoubleVb6(Arguments[2], out span);
				//Arguments 3 is the repetiton distance, again only supports legacy 25m block lengths
				Interface.TryParseDoubleVb6(Arguments[3], out interval);

				//Structure key
				Arguments[4] = Arguments[4].Trim();
				//Trim single quotes
				if (Arguments[4].StartsWith("'") && Arguments[4].EndsWith("'"))
				{
					Arguments[4] = Arguments[4].Substring(1, Arguments[4].Length - 2);
				}
				//Find the structure type
				sttype = FindStructureIndex(Arguments[4], Data);
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

			Data.Blocks[BlockIndex].Repeaters[n].Name = key;
			Data.Blocks[BlockIndex].Repeaters[n].Type = type;
			Data.Blocks[BlockIndex].Repeaters[n].StructureTypes = new int[1];
			Data.Blocks[BlockIndex].Repeaters[n].StructureTypes[0] = sttype;
			Data.Blocks[BlockIndex].Repeaters[n].Span = span;
			Data.Blocks[BlockIndex].Repeaters[n].RepetitionInterval = interval;
			Data.Blocks[BlockIndex].Repeaters[n].TrackPosition = Data.TrackPosition;
			//Finally, place the first object- All objects after this are placed via the block generation command

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
		static void StartSpeedLimit(double Limit, ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			int n = Data.Blocks[BlockIndex].Limit.Length;
			Array.Resize<Limit>(ref Data.Blocks[BlockIndex].Limit, n + 1);
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
		static void EndSpeedLimit(ref RouteData Data, int BlockIndex, double[] UnitOfLength)
		{
			int n = Data.Blocks[BlockIndex].Limit.Length;
			Array.Resize<Limit>(ref Data.Blocks[BlockIndex].Limit, n + 1);
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
					if (!Interface.TryParseIntVb6(Arguments[i], out aspects[i]))
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
				Array.Resize<Section>(ref Data.Blocks[BlockIndex].Section, n + 1);
				Data.Blocks[BlockIndex].Section[n].TrackPosition = Data.TrackPosition;
				Data.Blocks[BlockIndex].Section[n].Aspects = aspects;
				Data.Blocks[BlockIndex].Section[n].Type = Game.SectionType.IndexBased;
				Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = -1;
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
		static void PlaceSignal(string key, string[] Arguments, ref RouteData Data, int BlockIndex, int CurrentSection, double[] UnitOfLength)
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
			    !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				x = 0.0;
			}
			if (Arguments.Length >= 4 && Arguments[3].Length > 0 &&
			    !Interface.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				y = 0.0;
			}
			double yaw = 0.0, pitch = 0.0, roll = 0.0;
			if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[4], out yaw))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				yaw = 0.0;
			}
			if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[5], out pitch))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false,"Pitch is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				pitch = 0.0;
			}
			if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[6], out roll))
			{
				//Interface.AddMessage(Interface.MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
				roll = 0.0;
			}


			int n = Data.Blocks[BlockIndex].Section.Length;
			Array.Resize<Section>(ref Data.Blocks[BlockIndex].Section, n + 1);
			Data.Blocks[BlockIndex].Section[n].TrackPosition = Data.TrackPosition;
			Data.Blocks[BlockIndex].Section[n].Aspects = aspects;
			Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = -1;
			Data.Blocks[BlockIndex].Section[n].Invisible = x == 0.0;
			Data.Blocks[BlockIndex].Section[n].Type = Game.SectionType.ValueBased;
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
			Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signal, n + 1);
			Data.Blocks[BlockIndex].Signal[n].TrackPosition = Data.TrackPosition;
			Data.Blocks[BlockIndex].Signal[n].Section = CurrentSection;
			Data.Blocks[BlockIndex].Signal[n].SignalCompatibilityObjectIndex = comp;
			Data.Blocks[BlockIndex].Signal[n].SignalObjectIndex = -1;
			Data.Blocks[BlockIndex].Signal[n].X = x;
			Data.Blocks[BlockIndex].Signal[n].Y = y < 0.0 ? 4.8 : y;
			Data.Blocks[BlockIndex].Signal[n].Yaw = 0.0174532925199433*yaw;
			Data.Blocks[BlockIndex].Signal[n].Pitch = 0.0174532925199433*pitch;
			Data.Blocks[BlockIndex].Signal[n].Roll = 0.0174532925199433*roll;
			Data.Blocks[BlockIndex].Signal[n].ShowObject = x != 0.0;
			Data.Blocks[BlockIndex].Signal[n].ShowPost = x != 0.0 & y < 0.0;

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
			idx++;
			
		}

		/// <summary>Changes the current flange sound</summary>
		/// <param name="FlangeSoundIndex">The index of the new flange sound from this point onwards</param>
		/// <param name="Data">The RouteData (updated via 'ref')</param>
		/// <param name="BlockIndex">The index of the current block</param>
		static void ChangeFlangeSound(int FlangeSoundIndex, ref RouteData Data, int BlockIndex)
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
			idx++;
		}
	}
}
