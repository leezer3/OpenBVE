using System;
using System.Drawing;
using OpenBveApi;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static class Timetable {

		// members (built-in timetable)
		internal static string DefaultTimetableDescription = "";
		internal static Texture DefaultTimetableTexture;
		internal static double DefaultTimetablePosition = 0.0;
		
		// members (custom timetable)
		internal static ObjectManager.AnimatedObject[] CustomObjects = new ObjectManager.AnimatedObject[16];
		internal static int CustomObjectsUsed;
		internal static bool CustomTimetableAvailable;
		internal static Texture CurrentCustomTimetableDaytimeTexture;
		internal static Texture CurrentCustomTimetableNighttimeTexture;
		internal static double CustomTimetablePosition = 0.0;
		
		// members (interface)
		internal enum TimetableState {
			None = 0,
			Custom = 1,
			Default = 2
		}
		/// <summary>Holds the currently displayed timetable state</summary>
		internal static TimetableState CurrentTimetable = TimetableState.None;

		// data
		internal struct Time {
			internal string Hour;
			internal string Minute;
			internal string Second;
		}
		internal struct Station {
			internal string Name;
			internal bool NameJapanese;
			internal Time Arrival;
			internal Time Departure;
			internal bool Pass;
			internal bool Terminal;
		}
		internal struct Track {
			internal Time Time;
			internal string Speed;
		}

		internal struct Table
		{
			internal Station[] Stations;
			internal Track[] Tracks;

			/// <summary>Collects the timetable data for the current route</summary>
			internal void CollectData()
			{
				System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
				Stations = new Station[16];
				Tracks = new Track[16];
				int n = 0;
				double Limit = -1.0, LastLimit = 6.94444444444444;
				int LastArrivalHours = -1, LastDepartureHours = -1;
				double LastTime = -1.0;
				for (int i = 0; i < TrackManager.Tracks[0].Elements.Length; i++)
				{
					for (int j = 0; j < TrackManager.Tracks[0].Elements[i].Events.Length; j++)
					{
						TrackManager.StationStartEvent sse = TrackManager.Tracks[0].Elements[i].Events[j] as TrackManager.StationStartEvent;
						if (sse != null && Game.Stations[sse.StationIndex].Name != string.Empty)
						{
							if (Limit == -1.0) Limit = LastLimit;
							// update station
							if (n == Stations.Length)
							{
								Array.Resize<Station>(ref Stations, Stations.Length << 1);
							}

							Stations[n].Name = Game.Stations[sse.StationIndex].Name;
							Stations[n].NameJapanese = Game.Stations[sse.StationIndex].Name.IsJapanese();
							Stations[n].Pass = !Game.PlayerStopsAtStation(sse.StationIndex);
							Stations[n].Terminal = Game.Stations[sse.StationIndex].Type != StationType.Normal;
							double x;
							if (Game.Stations[sse.StationIndex].ArrivalTime >= 0.0)
							{
								x = Game.Stations[sse.StationIndex].ArrivalTime;
								x -= 86400.0 * Math.Floor(x / 86400.0);
								int hours = (int) Math.Floor(x / 3600.0);
								x -= 3600.0 * (double) hours;
								int minutes = (int) Math.Floor(x / 60.0);
								x -= 60.0 * (double) minutes;
								int seconds = (int) Math.Floor(x);
								Stations[n].Arrival.Hour = hours != LastArrivalHours ? hours.ToString("00", Culture) : "";
								Stations[n].Arrival.Minute = minutes.ToString("00", Culture);
								Stations[n].Arrival.Second = seconds.ToString("00", Culture);
								LastArrivalHours = hours;
							}
							else
							{
								Stations[n].Arrival.Hour = "";
								Stations[n].Arrival.Minute = "";
								Stations[n].Arrival.Second = "";
							}

							if (Game.Stations[sse.StationIndex].DepartureTime >= 0.0)
							{
								x = Game.Stations[sse.StationIndex].DepartureTime;
								x -= 86400.0 * Math.Floor(x / 86400.0);
								int hours = (int) Math.Floor(x / 3600.0);
								x -= 3600.0 * (double) hours;
								int minutes = (int) Math.Floor(x / 60.0);
								x -= 60.0 * (double) minutes;
								int seconds = (int) Math.Floor(x);
								Stations[n].Departure.Hour = hours != LastDepartureHours ? hours.ToString("00", Culture) : "";
								Stations[n].Departure.Minute = minutes.ToString("00", Culture);
								Stations[n].Departure.Second = seconds.ToString("00", Culture);
								LastDepartureHours = hours;
							}
							else
							{
								Stations[n].Departure.Hour = "";
								Stations[n].Departure.Minute = "";
								Stations[n].Departure.Second = "";
							}

							// update track
							if (n >= 1)
							{
								int m = n - 1;
								if (m == Tracks.Length)
								{
									Array.Resize<Track>(ref Tracks, Tracks.Length << 1);
								}

								// speed
								x = Math.Round(3.6 * Limit);
								Tracks[m].Speed = x.ToString(Culture);
								// time
								if (LastTime >= 0.0)
								{
									if (Game.Stations[sse.StationIndex].ArrivalTime >= 0.0)
									{
										x = Game.Stations[sse.StationIndex].ArrivalTime;
									}
									else if (Game.Stations[sse.StationIndex].DepartureTime >= 0.0)
									{
										x = Game.Stations[sse.StationIndex].DepartureTime;
									}
									else x = -1.0;

									if (x >= 0.0)
									{
										x -= LastTime;
										int hours = (int) Math.Floor(x / 3600.0);
										x -= 3600.0 * (double) hours;
										int minutes = (int) Math.Floor(x / 60.0);
										x -= 60.0 * (double) minutes;
										int seconds = (int) Math.Floor(x);
										Tracks[m].Time.Hour = hours != 0 ? hours.ToString("0", Culture) : "";
										Tracks[m].Time.Minute = minutes != 0 ? minutes.ToString("00", Culture) : "";
										Tracks[m].Time.Second = seconds != 0 ? seconds.ToString("00", Culture) : "";
									}
									else
									{
										Tracks[m].Time.Hour = "";
										Tracks[m].Time.Minute = "";
										Tracks[m].Time.Second = "";
									}
								}
								else
								{
									Tracks[m].Time.Hour = "";
									Tracks[m].Time.Minute = "";
									Tracks[m].Time.Second = "";
								}
							}

							// update last data
							if (Game.Stations[sse.StationIndex].DepartureTime >= 0.0)
							{
								LastTime = Game.Stations[sse.StationIndex].DepartureTime;
							}
							else if (Game.Stations[sse.StationIndex].ArrivalTime >= 0.0)
							{
								LastTime = Game.Stations[sse.StationIndex].ArrivalTime;
							}
							else
							{
								LastTime = -1.0;
							}

							LastLimit = Limit;
							Limit = -1.0;
							n++;
						}

						if (n >= 1)
						{
							TrackManager.LimitChangeEvent lce = TrackManager.Tracks[0].Elements[i].Events[j] as TrackManager.LimitChangeEvent;
							if (lce != null)
							{
								if (lce.NextSpeedLimit != double.PositiveInfinity & lce.NextSpeedLimit > Limit) Limit = lce.NextSpeedLimit;
							}
						}
					}
				}

				Array.Resize<Station>(ref Stations, n);
				if (n >= 2)
				{
					Array.Resize<Track>(ref Tracks, n - 1);
				}
				else
				{
					Tracks = new Track[] { };
				}
			}

			/// <summary>Renders the timetable data</summary>
			/// <param name="timetableTexture">The texture to create</param>
			internal void RenderData(ref Texture timetableTexture)
			{
				// prepare timetable
				int w = 384, h = 192;
				int offsetx = 0;
				int actualheight = h;
				float descriptionwidth = 256;
				float descriptionheight = 16;
				float stationnamewidth = 16;
				for (int k = 0; k < 2; k++)
				{
					Bitmap b = new Bitmap(w, h);
					Graphics g = Graphics.FromImage(b);
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
					g.Clear(Color.Transparent);
					g.FillRectangle(Brushes.White, new RectangleF(offsetx, 0, w, actualheight));
					Font f = new Font(FontFamily.GenericSansSerif, 13.0f, GraphicsUnit.Pixel);
					Font fs = new Font(FontFamily.GenericSansSerif, 11.0f, GraphicsUnit.Pixel);
					Font fss = new Font(FontFamily.GenericSansSerif, 9.0f, GraphicsUnit.Pixel);
					// draw timetable
					string t;
					SizeF s;
					// description
					float x0 = offsetx + 8;
					float y0 = 8;
					if (k == 1)
					{
						t = DefaultTimetableDescription;
						g.DrawString(t, f, Brushes.Black, new RectangleF(x0, 6, descriptionwidth, descriptionheight + 8));
						y0 += descriptionheight + 2;
					}

					// highest speed
					t = Translations.GetInterfaceString("timetable_highestspeed");
					s = g.MeasureString(t, fs);
					g.DrawString(t, fs, Brushes.Black, x0, y0);
					float y0a = y0 + s.Height + 2;
					float x1 = x0 + s.Width + 4;
					for (int i = 0; i < Tracks.Length; i++)
					{
						float y = y0a + 18 * i;
						t = Tracks[i].Speed;
						g.DrawString(t, f, Brushes.Black, x0, y);
						s = g.MeasureString(t, f);
						float x = x0 + s.Width + 4;
						if (x > x1) x1 = x;
					}

					g.DrawLine(Pens.LightGray, new PointF(x1 - 2, 4 + descriptionheight), new PointF(x1 - 2, y0a + 18 * Tracks.Length - 1));
					// driving time
					t = Translations.GetInterfaceString("timetable_drivingtime");
					s = g.MeasureString(t, fs);
					g.DrawString(t, fs, Brushes.Black, x1, y0);
					float x2 = x1 + s.Width + 4;
					for (int i = 0; i < Tracks.Length; i++)
					{
						float y = y0a + 18 * i;
						if (Tracks[i].Time.Hour.Length != 0)
						{
							t = Tracks[i].Time.Hour;
							g.DrawString(t, fss, Brushes.Black, x1, y + 2);
						}
						else
						{
							t = "0";
						}

						s = g.MeasureString(t, fss, 9999, StringFormat.GenericTypographic);
						float x = x1 + s.Width - 1;
						if (Tracks[i].Time.Minute.Length != 0)
						{
							t = Tracks[i].Time.Minute;
							g.DrawString(t, fs, Brushes.Black, x, y + 2);
						}
						else
						{
							t = "00:";
						}

						s = g.MeasureString(t, fs, 9999, StringFormat.GenericTypographic);
						x += s.Width + 1;
						t = Tracks[i].Time.Second;
						g.DrawString(t, fss, Brushes.Black, x, y + 2);
						s = g.MeasureString(t, fss, 9999, StringFormat.GenericTypographic);
						x += s.Width + 8;
						if (x > x2) x2 = x;
					}

					for (int i = 0; i < Tracks.Length; i++)
					{
						float y = y0a + 18 * i;
						g.DrawLine(Pens.LightGray, new PointF(offsetx + 4, y - 1), new PointF(x2 - 2, y - 1));
					}

					g.DrawLine(Pens.LightGray, new PointF(x2 - 2, 4 + descriptionheight), new PointF(x2 - 2, y0a + 18 * Tracks.Length - 1));
					// station name
					float y2 = y0;
					t = Translations.GetInterfaceString("timetable_stationname");
					s = g.MeasureString(t, f);
					g.DrawString(t, f, Brushes.Black, x2, y2);
					float x3 = x2 + s.Width + 4;
					for (int i = 0; i < Stations.Length; i++)
					{
						float y = y0 + 18 * (i + 1) + 2;
						g.DrawLine(Pens.LightGray, new PointF(x2 - 2, y - 1), new PointF(w - 4, y - 1));
						t = Stations[i].Name;
						if (Stations[i].NameJapanese & Stations[i].Name.Length > 1)
						{
							float[] sizes = new float[t.Length];
							float totalsize = 0.0f;
							for (int j = 0; j < t.Length; j++)
							{
								sizes[j] = g.MeasureString(new string(t[j], 1), f, 9999, StringFormat.GenericTypographic).Width;
								totalsize += sizes[j];
							}

							float space = (stationnamewidth - totalsize) / (float) (t.Length - 1);
							float x = 0.0f;
							for (int j = 0; j < t.Length; j++)
							{
								g.DrawString(new string(t[j], 1), f, Brushes.Black, x2 + x, y);
								x += sizes[j] + space;
							}
						}
						else
						{
							g.DrawString(t, f, Brushes.Black, x2, y);
						}

						s = g.MeasureString(t, f);
						{
							float x = x2 + s.Width + 4;
							if (x > x3) x3 = x;
						}
					}

					g.DrawLine(Pens.LightGray, new PointF(x3 - 2, 4 + descriptionheight), new PointF(x3 - 2, y0 + 18 * (Stations.Length + 1)));
					if (k == 0)
					{
						stationnamewidth = x3 - x2 - 6;
					}

					// arrival time
					t = Translations.GetInterfaceString("timetable_arrivaltime");
					s = g.MeasureString(t, f);
					g.DrawString(t, f, Brushes.Black, x3, y2);
					float x4 = x3 + s.Width + 4;
					for (int i = 0; i < Stations.Length; i++)
					{
						float y = y0 + 18 * (i + 1) + 2;
						if (Stations[i].Pass)
						{
							t = "00";
							s = g.MeasureString(t, fs);
							float x = x3 + s.Width;
							t = "   ↓";
							g.DrawString(t, f, Brushes.Black, x, y);
							s = g.MeasureString(t, f);
							x += +s.Width + 4;
							if (x > x4) x4 = x;
						}
						else
						{
							if (Stations[i].Arrival.Hour.Length != 0)
							{
								t = Stations[i].Arrival.Hour;
								g.DrawString(t, fs, Brushes.Black, x3, y);
							}
							else
							{
								t = "00";
							}

							s = g.MeasureString(t, fs);
							float x = x3 + s.Width;
							if (Stations[i].Arrival.Minute.Length != 0 & Stations[i].Arrival.Second.Length != 0)
							{
								t = Stations[i].Arrival.Minute + ":" + Stations[i].Arrival.Second;
							}
							else t = "";

							g.DrawString(t, f, Brushes.Black, x, y);
							s = g.MeasureString(t, f);
							x += s.Width + 4;
							if (x > x4) x4 = x;
						}
					}

					g.DrawLine(Pens.LightGray, new PointF(x4 - 2, 4 + descriptionheight), new PointF(x4 - 2, y0 + 18 * (Stations.Length + 1)));
					// departure time
					t = Translations.GetInterfaceString("timetable_departuretime");
					s = g.MeasureString(t, f);
					g.DrawString(t, f, Brushes.Black, x4, y2);
					float x5 = x4 + s.Width + 4;
					for (int i = 0; i < Stations.Length; i++)
					{
						float y = y0 + 18 * (i + 1) + 2;
						if (Stations[i].Terminal)
						{
							t = "00";
							s = g.MeasureString(t, fs);
							float x = x4 + s.Width;
							const float c0 = 4;
							const float c1 = 32;
							g.DrawLine(Pens.Black, new PointF(x + c0, y + 6), new PointF(x + c1, y + 6));
							g.DrawLine(Pens.Black, new PointF(x + c0, y + 10), new PointF(x + c1, y + 10));
							x += c1 + 4;
							if (x > x5) x5 = x;
						}
						else
						{
							if (Stations[i].Departure.Hour.Length != 0)
							{
								t = Stations[i].Departure.Hour;
								g.DrawString(t, fs, Brushes.Black, x4, y);
							}
							else
							{
								t = "00";
							}

							s = g.MeasureString(t, fs);
							float x = x4 + s.Width;
							if (Stations[i].Departure.Minute.Length != 0 & Stations[i].Departure.Second.Length != 0)
							{
								t = Stations[i].Departure.Minute + ":" + Stations[i].Departure.Second;
							}
							else t = "";

							g.DrawString(t, f, Brushes.Black, x, y);
							s = g.MeasureString(t, f);
							x += s.Width + 4;
							if (x > x5) x5 = x;
						}
					}

					for (int i = 0; i < Stations.Length; i++)
					{
						float y = y0 + 18 * (i + 1) + 2;
						g.DrawLine(Pens.LightGray, new PointF(x2 - 2, y - 1), new PointF(w - 4, y - 1));
					}

					// border
					if (k == 1)
					{
						g.DrawLine(Pens.Black, new PointF(offsetx + 4, 4), new PointF(offsetx + 4, y0a + 18 * Tracks.Length - 1));
						g.DrawLine(Pens.Black, new PointF(offsetx + 4, y0a + 18 * Tracks.Length - 1), new PointF(x2 - 2, y0a + 18 * Tracks.Length - 1));
						g.DrawLine(Pens.Black, new PointF(offsetx + 4, 4), new PointF(w - 4, 4));
						g.DrawLine(Pens.Black, new PointF(offsetx + 4, 4 + descriptionheight), new PointF(w - 4, 4 + descriptionheight));
						g.DrawLine(Pens.Black, new PointF(x2 - 2, y0 + 18 * (Stations.Length + 1)), new PointF(w - 4, y0 + 18 * (Stations.Length + 1)));
						g.DrawLine(Pens.Black, new PointF(w - 4, 4), new PointF(w - 4, y0 + 18 * (Stations.Length + 1)));
						g.DrawLine(Pens.Black, new PointF(x2 - 2, y0a + 18 * Tracks.Length - 1), new PointF(x2 - 2, y0 + 18 * (Stations.Length + 1)));
					}

					// measure
					w = (int) Math.Ceiling((double) (x5 + 1));
					h = (int) Math.Ceiling((double) (y0 + 18 * (Stations.Length + 1) + 4));
					// description
					if (k == 0)
					{
						t = DefaultTimetableDescription;
						s = g.MeasureString(t, f, w - 16);
						descriptionwidth = s.Width;
						descriptionheight = s.Height + 2;
						h += (int) Math.Ceiling((double) s.Height) + 4;
					}

					// finish
					if (k == 0)
					{
						// measures
						int nw = Textures.RoundUpToPowerOfTwo(w);
						offsetx = nw - w;
						w = nw;
						actualheight = h;
						h = Textures.RoundUpToPowerOfTwo(h);
					}
					else
					{
						// create texture
						g.Dispose();
						timetableTexture = Textures.RegisterTexture(b);
					}
				}
			}
		}

		/// <summary>Creates the texture used to display the default auto-generated timetable</summary>
		internal static void CreateTimetable()
		{
			Table Table = new Table();
			Table.CollectData();
			Table.RenderData(ref DefaultTimetableTexture);
		}


		// update custom timetable
		internal static void UpdateCustomTimetable(Texture daytime, Texture nighttime) {
			for (int i = 0; i < CustomObjectsUsed; i++) {
				for (int j = 0; j < CustomObjects[i].States.Length; j++) {
					for (int k = 0; k < CustomObjects[i].States[j].Object.Mesh.Materials.Length; k++) {
						if (daytime != null) {
							CustomObjects[i].States[j].Object.Mesh.Materials[k].DaytimeTexture = daytime;
						}
						if (nighttime != null) {
							CustomObjects[i].States[j].Object.Mesh.Materials[k].NighttimeTexture = nighttime;
						}
					}
				}
			}
			if (daytime != null) {
				CurrentCustomTimetableDaytimeTexture = daytime;
			}
			if (nighttime != null) {
				CurrentCustomTimetableNighttimeTexture = nighttime;
			}
			if (CurrentCustomTimetableDaytimeTexture != null | CurrentCustomTimetableNighttimeTexture != null) {
				CustomTimetableAvailable = true;
			} else {
				CustomTimetableAvailable = false;
			}
		}
		
		// add object for custom timetable
		internal static void AddObjectForCustomTimetable(ObjectManager.AnimatedObject obj) {
			if (CustomObjectsUsed >= CustomObjects.Length) {
				Array.Resize<ObjectManager.AnimatedObject>(ref CustomObjects, CustomObjects.Length << 1);
			}
			CustomObjects[CustomObjectsUsed] = obj;
			CustomObjectsUsed++;
		}
	}
}
