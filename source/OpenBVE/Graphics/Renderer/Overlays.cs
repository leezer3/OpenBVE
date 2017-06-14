using System;
using OpenBveApi.Colors;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/* --------------------------------------------------------------
		 * This file contains the base drawing routines for screen overlays
		 * -------------------------------------------------------------- */
		
		/// <summary>Is called once by the main renderer loop, in order to render all overlays shown on the screen</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		private static void RenderOverlays(double TimeElapsed)
		{
			//Initialize openGL
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			GL.Enable(EnableCap.Blend); BlendEnabled = true;
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0.0, (double)Screen.Width, (double)Screen.Height, 0.0, -1.0, 1.0);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			GL.LoadIdentity();
			//Check which overlays to show
			if (CurrentOutputMode == OutputMode.Default)
			{
				//Route info overlay (if selected)
				Game.routeInfoOverlay.Show();
				//HUD
				for (int i = 0; i < Interface.CurrentHudElements.Length; i++)
				{
					string Command = Interface.CurrentHudElements[i].Subject.ToLowerInvariant();
					switch (Command)
					{
						case "messages":
						{
							RenderGameMessages(Interface.CurrentHudElements[i], TimeElapsed);
						} 
						break;
						case "scoremessages":
						{
							RenderScoreMessages(Interface.CurrentHudElements[i], TimeElapsed);
						}
						break;
						case "ats":
						{
							RenderATSLamps(Interface.CurrentHudElements[i], TimeElapsed);
						}
						break;
						default:
						{
							RenderHUDElement(Interface.CurrentHudElements[i], TimeElapsed);
						}
						break;
					}
				}
				//Marker textures
				if (Interface.CurrentOptions.GameMode != Interface.GameMode.Expert)
				{
					double y = 8.0;
					for (int i = 0; i < Game.MarkerTextures.Length; i++)
					{
						if (Textures.LoadTexture(Game.MarkerTextures[i], Textures.OpenGlTextureWrapMode.ClampClamp))
						{
							double w = (double)Game.MarkerTextures[i].Width;
							double h = (double)Game.MarkerTextures[i].Height;
							GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
							RenderOverlayTexture(Game.MarkerTextures[i], (double)Screen.Width - w - 8.0, y, (double)Screen.Width - 8.0, y + h);
							y += h + 8.0;
						}
					}
				}
				//Timetable overlay
				//NOTE: Only affects auto-generated timetable, possibly change this inconsistant behaviour
				if (Timetable.CurrentTimetable == Timetable.TimetableState.Default)
				{
					// default
					if (Textures.LoadTexture(Timetable.DefaultTimetableTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						int w = Timetable.DefaultTimetableTexture.Width;
						int h = Timetable.DefaultTimetableTexture.Height;
						GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
						RenderOverlayTexture(Timetable.DefaultTimetableTexture, (double)(Screen.Width - w), Timetable.DefaultTimetablePosition, (double)Screen.Width, (double)h + Timetable.DefaultTimetablePosition);
					}
				}
				else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom & Timetable.CustomObjectsUsed == 0)
				{
					// custom
					if (Textures.LoadTexture(Timetable.CurrentCustomTimetableDaytimeTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						int w = Timetable.CurrentCustomTimetableDaytimeTexture.Width;
						int h = Timetable.CurrentCustomTimetableDaytimeTexture.Height;
						GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
						RenderOverlayTexture(Timetable.CurrentCustomTimetableDaytimeTexture, (double)(Screen.Width - w), Timetable.CustomTimetablePosition, (double)Screen.Width, (double)h + Timetable.CustomTimetablePosition);
					}
					if (Textures.LoadTexture(Timetable.CurrentCustomTimetableDaytimeTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						int w = Timetable.CurrentCustomTimetableDaytimeTexture.Width;
						int h = Timetable.CurrentCustomTimetableDaytimeTexture.Height;
						float alpha;
						if (Timetable.CurrentCustomTimetableDaytimeTexture != null)
						{
							double t = (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.TrackPosition - TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.PreviousTrackPosition) / (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.NextTrackPosition - TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.PreviousTrackPosition);
							alpha = (float)((1.0 - t) * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.PreviousBrightness + t * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.NextBrightness);
						}
						else
						{
							alpha = 1.0f;
						}
						GL.Color4(1.0f, 1.0f, 1.0f, alpha);
						RenderOverlayTexture(Timetable.CurrentCustomTimetableDaytimeTexture, (double)(Screen.Width - w), Timetable.CustomTimetablePosition, (double)Screen.Width, (double)h + Timetable.CustomTimetablePosition);
					}
				}
			}
			else if (CurrentOutputMode == OutputMode.Debug)
			{
				RenderDebugOverlays();
			}
			// air brake debug output
			if (Interface.CurrentOptions.GameMode != Interface.GameMode.Expert & OptionBrakeSystems)
			{
				RenderBrakeSystemDebug();
			}
			//If paused, fade out the screen & write PAUSE
			if (Game.CurrentInterface == Game.InterfaceType.Pause)
			{
				// pause
				GL.Color4(0.0f, 0.0f, 0.0f, 0.5f);
				RenderOverlaySolid(0.0, 0.0, (double)Screen.Width, (double)Screen.Height);
				GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
				DrawString(Fonts.VeryLargeFont, "PAUSE", new System.Drawing.Point(Screen.Width / 2, Screen.Height / 2), TextAlignment.CenterMiddle, Color128.White, true);
			}
			else if (Game.CurrentInterface == Game.InterfaceType.Menu)
				Game.Menu.Draw();
			//Fade to black on change ends
			if (TrainManager.PlayerTrain.Station >= 0 && Game.Stations[TrainManager.PlayerTrain.Station].StationType == Game.StationType.ChangeEnds && TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Boarding)
			{
				double time = TrainManager.PlayerTrain.StationDepartureTime - Game.SecondsSinceMidnight;
				if (time < 1.0)
				{
					FadeToBlackDueToChangeEnds = Math.Max(0.0, 1.0 - time);
				}
				else if (FadeToBlackDueToChangeEnds > 0.0)
				{
					FadeToBlackDueToChangeEnds -= TimeElapsed;
					if (FadeToBlackDueToChangeEnds < 0.0)
					{
						FadeToBlackDueToChangeEnds = 0.0;
					}
				}
			}
			else if (FadeToBlackDueToChangeEnds > 0.0)
			{
				FadeToBlackDueToChangeEnds -= TimeElapsed;
				if (FadeToBlackDueToChangeEnds < 0.0)
				{
					FadeToBlackDueToChangeEnds = 0.0;
				}
			}
			if (FadeToBlackDueToChangeEnds > 0.0 & (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead))
			{
				GL.Color4(0.0, 0.0, 0.0, FadeToBlackDueToChangeEnds);
				RenderOverlaySolid(0.0, 0.0, (double)Screen.Width, (double)Screen.Height);
			}
			// finalize
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.Disable(EnableCap.Blend);
		}

		/// <summary>Renders the list of game (textual) messages</summary>
		/// <param name="Element">The HUD element these are to be rendered onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private static void RenderGameMessages(Interface.HudElement Element, double TimeElapsed)
		{
			//Calculate the size of the viewing plane
			int n = MessageManager.TextualMessages.Count;
			//Minimum initial width is 16px
			double totalwidth = 16.0f;
			for (int j = 0; j < n; j++)
			{
				//Update font size for the renderer
				System.Drawing.Size size = Renderer.MeasureString(Element.Font, (string)MessageManager.TextualMessages[j].MessageToDisplay);
				MessageManager.TextualMessages[j].Width = size.Width;
				MessageManager.TextualMessages[j].Height = size.Height;
				//Run through the list of current messages
				double a = MessageManager.TextualMessages[j].Width - j * (double)Element.Value1;
				//If our width is wider than the old, use this as the NEW viewing plane width
				if (a > totalwidth) totalwidth = a;
			}
			//Calculate the X-width of the viewing plane
			Game.MessagesRendererSize.X += 16.0 * TimeElapsed * ((double)totalwidth - Game.MessagesRendererSize.X);
			totalwidth = (float)Game.MessagesRendererSize.X;
			double lcrh, lw, rw;
			//Calculate final viewing plane size to pass to openGL
			CalculateViewingPlaneSize(Element, out lw, out rw, out lcrh);

			// start
			double w = totalwidth + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? Screen.Width - w : 0.5 * (Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? Screen.Height - h : 0.5 * (Screen.Height - h);
			x += Element.Position.X;
			y += Element.Position.Y;
			int m = 0;
			for (int j = 0; j < n; j++)
			{
				var mm = MessageManager.TextualMessages[j];
				float br, bg, bb, ba;
				CreateBackColor(Element.BackgroundColor, mm.Color, out br, out bg, out bb, out ba);
				float tr, tg, tb, ta;
				CreateTextColor(Element.TextColor, mm.Color, out tr, out tg, out tb, out ta);
				float or, og, ob, oa;
				CreateBackColor(Element.OverlayColor, mm.Color, out or, out og, out ob, out oa);
				double tx, ty;
				bool preserve = false;
				if ((Element.Transition & Interface.HudTransition.Move) != 0)
				{
					if (Game.SecondsSinceMidnight < mm.Timeout)
					{
						if (mm.RendererAlpha == 0.0)
						{
							mm.RendererPosition.X = x + Element.TransitionVector.X;
							mm.RendererPosition.Y = y + Element.TransitionVector.Y;
							mm.RendererAlpha = 1.0;
						}
						tx = x;
						ty = y + m * Element.Value2;
						preserve = true;
					}
					else if (Element.Transition == Interface.HudTransition.MoveAndFade)
					{
						tx = x;
						ty = y + m * Element.Value2;
					}
					else
					{
						tx = x + Element.TransitionVector.X;
						ty = y + (j + 1) * Element.TransitionVector.Y;
					}
					const double speed = 2.0;
					double dx = (speed * Math.Abs(tx - mm.RendererPosition.X) + 0.1) * TimeElapsed;
					double dy = (speed * Math.Abs(ty - mm.RendererPosition.Y) + 0.1) * TimeElapsed;
					if (Math.Abs(tx - mm.RendererPosition.X) < dx)
					{
						mm.RendererPosition.X = tx;
					}
					else
					{
						mm.RendererPosition.X += Math.Sign(tx - mm.RendererPosition.X) * dx;
					}
					if (Math.Abs(ty - mm.RendererPosition.Y) < dy)
					{
						mm.RendererPosition.Y = ty;
					}
					else
					{
						mm.RendererPosition.Y += Math.Sign(ty - mm.RendererPosition.Y) * dy;
					}
				}
				else
				{
					tx = x;
					ty = y + m * Element.Value2;
					mm.RendererPosition.X = 0.0;
					const double speed = 12.0;
					double dy = (speed * Math.Abs(ty - mm.RendererPosition.Y) + 0.1) * TimeElapsed;
					mm.RendererPosition.X = x;
					if (Math.Abs(ty - mm.RendererPosition.Y) < dy)
					{
						mm.RendererPosition.Y = ty;
					}
					else
					{
						mm.RendererPosition.Y += Math.Sign(ty - mm.RendererPosition.Y) * dy;
					}
				}
				if ((Element.Transition & Interface.HudTransition.Fade) != 0)
				{
					if (Game.SecondsSinceMidnight >= mm.Timeout)
					{
						mm.RendererAlpha -= TimeElapsed;
						if (mm.RendererAlpha < 0.0) mm.RendererAlpha = 0.0;
					}
					else
					{
						mm.RendererAlpha += TimeElapsed;
						if (mm.RendererAlpha > 1.0) mm.RendererAlpha = 1.0;
						preserve = true;
					}
				}
				else if (Game.SecondsSinceMidnight > mm.Timeout)
				{
					if (Math.Abs(mm.RendererPosition.X - tx) < 0.1 & Math.Abs(mm.RendererPosition.Y - ty) < 0.1)
					{
						mm.RendererAlpha = 0.0;
					}
				}
				if (preserve) m++;
				double px = mm.RendererPosition.X + (double)j * (double)Element.Value1;
				double py = mm.RendererPosition.Y;
				float alpha = (float)(mm.RendererAlpha * mm.RendererAlpha);
				// graphics
				Interface.HudImage Left = j == 0
					? Element.TopLeft
					: j < n - 1
						? Element.CenterLeft
						: Element.BottomLeft;
				Interface.HudImage Middle = j == 0
					? Element.TopMiddle
					: j < n - 1
						? Element.CenterMiddle
						: Element.BottomMiddle;
				Interface.HudImage Right = j == 0
					? Element.TopRight
					: j < n - 1
						? Element.CenterRight
						: Element.BottomRight;
				// left background
				if (Left.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Left.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.BackgroundTexture.Width;
						double v = (double)Left.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Left.BackgroundTexture, px, py, px + u, py + v);
					}
				}
				// right background
				if (Right.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Right.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.BackgroundTexture.Width;
						double v = (double)Right.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Right.BackgroundTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle background
				if (Middle.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Middle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Middle.BackgroundTexture, px + lw, py, px + w - rw, py + v);
					}
				}
				{
					// text
					string t = (string)mm.MessageToDisplay;
					double u = mm.Width;
					double v = mm.Height;
					double p =
						Math.Round(
							(Element.TextAlignment.X < 0
								? px
								: Element.TextAlignment.X > 0
									? px + w - u
									: px + 0.5 * (w - u)) - j * Element.Value1);
					double q = Math.Round(Element.TextAlignment.Y < 0
						? py
						: Element.TextAlignment.Y > 0
							? py + lcrh - v
							: py + 0.5 * (lcrh - v));
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					DrawString(Element.Font, t, new System.Drawing.Point((int)p, (int)q),
						TextAlignment.TopLeft, new Color128(tr, tg, tb, ta * alpha), Element.TextShadow);
				}
				// left overlay
				if (Left.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Left.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.OverlayTexture.Width;
						double v = (double)Left.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Left.OverlayTexture, px, py, px + u, py + v);
					}
				}
				// right overlay
				if (Right.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Right.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.OverlayTexture.Width;
						double v = (double)Right.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Right.OverlayTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle overlay
				if (Middle.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Middle.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Middle.OverlayTexture, px + lw, py, px + w - rw, py + v);
					}
				}
			}
		}

		/// <summary>Renders the list of score messages</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private static void RenderScoreMessages(Interface.HudElement Element, double TimeElapsed)
		{
			// score messages
			int n = Game.ScoreMessages.Length;
			float totalwidth = 16.0f;
			float[] widths = new float[n];
			float[] heights = new float[n];
			for (int j = 0; j < n; j++)
			{
				System.Drawing.Size size = MeasureString(Element.Font, Game.ScoreMessages[j].Text);
				widths[j] = size.Width;
				heights[j] = size.Height;
				float a = widths[j] - j * Element.Value1;
				if (a > totalwidth) totalwidth = a;
			}
			Game.ScoreMessagesRendererSize.X += 16.0 * TimeElapsed * ((double)totalwidth - Game.ScoreMessagesRendererSize.X);
			totalwidth = (float)Game.ScoreMessagesRendererSize.X;
			double lcrh, lw, rw;
			CalculateViewingPlaneSize(Element, out lw, out rw, out lcrh);
			// start
			double w = Element.Alignment.X == 0 ? lw + rw + 128 : totalwidth + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? Screen.Width - w : 0.5 * (Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? Screen.Height - h : 0.5 * (Screen.Height - h);
			x += Element.Position.X;
			y += Element.Position.Y;
			int m = 0;
			for (int j = 0; j < n; j++)
			{
				float br, bg, bb, ba;
				CreateBackColor(Element.BackgroundColor, Game.ScoreMessages[j].Color, out br, out bg, out bb, out ba);
				float tr, tg, tb, ta;
				CreateTextColor(Element.TextColor, Game.ScoreMessages[j].Color, out tr, out tg, out tb, out ta);
				float or, og, ob, oa;
				CreateBackColor(Element.OverlayColor, Game.ScoreMessages[j].Color, out or, out og, out ob, out oa);
				double tx, ty;
				bool preserve = false;
				if ((Element.Transition & Interface.HudTransition.Move) != 0)
				{
					if (Game.SecondsSinceMidnight < Game.ScoreMessages[j].Timeout)
					{
						if (Game.ScoreMessages[j].RendererAlpha == 0.0)
						{
							Game.ScoreMessages[j].RendererPosition.X = x + Element.TransitionVector.X;
							Game.ScoreMessages[j].RendererPosition.Y = y + Element.TransitionVector.Y;
							Game.ScoreMessages[j].RendererAlpha = 1.0;
						}
						tx = x;
						ty = y + m * Element.Value2;
						preserve = true;
					}
					else if (Element.Transition == Interface.HudTransition.MoveAndFade)
					{
						tx = x;
						ty = y + m * Element.Value2;
					}
					else
					{
						tx = x + Element.TransitionVector.X;
						ty = y + (j + 1) * Element.TransitionVector.Y;
					}
					const double speed = 2.0;
					double dx = (speed * Math.Abs(tx - Game.ScoreMessages[j].RendererPosition.X) + 0.1) * TimeElapsed;
					double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
					if (Math.Abs(tx - Game.ScoreMessages[j].RendererPosition.X) < dx)
					{
						Game.ScoreMessages[j].RendererPosition.X = tx;
					}
					else
					{
						Game.ScoreMessages[j].RendererPosition.X += Math.Sign(tx - Game.ScoreMessages[j].RendererPosition.X) * dx;
					}
					if (Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) < dy)
					{
						Game.ScoreMessages[j].RendererPosition.Y = ty;
					}
					else
					{
						Game.ScoreMessages[j].RendererPosition.Y += Math.Sign(ty - Game.ScoreMessages[j].RendererPosition.Y) * dy;
					}
				}
				else
				{
					tx = x;
					ty = y + m * Element.Value2;
					Game.ScoreMessages[j].RendererPosition.X = 0.0;
					const double speed = 12.0;
					double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
					Game.ScoreMessages[j].RendererPosition.X = x;
					if (Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) < dy)
					{
						Game.ScoreMessages[j].RendererPosition.Y = ty;
					}
					else
					{
						Game.ScoreMessages[j].RendererPosition.Y += Math.Sign(ty - Game.ScoreMessages[j].RendererPosition.Y) * dy;
					}
				}
				if ((Element.Transition & Interface.HudTransition.Fade) != 0)
				{
					if (Game.SecondsSinceMidnight >= Game.ScoreMessages[j].Timeout)
					{
						Game.ScoreMessages[j].RendererAlpha -= TimeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha < 0.0) Game.ScoreMessages[j].RendererAlpha = 0.0;
					}
					else
					{
						Game.ScoreMessages[j].RendererAlpha += TimeElapsed;
						if (Game.ScoreMessages[j].RendererAlpha > 1.0) Game.ScoreMessages[j].RendererAlpha = 1.0;
						preserve = true;
					}
				}
				else if (Game.SecondsSinceMidnight > Game.ScoreMessages[j].Timeout)
				{
					if (Math.Abs(Game.ScoreMessages[j].RendererPosition.X - tx) < 0.1 & Math.Abs(Game.ScoreMessages[j].RendererPosition.Y - ty) < 0.1)
					{
						Game.ScoreMessages[j].RendererAlpha = 0.0;
					}
				}
				if (preserve) m++;
				double px = Game.ScoreMessages[j].RendererPosition.X + (double)j * (double)Element.Value1;
				double py = Game.ScoreMessages[j].RendererPosition.Y;
				float alpha = (float)(Game.ScoreMessages[j].RendererAlpha * Game.ScoreMessages[j].RendererAlpha);
				// graphics
				Interface.HudImage Left = j == 0 ? Element.TopLeft : j < n - 1 ? Element.CenterLeft : Element.BottomLeft;
				Interface.HudImage Middle = j == 0 ? Element.TopMiddle : j < n - 1 ? Element.CenterMiddle : Element.BottomMiddle;
				Interface.HudImage Right = j == 0 ? Element.TopRight : j < n - 1 ? Element.CenterRight : Element.BottomRight;
				// left background
				if (Left.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Left.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.BackgroundTexture.Width;
						double v = (double)Left.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Left.BackgroundTexture, px, py, px + u, py + v);
					}
				}
				// right background
				if (Right.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Right.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.BackgroundTexture.Width;
						double v = (double)Right.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Right.BackgroundTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle background
				if (Middle.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Middle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.BackgroundTexture.Height;
						GL.Color4(br, bg, bb, ba * alpha);
						RenderOverlayTexture(Middle.BackgroundTexture, px + lw, py, px + w - rw, py + v);
					}
				}
				{ // text
					string t = Game.ScoreMessages[j].Text;
					double u = widths[j];
					double v = heights[j];
					double p = Math.Round((Element.TextAlignment.X < 0 ? px : Element.TextAlignment.X > 0 ? px + w - u : px + 0.5 * (w - u)) - j * Element.Value1);
					double q = Math.Round(Element.TextAlignment.Y < 0 ? py : Element.TextAlignment.Y > 0 ? py + lcrh - v : py + 0.5 * (lcrh - v));
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					DrawString(Element.Font, t, new System.Drawing.Point((int)p, (int)q), TextAlignment.TopLeft, new Color128(tr, tg, tb, ta * alpha), Element.TextShadow);
				}
				// left overlay
				if (Left.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Left.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Left.OverlayTexture.Width;
						double v = (double)Left.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Left.OverlayTexture, px, py, px + u, py + v);
					}
				}
				// right overlay
				if (Right.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Right.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double u = (double)Right.OverlayTexture.Width;
						double v = (double)Right.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Right.OverlayTexture, px + w - u, py, px + w, py + v);
					}
				}
				// middle overlay
				if (Middle.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Middle.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						double v = (double)Middle.OverlayTexture.Height;
						GL.Color4(or, og, ob, oa * alpha);
						RenderOverlayTexture(Middle.OverlayTexture, px + lw, py, px + w - rw, py + v);
					}
				}
			}
		}

		/// <summary>Renders the ATS lamp overlay</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private static void RenderATSLamps(Interface.HudElement Element, double TimeElapsed)
		{
			// ats lamps
			if (CurrentLampCollection.Lamps == null)
			{
				InitializeLamps();
			}
			double lcrh, lw, rw;
			CalculateViewingPlaneSize(Element, out lw, out rw, out lcrh);
			// start
			int n = CurrentLampCollection.Lamps.Length;
			double w = (double)CurrentLampCollection.Width + lw + rw;
			double h = Element.Value2 * n;
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X > 0 ? Screen.Width - w : 0.5 * (Screen.Width - w);
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y > 0 ? Screen.Height - h : 0.5 * (Screen.Height - h);
			x += Element.Position.X;
			y += Element.Position.Y;
			for (int j = 0; j < n; j++)
			{
				if (CurrentLampCollection.Lamps[j].Type != LampType.None)
				{
					int o;
					if (j == 0)
					{
						o = -1;
					}
					else if (CurrentLampCollection.Lamps[j - 1].Type == LampType.None)
					{
						o = -1;
					}
					else if (j < n - 1 && CurrentLampCollection.Lamps[j + 1].Type == LampType.None)
					{
						o = 1;
					}
					else if (j == n - 1)
					{
						o = 1;
					}
					else
					{
						o = 0;
					}
					Interface.HudImage Left = o < 0 ? Element.TopLeft : o == 0 ? Element.CenterLeft : Element.BottomLeft;
					Interface.HudImage Middle = o < 0 ? Element.TopMiddle : o == 0 ? Element.CenterMiddle : Element.BottomMiddle;
					Interface.HudImage Right = o < 0 ? Element.TopRight : o == 0 ? Element.CenterRight : Element.BottomRight;
					MessageColor sc = MessageColor.Gray;
					if (TrainManager.PlayerTrain.Plugin.Panel.Length >= 272)
					{
						switch (CurrentLampCollection.Lamps[j].Type)
						{
							case LampType.Ats:
								if (TrainManager.PlayerTrain.Plugin.Panel[256] != 0)
								{
									sc = MessageColor.Orange;
								} break;
							case LampType.AtsOperation:
								if (TrainManager.PlayerTrain.Plugin.Panel[258] != 0)
								{
									sc = MessageColor.Red;
								} break;
							case LampType.AtsPPower:
								if (TrainManager.PlayerTrain.Plugin.Panel[259] != 0)
								{
									sc = MessageColor.Green;
								} break;
							case LampType.AtsPPattern:
								if (TrainManager.PlayerTrain.Plugin.Panel[260] != 0)
								{
									sc = MessageColor.Orange;
								} break;
							case LampType.AtsPBrakeOverride:
								if (TrainManager.PlayerTrain.Plugin.Panel[261] != 0)
								{
									sc = MessageColor.Orange;
								} break;
							case LampType.AtsPBrakeOperation:
								if (TrainManager.PlayerTrain.Plugin.Panel[262] != 0)
								{
									sc = MessageColor.Orange;
								} break;
							case LampType.AtsP:
								if (TrainManager.PlayerTrain.Plugin.Panel[263] != 0)
								{
									sc = MessageColor.Green;
								} break;
							case LampType.AtsPFailure:
								if (TrainManager.PlayerTrain.Plugin.Panel[264] != 0)
								{
									sc = MessageColor.Red;
								} break;
							case LampType.Atc:
								if (TrainManager.PlayerTrain.Plugin.Panel[265] != 0)
								{
									sc = MessageColor.Orange;
								} break;
							case LampType.AtcPower:
								if (TrainManager.PlayerTrain.Plugin.Panel[266] != 0)
								{
									sc = MessageColor.Orange;
								} break;
							case LampType.AtcUse:
								if (TrainManager.PlayerTrain.Plugin.Panel[267] != 0)
								{
									sc = MessageColor.Orange;
								} break;
							case LampType.AtcEmergency:
								if (TrainManager.PlayerTrain.Plugin.Panel[268] != 0)
								{
									sc = MessageColor.Red;
								} break;
							case LampType.Eb:
								if (TrainManager.PlayerTrain.Plugin.Panel[270] != 0)
								{
									sc = MessageColor.Green;
								} break;
							case LampType.ConstSpeed:
								if (TrainManager.PlayerTrain.Plugin.Panel[269] != 0)
								{
									sc = MessageColor.Orange;
								} break;
						}
					}
					// colors
					float br, bg, bb, ba;
					CreateBackColor(Element.BackgroundColor, sc, out br, out bg, out bb, out ba);
					float tr, tg, tb, ta;
					CreateTextColor(Element.TextColor, sc, out tr, out tg, out tb, out ta);
					float or, og, ob, oa;
					CreateBackColor(Element.OverlayColor, sc, out or, out og, out ob, out oa);
					// left background
					if (Left.BackgroundTexture != null)
					{
						if (Textures.LoadTexture(Left.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
						{
							double u = (double)Left.BackgroundTexture.Width;
							double v = (double)Left.BackgroundTexture.Height;
							GL.Color4(br, bg, bb, ba);
							RenderOverlayTexture(Left.BackgroundTexture, x, y, x + u, y + v);
						}
					}
					// right background
					if (Right.BackgroundTexture != null)
					{
						if (Textures.LoadTexture(Right.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
						{
							double u = (double)Right.BackgroundTexture.Width;
							double v = (double)Right.BackgroundTexture.Height;
							GL.Color4(br, bg, bb, ba);
							RenderOverlayTexture(Right.BackgroundTexture, x + w - u, y, x + w, y + v);
						}
					}
					// middle background
					if (Middle.BackgroundTexture != null)
					{
						if (Textures.LoadTexture(Middle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
						{
							double v = (double)Middle.BackgroundTexture.Height;
							GL.Color4(br, bg, bb, ba);
							RenderOverlayTexture(Middle.BackgroundTexture, x + lw, y, x + w - rw, y + v);
						}
					}
					{ // text
						string t = CurrentLampCollection.Lamps[j].Text;
						double u = CurrentLampCollection.Lamps[j].Width;
						double v = CurrentLampCollection.Lamps[j].Height;
						double p = Math.Round(Element.TextAlignment.X < 0 ? x : Element.TextAlignment.X > 0 ? x + w - u : x + 0.5 * (w - u));
						double q = Math.Round(Element.TextAlignment.Y < 0 ? y : Element.TextAlignment.Y > 0 ? y + lcrh - v : y + 0.5 * (lcrh - v));
						p += Element.TextPosition.X;
						q += Element.TextPosition.Y;
						DrawString(Element.Font, t, new System.Drawing.Point((int)p, (int)q), TextAlignment.TopLeft, new Color128(tr, tg, tb, ta), Element.TextShadow);
					}
					// left overlay
					if (Left.OverlayTexture != null)
					{
						if (Textures.LoadTexture(Left.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
						{
							double u = (double)Left.OverlayTexture.Width;
							double v = (double)Left.OverlayTexture.Height;
							GL.Color4(or, og, ob, oa);
							RenderOverlayTexture(Left.OverlayTexture, x, y, x + u, y + v);
						}
					}
					// right overlay
					if (Right.OverlayTexture != null)
					{
						if (Textures.LoadTexture(Right.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
						{
							double u = (double)Right.OverlayTexture.Width;
							double v = (double)Right.OverlayTexture.Height;
							GL.Color4(or, og, ob, oa);
							RenderOverlayTexture(Right.OverlayTexture, x + w - u, y, x + w, y + v);
						}
					}
					// middle overlay
					if (Middle.OverlayTexture != null)
					{
						if (Textures.LoadTexture(Middle.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
						{
							double v = (double)Middle.OverlayTexture.Height;
							GL.Color4(or, og, ob, oa);
							RenderOverlayTexture(Middle.OverlayTexture, x + lw, y, x + w - rw, y + v);
						}
					}
				}
				y += (double)Element.Value2;
			}
		}

		/// <summary>Renders all default HUD elements</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private static void RenderHUDElement(Interface.HudElement Element, double TimeElapsed)
		{
			TrainManager.TrainDoorState LeftDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false);
			TrainManager.TrainDoorState RightDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Command = Element.Subject.ToLowerInvariant();
			// default
			double w, h;
			if (Element.CenterMiddle.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.CenterMiddle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
				{
					w = (double)Element.CenterMiddle.BackgroundTexture.Width;
					h = (double)Element.CenterMiddle.BackgroundTexture.Height;
				}
				else
				{
					w = 0.0; h = 0.0;
				}
			}
			else
			{
				w = 0.0; h = 0.0;
			}
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X == 0 ? 0.5 * (Screen.Width - w) : Screen.Width - w;
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y == 0 ? 0.5 * (Screen.Height - h) : Screen.Height - h;
			x += Element.Position.X;
			y += Element.Position.Y;
			// command
			const double speed = 1.0;
			MessageColor sc = MessageColor.None;
			string t;
			switch (Command)
			{
				case "reverser":
					if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver < 0)
					{
						sc = MessageColor.Orange; t = Interface.QuickReferences.HandleBackward;
					}
					else if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver > 0)
					{
						sc = MessageColor.Blue; t = Interface.QuickReferences.HandleForward;
					}
					else
					{
						sc = MessageColor.Gray; t = Interface.QuickReferences.HandleNeutral;
					}
					Element.TransitionState = 0.0;
					break;
				case "power":
					if (TrainManager.PlayerTrain.Specs.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver == 0)
					{
						sc = MessageColor.Gray; t = Interface.QuickReferences.HandlePowerNull;
					}
					else
					{
						sc = MessageColor.Blue; t = Interface.QuickReferences.HandlePower + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture);
					}
					Element.TransitionState = 0.0;
					break;
				case "brake":
					if (TrainManager.PlayerTrain.Specs.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
					{
						if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver)
						{
							sc = MessageColor.Red; t = Interface.QuickReferences.HandleEmergency;
						}
						else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release)
						{
							sc = MessageColor.Gray; t = Interface.QuickReferences.HandleRelease;
						}
						else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap)
						{
							sc = MessageColor.Blue; t = Interface.QuickReferences.HandleLap;
						}
						else
						{
							sc = MessageColor.Orange; t = Interface.QuickReferences.HandleService;
						}
					}
					else
					{
						if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver)
						{
							sc = MessageColor.Red; t = Interface.QuickReferences.HandleEmergency;
						}
						else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
						{
							sc = MessageColor.Green; t = Interface.QuickReferences.HandleHoldBrake;
						}
						else if (TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver == 0)
						{
							sc = MessageColor.Gray; t = Interface.QuickReferences.HandleBrakeNull;
						}
						else
						{
							sc = MessageColor.Orange; t = Interface.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture);
						}
					}
					Element.TransitionState = 0.0;
					break;
				case "single":
					if (!TrainManager.PlayerTrain.Specs.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver)
					{
						sc = MessageColor.Red; t = Interface.QuickReferences.HandleEmergency;
					}
					else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver)
					{
						sc = MessageColor.Green; t = Interface.QuickReferences.HandleHoldBrake;
					}
					else if (TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver > 0)
					{
						sc = MessageColor.Orange; t = Interface.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture);
					}
					else if (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver > 0)
					{
						sc = MessageColor.Blue; t = Interface.QuickReferences.HandlePower + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture);
					}
					else
					{
						sc = MessageColor.Gray; t = Interface.QuickReferences.HandlePowerNull;
					}
					Element.TransitionState = 0.0;
					break;
				case "doorsleft":
				case "doorsright":
				{
					if ((LeftDoors & TrainManager.TrainDoorState.AllClosed) == 0 | (RightDoors & TrainManager.TrainDoorState.AllClosed) == 0)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
					TrainManager.TrainDoorState Doors = Command == "doorsleft" ? LeftDoors : RightDoors;
					if ((Doors & TrainManager.TrainDoorState.Mixed) != 0)
					{
						sc = MessageColor.Orange;
					}
					else if ((Doors & TrainManager.TrainDoorState.AllClosed) != 0)
					{
						sc = MessageColor.Gray;
					}
					else if (TrainManager.PlayerTrain.Specs.DoorCloseMode == TrainManager.DoorMode.Manual)
					{
						sc = MessageColor.Green;
					}
					else
					{
						sc = MessageColor.Blue;
					}
					t = Command == "doorsleft" ? Interface.QuickReferences.DoorsLeft : Interface.QuickReferences.DoorsRight;
				} break;
				case "stopleft":
				case "stopright":
				case "stopnone":
				{
					int s = TrainManager.PlayerTrain.Station;
					if (s >= 0 && Game.PlayerStopsAtStation(s) && Interface.CurrentOptions.GameMode != Interface.GameMode.Expert)
					{
						bool cond;
						if (Command == "stopleft")
						{
							cond = Game.Stations[s].OpenLeftDoors;
						}
						else if (Command == "stopright")
						{
							cond = Game.Stations[s].OpenRightDoors;
						}
						else
						{
							cond = !Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors;
						}
						if (TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending & cond)
						{
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
						}
						else
						{
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
						}
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
					t = Element.Text;
				} break;
				case "stoplefttick":
				case "stoprighttick":
				case "stopnonetick":
				{
					int s = TrainManager.PlayerTrain.Station;
					if (s >= 0 && Game.PlayerStopsAtStation(s) && Interface.CurrentOptions.GameMode != Interface.GameMode.Expert)
					{
						int c = Game.GetStopIndex(s, TrainManager.PlayerTrain.Cars.Length);
						if (c >= 0)
						{
							bool cond;
							if (Command == "stoplefttick")
							{
								cond = Game.Stations[s].OpenLeftDoors;
							}
							else if (Command == "stoprighttick")
							{
								cond = Game.Stations[s].OpenRightDoors;
							}
							else
							{
								cond = !Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors;
							}
							if (TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending & cond)
							{
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							}
							else
							{
								Element.TransitionState += speed * TimeElapsed;
								if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
							}
							double d = TrainManager.PlayerTrain.StationDistanceToStopPoint;
							double r;
							if (d > 0.0)
							{
								r = d / Game.Stations[s].Stops[c].BackwardTolerance;
							}
							else
							{
								r = d / Game.Stations[s].Stops[c].ForwardTolerance;
							}
							if (r < -1.0) r = -1.0;
							if (r > 1.0) r = 1.0;
							y -= r * (double)Element.Value1;
						}
						else
						{
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
						}
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
					t = Element.Text;
				} break;
				case "clock":
				{
					int hours = (int)Math.Floor(Game.SecondsSinceMidnight);
					int seconds = hours % 60; hours /= 60;
					int minutes = hours % 60; hours /= 60;
					hours %= 24;
					t = hours.ToString(Culture).PadLeft(2, '0') + ":" + minutes.ToString(Culture).PadLeft(2, '0') + ":" + seconds.ToString(Culture).PadLeft(2, '0');
					if (OptionClock)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
				} break;
				case "gradient":
					if (OptionGradient == GradientDisplayMode.Percentage)
					{
						if (World.CameraTrackFollower.Pitch != 0)
						{
							double pc = World.CameraTrackFollower.Pitch;
							t = Math.Abs(pc).ToString("0.00", Culture) + "%" + (Math.Abs(pc) == pc ? " ↗" : " ↘");
						}
						else
						{
							t = "Level";
						}
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else if (OptionGradient == GradientDisplayMode.UnitOfChange)
					{
						if (World.CameraTrackFollower.Pitch != 0)
						{
							double gr = 1000 / World.CameraTrackFollower.Pitch;
							t = "1 in " + Math.Abs(gr).ToString("0", Culture) + (Math.Abs(gr) == gr ? " ↗" : " ↘");
						}
						else
						{
							t = "Level";
						}
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						if (World.CameraTrackFollower.Pitch != 0)
						{
							double gr = 1000 / World.CameraTrackFollower.Pitch;
							t = "1 in " + Math.Abs(gr).ToString("0", Culture) + (Math.Abs(gr) == gr ? " ↗" : " ↘");
						}
						else
						{
							t = "Level";
						}
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "speed":
					if (OptionSpeed == SpeedDisplayMode.Kmph)
					{
						double kmph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 3.6;
						t = kmph.ToString("0.00", Culture) + " km/h";
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else if (OptionSpeed == SpeedDisplayMode.Mph)
					{
						double mph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 2.2369362920544;
						t = mph.ToString("0.00", Culture) + " mph";
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						double mph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 2.2369362920544;
						t = mph.ToString("0.00", Culture) + " mph";
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "fps":
					int fps = (int)Math.Round(Game.InfoFrameRate);
					t = fps.ToString(Culture) + " fps";
					if (OptionFrameRates)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "ai":
					t = "A.I.";
					if (TrainManager.PlayerTrain.AI != null)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "score":
					if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade)
					{
						t = Game.CurrentScore.Value.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture);
						if (Game.CurrentScore.Value < 0)
						{
							sc = MessageColor.Red;
						}
						else if (Game.CurrentScore.Value > 0)
						{
							sc = MessageColor.Green;
						}
						else
						{
							sc = MessageColor.Gray;
						}
						Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState = 1.0;
						t = "";
					} break;
				default:
					t = Element.Text;
					break;
			}
			// transitions
			float alpha = 1.0f;
			if ((Element.Transition & Interface.HudTransition.Move) != 0)
			{
				double s = Element.TransitionState;
				x += Element.TransitionVector.X * s * s;
				y += Element.TransitionVector.Y * s * s;
			}
			if ((Element.Transition & Interface.HudTransition.Fade) != 0)
			{
				alpha = (float)(1.0 - Element.TransitionState);
			}
			else if (Element.Transition == Interface.HudTransition.None)
			{
				alpha = (float)(1.0 - Element.TransitionState);
			}
			// render
			if (alpha != 0.0f)
			{
				// background
				if (Element.CenterMiddle.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Element.CenterMiddle.BackgroundTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						float r, g, b, a;
						CreateBackColor(Element.BackgroundColor, sc, out r, out g, out b, out a);
						GL.Color4(r, g, b, a * alpha);
						RenderOverlayTexture(Element.CenterMiddle.BackgroundTexture, x, y, x + w, y + h);
					}
				}
				{ // text
					System.Drawing.Size size = MeasureString(Element.Font, t);
					float u = size.Width;
					float v = size.Height;
					double p = Math.Round(Element.TextAlignment.X < 0 ? x : Element.TextAlignment.X == 0 ? x + 0.5 * (w - u) : x + w - u);
					double q = Math.Round(Element.TextAlignment.Y < 0 ? y : Element.TextAlignment.Y == 0 ? y + 0.5 * (h - v) : y + h - v);
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					float r, g, b, a;
					CreateTextColor(Element.TextColor, sc, out r, out g, out b, out a);
					DrawString(Element.Font, t, new System.Drawing.Point((int)p, (int)q), TextAlignment.TopLeft, new Color128(r, g, b, a * alpha), Element.TextShadow);
				}
				// overlay
				if (Element.CenterMiddle.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Element.CenterMiddle.OverlayTexture, Textures.OpenGlTextureWrapMode.ClampClamp))
					{
						float r, g, b, a;
						CreateBackColor(Element.OverlayColor, sc, out r, out g, out b, out a);
						GL.Color4(r, g, b, a * alpha);
						RenderOverlayTexture(Element.CenterMiddle.OverlayTexture, x, y, x + w, y + h);
					}
				}
			}
		}

		/// <summary>Renders the debug (F10) overlay</summary>
		private static void RenderDebugOverlays()
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// debug
			GL.Color4(0.5, 0.5, 0.5, 0.5);
			RenderOverlaySolid(0.0f, 0.0f, (double)Screen.Width, (double)Screen.Height);
			// actual handles
			{
				string t = "actual: " + (TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == -1 ? "B" : TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == 1 ? "F" : "N");
				if (TrainManager.PlayerTrain.Specs.SingleHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual.ToString(Culture) : "N");
				}
				else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Actual == TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Actual == TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : "N");
				}
				DrawString(Fonts.SmallFont, t, new System.Drawing.Point(2, Screen.Height - 46), TextAlignment.TopLeft, Color128.White, true);
			}
			// safety handles
			{
				string t = "safety: " + (TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == -1 ? "B" : TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == 1 ? "F" : "N");
				if (TrainManager.PlayerTrain.Specs.SingleHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety.ToString(Culture) : "N");
				}
				else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Safety == TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Safety == TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : "N");
				}
				DrawString(Fonts.SmallFont, t, new System.Drawing.Point(2, Screen.Height - 32), TextAlignment.TopLeft, Color128.White, true);
			}
			// driver handles
			{
				string t = "driver: " + (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver == -1 ? "B" : TrainManager.PlayerTrain.Specs.CurrentReverser.Driver == 1 ? "F" : "N");
				if (TrainManager.PlayerTrain.Specs.SingleHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver ? "HLD" : TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture) : "N");
				}
				else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver ? "HLD" : "N");
				}
				DrawString(Fonts.SmallFont, t, new System.Drawing.Point(2, Screen.Height - 18), TextAlignment.TopLeft, Color128.White, true);
			}
			// debug information
			int texturesLoaded = Textures.GetNumberOfLoadedTextures();
			int texturesRegistered = Textures.GetNumberOfRegisteredTextures();
			int soundBuffersRegistered = Sounds.GetNumberOfLoadedBuffers();
			int soundBuffersLoaded = Sounds.GetNumberOfLoadedBuffers();
			int soundSourcesRegistered = Sounds.GetNumberOfRegisteredSources();
			int soundSourcesPlaying = Sounds.GetNumberOfPlayingSources();
			int car = 0;
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				if (TrainManager.PlayerTrain.Cars[i].Specs.IsMotorCar)
				{
					car = i;
					break;
				}
			}
			double mass = 0.0;
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				mass += TrainManager.PlayerTrain.Cars[i].Specs.MassCurrent;
			}
			string[] Lines = new string[] {
				"=system",
				"fps: " + Game.InfoFrameRate.ToString("0.0", Culture) + (MainLoop.LimitFramerate ? " (low cpu)" : ""),
				"score: " + Game.CurrentScore.Value.ToString(Culture),
				"",
				"=train",
				"speed: " + (Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 3.6).ToString("0.00", Culture) + " km/h",
				"power (car " + car.ToString(Culture) +  "): " + (TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput < 0.0 ? TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * (double)Math.Sign(TrainManager.PlayerTrain.Cars[car].Specs.CurrentSpeed) : TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * (double)TrainManager.PlayerTrain.Specs.CurrentReverser.Actual).ToString("0.0000", Culture) + " m/s²",
				"acceleration: " + TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration.ToString("0.0000", Culture) + " m/s²",
				"position: " + (TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - TrainManager.PlayerTrain.Cars[0].FrontAxlePosition + 0.5 * TrainManager.PlayerTrain.Cars[0].Length).ToString("0.00", Culture) + " m",
				"elevation: " + (Game.RouteInitialElevation + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.WorldPosition.Y).ToString("0.00", Culture) + " m",
				"temperature: " + (TrainManager.PlayerTrain.Specs.CurrentAirTemperature - 273.15).ToString("0.00", Culture) + " °C",
				"air pressure: " + (0.001 * TrainManager.PlayerTrain.Specs.CurrentAirPressure).ToString("0.00", Culture) + " kPa",
				"air density: " + TrainManager.PlayerTrain.Specs.CurrentAirDensity.ToString("0.0000", Culture) + " kg/m³",
				"speed of sound: " + (Game.GetSpeedOfSound(TrainManager.PlayerTrain.Specs.CurrentAirDensity) * 3.6).ToString("0.00", Culture) + " km/h",
				"passenger ratio: " + TrainManager.PlayerTrain.Passengers.PassengerRatio.ToString("0.00"),
				"total mass: " + mass.ToString("0.00", Culture) + " kg",
				"",
				"=route",
				"track limit: " + (TrainManager.PlayerTrain.CurrentRouteLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentRouteLimit * 3.6).ToString("0.0", Culture) + " km/h")),
				"signal limit: " + (TrainManager.PlayerTrain.CurrentSectionLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentSectionLimit * 3.6).ToString("0.0", Culture) + " km/h")),
				"total static objects: " + ObjectManager.ObjectsUsed.ToString(Culture),
				"total static GL_TRIANGLES: " + Game.InfoTotalTriangles.ToString(Culture),
				"total static GL_TRIANGLE_STRIP: " + Game.InfoTotalTriangleStrip.ToString(Culture),
				"total static GL_QUADS: " + Game.InfoTotalQuads.ToString(Culture),
				"total static GL_QUAD_STRIP: " + Game.InfoTotalQuadStrip.ToString(Culture),
				"total static GL_POLYGON: " + Game.InfoTotalPolygon.ToString(Culture),
				"total animated objects: " + ObjectManager.AnimatedWorldObjectsUsed.ToString(Culture),
				"",
				"=renderer",
				"static opaque faces: " + Game.InfoStaticOpaqueFaceCount.ToString(Culture),
				"dynamic opaque faces: " + DynamicOpaque.FaceCount.ToString(Culture),
				"dynamic alpha faces: " + DynamicAlpha.FaceCount.ToString(Culture),
				"overlay opaque faces: " + OverlayOpaque.FaceCount.ToString(Culture),
				"overlay alpha faces: " + OverlayAlpha.FaceCount.ToString(Culture),
				"textures loaded: " + texturesLoaded.ToString(Culture) + " / " + texturesRegistered.ToString(Culture),
				"",
				"=camera",
				"position: " + World.CameraTrackFollower.TrackPosition.ToString("0.00", Culture) + " m",
				"curve radius: " + World.CameraTrackFollower.CurveRadius.ToString("0.00", Culture) + " m",
				"curve cant: " + (1000.0 * Math.Abs(World.CameraTrackFollower.CurveCant)).ToString("0.00", Culture) + " mm" + (World.CameraTrackFollower.CurveCant < 0.0 ? " (left)" : World.CameraTrackFollower.CurveCant > 0.0 ? " (right)" : ""),
				"",
				"=sound",
				"sound buffers: " + soundBuffersLoaded.ToString(Culture) + " / " + soundBuffersRegistered.ToString(Culture),
				"sound sources: " + soundSourcesPlaying.ToString(Culture) + " / " + soundSourcesRegistered.ToString(Culture),
				(Interface.CurrentOptions.SoundModel == Sounds.SoundModels.Inverse ? "log clamp factor: " + Sounds.LogClampFactor.ToString("0.00") : "outer radius factor: " + Sounds.OuterRadiusFactor.ToString("0.00", Culture)),
				"",
				"=debug",
				"train plugin status: " + (TrainManager.PlayerTrain.Plugin != null ? (TrainManager.PlayerTrain.Plugin.PluginValid ? "ok" : "error") : "n/a"),
				"train plugin message: " + (TrainManager.PlayerTrain.Plugin != null ? (TrainManager.PlayerTrain.Plugin.PluginMessage ?? "n/a") : "n/a"),
				Game.InfoDebugString ?? ""
			};
			double x = 4.0;
			double y = 4.0;
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length != 0)
				{
					if (Lines[i][0] == '=')
					{
						string text = Lines[i].Substring(1);
						System.Drawing.Size size = MeasureString(Fonts.SmallFont, text);
						GL.Color4(0.35f, 0.65f, 0.90f, 0.8f);
						RenderOverlaySolid(x, y, x + size.Width + 6.0f, y + size.Height + 2.0f);
						DrawString(Fonts.SmallFont, text, new System.Drawing.Point((int)x + 3, (int)y), TextAlignment.TopLeft, Color128.White);
					}
					else
					{
						DrawString(Fonts.SmallFont, Lines[i], new System.Drawing.Point((int)x, (int)y), TextAlignment.TopLeft, Color128.White, true);
					}
					y += 14.0;
				}
				else if (y >= (double)Screen.Height - 240.0)
				{
					x += 280.0;
					y = 4.0;
				}
				else
				{
					y += 14.0;
				}
			}
		}

		/// <summary>Renders the brake system debug overlay</summary>
		private static void RenderBrakeSystemDebug()
		{
			double oy = 64.0, y = oy, h = 16.0;
			bool[] heading = new bool[6];
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				double x = 96.0, w = 128.0;
				// brake pipe
				if (TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake)
				{
					if (!heading[0])
					{
						DrawString(Fonts.SmallFont, "Brake pipe", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[0] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakePipeNormalPressure;
					GL.Color3(1.0f, 1.0f, 0.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// auxillary reservoir
				if (TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake)
				{
					if (!heading[1])
					{
						//RenderString(x, oy - 16.0, Fonts.FontType.Small, "Auxillary reservoir", -1, 0.75f, 0.75f, 0.75f, true);
						DrawString(Fonts.SmallFont, "Auxillary reservoir", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[1] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
					GL.Color3(0.5f, 0.5f, 0.5f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// brake cylinder
				{
					if (!heading[2])
					{
						//RenderString(x, oy - 16.0, Fonts.FontType.Small, "Brake cylinder", -1, 0.75f, 0.5f, 0.25f, true);
						DrawString(Fonts.SmallFont, "Brake cylinder", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[2] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					GL.Color3(0.75f, 0.5f, 0.25f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// main reservoir
				if (TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main)
				{
					if (!heading[3])
					{
						//RenderString(x, oy - 16.0, Fonts.FontType.Small, "Main reservoir", -1, 1.0f, 0.0f, 0.0f, true);
						DrawString(Fonts.SmallFont, "Main reservoir", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[3] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.MainReservoirCurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.AirCompressorMaximumPressure;
					GL.Color3(1.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// equalizing reservoir
				if (TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main)
				{
					if (!heading[4])
					{
						//RenderString(x, oy - 16.0, Fonts.FontType.Small, "Equalizing reservoir", -1, 0.0f, 0.75f, 0.0f, true);
						DrawString(Fonts.SmallFont, "Equalizing reservoir", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[4] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.EqualizingReservoirNormalPressure;
					GL.Color3(0.0f, 0.75f, 0.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// straight air pipe
				if (TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake & TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main)
				{
					if (!heading[5])
					{
						//RenderString(x, oy - 16.0, Fonts.FontType.Small, "Straight air pipe", -1, 0.0f, 0.75f, 1.0f, true);
						DrawString(Fonts.SmallFont, "Straight air pipe", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[5] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					GL.Color3(0.0f, 0.75f, 1.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} //x += w + 8.0;
				GL.Color3(0.0f, 0.0f, 0.0f);
				y += h + 8.0;
			}
		}

		
	}
}
