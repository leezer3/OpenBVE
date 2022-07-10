using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBveApi.Objects
{
	/// <summary>An object using a hiearchy of animated parts</summary>
	public class HierarchyAnimatedObject : UnifiedObject
	{
		internal readonly HostInterface currentHost;
		/// <summary>The animation hiearchy, containing the transformation and rotation matricies</summary>
		public Dictionary<string, HierarchyEntry> HierarchyParts;
		/// <summary>The actual objects to be animated</summary>
		public HiearchyObject[] Objects;
		/// <summary>The time since the last update of this object</summary>
		public double SecondsSinceLastUpdate;

		/// <summary>Creates a new HierarchyAnimatedObject</summary>
		/// <param name="Host">Reference to the host application</param>
		public HierarchyAnimatedObject(HostInterface Host)
		{
			currentHost = Host;
			HierarchyParts = new Dictionary<string, HierarchyEntry>();
		}

		/// <summary>Updates the animated object</summary>
		public void Update(bool IsPartOfTrain, AbstractTrain Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping, bool IsTouch = false, dynamic Camera = null)
		{
			for (int i = 0; i < HierarchyParts.Count; i++)
			{
				string key = HierarchyParts.ElementAt(i).Key;
				HierarchyParts[key].FunctionScript.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, -1);
			}

			for (int i = 0; i < Objects.Length; i++)
			{
				Objects[i].Update();
				Objects[i].StateFunctionScript.ExecuteScript(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, (int)Objects[i].StateFunctionScript.LastResult);
				if (Show && Objects[i].StateFunctionScript.LastResult != -1)
				{
					if (Camera != null)
					{
						currentHost.ShowObject(Objects[i].State, ObjectType.Overlay);
					}
					else
					{
						currentHost.ShowObject(Objects[i].State, ObjectType.Dynamic);
					}
				}
				else
				{
					currentHost.HideObject(Objects[i].State);
				}
			}
		}

		/// <inheritdoc/>
		public override void CreateObject(Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, int SectionIndex, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness, bool DuplicateMaterials = false)
		{
			for (int i = 0; i < Objects.Length; i++)
			{
				Objects[i].State.Prototype.CreateObject(Position, WorldTransformation, LocalTransformation, StartingDistance, EndingDistance, TrackPosition);
			}
		}

		/// <inheritdoc/>
		public override void OptimizeObject(bool PreserveVerticies, int Threshold, bool VertexCulling)
		{
			// Should not be necessary as these are never hand-crafted
		}

		/// <inheritdoc/>
		public override UnifiedObject Clone()
		{
			throw new NotImplementedException("TODO");
		}

		/// <inheritdoc/>
		public override UnifiedObject Mirror()
		{
			throw new NotImplementedException("TODO");
		}

		/// <inheritdoc/>
		public override UnifiedObject Transform(double NearDistance, double FarDistance)
		{
			throw new NotSupportedException("Cannot be used as a transformed object");
		}
	}

	/// <summary>An object stored within a hierarchy of animated parts</summary>
	public class HiearchyObject
	{
		/// <summary>Holds a reference to the root object</summary>
		private readonly HierarchyAnimatedObject rootObject;
		/// <summary>Contains the hiearchy list</summary>
		public readonly string[] Hiearchy;
		/// <summary>The object state to be transformed</summary>
		public readonly ObjectState State;
		/// <summary>The state function script</summary>
		public readonly FunctionScript StateFunctionScript;
		
		/// <summary>Creates a new hierarchy object</summary>
		public HiearchyObject(HierarchyAnimatedObject RootObject, string[] hiearchy, ObjectState state, FunctionScript stateFunctionScript)
		{
			rootObject = RootObject;
			Hiearchy = hiearchy;
			State = state;
			StateFunctionScript = stateFunctionScript;
		}

		/// <summary>Updates the final object state</summary>
		internal void Update()
		{
			Matrix4D translation = Matrix4D.NoTransformation;
			Matrix4D rotation = Matrix4D.NoTransformation;
			for (int i = 0; i < Hiearchy.Length; i++)
			{
				if (rootObject.HierarchyParts.ContainsKey(Hiearchy[i]))
				{
					translation += rootObject.HierarchyParts[Hiearchy[i]].CurrentTranslationMatrix;
					rotation += rootObject.HierarchyParts[Hiearchy[i]].CurrentRotationMatrix;
				}
			}

			State.Translation = translation;
			State.Rotate = rotation;
		}

	}

	/// <summary>An animation hiearchy entry</summary>
	public class HierarchyEntry
	{
		/// <summary>The name of this entry</summary>
		public readonly string Name;
		/// <summary>The controlling function script</summary>
		public readonly FunctionScript FunctionScript;
		/// <summary>The list of translation matricies</summary>
		public readonly Matrix4D[] TranslationMatricies;
		/// <summary>The list of rotation matricies</summary>
		public readonly Matrix4D[] RotationMatricies;
		
		/// <summary>Creates a new hieararchy entry</summary>
		public HierarchyEntry(HostInterface host, string partName, string functionScript, Matrix4D[] translationMatricies, Matrix4D[] rotationMatricies)
		{
			Name = partName;
			if(string.IsNullOrEmpty(functionScript))
			{
				FunctionScript = new FunctionScript(host, "1", false);
			}
			else
			{
				FunctionScript = new FunctionScript(host, functionScript, false);	
			}

			if (translationMatricies == null)
			{
				TranslationMatricies = new[]
				{
					Matrix4D.NoTransformation
				};
			}
			else
			{
				TranslationMatricies = translationMatricies;	
			}

			if (rotationMatricies == null)
			{
				RotationMatricies = new[]
				{
					Matrix4D.NoTransformation
				};
			}
			else
			{
				RotationMatricies = rotationMatricies;	
			}
			
		}

		/// <summary>Gets the current matrix</summary>
		public Matrix4D CurrentTranslationMatrix => TranslationMatricies[(int)FunctionScript.LastResult % TranslationMatricies.Length];
		/// <summary>Gets the current matrix</summary>
		public Matrix4D CurrentRotationMatrix => RotationMatricies[(int)FunctionScript.LastResult % RotationMatricies.Length];
	}
}
