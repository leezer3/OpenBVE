using System;
using System.Drawing;

namespace OpenBve {
	internal static class Illustrations {

		// create route map
		internal static Bitmap CreateRouteMap(int Width, int Height) {
			// find first and last used element based on stations
			int n = TrackManager.CurrentTrack.Elements.Length;
			int n0 = n - 1;
			int n1 = 0;
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
					if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
						if (i < n0) n0 = i;
						if (i > n1) n1 = i;
					}
				}
			}
			n0 -= 4;
			n1 += 8;
			if (n0 < 0) n0 = 0;
			if (n1 >= TrackManager.CurrentTrack.Elements.Length) n1 = TrackManager.CurrentTrack.Elements.Length - 1;
			if (n1 <= n0) n1 = n0 + 1;
			// find dimensions
			double x0 = double.PositiveInfinity, z0 = double.PositiveInfinity;
			double x1 = double.NegativeInfinity, z1 = double.NegativeInfinity;
			for (int i = n0; i <= n1; i++) {
				double x = TrackManager.CurrentTrack.Elements[i].WorldPosition.X;
				double z = TrackManager.CurrentTrack.Elements[i].WorldPosition.Z;
				if (x < x0) x0 = x;
				if (x > x1) x1 = x;
				if (z < z0) z0 = z;
				if (z > z1) z1 = z;
			}
			if (x0 >= x1 - 1.0) x0 = x1 - 1.0;
			if (z0 >= z1 - 1.0) z0 = z1 - 1.0;
			double wrh = (double)Width / (double)Height;
			if ((x1 - x0) / (z1 - z0) <= wrh) {
				double dx = 0.5 * (z1 - z0) * wrh;
				double px = 0.5 * (x0 + x1);
				x0 = px - dx;
				x1 = px + dx;
			} else {
				double dz = 0.5 * (x1 - x0) / wrh;
				double pz = 0.5 * (z0 + z1);
				z0 = pz - dz;
				z1 = pz + dz;
			}
			double xd = 1.0 / (x1 - x0);
			double zd = 1.0 / (z1 - z0);
			double ox = 8.0, oy = 8.0;
			double w = (double)(Width - 16);
			double h = (double)(Height - 16);
			// create bitmap
			Bitmap b = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(b);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			g.Clear(Color.White);
			// draw route path
			{
				int start = 0;
				bool atc = false;
				PointF[] p = new PointF[n];
				for (int i = 0; i < n; i++) {
					double x = TrackManager.CurrentTrack.Elements[i].WorldPosition.X;
					double z = TrackManager.CurrentTrack.Elements[i].WorldPosition.Z;
					x = ox + w * (x - x0) * xd;
					z = oy + h + h * zd * (z0 - z);
					p[i] = new PointF((float)x, (float)z);
					// ats/atc
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
						if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
							TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
							if (Game.Stations[e.StationIndex].SafetySystem == Game.SafetySystem.Atc) {
								if (!atc) {
									atc = true;
									if (i - start - 1 > 0) g.DrawCurve(Pens.Black, p, start, i - start - 1);
									start = i;
								}
							} else {
								if (atc) {
									atc = false;
									if (i - start - 1 > 0) g.DrawCurve(Pens.DarkRed, p, start, i - start - 1);
									start = i;
								}
							}
						}
					}
				}
				DrawSegmentedCurve(g, atc ? Pens.DarkRed : Pens.Black, p, start, n - start - 1);
			}
			// draw station circles
			for (int i = 0; i < n; i++) {
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
					if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
						TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
						if (Game.Stations[e.StationIndex].Name != string.Empty) {
							double x = TrackManager.CurrentTrack.Elements[i].WorldPosition.X;
							double y = TrackManager.CurrentTrack.Elements[i].WorldPosition.Z;
							x = ox + w * (x - x0) * xd;
							y = oy + h + h * zd * (z0 - y);
							// station circle
							RectangleF r = new RectangleF((float)x - 4.0f, (float)y - 4.0f, 8.0f, 8.0f);
							bool q = Game.PlayerStopsAtStation(e.StationIndex);
							g.FillEllipse(q ? Brushes.SkyBlue : Brushes.LightGray, r);
							g.DrawEllipse(q ? Pens.Black : Pens.Gray, r);
						}
					}
				}
			}
			// draw station names
			{
				double wh = w * h;
				Font f = new Font(FontFamily.GenericSansSerif, wh < 65536.0 ? 9.0f : 10.0f, GraphicsUnit.Pixel);
				for (int i = 0; i < n; i++) {
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
						if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
							TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
							if (Game.Stations[e.StationIndex].Name != string.Empty) {
								double x = TrackManager.CurrentTrack.Elements[i].WorldPosition.X;
								double y = TrackManager.CurrentTrack.Elements[i].WorldPosition.Z;
								x = ox + w * (x - x0) * xd;
								y = oy + h + h * zd * (z0 - y);
								RectangleF r = new RectangleF((float)x - 4.0f, (float)y - 4.0f, 8.0f, 8.0f);
								bool stop = Game.PlayerStopsAtStation(e.StationIndex);
								string t = Game.Stations[e.StationIndex].Name;
								SizeF m = g.MeasureString(t, f, Width, StringFormat.GenericDefault);
								double sx = TrackManager.CurrentTrack.Elements[i].WorldSide.X;
								double sz = TrackManager.CurrentTrack.Elements[i].WorldSide.Z;
								double xt, yt;
								if (Math.Sign(sx) == Math.Sign(sz)) {
									// descending
									bool o = (x - ox) * (h - y) <= (w - x) * (y - oy);
									if (o) {
										// up-right
										xt = x + 6.0;
										yt = y - 6.0 - m.Height;
									} else {
										// down-left
										xt = x - 6.0 - m.Width;
										yt = y + 6.0;
									}
								} else {
									// ascending
									bool o = (h - y) * (w - x) <= (x - ox) * (y - oy);
									if (o) {
										// up-left
										xt = x - 6.0 - m.Width;
										yt = y - 6.0 - m.Height;
									} else {
										// down-right
										xt = x + 6.0;
										yt = y + 6.0;
									}
								}
								if (xt < ox) {
									xt = ox;
								} else if (xt + m.Width > w) {
									xt = w - m.Width;
								}
								if (yt < oy) {
									yt = oy;
								} else if (yt + m.Height > h) {
									yt = h - m.Height;
								}
								r = new RectangleF((float)xt, (float)yt, m.Width, m.Height);
								g.FillRectangle(stop ? Brushes.White : Brushes.LightGray, r.Left - 1.0f, r.Top - 1.0f, r.Width + 2.0f, r.Height + 2.0f);
								g.DrawRectangle(stop ? Pens.Black : Pens.Gray, r.Left - 1.0f, r.Top - 1.0f, r.Width + 2.0f, r.Height + 2.0f);
								g.DrawString(t, f, stop ? Brushes.Black : Brushes.Gray, (float)xt, (float)yt);
							}
						}
					}
				}
			}
			// finalize
			return b;
		}

		// create route gradient profile
		internal static Bitmap CreateRouteGradientProfile(int Width, int Height) {
			// find first and last used element based on stations
			int n = TrackManager.CurrentTrack.Elements.Length;
			int n0 = n - 1;
			int n1 = 0;
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
				for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
					if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
						if (i < n0) n0 = i;
						if (i > n1) n1 = i;
					}
				}
			}
			n0 -= 4;
			n1 += 8;
			if (n0 < 0) n0 = 0;
			if (n1 >= TrackManager.CurrentTrack.Elements.Length) n1 = TrackManager.CurrentTrack.Elements.Length - 1;
			if (n1 <= n0) n1 = n0 + 1;
			// find dimensions
			double y0 = double.PositiveInfinity, y1 = double.NegativeInfinity;
			for (int i = n0; i <= n1; i++) {
				double y = TrackManager.CurrentTrack.Elements[i].WorldPosition.Y;
				if (y < y0) y0 = y;
				if (y > y1) y1 = y;
			}
			if (y0 >= y1 - 1.0) y0 = y1 - 1.0;
			double nd = 1.0 / (double)(n1 - n0);
			double yd = 1.0 / (double)(y1 - y0);
			double ox = 8.0, oy = 8.0;
			double w = (double)(Width - 16);
			double h = (double)(Height - 32);
			// create bitmap
			Bitmap b = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(b);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			g.Clear(Color.White);
			// draw below sea level
			{
				double y = oy + h * (1.0 - 0.5 * (double)(-Game.RouteInitialElevation - y0) * yd);
				double x0 = ox - w * (double)(n0) * nd;
				double x1 = ox + w * (double)(n - n0) * nd;
				g.FillRectangle(Brushes.PaleGoldenrod, (float)x0, (float)y, (float)x1, (float)(oy + h) - (float)y);
				g.DrawLine(Pens.Gray, (float)x0, (float)y, (float)x1, (float)y);
			}
			// draw route path
			{
				PointF[] p = new PointF[n + 2];
				p[0] = new PointF((float)(ox - w * (double)n0 * nd), (float)(oy + h));
				for (int i = 0; i < n; i++) {
					double x = ox + w * (double)(i - n0) * nd;
					double y = oy + h * (1.0 - 0.5 * (double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd);
					p[i + 1] = new PointF((float)x, (float)y);
				}
				p[n + 1] = new PointF((float)(ox + w * (double)(n - n0 - 1) * nd), (float)(oy + h));
				g.FillPolygon(Brushes.Tan, p);
				for (int i = 1; i < n; i++) {
					g.DrawLine(Pens.Black, p[i], p[i + 1]);
				}
				g.DrawLine(Pens.Black, 0.0f, (float)(oy + h), (float)Width, (float)(oy + h));
			}
			// draw station names
			{
				Font f = new Font(FontFamily.GenericSansSerif, 12.0f, GraphicsUnit.Pixel);
				StringFormat m = new StringFormat();
				for (int i = 0; i < n; i++) {
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
						if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
							TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
							if (Game.Stations[e.StationIndex].Name != string.Empty) {
								bool stop = Game.PlayerStopsAtStation(e.StationIndex);
								if (Interface.IsJapanese(Game.Stations[e.StationIndex].Name)) {
									m.Alignment = StringAlignment.Near;
									m.LineAlignment = StringAlignment.Near;
									double x = ox + w * (double)(i - n0) * nd;
									double y = oy + h * (1.0 - 0.5 * (double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd);
									string t = Game.Stations[e.StationIndex].Name;
									float tx = 0.0f, ty = (float)oy;
									for (int k = 0; k < t.Length; k++) {
										SizeF s = g.MeasureString(t.Substring(k, 1), f, 65536, StringFormat.GenericTypographic);
										if (s.Width > tx) tx = s.Width;
									}
									for (int k = 0; k < t.Length; k++) {
										g.DrawString(t.Substring(k, 1), f, stop ? Brushes.Black : Brushes.LightGray, (float)x - 0.5f * tx, ty);
										SizeF s = g.MeasureString(t.Substring(k, 1), f, 65536, StringFormat.GenericTypographic);
										ty += s.Height;
									}
									g.DrawLine(stop ? Pens.Gray : Pens.LightGray, new PointF((float)x, ty + 4.0f), new PointF((float)x, (float)y));
								} else {
									m.Alignment = StringAlignment.Far;
									m.LineAlignment = StringAlignment.Near;
									double x = ox + w * (double)(i - n0) * nd;
									double y = oy + h * (1.0 - 0.5 * (double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd);
									g.RotateTransform(-90.0f);
									g.TranslateTransform((float)x, (float)oy, System.Drawing.Drawing2D.MatrixOrder.Append);
									g.DrawString(Game.Stations[e.StationIndex].Name, f, stop ? Brushes.Black : Brushes.LightGray, new PointF(0.0f, -5.0f), m);
									g.ResetTransform();
									SizeF s = g.MeasureString(Game.Stations[e.StationIndex].Name, f);
									g.DrawLine(stop ? Pens.Gray : Pens.LightGray, new PointF((float)x, (float)(oy + s.Width + 4)), new PointF((float)x, (float)y));
								}
							}
						}
					}
				}
			}
			// draw route markers
			{
				Font f = new Font(FontFamily.GenericSansSerif, 10.0f, GraphicsUnit.Pixel);
				Font fs = new Font(FontFamily.GenericSansSerif, 9.0f, GraphicsUnit.Pixel);
				StringFormat m = new StringFormat();
				m.Alignment = StringAlignment.Far;
				m.LineAlignment = StringAlignment.Center;
				System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
				int k = 48 * n / Width;
				for (int i = 0; i < n; i += k) {
					double x = ox + w * (double)(i - n0) * nd;
					double y = (double)(TrackManager.CurrentTrack.Elements[i].WorldPosition.Y - y0) * yd;
					if (x < w) {
						string t = ((int)Math.Round(TrackManager.CurrentTrack.Elements[i].StartingTrackPosition)).ToString(Culture);
						g.DrawString(t + "m", f, Brushes.Black, (float)x, (float)(oy + h + 6.0));
					}
					{
						y = oy + h * (1.0 - 0.5 * y) + 2.0f;
						string t = ((int)Math.Round(Game.RouteInitialElevation + TrackManager.CurrentTrack.Elements[i].WorldPosition.Y)).ToString(Culture);
						SizeF s = g.MeasureString(t, fs);
						if (y < oy + h - (double)s.Width - 10.0) {
							g.RotateTransform(-90.0f);
							g.TranslateTransform((float)x, (float)y + 4.0f, System.Drawing.Drawing2D.MatrixOrder.Append);
							g.DrawString(t + "m", fs, Brushes.Black, 0.0f, 0.0f, m);
							g.ResetTransform();
							g.DrawLine(Pens.Gray, (float)x, (float)(y + s.Width + 12.0), (float)x, (float)(oy + h));
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
			for (int k = Start; k <= End; k += Count) {
				int m = End - k + 1;
				if (m > Count) m = Count;
				Graphics.DrawCurve(Pen, Points, k, m);
			}
		}

	}
}