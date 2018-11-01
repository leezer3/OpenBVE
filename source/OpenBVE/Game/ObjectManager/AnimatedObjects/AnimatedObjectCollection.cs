using OpenBveApi.Math;
using OpenBveApi.World;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>The AnimatedObjectCollection is a simple container class containing one or more animated objects</summary>
		internal class AnimatedObjectCollection : UnifiedObject
		{
			/// <summary>The objects that this collection contains</summary>
			internal AnimatedObject[] Objects;
			internal WorldObject[] Sounds;

			internal override void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation,
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
								p.X += Objects[i].States[0].Position.X * s.X + Objects[i].States[0].Position.Y * u.X + Objects[i].States[0].Position.Z * d.X;
								p.Y += Objects[i].States[0].Position.X * s.Y + Objects[i].States[0].Position.Y * u.Y + Objects[i].States[0].Position.Z * d.Y;
								p.Z += Objects[i].States[0].Position.X * s.Z + Objects[i].States[0].Position.Y * u.Z + Objects[i].States[0].Position.Z * d.Z;
								double zOffset = Objects[i].States[0].Position.Z;
								CreateStaticObject(Objects[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
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

			internal override void OptimizeObject(bool PreserveVerticies)
			{
				for (int i = 0; i < Objects.Length; i++)
				{
					for (int j = 0; j < Objects[i].States.Length; j++)
					{
						if (Objects[i].States[j].Object == null)
						{
							continue;
						}
						Objects[i].States[j].Object.OptimizeObject(PreserveVerticies);
					}
				}
			}
		}
	}
}
