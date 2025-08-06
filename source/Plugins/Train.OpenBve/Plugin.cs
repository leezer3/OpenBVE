using System;
using System.Collections.Generic;
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
using TrainManager.Power;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
    public class Plugin : TrainInterface
    {
	    internal static HostInterface CurrentHost;

	    internal static FileSystem FileSystem;

	    internal static BaseOptions CurrentOptions;

	    internal static Random RandomNumberGenerator = new Random();

	    internal static BaseRenderer Renderer;

	    internal TrainDatParser TrainDatParser;

	    internal ExtensionsCfgParser ExtensionsCfgParser;

	    internal SoundCfgParser SoundCfgParser;

		// ReSharper disable InconsistentNaming
		internal BVE2SoundParser BVE2SoundParser;
		
	    internal BVE4SoundParser BVE4SoundParser;
	    // ReSharper restore InconsistentNaming

	    internal SoundXmlParser SoundXmlParser;

	    internal PanelCfgParser PanelCfgParser;

	    internal Panel2CfgParser Panel2CfgParser;

	    internal PanelXmlParser PanelXmlParser;

	    internal PanelAnimatedXmlParser PanelAnimatedXmlParser;

	    internal TrainXmlParser TrainXmlParser;

	    internal Control[] CurrentControls;

	    internal double LastProgress;

	    internal static BVEMotorSoundTable[] MotorSoundTables;
	    internal static BveAccelerationCurve[] AccelerationCurves;
	    internal static double MaximumAcceleration;

		public Plugin()
	    {
		    TrainDatParser = new TrainDatParser(this);

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

		public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions options, object rendererReference)
		{
			CurrentHost = host;
			FileSystem = fileSystem;
			CurrentOptions = options;
			// ReSharper disable once MergeCastWithTypeCheck
			if (rendererReference is BaseRenderer)
			{
				Renderer = (BaseRenderer)rendererReference;
			}
		}

		public override bool CanLoadTrain(string path)
		{
			if (string.IsNullOrEmpty(path) || !Directory.Exists(Path.GetDirectoryName(path)))
			{
				return false;
			}
			if (Directory.Exists(path))
			{
				string vehicleTxt;
				try {
					vehicleTxt = Path.CombineFile(path, "vehicle.txt");
				} catch {
					return false;
				}
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
				}
			}
			return false;
		}

	    public override bool LoadTrain(Encoding encoding, string trainPath, ref AbstractTrain train, ref Control[] currentControls)
	    {
		    CurrentProgress = 0.0;
		    LastProgress = 0.0;
		    IsLoading = true;
		    CurrentControls = currentControls;
		    TrainBase currentTrain = train as TrainBase;
		    if (currentTrain == null)
		    {
				CurrentHost.ReportProblem(ProblemType.InvalidData, "Train was not valid");
				IsLoading = false;
				return false;
		    }

		    if (currentTrain.State == TrainState.Bogus)
		    {
			    // bogus train
			    string trainData = Path.CombineFile(FileSystem.GetDataFolder("Compatibility", "PreTrain"), "train.dat");
			    TrainDatParser.Parse(trainData, Encoding.UTF8, currentTrain);
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

				string trainData = null;
				if (!currentTrain.IsPlayerTrain)
				{
					trainData = Path.CombineFile(currentTrain.TrainFolder, "train.ai");
				}
				if (currentTrain.IsPlayerTrain || !File.Exists(trainData))
				{
					trainData = Path.CombineFile(currentTrain.TrainFolder, "train.dat");
				}
				TrainDatParser.Parse(trainData, encoding, currentTrain);
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
			    ParsePanelConfig(currentTrain, encoding);
			    Thread.Sleep(1);
			    if (Cancel)
			    {
				    IsLoading = false;
				    return false;
			    }
			    FileSystem.AppendToLogFile("Train panel loaded sucessfully.");
		    }

		    CurrentProgress = 0.5;
		    LastProgress = 0.5;
		    
			// add exterior section
			if (currentTrain.State != TrainState.Bogus)
			{
				bool[] visibleFromInterior = null;
				UnifiedObject[] carObjects = new UnifiedObject[currentTrain.Cars.Length];
				UnifiedObject[] bogieObjects = new UnifiedObject[currentTrain.Cars.Length * 2];
				UnifiedObject[] couplerObjects = new UnifiedObject[currentTrain.Cars.Length];

				string tXml = Path.CombineFile(currentTrain.TrainFolder, "train.xml");

				bool parseExtensionsCfg = true;
				if (File.Exists(tXml))
				{
					try
					{
						TrainXmlParser.Parse(tXml, currentTrain, ref carObjects, ref bogieObjects, ref couplerObjects, out visibleFromInterior);
						parseExtensionsCfg = false;
					}
					catch(Exception e)
					{
						CurrentHost.ReportProblem(ProblemType.UnexpectedException, "Whilst processing XML file " + tXml + " encountered the following exeception:" + Environment.NewLine + e);
					}
					
				}

				if(parseExtensionsCfg)
				{
					// train.xml is either missing or broken
					ExtensionsCfgParser.ParseExtensionsConfig(currentTrain.TrainFolder, encoding, ref carObjects, ref bogieObjects, ref couplerObjects, out visibleFromInterior, currentTrain);
				}

				currentTrain.CameraCar = currentTrain.DriverCar;
				Thread.Sleep(1);
				if (Cancel)
				{
					IsLoading = false;
					return false;
				}
				
				CurrentProgress = 0.75;
				LastProgress = 0.75;
				
				//Stores the current array index of the bogie object to add
				//Required as there are two bogies per car, and we're using a simple linear array....
				int currentBogieObject = 0;
				for (int i = 0; i < currentTrain.Cars.Length; i++)
				{
					if (carObjects[i] == null)
					{
						// load default exterior object
						string file = Path.CombineFile(FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
						CurrentHost.LoadStaticObject(file, Encoding.UTF8, false, out var so);
						if (so == null)
						{
							carObjects[i] = null;
						}
						else
						{
							StaticObject c = (StaticObject) so.Clone(); //Clone as otherwise the cached object doesn't scale right
							c.ApplyScale(currentTrain.Cars[i].Width, currentTrain.Cars[i].Height, currentTrain.Cars[i].Length);
							carObjects[i] = c;
						}
					}

					if (carObjects[i] != null)
					{
						// add object
						currentTrain.Cars[i].CarSections.Add(CarSectionType.Exterior, new CarSection(CurrentHost, ObjectType.Dynamic, visibleFromInterior[i], currentTrain.Cars[i], carObjects[i]));
					}

					if (couplerObjects[i] != null)
					{
						currentTrain.Cars[i].Coupler.LoadCarSections(couplerObjects[i], visibleFromInterior[i]);
					}

					//Load bogie objects
					if (bogieObjects[currentBogieObject] != null)
					{
						currentTrain.Cars[i].FrontBogie.LoadCarSections(bogieObjects[currentBogieObject], visibleFromInterior[i]);
					}

					currentBogieObject++;
					if (bogieObjects[currentBogieObject] != null)
					{
						currentTrain.Cars[i].RearBogie.LoadCarSections(bogieObjects[currentBogieObject], visibleFromInterior[i]);
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

				int numMotorCars = 0;
				for (int i = 0; i < currentTrain.Cars.Length; i++)
				{
					currentTrain.Cars[i].DetermineDoorClosingSpeed();
					if (currentTrain.Cars[i].TractionModel.ProvidesPower)
					{
						numMotorCars++;
						if (currentTrain.Cars[i].TractionModel.MotorSounds == null && TrainXmlParser.MotorSoundXMLParsed != null)
						{
							if(!TrainXmlParser.MotorSoundXMLParsed[i])
							{
								currentTrain.Cars[i].TractionModel.MotorSounds = new BVEMotorSound(currentTrain.Cars[i], 18.0, MotorSoundTables);
							}
						}
						
					}
				}

				if (currentTrain.IsPlayerTrain && numMotorCars == 0)
				{
					/*
					 * https://bveworldwide.forumotion.com/t2389-engine-sound-in-correct-car-wagon#21789
					 *
					 * Workaround for the suspected cause of this one- Malformed XML where all cars are set as trailer
					 * Speed / physics are likely to be off, but let's at least do something
					 */
					CurrentHost.AddMessage(MessageType.Error, false, "Player train appears to have no motor cars, assigning Car 0 as a motor car.");
					currentTrain.Cars[0].TractionModel = new BVEMotorCar(currentTrain.Cars[0], null);
					currentTrain.Cars[0].TractionModel.MotorSounds = new BVEMotorSound(currentTrain.Cars[0], 18.0, MotorSoundTables);
				}
			}
			CurrentProgress = 1;
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
				    XmlNodeList documentNodes = currentXML.DocumentElement?.SelectNodes("/openBVE/Train/Description");
				    if (documentNodes != null && documentNodes.Count > 0)
				    {
					    for (int i = 0; i < documentNodes.Count; i++)
					    {
						    if (!string.IsNullOrEmpty(documentNodes[i].InnerText))
						    {
							    return documentNodes[i].InnerText;
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
			    if (!File.Exists(descriptionFile))
			    {
				    // another variant on readme
				    descriptionFile = Path.CombineFile(trainPath, "read me.txt");
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
				CurrentHost.ReportProblem(ProblemType.UnexpectedException, "Unable to get the description for train " + trainPath + " due to the exeception: " + ex.Message);
		    }
		    return string.Empty;
	    }

	    public override string GetImage(string trainPath)
	    {
		    try
		    {
			    string imageFile = Path.CombineFile(trainPath, "train.png");
			    if (File.Exists(imageFile))
			    {
				    return imageFile;
			    }
			    imageFile  = Path.CombineFile(trainPath, "train.gif");
			    if (File.Exists(imageFile))
			    {
				    return imageFile;
			    }
			    imageFile  = Path.CombineFile(trainPath, "train.bmp");
			    if (File.Exists(imageFile))
			    {
				    return imageFile;
			    }
		    }
		    catch (Exception ex)
		    {
			    CurrentHost.ReportProblem(ProblemType.UnexpectedException, "Unable to get the image for train " + trainPath + " due to the exeception: " + ex.Message);
		    }
		    return null;
	    }


	    /// <summary>Attempts to load and parse the current train's panel configuration file.</summary>
	    /// <param name="train">The train</param>
	    /// <param name="encoding">The selected train encoding</param>
	    internal void ParsePanelConfig(TrainBase train, Encoding encoding)
	    {
		    train.Cars[train.DriverCar].CarSections = new Dictionary<CarSectionType, CarSection>();
		    train.Cars[train.DriverCar].CarSections.Add(CarSectionType.Interior, new CarSection(CurrentHost, ObjectType.Overlay, true));
		    string panelFile = Path.CombineFile(train.TrainFolder, "panel.xml");
		    if (!File.Exists(panelFile))
		    {
			    //Try animated variant too
			    panelFile = Path.CombineFile(train.TrainFolder, "panel.animated.xml");
		    }

		    if (File.Exists(panelFile))
		    {
			    FileSystem.AppendToLogFile("Loading train panel: " + panelFile);
			    try
			    {
				    /*
				     * First load the XML. We use this to determine
				     * whether this is a 2D or a 3D animated panel
				     */
				    XDocument currentXML = XDocument.Load(panelFile, LoadOptions.SetLineInfo);

				    // Check for null
				    if (currentXML.Root != null)
				    {
						List<XElement> documentElements = currentXML.Root.Elements("PanelAnimated").ToList();
					    if (documentElements.Count != 0)
					    {
						    PanelAnimatedXmlParser.ParsePanelAnimatedXml(System.IO.Path.GetFileName(panelFile), train, train.DriverCar);
						    if (train.Cars[train.DriverCar].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
						    {
							    train.Cars[train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
						    }
							return;
					    }

					    documentElements = currentXML.Root.Elements("Panel").ToList();
					    if (documentElements.Count != 0)
					    {
						    PanelXmlParser.ParsePanelXml(System.IO.Path.GetFileName(panelFile), train, train.DriverCar);
						    train.Cars[train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
						    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
						    return;
					    }
				    }
			    }
			    catch
			    {
				    var currentError = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","critical_file"});
				    currentError = currentError.Replace("[file]", "panel.xml");
				    CurrentHost.ReportProblem(ProblemType.InvalidData, currentError);
				    Cancel = true;
				    return;
			    }

			    CurrentHost.AddMessage(MessageType.Error, false, "The panel.xml file " + panelFile + " failed to load. Falling back to legacy panel.");
		    }
		    else
		    {
			    panelFile = Path.CombineFile(train.TrainFolder, "panel.animated");
			    if (File.Exists(panelFile))
			    {
				    FileSystem.AppendToLogFile("Loading train panel: " + panelFile);
				    if (File.Exists(Path.CombineFile(train.TrainFolder, "panel2.cfg")) || File.Exists(Path.CombineFile(train.TrainFolder, "panel.cfg")))
				    {
					    FileSystem.AppendToLogFile("INFO: This train contains both a 2D and a 3D panel. The 3D panel will always take precedence");
				    }

				    CurrentHost.LoadObject(panelFile, encoding, out var currentObject);
				    var a = (AnimatedObjectCollection)currentObject;
				    if (a != null)
				    {
					    //HACK: If a == null , loading our animated object completely failed (Missing objects?). Fallback to trying the panel2.cfg
					    try
					    {
						    for (int i = 0; i < a.Objects.Length; i++)
						    {
							    CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
						    }

						    train.Cars[train.DriverCar].CarSections[CarSectionType.Interior].Groups[0].Elements = a.Objects;
						    if (train.Cars[train.DriverCar].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
						    {
							    train.Cars[train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
						    }

						    return;
					    }
					    catch
					    {
						    var currentError = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","critical_file"});
						    currentError = currentError.Replace("[file]", "panel.animated");
						    CurrentHost.ReportProblem(ProblemType.InvalidData, currentError);
						    Cancel = true;
						    return;
					    }
				    }

				    CurrentHost.AddMessage(MessageType.Error, false, "The panel.animated file " + panelFile + " failed to load. Falling back to 2D panel.");
			    }
		    }

		    bool panel2 = false;
		    try
		    {
			    panelFile = Path.CombineFile(train.TrainFolder, "panel2.cfg");
			    if (File.Exists(panelFile))
			    {
				    FileSystem.AppendToLogFile("Loading train panel: " + panelFile);
				    panel2 = true;
				    Panel2CfgParser.ParsePanel2Config("panel2.cfg", train.TrainFolder, train.Cars[train.DriverCar]);
				    train.Cars[train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
				    Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
			    }
			    else
			    {
				    panelFile = Path.CombineFile(train.TrainFolder, "panel.cfg");
				    if (File.Exists(panelFile))
				    {
					    FileSystem.AppendToLogFile("Loading train panel: " + panelFile);
					    PanelCfgParser.ParsePanelConfig(train.TrainFolder, encoding, train.Cars[train.DriverCar]);
					    train.Cars[train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
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
			    var currentError = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","critical_file"});
			    currentError = currentError.Replace("[file]", panel2 ? "panel2.cfg" : "panel.cfg");
			    CurrentHost.ReportProblem(ProblemType.InvalidData, currentError);
			    Cancel = true;
		    }
	    }
    }
}
