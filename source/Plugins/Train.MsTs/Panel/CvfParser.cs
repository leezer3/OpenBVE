//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using LibRender2.Trains;
using OpenBve.Formats.MsTs;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenBveApi.World;
using TrainManager.Car;
using TrainManager.Trains;

namespace Train.MsTs
{
	internal class CabviewFileParser
	{
		// constants
		internal const double StackDistance = 0.000001;

		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double eyeDistance = 1.0;

		internal static string CurrentFolder;

		internal static string FileName;

		private static CabView currentCabView;

		private static readonly List<CabView> cabViews = new List<CabView>();

		private static readonly List<CabComponent> cabComponents = new List<CabComponent>();

		// parse panel config
		internal static bool ParseCabViewFile(string fileName, ref CarBase currentCar)
		{
			FileName = fileName;
			cabViews.Clear();
			cabComponents.Clear();
			CurrentFolder = Path.GetDirectoryName(fileName);
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read);

			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = buffer[0] == 0xFF && buffer[1] == 0xFE;

			string headerString;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				headerString = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 2, 14);
				headerString = Encoding.ASCII.GetString(buffer, 0, 8);
			}

			// SIMISA@F  means compressed
			// SIMISA@@  means uncompressed
			if (headerString.StartsWith("SIMISA@F"))
			{
				fb = new ZlibStream(fb, CompressionMode.Decompress);
			}
			else if (headerString.StartsWith("\r\nSIMISA"))
			{
				// ie us1rd2l1000r10d.s, we are going to allow this but warn
				Console.Error.WriteLine("Improper header in " + fileName);
				fb.Read(buffer, 0, 4);
			}
			else if (!headerString.StartsWith("SIMISA@@"))
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS CVF Parser: Unrecognized cabview file header " + headerString + " in " + FileName);
				return false;
			}

			string subHeader;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				subHeader = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 0, 16);
				subHeader = Encoding.ASCII.GetString(buffer, 0, 8);
			}

			if (subHeader[7] == 't')
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					byte[] newBytes = reader.ReadBytes((int) (fb.Length - fb.Position));
					string s = unicode ? Encoding.Unicode.GetString(newBytes) : Encoding.ASCII.GetString(newBytes);
					TextualBlock block = new TextualBlock(s, KujuTokenID.Tr_CabViewFile);
					ParseBlock(block);
				}

			}
			else if (subHeader[7] != 'b')
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS CVF Parser: Unrecognized subHeader " + subHeader + " in " + FileName);
				return false;
			}
			else
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					KujuTokenID currentToken = (KujuTokenID) reader.ReadUInt16();
					if (currentToken != KujuTokenID.Tr_CabViewFile)
					{
						throw new Exception(); //Shape definition
					}

					reader.ReadUInt16();
					uint remainingBytes = reader.ReadUInt32();
					byte[] newBytes = reader.ReadBytes((int) remainingBytes);
					BinaryBlock block = new BinaryBlock(newBytes, KujuTokenID.Tr_CabViewFile);
					ParseBlock(block);
				}
			}

			//Create panel
			//Create camera restriction
			double worldWidth, worldHeight;
			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
			{
				worldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * eyeDistance;
				worldHeight = worldWidth / Plugin.Renderer.Screen.AspectRatio;
			}
			else
			{
				worldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * eyeDistance / Plugin.Renderer.Screen.AspectRatio;
				worldWidth = worldHeight * Plugin.Renderer.Screen.AspectRatio;
			}

			double x0 = -panelCenter.X / panelResolution;
			double x1 = (panelSize.X - panelCenter.X) / panelResolution;
			double y0 = (panelCenter.Y - panelSize.Y) / panelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = panelCenter.Y / panelResolution * Plugin.Renderer.Screen.AspectRatio;
			currentCar.CameraRestriction.BottomLeft = new Vector3(x0 * worldWidth, y0 * worldHeight, eyeDistance);
			currentCar.CameraRestriction.TopRight = new Vector3(x1 * worldWidth, y1 * worldHeight, eyeDistance);
			currentCar.DriverYaw = Math.Atan((panelCenter.X - panelOrigin.X) * worldWidth / panelResolution);
			currentCar.DriverPitch = Math.Atan((panelOrigin.Y - panelCenter.Y) * worldWidth / panelResolution);

			if(cabViews.Count == 0 || !File.Exists(cabViews[0].FileName))
			{
				return false;
			}
			currentCar.CameraRestrictionMode = CameraRestrictionMode.On;
			Plugin.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
			currentCar.Driver = cabViews[0].Position;
			for (int i = 0; i < cabViews.Count; i++)
			{
				
				Plugin.CurrentHost.RegisterTexture(cabViews[i].FileName, new TextureParameters(null, null), out Texture tday, true);
				switch (i)
				{
					case 0:
						currentCar.CarSections.Add(CarSectionType.Interior, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true, currentCar));
						CreateElement(ref currentCar.CarSections[CarSectionType.Interior].Groups[0], Vector2.Null, panelSize, new Vector2(0.5, 0.5), 0.0, cabViews[0].Position, tday, null, new Color32(255, 255, 255, 255));
						currentCar.CarSections[CarSectionType.Interior].ViewDirection = new Transformation(cabViews[0].Direction.Y.ToRadians(), -cabViews[0].Direction.X.ToRadians(), -cabViews[0].Direction.Z.ToRadians());
						break;
					case 1:
						currentCar.CarSections.Add(CarSectionType.HeadOutLeft, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true, currentCar));
						CreateElement(ref currentCar.CarSections[CarSectionType.HeadOutLeft].Groups[0], Vector2.Null, panelSize, new Vector2(0.5, 0.5), 0.0, cabViews[1].Position, tday, null, new Color32(255, 255, 255, 255));
						currentCar.CarSections[CarSectionType.HeadOutLeft].ViewDirection = new Transformation(cabViews[1].Direction.Y.ToRadians(), -cabViews[1].Direction.X.ToRadians(), -cabViews[1].Direction.Z.ToRadians());
						break;
					case 2:
						currentCar.CarSections.Add(CarSectionType.HeadOutRight, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true, currentCar));
						CreateElement(ref currentCar.CarSections[CarSectionType.HeadOutRight].Groups[0], Vector2.Null, panelSize, new Vector2(0.5, 0.5), 0.0, cabViews[2].Position, tday, null, new Color32(255, 255, 255, 255));
						currentCar.CarSections[CarSectionType.HeadOutRight].ViewDirection = new Transformation(cabViews[2].Direction.Y.ToRadians(), -cabViews[2].Direction.X.ToRadians(), -cabViews[2].Direction.Z.ToRadians());
						break;
				}
			}
			
			int currentLayer = 1;
			for (int i = 0; i < cabComponents.Count; i++)
			{
				cabComponents[i].Create(ref currentCar, currentLayer);
				currentLayer++; // component layering stacks downwards directly through the cabview
			}

			return true;
		}
		
		private static void ParseBlock(Block block)
		{
			Block newBlock;
			switch (block.Token)
			{
				case KujuTokenID.CabViewControls:
					int count = block.ReadInt16();
					int controlCount = count;
					while (block.Length() - block.Position() > 5)
					{
						newBlock = block.ReadSubBlock(true);
						if (newBlock.Token == KujuTokenID.Skip)
						{
							continue;
						}
						CabComponent currentComponent = new CabComponent(newBlock, cabViews[0].Position); // cab components can only be applied to CabView #0, others are static views
						currentComponent.Parse();
						cabComponents.Add(currentComponent);
						controlCount--;
					}

					if (controlCount != 0)
					{
						// control count was wrong...
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS CVF Parser: Expected " + count + " controls, but found " + (count - controlCount) + " in file " + FileName);
					}

					break;
				case KujuTokenID.Direction:
					currentCabView.Direction.X = block.ReadSingle();
					currentCabView.Direction.Y = block.ReadSingle();
					currentCabView.Direction.Z = block.ReadSingle();
					break;
				case KujuTokenID.Position:
					currentCabView.Position.X = block.ReadSingle();
					currentCabView.Position.Y = block.ReadSingle();
					currentCabView.Position.Z = block.ReadSingle();
					break;
				case KujuTokenID.CabViewWindow:
					currentCabView.TopLeft.X = block.ReadInt16();
					currentCabView.TopLeft.Y = block.ReadInt16();
					currentCabView.PanelSize.X = block.ReadInt16();
					currentCabView.PanelSize.Y = block.ReadInt16();
					break;
				case KujuTokenID.CabViewFile:
				case KujuTokenID.CabViewWindowFile:
					if (string.IsNullOrEmpty(currentCabView.FileName))
					{
						currentCabView.SetCabView(CurrentFolder, block.ReadString());
					}

					break;
				case KujuTokenID.CabViewType:
				case KujuTokenID.EngineData:
					block.Skip((int) block.Length());
					break;
				case KujuTokenID.Tr_CabViewFile:
					currentCabView = new CabView();
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewType);
					ParseBlock(newBlock);
					//The main front cabview
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewFile);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewWindow);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewWindowFile); //Appears to be a duplicate of CabViewFile, some are empty or v/v
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.Position); //Position within loco X,Y,Z
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.Direction); // ?? CAMERA DIRECTION ==> ROT Y, ROT X, ROT Z
					ParseBlock(newBlock);
					cabViews.Add(currentCabView);
					currentCabView = new CabView();
					//View #2, normally L
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewFile);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewWindow);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewWindowFile); //Appears to be a duplicate of CabViewFile, some are empty or v/v
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.Position); //Position within loco X,Y,Z
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.Direction); // ?? CAMERA DIRECTION ==> ROT Y, ROT X, ROT Z
					ParseBlock(newBlock);
					cabViews.Add(currentCabView);
					currentCabView = new CabView();
					//View #3, normally R
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewFile);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewWindow);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewWindowFile); //Appears to be a duplicate of CabViewFile, some are empty or v/v
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.Position); //Position within loco X,Y,Z
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.Direction); // ?? CAMERA DIRECTION ==> ROT Y, ROT X, ROT Z
					ParseBlock(newBlock);
					cabViews.Add(currentCabView);
					newBlock = block.ReadSubBlock(KujuTokenID.EngineData);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewControls);
					ParseBlock(newBlock);
					break;
			}
		}

		private const double panelResolution = 1024.0;
		private static readonly Vector2 panelSize = new Vector2(1024, 768);
		private static readonly Vector2 panelCenter = new Vector2(0, 240);
		private static readonly Vector2 panelOrigin = new Vector2(0, 240);
		
		// get stack language from subject
		internal static string GetStackLanguageFromSubject(TrainBase train, PanelSubject subject, Units subjectUnits)
		{
			// transform subject
			string Code = string.Empty;
			switch (subject)
			{
				case PanelSubject.Load_Meter:
				case PanelSubject.Ammeter:
					Code = "amps";
					break;
				case PanelSubject.Ammeter_Abs:
					Code = "amps abs";
					break;
				case PanelSubject.Brake_Cyl:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "brakecylinder 0.000145038 *";
							break;
						case Units.Inches_Of_Mercury:
							Code = "brakecylinder 0.0002953 *";
							break;
						case Units.Kilopascals:
							Code = "brakecylinder 0.001 *";
							break;
						case Units.Bar:
							Code = "brakecylinder 0.00001 *";
							break;
						case Units.Kgs_Per_Square_Cm:
							Code = "brakecylinder 98066.5 *";
							break;
						default:
							throw new Exception(subjectUnits + " is not a valid unit for " + subject);
					}
					break;
				case PanelSubject.Brake_Pipe:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "brakepipe 0.000145038 *";
							break;
						case Units.Inches_Of_Mercury:
							Code = "brakepipe 0.0002953 *";
							break;
						case Units.Kilopascals:
							Code = "brakepipe 0.001 *";
							break;
						case Units.Bar:
							Code = "brakepipe 0.00001 *";
							break;
						case Units.Kgs_Per_Square_Cm:
							Code = "brakepipe 98066.5 *";
							break;
						default:
							throw new Exception(subjectUnits + " is not a valid unit for " + subject);
					}
					break;
				case PanelSubject.Main_Res:
				case PanelSubject.Vacuum_Reservoir_Pressure:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "mainreservoir 0.000145038 *";
							break;
						case Units.Inches_Of_Mercury:
							Code = "mainreservoir 0.0002953 *";
							break;
						case Units.Kilopascals:
							Code = "mainreservoir 0.001 *";
							break;
						case Units.Bar:
							Code = "mainreservoir 0.00001 *";
							break;
						case Units.Kgs_Per_Square_Cm:
							Code = "mainreservoir 98066.5 *";
							break;
						default:
							throw new Exception(subjectUnits + " is not a valid unit for " + subject);
					}
					break;
				case PanelSubject.Eq_Res:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "equalizingreservoir 0.000145038 *";
							break;
						case Units.Inches_Of_Mercury:
							Code = "equalizingreservoir 0.0002953 *";
							break;
						case Units.Kilopascals:
							Code = "equalizingreservoir 0.001 *";
							break;
						case Units.Bar:
							Code = "equalizingreservoir 0.00001 *";
							break;
						case Units.Kgs_Per_Square_Cm:
							Code = "equalizingreservoir 98066.5 *";
							break;
						default:
							throw new Exception(subjectUnits + " is not a valid unit for " + subject);
					}
					break;
				case PanelSubject.Direction:
					Code = "reverserNotch ++";
					break;
				case PanelSubject.Engine_Brake:
					Code = "locoBrakeNotch";
					break;
				case PanelSubject.Front_Hlight:
					Code = "headlights";
					break;
				case PanelSubject.Bell:
					Code = "musichorn";
					break;
				case PanelSubject.Whistle:
				case PanelSubject.Horn:
					Code = "horn";
					break;
				case PanelSubject.Speedometer:
					// use speed not speedometer at the minute as wheelslip isn't right
					switch (subjectUnits)
					{
						case Units.Miles_Per_Hour:
							Code = "speed abs 2.2369362920544 *";
							break;
						case Units.Kilometers_Per_Hour:
							Code = "speed abs 3.6 *";
							break;
						default:
							throw new Exception(subjectUnits + " is not a valid unit for " + subject);
					}
					break;
				case PanelSubject.Throttle:
					Code = "brakeNotchLinear 0 powerNotch ?";
					break;
				case PanelSubject.Train_Brake:
					Code = "brakeNotchLinear";
					break;
				case PanelSubject.Wipers:
					Code = "wiperstate";
					break;
				case PanelSubject.Panto_Display:
				case PanelSubject.Pantograph:
					Code = "pantographstate";
					break;
				case PanelSubject.Speedlim_Display:
					switch (subjectUnits)
					{
						case Units.Miles_Per_Hour:
							Code = "routelimit sectionlimit max 1 Minus == 1 Minus routelimit sectionlimit max 2.2369362920544 * ?";
							break;
						case Units.Kilometers_Per_Hour:
							Code = "routelimit sectionlimit max 1 Minus == 1 Minus routelimit sectionlimit max 3.6 * ?";
							break;
						default:
							throw new Exception(subjectUnits + " is not a valid unit for " + subject);
					}
					break;
				case PanelSubject.Emergency_Brake:
					Code = "emergencybrake";
					break;
				default:
					Code = "0";
					break;
			}
			return Code;
		}

		internal static int CreateElement(ref ElementsGroup Group, Vector2 TopLeft, Vector2 Size, Vector2 RelativeRotationCenter, double Distance, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement = false)
		{
			if (Size.X == 0 || Size.Y == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "MSTS Cabview Parser: Attempted to create an invalid size element");
			}

			double worldWidth, worldHeight;
			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
			{
				worldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * eyeDistance;
				worldHeight = worldWidth / Plugin.Renderer.Screen.AspectRatio;
			}
			else
			{
				worldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * eyeDistance / Plugin.Renderer.Screen.AspectRatio;
				worldWidth = worldHeight * Plugin.Renderer.Screen.AspectRatio;
			}

			double x0 = TopLeft.X / panelResolution;
			double x1 = (TopLeft.X + Size.X) / panelResolution;
			double y0 = (panelSize.Y - TopLeft.Y) / panelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (panelSize.Y - (TopLeft.Y + Size.Y)) / panelResolution * Plugin.Renderer.Screen.AspectRatio;
			double xd = 0.5 - panelCenter.X / panelResolution;
			x0 += xd;
			x1 += xd;
			double yt = panelSize.Y - panelResolution / Plugin.Renderer.Screen.AspectRatio;
			double yd = (panelCenter.Y - yt) / (panelSize.Y - yt) - 0.5;
			y0 += yd;
			y1 += yd;
			x0 = (x0 - 0.5) * worldWidth;
			x1 = (x1 - 0.5) * worldWidth;
			y0 = (y0 - 0.5) * worldHeight;
			y1 = (y1 - 0.5) * worldHeight;
			double xm = x0 * (1.0 - RelativeRotationCenter.X) + x1 * RelativeRotationCenter.X;
			double ym = y0 * (1.0 - RelativeRotationCenter.Y) + y1 * RelativeRotationCenter.Y;
			Vector3[] v = new Vector3[4];
			v[0] = new Vector3(x0 - xm, y1 - ym, 0);
			v[1] = new Vector3(x0 - xm, y0 - ym, 0);
			v[2] = new Vector3(x1 - xm, y0 - ym, 0);
			v[3] = new Vector3(x1 - xm, y1 - ym, 0);
			Vertex t0 = new Vertex(v[0], new Vector2(0.0f, 1.0f));
			Vertex t1 = new Vertex(v[1], new Vector2(0.0f, 0.0f));
			Vertex t2 = new Vertex(v[2], new Vector2(1.0f, 0.0f));
			Vertex t3 = new Vertex(v[3], new Vector2(1.0f, 1.0f));
			StaticObject staticObject = new StaticObject(Plugin.CurrentHost);
			staticObject.Mesh.Vertices = new VertexTemplate[] {t0, t1, t2, t3};
			staticObject.Mesh.Faces = new[] {new MeshFace(new[] {0, 1, 2, 0, 2, 3}, FaceFlags.Triangles)}; //Must create as a single face like this to avoid Z-sort issues with overlapping bits
			staticObject.Mesh.Materials = new MeshMaterial[1];
			staticObject.Mesh.Materials[0].Flags = new MaterialFlags();
			if (DaytimeTexture != null)
			{
				staticObject.Mesh.Materials[0].Flags |= MaterialFlags.TransparentColor;
			}

			staticObject.Mesh.Materials[0].Color = Color;
			staticObject.Mesh.Materials[0].TransparentColor = Color24.Blue;
			staticObject.Mesh.Materials[0].DaytimeTexture = DaytimeTexture;
			staticObject.Mesh.Materials[0].NighttimeTexture = NighttimeTexture;
			staticObject.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = xm + Driver.X;
			o.Y = ym + Driver.Y;
			o.Z = eyeDistance - Distance + Driver.Z;
			// add object
			if (AddStateToLastElement)
			{
				int n = Group.Elements.Length - 1;
				int j = Group.Elements[n].States.Length;
				Array.Resize(ref Group.Elements[n].States, j + 1);
				Group.Elements[n].States[j] = new ObjectState
				{
					Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z),
					Prototype = staticObject
				};
				return n;
			}
			else
			{
				int n = Group.Elements.Length;
				Array.Resize(ref Group.Elements, n + 1);
				Group.Elements[n] = new AnimatedObject(Plugin.CurrentHost);
				Group.Elements[n].States = new[] {new ObjectState()};
				Group.Elements[n].States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
				Group.Elements[n].States[0].Prototype = staticObject;
				Group.Elements[n].CurrentState = 0;
				Group.Elements[n].internalObject = new ObjectState {Prototype = staticObject};
				Plugin.CurrentHost.CreateDynamicObject(ref Group.Elements[n].internalObject);
				return n;
			}
		}
	}
}
