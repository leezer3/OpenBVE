using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.SignalManager.PreTrain;
using RouteManager2.Stations;

namespace RouteManager2
{
	public class CurrentRoute
	{

		private readonly HostInterface currentHost;

		private readonly BaseRenderer renderer;

		/// <summary>Holds the information properties of the route</summary>
		public RouteInformation Information;
		/// <summary>The route's comment (For display in the main menu)</summary>
		public string Comment = "";
		/// <summary>The route's image file (For display in the main menu)</summary>
		public string Image = "";

		/// <summary>The list of tracks available in the simulation.</summary>
		public Dictionary<int, Track> Tracks;

		/// <summary>Holds all signal sections within the current route</summary>
		public Section[] Sections;

		/// <summary>Holds all stations within the current route</summary>
		public RouteStation[] Stations;

		/// <summary>The name of the initial station on game startup, if set via command-line arguments</summary>
		public string InitialStationName;
		/// <summary>The start time at the initial station, if set via command-line arguments</summary>
		public double InitialStationTime = -1;

		/// <summary>Holds all .PreTrain instructions for the current route</summary>
		/// <remarks>Must be in distance and time ascending order</remarks>
		public BogusPreTrainInstruction[] BogusPreTrainInstructions;

		public double[] PrecedingTrainTimeDeltas = new double[] { };

		/// <summary>Holds all points of interest within the game world</summary>
		public PointOfInterest[] PointsOfInterest;

		/// <summary>The currently displayed background texture</summary>
		public BackgroundHandle CurrentBackground;

		/// <summary>The new background texture (Currently fading in)</summary>
		public BackgroundHandle TargetBackground;

		/// <summary>The list of dynamic light definitions</summary>
		public LightDefinition[] LightDefinitions;

		/// <summary>Whether dynamic lighting is currently enabled</summary>
		public bool DynamicLighting = false;

		/// <summary>The start of a region without fog</summary>
		/// <remarks>Must not be below the viewing distance (e.g. 600m)</remarks>
		public float NoFogStart;

		/// <summary>The end of a region without fog</summary>
		public float NoFogEnd;

		/// <summary>Holds the previous fog</summary>
		public Fog PreviousFog;

		/// <summary>Holds the current fog</summary>
		public Fog CurrentFog;

		/// <summary>Holds the next fog</summary>
		public Fog NextFog;

		public Atmosphere Atmosphere;

		public double[] BufferTrackPositions = new double[] { };

		/// <summary>The current in game time, expressed as the number of seconds since midnight on the first day</summary>
		public double SecondsSinceMidnight;

		/// <summary>Holds the length conversion units</summary>
		public double[] UnitOfLength = new double[] { 1.0 };

		/// <summary>The length of a block in meters</summary>
		public double BlockLength = 25.0;

		/// <summary>Controls the object disposal mode</summary>
		public ObjectDisposalMode AccurateObjectDisposal;

