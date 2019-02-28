using System;
using System.Drawing;
using OpenBveApi;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static class Illustrations {

		private const double	LeftPad			= 8.0;
		private const double	TopPad			= 8.0;
		private const double	RightPad		= 8.0;
		private const double	BottomPad		= 8.0;
		private const int		TrackOffDist	= 48;		// the distance in pixels between track offset labels
		private const double	TrackOffY		= 6.0;		// distance from bottom of track offset labels
		private const double	TrackOffsPad	= 16.0;		// how much space to leave for track offsets
															// at the bottom of the gradient profile
		private const float		StationRadius	= 4.0f;
		private const float 	StationDiameter	= (StationRadius*2.0f);
		private const double	StationTextPad	= 6.0;

		// a struct for the colours used in the two different graphic contexts, as a GTK+ window and in-game
		private struct MapColors
		{										// ROUTE MAP
			public Color	background;			// global image background
			public Pen		atcMap;				// atc-controlled track
			public Pen		normalMap;			// normal track
			public Brush	actStatnFill;		// dot fill for active station
			public Brush	inactStatnFill;		// dot fill for inactive station
			public Pen		actStatnBrdr;		// dot border for active station
			public Pen		inactStatnBrdr;		// dot border for inactive station
			public Brush	actNameFill;		// rect. fill for active station names
			public Brush	inactNameFill;		// rect. fill for inactive station names
			public Pen		actNameBrdr;		// rect. border for active station names
			public Pen		inactNameBrdr;		// rect. border for inactive station names
			public Brush	actNameText;		// active station name text
			public Brush	inactNameText;		// inactive station name text
												// ROUTE ELEVATION
			public Brush	belowSeaFill;		// fill for below-sea level areas
			public Pen		belowSeaBrdr;		// border for below sea level areas
			public Brush	elevFill;			// fill for elevation contour
			public Pen		elevBrdr;			// border for elevation contour
		};

		// the colours used for the images
		private static readonly MapColors[]	mapColors = new MapColors[]
		{					// colours for windowed mode display
			new MapColors() {background=Color.White,		atcMap=Pens.DarkRed,	normalMap=Pens.Black,
							actStatnFill=Brushes.SkyBlue,	inactStatnFill=Brushes.LightGray,
							actStatnBrdr=Pens.Black,		inactStatnBrdr=Pens.Gray,
							actNameFill=Brushes.White,		inactNameFill=Brushes.LightGray,
							actNameBrdr=Pens.Black,			inactNameBrdr=Pens.Gray,
							actNameText=Brushes.Black,		inactNameText=Brushes.Gray,
							belowSeaFill=Brushes.PaleGoldenrod,	belowSeaBrdr=Pens.Gray,
							elevFill=Brushes.Tan,			elevBrdr=Pens.Black},
							// colours for in-game display
			new MapColors() {background=Color.FromArgb(0x64000000),	atcMap=Pens.Red,	normalMap=Pens.White,
							actStatnFill=Brushes.SkyBlue,	inactStatnFill=Brushes.Gray,
							actStatnBrdr=Pens.White,		inactStatnBrdr=Pens.LightGray,
							actNameFill=Brushes.Black,		inactNameFill=Brushes.Gray,
							actNameBrdr=Pens.White,			inactNameBrdr=Pens.LightGray,
							actNameText=Brushes.White,		inactNameText=Brushes.LightGray,
							belowSeaFill= new SolidBrush(Color.FromArgb(0x7feee8aa)),	belowSeaBrdr=Pens.Gray,
							elevFill= new SolidBrush(Color.FromArgb(0x7fd2b48c)),		elevBrdr=Pens.Gray}
		};

		// data about world ranges of last generated images
		private static int	lastGradientMinTrack, lastGradientMaxTrack;
		private static int	lastRouteMinX, lastRouteMinZ, lastRouteMaxX, lastRouteMaxZ;
		// access these properties as read-only
		public  static int	LastGradientMinTrack	{ get { return lastGradientMinTrack; } }
		public  static int	LastGradientMaxTrack	{ get { return lastGradientMaxTrack; } }
		public  static int	LastRouteMinX			{ get { return lastRouteMinX; } }
		public  static int	LastRouteMaxX			{ get { return lastRouteMaxX; } }
		public  static int	LastRouteMinZ			{ get { return lastRouteMinZ; } }
		public  static int	LastRouteMaxZ			{ get { return lastRouteMaxZ; } }

		//GDI Plus is not thread-safe
		//This object should be locked on when drawing a route illustration / gradient profile
		/// <summary>Holds the current lock for the illustrations drawing functions</summary>
		public static readonly object Locker =  new Object();

		//
		// CREATE ROUTE MAP
		//
		/// <summary>Creates and returns the route map as Bitmap.</summary>
		/// <returns>The route map.</returns>
		/// <param name="Width">The width of the bitmap to create.</param>
		/// <param name="Height">The height of the bitmap to create.</param>
		/// <param name="inGame"><c>true</c> = bitmap for in-game overlay | <c>false</c> = for standard window.</param>
		internal static Bitmap CreateRouteMap(int Width, int Height, bool inGame)
		{
			int n, n0, n1;
			RouteRange(out n, out n0, out n1);
			// find dimensions
			double x0 = double.PositiveInfinity, z0 = double.PositiveInfinity;
			double x1 = double.NegativeInfinity, z1 = double.NegativeInfinity;
			for (int i = n0; i <= n1; i++)
			{
				double x = TrackManager.CurrentTrack.Elements[i].WorldPosition.X;
				double z = TrackManager.CurrentTrack.Elements[i].WorldPosition.Z;
				if (x < x0) x0 = x;
				if (x > x1) x1 = x;
				if (z < z0) z0 = z;
				if (z > z1) z1 = z;
			}
			// avoid 0 or negative height or width
			if (x0 >= x1 - 1.0) x0 = x1 - 1.0;
			if (z0 >= z1 - 1.0) z0 = z1 - 1.0;
			// remember area occupied so far
			double xMin, xMax, zMin, zMax;			// used to track the bitmap area actually occupied by drawings
			xMin = x0;
			xMax = x1;
			zMin = z1;								// bitmap z goes down, while world z goes up
			zMax = z0;
			// fit route w/h ratio in image w/h ratio
			double wrh = (double)Width / (double)Height;
			if ((x1 - x0) / (z1 - z0) <= wrh)		// if route ratio is taller than bitmap ratio
			{
				double dx = 0.5 * (z1 - z0) * wrh;	//	scale (half of) x range as much as (half of) z range
				double px = 0.5 * (x0 + x1);		//	x range mid point
				x0 = px - dx;						//	centre scaled x range around mid point
				x1 = px + dx;
			} else									// if route ratio is wider than bitmap ratio
			{										//	do the opposite (scale z range on x)
				double dz = 0.5 * (x1 - x0) / wrh;
				double pz = 0.5 * (z0 + z1);
				z0 = pz - dz;
				z1 = pz + dz;
			}
			double ox = LeftPad, oy = TopPad;
			double w = (double)(Width - (LeftPad+RightPad));
			double h = (double)(Height- (TopPad+BottomPad));
			// horizontal and vertical scales
			double xd = w / (x1 - x0);
			double zd = h / (z1 - z0);
			// convert bitmap occupied area from world to bitmap coordinates
			xMin = ox + (xMin - x0) * xd;
			xMax = ox + (xMax - x0) * xd;
			zMin = oy + (z0 - zMin) * zd + h;
			zMax = oy + (z0 - zMax) * zd + h;
			// create bitmap
			int		mode = inGame ? 1 : 0;
			Bitmap b = new Bitmap(Width, Height, inGame ? System.Drawing.Imaging.PixelFormat.Format32bppArgb
				: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(b);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			g.Clear(mapColors[mode].background);

			// ROUTE PATH
			{
				int start = 0;
				bool atc = false;
				n = n1 - n0 + 1;
				PointF[] p = new PointF[n];
				for (int i = 0; i < n; i++)
				{
					double x = TrackManager.CurrentTrack.Elements[i+n0].WorldPosition.X;
					double z = TrackManager.CurrentTrack.Elements[i+n0].WorldPosition.Z;
					x = ox + (x - x0) * xd;
					z = oy + (z0 - z) * zd + h;
					p[i] = new PointF((float)x, (float)z);
					// ATS / ATC
					// for each track element, look for a StationStartEvent
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i+n0].Events.Length; j++)
					{
						if (TrackManager.CurrentTrack.Elements[i+n0].Events[j] is TrackManager.StationStartEvent)
						{
							TrackManager.StationStartEvent e =
								(TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i+n0].Events[j];
							// if StationStartEvent found, look for a change in ATS/ATC control;
							// if there is a change, draw all previous track elements
							// with colour for the previous control state
							if (Game.Stations[e.StationIndex].SafetySystem == Game.SafetySystem.Atc)
							{
								if (!atc)
								{
									atc = true;
									if (i - start - 1 > 0)
										g.DrawCurve(mapColors[mode].normalMap, p, start, i - start - 1);
									start = i;
								}
							} else
							{
								if (atc)
								{
									atc = false;
									if (i - start - 1 > 0)
										g.DrawCurve(mapColors[mode].atcMap, p, start, i - start - 1);
									start = i;
								}
							}
							break;
						}
					}
				}
				// draw all remaining track element not drawn yet
				DrawSegmentedCurve(g, atc ? mapColors[mode].atcMap : mapColors[mode].normalMap, p, start, n - start - 1);
			}

			// STATION ICONS
			for (int i = n0; i <= n1; i++)
			{
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
				{
					if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
					{
						TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
						if (Game.Stations[e.StationIndex].Name != string.Empty)
						{
							double x = TrackManager.CurrentTrack.Elements[i].WorldPosition.X;
							double y = TrackManager.CurrentTrack.Elements[i].WorldPosition.Z;
							x = ox + (x - x0) * xd;
							y = oy + (z0 - y) * zd + h;
							// station circle
							RectangleF r = new RectangleF((float)x - StationRadius, (float)y - StationRadius,
								StationDiameter, StationDiameter);
							bool q = Game.PlayerStopsAtStation(e.StationIndex);
							g.FillEllipse(q ? mapColors[mode].actStatnFill : mapColors[mode].inactStatnFill, r);
							g.DrawEllipse(q ? mapColors[mode].actStatnBrdr : mapColors[mode].inactStatnBrdr, r);
							// adjust bitmap occupied area
							if (r.Left < xMin)
								xMin = r.Left;
							if (r.Top  < zMin)
								zMin = r.Top;
							if (r.Right > xMax)
								xMax = r.Right;
							if (r.Bottom > zMax)
								zMax = r.Bottom;
						}
					}
				}
			}

			// STATION NAMES
			{
				double wh = w * h;
				Font f = new Font(FontFamily.GenericSansSerif, wh < 65536.0 ? 9.0f : 10.0f, GraphicsUnit.Pixel);
				for (int i = n0; i <= n1; i++)
				{
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
					{
						if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
						{
							TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
							if (Game.Stations[e.StationIndex].Name != string.Empty)
							{
								double x = TrackManager.CurrentTrack.Elements[i].WorldPosition.X;
								double y = TrackManager.CurrentTrack.Elements[i].WorldPosition.Z;
								x = ox + (x - x0) * xd;
								y = oy + (z0 - y) * zd + h;
								bool stop = Game.PlayerStopsAtStation(e.StationIndex);
								string t = Game.Stations[e.StationIndex].Name;
								SizeF m = g.MeasureString(t, f, Width, StringFormat.GenericDefault);
								double sx = TrackManager.CurrentTrack.Elements[i].WorldSide.X;
								double sz = TrackManager.CurrentTrack.Elements[i].WorldSide.Z;
								double xt, yt;
								if (Math.Sign(sx) == Math.Sign(sz))
								{
									// descending
									bool o = (x - ox) * (h - y) <= (w - x) * (y - oy);
									if (o)
									{
										// up-right
										xt = x + StationTextPad;
										yt = y - StationTextPad - m.Height;
									} else
									{
										// down-left
										xt = x - StationTextPad - m.Width;
										yt = y + StationTextPad;
									}
								} else
								{
									// ascending
									bool o = (h - y) * (w - x) <= (x - ox) * (y - oy);
									if (o)
									{
										// up-left
										xt = x - StationTextPad - m.Width;
										yt = y - StationTextPad - m.Height;
									} else
									{
										// down-right
										xt = x + StationTextPad;
										yt = y + StationTextPad;
									}
								}
								// constrain text within bitmap edges (taking into account also paddings)
								if (xt < ox)
									xt = ox;
								else if (xt + m.Width > w)
									xt = w - m.Width;
								if (yt < oy)
									yt = oy;
								else if (yt + m.Height > h)
									yt = h - m.Height;
								RectangleF r = new RectangleF((float)xt - 1.0f, (float)yt - 1.0f, m.Width + 2.0f, m.Height + 2.0f);
								g.FillRectangle(stop ? mapColors[mode].actNameFill : mapColors[mode].inactNameFill,
									r.Left, r.Top, r.Width, r.Height);
								g.DrawRectangle(stop ? mapColors[mode].actNameBrdr : mapColors[mode].inactNameBrdr,
									r.Left, r.Top, r.Width, r.Height);
								g.DrawString(t, f, stop ? mapColors[mode].actNameText : mapColors[mode].inactNameText,
									(float)xt, (float)yt);
								// adjust bitmap occupied area
								if (r.Left < xMin)
									xMin = r.Left;
								if (r.Top  < zMin)
									zMin = r.Top;
								if (r.Right > xMax)
									xMax = r.Right;
								if (r.Bottom > zMax)
									zMax = r.Bottom;
							}
						}
					}
				}
			}
			// if in-game, trim unused parts of the bitmap
			if (inGame)
			{
				xMin -= LeftPad;
				xMax += RightPad;
				zMin -= TopPad;
				zMax += BottomPad;
				if (xMin < 0)
					xMin = 0;
				if (xMax >= Width)
					xMax = Width - 1;
				if (zMin < 0)
					zMin = 0;
				if (zMax >= Height)
					zMax = Height - 1;
				Bitmap nb = new Bitmap((int)(xMax - xMin + 1.0), (int)(zMax - zMin + 1.0));	// round up
				g = Graphics.FromImage(nb);
				g.DrawImage(b, (int)-xMin, (int)-zMin);										// round down
				// set total bitmap world X and Z ranges from bitmap ranges
				lastRouteMinX = (int)((xMin - ox) / xd + x0);
				lastRouteMaxX = (int)((xMax - ox) / xd + x0);
				lastRouteMinZ = (int)(z0 - (zMax - oy - h) / zd);
				lastRouteMaxZ = (int)(z0 - (zMin - oy - h) / zd);
				return nb;
			}
			//set total bitmap X and Z ranges
			lastRouteMinX = (int)(x0 - ox * (x1 - x0) / w);
			lastRouteMaxX = (int)(x1 + (Width - w - ox) * (x1 - x0) / w);
			lastRouteMinZ = (int)(z0 - oy * (z1 - z0) / h);
			lastRouteMaxZ = (int)(z1 + (Height- h - oy) * (z1 - z0) / h);
			return b;
		}

		//
		// CREATE GRADIENT PROFILE
		//
		/// <summary>Creates the route gradient profile.</summary>
		/// <returns>The route gradient profile as Bitmap.</returns>
		/// <param name="Width">The width of the bitmap to create.</param>
		/// <param name="Height">The height of the bitmap to create.</param>
		/// <param name="inGame"><c>true</c> = bitmap for in-game overlay | <c>false</c> = for standard window.</param>
		internal static Bitmap CreateRouteGradientProfile(int Width, int Height, bool inGame)
		{
			if (TrackManager.CurrentTrack.Elements.Length > 36 && Game.Stations.Length == 0)
			{
				// If we have track elements, but no stations, show a specific error message, rather
				// than the more generic one thrown later
				// NOTE: Will throw the generic error message on routes shorter than 900m with no stations
				throw new Exception(Translations.GetInterfaceString("errors_route_corrupt_nostations"));
			}
			// Track elements are assumed to be all of the same length, and this length
			// is used as measure unit, rather than computing the incremental track length
			// in any 'real world' unit (like m).

			// HORIZONTAL RANGE: find first and last used element based on stations
			int n, n0, n1;
			RouteRange(out n, out n0, out n1);
			// VERTICAL RANGE
			double y0 = double.PositiveInfinity, y1 = double.NegativeInfinity;
			for (int i = n0; i <= n1; i++)
			{
				double y = TrackManager.CurrentTrack.Elements[i].WorldPosition.Y;
				if (y < y0) y0 = y;
				if (y > y1) y1 = y;
			}
			if (y0 >= y1 - 1.0)
				y0 = y1 - 1.0;

			// allow for some padding around actual data
			double ox = LeftPad, oy = TopPad;
			double w = (double)(Width - (LeftPad+RightPad));
			double h = (double)(Height - (TopPad+BottomPad+TrackOffsPad));
			// horizontal and vertical scale
			double nd = w / (double)(n1 - n0);
			double yd = h / (double)(y1 - y0);
			// set total bitmap track position range; used by in-game profile to place
			// the current position of the trains; as the train positions are known as track positions,
			// actual track positions are needed here, rather than indices into the track element array.
			double minX = TrackManager.CurrentTrack.Elements[n0].StartingTrackPosition;
			double maxX = TrackManager.CurrentTrack.Elements[n1].StartingTrackPosition;
			double offX = ox * (maxX - minX) / w;
			lastGradientMinTrack = (int)(minX - offX);
			lastGradientMaxTrack = (int)(maxX + offX);

			// BITMAP (in-game display needs transparency)
			Bitmap b = new Bitmap(Width, Height,
					inGame ? System.Drawing.Imaging.PixelFormat.Format32bppArgb
					: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(b);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			int mode = inGame ? 1 : 0;
			g.Clear(mapColors[mode].background);

			// BELOW SEA LEVEL
			{
				double y = oy + (h - 0.5 * (double)(-Game.RouteInitialElevation - y0) * yd);
				double x0 = ox - (double)(0) * nd;
				double x1 = ox + (double)(n1 - n0) * nd;
				g.FillRectangle(mapColors[mode].belowSeaFill, (float)x0, (float)y, (float)x1, (float)(oy + h) - (float)y);
				g.DrawLine(mapColors[mode].belowSeaBrdr, (float)x0, (float)y, (float)x1, (float)y);
			}
			// GRADIENT PROFILE
			{
				n = n1 - n0 + 1;
				PointF[] p = new PointF[n + 2];
				p[0] = new PointF((float)ox, (float)(oy + h));
				for (int i = n0; i <= n1; i++)
				{
					double x = ox + (double)(i - n0) * nd;
					double y = oy + (h - 0.5 *
						(double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd);
					p[i -n0 + 1] = new PointF((float)x, (float)y);
				}
				p[n + 1] = new PointF((float)(ox + (double)(n - 1) * nd), (float)(oy + h));
				g.FillPolygon(mapColors[mode].elevFill, p);
				for (int i = 1; i < n; i++)
					g.DrawLine(mapColors[mode].elevBrdr, p[i], p[i + 1]);
				g.DrawLine(mapColors[mode].elevBrdr, 0.0f, (float)(oy + h), (float)Width, (float)(oy + h));
			}
			// STATION NAMES
			{
				Font f = new Font(FontFamily.GenericSansSerif, 12.0f, GraphicsUnit.Pixel);
				StringFormat m = new StringFormat();
				for (int i = n0; i <= n1; i++)
				{
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
					{
						if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
						{
							TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
							if (Game.Stations[e.StationIndex].Name != string.Empty)
							{
								bool stop = Game.PlayerStopsAtStation(e.StationIndex);
								if (Game.Stations[e.StationIndex].Name.IsJapanese())
								{
									m.Alignment = StringAlignment.Near;
									m.LineAlignment = StringAlignment.Near;
									double x = ox + (double)(i - n0) * nd;
									double y = oy + (h - 0.5 *
										(double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd);
									string t = Game.Stations[e.StationIndex].Name;
									float tx = 0.0f, ty = (float)oy;
									for (int k = 0; k < t.Length; k++)
									{
										SizeF s = g.MeasureString(t.Substring(k, 1), f, 65536, StringFormat.GenericTypographic);
										if (s.Width > tx) tx = s.Width;
									}
									for (int k = 0; k < t.Length; k++)
									{
										g.DrawString(t.Substring(k, 1), f,
											stop ? mapColors[mode].actNameText : mapColors[mode].inactNameText,
											(float)x - 0.5f * tx, ty);
										SizeF s = g.MeasureString(t.Substring(k, 1), f, 65536, StringFormat.GenericTypographic);
										ty += s.Height;
									}
									g.DrawLine(stop ? mapColors[mode].actNameBrdr : mapColors[mode].inactNameBrdr,
										new PointF((float)x, ty + 4.0f), new PointF((float)x, (float)y));
								} else
								{
									m.Alignment = StringAlignment.Far;
									m.LineAlignment = StringAlignment.Near;
									double x = ox + (double)(i - n0) * nd;
									double y = oy + (h - 0.5 *
										(double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd);
									g.RotateTransform(-90.0f);
									g.TranslateTransform((float)x, (float)oy, System.Drawing.Drawing2D.MatrixOrder.Append);
									g.DrawString(Game.Stations[e.StationIndex].Name, f,
										stop ? mapColors[mode].actNameText : mapColors[mode].inactNameText,
										new PointF(0.0f, -5.0f), m);
									g.ResetTransform();
									SizeF s = g.MeasureString(Game.Stations[e.StationIndex].Name, f);
									g.DrawLine(stop ? mapColors[mode].actNameBrdr : mapColors[mode].inactNameBrdr,
										new PointF((float)x, (float)(oy + s.Width + 4)), new PointF((float)x, (float)y));
								}
							}
						}
					}
				}
			}
			// ROUTE MARKERS
			{
				Font f = new Font(FontFamily.GenericSansSerif, 10.0f, GraphicsUnit.Pixel);
				Font fs = new Font(FontFamily.GenericSansSerif, 9.0f, GraphicsUnit.Pixel);
				StringFormat m = new StringFormat
				{
					Alignment = StringAlignment.Far,
					LineAlignment = StringAlignment.Center
				};
				System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
				int k = TrackOffDist * n / Width;
				if (k == 0)
				{
					//If k is equal to zero, this generally means that the WithTrack section is missing from our routefile
					//Adding zero to the loop control variable will also produce an infinite loop, so that's a bad idea too
					throw new Exception(Translations.GetInterfaceString("errors_route_corrupt_withtrack"));
				}
				for (int i = n0; i <= n1; i += k)
				{
					double x = ox + (double)(i - n0) * nd;
					double y = (double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd;
					// track offset label
					if (x < w)
					{
						string t = ((int)Math.Round(TrackManager.CurrentTrack.Elements[i].StartingTrackPosition)).ToString(Culture);
						g.DrawString(t + "m", f, mapColors[mode].actNameText, (float)x, (float)(oy + h + TrackOffY));
					}
					// route height at track offset (with measure and vertical line)
					{
						y = oy + (h - 0.5 * y) + 2.0f;
						string t = ((int)Math.Round(Game.RouteInitialElevation + TrackManager.CurrentTrack.Elements[i].WorldPosition.Y)).ToString(Culture);
						SizeF s = g.MeasureString(t, fs);
						if (y < oy + h - (double)s.Width - 10.0)
						{
							g.RotateTransform(-90.0f);
							g.TranslateTransform((float)x, (float)y + 4.0f, System.Drawing.Drawing2D.MatrixOrder.Append);
							g.DrawString(t + "m", fs, mapColors[mode].actNameText, 0.0f, 0.0f, m);
							g.ResetTransform();
							g.DrawLine(mapColors[mode].inactNameBrdr,
								(float)x, (float)(y + s.Width + 12.0), (float)x, (float)(oy + h));
						}
					}
				}
			}
			// finalize
			return b;
		}

		// draw segmented curve
		private static void DrawSegmentedCurve(Graphics Graphics, Pen Pen, PointF[] Points, int Start, int Length) {
			const int Count = 1000;
			int End = Start + Length - 1;
			for (int k = Start; k <= End; k += Count)
			{
				int m = End - k + 1;
				if (m > Count) m = Count;
				Graphics.DrawCurve(Pen, Points, k, m);
			}
		}

		//
		// FIND USED ROUTE RANGE
		//
		/// <summary>Finds the route range actually used by stations (with some margin before and after.</summary>
		/// <param name="n">Route total number of elements</param>
		/// <param name="n0">First element in used range</param>
		/// <param name="n1">Last element in used range</param>
		private static void RouteRange(out int n, out int n0, out int n1)
		{
			// find first and last track element actually used by stations
			n	= TrackManager.CurrentTrack.Elements.Length;
			n0	= n - 1;
			n1	= 0;
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
			{
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
				{
					if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
					{
						if (i < n0) n0 = i;
						if (i > n1) n1 = i;
						break;
					}
				}
			}
			// Allow for 4 track units before first station and 8 track units after the last one
			n0 -= 4;
			n1 += 8;
			// But not outside of actual track element array!
			if (n0 < 0) n0 = 0;
			if (n1 >= TrackManager.CurrentTrack.Elements.Length)
				n1 = TrackManager.CurrentTrack.Elements.Length - 1;
			if (n1 <= n0)		// neither a 0-length or 'negative' track!
				n1 = n0 + 1;
		}

	}
}