using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using RouteManager2.Events;
using RouteManager2.SignalManager;


namespace RouteManager2
{
	/// <summary>Used for creating illustrations</summary>
	public static class Illustrations
	{
		/// <summary>Holds the current lock for the illustrations drawing functions</summary>
		/// <remarks>GDI Plus is not thread-safe- This object should be locked on when drawing a route illustration / gradient profile</remarks>
		public static readonly object Locker =  new object();

		internal static CurrentRoute CurrentRoute;

		private const double	LeftPad			= 8.0;
		private const double	TopPad			= 8.0;
		private const double	RightPad		= 8.0;
		private const double	BottomPad		= 8.0;
		private const int		TrackOffDist	= 48;		// the distance in pixels between track offset labels
		private const double	TrackOffY		= 6.0;		// distance from bottom of track offset labels
		private const double	TrackOffsPad	= 16.0;		// how much space to leave for track offsets
															// at the bottom of the gradient profile
		private const float		StationRadius	= 4.0f;
		private const float 	StationDiameter	= StationRadius * 2.0f;
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
			public Brush	limitFill;			// fill for elevation contour
			public Pen		limitBrdr;			// border for elevation contour
		};

		// the colours used for the images
		private static readonly MapColors[]	mapColors =
		{					// colours for windowed mode display
			new MapColors() {background=Color.White,		atcMap=Pens.DarkRed,	normalMap=Pens.Black,
							actStatnFill=Brushes.SkyBlue,	inactStatnFill=Brushes.LightGray,
							actStatnBrdr=Pens.Black,		inactStatnBrdr=Pens.Gray,
							actNameFill=Brushes.White,		inactNameFill=Brushes.LightGray,
							actNameBrdr=Pens.Black,			inactNameBrdr=Pens.Gray,
							actNameText=Brushes.Black,		inactNameText=Brushes.Gray,
							belowSeaFill=Brushes.PaleGoldenrod,	belowSeaBrdr=Pens.Gray,
							elevFill=Brushes.Tan,			elevBrdr=Pens.Black,
							limitFill=Brushes.White,			limitBrdr= new Pen(Color.Red, 5f)},
							// colours for in-game display
			new MapColors() {background=Color.FromArgb(0x64000000),	atcMap=Pens.Red,	normalMap=Pens.White,
							actStatnFill=Brushes.SkyBlue,	inactStatnFill=Brushes.Gray,
							actStatnBrdr=Pens.White,		inactStatnBrdr=Pens.LightGray,
							actNameFill=Brushes.Black,		inactNameFill=Brushes.Gray,
							actNameBrdr=Pens.White,			inactNameBrdr=Pens.LightGray,
							actNameText=Brushes.White,		inactNameText=Brushes.LightGray,
							belowSeaFill= new SolidBrush(Color.FromArgb(0x7feee8aa)),	belowSeaBrdr=Pens.Gray,
							elevFill= new SolidBrush(Color.FromArgb(0x7fd2b48c)),		elevBrdr=Pens.Gray,
							limitFill=Brushes.White,			limitBrdr= new Pen(Color.Red, 5f)},
			new MapColors() {background=Color.FromArgb(0x64000000),	atcMap=Pens.Red,	normalMap=Pens.White,
			actStatnFill=Brushes.SkyBlue,	inactStatnFill=Brushes.Gray,
			actStatnBrdr=Pens.White,		inactStatnBrdr=Pens.LightGray,
			actNameFill=Brushes.Black,		inactNameFill=Brushes.Gray,
			actNameBrdr=Pens.White,			inactNameBrdr=Pens.LightGray,
			actNameText=Brushes.White,		inactNameText=Brushes.LightGray,
			belowSeaFill= new SolidBrush(Color.FromArgb(0x7feee8aa)),	belowSeaBrdr=Pens.Gray,
			elevFill= new SolidBrush(Color.FromArgb(0x7fd2b48c)),		elevBrdr=Pens.Gray,
			limitFill=Brushes.White,			limitBrdr= new Pen(Color.Red, 5f)}
		};

