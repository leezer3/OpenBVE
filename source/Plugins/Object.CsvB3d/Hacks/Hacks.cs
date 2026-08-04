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

using OpenBveApi.Colors;
using OpenBveApi.Objects;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using OpenBveApi;

namespace Object.CsvB3d
{
	internal partial class NewParser
	{
		/// <summary>Checks for any applicable Face hacks</summary>
		internal static void CheckForFaceHacks(string fileName, MeshBuilder currentMeshBuilder, StaticObject staticObject, bool isFace2, ref int[] indicies)
		{
			if (Plugin.enabledHacks.BveTsHacks == false)
			{
				return;
			}

			if (fileName.FileNameContains(new[] { "hira2\\car" }) && indicies.SequenceEqual(new[] { 0, 1, 4, 5 }))
			{
				/*
				 * Fix glitchy Hirakami railway stock
				 */
				indicies = new[] { 0, 1, 5, 4 };
			}

			if (fileName.FileNameEndsWith(new[] { "Ryouso\\BlackRoof.csv" }) && indicies.SequenceEqual(new[] { 0, 1, 2, 3 }))
			{
				// broken roof
				indicies = new[] { 0, 2, 1, 3 };
			}

			if (fileName.FileNameEndsWith(new[] { "Ryouso\\wall\\wall_ugL.csv", "Ryouso\\wall\\wall_ugR.csv", "Ryouso\\wall\\wall_ug-staL.csv",
				    "Ryouso\\wall\\wall_ug-staR.csv"
			    }) && indicies.SequenceEqual(new[] { 0, 1, 2, 3 }) && staticObject.Mesh.Vertices.Length >= 8)
			{
				// broken roof (first face is OK though....)
				indicies = new[] { 0, 2, 3, 1 };
			}

			if (fileName.FileNameEndsWith(
				    new[] { "Ryouso\\wall\\wall_ug-staL_.csv", "Ryouso\\wall\\wall_ug-staR_.csv" }) &&
			    indicies.SequenceEqual(new[] { 0, 1, 2, 3 }) &&
			    (staticObject.Mesh.Vertices.Length == 4 || staticObject.Mesh.Vertices.Length == 12))
			{
				// broken roof (first face is OK though....)
				indicies = new[] { 0, 2, 3, 1 };
			}

			if (fileName.IndexOf("shr\\nature\\tree\\treeset", StringComparison.OrdinalIgnoreCase) != -1 ||
			    ((fileName.EndsWith("shr\\form\\accessory\\set2l.csv", StringComparison.InvariantCultureIgnoreCase) ||
			      fileName.EndsWith("shr\\form\\accessory\\set2r.csv", StringComparison.InvariantCultureIgnoreCase)) &&
			     staticObject.Mesh.Faces.Length < 2))
			{
				indicies = new[] { 0, 3, 1, 2 };
			}

			if (fileName.IndexOf("sanaro", StringComparison.OrdinalIgnoreCase) != -1)
			{
				/*
				 * A bunch of objects with the glitched face issue in this one
				 * Unfortunately, we can't just check the face windings, as there
				 * seem to be 'correct' faces with an identical winding to the broken
				 * ones
				 *
				 * At some point, possibly need to consider writing an algorithm to sort
				 * points in clockwise order rather than special-casing each one...
				 */
				if (fileName.FileNameEndsWith(new[] { "sanaro\\edifici\\casello1.csv" }))
				{
					switch (staticObject.Mesh.Faces.Length)
					{
						case 9:
							indicies = new[] { 0, 1, 3, 2 };
							break;
						case 15:
						case 16:
						case 17:
						case 18:
						case 19:
							indicies = new[] { 0, 1, 2, 3 };
							break;
					}
				}

				if (fileName.FileNameEndsWith(new[] { "sanaro\\edifici\\casello2.csv" }))
				{
					switch (staticObject.Mesh.Faces.Length)
					{
						case 10:
						case 11:
							indicies = new[] { 0, 1, 3, 2 };
							break;
					}
				}

				if (fileName.FileNameEndsWith(new[] { "sanaro\\edifici\\casello3.csv" }))
				{
					switch (staticObject.Mesh.Faces.Length)
					{
						case 9:
							indicies = new[] { 0, 1, 2, 3 };
							break;
					}
				}

				if (fileName.FileNameEndsWith(new[] { "sanaro\\edifici\\carisio.csv" }))
				{
					switch (staticObject.Mesh.Faces.Length)
					{
						case 9:
							indicies = new[] { 0, 1, 2, 3 };
							break;
					}
				}

				if (fileName.FileNameEndsWith(new[] { "sanaro\\edifici\\carisio.csv" }))
				{
					switch (staticObject.Mesh.Faces.Length)
					{
						case 27:
						case 28:
							indicies = new[] { 0, 1, 2, 3 };
							break;
					}
				}

				if (fileName.FileNameEndsWith(new[] { "sanaro\\edifici\\tettoia.csv" }))
				{
					switch (staticObject.Mesh.Faces.Length)
					{
						case 8:
						case 9:
						case 10:
							indicies = new[] { 0, 1, 2, 3 };
							break;
					}
				}
			}

			if (fileName.FileNameEndsWith(new[] { "USF\\bridge.b3d" }))
			{
				switch (staticObject.Mesh.Faces.Length)
				{
					case 0:
						indicies = new[] { 0, 2, 3, 1 };
						break;
				}
			}

			if (fileName.FileNameEndsWith(new[] { "hanzomon\\enshin\\sumi\\sumiendForm.csv" }))
			{
				if (staticObject.Mesh.Faces.Length == 8 && indicies.SequenceEqual(new[] { 3, 2, 4, 5 }))
				{
					indicies = new[] { 2, 3, 4, 5 };
				}
			}

			int[] vertexIndices = (int[])indicies.Clone();
			Array.Sort(vertexIndices);
			for (int k = 0; k < currentMeshBuilder.Faces.Count; k++)
			{
				/*
				 * Some BVE2 content declares a Face2 twice with the vertices in a differing order, e.g.
				 *
				 * Face2 0, 1, 3, 2
				 * Face2 1, 0, 2, 3
				 *
				 * Doing this in OpenBVE causes some very funky glitches with Z-fighting,
				 * as it attempts to render both faces in the same space
				 *
				 * With this hack, the lighting may be off but the Z-fighting is gone
				 * (BVE2 / BVE4 operate in a strict draw-order, so the most recent face is always on top,
				 * wheras OpenBVE has no fixed draw order)
				 */
				MeshFace currentFace = currentMeshBuilder.Faces[k];
				if (!isFace2 || (currentFace.Flags & FaceFlags.Face2Mask) == 0)
				{
					continue;
				}

				if (currentFace.Vertices.Length != indicies.Length)
				{
					continue;
				}

				int[] faceVertexIndices = new int[indicies.Length];
				for (int v = 0; v < currentFace.Vertices.Length; v++)
				{
					faceVertexIndices[v] = currentFace.Vertices[v].Index;
				}

				Array.Sort(faceVertexIndices);
				if (vertexIndices.SequenceEqual(faceVertexIndices))
				{
					indicies = Array.Empty<int>();
				}
			}
		}

