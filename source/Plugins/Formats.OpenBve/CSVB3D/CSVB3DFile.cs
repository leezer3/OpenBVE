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
using OpenBveApi.Hosts;
using OpenBveApi.Input;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Formats.OpenBve
{
	public abstract class CSVB3DBlock<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public CSVB3DBlock(string myFile, HostInterface host)
		{
			Key = default;
			FileName = myFile;
			currentHost = host;
		}

		public CSVB3DBlock(T1 myKey, string myFile, HostInterface host)
		{
			Key = myKey;
			FileName = myFile;
			currentHost = host;
		}

		public readonly T1 Key;

		public readonly string FileName;

		public int CurrentLine;

		internal readonly HostInterface currentHost;

		public virtual int RemainingDataValues => 0;

		protected Queue<CSVB3DBlock<T1, T2>> subBlocks = new Queue<CSVB3DBlock<T1, T2>>();

		public int RemainingSubBlocks => subBlocks.Count;

		public virtual CSVB3DBlock<T1, T2> ReadNextBlock()
		{
			return subBlocks.Dequeue();
		}

		/// <summary>Gets the next enum value from the block</summary>
		/// <typeparam name="T3">The type of the enum</typeparam>
		public virtual bool GetNextEnumValue<T3>(out T3 value) where T3 : struct, Enum
		{
			value = default;
			return false;
		}

		/// <summary>Gets the next path from the block</summary>
		/// <param name="absolutePath"></param>
		/// <param name="finalPath"></param>
		/// <returns></returns>
		public virtual bool GetNextPath(string absolutePath, out string finalPath)
		{
			finalPath = absolutePath;
			return false;
		}


		/// <summary>Gets the type of the next value</summary>
		public virtual T2 NextValue
		{
			get => default;
		}

		/// <summary>Gets the next vertex from the block</summary>
		public virtual Vertex GetNextVertex(out Vector3 currentNormal)
		{
			currentNormal = Vector3.Zero;
			return new Vertex();
		}

		/// <summary>Gets the next int array from the block</summary>
		public virtual bool TryGetNextVertexIndexArray(out int[] values, bool enableHacks)
		{
			values = Array.Empty<int>();
			return false;
		}

		/// <summary>Gets the next double array from the block</summary>
		public virtual bool TryGetNextDoubleArray(out double[] values, int neededValues = int.MaxValue)
		{
			values = Array.Empty<double>();
			return false;
		}

		/// <summary>Gets the next Color32 from the block</summary>
		public virtual bool GetNextColor32(out Color32 c)
		{
			c = Color32.White;
			return false;
		}

		/// <summary>Gets the next paired texture paths</summary>
		public virtual bool GetNextTexturePath(string absolutePath, out string tDay, out string tNight)
		{
			tDay = string.Empty;
			tNight = string.Empty;
			return false;
		}

		/// <summary>Gets the next set of texture coordinates from the block</summary>
		public virtual bool GetNextTextureCoordinates(out int index, out Vector2 coordinates, out Vector2 lightMapCoordinates)
		{
			index = -1;
			coordinates = Vector2.Null;
			lightMapCoordinates = Vector2.Null;
			return false;
		}

		public virtual bool GetBlendMode<T3, T4>(out T3 enumValue, out double glowHalfDistance, out T4 glowAttenuationMode)
		{
			enumValue = default;
			glowHalfDistance = 0;
			glowAttenuationMode = default;
			return false;
		}

		/// <summary>Gets the next shear mapping from the block</summary>
		public virtual bool GetNextShear(out Vector3 shearDirection, out Vector3 shearStress, out double shearRatio)
		{
			shearDirection = Vector3.Zero;
			shearStress = Vector3.Zero;
			shearRatio = 0;
			return false;
		}

		/// <summary>Gets the next mirror mapping from the block</summary>
		public virtual bool GetNextMirror(out Vector3 mirrorVertices, out Vector3 mirrorNormals)
		{
			mirrorVertices = Vector3.Zero;
			mirrorNormals = Vector3.Zero;
			return false;
		}

		/// <summary>Gets the next Vector2 from the block</summary>
		public virtual bool GetNextVector2(out Vector2 vector)
		{
			vector = Vector2.Null;
			return false;
		}

		/// <summary>Gets the next Vector3 from the block</summary>
		public virtual bool GetNextVector3(out Vector3 vector)
		{
			vector = Vector3.Zero;
			return false;
		}

		public virtual bool GetNextRawValue(out string text)
		{
			text = string.Empty;
			return false;
		}

		/// <summary>Skips the next value</summary>
		public virtual void SkipNextValue()
		{

		}
	}
	public class CSVB3DFile <T1, T2> : CSVB3DBlock<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		public CSVB3DFile(string myFile, HostInterface currentHost) : base(myFile, currentHost)
		{
			List<string> Lines = File.ReadAllLines(myFile).ToList();

			CSVB3DFileSection<T1, T2> currentSection = null;

			bool IsB3D = string.Equals(Path.GetExtension(myFile), ".b3d", StringComparison.OrdinalIgnoreCase);

			for (int i = 0; i < Lines.Count; i++)
			{
				// as we're splitting on whitespace (B3D) perform trim first to get rid of any leading whitespace
				int semiColon = Lines[i].IndexOf(';');
				if (semiColon != -1)
				{
					Lines[i] = Lines[i].Substring(0, semiColon);
				}
				Lines[i] = Lines[i].Trim();

				string[] splitLine;
				splitLine = IsB3D ? Lines[i].Split(Array.Empty<char>(), 2) : Lines[i].Split(',');
				
				if (!IsB3D)
				{
					if (splitLine.Length > 4)
					{
						for (int j = 4; j < splitLine.Length; j++)
						{
							if (double.TryParse(splitLine[j], out _))
							{
								/*
								 * Altered approach slightly:
								 * It turns out that using Enum.TryParse and IsDefined is terminally slow (~13s to ~1m30s)
								 * Fast-skip this wherever possible.
								 * 
								 * This makes two key assumptions-
								 * 1. The first part must contain 4 members as a minimum (e.g. AddVertex,X,Y,Z)
								 * 2. If our split part parses to a valid number, it can't be an enum member
								 * 
								 * Multi-column objects are however vanishingly rare (I've seen only about two instances in the wild)
								 * 
								 * It may be better to hide this behind a dedicated detection and entry in the compatibility database
								 */

								continue;
							}
							if (Enum.TryParse(splitLine[j], true, out CSVB3DSection _) || Enum.TryParse(splitLine[j], out CSVB3DKey _))
							{
								// add multi-columns to the end of our line list (to be parsed)
								Lines.Add(string.Join(",", splitLine.Skip(j)));
								// dump them
								Array.Resize(ref splitLine, j);
								break;
							}
						}
					}
					
				}

				splitLine[0] = splitLine[0].Trim();
				if (IsB3D)
				{
					// B3D bracket enclosed section names
					splitLine[0] = splitLine[0].TrimStart('[').TrimEnd(']').Trim();
				}

				if(Enum.TryParse(splitLine[0], true, out T1 section))
				{
					if (currentSection != null)
					{
						subBlocks.Enqueue(currentSection);
						
					}
					currentSection = new CSVB3DFileSection<T1, T2>(section, myFile, currentHost);
				}

				if (Enum.TryParse(splitLine[0], true, out T2 key))
				{
					// Unfortunately, can't cast to generic directly- we've got to go via object
					if (currentSection == null)
					{
						if ((CSVB3DKey)(object)key == CSVB3DKey.AddVertex || (CSVB3DKey)(object)key == CSVB3DKey.Cylinder || (CSVB3DKey)(object)key == CSVB3DKey.Cube)
						{
							// https://github.com/leezer3/OpenBVE/issues/448
							// Further testing reveals that this is an OpenBVE specific issue (BVE2 / BVE4 just crash in this case), but that objects are in the wild with this bug...
							currentHost.AddMessage(MessageType.Warning, false, "Attempted to add a vertex without first creating a MeshBuilder - This may produce unexpected results - at line " + (i + 1).ToString(CultureInfo.InvariantCulture) + " in file " + FileName);
						}
						currentSection = new CSVB3DFileSection<T1, T2>((T1)(object)CSVB3DSection.CreateMeshBuilder, myFile, currentHost);
					}
					else
					{
						if ((CSVB3DKey)(object)key == CSVB3DKey.Load || (CSVB3DKey)(object)key == CSVB3DKey.LoadLightMap && (CSVB3DSection)(object)currentSection.Key != CSVB3DSection.LoadTexture)
						{
							subBlocks.Enqueue(currentSection);
							currentSection = new CSVB3DFileSection<T1, T2>((T1)(object)CSVB3DSection.LoadTexture, myFile, currentHost);
						}
					}

					if (IsB3D)
					{
						if (splitLine.Length >= 2)
						{
							splitLine = splitLine[1].Split(',');
						}
						else
						{
							splitLine = Array.Empty<string>();
						}
					}
					else
					{
						splitLine = splitLine.Skip(1).ToArray();
					}

					currentSection.Values.Enqueue(new KeyValuePair<int, KeyValuePair<T2, string[]>>(i, new KeyValuePair<T2, string[]>(key, splitLine)));
				}
			}

			if (currentSection != null && currentSection.RemainingDataValues > 0)
			{
				subBlocks.Enqueue(currentSection);
			}
		}
	}

	public class CSVB3DFileSection<T1, T2> : CSVB3DBlock<T1, T2> where T1 : struct, Enum where T2 : struct, Enum
	{
		internal Queue<KeyValuePair<int, KeyValuePair<T2, string[]>>> Values = new Queue<KeyValuePair<int, KeyValuePair<T2, string[]>>>();

		private KeyValuePair<T2, string[]> Dequeue()
		{
			KeyValuePair<int, KeyValuePair<T2, string[]>> value = Values.Dequeue();
			CurrentLine = value.Key;
			return value.Value;
		}

		public CSVB3DFileSection(T1 myKey, string myFile, HostInterface currentHost) : base(myKey, myFile, currentHost)
		{
		}

		public override int RemainingDataValues => Values.Count;

		public override T2 NextValue
		{
			get => Values.Peek().Value.Key;
		}

		public override Vertex GetNextVertex(out Vector3 currentNormal)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			Vertex currentVertex = new Vertex();
			if (value.Value.Length >= 1 && value.Value[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(value.Value[0], out currentVertex.Coordinates.X))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument vX in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				currentVertex.Coordinates.X = 0.0;
			}
			if (value.Value.Length >= 2 && value.Value[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(value.Value[1], out currentVertex.Coordinates.Y))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument vY in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				currentVertex.Coordinates.Y = 0.0;
			}
			if (value.Value.Length >= 3 && value.Value[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(value.Value[2], out currentVertex.Coordinates.Z))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument vZ in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				currentVertex.Coordinates.Z = 0.0;
			}
			currentNormal = new Vector3();
			if (value.Value.Length >= 4 && value.Value[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(value.Value[3], out currentNormal.X))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument nX in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				currentNormal.X = 0.0;
			}
			if (value.Value.Length >= 5 && value.Value[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(value.Value[4], out currentNormal.Y))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument nY in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				currentNormal.Y = 0.0;
			}
			if (value.Value.Length >= 6 && value.Value[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(value.Value[5], out currentNormal.Z))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument nZ in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				currentNormal.Z = 0.0;
			}
			currentNormal.Normalize();
			return currentVertex;
		}

		public override bool TryGetNextVertexIndexArray(out int[] parsedValues, bool enableHacks)
		{
			KeyValuePair<T2, string[]> value = Dequeue();

			parsedValues = new int[value.Value.Length];
			for (int i = 0; i < value.Value.Length; i++)
			{
				/*
				 * NOTE: Face ,1,2,3
				 * is interpreted by BVE as Face 0,1,2,3
				 * Applies to both CSV and B3D files
				 */
				if (!NumberFormats.TryParseIntVb6(value.Value[i], out parsedValues[i]) && (i != 0 || enableHacks == false))
				{
					if (!string.IsNullOrEmpty(value.Value[i]))
					{
						currentHost.AddMessage(MessageType.Error, false, "The vertex referenced at index " + i + " is not a valid integer in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
					}
					Array.Resize(ref parsedValues, i);
					break;
				}

				if (parsedValues[i] > 65535)
				{
					currentHost.AddMessage(MessageType.Error, false, "A vertex with an index above 65535 is not currently supported in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
					return false;
				}
			}

			if (parsedValues.Length < 3)
			{
				// insufficient vertices to make a face
				currentHost.AddMessage(MessageType.Error, false, value.Key + " contains an insufficient number of arguments at line " + CurrentLine + " in file " + FileName);
				return false;
			}

			return true;
		}

		public override bool TryGetNextDoubleArray(out double[] parsedValues, int neededValues = int.MaxValue)
		{
			KeyValuePair<T2, string[]> value = Dequeue();

			parsedValues = new double[value.Value.Length];
			for (int i = 0; i < Math.Min(value.Value.Length, neededValues); i++)
			{
				if (!NumberFormats.TryParseDoubleVb6(value.Value[i], out parsedValues[i]) && !string.IsNullOrEmpty(value.Value[i]))
				{
					currentHost.AddMessage(MessageType.Error, false, "The value at array index " + i + " is not a valid double in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
					Array.Resize(ref parsedValues, i);
					break;
				}
			}
			return parsedValues.Length != 0;
		}

		public override bool GetNextColor32(out Color32 c)
		{
			KeyValuePair<T2, string[]> value = Dequeue();

			// Supports 3 args (RGB, alpha=255) or 4 args (RGBA) for backward compatibility
			int r = 0, g = 0, b = 0, a = 255;
			if (value.Value.Length == 0)
			{
				c = Color32.White;
				return false;
			}

			if (value.Value.Length >= 1 && value.Value[0].Length > 0 && !NumberFormats.TryParseByteVb6(value.Value[0], out r))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid value for Red in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
			}
			if (value.Value.Length >= 2 && value.Value[1].Length > 0 && !NumberFormats.TryParseByteVb6(value.Value[1], out g))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid value for Green in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
			}
			if (value.Value.Length >= 3 && value.Value[2].Length > 0 && !NumberFormats.TryParseByteVb6(value.Value[2], out b))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid value for Blue in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
			}
			if (value.Value.Length >= 4 && value.Value[3].Length > 0 && !NumberFormats.TryParseByteVb6(value.Value[3], out a))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid value for Alpha in " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				a = 255; // special-case
			}
			c = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
			return true;
		}

		public override bool GetNextTexturePath(string absolutePath, out string tDay, out string tNight)
		{
			KeyValuePair<T2, string[]> value = Dequeue();

			tDay = value.Value.Length >= 1 && !string.IsNullOrEmpty(value.Value[0]) ? OpenBveApi.Path.CombineFile(absolutePath, value.Value[0].Trim()) : null;
			tNight = value.Value.Length >= 2 && !string.IsNullOrEmpty(value.Value[1]) ? OpenBveApi.Path.CombineFile(absolutePath, value.Value[1].Trim()) : null;

			bool textureFound = false;
			
			if(File.Exists(tDay))
			{
				// daytime texture required, nighttime optional
				textureFound = true;
			}
			else
			{
				tDay = value.Value.Length >= 1 ? value.Value[0] : string.Empty;
				currentHost.AddMessage(MessageType.Error, false, "DaytimeTexture File + " + tDay + " was not found for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
			}

			if (!string.IsNullOrEmpty(tNight) && !File.Exists(tNight))
			{
				tNight = value.Value.Length >= 2 ? value.Value[1] : string.Empty;
				currentHost.AddMessage(MessageType.Error, false, "NighttimeTexture File " + tNight + " was not found for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
			}

			// TODO: Add the BVE2 signal hacks stuff back in
			return textureFound;
		}

		public override bool GetNextPath(string absolutePath, out string finalPath)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			finalPath = value.Value.Length >= 1 && !string.IsNullOrEmpty(value.Value[0]) ? OpenBveApi.Path.CombineFile(absolutePath, value.Value[0]) : string.Empty;
			if (File.Exists(finalPath))
			{
				return true;
			}

			currentHost.AddMessage(MessageType.Error, false, "Texture File was not found for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
			return false;
		}

		public override bool GetNextTextureCoordinates(out int index, out Vector2 coordinates, out Vector2 lightMapCoordinates)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			index = -1;
			coordinates = Vector2.Null;
			lightMapCoordinates = Vector2.Null;
			if (value.Value.Length >= 1 && value.Value[0].Length > 0 && !NumberFormats.TryParseIntVb6(value.Value[0], out index))
			{
				currentHost.AddMessage(MessageType.Error, false, "Vertex index was invalid for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				return false;
			}

			if (index == -1)
			{
				return false;
			}

			coordinates = GetVector2(value.Value, 1, (T2)(object)CSVB3DKey.Coordinates, CurrentLine);
			lightMapCoordinates = GetVector2(value.Value, 3, (T2)(object)CSVB3DKey.LightMapCoordinates, CurrentLine);
			return true;
		}

		public override bool GetBlendMode<T3, T4>(out T3 enumValue, out double glowHalfDistance, out T4 glowAttenuationMode)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			enumValue = (T3)(object)MeshMaterialBlendMode.Normal;
			glowHalfDistance = 0;
			glowAttenuationMode = (T4)(object)GlowAttenuationMode.DivisionExponent4;
			
			if (value.Value.Length >= 2)
			{
				if (Enum.TryParse(value.Value[0], out MeshMaterialBlendMode mode))
				{
					enumValue = (T3)(object)mode;
				}
				else
				{
					currentHost.AddMessage(MessageType.Error, false, "BlendMode was invalid for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
					return false;
				}
			}
			
			
			if(value.Value.Length >= 2)
			{
				double.TryParse(value.Value[1], out glowHalfDistance);
			}

			if (value.Value.Length >= 3)
			{
				if (Enum.TryParse(value.Value[2], out GlowAttenuationMode glowMode))
				{
					glowAttenuationMode = (T4)(object)glowMode;
				}
				else
				{
					currentHost.AddMessage(MessageType.Error, false, "GlowAttenuationMode was invalid for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				}
			}
			return true;
		}

		public override bool GetNextEnumValue<T3>(out T3 enumValue)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			enumValue = default;

			if (value.Value.Length >= 1 && !Enum.TryParse(value.Value[0], out enumValue))
			{
				currentHost.AddMessage(MessageType.Error, false, "Value was invalid for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				return false;
			}

			return true;
		}

		public override bool GetNextVector2(out Vector2 vector)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			vector = GetVector2(value.Value, 1, value.Key, CurrentLine);
			return true;
		}

		private Vector2 GetVector2(string[] values, int startingIndex, T2 key, int line)
		{
			Vector2 v = Vector2.Null;
			if (values.Length >= startingIndex + 1 && values[startingIndex].Length > 0 && !NumberFormats.TryParseDoubleVb6(values[startingIndex], out v.X))
			{
				currentHost.AddMessage(MessageType.Error, false, "X was invalid for " + key + " at line " + line + " in file " + FileName);
			}

			if (values.Length >= startingIndex + 2 && values[startingIndex + 1].Length > 0 && !NumberFormats.TryParseDoubleVb6(values[startingIndex + 1], out v.Y))
			{
				currentHost.AddMessage(MessageType.Error, false, "Y was invalid for " + key + " at line " + line + " in file " + FileName);
			}

			return v;
		}

		public override bool GetNextVector3(out Vector3 vector)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			if (value.Value.Length == 0)
			{
				currentHost.AddMessage(MessageType.Warning, false, "No arguments were supplied for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				vector = Vector3.Zero;
				return false;
			}
			vector = GetVector3(value.Value, 0, value.Key, CurrentLine);
			return true;
		}

		private Vector3 GetVector3(string[] values, int startingIndex, T2 key, int line)
		{
			Vector3 v = Vector3.Zero;
			if (values.Length >= startingIndex + 1 && values[startingIndex].Length > 0 && !NumberFormats.TryParseDoubleVb6(values[startingIndex], out v.X))
			{
				currentHost.AddMessage(MessageType.Error, false, "X was invalid for " + key + " at line " + line + " in file " + FileName);
			}

			if (values.Length >= startingIndex + 2 && values[startingIndex + 1].Length > 0 && !NumberFormats.TryParseDoubleVb6(values[startingIndex + 1], out v.Y))
			{
				currentHost.AddMessage(MessageType.Error, false, "Y was invalid for " + key + " at line " + line + " in file " + FileName);
			}

			if (values.Length >= startingIndex + 3 && values[startingIndex + 2].Length > 0 && !NumberFormats.TryParseDoubleVb6(values[startingIndex + 2], out v.Z))
			{
				currentHost.AddMessage(MessageType.Error, false, "Y was invalid for " + key + " at line " + line + " in file " + FileName);
			}

			return v;
		}

		public override bool GetNextShear(out Vector3 shearDirection, out Vector3 shearStress, out double shearRatio)
		{
			KeyValuePair<T2, string[]> value = Dequeue();

			shearRatio = 0;
			shearDirection = GetVector3(value.Value, 0, (T2)(object)CSVB3DKey.ShearDirection, CurrentLine);
			shearStress = GetVector3(value.Value, 3, (T2)(object)CSVB3DKey.ShearStress, CurrentLine);

			if (value.Value.Length >= 7 && !NumberFormats.TryParseDoubleVb6(value.Value[6], out shearRatio))
			{
				currentHost.AddMessage(MessageType.Error, false, "ShearRatio was invalid for " + value.Key + " at line " + CurrentLine + " in file " + FileName);
				shearRatio = 0;
			}
			
			return shearRatio != 0;
		}

		public override bool GetNextMirror(out Vector3 mirrorVertices, out Vector3 mirrorNormals)
		{
			KeyValuePair<T2, string[]> value = Dequeue();

			mirrorVertices = GetVector3(value.Value, 0, (T2)(object)CSVB3DKey.MirrorVertices, CurrentLine);
			mirrorNormals = value.Value.Length >= 4 ? GetVector3(value.Value, 3, (T2)(object)CSVB3DKey.MirrorNormals, CurrentLine) : mirrorVertices;

			return mirrorVertices != Vector3.Zero || mirrorNormals != Vector3.Zero;
		}

		public override bool GetNextRawValue(out string text)
		{
			KeyValuePair<T2, string[]> value = Dequeue();
			text = string.Join(",", value.Value); 
			return true;
		}

		public override void SkipNextValue()
		{
			Values.Dequeue();
		}
	}
}
