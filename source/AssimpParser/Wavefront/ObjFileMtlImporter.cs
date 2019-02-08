// Open Asset Import Library (assimp)
//
// Copyright (c) 2006-2016, assimp team, 2018, The openBVE Project
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms,
// with or without modification, are permitted provided that the
// following conditions are met:
//
// * Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// * Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// * Neither the name of the assimp team, nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission of the assimp team.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//
//
// ******************************************************************************
//
// AN EXCEPTION applies to all files in the ./test/models-nonbsd folder.
// These are 3d models for testing purposes, from various free sources
// on the internet. They are - unless otherwise stated - copyright of
// their respective creators, which may impose additional requirements
// on the use of their work. For any of these models, see
// <model-name>.source.txt for more legal information. Contact us if you
// are a copyright holder and believe that we credited you inproperly or
// if you don't want your files to appear in the repository.
//
//
// ******************************************************************************
//
// Poly2Tri Copyright (c) 2009-2010, Poly2Tri Contributors
// http://code.google.com/p/poly2tri/
//
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// * Neither the name of Poly2Tri nor the names of its contributors may be
//   used to endorse or promote products derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics;

namespace AssimpNET.Obj
{

	internal class ObjFileMtlImporter : ObjTools
	{

		// Material specific token (case insensitive compare)
		private const string DiffuseTexture = "map_Kd";

		private const string AmbientTexture = "map_Ka";
		private const string SpecularTexture = "map_Ks";
		private const string OpacityTexture = "map_d";
		private const string EmissiveTexture1 = "map_emissive";
		private const string EmissiveTexture2 = "map_Ke";
		private const string BumpTexture1 = "map_bump";
		private const string BumpTexture2 = "bump";
		private const string NormalTexture = "map_Kn";
		private const string ReflectionTexture = "refl";
		private const string DisplacementTexture1 = "map_disp";
		private const string DisplacementTexture2 = "disp";
		private const string SpecularityTexture = "map_ns";

		// texture option specific token
		private const string BlendUOption = "-blendu";

		private const string BlendVOption = "-blendv";
		private const string BoostOption = "-boost";
		private const string ModifyMapOption = "-mm";
		private const string OffsetOption = "-o";
		private const string ScaleOption = "-s";
		private const string TurbulenceOption = "-t";
		private const string ResolutionOption = "-texres";
		private const string ClampOption = "-clamp";
		private const string BumpOption = "-bm";
		private const string ChannelOption = "-imfchan";
		private const string TypeOption = "-type";

		//! \brief  Default constructor
		internal ObjFileMtlImporter(string[] lines, ref Model model)
		{
			Model = model;
			Line = 0;

			Debug.Assert(Model != null);
			if (Model.DefaultMaterial == null)
			{
				Model.DefaultMaterial = new Material();
				Model.DefaultMaterial.MaterialName = "default";
			}
			Load(lines);
			model = Model;
		}

