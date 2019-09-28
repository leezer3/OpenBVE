using System;
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
			public override void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation,
				int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength,
				double TrackPosition, double Brightness, bool DuplicateMaterials)
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
					if (AuxTransformation.X != Vector3.Right || AuxTransformation.Y != Vector3.Down || AuxTransformation.Z != Vector3.Forward)
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
								OpenTK.Matrix4d mat = OpenTK.Matrix4d.Identity;
								mat *= Objects[i].States[0].Translation;
								mat *= Objects[i].States[0].Rotate;
								double zOffset = mat.ExtractTranslation().Z * -1.0;
								mat *= (OpenTK.Matrix4d)new Transformation(BaseTransformation, AuxTransformation);
								OpenTK.Vector4d p = OpenTK.Vector4d.Transform(new OpenTK.Vector4d(Position.X, Position.Y, -Position.Z, 1.0), mat);
								currentHost.CreateStaticObject(Objects[i].States[0].Prototype, new Vector3(p.X, p.Y, -p.Z), BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
							}
							else
							{
								Objects[i].CreateObject(Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
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
							Objects[i].CreateObject(Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
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
					var snd = this.Sounds[i] as WorldSound;
					if (snd != null)
					{
						snd.CreateSound(Sounds[i].Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition);
					}
					var snd2 = this.Sounds[i] as AnimatedWorldObjectStateSound;
					if (snd2 != null)
					{
						snd2.Create(Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
					}
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
				throw new NotSupportedException();
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
					Result.Objects[i].RotateXDirection.X *= -1.0;
					Result.Objects[i].RotateYDirection.X *= -1.0;
					Result.Objects[i].RotateZDirection.X *= -1.0;
				}
				return Result;
			}
		}
}