		internal static void CheckForColorHacks(string fileName, MeshBuilder currentMeshBuilder, StaticObject staticObject, ref Color32 c)
		{
			if (Plugin.enabledHacks.BveTsHacks == false)
			{
				return;
			}

			if (c.A == 0 && Plugin.enabledHacks.DisableSemiTransparentFaces)
			{
				/*
				 * BVE2 didn't support semi-transparent faces at all
				 * BVE4 treats faces with an opacity value of 0 as having an opacity value of 1
				 */
				c.A = 255;
			}

			if (!string.IsNullOrEmpty(currentMeshBuilder.Materials[0].DaytimeTexture) && currentMeshBuilder.Materials[0].DaytimeTexture.EndsWith("Loop3\\CF_BW.bmp", StringComparison.InvariantCultureIgnoreCase))
			{
				/*
				 * Glitched container color in Loop v3
				 * This appears to actually be a 'true' developer typo (same in BVE Structure Viewer) but looks awful so let's fix
				 */
				c = Color32.White;
			}
		}

		internal static bool CheckForTextureHacks(string fileName, ref string textureFile)
		{
			if (Plugin.enabledHacks.BveTsHacks == false)
			{
				return true;
			}
			//Original BVE2 signal graphics
			Match m = Regex.Match(textureFile, @"(signal\d{1,2}\.bmp)", RegexOptions.IgnoreCase);
			if (m.Success)
			{
				string s = "Signals\\Static\\" + m.Groups[0].Value.Replace(".bmp", ".png");
				textureFile = Path.CombineFile(Plugin.CompatibilityFolder, s);
				return true;
			}

			if (textureFile.StartsWith("swiss1/", StringComparison.InvariantCultureIgnoreCase))
			{
				textureFile = textureFile.Substring(7);
				textureFile = Path.CombineFile(Path.GetDirectoryName(fileName), textureFile);
				if (System.IO.File.Exists(textureFile))
				{
					return true;
				}
			}

			if (textureFile.StartsWith("/U5/", StringComparison.InvariantCultureIgnoreCase))
			{
				textureFile = textureFile.Substring(4);
				textureFile = Path.CombineFile(Path.GetDirectoryName(fileName), textureFile);
				if (System.IO.File.Exists(textureFile))
				{
					return true;
				}
			}

			if (textureFile.StartsWith("U5/", StringComparison.InvariantCultureIgnoreCase))
			{
				textureFile = textureFile.Substring(3);
				textureFile = Path.CombineFile(Path.GetDirectoryName(fileName), textureFile);
				if (System.IO.File.Exists(textureFile))
				{
					return true;
				}
			}

			return false;
		}
	}
}
