using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LibRender2.Trains;
using OpenBve.Formats.MsTs;
using OpenBveApi.Colors;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using TrainManager.Car;
using TrainManager.Trains;

namespace Train.MsTs
{
	class CabviewFileParser
	{
		// constants
		private const double stackDistance = 0.000001;

		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double eyeDistance = 1.0;

		private static string currentFolder;

		private static readonly List<Component> cabComponents = new List<Component>();

		// parse panel config
		internal static bool ParseCabViewFile(string fileName, ref CarBase Car)
		{
			currentFolder = Path.GetDirectoryName(fileName);
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
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognized cabview file header " + headerString + " in " + fileName);
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
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognized subHeader " + subHeader + " in " + fileName);
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

			double x0 = (PanelLeft - PanelCenter.X) / PanelResolution;
			double x1 = (PanelRight - PanelCenter.X) / PanelResolution;
			double y0 = (PanelCenter.Y - PanelBottom) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (PanelCenter.Y - PanelTop) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			Car.CameraRestriction.BottomLeft = new Vector3(x0 * WorldWidth, y0 * WorldHeight, eyeDistance);
			Car.CameraRestriction.TopRight = new Vector3(x1 * WorldWidth, y1 * WorldHeight, eyeDistance);
			Car.DriverYaw = Math.Atan((PanelCenter.X - PanelOrigin.X) * WorldWidth / PanelResolution);
			Car.DriverPitch = Math.Atan((PanelOrigin.Y - PanelCenter.Y) * WorldWidth / PanelResolution);

			if (File.Exists(CabViews[0].fileName))
			{
				Car.Driver = CabViews[0].position;
				Plugin.currentHost.RegisterTexture(CabViews[0].fileName, new TextureParameters(null, null), out Texture tday, true);
				CreateElement(ref Car.CarSections[0].Groups[0], 0.0, 0.0, 1024, 768, new Vector2(0.5, 0.5), 0.0, Car.Driver, tday, null, new Color32(255, 255, 255, 255));
			}
			else
			{
				//Main panel image doesn't exist
				return false;
			}

			int currentLayer = 1;
			for (int i = 0; i < cabComponents.Count; i++)
			{
				cabComponents[i].Create(ref Car, currentLayer);

			}

			return true;
		}

		internal struct CabView
		{
			internal string fileName;
			internal Vector2 topLeft;
			internal Vector2 panelSize;
			internal Vector3 position;
			internal Vector3 direction;

			internal void setCabView(string cabViewFile)
			{
				cabViewFile = cabViewFile.Replace(@"\\", @"\");
				fileName = OpenBveApi.Path.CombineFile(currentFolder, cabViewFile);
			}
		}

		private static CabView currentCabView;

		private static List<CabView> CabViews = new List<CabView>();

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
						Component currentComponent = new Component(newBlock);
						currentComponent.Parse();
						cabComponents.Add(currentComponent);
						controlCount--;
					}

					break;
				case KujuTokenID.Direction:
					currentCabView.direction.X = block.ReadSingle();
					currentCabView.direction.Y = block.ReadSingle();
					currentCabView.direction.Z = block.ReadSingle();
					break;
				case KujuTokenID.Position:
					currentCabView.position.X = block.ReadSingle();
					currentCabView.position.Y = block.ReadSingle();
					currentCabView.position.Z = block.ReadSingle();
					break;
				case KujuTokenID.CabViewWindow:
					currentCabView.topLeft.X = block.ReadInt16();
					currentCabView.topLeft.Y = block.ReadInt16();
					currentCabView.panelSize.X = block.ReadInt16();
					currentCabView.panelSize.Y = block.ReadInt16();
					break;
				case KujuTokenID.CabViewFile:
				case KujuTokenID.CabViewWindowFile:
					if (string.IsNullOrEmpty(currentCabView.fileName))
					{
						currentCabView.setCabView(block.ReadString());
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
					newBlock = block.ReadSubBlock(KujuTokenID.Direction); // ?? CAMERA DIRECTION ==> ROT X, ROT Y, ROT Z ??
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
					newBlock = block.ReadSubBlock(KujuTokenID.Direction); // ?? CAMERA DIRECTION ==> ROT X, ROT Y, ROT Z ??
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
					newBlock = block.ReadSubBlock(KujuTokenID.Direction); // ?? CAMERA DIRECTION ==> ROT X, ROT Y, ROT Z ??
					ParseBlock(newBlock);
					CabViews.Add(currentCabView);
					newBlock = block.ReadSubBlock(KujuTokenID.EngineData);
					ParseBlock(newBlock);
					newBlock = block.ReadSubBlock(KujuTokenID.CabViewControls);
					ParseBlock(newBlock);
					break;
			}
		}

