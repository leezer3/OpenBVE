using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Primitives;
using LibRender2.Screens;
using LibRender2.Textures;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
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
		internal Button CloseButton = new Button(Program.Renderer, "Close");
		/// <summary>The GUID of the currently selected switch</summary>
		private Guid selectedSwitch = Guid.Empty;
		/// <summary>The list of available switches</summary>
		internal Dictionary<Guid, Vector2> AvailableSwitches;
		/// <summary>Stores the previous output mode of the renderer</summary>
		private OutputMode previousOutputMode;

		internal SwitchChangeDialog()
		{
			MapPicturebox.Location = new Vector2(0, 0);
			CloseButton.OnClick += Close;
		}

		internal void Show()
		{
			previousOutputMode = Program.Renderer.CurrentOutputMode;
			Program.Renderer.CurrentOutputMode = OutputMode.None;
			MapPicturebox.Size = new Vector2(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height); // as size may have changed between fullscreen etc.
			CloseButton.Location = new Vector2(Program.Renderer.Screen.Width * 0.9, Program.Renderer.Screen.Height * 0.9);
			TextureManager.UnloadTexture(ref MapPicturebox.Texture);
			Program.CurrentHost.RegisterTexture(Illustrations.CreateRouteMap(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, true, out AvailableSwitches, TrainManagerBase.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition), new TextureParameters(null, null), out MapPicturebox.Texture);
		}

		internal void Draw()
		{
			MapPicturebox.Draw();
			CloseButton.Draw();
			if (selectedSwitch != Guid.Empty)
			{
				// Switch details
				Program.Renderer.OpenGlString.Draw(Program.Renderer.Fonts.NormalFont, "Selected Switch: " + Program.CurrentRoute.Switches[selectedSwitch].Name, new Vector2(10, 10), TextAlignment.CenterLeft, Color128.White);
				Program.Renderer.OpenGlString.Draw(Program.Renderer.Fonts.NormalFont, "Current Setting: Rail " + Program.CurrentRoute.Switches[selectedSwitch].CurrentlySetTrack, new Vector2(10, 30), TextAlignment.CenterLeft, Color128.White);
				Program.Renderer.OpenGlString.Draw(Program.Renderer.Fonts.NormalFont, "Distance From Player: " + (TrainManagerBase.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - Program.CurrentRoute.Switches[selectedSwitch].TrackPosition) + "m", new Vector2(10, 50), TextAlignment.CenterLeft, Color128.White);
			}
		}

		/// <summary>Processes a mouse move event</summary>
		/// <param name="x">The screen-relative x coordinate of the move event</param>
		/// <param name="y">The screen-relative y coordinate of the move event</param>
		internal void ProcessMouseMove(int x, int y)
		{
			CloseButton.MouseMove(x, y);
			selectedSwitch = Guid.Empty;
			for(int i = 0; i < AvailableSwitches.Count; i++)
			{
				Guid guid = AvailableSwitches.ElementAt(i).Key;
				if (x > AvailableSwitches[guid].X - 10 && x < AvailableSwitches[guid].X + 10 && 
				    y > AvailableSwitches[guid].Y - 10 && y < AvailableSwitches[guid].Y + 10)
				{
					// Selection radius of 10px either way, so 20px circle
					Program.Renderer.SetCursor(AvailableCursors.CurrentCursor);
					selectedSwitch = guid;
					break;
				}
			}
			if (selectedSwitch == Guid.Empty)
			{
				// Not found an appropriate switch, so set back to default
				Program.currentGameWindow.Cursor = MouseCursor.Default;
			}

		}

		/// <summary>Processes a mouse down event</summary>
		/// <param name="x">The screen-relative x coordinate of the down event</param>
		/// <param name="y">The screen-relative y coordinate of the down event</param>
		internal void ProcessMouseDown(int x, int y)
		{
			ProcessMouseMove(x, y);
			if (selectedSwitch != Guid.Empty)
			{
				Program.CurrentRoute.Switches[selectedSwitch].Toggle();
				// Unload existing texture and re-create with new path
				TextureManager.UnloadTexture(ref MapPicturebox.Texture);
				Program.CurrentHost.RegisterTexture(Illustrations.CreateRouteMap(Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, true, out AvailableSwitches, TrainManagerBase.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition), new TextureParameters(null, null), out MapPicturebox.Texture);
			}
			CloseButton.MouseDown(x, y);
		}

		internal void Close(object sender, EventArgs e)
		{
			TextureManager.UnloadTexture(ref MapPicturebox.Texture);
			Program.Renderer.CurrentInterface = InterfaceType.Normal;
			Program.Renderer.CurrentOutputMode = previousOutputMode;
		}
	}
}
