using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>Represents a keyframe animated object</summary>
	public class KeyframeAnimatedObject : UnifiedObject
	{
		private readonly HostInterface currentHost;
		/// <summary>The refresh rate in seconds</summary>
		public double RefreshRate;
		/// <summary>The time since the last update of this object</summary>
		public double SecondsSinceLastUpdate;
		/// <summary>Contains the list of animations contained within this object</summary>
		public Dictionary<string, KeyframeAnimation> Animations;
		/// <summary>The objects within this object</summary>
		public ObjectState[] Objects;
		/// <summary>The keyframe matricies</summary>
		public KeyframeMatrix[] Matricies;

		/// <summary>Creates an empty KeyFrameAnimatedObject</summary>
		/// <param name="currentHost">The host application interface</param>
		public KeyframeAnimatedObject(HostInterface currentHost)
		{
			this.currentHost = currentHost;
			RefreshRate = 0;
			SecondsSinceLastUpdate = 0;
			Matricies = new KeyframeMatrix[0];
			Animations = new Dictionary<string, KeyframeAnimation>();
		}

		/// <inheritdoc />
		public override void CreateObject(Vector3 position, Transformation worldTransformation, Transformation localTransformation, int sectionIndex, double startingDistance, double endingDistance, double trackPosition, double brightness, bool duplicateMaterials = false)
		{
			Matrix4D[] matriciesToShader = new Matrix4D[Matricies.Length];
			for (int i = 0; i < matriciesToShader.Length; i++)
			{
				matriciesToShader[i] = Matricies[i].Matrix;
			}
			for (int i = 0; i < Objects.Length; i++)
			{
				Objects[i].Rotate = Matrix4D.NoTransformation;
				Objects[i].Translation = Matrix4D.NoTransformation;
				currentHost.ShowObject(Objects[i], ObjectType.Dynamic);
				Objects[i].Matricies = matriciesToShader;
			}
			int a = currentHost.AnimatedWorldObjectsUsed;
			KeyframeWorldObject currentObject = new KeyframeWorldObject(currentHost)
			{
				Object = this
			};
			currentHost.AnimatedWorldObjects[a] = currentObject;
			currentHost.AnimatedWorldObjectsUsed++;

		}

		/// <inheritdoc />
		public override void OptimizeObject(bool preserveVerticies, int threshold, bool vertexCulling)
		{
			// not required
		}

		/// <inheritdoc />
		public override UnifiedObject Clone()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override UnifiedObject Mirror()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override UnifiedObject Transform(double nearDistance, double farDistance)
		{
			throw new NotSupportedException("Cannot transform a KeyframeObject");
		}

		/// <inheritdoc />
		public override UnifiedObject TransformLeft(double nearDistance, double farDistance)
		{
			throw new NotSupportedException("Cannot transform a KeyframeObject");
		}

		/// <inheritdoc />
		public override UnifiedObject TransformRight(double nearDistance, double farDistance)
		{
			throw new NotSupportedException("Cannot transform a KeyframeObject");
		}

		/// <summary> Updates the position and state of the animated object</summary>
		/// <param name="train">The train, or a null reference otherwise</param>
		/// <param name="carIndex">If this object forms part of a train, the car index it refers to</param>
		/// <param name="trackPosition"></param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="up"></param>
		/// <param name="side"></param>
		/// <param name="updateFunctions">Whether the functions associated with this object should be re-evaluated</param>
		/// <param name="show"></param>
		/// <param name="timeElapsed">The time elapsed since this object was last updated</param>
		/// <param name="enableDamping">Whether damping is to be applied for this call</param>
		/// <param name="isTouch">Whether Animated Object belonging to TouchElement class.</param>
		/// <param name="camera"></param>
		public void Update(AbstractTrain train, int carIndex, double trackPosition, Vector3 position, Vector3 direction, Vector3 up, Vector3 side, bool updateFunctions, bool show, double timeElapsed, bool enableDamping, bool isTouch = false, dynamic camera = null)
		{
			// Update animations
			for (int i = 0; i < Animations.Count; i++)
			{
				string key = Animations.ElementAt(i).Key;
				Animations[key].Update(train, carIndex, position, trackPosition, -1, train != null, timeElapsed, -1); // current state not applicable, no hand-crafted functions
			}
			Matrix4D[] matriciesToShader = new Matrix4D[Matricies.Length];
			for (int i = 0; i < matriciesToShader.Length; i++)
			{
				matriciesToShader[i] = Matricies[i].Matrix;
			}
			for (int i = 0; i < Objects.Length; i++)
			{
				Objects[i].Matricies = matriciesToShader;
				Objects[i].Translation = Matrix4D.CreateTranslation(position.X, position.Y, -position.Z);
				currentHost.ShowObject(Objects[i], ObjectType.Dynamic);
			}
		}
	}

	public class KeyframeMatrix
	{
		private readonly KeyframeAnimatedObject containerObject;

		public KeyframeMatrix(KeyframeAnimatedObject container, string name, Matrix4D matrix)
		{
			containerObject = container;
			Name = name;
			_matrix = matrix;
		}

		public readonly string Name;

		private readonly Matrix4D _matrix;

		public Matrix4D Matrix
		{
			get
			{
				if (containerObject.Animations != null && containerObject.Animations.ContainsKey(Name))
				{
					return containerObject.Animations[Name].Matrix;

				}
				return _matrix;
			}
		}
	}

}
