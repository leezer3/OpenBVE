using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using LibRender2.Screens;
using LibRender2.Trains;
using TrainManager.BrakeSystems;
using OpenBve.Parsers.Panel;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train : TrainBase
		{
			/// <summary>The plugin used by this train.</summary>
			internal PluginManager.Plugin Plugin;
			/// <summary>The driver body</summary>
			internal DriverBody DriverBody;
			
			private double InternalTimerTimeElapsed;
			internal bool Derailed;
			
			

			private double previousRouteLimit = 0.0;

			internal Train(TrainState state)
			{
				State = state;
				Destination = Interface.CurrentOptions.InitialDestination;
				Station = -1;
				RouteLimits = new double[] { double.PositiveInfinity };
				CurrentRouteLimit = double.PositiveInfinity;
				CurrentSectionLimit = double.PositiveInfinity;
				Cars = new CarBase[] { };
				
				Specs.DoorOpenMode = DoorMode.AutomaticManualOverride;
				Specs.DoorCloseMode = DoorMode.AutomaticManualOverride;
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
			
			/// <summary>Disposes of the train</summary>
			public override void Dispose()
			{
				State = TrainState.Disposed;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].ChangeCarSection(CarSectionType.NotVisible);
					Cars[i].FrontBogie.ChangeSection(-1);
					Cars[i].RearBogie.ChangeSection(-1);
					Cars[i].Coupler.ChangeSection(-1);
				}
				Program.Sounds.StopAllSounds(this);

				for (int i = 0; i < Program.CurrentRoute.Sections.Length; i++)
				{
					Program.CurrentRoute.Sections[i].Leave(this);
				}
				if (Program.CurrentRoute.Sections.Length != 0)
				{
					Program.CurrentRoute.UpdateAllSections();
				}
			}

			/// <inheritdoc/>
			public override bool IsPlayerTrain
			{
				get
				{
					return this == PlayerTrain;
				}
			}

			/// <inheritdoc/>
			public override int NumberOfCars
			{
				get
				{
					return this.Cars.Length;
				}
			}

			/// <inheritdoc/>
			public override void SectionChange()
			{
				if (CurrentSectionLimit == 0.0 && Game.MinimalisticSimulation == false)
				{
					MessageManager.AddMessage(Translations.GetInterfaceString("message_signal_stop"), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
				}
				else if (CurrentSpeed > CurrentSectionLimit)
				{
					MessageManager.AddMessage(Translations.GetInterfaceString("message_signal_overspeed"), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
				}
			}

			/// <inheritdoc/>
			public override void UpdateBeacon(int transponderType, int sectionIndex, int optional)
			{
				if (Plugin != null)
				{
					Plugin.UpdateBeacon(transponderType, sectionIndex, optional);
				}
			}

			/// <inheritdoc/>
			public override void Update(double TimeElapsed)
			{
				if (State == TrainState.Pending)
				{
					// pending train
					bool forceIntroduction = !IsPlayerTrain && !Game.MinimalisticSimulation;
					double time = 0.0;
					if (!forceIntroduction)
					{
						for (int i = 0; i < Program.CurrentRoute.Stations.Length; i++)
						{
							if (Program.CurrentRoute.Stations[i].StopMode == StationStopMode.AllStop | Program.CurrentRoute.Stations[i].StopMode == StationStopMode.PlayerPass)
							{
								if (Program.CurrentRoute.Stations[i].ArrivalTime >= 0.0)
								{
									time = Program.CurrentRoute.Stations[i].ArrivalTime;
								}
								else if (Program.CurrentRoute.Stations[i].DepartureTime >= 0.0)
								{
									time = Program.CurrentRoute.Stations[i].DepartureTime - Program.CurrentRoute.Stations[i].StopTime;
								}
								break;
							}
						}
						time -= TimetableDelta;
					}
					if (Program.CurrentRoute.SecondsSinceMidnight >= time | forceIntroduction)
					{
						bool introduce = true;
						if (!forceIntroduction)
						{
							if (CurrentSectionIndex >= 0)
							{
								if (!Program.CurrentRoute.Sections[CurrentSectionIndex].IsFree())
								{
									introduce = false;
								}
							}
						}

						if (this == PlayerTrain && Loading.SimulationSetup)
						{
							/* Loading has finished, but we still have an AI train in the current section
							 * This may be caused by an iffy RunInterval value, or simply by having no sections							 *
							 *
							 * We must introduce the player's train as otherwise the cab and loop sounds are missing
							 * NOTE: In this case, the signalling cannot prevent the player from colliding with
							 * the AI train
							 */

							introduce = true;
						}
						if (introduce)
						{
							// train is introduced
							State = TrainState.Available;
							for (int j = 0; j < Cars.Length; j++)
							{
								if (Cars[j].CarSections.Length != 0)
								{
									if (j == this.DriverCar && IsPlayerTrain && Interface.CurrentOptions.InitialViewpoint == 0)
									{
										this.Cars[j].ChangeCarSection(CarSectionType.Interior);
									}
									else
									{
										/*
										 * HACK: Load in exterior mode first to ensure everything is cached
										 * before switching immediately to not visible
										 * https://github.com/leezer3/OpenBVE/issues/226
										 * Stuff like the R142A really needs to downsize the textures supplied,
										 * but we have no control over external factors....
										 */
										this.Cars[j].ChangeCarSection(CarSectionType.Exterior);
										if (IsPlayerTrain && Interface.CurrentOptions.InitialViewpoint == 0)
										{
											this.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);

										}
									}

								}
								Cars[j].FrontBogie.ChangeSection(!IsPlayerTrain ? 0 : -1);
								Cars[j].RearBogie.ChangeSection(!IsPlayerTrain ? 0 : -1);
								Cars[j].Coupler.ChangeSection(!IsPlayerTrain ? 0 : -1);
								
								if (Cars[j].Specs.IsMotorCar && Cars[j].Sounds.Loop != null)
								{
									Cars[j].Sounds.Loop.Play(Cars[j], true);
								}
							}
						}
					}
				}
				else if (State == TrainState.Available)
				{
					// available train
					UpdatePhysicsAndControls(TimeElapsed);
					if (CurrentSpeed > CurrentRouteLimit)
					{
						if (previousRouteLimit != CurrentRouteLimit || Interface.CurrentOptions.GameMode == GameMode.Arcade)
						{
							/*
							 * HACK: If the limit has changed, or we are in arcade mode, notify the player
							 *       This conforms to the original behaviour, but doesn't need to raise the message from the event.
							 */
							 MessageManager.AddMessage(Translations.GetInterfaceString("message_route_overspeed"), MessageDependency.RouteLimit, GameMode.Normal, MessageColor.Orange, Double.PositiveInfinity, null);
						}
						
					}
					previousRouteLimit = CurrentRouteLimit;
					if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
					{
						if (CurrentSectionLimit == 0.0)
						{
							MessageManager.AddMessage(Translations.GetInterfaceString("message_signal_stop"), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						}
						else if (CurrentSpeed > CurrentSectionLimit)
						{
							MessageManager.AddMessage(Translations.GetInterfaceString("message_signal_overspeed"), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, Double.PositiveInfinity, null);
						}
					}
					if (AI != null)
					{
						AI.Trigger(TimeElapsed);
					}
				}
				else if (State == TrainState.Bogus)
				{
					// bogus train
					if (AI != null)
					{
						AI.Trigger(TimeElapsed);
					}
				}
				//Trigger point sounds if appropriate
				for (int i = 0; i < Cars.Length; i++)
				{
					CarSound c = null;
					if (Cars[i].FrontAxle.PointSoundTriggered)
					{
						Cars[i].FrontAxle.PointSoundTriggered = false;
						int bufferIndex = Cars[i].FrontAxle.RunIndex;
						if (bufferIndex > Cars[i].FrontAxle.PointSounds.Length - 1)
						{
							//If the switch sound does not exist, return zero
							//Required to handle legacy trains which don't have idx specific run sounds defined
							bufferIndex = 0;
						}
						if (Cars[i].FrontAxle.PointSounds == null || Cars[i].FrontAxle.PointSounds.Length == 0)
						{
							//No point sounds defined at all
							continue;
						}
						c = (CarSound) Cars[i].FrontAxle.PointSounds[bufferIndex];
						if (c.Buffer == null)
						{
							c = (CarSound)Cars[i].FrontAxle.PointSounds[0];
						}
					}
					if (c != null)
					{
						double spd = Math.Abs(CurrentSpeed);
						double pitch = spd / 12.5;
						double gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
						if (pitch > 0.2 && gain > 0.2)
						{
							c.Play(pitch, gain, Cars[i], false);
						}
					}
				}
			}

			

			/// <summary>Updates the physics and controls for this train</summary>
			/// <param name="TimeElapsed">The time elapsed</param>
			private void UpdatePhysicsAndControls(double TimeElapsed)
			{
				if (TimeElapsed == 0.0 || TimeElapsed > 1000)
				{
					//HACK: The physics engine really does not like update times above 1000ms
					//This works around a bug experienced when jumping to a station on a steep hill
					//causing exessive acceleration
					return;
				}
				// move cars
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Move(Cars[i].CurrentSpeed * TimeElapsed);
					if (State == TrainState.Disposed)
					{
						return;
					}
				}
				// update station and doors
				UpdateStation(TimeElapsed);
				UpdateDoors(TimeElapsed);
				// delayed handles
				if (Plugin == null)
				{
					Handles.Power.Safety = Handles.Power.Driver;
					Handles.Brake.Safety = Handles.Brake.Driver;
					Handles.EmergencyBrake.Safety = Handles.EmergencyBrake.Driver;
				}
				Handles.Power.Update();
				Handles.Brake.Update();
				Handles.Brake.Update();
				Handles.EmergencyBrake.Update();
				Handles.HoldBrake.Actual = Handles.HoldBrake.Driver;
				// update speeds
				UpdateSpeeds(TimeElapsed);
				// Update Run and Motor sounds
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].UpdateRunSounds(TimeElapsed);
					Cars[i].UpdateMotorSounds(TimeElapsed);
				}

				// safety system
				if (!Game.MinimalisticSimulation | !IsPlayerTrain)
				{
					UpdateSafetySystem();
				}
				{
					// breaker sound
					bool breaker;
					if (Cars[DriverCar].CarBrake is AutomaticAirBrake)
					{
						breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == (int)AirBrakeHandleState.Release & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
					}
					else
					{
						breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == 0 & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
					}
					Cars[DriverCar].Breaker.Update(breaker);
				}
				// passengers
				Passengers.Update(Specs.CurrentAverageAcceleration, TimeElapsed);
				// signals
				if (CurrentSectionLimit == 0.0)
				{
					if (Handles.EmergencyBrake.Driver & CurrentSpeed > -0.03 & CurrentSpeed < 0.03)
					{
						CurrentSectionLimit = 6.94444444444444;
						if (IsPlayerTrain)
						{
							string s = Translations.GetInterfaceString("message_signal_proceed");
							double a = (3.6 * CurrentSectionLimit) * Interface.CurrentOptions.SpeedConversionFactor;
							s = s.Replace("[speed]", a.ToString("0", CultureInfo.InvariantCulture));
							s = s.Replace("[unit]", Game.UnitOfSpeed);
							MessageManager.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Red, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						}
					}
				}
				// infrequent updates
				InternalTimerTimeElapsed += TimeElapsed;
				if (InternalTimerTimeElapsed > 10.0)
				{
					InternalTimerTimeElapsed -= 10.0;
					Synchronize();
				}
			}

			private void UpdateSpeeds(double TimeElapsed)
			{
				if (Game.MinimalisticSimulation & IsPlayerTrain)
				{
					// hold the position of the player's train during startup
					for (int i = 0; i < Cars.Length; i++)
					{
						Cars[i].CurrentSpeed = 0.0;
						Cars[i].Specs.MotorAcceleration = 0.0;
					}
					return;
				}
				// update brake system
				double[] DecelerationDueToBrake, DecelerationDueToMotor;
				UpdateBrakeSystem(TimeElapsed, out DecelerationDueToBrake, out DecelerationDueToMotor);
				// calculate new car speeds
				double[] NewSpeeds = new double[Cars.Length];
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].UpdateSpeed(TimeElapsed, DecelerationDueToMotor[i], DecelerationDueToBrake[i], out NewSpeeds[i]);
				}
				// calculate center of mass position
				double[] CenterOfCarPositions = new double[Cars.Length];
				double CenterOfMassPosition = 0.0;
				double TrainMass = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					double pr = Cars[i].RearAxle.Follower.TrackPosition - Cars[i].RearAxle.Position;
					double pf = Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].FrontAxle.Position;
					CenterOfCarPositions[i] = 0.5 * (pr + pf);
					CenterOfMassPosition += CenterOfCarPositions[i] * Cars[i].CurrentMass;
					TrainMass += Cars[i].CurrentMass;
				}
				if (TrainMass != 0.0)
				{
					CenterOfMassPosition /= TrainMass;
				}
				{ // coupler
				  // determine closest cars
					int p = -1; // primary car index
					int s = -1; // secondary car index
					{
						double PrimaryDistance = double.MaxValue;
						for (int i = 0; i < Cars.Length; i++)
						{
							double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
							if (d < PrimaryDistance)
							{
								PrimaryDistance = d;
								p = i;
							}
						}
						double SecondDistance = Double.MaxValue;
						for (int i = p - 1; i <= p + 1; i++)
						{
							if (i >= 0 & i < Cars.Length & i != p)
							{
								double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
								if (d < SecondDistance)
								{
									SecondDistance = d;
									s = i;
								}
							}
						}
						if (s >= 0 && PrimaryDistance <= 0.25 * (PrimaryDistance + SecondDistance))
						{
							s = -1;
						}
					}
					// coupler
					bool[] CouplerCollision = new bool[Cars.Length - 1];
					int cf, cr;
					if (s >= 0)
					{
						// use two cars as center of mass
						if (p > s)
						{
							int t = p; p = s; s = t;
						}
						double min = Cars[p].Coupler.MinimumDistanceBetweenCars;
						double max = Cars[p].Coupler.MaximumDistanceBetweenCars;
						double d = CenterOfCarPositions[p] - CenterOfCarPositions[s] - 0.5 * (Cars[p].Length + Cars[s].Length);
						if (d < min)
						{
							double t = (min - d) / (Cars[p].CurrentMass + Cars[s].CurrentMass);
							double tp = t * Cars[s].CurrentMass;
							double ts = t * Cars[p].CurrentMass;
							Cars[p].UpdateTrackFollowers(tp, false, false);
							Cars[s].UpdateTrackFollowers(-ts, false, false);
							CenterOfCarPositions[p] += tp;
							CenterOfCarPositions[s] -= ts;
							CouplerCollision[p] = true;
						}
						else if (d > max & !Cars[p].Derailed & !Cars[s].Derailed)
						{
							double t = (d - max) / (Cars[p].CurrentMass + Cars[s].CurrentMass);
							double tp = t * Cars[s].CurrentMass;
							double ts = t * Cars[p].CurrentMass;

							Cars[p].UpdateTrackFollowers(-tp, false, false);
							Cars[s].UpdateTrackFollowers(ts, false, false);
							CenterOfCarPositions[p] -= tp;
							CenterOfCarPositions[s] += ts;
							CouplerCollision[p] = true;
						}
						cf = p;
						cr = s;
					}
					else
					{
						// use one car as center of mass
						cf = p;
						cr = p;
					}
					// front cars
					for (int i = cf - 1; i >= 0; i--)
					{
						double min = Cars[i].Coupler.MinimumDistanceBetweenCars;
						double max = Cars[i].Coupler.MaximumDistanceBetweenCars;
						double d = CenterOfCarPositions[i] - CenterOfCarPositions[i + 1] - 0.5 * (Cars[i].Length + Cars[i + 1].Length);
						if (d < min)
						{
							double t = min - d + 0.0001;
							Cars[i].UpdateTrackFollowers(t, false, false);
							CenterOfCarPositions[i] += t;
							CouplerCollision[i] = true;
						}
						else if (d > max & !Cars[i].Derailed & !Cars[i + 1].Derailed)
						{
							double t = d - max + 0.0001;
							Cars[i].UpdateTrackFollowers(-t, false, false);
							CenterOfCarPositions[i] -= t;
							CouplerCollision[i] = true;
						}
					}
					// rear cars
					for (int i = cr + 1; i < Cars.Length; i++)
					{
						double min = Cars[i - 1].Coupler.MinimumDistanceBetweenCars;
						double max = Cars[i - 1].Coupler.MaximumDistanceBetweenCars;
						double d = CenterOfCarPositions[i - 1] - CenterOfCarPositions[i] - 0.5 * (Cars[i].Length + Cars[i - 1].Length);
						if (d < min)
						{
							double t = min - d + 0.0001;
							Cars[i].UpdateTrackFollowers(-t, false, false);
							CenterOfCarPositions[i] -= t;
							CouplerCollision[i - 1] = true;
						}
						else if (d > max & !Cars[i].Derailed & !Cars[i - 1].Derailed)
						{
							double t = d - max + 0.0001;
							Cars[i].UpdateTrackFollowers(t, false, false);

							CenterOfCarPositions[i] += t;
							CouplerCollision[i - 1] = true;
						}
					}
					// update speeds
					for (int i = 0; i < Cars.Length - 1; i++)
					{
						if (CouplerCollision[i])
						{
							int j;
							for (j = i + 1; j < Cars.Length - 1; j++)
							{
								if (!CouplerCollision[j])
								{
									break;
								}
							}
							double v = 0.0;
							double m = 0.0;
							for (int k = i; k <= j; k++)
							{
								v += NewSpeeds[k] * Cars[k].CurrentMass;
								m += Cars[k].CurrentMass;
							}
							if (m != 0.0)
							{
								v /= m;
							}
							for (int k = i; k <= j; k++)
							{
								if (Interface.CurrentOptions.Derailments && Math.Abs(v - NewSpeeds[k]) > 0.5 * CriticalCollisionSpeedDifference)
								{
									Derail(k, TimeElapsed);
								}
								NewSpeeds[k] = v;
							}
							i = j - 1;
						}
					}
				}
				// update average data
				CurrentSpeed = 0.0;
				Specs.CurrentAverageAcceleration = 0.0;
				double invtime = TimeElapsed != 0.0 ? 1.0 / TimeElapsed : 1.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Specs.Acceleration = (NewSpeeds[i] - Cars[i].CurrentSpeed) * invtime;
					Cars[i].CurrentSpeed = NewSpeeds[i];
					CurrentSpeed += NewSpeeds[i];
					Specs.CurrentAverageAcceleration += Cars[i].Specs.Acceleration;
				}
				double invcarlen = 1.0 / (double)Cars.Length;
				CurrentSpeed *= invcarlen;
				Specs.CurrentAverageAcceleration *= invcarlen;
			}

			/// <summary>Updates the safety system plugin for this train</summary>
			internal void UpdateSafetySystem()
			{
				if (Plugin != null)
				{
					SignalData[] data = new SignalData[16];
					int count = 0;
					int start = CurrentSectionIndex >= 0 ? CurrentSectionIndex : 0;
					for (int i = start; i < Program.CurrentRoute.Sections.Length; i++)
					{
						SignalData signal = Program.CurrentRoute.Sections[i].GetPluginSignal(this);
						if (data.Length == count)
						{
							Array.Resize(ref data, data.Length << 1);
						}
						data[count] = signal;
						count++;
						if (signal.Aspect == 0 | count == 16)
						{
							break;
						}
					}
					Array.Resize(ref data, count);
					Plugin.UpdateSignals(data);
					Plugin.LastSection = CurrentSectionIndex;
					Plugin.UpdatePlugin();
				}
			}
			
			

			

			/// <summary>Call this method to derail a car</summary>
			/// <param name="CarIndex">The car index to derail</param>
			/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
			public override void Derail(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Derailed = true;
				this.Derailed = true;
				if (Program.GenerateDebugLogging)
				{
					Program.FileSystem.AppendToLogFile("Train " + Array.IndexOf(TrainManager.Trains, this) + ", Car " + CarIndex + " derailed. Current simulation time: " + Program.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

			/// <inheritdoc/>
			public override void Derail(AbstractCar Car, double ElapsedTime)
			{
				if (this.Cars.Contains(Car))
				{
					var c = Car as CarBase;
					c.Derailed = true;
					this.Derailed = true;
					if (Program.GenerateDebugLogging)
					{
						Program.FileSystem.AppendToLogFile("Train " + Array.IndexOf(TrainManager.Trains, this) + ", Car " + c.Index + " derailed. Current simulation time: " + Program.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
					}
				}
			}

			/// <inheritdoc/>
			public override void Reverse()
			{
				double trackPosition = Cars[0].TrackPosition;
				Cars = Cars.Reverse().ToArray();
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Reverse();
				}
				PlaceCars(trackPosition);
				DriverCar = Cars.Length - 1 - DriverCar;
				UpdateCabObjects();
			}

			

			/// <inheritdoc/>
			public override double FrontCarTrackPosition()
			{
				return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;
			}

			/// <inheritdoc/>
			public override double RearCarTrackPosition()
			{
				return Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition - Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[Cars.Length - 1].Length;
			}

			public override void Jump(int stationIndex)
			{
				SafetySystems.PassAlarm.Halt();
				int currentTrackElement = Cars[0].FrontAxle.Follower.LastTrackElement;
				if (IsPlayerTrain)
				{
					for (int i = 0; i < ObjectManager.AnimatedWorldObjects.Length; i++)
					{
						var obj = ObjectManager.AnimatedWorldObjects[i] as OpenBveApi.Objects.TrackFollowingObject;
						if (obj != null)
						{
							//Track followers should be reset if we jump between stations
							obj.FrontAxleFollower.TrackPosition = ObjectManager.AnimatedWorldObjects[i].TrackPosition + obj.FrontAxlePosition;
							obj.FrontAxleFollower.TrackPosition = ObjectManager.AnimatedWorldObjects[i].TrackPosition + obj.RearAxlePosition;
							obj.FrontAxleFollower.UpdateWorldCoordinates(false);
							obj.RearAxleFollower.UpdateWorldCoordinates(false);
						}

					}
				}

				StationState = TrainStopState.Jumping;
				int stopIndex = Program.CurrentRoute.Stations[stationIndex].GetStopIndex(NumberOfCars);
				if (stopIndex >= 0)
				{
					if (IsPlayerTrain)
					{
						if (Plugin != null)
						{
							Plugin.BeginJump((OpenBveApi.Runtime.InitializationModes) Interface.CurrentOptions.TrainStart);
						}
					}

					for (int h = 0; h < Cars.Length; h++)
					{
						Cars[h].CurrentSpeed = 0.0;
					}

					double d = Program.CurrentRoute.Stations[stationIndex].Stops[stopIndex].TrackPosition - Cars[0].FrontAxle.Follower.TrackPosition + Cars[0].FrontAxle.Position - 0.5 * Cars[0].Length;
					if (IsPlayerTrain)
					{
						SoundsBase.SuppressSoundEvents = true;
					}

					while (d != 0.0)
					{
						double x;
						if (Math.Abs(d) > 1.0)
						{
							x = (double) Math.Sign(d);
						}
						else
						{
							x = d;
						}

						for (int h = 0; h < Cars.Length; h++)
						{
							Cars[h].Move(x);
						}

						if (Math.Abs(d) >= 1.0)
						{
							d -= x;
						}
						else
						{
							break;
						}
					}

					if (IsPlayerTrain)
					{
						TrainManager.UnderailTrains();
						SoundsBase.SuppressSoundEvents = false;
					}

					if (Handles.EmergencyBrake.Driver)
					{
						ApplyNotch(0, false, 0, true);
					}
					else
					{
						ApplyNotch(0, false, Handles.Brake.MaximumNotch, false);
						ApplyAirBrakeHandle(AirBrakeHandleState.Service);
					}

					if (Program.CurrentRoute.Sections.Length > 0)
					{
						Program.CurrentRoute.UpdateAllSections();
					}

					if (IsPlayerTrain)
					{
						if (Game.CurrentScore.ArrivalStation <= stationIndex)
						{
							Game.CurrentScore.ArrivalStation = stationIndex + 1;
						}
					}

					if (IsPlayerTrain)
					{
						if (Program.CurrentRoute.Stations[stationIndex].ArrivalTime >= 0.0)
						{
							Program.CurrentRoute.SecondsSinceMidnight = Program.CurrentRoute.Stations[stationIndex].ArrivalTime;
						}
						else if (Program.CurrentRoute.Stations[stationIndex].DepartureTime >= 0.0)
						{
							Program.CurrentRoute.SecondsSinceMidnight = Program.CurrentRoute.Stations[stationIndex].DepartureTime - Program.CurrentRoute.Stations[stationIndex].StopTime;
						}
					}

					for (int i = 0; i < Cars.Length; i++)
					{
						Cars[i].Doors[0].AnticipatedOpen = Program.CurrentRoute.Stations[stationIndex].OpenLeftDoors;
						Cars[i].Doors[1].AnticipatedOpen = Program.CurrentRoute.Stations[stationIndex].OpenRightDoors;
					}

					if (IsPlayerTrain)
					{
						Game.CurrentScore.DepartureStation = stationIndex;
						Program.Renderer.CurrentInterface = InterfaceType.Normal;
					}

					ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
					TrainManager.UpdateTrainObjects(0.0, true);
					if (IsPlayerTrain)
					{
						if (Plugin != null)
						{
							Plugin.EndJump();
						}
					}

					StationState = TrainStopState.Pending;
					if (IsPlayerTrain)
					{
						LastStation = stationIndex;
					}

					int newTrackElement = Cars[0].FrontAxle.Follower.LastTrackElement;
					if (newTrackElement < currentTrackElement)
					{
						for (int i = newTrackElement; i < currentTrackElement; i++)
						{
							for (int j = 0; j < Program.CurrentHost.Tracks[0].Elements[i].Events.Length; j++)
							{
								Program.CurrentHost.Tracks[0].Elements[i].Events[j].Reset();
							}

						}
					}
				}
			}
		}
	}
}