		static double PanelResolution = 1024.0;
		static double PanelLeft = 0.0, PanelRight = 1024.0;
		static double PanelTop = 0.0, PanelBottom = 768.0;
		static Vector2 PanelCenter = new Vector2(0, 240);
		private static Vector2 PanelOrigin = new Vector2(0, 240);

		private class Component
		{
			private CabComponentType Type = CabComponentType.None;
			private string TexturePath;
			private PanelSubject panelSubject;
			private Units Units;
			private Vector2 Position = new Vector2(0, 0);
			private Vector2 Size = new Vector2(0, 0);
			private double PivotPoint;
			private double InitialAngle;
			private double LastAngle;
			private double Maximum;
			private double Minimum;
			private int TotalFrames;
			private int HorizontalFrames;
			private int VerticalFrames;
			private bool MouseControl;
			private bool DirIncrease;

			internal void Parse()
			{
				if (!Enum.TryParse(myBlock.Token.ToString(), true, out Type))
				{
					Plugin.currentHost.AddMessage(MessageType.Error, false, "Unrecognised CabViewComponent type.");
					return;
				}

				while (myBlock.Position() < myBlock.Length() - 2)
				{
					//Components in CVF files are considerably less structured, so read *any* valid block
					try
					{
						Block newBlock = myBlock.ReadSubBlock();
						ReadSubBlock(newBlock);
					}
					catch
					{
						break;
					}
					
				}
			}

