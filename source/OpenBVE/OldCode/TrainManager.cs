using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using OpenBve.Parsers.Panel;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		// trains
		/// <summary>The list of trains available in the simulation.</summary>
		internal static Train[] Trains = new Train[] { };
		/// <summary>A reference to the train of the Trains element that corresponds to the player's train.</summary>
		internal static Train PlayerTrain = null;
		/// <summary>The list of TrackFollowingObject available on other tracks in the simulation.</summary>
		internal static TrackFollowingObject[] TFOs = new TrackFollowingObject[] { };

		/// <summary>Attempts to load and parse the current train's panel configuration file.</summary>
		/// <param name="TrainPath">The absolute on-disk path to the train folder.</param>
		/// <param name="Encoding">The automatically detected or manually set encoding of the panel configuration file.</param>
		/// <param name="Train">The base train on which to apply the panel configuration.</param>
		internal static void ParsePanelConfig(string TrainPath, System.Text.Encoding Encoding, Train Train)
		{
			Train.Cars[Train.DriverCar].CarSections = new CarSection[1];
			Train.Cars[Train.DriverCar].CarSections[0] = new CarSection
			{
				Groups = new ElementsGroup[1]
			};
			Train.Cars[Train.DriverCar].CarSections[0].Groups[0] = new ElementsGroup
			{
				Elements = new AnimatedObject[] { },
				Overlay = true
			};
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
							PanelAnimatedXmlParser.ParsePanelAnimatedXml(System.IO.Path.GetFileName(File), TrainPath, Train, Train.DriverCar);
							Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
						}

						DocumentElements = CurrentXML.Root.Elements("Panel");
						if (DocumentElements.Any())
						{
							PanelXmlParser.ParsePanelXml(System.IO.Path.GetFileName(File), TrainPath, Train, Train.DriverCar);
							Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
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

				if (Train.Cars[Train.DriverCar].CarSections[0].Groups[0].Elements.Any())
				{
					Program.Renderer.InitializeVisibility();
					World.UpdateViewingDistances();
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
							Train.Cars[Train.DriverCar].CarSections[0].Groups[0].Elements = a.Objects;
							Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.NotAvailable;
							Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
							World.UpdateViewingDistances();
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
					Panel2CfgParser.ParsePanel2Config("panel2.cfg", TrainPath, Encoding, Train, Train.DriverCar);
					Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
					Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.On;
				}
				else
				{
					File = OpenBveApi.Path.CombineFile(TrainPath, "panel.cfg");
					if (System.IO.File.Exists(File))
					{
						Program.FileSystem.AppendToLogFile("Loading train panel: " + File);
						PanelCfgParser.ParsePanelConfig(TrainPath, Encoding, Train);
						Train.Cars[Train.DriverCar].CameraRestrictionMode = CameraRestrictionMode.On;
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
				currentError = currentError.Replace("[file]",Panel2 ? "panel2.cfg" : "panel.cfg");
				MessageBox.Show(currentError, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Program.RestartArguments = " ";
				Loading.Cancel = true;
			}
		}
		
		// get resistance
		private static double GetResistance(Train Train, int CarIndex, ref Axle Axle, double Speed)
		{
			double t;
			if (CarIndex == 0 & Train.Cars[CarIndex].CurrentSpeed >= 0.0 || CarIndex == Train.Cars.Length - 1 & Train.Cars[CarIndex].CurrentSpeed <= 0.0)
			{
				t = Train.Cars[CarIndex].Specs.ExposedFrontalArea;
			}
			else
			{
				t = Train.Cars[CarIndex].Specs.UnexposedFrontalArea;
			}
			double f = t * Train.Cars[CarIndex].Specs.AerodynamicDragCoefficient * Train.Specs.CurrentAirDensity / (2.0 * Train.Cars[CarIndex].Specs.MassCurrent);
			double a = Program.CurrentRoute.Atmosphere.AccelerationDueToGravity * Train.Cars[CarIndex].Specs.CoefficientOfRollingResistance + f * Speed * Speed;
			return a;
		}

		// get critical wheelslip acceleration
		private static double GetCriticalWheelSlipAccelerationForElectricMotor(Train Train, int CarIndex, double AdhesionMultiplier, double UpY, double Speed)
		{
			double NormalForceAcceleration = UpY * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			// TODO: Implement formula that depends on speed here.
			double coefficient = Train.Cars[CarIndex].Specs.CoefficientOfStaticFriction;
			return coefficient * AdhesionMultiplier * NormalForceAcceleration;
		}
		private static double GetCriticalWheelSlipAccelerationForFrictionBrake(Train Train, int CarIndex, double AdhesionMultiplier, double UpY, double Speed)
		{
			double NormalForceAcceleration = UpY * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			// TODO: Implement formula that depends on speed here.
			double coefficient = Train.Cars[CarIndex].Specs.CoefficientOfStaticFriction;
			return coefficient * AdhesionMultiplier * NormalForceAcceleration;
		}

		
		/// <summary>Updates the objects for all trains within the simulation world</summary>
		/// <param name="TimeElapsed">The time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update</param>
		internal static void UpdateTrainObjects(double TimeElapsed, bool ForceUpdate)
		{
			for (int i = 0; i < Trains.Length; i++)
			{
				Trains[i].UpdateObjects(TimeElapsed, ForceUpdate);
			}

			foreach (var Train in TFOs)
			{
				Train.UpdateObjects(TimeElapsed, ForceUpdate);
			}
		}

		
		
		

		

		/// <summary>This method should be called once a frame to update the position, speed and state of all trains within the simulation</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		internal static void UpdateTrains(double TimeElapsed)
		{
			for (int i = 0; i < Trains.Length; i++) {
				Trains[i].Update(TimeElapsed);
			}

			foreach (var Train in TFOs)
			{
				Train.Update(TimeElapsed);
			}

			// detect collision
			if (!Game.MinimalisticSimulation & Interface.CurrentOptions.Collisions)
			{
				
				//for (int i = 0; i < Trains.Length; i++) {
				System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
				{
					// with other trains
					if (Trains[i].State == TrainState.Available)
					{
						double a = Trains[i].FrontCarTrackPosition();
						double b = Trains[i].RearCarTrackPosition();
						for (int j = i + 1; j < Trains.Length; j++)
						{
							if (Trains[j].State == TrainState.Available)
							{
								double c = Trains[j].FrontCarTrackPosition();
								double d = Trains[j].RearCarTrackPosition();
								if (a > d & b < c)
								{
									if (a > c)
									{
										// i > j
										int k = Trains[i].Cars.Length - 1;
										if (Trains[i].Cars[k].CurrentSpeed < Trains[j].Cars[0].CurrentSpeed)
										{
											double v = Trains[j].Cars[0].CurrentSpeed - Trains[i].Cars[k].CurrentSpeed;
											double s = (Trains[i].Cars[k].CurrentSpeed*Trains[i].Cars[k].Specs.MassCurrent +
														Trains[j].Cars[0].CurrentSpeed*Trains[j].Cars[0].Specs.MassCurrent)/
													   (Trains[i].Cars[k].Specs.MassCurrent + Trains[j].Cars[0].Specs.MassCurrent);
											Trains[i].Cars[k].CurrentSpeed = s;
											Trains[j].Cars[0].CurrentSpeed = s;
											double e = 0.5*(c - b) + 0.0001;
											Trains[i].Cars[k].FrontAxle.Follower.UpdateRelative(e, false, false);
											Trains[i].Cars[k].RearAxle.Follower.UpdateRelative(e, false, false);
											Trains[j].Cars[0].FrontAxle.Follower.UpdateRelative(-e, false, false);

											Trains[j].Cars[0].RearAxle.Follower.UpdateRelative(-e, false, false);
											if (Interface.CurrentOptions.Derailments)
											{
												double f = 2.0/ (Trains[i].Cars[k].Specs.MassCurrent + Trains[j].Cars[0].Specs.MassCurrent);
												double fi = Trains[j].Cars[0].Specs.MassCurrent*f;
												double fj = Trains[i].Cars[k].Specs.MassCurrent*f;
												double vi = v*fi;
												double vj = v*fj;
												if (vi > Game.CriticalCollisionSpeedDifference)
													Trains[i].Derail(k, TimeElapsed);
												if (vj > Game.CriticalCollisionSpeedDifference)
													Trains[j].Derail(i, TimeElapsed);
											}
											// adjust cars for train i
											for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h + 1].FrontAxle.Position + 0.5*Trains[i].Cars[h + 1].Length;
												b = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
													Trains[i].Cars[h].RearAxle.Position - 0.5*Trains[i].Cars[h].Length;
												d = b - a - Trains[i].Cars[h].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[i].Cars[h].FrontAxle.Follower.UpdateRelative(-d, false, false);
													Trains[i].Cars[h].RearAxle.Follower.UpdateRelative(-d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/ (Trains[i].Cars[h + 1].Specs.MassCurrent + Trains[i].Cars[h].Specs.MassCurrent);
														double fi = Trains[i].Cars[h + 1].Specs.MassCurrent*f;
														double fj = Trains[i].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h + 1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h, TimeElapsed);
													}
													Trains[i].Cars[h].CurrentSpeed =
														Trains[i].Cars[h + 1].CurrentSpeed;
												}
											}
											// adjust cars for train j
											for (int h = 1; h < Trains[j].Cars.Length; h++)
											{
												a = Trains[j].Cars[h - 1].RearAxle.Follower.TrackPosition -
													Trains[j].Cars[h - 1].RearAxle.Position - 0.5*Trains[j].Cars[h - 1].Length;
												b = Trains[j].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h].FrontAxle.Position + 0.5*Trains[j].Cars[h].Length;
												d = a - b - Trains[j].Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[j].Cars[h].FrontAxle.Follower.UpdateRelative(d, false, false);
													Trains[j].Cars[h].RearAxle.Follower.UpdateRelative(d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/ (Trains[j].Cars[h - 1].Specs.MassCurrent + Trains[j].Cars[h].Specs.MassCurrent);
														double fi = Trains[j].Cars[h - 1].Specs.MassCurrent*f;
														double fj = Trains[j].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h -1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h, TimeElapsed);
													}
													Trains[j].Cars[h].CurrentSpeed =
														Trains[j].Cars[h - 1].CurrentSpeed;
												}
											}
										}
									}
									else
									{
										// i < j
										int k = Trains[j].Cars.Length - 1;
										if (Trains[i].Cars[0].CurrentSpeed > Trains[j].Cars[k].CurrentSpeed)
										{
											double v = Trains[i].Cars[0].CurrentSpeed -
													   Trains[j].Cars[k].CurrentSpeed;
											double s = (Trains[i].Cars[0].CurrentSpeed*Trains[i].Cars[0].Specs.MassCurrent +
														Trains[j].Cars[k].CurrentSpeed*Trains[j].Cars[k].Specs.MassCurrent)/
													   (Trains[i].Cars[0].Specs.MassCurrent + Trains[j].Cars[k].Specs.MassCurrent);
											Trains[i].Cars[0].CurrentSpeed = s;
											Trains[j].Cars[k].CurrentSpeed = s;
											double e = 0.5*(a - d) + 0.0001;
											Trains[i].Cars[0].FrontAxle.Follower.UpdateRelative(-e, false, false);
											Trains[i].Cars[0].RearAxle.Follower.UpdateRelative(-e, false, false);
											Trains[j].Cars[k].FrontAxle.Follower.UpdateRelative(e, false, false);
											Trains[j].Cars[k].RearAxle.Follower.UpdateRelative(e, false, false);
											if (Interface.CurrentOptions.Derailments)
											{
												double f = 2.0/ (Trains[i].Cars[0].Specs.MassCurrent + Trains[j].Cars[k].Specs.MassCurrent);
												double fi = Trains[j].Cars[k].Specs.MassCurrent*f;
												double fj = Trains[i].Cars[0].Specs.MassCurrent*f;
												double vi = v*fi;
												double vj = v*fj;
												if (vi > Game.CriticalCollisionSpeedDifference)
													Trains[i].Derail(0, TimeElapsed);
												if (vj > Game.CriticalCollisionSpeedDifference)
													Trains[j].Derail(k, TimeElapsed);
											}
											// adjust cars for train i
											for (int h = 1; h < Trains[i].Cars.Length; h++)
											{
												a = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
													Trains[i].Cars[h - 1].RearAxle.Position - 0.5*Trains[i].Cars[h - 1].Length;
												b = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h].FrontAxle.Position + 0.5*Trains[i].Cars[h].Length;
												d = a - b - Trains[i].Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[i].Cars[h].FrontAxle.Follower.UpdateRelative(d, false, false);
													Trains[i].Cars[h].RearAxle.Follower.UpdateRelative(d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/ (Trains[i].Cars[h - 1].Specs.MassCurrent + Trains[i].Cars[h].Specs.MassCurrent);
														double fi = Trains[i].Cars[h - 1].Specs.MassCurrent*f;
														double fj = Trains[i].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h -1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h, TimeElapsed);
													}
													Trains[i].Cars[h].CurrentSpeed =
														Trains[i].Cars[h - 1].CurrentSpeed;
												}
											}
											// adjust cars for train j
											for (int h = Trains[j].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[j].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h + 1].FrontAxle.Position + 0.5*Trains[j].Cars[h + 1].Length;
												b = Trains[j].Cars[h].RearAxle.Follower.TrackPosition -
													Trains[j].Cars[h].RearAxle.Position - 0.5*Trains[j].Cars[h].Length;
												d = b - a - Trains[j].Cars[h].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[j].Cars[h].FrontAxle.Follower.UpdateRelative(-d, false, false);
													Trains[j].Cars[h].RearAxle.Follower.UpdateRelative(-d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/ (Trains[j].Cars[h + 1].Specs.MassCurrent + Trains[j].Cars[h].Specs.MassCurrent);
														double fi = Trains[j].Cars[h + 1].Specs.MassCurrent*f;
														double fj = Trains[j].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h + 1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h, TimeElapsed);
													}
													Trains[j].Cars[h].CurrentSpeed =
														Trains[j].Cars[h + 1].CurrentSpeed;
												}
											}
										}
									}
								}
							}

						}
					}
					// with buffers
					if (Trains[i].IsPlayerTrain)
					{
						double a = Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - Trains[i].Cars[0].FrontAxle.Position +
								   0.5*Trains[i].Cars[0].Length;
						double b = Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
								   Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Position - 0.5*Trains[i].Cars[0].Length;
						for (int j = 0; j < Game.BufferTrackPositions.Length; j++)
						{
							if (a > Game.BufferTrackPositions[j] & b < Game.BufferTrackPositions[j])
							{
								a += 0.0001;
								b -= 0.0001;
								double da = a - Game.BufferTrackPositions[j];
								double db = Game.BufferTrackPositions[j] - b;
								if (da < db)
								{
									// front
									Trains[i].Cars[0].UpdateTrackFollowers(-da, false, false);
									if (Interface.CurrentOptions.Derailments &&
										Math.Abs(Trains[i].Cars[0].CurrentSpeed) > Game.CriticalCollisionSpeedDifference)
									{
										Trains[i].Derail(0, TimeElapsed);
									}
									Trains[i].Cars[0].CurrentSpeed = 0.0;
									for (int h = 1; h < Trains[i].Cars.Length; h++)
									{
										a = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
											Trains[i].Cars[h - 1].RearAxle.Position - 0.5*Trains[i].Cars[h - 1].Length;
										b = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
											Trains[i].Cars[h].FrontAxle.Position + 0.5*Trains[i].Cars[h].Length;
										double d = a - b - Trains[i].Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
										if (d < 0.0)
										{
											d -= 0.0001;
											Trains[i].Cars[h].UpdateTrackFollowers(d, false, false);
											if (Interface.CurrentOptions.Derailments &&
												Math.Abs(Trains[i].Cars[h].CurrentSpeed) >
												Game.CriticalCollisionSpeedDifference)
											{
												Trains[i].Derail(h, TimeElapsed);
											}
											Trains[i].Cars[h].CurrentSpeed = 0.0;
										}
									}
								}
								else
								{
									// rear
									int c = Trains[i].Cars.Length - 1;
									Trains[i].Cars[c].UpdateTrackFollowers(db, false, false);
									if (Interface.CurrentOptions.Derailments &&
										Math.Abs(Trains[i].Cars[c].CurrentSpeed) > Game.CriticalCollisionSpeedDifference)
									{
										Trains[i].Derail(c, TimeElapsed);
									}
									Trains[i].Cars[c].CurrentSpeed = 0.0;
									for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
									{
										a = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
											Trains[i].Cars[h + 1].FrontAxle.Position + 0.5*Trains[i].Cars[h + 1].Length;
										b = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
											Trains[i].Cars[h].RearAxle.Position - 0.5*Trains[i].Cars[h].Length;
										double d = b - a - Trains[i].Cars[h].Coupler.MinimumDistanceBetweenCars;
										if (d < 0.0)
										{
											d -= 0.0001;
											Trains[i].Cars[h].UpdateTrackFollowers(-d, false, false);
											if (Interface.CurrentOptions.Derailments &&
												Math.Abs(Trains[i].Cars[h].CurrentSpeed) >
												Game.CriticalCollisionSpeedDifference)
											{
												Trains[i].Derail(h, TimeElapsed);
											}
											Trains[i].Cars[h].CurrentSpeed = 0.0;
										}
									}
								}
							}
						}
					}
				});
			}
			// compute final angles and positions
			//for (int i = 0; i < Trains.Length; i++) {
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				if (Trains[i].State != TrainState.Disposed & Trains[i].State != TrainState.Bogus)
				{
					for (int j = 0; j < Trains[i].Cars.Length; j++)
					{
						Trains[i].Cars[j].FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
						{
							//Don't update the toppling etc. with excessive or no time
							continue;
						}
						Trains[i].Cars[j].UpdateTopplingCantAndSpring(TimeElapsed);
						Trains[i].Cars[j].FrontBogie.UpdateTopplingCantAndSpring();
						Trains[i].Cars[j].RearBogie.UpdateTopplingCantAndSpring();
					}
				}
			});

			System.Threading.Tasks.Parallel.For(0, TFOs.Length, i =>
			{
				if (TFOs[i].State != TrainState.Disposed & TFOs[i].State != TrainState.Bogus)
				{
					foreach (var Car in TFOs[i].Cars)
					{
						Car.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
						{
							//Don't update the toppling etc. with excessive or no time
							continue;
						}
						Car.UpdateTopplingCantAndSpring(TimeElapsed);
						Car.FrontBogie.UpdateTopplingCantAndSpring();
						Car.RearBogie.UpdateTopplingCantAndSpring();
					}
				}
			});
		}
	}
}