		public CurrentRoute(HostInterface host, BaseRenderer renderer)
		{
			currentHost = host;
			this.renderer = renderer;
			
			Tracks = new Dictionary<int, Track>();
			Track t = new Track()
			{
				Elements = new TrackElement[0]
			};
			Tracks.Add(0, t);
			Sections = new Section[0];
			Stations = new RouteStation[0];
			BogusPreTrainInstructions = new BogusPreTrainInstruction[0];
			PointsOfInterest = new PointOfInterest[0];
			CurrentBackground = new StaticBackground(null, 6, false);
			TargetBackground = new StaticBackground(null, 6, false);
			NoFogStart = 800.0f;
			NoFogEnd = 1600.0f;
			PreviousFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 0.0);
			CurrentFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 0.5);
			NextFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 1.0);
			Atmosphere = new Atmosphere();
			SecondsSinceMidnight = 0.0;
			Information = new RouteInformation();
			Illustrations.CurrentRoute = this;
		}
		
		/// <summary>Updates all sections within the route</summary>
		public void UpdateAllSections()
		{
			/*
			 * When there are an insane amount of sections, updating via a reference chain
			 * may trigger a StackOverflowException
			 *
			 * Instead, pull out the reference to the next section in an out variable
			 * and use a while loop
			 * https://github.com/leezer3/OpenBVE/issues/557
			 */
			Section nextSectionToUpdate;
			UpdateSection(Sections.LastOrDefault(), out nextSectionToUpdate);
			while (nextSectionToUpdate != null)
			{
				UpdateSection(nextSectionToUpdate, out nextSectionToUpdate);
			}
		}

		/// <summary>Updates the specified signal section</summary>
		/// <param name="SectionIndex"></param>
		public void UpdateSection(int SectionIndex)
		{
			Section nextSectionToUpdate;
			UpdateSection(Sections[SectionIndex], out nextSectionToUpdate);
			while (nextSectionToUpdate != null)
			{
				UpdateSection(nextSectionToUpdate, out nextSectionToUpdate);
			}
		}

		/// <summary>Updates the specified signal section</summary>
		/// <param name="Section"></param>
		/// <param name="PreviousSection"></param>
		public void UpdateSection(Section Section, out Section PreviousSection)
		{
			if (Section == null)
			{
				PreviousSection = null;
				return;
			}

			double timeElapsed = SecondsSinceMidnight - Section.LastUpdate;
			Section.LastUpdate = SecondsSinceMidnight;

			// preparations
			int zeroAspect = 0;
			bool setToRed = false;

			if (Section.Type == SectionType.ValueBased)
			{
				// value-based
				zeroAspect = 0;

				for (int i = 1; i < Section.Aspects.Length; i++)
				{
					if (Section.Aspects[i].Number < Section.Aspects[zeroAspect].Number)
					{
						zeroAspect = i;
					}
				}
			}
			
			// hold station departure signal at red
			int d = Section.StationIndex;

			if (d >= 0)
			{
				// look for train in previous blocks
				Section l = Section.PreviousSection;
				AbstractTrain train = null;

				while (true)
				{
					if (l != null)
					{
						train = l.GetFirstTrain(false);

						if (train != null)
						{
							break;
						}

						l = l.PreviousSection;
					}
					else
					{
						break;
					}
				}

				if (train == null)
				{
					double b = -Double.MaxValue;

					foreach (AbstractTrain t in currentHost.Trains)
					{
						if (t.State == TrainState.Available)
						{
							if (t.TimetableDelta > b)
							{
								b = t.TimetableDelta;
								train = t;
							}
						}
					}
				}

				// set to red where applicable
				if (train != null)
				{
					if (!Section.TrainReachedStopPoint)
					{
						if (train.Station == d)
						{
							int c = Stations[d].GetStopIndex(train.NumberOfCars);

							if (c >= 0)
							{
								double p0 = train.FrontCarTrackPosition();
								double p1 = Stations[d].Stops[c].TrackPosition - Stations[d].Stops[c].BackwardTolerance;

								if (p0 >= p1)
								{
									Section.TrainReachedStopPoint = true;
								}
							}
							else
							{
								Section.TrainReachedStopPoint = true;
							}
						}
					}

					double t = -15.0;

					if (Stations[d].DepartureTime >= 0.0)
					{
						t = Stations[d].DepartureTime - 15.0;
					}
					else if (Stations[d].ArrivalTime >= 0.0)
					{
						t = Stations[d].ArrivalTime;
					}

					if (AccurateObjectDisposal == ObjectDisposalMode.Mechanik)
					{
						if (train.LastStation == d - 1 || train.Station == d)
						{
							if (Section.RedTimer == -1)
							{
								Section.RedTimer = 30;
							}
							else
							{
								Section.RedTimer -= timeElapsed;
							}

							setToRed = !(Section.RedTimer <= 0);
						}
						else
						{
							Section.RedTimer = -1;
						}
					}

					if (train.IsPlayerTrain & Stations[d].Type != StationType.Normal & Stations[d].DepartureTime < 0.0)
					{
						setToRed = true;
					}
					else if (t >= 0.0 & SecondsSinceMidnight < t - train.TimetableDelta)
					{
						setToRed = true;
					}
					else if (!Section.TrainReachedStopPoint)
					{
						setToRed = true;
					}
				}
				else if (Stations[d].Type != StationType.Normal)
				{
					setToRed = true;
				}
			}

			// train in block
			if (!Section.IsFree())
			{
				setToRed = true;
			}

			// free sections
			int newAspect = -1;

			if (setToRed)
			{
				Section.FreeSections = 0;
				newAspect = zeroAspect;
			}
			else
			{
				Section n = Section.NextSection;

				if (n != null)
				{
					if (n.FreeSections == -1)
					{
						Section.FreeSections = -1;
					}
					else
					{
						Section.FreeSections = n.FreeSections + 1;
					}
				}
				else
				{
					Section.FreeSections = -1;
				}
			}

			// change aspect
			if (newAspect == -1)
			{
				if (Section.Type == SectionType.ValueBased)
				{
					// value-based
					Section n = Section.NextSection;
					int a = Section.Aspects.Last().Number;

					if (n != null && n.CurrentAspect >= 0)
					{

						a = n.Aspects[n.CurrentAspect].Number;
					}

					for (int i = Section.Aspects.Length - 1; i >= 0; i--)
					{
						if (Section.Aspects[i].Number > a)
						{
							newAspect = i;
						}
					}

					if (newAspect == -1)
					{
						newAspect = Section.Aspects.Length - 1;
					}
				}
				else
				{
					// index-based
					if (Section.FreeSections >= 0 & Section.FreeSections < Section.Aspects.Length)
					{
						newAspect = Section.FreeSections;
					}
					else
					{
						newAspect = Section.Aspects.Length - 1;
					}
				}
			}

			// apply new aspect
			Section.CurrentAspect = newAspect;

			// update previous section
			PreviousSection = Section.PreviousSection;
		}

		/// <summary>Gets the next section from the specified track position</summary>
		/// <param name="trackPosition">The track position</param>
		public Section NextSection(double trackPosition)
		{
			if (Sections == null || Sections.Length < 2)
			{
				return null;
			}
			for (int i = 1; i < Sections.Length; i++)
			{
				if (Sections[i].TrackPosition > trackPosition && Sections[i -1].TrackPosition <= trackPosition)
				{
					return Sections[i];
				}
			}
			return null;
		}

		/// <summary>Gets the next station from the specified track position</summary>
		/// <param name="trackPosition">The track position</param>
		public RouteStation NextStation(double trackPosition)
		{
			if (Stations == null || Stations.Length == 0)
			{
				return null;
			}
			for (int i = 1; i < Stations.Length; i++)
			{
				if (Stations[i].DefaultTrackPosition > trackPosition && Stations[i -1].DefaultTrackPosition <= trackPosition)
				{
					return Stations[i];
				}
			}
			return null;
		}

		/// <summary>Updates the currently displayed background</summary>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
		/// <param name="GamePaused">Whether the game is currently paused</param>
		public void UpdateBackground(double TimeElapsed, bool GamePaused)
		{
			if (GamePaused)
			{
				//Don't update the transition whilst paused
				TimeElapsed = 0.0;
			}

			const float scale = 0.5f;

			// fog
			const float fogDistance = 600.0f;

			if (CurrentFog.Start < CurrentFog.End & CurrentFog.Start < fogDistance)
			{
				float ratio = (float)CurrentBackground.BackgroundImageDistance / fogDistance;

				renderer.OptionFog = true;
				renderer.Fog.Start = CurrentFog.Start * ratio * scale;
				renderer.Fog.End = CurrentFog.End * ratio * scale;
				renderer.Fog.Color = CurrentFog.Color;
				renderer.Fog.Density = CurrentFog.Density;
				renderer.Fog.IsLinear = CurrentFog.IsLinear;
				renderer.Fog.SetForImmediateMode();
			}
			else
			{
				renderer.OptionFog = false;
			}

			//Update the currently displayed background
			CurrentBackground.UpdateBackground(SecondsSinceMidnight, TimeElapsed, false);

			if (TargetBackground == null || TargetBackground == CurrentBackground)
			{
				//No target background, so call the render function
				renderer.Background.Render(CurrentBackground, scale);
				return;
			}

			//Update the target background
			if (TargetBackground is StaticBackground)
			{
				TargetBackground.Countdown += TimeElapsed;
			}

			TargetBackground.UpdateBackground(SecondsSinceMidnight, TimeElapsed, true);

			switch (TargetBackground.Mode)
			{
				//Render, switching on the transition mode
				case BackgroundTransitionMode.FadeIn:
					renderer.Background.Render(CurrentBackground, 1.0f, scale);
					renderer.Background.Render(TargetBackground, TargetBackground.CurrentAlpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					renderer.Background.Render(TargetBackground, 1.0f, scale);
					renderer.Background.Render(CurrentBackground, TargetBackground.CurrentAlpha, scale);
					break;
			}

			//If our target alpha is greater than or equal to 1.0f, the background is fully displayed
			if (TargetBackground.CurrentAlpha >= 1.0f)
			{
				//Set the current background to the target & reset target to null
				CurrentBackground = TargetBackground;
				TargetBackground = null;
			}
		}
	}
}
