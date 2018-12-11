using System;
using BackgroundManager;
using OpenBveApi.Objects;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenBveShared
{
	public static partial class Renderer
	{
		/// <summary>The mode the viewport should change to</summary>
		public enum ViewPortChangeMode
		{
			ChangeToScenery = 0,
			ChangeToCab = 1,
			NoChange = 2
		}

		/// <summary>The viewport modes</summary>
		public enum ViewPortMode
		{
			Scenery = 0,
			Cab = 1
		}
	}
}
