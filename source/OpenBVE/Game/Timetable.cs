using System;
using System.Collections.Generic;
using System.Drawing;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2.Events;

namespace OpenBve {
	internal static class Timetable {

		// members (built-in timetable)
		
		internal static Texture DefaultTimetableTexture;
		internal static double DefaultTimetablePosition = 0.0;
		
		// members (custom timetable)
		internal static AnimatedObject[] CustomObjects = new AnimatedObject[16];
		internal static int CustomObjectsUsed;
		internal static bool CustomTimetableAvailable;
		internal static Texture CurrentCustomTimetableDaytimeTexture;
		internal static Texture CurrentCustomTimetableNighttimeTexture;
		internal static double CustomTimetablePosition = 0.0;
		
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
			internal List<Station> Stations;
			internal Track[] Tracks;

			/// <summary>Collects the timetable data for the current route</summary>
			internal void CollectData()
			{
				System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
				Stations = new List<Station>();
				Tracks = new Track[16];
				int n = 0;
				double currentLimit = -1.0, lastLimit = 6.94444444444444;
				int lastArrivalHours = -1, lastDepartureHours = -1;
				double LastTime = -1.0;
				for (int i = 0; i < Program.CurrentRoute.Tracks[0].Elements.Length; i++)
				{
					for (int j = 0; j < Program.CurrentRoute.Tracks[0].Elements[i].Events.Count; j++)
					{
						if (Program.CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent sse && Program.CurrentRoute.Stations[sse.StationIndex].Name != string.Empty && !Program.CurrentRoute.Stations[sse.StationIndex].Dummy)
						{
							if (currentLimit == -1.0) currentLimit = lastLimit;
							// update station
							Station currentStation;

							currentStation.Name = Program.CurrentRoute.Stations[sse.StationIndex].Name;
							currentStation.NameJapanese = Program.CurrentRoute.Stations[sse.StationIndex].Name.IsJapanese();
							currentStation.Pass = !Program.CurrentRoute.Stations[sse.StationIndex].PlayerStops();
							currentStation.Terminal = Program.CurrentRoute.Stations[sse.StationIndex].Type != StationType.Normal;
							double x;
							if (Program.CurrentRoute.Stations[sse.StationIndex].ArrivalTime >= 0.0)
							{
								x = Program.CurrentRoute.Stations[sse.StationIndex].ArrivalTime;
								x -= 86400.0 * Math.Floor(x / 86400.0);
								int hours = (int) Math.Floor(x / 3600.0);
								x -= 3600.0 * hours;
								int minutes = (int) Math.Floor(x / 60.0);
								x -= 60.0 * minutes;
								int seconds = (int) Math.Floor(x);
								currentStation.Arrival.Hour = hours != lastArrivalHours ? hours.ToString("00", Culture) : "";
								currentStation.Arrival.Minute = minutes.ToString("00", Culture);
								currentStation.Arrival.Second = seconds.ToString("00", Culture);
								lastArrivalHours = hours;
							}
							else
							{
								currentStation.Arrival.Hour = "";
								currentStation.Arrival.Minute = "";
								currentStation.Arrival.Second = "";
							}

							if (Program.CurrentRoute.Stations[sse.StationIndex].DepartureTime >= 0.0)
							{
								x = Program.CurrentRoute.Stations[sse.StationIndex].DepartureTime;
								x -= 86400.0 * Math.Floor(x / 86400.0);
								int hours = (int) Math.Floor(x / 3600.0);
								x -= 3600.0 * hours;
								int minutes = (int) Math.Floor(x / 60.0);
								x -= 60.0 * minutes;
								int seconds = (int) Math.Floor(x);
								currentStation.Departure.Hour = hours != lastDepartureHours ? hours.ToString("00", Culture) : "";
								currentStation.Departure.Minute = minutes.ToString("00", Culture);
								currentStation.Departure.Second = seconds.ToString("00", Culture);
								lastDepartureHours = hours;
							}
							else
							{
								currentStation.Departure.Hour = "";
								currentStation.Departure.Minute = "";
								currentStation.Departure.Second = "";
							}

							// update track
							if (n >= 1)
							{
								int m = n - 1;
								if (m == Tracks.Length)
								{
									Array.Resize(ref Tracks, Tracks.Length << 1);
								}

								// speed
								x = Math.Round(3.6 * currentLimit);
								Tracks[m].Speed = x.ToString(Culture);
								// time
								if (LastTime >= 0.0)
								{
									if (Program.CurrentRoute.Stations[sse.StationIndex].ArrivalTime >= 0.0)
									{
										x = Program.CurrentRoute.Stations[sse.StationIndex].ArrivalTime;
									}
									else if (Program.CurrentRoute.Stations[sse.StationIndex].DepartureTime >= 0.0)
									{
										x = Program.CurrentRoute.Stations[sse.StationIndex].DepartureTime;
									}
									else x = -1.0;

									if (x >= 0.0)
									{
										x -= LastTime;
										int hours = (int) Math.Floor(x / 3600.0);
										x -= 3600.0 * hours;
										int minutes = (int) Math.Floor(x / 60.0);
										x -= 60.0 * minutes;
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
							if (Program.CurrentRoute.Stations[sse.StationIndex].DepartureTime >= 0.0)
							{
								LastTime = Program.CurrentRoute.Stations[sse.StationIndex].DepartureTime;
							}
							else if (Program.CurrentRoute.Stations[sse.StationIndex].ArrivalTime >= 0.0)
							{
								LastTime = Program.CurrentRoute.Stations[sse.StationIndex].ArrivalTime;
							}
							else
							{
								LastTime = -1.0;
							}

							lastLimit = currentLimit;
							currentLimit = -1.0;
							n++;
							Stations.Add(currentStation);
						}

						if (n >= 1)
						{
							if (Program.CurrentRoute.Tracks[0].Elements[i].Events[j] is LimitChangeEvent lce)
							{
								if (lce.NextSpeedLimit != double.PositiveInfinity & lce.NextSpeedLimit > currentLimit) currentLimit = lce.NextSpeedLimit;
							}
						}
					}
				}
				
				if (n >= 2)
				{
					Array.Resize(ref Tracks, n - 1);
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
				if (Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse)
				{
					Stations.Reverse();
				}
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
					System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b);
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
					g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
					g.Clear(Color.Transparent);
					g.FillRectangle(Brushes.White, new RectangleF(offsetx, 0, w, actualheight));
					Font f = new Font(FontFamily.GenericSansSerif, 13.0f, GraphicsUnit.Pixel);
					Font fs = new Font(FontFamily.GenericSansSerif, 11.0f, GraphicsUnit.Pixel);
					Font fss = new Font(FontFamily.GenericSansSerif, 9.0f, GraphicsUnit.Pixel);
					// draw timetable
					string t;
					// description
					float x0 = offsetx + 8;
					float y0 = 8;
					if (k == 1)
					{
						t = Program.CurrentRoute.Information.DefaultTimetableDescription;
						g.DrawString(t, f, Brushes.Black, new RectangleF(x0, 6, descriptionwidth, descriptionheight + 8));
						y0 += descriptionheight + 2;
					}

					// highest speed
					t = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"timetable","highestspeed"});
					SizeF s = g.MeasureString(t, fs);
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
					t = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"timetable","drivingtime"});
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
					t = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"timetable","stationname"});
					s = g.MeasureString(t, f);
					g.DrawString(t, f, Brushes.Black, x2, y2);
					float x3 = x2 + s.Width + 4;
					for (int i = 0; i < Stations.Count; i++)
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

							float space = (stationnamewidth - totalsize) / (t.Length - 1);
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

					g.DrawLine(Pens.LightGray, new PointF(x3 - 2, 4 + descriptionheight), new PointF(x3 - 2, y0 + 18 * (Stations.Count + 1)));
					if (k == 0)
					{
						stationnamewidth = x3 - x2 - 6;
					}

					// arrival time
					t = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"timetable","arrivaltime"});
					s = g.MeasureString(t, f);
					g.DrawString(t, f, Brushes.Black, x3, y2);
					float x4 = x3 + s.Width + 4;
					for (int i = 0; i < Stations.Count; i++)
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

					g.DrawLine(Pens.LightGray, new PointF(x4 - 2, 4 + descriptionheight), new PointF(x4 - 2, y0 + 18 * (Stations.Count + 1)));
					// departure time
					t = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"timetable","departuretime"});
					s = g.MeasureString(t, f);
					g.DrawString(t, f, Brushes.Black, x4, y2);
					float x5 = x4 + s.Width + 4;
					for (int i = 0; i < Stations.Count; i++)
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

					for (int i = 0; i < Stations.Count; i++)
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
						g.DrawLine(Pens.Black, new PointF(x2 - 2, y0 + 18 * (Stations.Count + 1)), new PointF(w - 4, y0 + 18 * (Stations.Count + 1)));
						g.DrawLine(Pens.Black, new PointF(w - 4, 4), new PointF(w - 4, y0 + 18 * (Stations.Count + 1)));
						g.DrawLine(Pens.Black, new PointF(x2 - 2, y0a + 18 * Tracks.Length - 1), new PointF(x2 - 2, y0 + 18 * (Stations.Count + 1)));
					}

					// measure
					w = (int) Math.Ceiling(x5 + 1);
					h = (int) Math.Ceiling(y0 + 18 * (Stations.Count + 1) + 4);
					// description
					if (k == 0)
					{
						t = Program.CurrentRoute.Information.DefaultTimetableDescription;
						s = g.MeasureString(t, f, w - 16);
						descriptionwidth = s.Width;
						descriptionheight = s.Height + 2;
						h += (int) Math.Ceiling(s.Height) + 4;
					}
					f.Dispose();
					fs.Dispose();
					fss.Dispose();
					// finish
					if (k == 0)
					{
						// measures
						int nw = Program.Renderer.TextureManager.RoundUpToPowerOfTwo(w);
						offsetx = nw - w;
						w = nw;
						actualheight = h;
						h = Program.Renderer.TextureManager.RoundUpToPowerOfTwo(h);
					}
					else
					{
						// create texture
						g.Dispose();
						timetableTexture = Program.Renderer.TextureManager.RegisterTexture(b);
					}
				}
			}
		}

		/// <summary>Creates the texture used to display the default auto-generated timetable</summary>
		internal static void CreateTimetable()
		{
			try
			{
				Table Table = new Table();
				Table.CollectData();
				Table.RenderData(ref DefaultTimetableTexture);
			}
			catch
			{
				DefaultTimetableTexture = null;
			}
		}


		// update custom timetable
		internal static void UpdateCustomTimetable(Texture daytime, Texture nighttime) {
			for (int i = 0; i < CustomObjectsUsed; i++) {
				for (int j = 0; j < CustomObjects[i].States.Length; j++) {
					for (int k = 0; k < CustomObjects[i].States[j].Prototype.Mesh.Materials.Length; k++) {
						if (daytime != null) {
							CustomObjects[i].States[j].Prototype.Mesh.Materials[k].DaytimeTexture = daytime;
						}
						if (nighttime != null) {
							CustomObjects[i].States[j].Prototype.Mesh.Materials[k].NighttimeTexture = nighttime;
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
		internal static void AddObjectForCustomTimetable(AnimatedObject obj) {
			if (CustomObjectsUsed >= CustomObjects.Length) {
				Array.Resize(ref CustomObjects, CustomObjects.Length << 1);
			}
			CustomObjects[CustomObjectsUsed] = obj;
			CustomObjectsUsed++;
		}
	}
}