			internal void Create(ref CarBase Car, int Layer)
			{
				if (File.Exists(TexturePath))
				{
					//Create and register texture

					//Create element
					double rW = 1024.0 / 640.0;
					double rH = 768.0 / 480.0;
					int wday, hday;
					int j;
					string f;
					CultureInfo Culture = CultureInfo.InvariantCulture;
					switch (Type)
					{
						case CabComponentType.Dial:
							Plugin.currentHost.RegisterTexture(TexturePath, new TextureParameters(null, null), out Texture tday, true);
							//Get final position from the 640px panel (Yuck...)
							Position.X *= rW;
							Position.Y *= rH;
							Size.X *= rW;
							Size.Y *= rH;
							PivotPoint *= rH;
							j = CreateElement(ref Car.CarSections[0].Groups[0], Position.X, Position.Y, Size.X, Size.Y, new Vector2((0.5 * Size.X) / (tday.Width * rW), PivotPoint / (tday.Height * rH)), Layer * stackDistance, Car.Driver, tday, null, new Color32(255, 255, 255, 255));
							Car.CarSections[0].Groups[0].Elements[j].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
							Car.CarSections[0].Groups[0].Elements[j].RotateXDirection = new Vector3(1.0, 0.0, 0.0);
							Car.CarSections[0].Groups[0].Elements[j].RotateYDirection = Vector3.Cross(Car.CarSections[0].Groups[0].Elements[j].RotateZDirection, Car.CarSections[0].Groups[0].Elements[j].RotateXDirection);
							f = GetStackLanguageFromSubject(Car.baseTrain, panelSubject, Units);
							InitialAngle = InitialAngle.ToRadians();
							LastAngle = LastAngle.ToRadians();
							double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
							double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
							f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
							Car.CarSections[0].Groups[0].Elements[j].RotateZFunction = new FunctionScript(Plugin.currentHost, f, false);
							break;
						case CabComponentType.Lever:
							/*
							 * TODO:
							 * Need to revisit the actual position versus frame with MSTS content.
							 *
							 * Take the example of the stock Class 50
							 * This has a notched brake handle, with 5 physical notches
							 *
							 * The cabview has 12 frames for these 5 notches, which appear to be mapped using NumPositions
							 * Oddly, all frames appear to be distinct. Need to check OR + MSTS handling
							 * Suspect there's a notch delay or something that should use these.
							 */
							Position.X *= rW;
							Position.Y *= rH;
							Plugin.currentHost.QueryTextureDimensions(TexturePath, out wday, out hday);
							if (wday > 0 & hday > 0)
							{
								Texture[] textures = new Texture[TotalFrames];
								int row = 0;
								int column = 0;
								int frameWidth = wday / HorizontalFrames;
								int frameHeight = hday / VerticalFrames;
								for (int k = 0; k < TotalFrames; k++)
								{
									Plugin.currentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(column * frameWidth, row * frameHeight, frameWidth, frameHeight), null), out textures[k]);
									if (column < HorizontalFrames - 1)
									{
										column++;
									}
									else
									{
										column = 0;
										row++;
									}
								}

								j = -1;
								for (int k = 0; k < textures.Length; k++)
								{
									int l = CreateElement(ref Car.CarSections[0].Groups[0], Position.X, Position.Y, Size.X * rW, Size.Y * rH, new Vector2(0.5, 0.5), Layer * stackDistance, Car.Driver, textures[k], null, new Color32(255, 255, 255, 255), k != 0);
									if (k == 0) j = l;
								}

								f = GetStackLanguageFromSubject(Car.baseTrain, panelSubject, Units);
								Car.CarSections[0].Groups[0].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f, false);
							}

							break;
						case CabComponentType.TriState:
						case CabComponentType.TwoState:
							Position.X *= rW;
							Position.Y *= rH;
							Plugin.currentHost.QueryTextureDimensions(TexturePath, out wday, out hday);
							if (wday > 0 & hday > 0)
							{
								Texture[] textures = new Texture[TotalFrames];
								int row = 0;
								int column = 0;
								int frameWidth = wday / HorizontalFrames;
								int frameHeight = hday / VerticalFrames;
								for (int k = 0; k < TotalFrames; k++)
								{
									Plugin.currentHost.RegisterTexture(TexturePath, new TextureParameters(new TextureClipRegion(column * frameWidth, row * frameHeight, frameWidth, frameHeight), null), out textures[k]);
									if (column < HorizontalFrames - 1)
									{
										column++;
									}
									else
									{
										column = 0;
										row++;
									}
								}

								j = -1;
								for (int k = 0; k < textures.Length; k++)
								{
									int l = CreateElement(ref Car.CarSections[0].Groups[0], Position.X, Position.Y, Size.X * rW, Size.Y * rH, new Vector2(0.5, 0.5), Layer * stackDistance, Car.Driver, textures[k], null, new Color32(255, 255, 255, 255), k != 0);
									if (k == 0) j = l;
								}

								f = GetStackLanguageFromSubject(Car.baseTrain, panelSubject, Units);
								Car.CarSections[0].Groups[0].Elements[j].StateFunction = new FunctionScript(Plugin.currentHost, f, false);
							}

