
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using LibRender2.Trains;
using OpenBve.Parsers.Panel;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using TrainManager.Trains;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train : TrainBase
		{
			
			public Train(TrainState state) : base(state)
			{
			}

			/// <summary>Attempts to load and parse the current train's panel configuration file.</summary>
			/// <param name="TrainPath">The absolute on-disk path to the train folder.</param>
			/// <param name="Encoding">The selected train encoding</param>
			internal void ParsePanelConfig(string TrainPath, System.Text.Encoding Encoding)
			{
				Cars[DriverCar].CarSections = new CarSection[1];
				Cars[DriverCar].CarSections[0] = new CarSection(Program.CurrentHost, ObjectType.Overlay);
				string File = OpenBveApi.Path.CombineFile(TrainPath, "panel.xml");
				if (!System.IO.File.Exists(File))
				{
					//Try animated variant too
					File = OpenBveApi.Path.CombineFile(TrainPath, "panel.animated.xml");
				}

				if (System.IO.File.Exists(File))
				{
					Program.FileSystem.AppendToLogFile("Loading train panel: " + File);
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
								PanelAnimatedXmlParser.ParsePanelAnimatedXml(System.IO.Path.GetFileName(File), TrainPath, this, DriverCar);
								if (Cars[DriverCar].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
								{
									Cars[DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
								}
							}

							DocumentElements = CurrentXML.Root.Elements("Panel");
							if (DocumentElements.Any())
							{
								PanelXmlParser.ParsePanelXml(System.IO.Path.GetFileName(File), TrainPath, this, DriverCar);
								Cars[DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
								Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
							}
						}
					}
					catch
					{
						var currentError = Translations.GetInterfaceString("errors_critical_file");
						currentError = currentError.Replace("[file]", "panel.xml");
						MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
						Program.RestartArguments = " ";
						Loading.Cancel = true;
						return;
					}

					if (Cars[DriverCar].CarSections[0].Groups[0].Elements.Any())
					{
						OpenBVEGame.RunInRenderThread(() =>
						{
							//Needs to be on the thread containing the openGL context
							Program.Renderer.InitializeVisibility();
						});
						Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						return;
					}

					Interface.AddMessage(MessageType.Error, false, "The panel.xml file " + File + " failed to load. Falling back to legacy panel.");
				}
				else
				{
					File = OpenBveApi.Path.CombineFile(TrainPath, "panel.animated");
					if (System.IO.File.Exists(File))
					{
						Program.FileSystem.AppendToLogFile("Loading train panel: " + File);
						if (System.IO.File.Exists(OpenBveApi.Path.CombineFile(TrainPath, "panel2.cfg")) || System.IO.File.Exists(OpenBveApi.Path.CombineFile(TrainPath, "panel.cfg")))
						{
							Program.FileSystem.AppendToLogFile("INFO: This train contains both a 2D and a 3D panel. The 3D panel will always take precedence");
						}

						UnifiedObject currentObject;
						Program.CurrentHost.LoadObject(File, Encoding, out currentObject);
						var a = currentObject as AnimatedObjectCollection;
						if (a != null)
						{
							//HACK: If a == null , loading our animated object completely failed (Missing objects?). Fallback to trying the panel2.cfg
							try
							{
								for (int i = 0; i < a.Objects.Length; i++)
								{
									Program.CurrentHost.CreateDynamicObject(ref a.Objects[i].internalObject);
								}

								Cars[DriverCar].CarSections[0].Groups[0].Elements = a.Objects;
								if (Cars[DriverCar].CameraRestrictionMode != CameraRestrictionMode.Restricted3D)
								{
									Cars[DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
									Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
								}

								Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
								return;
							}
							catch
							{
								var currentError = Translations.GetInterfaceString("errors_critical_file");
								currentError = currentError.Replace("[file]", "panel.animated");
								MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
								Program.RestartArguments = " ";
								Loading.Cancel = true;
								return;
							}
						}

						Interface.AddMessage(MessageType.Error, false, "The panel.animated file " + File + " failed to load. Falling back to 2D panel.");
					}
				}

				var Panel2 = false;
				try
				{
					File = OpenBveApi.Path.CombineFile(TrainPath, "panel2.cfg");
					if (System.IO.File.Exists(File))
					{
						Program.FileSystem.AppendToLogFile("Loading train panel: " + File);
						Panel2 = true;
						Panel2CfgParser.ParsePanel2Config("panel2.cfg", TrainPath, Cars[DriverCar]);
						Cars[DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
						Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
					}
					else
					{
						File = OpenBveApi.Path.CombineFile(TrainPath, "panel.cfg");
						if (System.IO.File.Exists(File))
						{
							Program.FileSystem.AppendToLogFile("Loading train panel: " + File);
							PanelCfgParser.ParsePanelConfig(TrainPath, Encoding, Cars[DriverCar]);
							Cars[DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
							Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
						}
						else
						{
							Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
						}
					}
				}
				catch
				{
					var currentError = Translations.GetInterfaceString("errors_critical_file");
					currentError = currentError.Replace("[file]", Panel2 ? "panel2.cfg" : "panel.cfg");
					MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Program.RestartArguments = " ";
					Loading.Cancel = true;
				}
			}

		}
	}
}