		/// Load the whole material description
		private void Load(string[] lines)
		{
			foreach (string buffer in lines)
			{
				Buffer = buffer;
				DataIt = 0;
				DataEnd = Buffer.Length;

				if (Buffer.Length == 0)
				{
					continue;
				}

				switch (Buffer[DataIt])
				{
					case 'k':
					case 'K':
						{
							++DataIt;
							if (Buffer[DataIt] == 'a')  // Ambient color
							{
								++DataIt;
								GetColorRGBA(ref Model.CurrentMaterial.Ambient);
							}
							else if (Buffer[DataIt] == 'd')    // Diffuse color
							{
								++DataIt;
								GetColorRGBA(ref Model.CurrentMaterial.Diffuse);
							}
							else if (Buffer[DataIt] == 's')
							{
								++DataIt;
								GetColorRGBA(ref Model.CurrentMaterial.Specular);
							}
							else if (Buffer[DataIt] == 'e')
							{
								++DataIt;
								GetColorRGBA(ref Model.CurrentMaterial.Emissive);
							}
							DataIt = SkipLine(DataIt, DataEnd, ref Line);
						}
						break;
					case 'T':
						{
							++DataIt;
							if (Buffer[DataIt] == 'f')  // Material transmission
							{
								++DataIt;
								GetColorRGBA(ref Model.CurrentMaterial.Transparent);
							}
							DataIt = SkipLine(DataIt, DataEnd, ref Line);
						}
						break;
					case 'd':
						{
							if (Buffer.Length > DataIt + 3 && Buffer.Substring(DataIt + 1, 3) == "isp")
							{
								// A displacement map
								GetTexture();
							}
							else
							{
								// Alpha value
								++DataIt;
								GetFloatValue(out Model.CurrentMaterial.Alpha);
								DataIt = SkipLine(DataIt, DataEnd, ref Line);
							}
						}
						break;
					case 'N':
					case 'n':
						{
							++DataIt;
							switch (Buffer[DataIt])
							{
								case 's':  // Specular exponent
									++DataIt;
									GetFloatValue(out Model.CurrentMaterial.Shineness);
									break;
								case 'i':  // Index Of refraction
									++DataIt;
									GetFloatValue(out Model.CurrentMaterial.Ior);
									break;
								case 'e':  // New material
									CreateMaterial();
									break;
							}
							DataIt = SkipLine(DataIt, DataEnd, ref Line);
						}
						break;
					case 'm':  // Texture
					case 'b':  // quick'n'dirty - for 'bump' sections
					case 'r':  // quick'n'dirty - for 'refl' sections
						{
							GetTexture();
							DataIt = SkipLine(DataIt, DataEnd, ref Line);
						}
						break;
					case 'i':  // Illumination model
						{
							DataIt = GetNextToken(DataIt, DataEnd);
							GetIlluminationModel(out Model.CurrentMaterial.IlluminationModel);
							DataIt = SkipLine(DataIt, DataEnd, ref Line);
						}
						break;
					default:
						DataIt = SkipLine(DataIt, DataEnd, ref Line);
						break;
				}
			}
		}

		//  Loads a color definition
		private void GetColorRGBA(ref Color4 color)
		{
			//Debug.Assert(color != null);

			float r = 0, g = 0, b = 0;
			GetFloat(out r);
			color.R = r;

			// we have to check if color is default 0 with only one token
			if (!IsLineEnd(DataIt))
			{
				/*
				 * HACK:
				 * Using a try/ catch block here so that we don't blow up
				 * if our color is missing a component or ends with whitespace
				 * unexpectedly
				 */
				try
				{
					GetFloat(out g);
					GetFloat(out b);
				}
				catch
				{
				}
			}
			color.G = g;
			color.B = b;
		}

		//  Loads a single float value.
		private void GetFloatValue(out float result)
		{
			string tmp;
			CopyNextWord(out tmp);
			result = float.Parse(tmp);
		}

