//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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

using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenBveApi.World;
using System;
using System.Collections.Generic;
using Formats.OpenBve;
using OpenBveApi.Colors;

namespace LokSimRouteParser
{
	internal class LandschaftsObjekt
	{
		internal StaticObject internalObject;

		internal double TrackPosition;

		internal bool Polygon;

		internal double Scale;

		internal double Distance;

		internal double Length;

		internal LightStates Lighting;

		internal LandschaftsObjekt(Block<LoksimNode, LoksimAttribute> landschaftBlock)
		{
			// A Landschaftsobjekt is a world ground object
			internalObject = new StaticObject(Plugin.CurrentHost);

			string textureFile = string.Empty;
			bool shouldTexture = true;

			landschaftBlock.GetValue(LoksimAttribute.Abstand, out Distance);
			landschaftBlock.GetValue(LoksimAttribute.Position, out TrackPosition);
			landschaftBlock.GetValue(LoksimAttribute.Polygon, out Polygon);
			landschaftBlock.GetValue(LoksimAttribute.Scale, out Scale);
			if (landschaftBlock.GetPath(LoksimAttribute.TextureFile, Plugin.FileSystem.LoksimDataDirectory, out textureFile))
			{
				landschaftBlock.GetValue(LoksimAttribute.Texture, out shouldTexture);
			}
			else
			{
				shouldTexture = false;
			}

			// Zwischenpunkte : additional points for curving the object

			// may contain multiple Landschaftsobjekt blocks
			List<Vector3> coordinateList = new List<Vector3>();
			while (landschaftBlock.RemainingSubBlocks > 0)
			{
				Block<LoksimNode, LoksimAttribute> subBlock = landschaftBlock.ReadNextBlock();
				switch (subBlock.Key)
				{
					case LoksimNode.Landschaftsobjekt:
					{
						Vector3 coordinates = new Vector3();
						subBlock.GetValue(LoksimAttribute.Hoehe, out coordinates.Y);
						subBlock.GetValue(LoksimAttribute.Position, out coordinates.Z);
						Length = Math.Max(coordinates.Z, Length);
						subBlock.GetValue(LoksimAttribute.Verschiebung, out coordinates.X);
						// Hoehefrei : pops the point to the top of the Z stack
						coordinateList.Add(coordinates);
						break;
					}
					case LoksimNode.Lightning:
						Lighting = new LightStates(subBlock);
						break;
				}
			}

			
			/*
			 * Need to figure out how to deal with the handling of these properly.
			 *
			 * This works as follows:
			 * --------------------------------
			 * If should texture is not set (by an empty / missing texture file)
			 * there is actually a 'hole' in the Loksim generated ground,
			 * corresponding to the bounds of our LandschaftsObjeckt
			 *
			 * Need to dig into this some more, but I *suspect* that any area
			 * covered by a LandschaftsObjeckt actually doesn't generate a ground at all
			 * (most of this then masked by the badly limited camera view ability)
			 */

			internalObject.Mesh.Vertices = new VertexTemplate[coordinateList.Count];
			List<int> vertexWinding = new List<int>();
			for (int i = 0; i < coordinateList.Count; i++)
			{
				switch (i % 4)
				{
					case 0:
						internalObject.Mesh.Vertices[i] = new Vertex(coordinateList[i], new Vector2(0, 0));
						break;
					case 1:
						internalObject.Mesh.Vertices[i] = new Vertex(coordinateList[i], new Vector2(1, 0));
						break;
					case 2:
						internalObject.Mesh.Vertices[i] = new Vertex(coordinateList[i], new Vector2(1, 1));
						break;
					case 3:
						internalObject.Mesh.Vertices[i] = new Vertex(coordinateList[i], new Vector2(0, 1));
						break;
				}
				vertexWinding.Add(i);
			}

			internalObject.Mesh.Faces = new[]
			{
				new MeshFace(vertexWinding.ToArray(), 0, FaceFlags.Face2Mask)
			};
			internalObject.OptimizeObject(true, 0, true);

			MeshMaterial material = new MeshMaterial();
			material.Color = Color32.White;

			if (shouldTexture && !string.IsNullOrEmpty(textureFile))
			{
				Plugin.CurrentHost.RegisterTexture(textureFile, new TextureParameters(null, null), out material.DaytimeTexture);
			}
			internalObject.Mesh.Materials = new[]
			{
				material
			};
			
		}

		internal void Create(TrackFollower t)
		{
			t.UpdateAbsolute(TrackPosition, true, false);
			Vector3 fPos = t.WorldPosition;
			t.UpdateRelative(Length, true, false);
			Vector3 rPos = t.WorldPosition;
			Vector3 d = fPos == rPos ? fPos : new Vector3(rPos - fPos);
			double tt = d.Magnitude();
			d *= tt;
			tt = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
			double ex = d.X * tt;
			double ez = d.Z * tt;
			Vector3 s = new Vector3(ez, 0.0, -ex);
			Vector3 u = Vector3.Cross(d, s);
			Transformation tr = new Transformation(d, u, s);
			// 
			internalObject.CreateObject(fPos, tr, new ObjectCreationParameters(TrackPosition, TrackPosition -500, TrackPosition + 500));
		}
	}
}
