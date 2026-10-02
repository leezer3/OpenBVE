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

using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.Xml;
using Formats.OpenBve;
using OpenBveApi;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace LokSimRouteParser
{
	internal class L3DRail
	{
		internal double Gauge;

		internal double RailTopWidth;

		internal double RailTopHeight;

		internal double BallastWidth;

		internal bool UsesTransparentColor;

		internal List<Vector3> BallastPoints = new List<Vector3>();

		internal StaticObject Object;

		internal Vector2[] railTopTextureCoordinates  = new Vector2[2];

		internal Vector2[] railSideTextureCoordinates = new Vector2[2];

		internal Vector2[] ballastTextureCoordinates = new Vector2[2];

		internal Vector2 textureSize;

		internal string textureFile;

		internal L3DRail(string fileName)
		{
			// https://eu07.pl/forum/showthread.php?tid=6244&page=2
			// rail is generated in 10m lengths
			// follows that the texture co-ordinates must be relative to said 10m
			// unsure as yet as to how switches are generated
			// must do some twiddling to sort for radius
			// It might be easier if we just generate a ~0.5m segment and use this?? 


			AttributedXMLFile<LoksimNode, LoksimAttribute> railFile = new AttributedXMLFile<LoksimNode, LoksimAttribute>(fileName, "/RAIL", Plugin.CurrentHost);

			railFile.GetValue(LoksimAttribute.Transparent, out UsesTransparentColor);
			railFile.GetValue(LoksimAttribute.Spurbreite, out Gauge);
			Gauge /= 2;
			railFile.GetValue(LoksimAttribute.Schienenbreite, out RailTopWidth);
			railFile.GetValue(LoksimAttribute.Schienenhoehe, out RailTopHeight);
			railFile.GetValue(LoksimAttribute.Bettungsbreite, out BallastWidth);
			if (railFile.GetPath(LoksimAttribute.Texture, Path.GetDirectoryName(fileName), out textureFile))
			{
				Plugin.CurrentHost.QueryTextureDimensions(textureFile, out int tx, out int ty);
				textureSize.X = tx;
				textureSize.Y = ty;
			}

			while (railFile.RemainingSubBlocks > 0)
			{
				Block<LoksimNode, LoksimAttribute> subBlock = railFile.ReadNextBlock();
				switch (subBlock.Key)
				{
					case LoksimNode.Hoehe:
						subBlock.GetVector3(LoksimAttribute.Hoehe, ';', out Vector3 v);
						BallastPoints.Add(v);
						break;
					case LoksimNode.TexSchieneOben:
						subBlock.GetValue(LoksimAttribute.x1, out railTopTextureCoordinates[0].X);
						subBlock.GetValue(LoksimAttribute.x2, out railTopTextureCoordinates[1].X);
						subBlock.GetValue(LoksimAttribute.y1, out railTopTextureCoordinates[0].Y);
						subBlock.GetValue(LoksimAttribute.y2, out railTopTextureCoordinates[1].Y);
						break;
					case LoksimNode.TexSchieneSeite:
						subBlock.GetValue(LoksimAttribute.x1, out railSideTextureCoordinates[0].X);
						subBlock.GetValue(LoksimAttribute.x2, out railSideTextureCoordinates[1].X);
						subBlock.GetValue(LoksimAttribute.y1, out railSideTextureCoordinates[0].Y);
						subBlock.GetValue(LoksimAttribute.y2, out railSideTextureCoordinates[1].Y);
						break;
					case LoksimNode.TexBettung:
						subBlock.GetValue(LoksimAttribute.x1, out ballastTextureCoordinates[0].X);
						subBlock.GetValue(LoksimAttribute.x2, out ballastTextureCoordinates[1].X);
						subBlock.GetValue(LoksimAttribute.y1, out ballastTextureCoordinates[0].Y);
						subBlock.GetValue(LoksimAttribute.y2, out ballastTextureCoordinates[1].Y);
						break;
				}
			}
			
			Object = new StaticObject(Plugin.CurrentHost);
			
			MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);
			// create material to be used
			Material material = new Material(textureFile);
			builder.Materials[0] = material;

			// build rail models
			// NOTE: This can definitely be optimised, but not doing that at the minute

			// rail side L
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2 + RailTopHeight, 0), new Vector2(railSideTextureCoordinates[0].X / textureSize.X, railSideTextureCoordinates[1].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2, 0), new Vector2(railSideTextureCoordinates[0].X / textureSize.X, railSideTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2, 10), new Vector2(railSideTextureCoordinates[1].X / textureSize.X, railSideTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2 + RailTopHeight, 10), new Vector2(railSideTextureCoordinates[1].X / textureSize.X, railSideTextureCoordinates[1].Y / textureSize.Y)));
			
			// rail top L
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2 + RailTopHeight, 0), new Vector2(railTopTextureCoordinates[0].X / textureSize.X, railTopTextureCoordinates[1].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge - RailTopWidth, 0.2 + RailTopHeight, 0), new Vector2(railTopTextureCoordinates[0].X / textureSize.X, railTopTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge - RailTopWidth, 0.2 + RailTopHeight, 10), new Vector2(railTopTextureCoordinates[1].X / textureSize.X, railTopTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2 + RailTopHeight, 10), new Vector2(railTopTextureCoordinates[1].X / textureSize.X, railTopTextureCoordinates[1].Y / textureSize.Y)));
			MeshFace face = new MeshFace(new[] { 0, 1, 2, 3 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);
			face = new MeshFace(new[] { 4, 5, 6, 7 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);
			builder.Apply(ref Object);
			builder = new MeshBuilder(Plugin.CurrentHost);
			builder.Materials[0] = material;
			// rail side R
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2 + RailTopHeight, 0), new Vector2(railSideTextureCoordinates[0].X / textureSize.X, railSideTextureCoordinates[1].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2, 0), new Vector2(railSideTextureCoordinates[0].X / textureSize.X, railSideTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2, 10), new Vector2(railSideTextureCoordinates[1].X / textureSize.X, railSideTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2 + RailTopHeight, 10), new Vector2(railSideTextureCoordinates[1].X / textureSize.X, railSideTextureCoordinates[1].Y / textureSize.Y)));
			// rail top R
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2 + RailTopHeight, 0), new Vector2(railTopTextureCoordinates[0].X / textureSize.X, railTopTextureCoordinates[1].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge + RailTopWidth, 0.2 + RailTopHeight, 0), new Vector2(railTopTextureCoordinates[0].X / textureSize.X, railTopTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge + RailTopWidth, 0.2 + RailTopHeight, 10), new Vector2(railTopTextureCoordinates[1].X / textureSize.X, railTopTextureCoordinates[0].Y / textureSize.Y)));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2 + RailTopHeight, 10), new Vector2(railTopTextureCoordinates[1].X / textureSize.X, railTopTextureCoordinates[1].Y / textureSize.Y)));
			face = new MeshFace(new[] { 0, 1, 2, 3 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);
			face = new MeshFace(new[] { 4, 5, 6, 7 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);

			builder.Apply(ref Object);
			builder = new MeshBuilder(Plugin.CurrentHost);
			builder.Materials[0] = material;

			// build ballast model:
			// points are those of *half* an end profile in clockwise direction
			// e.g. 
			//			*-----*
			//				   \
			//					\
			//					 *
			//					 |
			//					 |
			//					 *
			// we then negate the X co-ordinate to build the other side
			// TexBettung appears to be the rectangle across the whole cross-section
			// current assumption is that our X-coords are proportional to the distance in X axis

			List<int> ballast = new List<int>();
			int[] sleeperBase = { 0,1,0,0 };

			ballastTextureCoordinates[0] /= textureSize;
			ballastTextureCoordinates[1] /= textureSize;

			for (int i = 0; i < BallastPoints.Count; i++)
			{
				Vector3 point = BallastPoints[i];
				Vector2 textureCoordinates = new Vector2(ballastTextureCoordinates[1]);
				textureCoordinates.X = ballastTextureCoordinates[1].X - ((ballastTextureCoordinates[1].X - ballastTextureCoordinates[0].X) * ((point.X - BallastPoints[0].X) / (BallastPoints[BallastPoints.Count - 1].X - BallastPoints[0].X)));
				if (i % 2 != 0)
				{
					// far
					point.Z += 10;
					builder.Vertices.Add(new Vertex(point, textureCoordinates));
					ballast.Add(builder.Vertices.Count - 1);
					point.Z -= 10;
				}
				else
				{
					// near
					textureCoordinates.Y = ballastTextureCoordinates[0].Y;
					builder.Vertices.Add(new Vertex(point, textureCoordinates));
					ballast.Add(builder.Vertices.Count - 1);
					point.Z += 10;
				}

				textureCoordinates.Y = ballastTextureCoordinates[0].Y;
				builder.Vertices.Add(new Vertex(new Vector3(point), Vector2.Null));
				ballast.Add(builder.Vertices.Count - 1);
			}
			face = new MeshFace(ballast.ToArray());
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);

			sleeperBase[2] = builder.Vertices.Count + 1;
			sleeperBase[3] = builder.Vertices.Count;
			
			ballast = new List<int>();
			for (int i = 0; i < BallastPoints.Count; i++)
			{
				Vector3 point = BallastPoints[i];
				Vector2 textureCoordinates = new Vector2(ballastTextureCoordinates[1]);
				textureCoordinates.X = ballastTextureCoordinates[1].X - ((ballastTextureCoordinates[1].X - ballastTextureCoordinates[0].X) * ((point.X - BallastPoints[0].X) / (BallastPoints[BallastPoints.Count - 1].X - BallastPoints[0].X)));
				point.X = -point.X;
				if (i % 2 != 0)
				{
					// far
					point.Z += 10;
					builder.Vertices.Add(new Vertex(point, textureCoordinates));
					ballast.Add(builder.Vertices.Count - 1);
					point.Z -= 10;
				}
				else
				{
					// near
					builder.Vertices.Add(new Vertex(point, textureCoordinates));
					ballast.Add(builder.Vertices.Count - 1);
					point.Z += 10;
				}

				textureCoordinates.Y = ballastTextureCoordinates[0].Y;
				builder.Vertices.Add(new Vertex(new Vector3(point), textureCoordinates));
				ballast.Add(builder.Vertices.Count - 1);
			}
			face = new MeshFace(ballast.ToArray());
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);

			face = new MeshFace(sleeperBase);
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);

			builder.Apply(ref Object);
		}
	}
}