		//  Gets a texture name from data.
		private void GetTexture()
		{
			int clampIndex = -1;

			if (String.Compare(Buffer, DataIt, DiffuseTexture, 0, DiffuseTexture.Length, true) == 0)
			{
				// Diffuse texture
				clampIndex = (int)Material.TextureType.TextureDiffuseType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.Texture);
			}
			else if (String.Compare(Buffer, DataIt, AmbientTexture, 0, AmbientTexture.Length, true) == 0)
			{
				// Ambient texture
				clampIndex = (int)Material.TextureType.TextureAmbientType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureAmbient);
			}
			else if (String.Compare(Buffer, DataIt, SpecularTexture, 0, SpecularTexture.Length, true) == 0)
			{
				// Specular texture
				clampIndex = (int)Material.TextureType.TextureSpecularType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureSpecular);
			}
			else if (String.Compare(Buffer, DataIt, OpacityTexture, 0, OpacityTexture.Length, true) == 0)
			{
				// Opacity texture
				clampIndex = (int)Material.TextureType.TextureOpacityType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureOpacity);
			}
			else if (String.Compare(Buffer, DataIt, EmissiveTexture1, 0, EmissiveTexture1.Length, true) == 0
			         || String.Compare(Buffer, DataIt, EmissiveTexture2, 0, EmissiveTexture2.Length, true) == 0)
			{
				// Emissive texture
				clampIndex = (int)Material.TextureType.TextureEmissiveType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureEmissive);
			}
			else if (String.Compare(Buffer, DataIt, BumpTexture1, 0, BumpTexture1.Length, true) == 0
			         || String.Compare(Buffer, DataIt, BumpTexture2, 0, BumpTexture2.Length, true) == 0)
			{
				// Bump texture
				clampIndex = (int)Material.TextureType.TextureBumpType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureBump);
			}
			else if (String.Compare(Buffer, DataIt, NormalTexture, 0, NormalTexture.Length, true) == 0)
			{
				// Normal map
				clampIndex = (int)Material.TextureType.TextureNormalType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureNormal);
			}
			else if (String.Compare(Buffer, DataIt, ReflectionTexture, 0, ReflectionTexture.Length, true) == 0)
			{
				// Reflection texture(s)
				// Do nothing here
				return;
			}
			else if (String.Compare(Buffer, DataIt, DisplacementTexture1, 0, DisplacementTexture1.Length, true) == 0
			         || String.Compare(Buffer, DataIt, DisplacementTexture2, 0, DisplacementTexture2.Length, true) == 0)
			{
				// Displacement texture
				clampIndex = (int)Material.TextureType.TextureDispType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureDisp);
			}
			else if (String.Compare(Buffer, DataIt, SpecularityTexture, 0, SpecularityTexture.Length, true) == 0)
			{
				// Specularity scaling (glossiness)
				clampIndex = (int)Material.TextureType.TextureSpecularityType;
				GetTextureOptionAndName(clampIndex, ref Model.CurrentMaterial.TextureSpecularity);
			}
			else
			{
				Debug.WriteLine("OBJ/MTL: Encountered unknown texture type");
				return;
			}
		}

		// /////////////////////////////////////////////////////////////////////////////
		// Texture Option
		// /////////////////////////////////////////////////////////////////////////////
		// According to http://en.wikipedia.org/wiki/Wavefront_.obj_file#Texture_options
		// Texture map statement can contains various texture option, for example:
		//
		//  map_Ka -o 1 1 1 some.png
		//  map_Kd -clamp on some.png
		//
		// So we need to parse and skip these options, and leave the last part which is
		// the url of image, otherwise we will get a wrong url like "-clamp on some.png".
		//
		// Because aiMaterial supports clamp option, so we also want to return it
		// /////////////////////////////////////////////////////////////////////////////
		private void GetTextureOptionAndName(int clampIndex, ref string result)
		{
			bool clamp = false;
			bool already = false;

			DataIt = GetNextToken(DataIt, DataEnd);

			// If there is any more texture option
			while (!IsEndOfBuffer(DataIt, DataEnd) && Buffer[DataIt] == '-')
			{
				//skip option key and value
				int skipToken = 1;

				if (String.Compare(Buffer, DataIt, ClampOption, 0, ClampOption.Length, true) == 0)
				{
					DataIt = GetNextToken(DataIt, DataEnd);
					string tmp;
					CopyNextWord(out tmp);
					if (String.Compare(tmp, 0, "on", 0, 2, true) == 0)
					{
						clamp = true;
					}

					skipToken = 2;
				}
				else if (String.Compare(Buffer, DataIt, TypeOption, 0, TypeOption.Length, true) == 0)
				{
					DataIt = GetNextToken(DataIt, DataEnd);
					string tmp;
					CopyNextWord(out tmp);
					if (String.Compare(tmp, 0, "cube_top", 0, 8, true) == 0)
					{
						clampIndex = (int)Material.TextureType.TextureReflectionCubeTopType;
						GetTextureName(ref Model.CurrentMaterial.TextureReflection[0]);
						already = true;
					}
					else if (String.Compare(tmp, 0, "cube_bottom", 0, 11, true) == 0)
					{
						clampIndex = (int)Material.TextureType.TextureReflectionCubeBottomType;
						GetTextureName(ref Model.CurrentMaterial.TextureReflection[1]);
						already = true;
					}
					else if (String.Compare(tmp, 0, "cube_front", 0, 10, true) == 0)
					{
						clampIndex = (int)Material.TextureType.TextureReflectionCubeFrontType;
						GetTextureName(ref Model.CurrentMaterial.TextureReflection[2]);
						already = true;
					}
					else if (String.Compare(tmp, 0, "cube_back", 0, 9, true) == 0)
					{
						clampIndex = (int)Material.TextureType.TextureReflectionCubeBackType;
						GetTextureName(ref Model.CurrentMaterial.TextureReflection[3]);
						already = true;
					}
					else if (String.Compare(tmp, 0, "cube_left", 0, 9, true) == 0)
					{
						clampIndex = (int)Material.TextureType.TextureReflectionCubeLeftType;
						GetTextureName(ref Model.CurrentMaterial.TextureReflection[4]);
						already = true;
					}
					else if (String.Compare(tmp, 0, "cube_right", 0, 10, true) == 0)
					{
						clampIndex = (int)Material.TextureType.TextureReflectionCubeRightType;
						GetTextureName(ref Model.CurrentMaterial.TextureReflection[5]);
						already = true;
					}
					else if (String.Compare(tmp, 0, "sphere", 0, 6, true) == 0)
					{
						clampIndex = (int)Material.TextureType.TextureReflectionSphereType;
						GetTextureName(ref Model.CurrentMaterial.TextureReflection[0]);
						already = true;
					}

					skipToken = 2;
				}
				else if (String.Compare(Buffer, DataIt, BlendUOption, 0, BlendUOption.Length, true) == 0
				         || String.Compare(Buffer, DataIt, BlendVOption, 0, BlendVOption.Length, true) == 0
				         || String.Compare(Buffer, DataIt, BoostOption, 0, BoostOption.Length, true) == 0
				         || String.Compare(Buffer, DataIt, ResolutionOption, 0, ResolutionOption.Length, true) == 0
				         || String.Compare(Buffer, DataIt, BumpOption, 0, BumpOption.Length, true) == 0
				         || String.Compare(Buffer, DataIt, ChannelOption, 0, ChannelOption.Length, true) == 0)
				{
					skipToken = 2;
				}
				else if (String.Compare(Buffer, DataIt, ModifyMapOption, 0, ModifyMapOption.Length, true) == 0)
				{
					skipToken = 3;
				}
				else if (String.Compare(Buffer, DataIt, OffsetOption, 0, OffsetOption.Length, true) == 0
				         || String.Compare(Buffer, DataIt, ScaleOption, 0, ScaleOption.Length, true) == 0
				         || String.Compare(Buffer, DataIt, TurbulenceOption, 0, TurbulenceOption.Length, true) == 0)
				{
					skipToken = 4;
				}

				for (int i = 0; i < skipToken; ++i)
				{
					DataIt = GetNextToken(DataIt, DataEnd);
				}
			}

			Model.CurrentMaterial.Clamp[clampIndex] = clamp;

			if (!already)
			{
				GetTextureName(ref result);
			}
		}

		private void GetTextureName(ref string result)
		{
			string texture;
			DataIt = GetName(DataIt, DataEnd, out texture);
			if (result == null)
			{
				result = texture;
			}
		}

		//  Creates a material from loaded data.
		private void CreateMaterial()
		{
			List<string> token;
			int numToken = Tokenize(Buffer, out token, " \t");
			string name;
			if (numToken <= 1)
			{
				name = Material.AI_DEFAULT_MATERIAL_NAME;
			}
			else
			{
				// skip newmtl and all following white spaces
				name = token[1];
			}
			name = name.Trim();

			Material tmp;
			if (!Model.MaterialMap.TryGetValue(name, out tmp))
			{
				// New Material created
				Model.CurrentMaterial = new Material();
				Model.CurrentMaterial.MaterialName = name;
				if (Model.CurrentMesh != null)
				{
					Model.CurrentMesh.MaterialIndex = (uint)Model.MaterialLib.Count - 1;
				}
				Model.MaterialLib.Add(name);
				Model.MaterialMap[name] = Model.CurrentMaterial;
			}
			else
			{
				// Use older material
				Model.CurrentMaterial = tmp;
			}
		}

		//  Loads the kind of illumination model.
		private void GetIlluminationModel(out int illumModel)
		{
			string tmp;
			CopyNextWord(out tmp);
			illumModel = int.Parse(tmp);
		}
	}
}
