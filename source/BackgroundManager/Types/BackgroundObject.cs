using System;
using LibRender;
using OpenBveApi.Objects;
using OpenBveApi.Routes;

namespace OpenBve.BackgroundManager
{
	/// <summary>Represents a background object</summary>
	public class BackgroundObject : BackgroundHandle
	{
		/// <summary>The object used for this background (NOTE: Static objects only)</summary>
		public readonly StaticObject ObjectBackground;
		/// <summary>The clipping distance required to fully render the object</summary>
		public readonly double ClipDistance = 0;

		/// <summary>Creates a new background object</summary>
		/// <param name="Object">The object to use for the background</param>
		public BackgroundObject(StaticObject Object)
		{
			this.ObjectBackground = Object;
			//As we are using an object based background, calculate the minimum clip distance
			for (int i = 0; i > Object.Mesh.Vertices.Length; i++)
			{
				double X = Math.Abs(Object.Mesh.Vertices[i].Coordinates.X);
				double Z = Math.Abs(Object.Mesh.Vertices[i].Coordinates.Z);
				if (X < ClipDistance)
				{
					ClipDistance = X;
				}
				if (Z < ClipDistance)
				{
					ClipDistance = Z;
				}
			}
		}

		public override void UpdateBackground(double TimeElapsed, bool Target)
		{
			//No updates required
		}

		public override void RenderBackground(float Alpha, float Scale)
		{
			Backgrounds.RenderBackground(this);
		}

		public override void RenderBackground(float Scale)
		{
			Backgrounds.RenderBackground(this);
		}
	}
}
