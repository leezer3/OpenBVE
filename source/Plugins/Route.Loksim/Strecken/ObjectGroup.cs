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
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.World;
using System;
using System.Collections.Generic;
using System.Text;
using Formats.OpenBve;

namespace LokSimRouteParser
{
	internal class ObjectGroup
	{
		/// <summary>The position of this on the rail</summary>
		internal double TrackPosition;
		/// <summary>The offset from the position</summary>
		internal Vector3 OffsetPosition;

		internal string Name;

		internal List<Object> Objects;
		internal ObjectGroup(Block<LoksimNode, LoksimAttribute> objectGroupBlock)
		{
			// An ObjectGroup [Object in the XML] is somewhere roughly analogous to a world tile
			Objects = new List<Object>();
			objectGroupBlock.GetValue(LoksimAttribute.Position, out TrackPosition);
			// We get an initial position to looking ahead on the transformed track
			// Then add an offset to get our final WPos
			objectGroupBlock.GetVector3(LoksimAttribute.Offset, ';', out OffsetPosition);

			/* Qualitaet,
			 * ---------
			 * Used by the renderer to determine visibility
			 * Our renderer can probably ignore this
			 * -----------------------------------------------
			 * 0 - Absolutely necessary (e.g. signals, tracks)
			 * 1 - Very important (e.g. railway embankments)
			 * 2 - Important (e.g. tunnels, bridges)
			 * 3 - Normal object (e.g houses)
			 * 4 - Unimportant object (e.g. trees)
			 * 5 - Completely unimportant (e.g. overpass- presumably road traffic)
			 */
			// RollMaterial : set to true if this ObjectGroup is another train
			// DynamicVisibility
			// FixedDynamicVisibility
			/* StreckenAbhaengigkeit
			 * ------------------
			 * 0 - Display independent of the train (default)
			 * 1 - Displayed only if no train is on this track
			 * 2 - Displayed only if train is on this track
			 */

			while (objectGroupBlock.RemainingSubBlocks > 0)
			{
				Block<LoksimNode, LoksimAttribute> subBlock = objectGroupBlock.ReadNextBlock();
				switch (subBlock.Key)
				{
					case LoksimNode.Eintrag:
						Objects.Add(new Object(subBlock));
						break;
				}
			}
		}

		internal void Create(TrackFollower t)
		{
			
			for (int i = 0; i < Objects.Count; i++)
			{
				// reset the track follower position, as we're using it for repetitions
				t.UpdateAbsolute(TrackPosition, true, false);
				Objects[i].Create(t, OffsetPosition);
			}
		}
	}

	internal class Object
	{
		/// <summary>When repetition is used, distance to next object</summary>
		internal double Distance;
		/// <summary>Number of copies of object to place</summary>
		internal int NumberOfObjects;
		internal bool FollowsTrackHeight;
		internal bool FollowsLandHeight;
		internal UnifiedObject UnifiedObject;
		/// <summary>Offset vector of the object from its rail position</summary>
		internal Vector3 Offset;
		/// <summary>Z-position of the object on the rail</summary>
		internal double Position;
		/// <summary>Rotation in degrees for each axis</summary>
		internal Vector3 Rotation;
		internal bool FarVisible;
		internal LightStates Lighting;
		internal Dictionary<LoksimAttribute, string> Properties;
		internal LoksimRandom Random;
		

		internal Object(Block<LoksimNode, LoksimAttribute> objectBlock)
		{
			objectBlock.GetValue(LoksimAttribute.Abstand, out Distance);
			objectBlock.GetValue(LoksimAttribute.Anzahl, out NumberOfObjects);
			if (!Plugin.previewOnly && objectBlock.GetPath(LoksimAttribute.Datei, Plugin.FileSystem.LoksimDataDirectory, out string objectPath))
			{
				if (!Plugin.ObjectCache.TryGetValue(objectPath, out UnifiedObject))
				{
					if (System.IO.File.Exists(objectPath))
					{
						Plugin.CurrentHost.LoadObject(objectPath, Encoding.UTF8, out UnifiedObject);
						Plugin.ObjectCache.Add(objectPath, UnifiedObject);
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, true, "LokSim3D: Object " + objectPath.Replace(Plugin.FileSystem.LoksimDataDirectory, string.Empty) + " was not found.");
					}
				}
			}

			objectBlock.GetValue(LoksimAttribute.GleisHoeheFolgen, out FollowsTrackHeight);
			objectBlock.GetValue(LoksimAttribute.HoeheLand, out FollowsLandHeight);
			objectBlock.GetValue(LoksimAttribute.Position, out Position);
			objectBlock.GetVector3(LoksimAttribute.Verschiebung, ';', out Offset);
			objectBlock.GetValue(LoksimAttribute.WeitSichtbar, out FarVisible);
			objectBlock.GetVector3(LoksimAttribute.Winkel, ';', out Rotation);

