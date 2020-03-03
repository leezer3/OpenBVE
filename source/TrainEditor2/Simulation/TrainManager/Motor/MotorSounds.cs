using System;
using System.Linq;
using OpenBveApi.Math;
using OpenBveApi.Units;
using SoundManager;

namespace TrainEditor2.Simulation.TrainManager
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal struct MotorSound
		{
			internal class Vertex<T> : ICloneable where T : struct
			{
				internal Quantity.VelocityF X;
				internal T Y;

				public virtual object Clone()
				{
					return MemberwiseClone();
				}
			}

			internal class Vertex<T, U> : Vertex<T> where T : struct where U : ICloneable
			{
				internal U Z;

				public override object Clone()
				{
					Vertex<T, U> vertex = (Vertex<T, U>)base.Clone();
					vertex.Z = (U)Z?.Clone();
					return vertex;
				}
			}

			internal struct Entry
			{
				internal double Pitch;
				internal double Gain;
				internal int SoundIndex;
				internal SoundBuffer Buffer;
			}

			internal class Table : ICloneable
			{
				internal Vertex<float>[] PitchVertices;
				internal Vertex<float>[] GainVertices;
				internal Vertex<int, SoundBuffer>[] BufferVertices;
				internal SoundBuffer PlayingBuffer;
				internal SoundSource PlayingSource;

				private static Tuple<int, SoundBuffer> GetVertex(Vertex<int, SoundBuffer>[] vertices, Quantity.VelocityF x)
				{
					if (!vertices.Any())
					{
						return new Tuple<int, SoundBuffer>(-1, null);
					}

					Vertex<int, SoundBuffer> left = vertices.LastOrDefault(v => v.X <= x) ?? vertices.First();
					return new Tuple<int, SoundBuffer>(left.Y, left.Z);
				}

				private static float GetVertex(Vertex<float>[] vertices, Quantity.VelocityF x)
				{
					if (!vertices.Any())
					{
						return 1.0f;
					}

					Vertex<float> left = vertices.LastOrDefault(v => v.X <= x) ?? vertices.First();
					Vertex<float> right = vertices.FirstOrDefault(v => x < v.X) ?? vertices.Last();

					if (left == right)
					{
						return left.Y;
					}

					return left.Y + (right.Y - left.Y) * (x - left.X) / (right.X - left.X);
				}

				internal Entry GetEntry(float speed)
				{
					float pitch = GetVertex(PitchVertices, new Quantity.VelocityF(speed));
					float gain = GetVertex(GainVertices, new Quantity.VelocityF(speed));
					Tuple<int, SoundBuffer> buffer = GetVertex(BufferVertices, new Quantity.VelocityF(speed));
					return new Entry { Pitch = pitch, Gain = gain, SoundIndex = buffer.Item1, Buffer = buffer.Item2 };
				}

				public object Clone()
				{
					return new Table
					{
						PitchVertices = PitchVertices.Select(x => (Vertex<float>)x.Clone()).ToArray(),
						GainVertices = GainVertices.Select(x => (Vertex<float>)x.Clone()).ToArray(),
						BufferVertices = BufferVertices.Select(x => (Vertex<int, SoundBuffer>)x.Clone()).ToArray()
					};
				}
			}

			internal Table[] PowerTables;
			internal Table[] BrakeTables;
			internal Vector3 Position;
			internal int CurrentAccelerationDirection;
		}
	}
}
