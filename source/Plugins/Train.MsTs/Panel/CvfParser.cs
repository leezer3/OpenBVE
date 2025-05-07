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
	class CabviewFileParser
	{
		// constants
		internal const double StackDistance = 0.000001;

		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double eyeDistance = 1.0;

		internal static string CurrentFolder;

		private static readonly List<CabComponent> cabComponents = new List<CabComponent>();

		// parse panel config
		internal static bool ParseCabViewFile(string fileName, ref CarBase Car)
		{
			CurrentFolder = Path.GetDirectoryName(fileName);
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read);

			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = (buffer[0] == 0xFF && buffer[1] == 0xFE);

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
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognized cabview file header " + headerString + " in " + fileName);
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
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unrecognized subHeader " + subHeader + " in " + fileName);
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

			double x0 = -PanelCenter.X / PanelResolution;
			double x1 = (PanelSize.X - PanelCenter.X) / PanelResolution;
			double y0 = (PanelCenter.Y - PanelSize.Y) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (PanelCenter.Y) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			Car.CameraRestriction.BottomLeft = new Vector3(x0 * worldWidth, y0 * worldHeight, eyeDistance);
			Car.CameraRestriction.TopRight = new Vector3(x1 * worldWidth, y1 * worldHeight, eyeDistance);
			Car.DriverYaw = Math.Atan((PanelCenter.X - PanelOrigin.X) * worldWidth / PanelResolution);
			Car.DriverPitch = Math.Atan((PanelOrigin.Y - PanelCenter.Y) * worldWidth / PanelResolution);

			if(CabViews.Count == 0 || !File.Exists(CabViews[0].FileName))
			{
				return false;
			}
			Car.CameraRestrictionMode = CameraRestrictionMode.On;
			Plugin.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
			Car.Driver = CabViews[0].Position;
			for (int i = 0; i < CabViews.Count; i++)
			{
				
				Plugin.CurrentHost.RegisterTexture(CabViews[i].FileName, new TextureParameters(null, null), out Texture tday, true);
				switch (i)
				{
					case 0:
						Car.CarSections.Add(CarSectionType.Interior, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true, Car));
						CreateElement(ref Car.CarSections[CarSectionType.Interior].Groups[0], Vector2.Null, PanelSize, new Vector2(0.5, 0.5), 0.0, CabViews[0].Position, tday, null, new Color32(255, 255, 255, 255));
						Car.CarSections[CarSectionType.Interior].ViewDirection = new Transformation(CabViews[0].Direction.Y.ToRadians(), -CabViews[0].Direction.X.ToRadians(), -CabViews[0].Direction.Z.ToRadians());
						break;
					case 1:
						Car.CarSections.Add(CarSectionType.HeadOutLeft, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true, Car));
						CreateElement(ref Car.CarSections[CarSectionType.HeadOutLeft].Groups[0], Vector2.Null, PanelSize, new Vector2(0.5, 0.5), 0.0, CabViews[1].Position, tday, null, new Color32(255, 255, 255, 255));
						Car.CarSections[CarSectionType.HeadOutLeft].ViewDirection = new Transformation(CabViews[1].Direction.Y.ToRadians(), -CabViews[1].Direction.X.ToRadians(), -CabViews[1].Direction.Z.ToRadians());
						break;
					case 2:
						Car.CarSections.Add(CarSectionType.HeadOutRight, new CarSection(Plugin.CurrentHost, ObjectType.Overlay, true, Car));
						CreateElement(ref Car.CarSections[CarSectionType.HeadOutRight].Groups[0], Vector2.Null, PanelSize, new Vector2(0.5, 0.5), 0.0, CabViews[2].Position, tday, null, new Color32(255, 255, 255, 255));
						Car.CarSections[CarSectionType.HeadOutRight].ViewDirection = new Transformation(CabViews[2].Direction.Y.ToRadians(), -CabViews[2].Direction.X.ToRadians(), -CabViews[2].Direction.Z.ToRadians());
						break;
				}
			}
			
			int currentLayer = 1;
			for (int i = 0; i < cabComponents.Count; i++)
			{
				cabComponents[i].Create(ref Car, currentLayer);

			}

			return true;
		}
		
		private static CabView currentCabView;

		private static readonly List<CabView> CabViews = new List<CabView>();

		private static void ParseBlock(Block block)
		{
			Block newBlock;
			switch (block.Token)
			{
				case KujuTokenID.CabViewControls:
					int controlCount = block.ReadInt16();
					while (controlCount > 0)
					{
						newBlock = block.ReadSubBlock();
						CabComponent currentComponent = new CabComponent(newBlock, currentCabView.Position);
						currentComponent.Parse();
						cabComponents.Add(currentComponent);
						controlCount--;
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
					CabViews.Add(currentCabView);
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
					CabViews.Add(currentCabView);
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
					CabViews.Add(currentCabView);
					newBlock = block.ReadSubBlock(KujuTokenID.EngineData);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewControls);
					ParseBlock(newBlock);
					break;
			}
		}

		const double PanelResolution = 1024.0;
		private static readonly Vector2 PanelSize = new Vector2(1024, 768);
		private static readonly Vector2 PanelCenter = new Vector2(0, 240);
		private static readonly Vector2 PanelOrigin = new Vector2(0, 240);
		
		// get stack language from subject
		internal static string GetStackLanguageFromSubject(TrainBase Train, PanelSubject subject, Units subjectUnits, string suffix = "")
		{
			// transform subject
			string Code = string.Empty;
			switch (subject)
			{
				case PanelSubject.Ammeter:
					Code = "amps";
					break;
				case PanelSubject.Brake_Cyl:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "brakecylinder 0.000145038 *";
							break;
						case Units.Inches_Of_Mercury:
							Code = "0";
							break;
					}
					break;
				case PanelSubject.Brake_Pipe:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "brakepipe 0.000145038 *";
							break;
						case Units.Inches_Of_Mercury:
							Code = "0";
							break;
					}
					break;
				case PanelSubject.Main_Res:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "mainreservoir 0.000145038 *";
							break;
						case Units.Inches_Of_Mercury:
							Code = "0";
							break;
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
				case PanelSubject.Horn:
					Code = "horn";
					break;
				case PanelSubject.Speedometer:
					switch (subjectUnits)
					{
						case Units.Miles_Per_Hour:
							Code = "speedometer abs 2.2369362920544 *";
							break;
						case Units.Kilometers_Per_Hour:
							Code = "speedometer abs 3.6 *";
							break;
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
					}
					break;
				case PanelSubject.Emergency_Brake:
					Code = "emergencybrake";
					break;
				default:
					Code = "0";
					break;
			}
			return Code + suffix;
		}

		internal static int CreateElement(ref ElementsGroup Group, Vector2 TopLeft, Vector2 Size, Vector2 RelativeRotationCenter, double Distance, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement = false)
		{
			if (Size.X == 0 || Size.Y == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Attempted to create an invalid size element");
			}

			double WorldWidth, WorldHeight;
			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
			{
				WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * eyeDistance;
				WorldHeight = WorldWidth / Plugin.Renderer.Screen.AspectRatio;
			}
			else
			{
				WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * eyeDistance / Plugin.Renderer.Screen.AspectRatio;
				WorldWidth = WorldHeight * Plugin.Renderer.Screen.AspectRatio;
			}

			double x0 = TopLeft.X / PanelResolution;
			double x1 = (TopLeft.X + Size.X) / PanelResolution;
			double y0 = (PanelSize.Y - TopLeft.Y) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (PanelSize.Y - (TopLeft.Y + Size.Y)) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double xd = 0.5 - PanelCenter.X / PanelResolution;
			x0 += xd;
			x1 += xd;
			double yt = PanelSize.Y - PanelResolution / Plugin.Renderer.Screen.AspectRatio;
			double yd = (PanelCenter.Y - yt) / (PanelSize.Y - yt) - 0.5;
			y0 += yd;
			y1 += yd;
			x0 = (x0 - 0.5) * WorldWidth;
			x1 = (x1 - 0.5) * WorldWidth;
			y0 = (y0 - 0.5) * WorldHeight;
			y1 = (y1 - 0.5) * WorldHeight;
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
			StaticObject Object = new StaticObject(Plugin.CurrentHost);
			Object.Mesh.Vertices = new VertexTemplate[] {t0, t1, t2, t3};
			Object.Mesh.Faces = new[] {new MeshFace(new[] {0, 1, 2, 0, 2, 3}, FaceFlags.Triangles)}; //Must create as a single face like this to avoid Z-sort issues with overlapping bits
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = new MaterialFlags();
			if (DaytimeTexture != null)
			{
				Object.Mesh.Materials[0].Flags |= MaterialFlags.TransparentColor;
			}

			Object.Mesh.Materials[0].Color = Color;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = DaytimeTexture;
			Object.Mesh.Materials[0].NighttimeTexture = NighttimeTexture;
			Object.Dynamic = true;
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
					Prototype = Object
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
				Group.Elements[n].States[0].Prototype = Object;
				Group.Elements[n].CurrentState = 0;
				Group.Elements[n].internalObject = new ObjectState {Prototype = Object};
				Plugin.CurrentHost.CreateDynamicObject(ref Group.Elements[n].internalObject);
				return n;
			}
		}
	}
}
