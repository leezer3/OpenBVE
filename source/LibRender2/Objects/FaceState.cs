﻿using System;
using OpenBveApi.Objects;

namespace LibRender2.Objects
{
	/// <summary>Represents a face state within the renderer</summary>
	public class FaceState : IDisposable
	{
		/// <summary>The containing object</summary>
		public readonly ObjectState Object;
		/// <summary>The face to draw</summary>
		public readonly MeshFace Face;
		/// <summary>Holds the reference to the base renderer</summary>
		public readonly BaseRenderer Renderer;

		public FaceState(ObjectState _object, MeshFace face, BaseRenderer renderer)
		{
			Object = _object;
			Face = face;
			Renderer = renderer;
		}

		public void Draw()
		{
			if (Renderer.AvailableNewRenderer)
			{
				Renderer.RenderFace(this);
			}
			else
			{
				Renderer.RenderFaceImmediateMode(this);
			}
		}

		public void Dispose()
		{
			Object?.Dispose();
		}
	}
}
