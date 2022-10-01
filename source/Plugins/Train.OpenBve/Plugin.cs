using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using LibRender2;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using TrainManager.Motor;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
    public class Plugin : TrainInterface
    {
	    internal static HostInterface currentHost;

	    internal static FileSystem FileSystem;

	    internal static BaseOptions CurrentOptions;

	    internal static Random RandomNumberGenerator = new Random();

	    internal static BaseRenderer Renderer;

	    internal TrainDatParser TrainDatParser;

	    internal ExtensionsCfgParser ExtensionsCfgParser;

	    internal SoundCfgParser SoundCfgParser;

	    internal BVE2SoundParser BVE2SoundParser;

	    internal BVE4SoundParser BVE4SoundParser;

	    internal SoundXmlParser SoundXmlParser;

	    internal PanelCfgParser PanelCfgParser;

	    internal Panel2CfgParser Panel2CfgParser;

	    internal PanelXmlParser PanelXmlParser;

	    internal PanelAnimatedXmlParser PanelAnimatedXmlParser;

	    internal TrainXmlParser TrainXmlParser;

	    internal Control[] CurrentControls;

	    internal double LastProgress;

	    internal static bool XMLOnly;

		public Plugin()
	    {
		    if (TrainDatParser == null)
		    {
			    TrainDatParser = new TrainDatParser(this);
		    }

		    if (ExtensionsCfgParser == null)
		    {
			    ExtensionsCfgParser = new ExtensionsCfgParser(this);
		    }

		    if (SoundCfgParser == null)
		    {
			    SoundCfgParser = new SoundCfgParser(this);
			    BVE2SoundParser = new BVE2SoundParser(this);
			    BVE4SoundParser = new BVE4SoundParser(this);
			    SoundXmlParser = new SoundXmlParser(this);
		    }

		    if (PanelCfgParser == null)
		    {
			    PanelCfgParser = new PanelCfgParser(this);
			    Panel2CfgParser = new Panel2CfgParser(this);
			    PanelXmlParser = new PanelXmlParser(this);
			    PanelAnimatedXmlParser = new PanelAnimatedXmlParser(this);
		    }

		    if (TrainXmlParser == null)
		    {
			    TrainXmlParser = new TrainXmlParser(this);
		    }
	    }

		public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions Options, object rendererReference)
		{
			currentHost = host;
			FileSystem = fileSystem;
			CurrentOptions = Options;
			// ReSharper disable once MergeCastWithTypeCheck
			if (rendererReference is BaseRenderer)
			{
				Renderer = (BaseRenderer)rendererReference;
			}
		}

		public override bool CanLoadTrain(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return false;
			}
			if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
			{
				string vehicleTxt = Path.CombineFile(path, "vehicle.txt");
				if (File.Exists(vehicleTxt))
				{
					string[] lines = File.ReadAllLines(vehicleTxt);
					for (int i = 10; i < lines.Length; i++)
					{
						if (lines[i].StartsWith(@"bvets vehicle ", StringComparison.InvariantCultureIgnoreCase))
						{
							/*
							 * BVE5 format train
							 * When the BVE5 plugin is implemented, this should return false, as BVE5 trains
							 * often seem to keep the train.dat lying around and we need to use the right plugin
							 *
							 * For the moment however, this is ignored....
							 */
						}
					}
				}

				string trainDat = Path.CombineFile(path, "train.dat");
				if (File.Exists(trainDat))
				{
					if (TrainDatParser.CanLoad(trainDat))
					{
						return true;
					}
				}

				string trainXML = Path.CombineFile(path, "train.xml");
				if (File.Exists(trainXML))
				{
					/*
					 * XML format train
					 * At present, XML is used only as an extension, but acceleration etc. will be implemented
					 * When this is done, return true here
					 */
					return true;
				}

				return false;
			}

			if (File.Exists(path))
			{
				if (path.EndsWith("vehicle.txt", StringComparison.InvariantCultureIgnoreCase))
				{
					/*
					 * BVE5 format train
					 * When the BVE5 plugin is implemented, this should return false, as BVE5 trains
					 * often seem to keep the train.dat lying around and we need to use the right plugin
					 *
					 * For the moment however, this is ignored....
					 */
				}

				if (path.EndsWith("train.dat", StringComparison.InvariantCultureIgnoreCase) || path.EndsWith("train.ai", StringComparison.InvariantCultureIgnoreCase))
				{
					if (TrainDatParser.CanLoad(path))
					{
						return true;
					}
				}

				if (path.EndsWith("train.xml", StringComparison.InvariantCultureIgnoreCase))
				{
					/*
					 * XML format train
					 * At present, XML is used only as an extension, but acceleration etc. will be implemented
					 * When this is done, return true here
					 */
					return true;
				}
			}
			return false;
		}

	    public override bool LoadTrain(Encoding Encoding, string trainPath, ref AbstractTrain train, ref Control[] currentControls)
	    {
		    bool[] VisibleFromInterior = new bool[0];
		    UnifiedObject[] CarObjects = new UnifiedObject[0];
		    UnifiedObject[] BogieObjects = new UnifiedObject[0];
		    UnifiedObject[] CouplerObjects = new UnifiedObject[0];
		    CurrentProgress = 0.0;
		    LastProgress = 0.0;
		    IsLoading = true;
		    CurrentControls = currentControls;
		    TrainBase currentTrain = train as TrainBase;
		    if (currentTrain == null)
		    {
				currentHost.ReportProblem(ProblemType.InvalidData, "Train was not valid");
				IsLoading = false;
				return false;
		    }

		    if (currentTrain.State == TrainState.Bogus)
		    {
			    // bogus train
			    string TrainData = Path.CombineFile(FileSystem.GetDataFolder("Compatibility", "PreTrain"), "train.dat");
			    TrainDatParser.Parse(TrainData, Encoding.UTF8, currentTrain);
			    Thread.Sleep(1);

			    if (Cancel)
			    {
				    IsLoading = false;
				    return false;
			    }
		    }
		    else
		    {
				currentTrain.TrainFolder = trainPath;
			    // real train
			    if (currentTrain.IsPlayerTrain)
			    {
				    FileSystem.AppendToLogFile("Loading player train: " + currentTrain.TrainFolder);
			    }
			    else
			    {
				    FileSystem.AppendToLogFile("Loading AI train: " + currentTrain.TrainFolder);
			    }

				string TrainData = null;
				if (!currentTrain.IsPlayerTrain)
				{
					TrainData = Path.CombineFile(currentTrain.TrainFolder, "train.ai");
				}
				if (currentTrain.IsPlayerTrain || !File.Exists(TrainData))
				{
					TrainData = Path.CombineFile(currentTrain.TrainFolder, "train.dat");
				}

				if (File.Exists(TrainData))
				{
					TrainDatParser.Parse(TrainData, Encoding, currentTrain);
				}
				else
				{
					string tXml = Path.CombineFile(currentTrain.TrainFolder, "train.xml");
					if (File.Exists(tXml))
					{
						XMLOnly = true;
						TrainXmlParser.Parse(tXml, currentTrain, ref CarObjects, ref BogieObjects, ref CouplerObjects, out VisibleFromInterior);
					}
				}
				
			    LastProgress = 0.1;
			    Thread.Sleep(1);
			    if (Cancel)
			    {
				    IsLoading = false;
				    return false;
			    }
		    }
		    // add panel section
		    if (currentTrain.IsPlayerTrain) {	
			    ParsePanelConfig(currentTrain, Encoding);
			    LastProgress = 0.6;
			    Thread.Sleep(1);
			    if (Cancel)
			    {
				    IsLoading = false;
				    return false;
			    }
			    FileSystem.AppendToLogFile("Train panel loaded sucessfully.");
		    }
			// add exterior section
			if (currentTrain.State != TrainState.Bogus)
			{
				string tXml = Path.CombineFile(currentTrain.TrainFolder, "train.xml");
				if (!XMLOnly)
				{
					// being used as an extension, so we need to load it now
					Array.Resize(ref CarObjects, currentTrain.Cars.Length + 1);
					Array.Resize(ref BogieObjects, (currentTrain.Cars.Length + 1) * 2);
					Array.Resize(ref CouplerObjects, currentTrain.Cars.Length + 1);
					if (File.Exists(tXml))
					{
						TrainXmlParser.Parse(tXml, currentTrain, ref CarObjects, ref BogieObjects, ref CouplerObjects, out VisibleFromInterior);
					}
					else
					{
						ExtensionsCfgParser.ParseExtensionsConfig(currentTrain.TrainFolder, Encoding, ref CarObjects, ref BogieObjects, ref CouplerObjects, out VisibleFromInterior, currentTrain);
					}
				}
				

				currentTrain.CameraCar = currentTrain.DriverCar;
				Thread.Sleep(1);
				if (Cancel)
				{
					IsLoading = false;
					return false;
				}
				//Stores the current array index of the bogie object to add
				//Required as there are two bogies per car, and we're using a simple linear array....
				int currentBogieObject = 0;
				for (int i = 0; i < currentTrain.Cars.Length; i++)
				{
					if (CarObjects[i] == null)
					{
						// load default exterior object
						string file = Path.CombineFile(FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
						currentHost.LoadStaticObject(file, Encoding.UTF8, false, out var so);
						if (so == null)
						{
							CarObjects[i] = null;
						}
						else
						{
							StaticObject c = (StaticObject) so.Clone(); //Clone as otherwise the cached object doesn't scale right
							c.ApplyScale(currentTrain.Cars[i].Width, currentTrain.Cars[i].Height, currentTrain.Cars[i].Length);
							CarObjects[i] = c;
						}
					}

					if (CarObjects[i] != null)
					{
						// add object
						currentTrain.Cars[i].LoadCarSections(CarObjects[i], VisibleFromInterior[i]);
					}

					if (CouplerObjects[i] != null)
					{
						currentTrain.Cars[i].Coupler.LoadCarSections(CouplerObjects[i], VisibleFromInterior[i]);
					}

					//Load bogie objects
					if (BogieObjects[currentBogieObject] != null)
					{
						currentTrain.Cars[i].FrontBogie.LoadCarSections(BogieObjects[currentBogieObject], VisibleFromInterior[i]);
					}

					currentBogieObject++;
					if (BogieObjects[currentBogieObject] != null)
					{
						currentTrain.Cars[i].RearBogie.LoadCarSections(BogieObjects[currentBogieObject], VisibleFromInterior[i]);
					}

					currentBogieObject++;
				}
			}

			if (currentTrain.State != TrainState.Bogus)
			{
				if (Cancel) return false;
				SoundCfgParser.ParseSoundConfig(currentTrain);
				/*
				 * Determine door opening / closing speed & copy in any missing bits (e.g. motor sound tables)
				 * This *must* be done after the sound configuration has loaded
				 * (As the original BVE / OpenBVE implimentation calculates this from
				 * the length of the sound buffer)
				 *
				 * However, as motor performance / sound tables may also be loaded via car XML files,
				 * this also needs to be done dead last
				 */

				for (int i = 0; i < currentTrain.Cars.Length; i++)
				{
					currentTrain.Cars[i].DetermineDoorClosingSpeed();
					if (currentTrain.Cars[i].Specs.IsMotorCar && TrainXmlParser.MotorSoundXMLParsed != null)
					{
						if(!TrainXmlParser.MotorSoundXMLParsed[i])
						{
							currentTrain.Cars[i].Sounds.Motor = new BVEMotorSound(currentTrain.Cars[i], TrainXmlParser.MotorSound.SpeedConversionFactor, TrainXmlParser.MotorSound.Tables);
						}
					}
				}
			}
			// place cars
			currentTrain.PlaceCars(0.0);
			currentControls = CurrentControls;
			IsLoading = false;
			return true;
	    }

	    public override string GetDescription(string trainPath, Encoding encoding = null)
	    {
		    try
		    {
			    string xmlFile = Path.CombineFile(trainPath, "train.xml");
			    if (File.Exists(xmlFile))
			    {
				    XmlDocument currentXML = new XmlDocument();
				    //Load the marker's XML file 
				    currentXML.Load(xmlFile);
				    XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Train/Description");
				    if (DocumentNodes != null && DocumentNodes.Count > 0)
				    {
					    for (int i = 0; i < DocumentNodes.Count; i++)
					    {
						    if (!string.IsNullOrEmpty(DocumentNodes[i].InnerText))
						    {
							    return DocumentNodes[i].InnerText;
						    }
					    }
				    }
			    }
				string descriptionFile = Path.CombineFile(trainPath, "train.txt");
			    if (!File.Exists(descriptionFile))
			    {
					// No description, but a readme- Let's try that instead to at least give something
					descriptionFile = Path.CombineFile(trainPath, "readme.txt");
			    }
			    if (File.Exists(descriptionFile))
			    {
				    if (encoding == null)
				    {
					    return File.ReadAllText(descriptionFile);
				    }
				    return File.ReadAllText(descriptionFile, encoding);

			    }
		    }
		    catch(Exception ex)
		    {
				currentHost.ReportProblem(ProblemType.UnexpectedException, "Unable to get the description for train " + trainPath + " due to the exeception: " + ex.Message);
		    }
		    return string.Empty;
	    }

	    public override Image GetImage(string trainPath)
	    {
		    try
		    {
			    string imageFile = Path.CombineFile(trainPath, "train.png");
			    if (File.Exists(imageFile))
			    {
				    return Image.FromFile(imageFile);
			    }
			    imageFile  = Path.CombineFile(trainPath, "train.gif");
			    if (File.Exists(imageFile))
			    {
				    return Image.FromFile(imageFile);
			    }
			    imageFile  = Path.CombineFile(trainPath, "train.bmp");
			    if (File.Exists(imageFile))
			    {
				    return Image.FromFile(imageFile);
			    }
		    }
		    catch (Exception ex)
		    {
			    currentHost.ReportProblem(ProblemType.UnexpectedException, "Unable to get the image for train " + trainPath + " due to the exeception: " + ex.Message);
		    }
		    return null;
	    }


	    /// <summary>Attempts to load and parse the current train's panel configuration file.</summary>
	    /// <param name="Train">The train</param>
	    /// <param name="Encoding">The selected train encoding</param>
	    internal void ParsePanelConfig(TrainBase Train, Encoding Encoding)
	    {
		    Train.Cars[Train.DriverCar].CarSections = new CarSection[1];
		    Train.Cars[Train.DriverCar].CarSections[0] = new CarSection(currentHost, ObjectType.Overlay, true);
		    string File = Path.CombineFile(Train.TrainFolder, "panel.xml");
		    if (!System.IO.File.Exists(File))
		    {
			    //Try animated variant too
			    File = Path.CombineFile(Train.TrainFolder, "panel.animated.xml");
		    }

		    if (System.IO.File.Exists(File))
		    {
			    FileSystem.AppendToLogFile("Loading train panel: " + File);
			    try
			    {
				    /*
				     * First load the XML. We use this to determine
				     * whether this is a 2D or a 3D animated panel
				     */
				    XDocument CurrentXML = XDocument.Load(File, LoadOptions.SetLineInfo);

				    // Check for null
				    if (CurrentXML.Root != null)
				    {

					    IEnumerable<XElement> DocumentElements = CurrentXML.Root.Elements("PanelAnimated");
					    if (DocumentElements.Any())
					    {
						    PanelAnimatedXmlParser.ParsePanelAnimatedXml(System.IO.Path.GetFileName(File), Train, Train.DriverCar);
						    if (Train.Cars[Train.DriverCar].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
						    {
							    Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
						    }
							return;
					    }

					    DocumentElements = CurrentXML.Root.Elements("Panel");
					    if (DocumentElements.Any())
					    {
						    PanelXmlParser.ParsePanelXml(System.IO.Path.GetFileName(File), Train, Train.DriverCar);
						    Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
						    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
						    return;
					    }
				    }
			    }
			    catch
			    {
				    var currentError = Translations.GetInterfaceString("errors_critical_file");
				    currentError = currentError.Replace("[file]", "panel.xml");
				    currentHost.ReportProblem(ProblemType.InvalidData, currentError);
				    Cancel = true;
				    return;
			    }

			    currentHost.AddMessage(MessageType.Error, false, "The panel.xml file " + File + " failed to load. Falling back to legacy panel.");
		    }
		    else
		    {
			    File = Path.CombineFile(Train.TrainFolder, "panel.animated");
			    if (System.IO.File.Exists(File))
			    {
				    FileSystem.AppendToLogFile("Loading train panel: " + File);
				    if (System.IO.File.Exists(Path.CombineFile(Train.TrainFolder, "panel2.cfg")) || System.IO.File.Exists(Path.CombineFile(Train.TrainFolder, "panel.cfg")))
				    {
					    FileSystem.AppendToLogFile("INFO: This train contains both a 2D and a 3D panel. The 3D panel will always take precedence");
				    }

				    currentHost.LoadObject(File, Encoding, out var currentObject);
				    var a = (AnimatedObjectCollection)currentObject;
				    if (a != null)
				    {
					    //HACK: If a == null , loading our animated object completely failed (Missing objects?). Fallback to trying the panel2.cfg
					    try
					    {
						    for (int i = 0; i < a.Objects.Length; i++)
						    {
							    currentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
						    }

						    Train.Cars[Train.DriverCar].CarSections[0].Groups[0].Elements = a.Objects;
						    if (Train.Cars[Train.DriverCar].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
						    {
							    Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
						    }

						    return;
					    }
					    catch
					    {
						    var currentError = Translations.GetInterfaceString("errors_critical_file");
						    currentError = currentError.Replace("[file]", "panel.animated");
						    currentHost.ReportProblem(ProblemType.InvalidData, currentError);
						    Cancel = true;
						    return;
					    }
				    }

				    currentHost.AddMessage(MessageType.Error, false, "The panel.animated file " + File + " failed to load. Falling back to 2D panel.");
			    }
		    }

		    var Panel2 = false;
		    try
		    {
			    File = Path.CombineFile(Train.TrainFolder, "panel2.cfg");
			    if (System.IO.File.Exists(File))
			    {
				    FileSystem.AppendToLogFile("Loading train panel: " + File);
				    Panel2 = true;
				    Panel2CfgParser.ParsePanel2Config("panel2.cfg", Train.TrainFolder, Train.Cars[Train.DriverCar]);
				    Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
				    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
			    }
			    else
			    {
				    File = Path.CombineFile(Train.TrainFolder, "panel.cfg");
				    if (System.IO.File.Exists(File))
				    {
					    FileSystem.AppendToLogFile("Loading train panel: " + File);
					    PanelCfgParser.ParsePanelConfig(Train.TrainFolder, Encoding, Train.Cars[Train.DriverCar]);
					    Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
					    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
				    }
				    else
				    {
					    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
				    }
			    }
		    }
		    catch
		    {
			    var currentError = Translations.GetInterfaceString("errors_critical_file");
			    currentError = currentError.Replace("[file]", Panel2 ? "panel2.cfg" : "panel.cfg");
			    currentHost.ReportProblem(ProblemType.InvalidData, currentError);
			    Cancel = true;
		    }
	    }
    }
}
