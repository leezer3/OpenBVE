using System;
using System.Linq;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>The AnimatedObjectCollection is a simple container class containing one or more animated objects</summary>
		public class AnimatedObjectCollection : UnifiedObject
		{

			private readonly HostInterface currentHost;
			/// <summary>The objects that this collection contains</summary>
			public AnimatedObject[] Objects;
			/// <summary>The sounds that this collection contains</summary>
			public WorldObject[] Sounds;

			/// <summary>Creates a new AnimatedObjectCollection</summary>
			public AnimatedObjectCollection(HostInterface Host)
			{
				currentHost = Host;
			}

			/// <inheritdoc/>
			public override void CreateObject(Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation,
				int SectionIndex, double StartingDistance, double EndingDistance,
				double TrackPosition, double Brightness, bool DuplicateMaterials = false)
			{
				bool[] free = new bool[Objects.Length];
				bool anyfree = false;
				bool allfree = true;
				for (int i = 0; i < Objects.Length; i++)
				{
					free[i] = Objects[i].IsFreeOfFunctions();
					if (free[i])
					{
						anyfree = true;
					}
					else
					{
						allfree = false;
					}
				}
				if (anyfree && !allfree && Objects.Length > 1)
				{
					//Optimise a little: If *all* are free of functions, this can safely be converted into a static object without regard to below
					if (LocalTransformation.X != Vector3.Right || LocalTransformation.Y != Vector3.Down || LocalTransformation.Z != Vector3.Forward)
					{
						//HACK:
						//An animated object containing a mix of functions and non-functions and using yaw, pitch or roll must not be converted into a mix
						//of animated and static objects, as this causes rounding differences....
						anyfree = false;
					}
				}
				if (anyfree)
				{
					for (int i = 0; i < Objects.Length; i++)
					{
						if (Objects[i].States.Length != 0)
						{
							if (free[i])
							{
								Matrix4D transformationMatrix = (Matrix4D)new Transformation(LocalTransformation, WorldTransformation);
								Matrix4D mat = Matrix4D.Identity;
								mat *= Objects[i].States[0].Translation;
								mat *= transformationMatrix;
								double zOffset = Objects[i].States[0].Translation.ExtractTranslation().Z * -1.0; //To calculate the Z-offset within the object, we want the untransformed co-ordinates, not the world co-ordinates
								
								currentHost.CreateStaticObject(Objects[i].States[0].Prototype, LocalTransformation, mat, Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z), zOffset, StartingDistance, EndingDistance, TrackPosition, Brightness);
							}
							else
							{
								Objects[i].CreateObject(Position, WorldTransformation, LocalTransformation, SectionIndex, TrackPosition, Brightness);
							}
						}
					}
				}
				else
				{
					for (int i = 0; i < Objects.Length; i++)
					{
						if (Objects[i].States.Length != 0)
						{
							Objects[i].CreateObject(Position, WorldTransformation, LocalTransformation, SectionIndex, TrackPosition, Brightness);
						}
					}
				}
				if (this.Sounds == null)
				{
					return;
				}
				for (int i = 0; i < Sounds.Length; i++)
				{
					if (this.Sounds[i] == null)
					{
						continue;
					}
					(Sounds[i] as WorldSound)?.CreateSound(Position + Sounds[i].Position, WorldTransformation, LocalTransformation, SectionIndex, TrackPosition);
					(Sounds[i] as AnimatedWorldObjectStateSound)?.Create(Position, WorldTransformation, LocalTransformation, SectionIndex, TrackPosition, Brightness);
				}
			}

			/// <inheritdoc/>
			public override void OptimizeObject(bool PreserveVerticies, int Threshold, bool VertexCulling)
			{
				for (int i = 0; i < Objects.Length; i++)
				{
					for (int j = 0; j < Objects[i].States.Length; j++)
					{
						if (Objects[i].States[j].Prototype == null)
						{
							continue;
						}
						Objects[i].States[j].Prototype.OptimizeObject(PreserveVerticies, Threshold, VertexCulling);
					}
				}
			}

			/// <inheritdoc/>
			public override UnifiedObject Clone()
			{
				AnimatedObjectCollection aoc = new AnimatedObjectCollection(currentHost);
				if (Objects != null)
				{
					aoc.Objects = Objects.Select(x => x?.Clone()).ToArray();
				}
				if (Sounds != null)
				{
					aoc.Sounds = Sounds.Select(x => x?.Clone()).ToArray();
				}
				return aoc;
			}

			/// <summary>Creates a mirrored clone of this object</summary>
			public override UnifiedObject Mirror()
			{
				AnimatedObjectCollection Result = new AnimatedObjectCollection(currentHost)
				{
					Objects = new AnimatedObject[Objects.Length]
				};
				for (int i = 0; i < Objects.Length; i++)
				{
					Result.Objects[i] = Objects[i].Clone();
					for (int j = 0; j < Objects[i].States.Length; j++)
					{
						Result.Objects[i].States[j].Prototype = (StaticObject)Result.Objects[i].States[j].Prototype.Mirror();
					}
					Result.Objects[i].TranslateXDirection.X *= -1.0;
					Result.Objects[i].TranslateYDirection.X *= -1.0;
					Result.Objects[i].TranslateZDirection.X *= -1.0;
					//As we are using a rotation matrix, we only need to reverse the translation and not the rotation
				}
				return Result;
			}

			/// <summary>Reverses the object</summary>
			public void Reverse()
			{
				foreach (AnimatedObject animatedObj in Objects)
				{
					animatedObj.Reverse();
				}
			}
			
			/// <inheritdoc/>
			public override UnifiedObject Transform(double NearDistance, double FarDistance)
			{
				throw new NotSupportedException();
			}
		}
}