			if (objectBlock.ReadBlock(LoksimNode.Eigenschaften, out Block<LoksimNode, LoksimAttribute> eigenschaftenBlock))
			{
				Properties = new Dictionary<LoksimAttribute, string>();
				while (eigenschaftenBlock.RemainingDataValues > 0)
				{
					eigenschaftenBlock.GetNextValue(out LoksimAttribute valueType, out string value);
					if (valueType != LoksimAttribute.Unknown && !Properties.ContainsKey(valueType))
					{
						Properties.Add(valueType, value);
					}
				}
			}

			if (objectBlock.ReadBlock(LoksimNode.Lightning, out Block<LoksimNode, LoksimAttribute> lightingBlock))
			{
				Lighting = new LightStates(lightingBlock);
			}

			if (objectBlock.ReadBlock(LoksimNode.Random, out Block<LoksimNode, LoksimAttribute> randomBlock))
			{
				Random = new LoksimRandom(randomBlock);
			}
		}

		internal void Create(TrackFollower tf, Vector3 offsetPosition)
		{
			if (UnifiedObject != null)
			{
				Vector3 o = new Vector3(Offset);
				o.Z += Position;
				o += offsetPosition;

				if (NumberOfObjects == 1)
				{
					
					if (Random != null)
					{
						Vector3 v = Random.GetPosition();
						o += v;
					}
					o.Rotate(tf.WorldDirection, tf.WorldUp, tf.WorldSide);
					Vector3 objectWpos = tf.WorldPosition + o;
					Transformation tr = new Transformation(tf.WorldDirection, tf.WorldUp, tf.WorldSide);
					UnifiedObject.CreateObject(objectWpos, tr, new Transformation(Rotation.X.ToRadians(), Rotation.Y.ToRadians(), Rotation.Z.ToRadians()), new ObjectCreationParameters(tf.TrackPosition, tf.TrackPosition, tf.TrackPosition + 500));
				}
				else
				{
					Vector3 lastWpos = tf.WorldPosition;
					for (int i = 0; i < NumberOfObjects; i++)
					{

						Vector3 objectWpos;
						Vector3 repetitionPosition = Vector3.Zero;
						if (i != 0)
						{
							tf.UpdateRelative(Distance, true, false);
						}


						Vector3 d = tf.WorldPosition == lastWpos ? tf.WorldPosition : new Vector3(tf.WorldPosition - lastWpos);
						double t = d.Magnitude();
						d *= t;
						t = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
						double ex = d.X * t;
						double ez = d.Z * t;
						Vector3 s = new Vector3(ez, 0.0, -ex);
						Vector3 u = Vector3.Cross(d, s);


						Vector3 oC = new Vector3(o);
						oC.Rotate(tf.WorldDirection, tf.WorldUp, tf.WorldSide);
						objectWpos = tf.WorldPosition + oC;

						if (Random != null)
						{
							repetitionPosition += Random.GetPosition();
						}
						repetitionPosition.Rotate(d,u,s);
						Transformation tr = new Transformation(d,u,s);
						UnifiedObject.CreateObject(objectWpos + repetitionPosition, tr, new Transformation(Rotation.X.ToRadians(), Rotation.Y.ToRadians(), Rotation.Z.ToRadians()), new ObjectCreationParameters(tf.TrackPosition, tf.TrackPosition, tf.TrackPosition + 500));


						lastWpos = tf.WorldPosition;
					}


				}
					
			}
		}
	}

	internal class LoksimRandom
	{
		/// <summary>The random seed</summary>
		internal readonly int Seed;

		internal readonly double XValue;

		internal readonly double YValue;

		internal readonly double ZValue;

		internal readonly double XRotation;

		internal readonly double YRotation;

		internal readonly double ZRotation;


		private readonly Random random;

		internal LoksimRandom(Block<LoksimNode, LoksimAttribute> randomBlock)
		{
			randomBlock.GetValue(LoksimAttribute.SRAND, out Seed);
			randomBlock.GetValue(LoksimAttribute.XValue, out XValue);
			randomBlock.GetValue(LoksimAttribute.YValue, out YValue);
			randomBlock.GetValue(LoksimAttribute.ZValue, out ZValue);
			random = new Random(Seed);

		}

		internal Vector3 GetPosition()
		{
			double randomResult = random.NextDouble();
			// random result returns zero plus / minus the value
			return new Vector3(XValue - randomResult * XValue * 2, YValue - randomResult * YValue * 2, ZValue - randomResult * ZValue * 2);
		}


	}

	internal class LightStates
	{
		internal double Day;
		internal double Night;
		internal bool TrainLit;
		internal LightStates(Block<LoksimNode, LoksimAttribute> lightingBlock)
		{
			// Going to be roughly analogous to the BVE DNB values
			lightingBlock.GetValue(LoksimAttribute.DayLight, out Day);
			lightingBlock.GetValue(LoksimAttribute.NightLight, out Night);
			lightingBlock.GetValue(LoksimAttribute.LokLight, out TrainLit);
		}
	}
}
