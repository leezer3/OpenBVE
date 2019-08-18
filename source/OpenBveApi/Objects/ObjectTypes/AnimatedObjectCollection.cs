using System;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>The AnimatedObjectCollection is a simple container class containing one or more animated objects</summary>
		public class AnimatedObjectCollection : UnifiedObject
		{

			private readonly HostInterface currentHost;

			private readonly Track[] Tracks;
			/// <summary>The objects that this collection contains</summary>
			public AnimatedObject[] Objects;
			/// <summary>The sounds that this collection contains</summary>
			public WorldObject[] Sounds;

			/// <summary>Creates a new AnimatedObjectCollection</summary>
			public AnimatedObjectCollection(HostInterface Host, Track[] tracks)
			{
				currentHost = Host;
				Tracks = tracks;
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
								Vector3 p = Position;
								Transformation t = new Transformation(BaseTransformation, AuxTransformation);
								Vector3 s = t.X;
								Vector3 u = t.Y;
								Vector3 d = t.Z;
								p += Objects[i].States[0].Position * s + Objects[i].States[0].Position * u + Objects[i].States[0].Position * d;
								double zOffset = Objects[i].States[0].Position.Z;
								currentHost.CreateStaticObject(Objects[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
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
						snd.CreateSound(Tracks, Sounds[i].Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition);
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
						if (Objects[i].States[j].Object == null)
						{
							continue;
						}
						Objects[i].States[j].Object.OptimizeObject(PreserveVerticies, Threshold, VertexCulling);
					}
				}
			}

			/// <inheritdoc/>
			public override UnifiedObject Clone()
			{
				throw new NotSupportedException();
			}
		}
}