		// data about world ranges of last generated images
		private static int	lastGradientMinTrack, lastGradientMaxTrack;
		private static int	lastRouteMinX, lastRouteMinZ, lastRouteMaxX, lastRouteMaxZ;
		// access these properties as read-only
		public  static int	LastGradientMinTrack => lastGradientMinTrack;
		public  static int	LastGradientMaxTrack => lastGradientMaxTrack;
		public  static int	LastRouteMinX => lastRouteMinX;
		public  static int	LastRouteMaxX => lastRouteMaxX;
		public  static int	LastRouteMinZ => lastRouteMinZ;
		public  static int	LastRouteMaxZ => lastRouteMaxZ;

		/// <summary>Creates and returns the route map as Bitmap.</summary>
		/// <returns>The route map.</returns>
		/// <param name="Width">The width of the bitmap to create.</param>
		/// <param name="Height">The height of the bitmap to create.</param>
		/// <param name="inGame"><c>true</c> = bitmap for in-game overlay | <c>false</c> = for standard window.</param>
		/// <param name="switchPositions">The list of switch positions on the map</param>
		/// <param name="follower">The TrackFollower used for drawing</param>
		/// <param name="drawRadius">The draw radius</param>
		public static Bitmap CreateRouteMap(int Width, int Height, bool inGame, out Dictionary<Guid, Vector2> switchPositions, TrackFollower follower = null, int drawRadius = 500)
		{
			switchPositions = null;
			if (CurrentRoute.Tracks[0].Elements == null)
			{
				//Usually caused by selecting another route before preview has finished
				return new Bitmap(1,1);
			}
			int firstUsedElement, lastUsedElement;
			if (follower != null)
			{
				RestrictedRouteRange(follower, drawRadius, out firstUsedElement, out lastUsedElement);
			}
			else
			{
				TotalRouteRange(out _, out firstUsedElement, out lastUsedElement);	
			}
			
			// find dimensions
			double x0 = double.PositiveInfinity, z0 = double.PositiveInfinity;
			double x1 = double.NegativeInfinity, z1 = double.NegativeInfinity;
			for (int i = firstUsedElement; i <= lastUsedElement; i++)
			{
				double x = CurrentRoute.Tracks[0].Elements[i].WorldPosition.X;
				double z = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Z;
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
			Vector2 imageOrigin = new Vector2(LeftPad, TopPad);
			Vector2 imageSize = new Vector2((Width - (LeftPad + RightPad)), Height - (TopPad + BottomPad));
			Vector2 imageScale = new Vector2(imageSize.X / (x1 - x0),imageSize.Y / (z1 - z0));
			// convert bitmap occupied area from world to bitmap coordinates
			xMin = imageOrigin.X + (xMin - x0) * imageScale.X;
			xMax = imageOrigin.X + (xMax - x0) * imageScale.X;
			zMin = imageOrigin.Y + (z0 - zMin) * imageScale.Y + imageSize.Y;
			zMax = imageOrigin.Y + (z0 - zMax) * imageScale.Y + imageSize.Y;
			// create bitmap

			MapMode mode = inGame ? MapMode.InGame : MapMode.Preview;
			if (follower != null)
			{
				mode = MapMode.SecondaryTrack;
			}
			Bitmap b = new Bitmap(Width, Height, inGame ? System.Drawing.Imaging.PixelFormat.Format32bppArgb
				: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(b);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			g.Clear(mapColors[(int)mode].background);

			for (int i = 0; i < CurrentRoute.Tracks.Count; i++)
			{
				int key = CurrentRoute.Tracks.ElementAt(i).Key;
				int finalElement = Math.Min(CurrentRoute.Tracks[key].Elements.Length, lastUsedElement);
				if (i == 0)
				{
					DrawRailPath(g, mode, key, firstUsedElement, lastUsedElement, imageOrigin, imageSize, imageScale, x0, z0);
				}
				else
				{
					int startElement = -1;
					for (int el = firstUsedElement; el < finalElement; el++)
					{
						if (CurrentRoute.Tracks[key].Elements[el].IsDriveable)
						{
							if (startElement == -1)
							{
								startElement = el;
							}
							
						}
						else
						{
							// For correct path drawing, the end-point is the second non-drivable element
							if (el > 0 && !CurrentRoute.Tracks[key].Elements[el -1].IsDriveable && startElement != -1)
							{
								DrawRailPath(g, mode, key, startElement, el, imageOrigin, imageSize, imageScale, x0, z0);
								startElement = -1;
							}
						}
					}

					if (startElement != -1)
					{
						DrawRailPath(g, mode, key, startElement, finalElement, imageOrigin, imageSize, imageScale, x0, z0);
					}
					
				}
			}

			if (mode == MapMode.SecondaryTrack)
			{
				RestrictedRouteRange(follower, drawRadius, out int firstTrackUsedElement, out int lastTrackUsedElement);
				DrawPlayerPath(g, follower, firstTrackUsedElement, lastTrackUsedElement, imageOrigin, imageSize, imageScale, x0, z0);
			}

			
			if (follower != null)
			{
				switchPositions = new Dictionary<Guid, Vector2>();
				// Find switches
				for (int t = 0; t < CurrentRoute.Tracks.Count; t++)
				{
					int key = CurrentRoute.Tracks.ElementAt(t).Key;
					int finalElement = Math.Min(CurrentRoute.Tracks[key].Elements.Length, lastUsedElement);
					for (int i = firstUsedElement; i <= finalElement; i++)
					{
						for (int j = 0; j < CurrentRoute.Tracks[key].Elements[i].Events.Count; j++)
						{
							double x = CurrentRoute.Tracks[key].Elements[i].WorldPosition.X;
							double y = CurrentRoute.Tracks[key].Elements[i].WorldPosition.Z;
							x = imageOrigin.X + (x - x0) * imageScale.X;
							y = imageOrigin.Y + (z0 - y) * imageScale.Y + imageSize.Y;
							// NOTE: key will appear twice, once per track but we only want the first instance
							if (CurrentRoute.Tracks[key].Elements[i].Events[j] is SwitchEvent se && !switchPositions.ContainsKey(se.Index))
							{
								switchPositions.Add(se.Index, new Vector2(x, y));
								// draw circle
								RectangleF r = new RectangleF((float)x - StationRadius, (float)y - StationRadius,
									StationDiameter, StationDiameter);
								g.FillEllipse(mapColors[(int)mode].inactStatnFill, r);
								g.DrawEllipse(mapColors[(int)mode].inactStatnBrdr, r);
							}
						}
					}
				}
			}
			
			for (int i = firstUsedElement; i <= lastUsedElement; i++)
			{
				for (int j = 0; j < CurrentRoute.Tracks[0].Elements[i].Events.Count; j++)
				{
					if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent e)
					{
						if (CurrentRoute.Stations[e.StationIndex].Name != string.Empty)
						{
							double x = CurrentRoute.Tracks[0].Elements[i].WorldPosition.X;
							double y = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Z;
							x = imageOrigin.X + (x - x0) * imageScale.X;
							y = imageOrigin.Y + (z0 - y) * imageScale.Y + imageSize.Y;
							// station circle
							RectangleF r = new RectangleF((float)x - StationRadius, (float)y - StationRadius,
								StationDiameter, StationDiameter);
							bool q = CurrentRoute.Stations[e.StationIndex].PlayerStops();
							g.FillEllipse(q ? mapColors[(int)mode].actStatnFill : mapColors[(int)mode].inactStatnFill, r);
							g.DrawEllipse(q ? mapColors[(int)mode].actStatnBrdr : mapColors[(int)mode].inactStatnBrdr, r);
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

			// STATION ICONS
			for (int i = firstUsedElement; i <= lastUsedElement; i++)
			{
				for (int j = 0; j < CurrentRoute.Tracks[0].Elements[i].Events.Count; j++)
				{
					if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent e)
					{
						if (CurrentRoute.Stations[e.StationIndex].Name != string.Empty)
						{
							double x = CurrentRoute.Tracks[0].Elements[i].WorldPosition.X;
							double y = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Z;
							x = imageOrigin.X + (x - x0) * imageScale.X;
							y = imageOrigin.Y + (z0 - y) * imageScale.Y + imageSize.Y;
							// station circle
							RectangleF r = new RectangleF((float)x - StationRadius, (float)y - StationRadius,
								StationDiameter, StationDiameter);
							bool q = CurrentRoute.Stations[e.StationIndex].PlayerStops();
							g.FillEllipse(q ? mapColors[(int)mode].actStatnFill : mapColors[(int)mode].inactStatnFill, r);
							g.DrawEllipse(q ? mapColors[(int)mode].actStatnBrdr : mapColors[(int)mode].inactStatnBrdr, r);
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
				double wh = imageSize.X * imageSize.Y;
				Font f = new Font(FontFamily.GenericSansSerif, wh < 65536.0 ? 9.0f : 10.0f, GraphicsUnit.Pixel);
				for (int i = firstUsedElement; i <= lastUsedElement; i++)
				{
					for (int j = 0; j < CurrentRoute.Tracks[0].Elements[i].Events.Count; j++)
					{
						if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent e)
						{
							if (CurrentRoute.Stations[e.StationIndex].Name != string.Empty)
							{
								double x = CurrentRoute.Tracks[0].Elements[i].WorldPosition.X;
								double y = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Z;
								x = imageOrigin.X + (x - x0) * imageScale.X;
								y = imageOrigin.Y + (z0 - y) * imageScale.Y + imageSize.Y;
								bool stop = CurrentRoute.Stations[e.StationIndex].PlayerStops();
								string t = CurrentRoute.Stations[e.StationIndex].Name;
								SizeF m = g.MeasureString(t, f, Width, StringFormat.GenericDefault);
								double sx = CurrentRoute.Tracks[0].Elements[i].WorldSide.X;
								double sz = CurrentRoute.Tracks[0].Elements[i].WorldSide.Z;
								double xt, yt;
								if (Math.Sign(sx) == Math.Sign(sz))
								{
									// descending
									bool o = (x - imageOrigin.X) * (imageSize.Y - y) <= (imageSize.X - x) * (y - imageOrigin.Y);
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
									bool o = (imageSize.Y - y) * (imageSize.X - x) <= (x - imageOrigin.X) * (y - imageOrigin.Y);
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
								if (xt < imageOrigin.X)
									xt = imageOrigin.X;
								else if (xt + m.Width > imageSize.X)
									xt = imageSize.X - m.Width;
								if (yt < imageOrigin.Y)
									yt = imageOrigin.Y;
								else if (yt + m.Height > imageSize.Y)
									yt = imageSize.Y - m.Height;
								RectangleF r = new RectangleF((float)xt - 1.0f, (float)yt - 1.0f, m.Width + 2.0f, m.Height + 2.0f);
								g.FillRectangle(stop ? mapColors[(int)mode].actNameFill : mapColors[(int)mode].inactNameFill,
									r.Left, r.Top, r.Width, r.Height);
								g.DrawRectangle(stop ? mapColors[(int)mode].actNameBrdr : mapColors[(int)mode].inactNameBrdr,
									r.Left, r.Top, r.Width, r.Height);
								g.DrawString(t, f, stop ? mapColors[(int)mode].actNameText : mapColors[(int)mode].inactNameText,
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

			
			if (inGame)
			{
				if (follower == null)
				{
					// in-game map shrinks bitmap
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
				}
				else
				{
					// switch change dialog should cover the whole screen
					xMin = 0;
					xMax = Width;
					zMin = 0;
					zMax = Height;
				}
				
				Bitmap nb = new Bitmap((int)(xMax - xMin + 1.0), (int)(zMax - zMin + 1.0));	// round up
				g = Graphics.FromImage(nb);
				g.DrawImage(b, (int)-xMin, (int)-zMin);										// round down
				// set total bitmap world X and Z ranges from bitmap ranges
				lastRouteMinX = (int)((xMin - imageOrigin.X) / imageScale.X + x0);
				lastRouteMaxX = (int)((xMax - imageOrigin.X) / imageScale.X + x0);
				lastRouteMinZ = (int)(z0 - (zMax - imageOrigin.Y - imageSize.Y) / imageScale.Y);
				lastRouteMaxZ = (int)(z0 - (zMin - imageOrigin.Y - imageSize.Y) / imageScale.Y);
				return nb;
			}
			//set total bitmap X and Z ranges
			lastRouteMinX = (int)(x0 - imageOrigin.X * (x1 - x0) / imageSize.X);
			lastRouteMaxX = (int)(x1 + (Width - imageSize.X - imageOrigin.X) * (x1 - x0) / imageSize.X);
			lastRouteMinZ = (int)(z0 - imageOrigin.Y * (z1 - z0) / imageSize.Y);
			lastRouteMaxZ = (int)(z1 + (Height- imageSize.Y - imageOrigin.Y) * (z1 - z0) / imageSize.Y);
			return b;
		}

		/// <summary>Creates the route gradient profile.</summary>
		/// <returns>The route gradient profile as Bitmap.</returns>
		/// <param name="Width">The width of the bitmap to create.</param>
		/// <param name="Height">The height of the bitmap to create.</param>
		/// <param name="inGame"><c>true</c> = bitmap for in-game overlay | <c>false</c> = for standard window.</param>
		public static Bitmap CreateRouteGradientProfile(int Width, int Height, bool inGame)
		{
			if (CurrentRoute.Tracks[0].Elements == null)
			{
				return new Bitmap(1,1);
			}
			if (CurrentRoute.Tracks[0].Elements.Length > 36 && CurrentRoute.Stations.Length == 0)
			{
				// If we have track elements, but no stations, show a specific error message, rather
				// than the more generic one thrown later
				// NOTE: Will throw the generic error message on routes shorter than 900m with no stations
				throw new InvalidDataException(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","route_corrupt_nostations"}));
			}
			// Track elements are assumed to be all of the same length, and this length
			// is used as measure unit, rather than computing the incremental track length
			// in any 'real world' unit (like m).

			// HORIZONTAL RANGE: find first and last used element based on stations
			TotalRouteRange(out int totalElements, out int firstUsedElement, out int lastUsedElement);
			// VERTICAL RANGE
			double y0 = double.PositiveInfinity, y1 = double.NegativeInfinity;
			for (int i = firstUsedElement; i <= lastUsedElement; i++)
			{
				double y = CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y;
				if (y < y0) y0 = y;
				if (y > y1) y1 = y;
			}
			if (y0 >= y1 - 1.0)
				y0 = y1 - 1.0;

			// allow for some padding around actual data
			double w = Width - (LeftPad+RightPad);
			double h = Height - (TopPad+BottomPad+TrackOffsPad);
			// horizontal and vertical scale
			double nd = w / (lastUsedElement - firstUsedElement);
			double yd = h / (y1 - y0);
			// set total bitmap track position range; used by in-game profile to place
			// the current position of the trains; as the train positions are known as track positions,
			// actual track positions are needed here, rather than indices into the track element array.
			double minX = CurrentRoute.Tracks[0].Elements[firstUsedElement].StartingTrackPosition;
			double maxX = CurrentRoute.Tracks[0].Elements[lastUsedElement].StartingTrackPosition;
			double offX = LeftPad * (maxX - minX) / w;
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
				double y = TopPad + (h - 0.5 * (-CurrentRoute.Atmosphere.InitialElevation - y0) * yd);
				double x1 = LeftPad + (lastUsedElement - firstUsedElement) * nd;
				g.FillRectangle(mapColors[mode].belowSeaFill, (float)LeftPad, (float)y, (float)x1, (float)(TopPad + h) - (float)y);
				g.DrawLine(mapColors[mode].belowSeaBrdr, (float)LeftPad, (float)y, (float)x1, (float)y);
			}
			// GRADIENT PROFILE
			{
				totalElements = lastUsedElement - firstUsedElement + 1;
				PointF[] p = new PointF[totalElements + 2];
				p[0] = new PointF((float)LeftPad, (float)(TopPad + h));
				for (int i = firstUsedElement; i <= lastUsedElement; i++)
				{
					double x = LeftPad + (i - firstUsedElement) * nd;
					double y = TopPad + (h - 0.5 *
						(CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y - y0) * yd);
					p[i -firstUsedElement + 1] = new PointF((float)x, (float)y);
				}
				p[totalElements + 1] = new PointF((float)(LeftPad + (totalElements - 1) * nd), (float)(TopPad + h));
				g.FillPolygon(mapColors[mode].elevFill, p);
				for (int i = 1; i < totalElements; i++)
					g.DrawLine(mapColors[mode].elevBrdr, p[i], p[i + 1]);
				g.DrawLine(mapColors[mode].elevBrdr, 0.0f, (float)(TopPad + h), (float)Width, (float)(TopPad + h));
			}
			// STATION NAMES
			{
				Font f = new Font(FontFamily.GenericSansSerif, 12.0f, GraphicsUnit.Pixel);
				StringFormat m = new StringFormat();
				for (int i = firstUsedElement; i <= lastUsedElement; i++)
				{
					for (int j = 0; j < CurrentRoute.Tracks[0].Elements[i].Events.Count; j++)
					{
						if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent e)
						{
							if (CurrentRoute.Stations[e.StationIndex].Name != string.Empty)
							{
								bool stop = CurrentRoute.Stations[e.StationIndex].PlayerStops();
								if (CurrentRoute.Stations[e.StationIndex].Name.IsJapanese())
								{
									m.Alignment = StringAlignment.Near;
									m.LineAlignment = StringAlignment.Near;
									double x = LeftPad + (i - firstUsedElement) * nd;
									double y = TopPad + (h - 0.5 *
										(CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y - y0) * yd);
									string t = CurrentRoute.Stations[e.StationIndex].Name;
									float tx = 0.0f, ty = (float)TopPad;
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
									double x = LeftPad + (i - firstUsedElement) * nd;
									double y = TopPad + (h - 0.5 *
										(CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y - y0) * yd);
									g.RotateTransform(-90.0f);
									g.TranslateTransform((float)x, (float)TopPad, System.Drawing.Drawing2D.MatrixOrder.Append);
									g.DrawString(CurrentRoute.Stations[e.StationIndex].Name, f,
										stop ? mapColors[mode].actNameText : mapColors[mode].inactNameText,
										new PointF(0.0f, -5.0f), m);
									g.ResetTransform();
									SizeF s = g.MeasureString(CurrentRoute.Stations[e.StationIndex].Name, f);
									g.DrawLine(stop ? mapColors[mode].actNameBrdr : mapColors[mode].inactNameBrdr,
										new PointF((float)x, (float)(TopPad + s.Width + 4)), new PointF((float)x, (float)y));
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
				int k = TrackOffDist * totalElements / Width;
				if (k == 0)
				{
					if (!inGame)
					{
						//If k is equal to zero, this generally means that the WithTrack section is missing from our routefile
						//Adding zero to the loop control variable will also produce an infinite loop, so that's a bad idea too
						throw new InvalidDataException(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","route_corrupt_withtrack"}));
					}
					/*
					 * A route with a single station can somehow sometimes work OK in preview but not in-game
					 * Whilst the routefile is probably broken don't chuck the exception here as it takes down the loader
					 */
					k = 1;
				}
				
				for (int i = firstUsedElement; i <= lastUsedElement; i += k)
				{
					double x = LeftPad + (i - firstUsedElement) * nd;
					double y = (CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y - y0) * yd;
					// track offset label
					if (x < w)
					{
						string t = ((int)Math.Round(CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition)).ToString(CultureInfo.InvariantCulture);
						g.DrawString(t + "m", f, mapColors[mode].actNameText, (float)x, (float)(TopPad + h + TrackOffY));
					}
					// route height at track offset (with measure and vertical line)
					{
						y = TopPad + (h - 0.5 * y) + 2.0f;
						string t = ((int)Math.Round(CurrentRoute.Atmosphere.InitialElevation + CurrentRoute.Tracks[0].Elements[i].WorldPosition.Y)).ToString(CultureInfo.InvariantCulture);
						SizeF s = g.MeasureString(t, fs);
						if (y < TopPad + h - s.Width - 10.0)
						{
							g.RotateTransform(-90.0f);
							g.TranslateTransform((float)x, (float)y + 4.0f, System.Drawing.Drawing2D.MatrixOrder.Append);
							g.DrawString(t + "m", fs, mapColors[mode].actNameText, 0.0f, 0.0f, m);
							g.ResetTransform();
							g.DrawLine(mapColors[mode].inactNameBrdr,
								(float)x, (float)(y + s.Width + 12.0), (float)x, (float)(TopPad + h));
						}
					}
				}
			}
			// finalize
			return b;
		}

		private static void DrawPlayerPath(Graphics g, TrackFollower follower, int firstUsedElement, int lastUsedElement, Vector2 imageOrigin, Vector2 imageSize, Vector2 imageScale, double x0, double z0)
		{
			int key = follower.TrackIndex;
			Track currentTrack = CurrentRoute.Tracks[key];
			int start = 0;
			int elementsToDraw = Math.Min(lastUsedElement - firstUsedElement + 1, currentTrack.Elements.Length - firstUsedElement);

			PointF[] p = new PointF[elementsToDraw];
			for (int i = 0; i < elementsToDraw; i++)
			{
				double x = currentTrack.Elements[i + firstUsedElement].WorldPosition.X;
				double z = currentTrack.Elements[i + firstUsedElement].WorldPosition.Z;
				x = imageOrigin.X + (x - x0) * imageScale.X;
				z = imageOrigin.Y + (z0 - z) * imageScale.Y + imageSize.Y;
				p[i] = new PointF((float)x, (float)z);
				if (currentTrack.Elements[i].Events == null)
				{
					continue;
				}
				Font boldFont = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Bold, GraphicsUnit.Pixel);
				int nextTrackIndex = -1;
				for (int j = 0; j < currentTrack.Elements[i + firstUsedElement].Events.Count; j++)
				{
					if (currentTrack.Elements[i + firstUsedElement].Events[j] is LimitChangeEvent lim)
					{
						// turns out centering text in a circle using System.Drawing is a PITA
						// numbers are fudges, need to check whether they work OK on non windows....
						string limitString = Math.Round(lim.NextSpeedLimit * 3.6, 2).ToString(CultureInfo.InvariantCulture);
						float radius = g.MeasureString(limitString, boldFont).Width * 0.9f;
						RectangleF r = new RectangleF((float)x - radius - 20, (float)z - radius,
							radius * 2.0f, radius * 2.0f);
						g.FillEllipse(mapColors[0].limitFill, r);
						g.DrawEllipse(mapColors[0].limitBrdr, r);
								
						g.DrawString(limitString, boldFont, Brushes.Black,
							(float)x - 20 - (radius /2), (float)z - (radius * 0.45f));
					}
					if (currentTrack.Elements[i + firstUsedElement].Events[j] is SwitchEvent se)
					{
						// switch to different track if appropriate
						if (CurrentRoute.Switches[se.Index].Direction == TrackDirection.Forwards && CurrentRoute.Switches[se.Index].CurrentlySetTrack != key)
						{
							key = CurrentRoute.Switches[se.Index].CurrentlySetTrack;
							currentTrack = CurrentRoute.Tracks[key];
							j = 0;
							if (i + firstUsedElement > currentTrack.Elements.Length)
							{
								elementsToDraw = i;
								goto End;
							}
							continue;
						}
					}
					
					if (!currentTrack.Elements[i + firstUsedElement].IsDriveable || currentTrack.Elements[i + firstUsedElement].Events[j] is TrackEndEvent)
					{
						// Playable path has ended [e.g. buffers etc]
						elementsToDraw = i;
						break;
					}
				}

				if (nextTrackIndex != -1)
				{
					currentTrack = CurrentRoute.Tracks[nextTrackIndex];
				}
			}
			End:
			DrawSegmentedCurve(g, Pens.Blue, p, start, elementsToDraw - 1);
		}

		private static void DrawRailPath(Graphics g, MapMode mode, int key, int firstUsedElement, int lastUsedElement, Vector2 imageOrigin, Vector2 imageSize, Vector2 imageScale, double x0, double z0)
		{
			Track currentTrack = CurrentRoute.Tracks[key];
			int start = 0;
			bool atc = false;
			int elementsToDraw = Math.Min(lastUsedElement - firstUsedElement + 1, currentTrack.Elements.Length - firstUsedElement);

			PointF[] p = new PointF[elementsToDraw];
			for (int i = 0; i < elementsToDraw; i++)
			{
				double x = currentTrack.Elements[i + firstUsedElement].WorldPosition.X;
				double z = currentTrack.Elements[i + firstUsedElement].WorldPosition.Z;
				x = imageOrigin.X + (x - x0) * imageScale.X;
				z = imageOrigin.Y + (z0 - z) * imageScale.Y + imageSize.Y;
				p[i] = new PointF((float)x, (float)z);
				if (currentTrack.Elements[i].Events == null)
				{
					continue;
				}
				// ATS / ATC
				// for each track element, look for a StationStartEvent
				for (int j = 0; j < currentTrack.Elements[i + firstUsedElement].Events.Count; j++)
				{

					if (currentTrack.Elements[i + firstUsedElement].Events[j] is StationStartEvent)
					{
						StationStartEvent e =
							(StationStartEvent)CurrentRoute.Tracks[0].Elements[i + firstUsedElement].Events[j];
						// if StationStartEvent found, look for a change in ATS/ATC control;
						// if there is a change, draw all previous track elements
						// with colour for the previous control state
						if (CurrentRoute.Stations[e.StationIndex].SafetySystem == SafetySystem.Atc)
						{
							if (!atc)
							{
								atc = true;
								if (i - start - 1 > 0)
									g.DrawCurve(mapColors[(int)mode].normalMap, p, start, i - start - 1);
								start = i;
							}
						}
						else
						{
							if (atc)
							{
								atc = false;
								if (i - start - 1 > 0)
									g.DrawCurve(mapColors[(int)mode].atcMap, p, start, i - start - 1);
								start = i;
							}
						}
						break;
					}
				}
			}

			if (mode == MapMode.SecondaryTrack)
			{
				DrawSegmentedCurve(g, Pens.LightGray, p, start, elementsToDraw - start - 1);
			}
			else
			{
				DrawSegmentedCurve(g, atc ? mapColors[(int)mode].atcMap : mapColors[(int)mode].normalMap, p, start, elementsToDraw - start - 1);
			}
			// draw all remaining track element not drawn yet
		}

		/// <summary>Draws a segmented curve from a list of points</summary>
		/// <param name="Graphics"></param>
		/// <param name="Pen"></param>
		/// <param name="Points">The list of points</param>
		/// <param name="Start">The starting point within the list</param>
		/// <param name="Length">The number of points (segments) to draw</param>
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


		/// <summary>Finds the route range actually used by stations (with some margin before and after.</summary>
		/// <param name="totalElements">Route total number of elements</param>
		/// <param name="firstUsedElement">First element in used range</param>
		/// <param name="lastUsedElement">Last element in used range</param>
		private static void TotalRouteRange(out int totalElements, out int firstUsedElement, out int lastUsedElement)
		{
			// find first and last track element actually used by stations
			totalElements	= CurrentRoute.Tracks[0].Elements.Length;
			firstUsedElement	= totalElements - 1;
			lastUsedElement	= 0;
			for (int i = 0; i < CurrentRoute.Tracks[0].Elements.Length; i++)
			{
				for (int j = 0; j < CurrentRoute.Tracks[0].Elements[i].Events.Count; j++)
				{
					if (CurrentRoute.Tracks[0].Elements[i].Events[j] is StationStartEvent)
					{
						if (i < firstUsedElement) firstUsedElement = i;
						if (i > lastUsedElement) lastUsedElement = i;
						break;
					}
				}
			}
			// Allow for 4 track units before first station and 8 track units after the last one
			firstUsedElement -= 4;
			lastUsedElement += 8;
			// But not outside of actual track element array!
			if (firstUsedElement < 0) firstUsedElement = 0;
			if (lastUsedElement >= CurrentRoute.Tracks[0].Elements.Length)
				lastUsedElement = CurrentRoute.Tracks[0].Elements.Length - 1;
			if (lastUsedElement <= firstUsedElement)		// neither a 0-length or 'negative' track!
				lastUsedElement = firstUsedElement + 1;
		}

		/// <summary>Finds the route range for the specified track follower and draw radius</summary>
		/// <param name="follower">The track follower</param>
		/// <param name="drawRadius">The draw radius</param>
		/// <param name="firstUsedElement">The index of the first used element</param>
		/// <param name="lastUsedElement">The index of the last used element</param>
		private static void RestrictedRouteRange(TrackFollower follower, int drawRadius, out int firstUsedElement, out int lastUsedElement)
		{
			lastUsedElement = 0;
			firstUsedElement = -1;
			double st = Math.Max(0, follower.TrackPosition - drawRadius);
			double et = follower.TrackPosition + drawRadius;
			for (int i = 0; i < CurrentRoute.Tracks[follower.TrackIndex].Elements.Length; i++)
			{
				if (CurrentRoute.Tracks[follower.TrackIndex].Elements[i].StartingTrackPosition > st && CurrentRoute.Tracks[follower.TrackIndex].Elements[i].IsDriveable && firstUsedElement == -1)
				{
					firstUsedElement = i == 0 ? 0 : i - 1;
				}
				if (firstUsedElement != -1 && (CurrentRoute.Tracks[follower.TrackIndex].Elements[i].StartingTrackPosition > et || !CurrentRoute.Tracks[follower.TrackIndex].Elements[i].IsDriveable))
				{
					lastUsedElement = i == 0 ? 0 : i - 1;
					break;
				}
			}
		}
	}
}
