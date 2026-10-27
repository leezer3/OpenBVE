using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Overlays
{
	/// <summary>A rendered rail path overlay</summary>
	public class RailPath
	{
		private readonly HostInterface currentHost;

		private readonly BaseRenderer Renderer;
		/// <summary>The length of a track block</summary>
		private readonly double BlockLength;
		/// <summary>The overlay color to render</summary>
		public Color24 Color;
		/// <summary>The line width to use</summary>
		internal int LineWidth;
		/// <summary>The key of the rail</summary>
		internal readonly int RailKey;
		/// <summary>A textual description of the rail's purpose</summary>
		public string Description;

		public bool Display;

		/// <summary>The VAO holding the line strip vertices.</summary>
		private VertexArrayObject pathVao;
		/// <summary>The number of vertices the current VAO can hold.</summary>
		private int pathVaoCapacity;
		/// <summary>Reusable vertex buffer matching the VAO capacity.</summary>
		private LibRenderVertex[] pathVertices = new LibRenderVertex[0];
		private readonly List<int> sectionStarts = new List<int>();
		private readonly List<int> sectionCounts = new List<int>();

		/// <summary>Whether the path is visible at the current camera location</summary>
		public bool CurrentlyVisible()
		{
			return Display && Visible(Renderer.CameraTrackFollower.TrackPosition, out _);
		}

		/// <summary>Whether the path is visible at the specified location</summary>
		/// <param name="trackPosition">The track position to check</param>
		/// <returns></returns>
		public bool Visible(double trackPosition)
		{
			return Display && Visible(trackPosition, out _);
		}

		/// <summary>Whether this path is visible at the specified location</summary>
		/// <param name="trackPosition">The track position to check</param>
		/// <param name="startElement">Returns the starting visible element number</param>
		private bool Visible(double trackPosition, out int startElement)
		{
			startElement = 0;
			if (Display)
			{
				for (int e = 0; e < currentHost.Tracks[RailKey].Elements.Length; e++)
				{
					if (Math.Abs(currentHost.Tracks[RailKey].Elements[e].StartingTrackPosition -trackPosition) <= 25.0)
					{
						startElement = e;
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>Renders the path overlay</summary>
		public void Render()
		{
			double halfDistance = (Math.Max(Renderer.currentOptions.ViewingDistance, 1000) / 2.0) * 1.1;
			int numElements = (int)(halfDistance / BlockLength);
			if (!Display || !Visible(Renderer.CameraTrackFollower.TrackPosition, out int startElement))
			{
				return;
			}
			int firstElement = Math.Max(0, startElement - numElements);
			int lastElement = Math.Min(currentHost.Tracks[RailKey].Elements.Length, startElement + numElements);
			if (lastElement < firstElement)
			{
				return;
			}

			List<List<Vector3>> sections = new List<List<Vector3>>{ new List<Vector3>() };
			bool lastElementInvalid = false;
			
			for (int e = firstElement; e < lastElement; e++)
			{
				if (currentHost.Tracks[RailKey].Elements[e].WorldPosition != Vector3.Zero)
				{
					bool invalidElement = currentHost.Tracks[RailKey].Elements[e].InvalidElement;
					
					if (invalidElement && lastElementInvalid)
					{
						if(sections.Last().Count > 0) sections.Add(new List<Vector3>());
					}
					else
					{
						sections.Last().Add(new Vector3(currentHost.Tracks[RailKey].Elements[e].WorldPosition.X, currentHost.Tracks[RailKey].Elements[e].WorldPosition.Y + 0.5, currentHost.Tracks[RailKey].Elements[e].WorldPosition.Z));
					}
					lastElementInvalid = invalidElement;
				}
			}

			// The shader and its matrices are set up by the renderer; calling Render
			// without it (e.g. during route loading) has nothing to draw.
			if (Renderer.LineShader == null || Renderer.CurrentShader != Renderer.LineShader)
			{
				return;
			}

			int vertexCount = 0;
			sectionStarts.Clear();
			sectionCounts.Clear();
			foreach (List<Vector3> points in sections)
			{
				if (points.Count < 2)
				{
					continue;
				}

				sectionStarts.Add(vertexCount);
				sectionCounts.Add(points.Count);
				vertexCount += points.Count;
			}

			if (vertexCount == 0)
			{
				return;
			}

			EnsurePathVao(vertexCount);

			int vertexIndex = 0;
			foreach (List<Vector3> points in sections)
			{
				if (points.Count < 2)
				{
					continue;
				}

				for (int j = 0; j < points.Count; j++)
				{
					pathVertices[vertexIndex++] = new LibRenderVertex(new Vector3f(points[j].X, points[j].Y, points[j].Z));
				}
			}

			pathVao.Bind();
			Renderer.lastVAO = pathVao.handle;
			pathVao.UpdateVBO(pathVertices);
			Renderer.LineShader.SetColor(new Color128(Color));

			GL.LineWidth(LineWidth);
			for (int i = 0; i < sectionStarts.Count; i++)
			{
				pathVao.DrawArrays(PrimitiveType.LineStrip, sectionStarts[i], sectionCounts[i]);
			}
			GL.LineWidth(1);
		}

		/// <summary>Creates or grows the VAO used to draw the line strips.</summary>
		private void EnsurePathVao(int vertexCount)
		{
			if (pathVao != null && pathVaoCapacity >= vertexCount)
			{
				return;
			}

			if (pathVao != null)
			{
				pathVao.UnBind();
				pathVao.Dispose();
			}

			pathVaoCapacity = Math.Max(vertexCount, 64);
			pathVertices = new LibRenderVertex[pathVaoCapacity];
			pathVao = new VertexArrayObject();
			pathVao.Bind();
			pathVao.SetVBO(new VertexBufferObject(pathVertices, BufferUsageHint.DynamicDraw));
			pathVao.SetAttributes(Renderer.LineShader.VertexLayout);
			Renderer.lastVAO = -1;
		}


		public RailPath(HostInterface host, BaseRenderer renderer, int key, double blockLength, Color24 color)
		{
			currentHost = host;
			Renderer = renderer;
			RailKey = key;
			BlockLength = blockLength;
			Color = color;
			LineWidth = 2;
			Display = true;
			Description = host.Tracks[RailKey].Name;
		}

	}
}
