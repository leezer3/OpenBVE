using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBve.Formats.MsTs;
using SharpCompress.Compressor.Deflate;

namespace OpenBve
{
	class MsTsCabviewFileParser
	{
		// constants
		private const double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		private const double EyeDistance = 1.0;

		private static string currentFolder;

		private static List<Component> cabComponents = new List<Component>();

		// parse panel config
		internal static bool ParseCabViewFile(string fileName, System.Text.Encoding Encoding, TrainManager.Train Train)
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
				fb = new ZlibStream(fb, SharpCompress.Compressor.CompressionMode.Decompress);
			}
			else if (headerString.StartsWith("\r\nSIMISA"))
			{
				// ie us1rd2l1000r10d.s, we are going to allow this but warn
				Console.Error.WriteLine("Improper header in " + fileName);
				fb.Read(buffer, 0, 4);
			}
			else if (!headerString.StartsWith("SIMISA@@"))
			{
				throw new Exception("Unrecognized cabview file header " + headerString + " in " + fileName);
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
					byte[] newBytes = reader.ReadBytes((int)(fb.Length - fb.Position));
					string s;
					if (unicode)
					{
						s = Encoding.Unicode.GetString(newBytes);
					}
					else
					{
						s = Encoding.ASCII.GetString(newBytes);
					}

					s = s.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
					if (!s.StartsWith("Tr_CabViewFile", StringComparison.InvariantCultureIgnoreCase))
					{
						throw new Exception();
					}
					TextualBlock block = new TextualBlock(s, KujuTokenID.Tr_CabViewFile);
					ParseBlock(block);
				}
					
			}
			else if (subHeader[7] != 'b')
			{
				throw new Exception("Unrecognized subHeader \"" + subHeader + "\" in " + fileName);
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
			if (Screen.Width >= Screen.Height)
			{
				WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / World.AspectRatio;
			}
			else
			{
				WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance / World.AspectRatio;
				WorldWidth = WorldHeight * World.AspectRatio;
			}

			double x0 = (PanelLeft - PanelCenterX) / PanelResolution;
			double x1 = (PanelRight - PanelCenterX) / PanelResolution;
			double y0 = (PanelCenterY - PanelBottom) / PanelResolution * World.AspectRatio;
			double y1 = (PanelCenterY - PanelTop) / PanelResolution * World.AspectRatio;
			World.CameraRestrictionBottomLeft = new Vector3(x0 * WorldWidth, y0 * WorldHeight, EyeDistance);
			World.CameraRestrictionTopRight = new Vector3(x1 * WorldWidth, y1 * WorldHeight, EyeDistance);
			Train.Cars[Train.DriverCar].DriverYaw = Math.Atan((PanelCenterX - PanelOriginX) * WorldWidth / PanelResolution);
			Train.Cars[Train.DriverCar].DriverPitch = Math.Atan((PanelOriginY - PanelCenterY) * WorldWidth / PanelResolution);
			
			if (System.IO.File.Exists(CabViews[0].fileName))
			{
				Textures.Texture tday;
				Textures.RegisterTexture(CabViews[0].fileName, new OpenBveApi.Textures.TextureParameters(null, null), out tday);
				OpenBVEGame.RunInRenderThread(() =>
				{
					Textures.LoadTexture(tday, Textures.OpenGlTextureWrapMode.ClampClamp);
				});
				PanelBitmapWidth = (double) tday.Width;
				PanelBitmapHeight = (double) tday.Height;
				CreateElement(Train, 0.0, 0.0, PanelBitmapWidth, PanelBitmapHeight, 0.5, 0.5, 0.0, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, Train.Cars[Train.DriverCar].Driver, tday, null, new Color32(255, 255, 255, 255), false);
			}
			else
			{
				//Main panel image doesn't exist
				return false;
			}
			int Layer = 1;
			for (int i = 0; i < cabComponents.Count; i++)
			{
				cabComponents[i].Create(ref Train, Layer, fileName);

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
				fileName = OpenBveApi.Path.CombineFile(currentFolder,cabViewFile);
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
						newBlock = block.ReadSubBlock(new KujuTokenID[] { KujuTokenID.CabSignalDisplay, KujuTokenID.Dial , KujuTokenID.Lever , KujuTokenID.MultiStateDisplay , KujuTokenID.TriState , KujuTokenID.TwoState });
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
					block.Skip((int)block.Length());
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
		static double PanelCenterX = 0.0, PanelCenterY = 240.0;
		static double PanelOriginX = 0.0, PanelOriginY = 240.0;
		static double PanelBitmapWidth = 640.0, PanelBitmapHeight = 480.0;



		private class Component
		{
			private ComponentType Type = ComponentType.None;
			private string TexturePath;
			private Subject Subject;
			private string Units;
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

			internal void Parse()
			{
				Block newBlock;
				if (!Enum.TryParse(myBlock.Token.ToString(), true, out Type))
				{
					Interface.AddMessage(Interface.MessageType.Error, false, "Unrecognised CabViewComponent type.");
				}

				while (myBlock.Position() < myBlock.Length() - 2)
				{
					//Components in CVF files are considerably less structured, so read *any* valid block
					newBlock = myBlock.ReadSubBlock();
					ReadSubBlock(newBlock);
				}
			}

			internal void Create(ref TrainManager.Train Train, int Layer, string fileName)
			{
				if (File.Exists(TexturePath) && Units != null)
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
						case ComponentType.Dial:
							Textures.Texture tday;
							Textures.RegisterTexture(TexturePath, new OpenBveApi.Textures.TextureParameters(null, null), out tday);
							OpenBVEGame.RunInRenderThread(() =>
							{
								Textures.LoadTexture(tday, Textures.OpenGlTextureWrapMode.ClampClamp);
							});
							//Get final position from the 640px panel (Yuck...)
							Position.X *= rW;
							Position.Y *= rH;
							Size.X *= rW;
							Size.Y *= rH;
							PivotPoint *= rH;
							double w = (double)tday.Width;
							double h = (double)tday.Height;
							j = CreateElement(Train, Position.X, Position.Y, Size.X, Size.Y, (0.5 * Size.X) / (w * rW), PivotPoint / (h * rH), (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, Train.Cars[Train.DriverCar].Driver, tday, null, new Color32(255, 255, 255), false);
							Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection = new Vector3(0.0, 0.0, -1.0);
							Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection = new Vector3(1.0, 0.0, 0.0);
							Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateYDirection = Vector3.Cross(Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZDirection, Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateXDirection);
							f = GetStackLanguageFromSubject(Train, Units, "Dial " + " in " + fileName);
							InitialAngle -= 360;
							InitialAngle *= 0.0174532925199433; //degrees to radians
							LastAngle *= 0.0174532925199433;
							double a0 = (InitialAngle * Maximum - LastAngle * Minimum) / (Maximum - Minimum);
							double a1 = (LastAngle - InitialAngle) / (Maximum - Minimum);
							f += " " + a1.ToString(Culture) + " * " + a0.ToString(Culture) + " +";
							Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
							//MSTS cab dials are backstopped as standard
							Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction.Minimum = InitialAngle;
							Train.Cars[Train.DriverCar].CarSections[0].Elements[j].RotateZFunction.Maximum = LastAngle;
							break;
						case ComponentType.Lever:
							Position.X *= rW;
							Position.Y *= rH;

							Program.CurrentHost.QueryTextureDimensions(TexturePath, out wday, out hday);
							if (wday > 0 & hday > 0)
							{
								Textures.Texture[] textures = new Textures.Texture[TotalFrames];
								int row = 0;
								int column = 0;
								int frameWidth = wday / HorizontalFrames;
								int frameHeight = hday / VerticalFrames;
								for (int k = 0; k < TotalFrames; k++)
								{
									Textures.RegisterTexture(TexturePath, new OpenBveApi.Textures.TextureParameters(new OpenBveApi.Textures.TextureClipRegion(column * frameWidth, row * frameHeight, frameWidth, frameHeight), null), out textures[k]);
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
									int l = CreateElement(Train, Position.X, Position.Y, Size.X * rW, Size.Y * rH, 0.5, 0.5, (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, Train.Cars[Train.DriverCar].Driver, textures[k], null, new Color32(255, 255, 255, 255), k != 0);
									if (k == 0) j = l;
								}
								f = GetStackLanguageFromSubject(Train, Units, "Lever " + " in " + fileName);
								Train.Cars[Train.DriverCar].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
							}
							break;
						case ComponentType.TriState:
						case ComponentType.TwoState:
							Position.X *= rW;
							Position.Y *= rH;
							Program.CurrentHost.QueryTextureDimensions(TexturePath, out wday, out hday);
							if (wday > 0 & hday > 0)
							{
								Textures.Texture[] textures = new Textures.Texture[TotalFrames];
								int row = 0;
								int column = 0;
								int frameWidth = wday / HorizontalFrames;
								int frameHeight = hday / VerticalFrames;
								for (int k = 0; k < TotalFrames; k++)
								{
									Textures.RegisterTexture(TexturePath, new OpenBveApi.Textures.TextureParameters(new OpenBveApi.Textures.TextureClipRegion(column * frameWidth, row * frameHeight, frameWidth, frameHeight), null), out textures[k]);
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
									int l = CreateElement(Train, Position.X, Position.Y, Size.X * rW, Size.Y * rH, 0.5, 0.5, (double)Layer * StackDistance, PanelResolution, PanelLeft, PanelRight, PanelTop, PanelBottom, PanelBitmapWidth, PanelBitmapHeight, PanelCenterX, PanelCenterY, PanelOriginX, PanelOriginY, Train.Cars[Train.DriverCar].Driver, textures[k], null, new Color32(255, 255, 255, 255), k != 0);
									if (k == 0) j = l;
								}
								f = Type == ComponentType.TwoState ? GetStackLanguageFromSubject(Train, Units, "TwoState " + " in " + fileName) : GetStackLanguageFromSubject(Train, Units, "TriState " + " in " + fileName);
								
								Train.Cars[Train.DriverCar].CarSections[0].Elements[j].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(f);
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
						string u = block.ReadString();
						switch (u.ToLowerInvariant())
						{
							case "amps":
								Units = "motor";
								break;
							case "miles_per_hour":
								Units = "mph";
								break;
							case "inches_of_mercury":
							case "psi":
								//We don't simulate vaccum brakes, so just hook to PSI if vaccum is declared
								switch (Subject)
								{
									case Subject.BrakeCylinder:
										Units = "bc_psi";
										break;
									case Subject.BrakePipe:
										Units = "bp_psi";
										break;
								}
								break;
						}
						break;
					case KujuTokenID.ScalePos:
						InitialAngle = block.ReadSingle();
						LastAngle = block.ReadSingle();
						break;
					case KujuTokenID.ScaleRange:
						if (Subject == Subject.Ammeter)
						{
							//As we're currently using the BVE ammeter hack, ignore the values
							Minimum = 0;
							Maximum = 1;
							block.Skip((int)block.Length());
						}

						Minimum = block.ReadSingle();
						Maximum = block.ReadSingle();
						break;
					case KujuTokenID.States:
						//Contains sub-blocks with Style and SwitchVal types
						//Doesn't appear to have any frame data
						//Examining image appears to show 1-frame V strip
						block.Skip((int)block.Length());
						break;
					case KujuTokenID.DirIncrease:
						//Do we start at 0 or max?
						block.Skip((int)block.Length());
						break;
					case KujuTokenID.Orientation:
						//Flip?
						block.Skip((int)block.Length());
						break;
					case KujuTokenID.NumValues:
					case KujuTokenID.NumPositions:
						//notch ==> frame data
						//We can skip for basic cabs
						block.Skip((int)block.Length());
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
						block.Skip((int)block.Length());
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
						string t = block.ReadString();
						while (block.Position() < block.Length() -2)
						{
							//Special case: Concated strings
							//#2 appears to be the type repeated
							//DO NOT RELY ON THIS THOUGH....
							t += @" " + block.ReadString();
						}
						switch (t.ToLowerInvariant())
						{
							case "speedometer dial":
								Subject = Subject.Speedometer;
								break;
							case "brake_pipe dial":
								Subject = Subject.BrakePipe;
								break;
							case "brake_cyl dial":
								Subject = Subject.BrakeCylinder;
								break;
							case "ammeter dial":
								Subject = Subject.Ammeter;
								break;
							case "aspect_display cab_signal_display":
								Subject = Subject.AWS;
								break;
							case "horn two_state":
								Subject = Subject.Horn;
								Units = "klaxon";
								break;
							case "direction tri_state":
								Subject = Subject.Direction;
								Units = "rev";
								break;
							case "throttle lever":
								Subject = Subject.PowerHandle;
								Units = "power";
								break;
							case "engine_brake lever":
								Subject = Subject.EngineBrakeHandle;
								Units = "brake";
								break;
							case "train_brake lever":
								Subject = Subject.TrainBrakeHandle;
								Units = "brake";
								break;
						}
						break;
				}
			}

			internal Component(Block block)
			{
				myBlock = block;
			}

			private readonly Block myBlock;
		}

		private enum ComponentType
		{
			/// <summary>None</summary>
			None = 0,
			/// <summary>Dial based control</summary>
			Dial = 1,
			/// <summary>Lever based control</summary>
			Lever = 2,
			/// <summary>Two-state based control</summary>
			TwoState = 3,
			/// <summary>Tri-state based control</summary>
			TriState = 4,
			/// <summary>A display capable of displaying N states</summary>
			MultiStateDisplay = 5,
			/// <summary>In cab signalling / safety system (e.g. AWS)</summary>
			CabSignalDisplay = 6
			

		}

		private enum Subject
		{
			Speedometer = 0,
			BrakePipe = 1,
			BrakeCylinder = 2,
			Ammeter = 3,
			Direction = 4,
			PowerHandle = 5,
			EngineBrakeHandle = 6,
			TrainBrakeHandle = 7,
			Horn = 8,
			AWS = 9
		}


		// get stack language from subject
		private static string GetStackLanguageFromSubject(TrainManager.Train Train, string Subject, string ErrorLocation)
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Suffix = "";
			{
				// detect d# suffix
				int i;
				for (i = Subject.Length - 1; i >= 0; i--)
				{
					int a = char.ConvertToUtf32(Subject, i);
					if (a < 48 | a > 57) break;
				}
				if (i >= 0 & i < Subject.Length - 1)
				{
					if (Subject[i] == 'd' | Subject[i] == 'D')
					{
						int n;
						if (int.TryParse(Subject.Substring(i + 1), System.Globalization.NumberStyles.Integer, Culture, out n))
						{
							if (n == 0)
							{
								Suffix = " floor 10 mod";
							}
							else
							{
								string t0 = Math.Pow(10.0, (double)n).ToString(Culture);
								string t1 = Math.Pow(10.0, (double)-n).ToString(Culture);
								Suffix = " ~ " + t0 + " >= <> " + t1 + " * floor 10 mod 10 ?";
							}
							Subject = Subject.Substring(0, i);
							i--;
						}
					}
				}
			}
			// transform subject
			string Code;
			switch (Subject.ToLowerInvariant())
			{
				case "acc":
					Code = "acceleration";
					break;
				case "motor":
					Code = "accelerationmotor";
					break;
				case "true":
					Code = "1";
					break;
				case "kmph":
					Code = "speedometer abs 3.6 *";
					break;
				case "mph":
					Code = "speedometer abs 2.2369362920544 *";
					break;
				case "ms":
					Code = "speedometer abs";
					break;
				case "bc":
					Code = "brakecylinder 0.001 *";
					break;
				case "bc_psi":
					Code = "brakecylinder 0.000145038 *";
					break;
				case "mr":
					Code = "mainreservoir 0.001 *";
					break;
				case "sap":
					Code = "straightairpipe 0.001 *";
					break;
				case "bp":
					Code = "brakepipe 0.001 *";
					break;
				case "bp_psi":
					Code = "brakepipe 0.000145038 *";
					break;
				case "er":
					Code = "equalizingreservoir 0.001 *";
					break;
				case "door":
					Code = "1 doors -";
					break;
				case "csc":
					Code = "constSpeed";
					break;
				case "power":
					Code = "brakeNotchLinear 0 powerNotch ?";
					break;
				case "brake":
					Code = "brakeNotchLinear";
					break;
				case "rev":
					Code = "reverserNotch ++";
					break;
				case "hour":
					Code = "0.000277777777777778 time * 24 mod floor";
					break;
				case "min":
					Code = "0.0166666666666667 time * 60 mod floor";
					break;
				case "sec":
					Code = "time 60 mod floor";
					break;
				case "atc":
					Code = "271 pluginstate";
					break;
				case "klaxon":
					Code = "klaxon";
					break;
				default:
				{
					Code = "0";
					bool unsupported = true;
					if (Subject.StartsWith("ats", StringComparison.OrdinalIgnoreCase))
					{
						string a = Subject.Substring(3);
						int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n))
						{
							if (n >= 0 & n <= 255)
							{
								Code = n.ToString(Culture) + " pluginstate";
								unsupported = false;
							}
						}
					}
					else if (Subject.StartsWith("doorl", StringComparison.OrdinalIgnoreCase))
					{
						string a = Subject.Substring(5);
						int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n))
						{
							if (n >= 0 & n < Train.Cars.Length)
							{
								Code = n.ToString(Culture) + " leftdoorsindex ceiling";
								unsupported = false;
							}
							else
							{
								Code = "2";
								unsupported = false;
							}
						}
					}
					else if (Subject.StartsWith("doorr", StringComparison.OrdinalIgnoreCase))
					{
						string a = Subject.Substring(5);
						int n; if (int.TryParse(a, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out n))
						{
							if (n >= 0 & n < Train.Cars.Length)
							{
								Code = n.ToString(Culture) + " rightdoorsindex ceiling";
								unsupported = false;
							}
							else
							{
								Code = "2";
								unsupported = false;
							}
						}
					}
					if (unsupported)
					{
						Interface.AddMessage(Interface.MessageType.Error, false, "Invalid subject " + Subject + " encountered in " + ErrorLocation);
					}
				}
					break;
			}
			return Code + Suffix;
		}

		/// <summary>Creates a panel element</summary>
		/// <param name="Train">The train to add this panel element to</param>
		/// <param name="Left">The top-left X co-ordinate</param>
		/// <param name="Top">The top-left Y co-ordinate</param>
		/// <param name="Width">The element width</param>
		/// <param name="Height">The element height</param>
		/// <param name="RelativeRotationCenterX">The relative center of rotation (X-axis)</param>
		/// <param name="RelativeRotationCenterY">The relative center of rotation (Y-axis)</param>
		/// <param name="Distance"></param>
		/// <param name="PanelResolution"></param>
		/// <param name="PanelLeft"></param>
		/// <param name="PanelRight"></param>
		/// <param name="PanelTop"></param>
		/// <param name="PanelBottom"></param>
		/// <param name="PanelBitmapWidth"></param>
		/// <param name="PanelBitmapHeight"></param>
		/// <param name="PanelCenterX"></param>
		/// <param name="PanelCenterY"></param>
		/// <param name="PanelOriginX"></param>
		/// <param name="PanelOriginY"></param>
		/// <param name="Driver"></param>
		/// <param name="DaytimeTexture"></param>
		/// <param name="NighttimeTexture"></param>
		/// <param name="Color"></param>
		/// <param name="AddStateToLastElement"></param>
		/// <returns></returns>
		private static int CreateElement(TrainManager.Train Train, double Left, double Top, double Width, double Height, double RelativeRotationCenterX, double RelativeRotationCenterY, double Distance, double PanelResolution, double PanelLeft, double PanelRight, double PanelTop, double PanelBottom, double PanelBitmapWidth, double PanelBitmapHeight, double PanelCenterX, double PanelCenterY, double PanelOriginX, double PanelOriginY, Vector3 Driver, Textures.Texture DaytimeTexture, Textures.Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement)
		{
			double WorldWidth, WorldHeight;
			if (Screen.Width >= Screen.Height)
			{
				WorldWidth = 2.0 * Math.Tan(0.5 * World.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / World.AspectRatio;
			}
			else
			{
				WorldHeight = 2.0 * Math.Tan(0.5 * World.VerticalViewingAngle) * EyeDistance / World.AspectRatio;
				WorldWidth = WorldHeight * World.AspectRatio;
			}
			double x0 = Left / PanelResolution;
			double x1 = (Left + Width) / PanelResolution;
			double y0 = (PanelBottom - Top) / PanelResolution * World.AspectRatio;
			double y1 = (PanelBottom - (Top + Height)) / PanelResolution * World.AspectRatio;
			double xd = 0.5 - PanelCenterX / PanelResolution;
			x0 += xd; x1 += xd;
			double yt = PanelBottom - PanelResolution / World.AspectRatio;
			double yd = (PanelCenterY - yt) / (PanelBottom - yt) - 0.5;
			y0 += yd; y1 += yd;
			x0 = (x0 - 0.5) * WorldWidth;
			x1 = (x1 - 0.5) * WorldWidth;
			y0 = (y0 - 0.5) * WorldHeight;
			y1 = (y1 - 0.5) * WorldHeight;
			double xm = x0 * (1.0 - RelativeRotationCenterX) + x1 * RelativeRotationCenterX;
			double ym = y0 * (1.0 - RelativeRotationCenterY) + y1 * RelativeRotationCenterY;
			Vector3[] v = new Vector3[4];
			v[0] = new Vector3(x0 - xm, y1 - ym, 0);
			v[1] = new Vector3(x0 - xm, y0 - ym, 0);
			v[2] = new Vector3(x1 - xm, y0 - ym, 0);
			v[3] = new Vector3(x1 - xm, y1 - ym, 0);
			Vertex t0 = new Vertex(v[0], new Vector2(0.0f, 1.0f));
			Vertex t1 = new Vertex(v[1], new Vector2(0.0f, 0.0f));
			Vertex t2 = new Vertex(v[2], new Vector2(1.0f, 0.0f));
			Vertex t3 = new Vertex(v[3], new Vector2(1.0f, 1.0f));
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new World.MeshFace[] { new World.MeshFace(new int[] { 0, 1, 2, 3 }) };
			Object.Mesh.Materials = new World.MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = (byte)(DaytimeTexture != null ? World.MeshMaterial.TransparentColorMask : 0);
			Object.Mesh.Materials[0].Color = Color;
			Object.Mesh.Materials[0].TransparentColor = new Color24(0, 0, 255);
			Object.Mesh.Materials[0].DaytimeTexture = DaytimeTexture;
			Object.Mesh.Materials[0].NighttimeTexture = NighttimeTexture;
			Object.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = xm + Driver.X;
			o.Y = ym + Driver.Y;
			o.Z = EyeDistance - Distance + Driver.Z;
			// add object
			if (AddStateToLastElement)
			{
				int n = Train.Cars[Train.DriverCar].CarSections[0].Elements.Length - 1;
				int j = Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States.Length;
				Array.Resize<ObjectManager.AnimatedObjectState>(ref Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States, j + 1);
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[j].Position = o;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[j].Object = Object;
				return n;
			}
			else
			{
				int n = Train.Cars[Train.DriverCar].CarSections[0].Elements.Length;
				Array.Resize<ObjectManager.AnimatedObject>(ref Train.Cars[Train.DriverCar].CarSections[0].Elements, n + 1);
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n] = new ObjectManager.AnimatedObject();
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States = new ObjectManager.AnimatedObjectState[1];
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[0].Position = o;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].States[0].Object = Object;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].CurrentState = 0;
				Train.Cars[Train.DriverCar].CarSections[0].Elements[n].ObjectIndex = ObjectManager.CreateDynamicObject();
				ObjectManager.Objects[Train.Cars[Train.DriverCar].CarSections[0].Elements[n].ObjectIndex] = Object.Clone();
				return n;
			}
		}
	}
}