							break;
					}
				}
			}

			private void ReadSubBlock(Block block)
			{
				switch (block.Token)
				{
					case KujuTokenID.Pivot:
						PivotPoint = block.ReadSingle();
						break;
					case KujuTokenID.Units:
						Units = block.ReadEnumValue(default(Units));
						break;
					case KujuTokenID.ScalePos:
						InitialAngle = block.ReadSingle();
						LastAngle = block.ReadSingle();
						break;
					case KujuTokenID.ScaleRange:
						Minimum = block.ReadSingle();
						Maximum = block.ReadSingle();
						break;
					case KujuTokenID.States:
						//Contains sub-blocks with Style and SwitchVal types
						//Doesn't appear to have any frame data
						//Examining image appears to show 1-frame V strip
						block.Skip((int) block.Length());
						break;
					case KujuTokenID.DirIncrease:
						//Do we start at 0 or max?
						DirIncrease = block.ReadInt16() == 1;
						break;
					case KujuTokenID.Orientation:
						//Flip?
						block.Skip((int) block.Length());
						break;
					case KujuTokenID.NumValues:
					case KujuTokenID.NumPositions:
						//notch ==> frame data
						//We can skip for basic cabs
						block.Skip((int) block.Length());
						break;
					case KujuTokenID.NumFrames:
						TotalFrames = block.ReadInt16();
						HorizontalFrames = block.ReadInt16();
						VerticalFrames = block.ReadInt16();
						break;
					case KujuTokenID.MouseControl:
						MouseControl = block.ReadInt16() == 1;
						break;
					case KujuTokenID.Style:
						block.Skip((int) block.Length());
						break;
					case KujuTokenID.Graphic:
						string s = block.ReadString();
						TexturePath = OpenBveApi.Path.CombineFile(currentFolder, s);
						break;
					case KujuTokenID.Position:
						Position.X = block.ReadSingle();
						Position.Y = block.ReadSingle();
						Size.X = block.ReadSingle();
						Size.Y = block.ReadSingle();
						break;
					case KujuTokenID.Type:
						panelSubject = block.ReadEnumValue(default(PanelSubject));
						

						break;
				}
			}

			internal Component(Block block)
			{
				myBlock = block;
			}

			private readonly Block myBlock;
		}
		
		// get stack language from subject
		private static string GetStackLanguageFromSubject(TrainBase Train, PanelSubject subject, Units subjectUnits)
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
					}
					break;
				case PanelSubject.Brake_Pipe:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "brakecylinder 0.000145038 *";
							break;
					}
					break;
				case PanelSubject.Main_Res:
					switch (subjectUnits)
					{
						case Units.PSI:
							Code = "mainreservoir 0.000145038 *";
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
				default:
					Code = "0";
					break;
			}
			return Code;
		}

		internal static int CreateElement(ref ElementsGroup Group, double Left, double Top, double Width, double Height, Vector2 RelativeRotationCenter, double Distance, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement = false)
		{
			if (Width == 0 || Height == 0)
			{
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Attempted to create an invalid size element");
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

			double x0 = Left / PanelResolution;
			double x1 = (Left + Width) / PanelResolution;
			double y0 = (PanelBottom - Top) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (PanelBottom - (Top + Height)) / PanelResolution * Plugin.Renderer.Screen.AspectRatio;
			double xd = 0.5 - PanelCenter.X / PanelResolution;
			x0 += xd;
			x1 += xd;
			double yt = PanelBottom - PanelResolution / Plugin.Renderer.Screen.AspectRatio;
			double yd = (PanelCenter.Y - yt) / (PanelBottom - yt) - 0.5;
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
			StaticObject Object = new StaticObject(Plugin.currentHost);
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
				Group.Elements[n] = new AnimatedObject(Plugin.currentHost);
				Group.Elements[n].States = new[] {new ObjectState()};
				Group.Elements[n].States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
				Group.Elements[n].States[0].Prototype = Object;
				Group.Elements[n].CurrentState = 0;
				Group.Elements[n].internalObject = new ObjectState {Prototype = Object};
				Plugin.currentHost.CreateDynamicObject(ref Group.Elements[n].internalObject);
				return n;
			}
		}
	}
}
