//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Primitives;
using LibRender2.Screens;
using LibRender2.Textures;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using RouteManager2;
using TrainManager;
using MouseCursor = OpenTK.MouseCursor;
using Vector2 = OpenBveApi.Math.Vector2;

namespace OpenBve
{
	internal class SwitchChangeDialog
	{
		/// <summary>The picturebox containing the map texture</summary>
		internal Picturebox MapPicturebox = new Picturebox(Program.Renderer);
		/// <summary>The close button</summary>
		internal Button CloseButton = new Button(Program.Renderer, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"panel", "close"}));
		/// <summary>The zoom in button</summary>
		internal Button ZoomInButton = new Button(Program.Renderer, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "switchmenu", "zoom_in" }));
		/// <summary>The zoom in button</summary>
		internal Button ZoomOutButton = new Button(Program.Renderer, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "switchmenu", "zoom_out" }));
		/// <summary>The title label</summary>
		internal Label TitleLabel = new Label(Program.Renderer, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"switchmenu", "title"}));
		/// <summary>The GUID of the currently selected switch</summary>
		private Guid selectedSwitch = Guid.Empty;
		/// <summary>The list of available switches</summary>
		internal Dictionary<Guid, Vector2> AvailableSwitches;

		private TrackFollower trackFollower;

		private int drawRadius;

		internal SwitchChangeDialog()
		{
			MapPicturebox.Location = new Vector2(0, 0);
			CloseButton.OnClick += Close;
			ZoomInButton.OnClick += ZoomIn;
			ZoomOutButton.OnClick += ZoomOut;
		}

		internal void Show()
		{
			trackFollower = Program.Renderer.Camera.CurrentMode <= CameraViewMode.Exterior ? TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower : Program.Renderer.CameraTrackFollower;
			drawRadius = 500;
			Program.Renderer.GameWindow.CursorVisible = true;
			Program.Renderer.SetCursor(MouseCursor.Default); // as we may have hidden the cursor through inactivity or be over a touch control when triggering
			Program.Renderer.CurrentOutputMode = OutputMode.None;
			MapPicturebox.Size = new Vector2(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height); // as size may have changed between fullscreen etc.
			CloseButton.Location = new Vector2(Program.Renderer.Screen.Width * 0.9, Program.Renderer.Screen.Height * 0.9);
			ZoomInButton.Location = new Vector2(Program.Renderer.Screen.Width * 0.7, Program.Renderer.Screen.Height * 0.9);
			ZoomOutButton.Location = new Vector2(ZoomInButton.Location.X - ZoomOutButton.Size.X - 5, Program.Renderer.Screen.Height * 0.9);
			TextureManager.UnloadTexture(ref MapPicturebox.Texture);
			Program.CurrentHost.RegisterTexture(Illustrations.CreateRouteMap(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, true, out AvailableSwitches, trackFollower, drawRadius), TextureParameters.NoChange, out MapPicturebox.Texture);
			TitleLabel.Location = new Vector2(Program.Renderer.Screen.Width * 0.5 - TitleLabel.Size.X * 0.5, 5);
			CloseButton.IsVisible = true;
			ZoomInButton.IsVisible = true;
			ZoomOutButton.IsVisible = true;
		}

		internal void Draw()
		{
			Program.Renderer.PushMatrix(MatrixMode.Projection);
			Matrix4D.CreateOrthographicOffCenter(0.0f, Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out Program.Renderer.CurrentProjectionMatrix);
			Program.Renderer.PushMatrix(MatrixMode.Modelview);
			Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;
			MapPicturebox.Draw();
			if (selectedSwitch != Guid.Empty)
			{
				// Switch details

				Vector2 textLocation = new Vector2(10, 30);
				Program.Renderer.OpenGlString.Draw(Program.Renderer.Fonts.NormalFont, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"switchmenu", "selected"}) + Program.CurrentRoute.Switches[selectedSwitch].Name, textLocation, TextAlignment.CenterLeft, Color128.White);
				textLocation.Y += 20;
				if (!Program.CurrentRoute.Switches[selectedSwitch].FixedRoute)
				{
					// don't draw alternate path names for player path
					Program.Renderer.OpenGlString.Draw(Program.Renderer.Fonts.NormalFont, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "switchmenu", "current" }) + Program.CurrentRoute.Switches[selectedSwitch].CurrentSetting, textLocation, TextAlignment.CenterLeft, Color128.White);
					textLocation.Y += 20;
				}
				Program.Renderer.OpenGlString.Draw(Program.Renderer.Fonts.NormalFont, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "switchmenu", "distance" }) + (TrainManagerBase.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - Program.CurrentRoute.Switches[selectedSwitch].TrackPosition) + "m", textLocation, TextAlignment.CenterLeft, Color128.White);	
			}
			// Draw last so they overlay any curves on the map which are OTT
			ZoomInButton.Draw();
			ZoomOutButton.Draw();
			CloseButton.Draw();
			TitleLabel.Draw();
			Program.Renderer.PopMatrix(MatrixMode.Modelview);
			Program.Renderer.PopMatrix(MatrixMode.Projection);
		}

		/// <summary>Processes a mouse move event</summary>
		/// <param name="x">The screen-relative x coordinate of the move event</param>
		/// <param name="y">The screen-relative y coordinate of the move event</param>
		internal void ProcessMouseMove(int x, int y)
		{
			CloseButton.MouseMove(x, y);
			selectedSwitch = Guid.Empty;
			if (CloseButton.CurrentlySelected)
			{
				// don't bother to look for a switch if we've selected the button
				Program.Renderer.SetCursor(MouseCursor.Default);
				return;
			}
			
			for(int i = 0; i < AvailableSwitches.Count; i++)
			{
				Guid guid = AvailableSwitches.ElementAt(i).Key;
				if (x > AvailableSwitches[guid].X - 10 && x < AvailableSwitches[guid].X + 10 && 
				    y > AvailableSwitches[guid].Y - 10 && y < AvailableSwitches[guid].Y + 10)
				{
					// Selection radius of 10px either way, so 20px circle
					// This doesn't appear to be quite positioned right, but not sure if this is GDI+ or our drawing being wonky- Ignore for the minute.....
					Program.Renderer.SetCursor(AvailableCursors.CurrentCursor);
					selectedSwitch = guid;
					break;
				}
			}
			if (selectedSwitch == Guid.Empty || Program.CurrentRoute.Switches[selectedSwitch].FixedRoute)
			{
				// Not found an appropriate switch, so set back to default
				Program.Renderer.GameWindow.Cursor = MouseCursor.Default;
			}
			ZoomInButton.MouseMove(x, y);
			ZoomOutButton.MouseMove(x, y);
		}

		/// <summary>Processes a mouse down event</summary>
		/// <param name="x">The screen-relative x coordinate of the down event</param>
		/// <param name="y">The screen-relative y coordinate of the down event</param>
		internal void ProcessMouseDown(int x, int y)
		{
			Program.Renderer.GameWindow.CursorVisible = true;
			ProcessMouseMove(x, y);
			if (selectedSwitch != Guid.Empty)
			{
				Program.CurrentRoute.Switches[selectedSwitch].Toggle();
				// Unload existing texture and re-create with new path
				TextureManager.UnloadTexture(ref MapPicturebox.Texture);
				Program.CurrentHost.RegisterTexture(Illustrations.CreateRouteMap(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, true, out AvailableSwitches, trackFollower, drawRadius), TextureParameters.NoChange, out MapPicturebox.Texture);
			}
			CloseButton.MouseDown(x, y);
			ZoomInButton.MouseDown(x, y);
			ZoomOutButton.MouseDown(x, y);
		}

		/// <summary>Zooms the map in</summary>
		internal void ZoomIn(object sender, EventArgs e)
		{
			if (drawRadius < 50)
			{
				return;
			}

			drawRadius /= 2;
			// Unload existing texture and re-create with new path
			TextureManager.UnloadTexture(ref MapPicturebox.Texture);
			Program.CurrentHost.RegisterTexture(Illustrations.CreateRouteMap(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, true, out AvailableSwitches, trackFollower, drawRadius), TextureParameters.NoChange, out MapPicturebox.Texture);
		}

		/// <summary>Zooms the map out</summary>
		internal void ZoomOut(object sender, EventArgs e)
		{
			if (drawRadius > 1000)
			{
				return;
			}
			
			drawRadius *= 2;
			// Unload existing texture and re-create with new path
			TextureManager.UnloadTexture(ref MapPicturebox.Texture);
			Program.CurrentHost.RegisterTexture(Illustrations.CreateRouteMap(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, true, out AvailableSwitches, trackFollower, drawRadius), TextureParameters.NoChange, out MapPicturebox.Texture);
		}

		internal void Close(object sender, EventArgs e)
		{
			TextureManager.UnloadTexture(ref MapPicturebox.Texture);
			Program.Renderer.CurrentInterface = Program.Renderer.PreviousInterface;
			Program.Renderer.CurrentOutputMode = Program.Renderer.PreviousOutputMode;
		}
	}
}
