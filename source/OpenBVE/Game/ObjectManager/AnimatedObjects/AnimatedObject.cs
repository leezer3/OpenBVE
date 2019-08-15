using System;
using System.IO;
using CSScriptLibrary;
using LibRender;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		
		internal class AnimatedObject : AnimatedObjectBase
		{
			internal AnimatedObject(HostInterface host)
			{
				currentHost = host;
			}

			/// <summary>Clones this object</summary>
			/// <returns>The new object</returns>
			internal AnimatedObject Clone()
			{
				AnimatedObject Result = new AnimatedObject(currentHost) { States = new AnimatedObjectState[this.States.Length] };
				for (int i = 0; i < this.States.Length; i++)
				{
					Result.States[i].Position = this.States[i].Position;
					if (this.States[i].Object != null)
					{
						Result.States[i].Object = (StaticObject)this.States[i].Object.Clone();
					}
				}
				Result.TrackFollowerFunction = this.TrackFollowerFunction == null ? null : this.TrackFollowerFunction.Clone();
				Result.FrontAxlePosition = this.FrontAxlePosition;
				Result.RearAxlePosition = this.RearAxlePosition;
				Result.TranslateXScriptFile = this.TranslateXScriptFile;
				Result.StateFunction = this.StateFunction == null ? null : this.StateFunction.Clone();
				Result.CurrentState = this.CurrentState;
				Result.TranslateZDirection = this.TranslateZDirection;
				Result.TranslateYDirection = this.TranslateYDirection;
				Result.TranslateXDirection = this.TranslateXDirection;
				Result.TranslateXFunction = this.TranslateXFunction == null ? null : this.TranslateXFunction.Clone();
				Result.TranslateYFunction = this.TranslateYFunction == null ? null : this.TranslateYFunction.Clone();
				Result.TranslateZFunction = this.TranslateZFunction == null ? null : this.TranslateZFunction.Clone();
				Result.RotateXDirection = this.RotateXDirection;
				Result.RotateYDirection = this.RotateYDirection;
				Result.RotateZDirection = this.RotateZDirection;
				Result.RotateXFunction = this.RotateXFunction == null ? null : this.RotateXFunction.Clone();
				Result.RotateXDamping = this.RotateXDamping == null ? null : this.RotateXDamping.Clone();
				Result.RotateYFunction = this.RotateYFunction == null ? null : this.RotateYFunction.Clone();
				Result.RotateYDamping = this.RotateYDamping == null ? null : this.RotateYDamping.Clone();
				Result.RotateZFunction = this.RotateZFunction == null ? null : this.RotateZFunction.Clone();
				Result.RotateZDamping = this.RotateZDamping == null ? null : this.RotateZDamping.Clone();
				Result.TextureShiftXDirection = this.TextureShiftXDirection;
				Result.TextureShiftYDirection = this.TextureShiftYDirection;
				Result.TextureShiftXFunction = this.TextureShiftXFunction == null ? null : this.TextureShiftXFunction.Clone();
				Result.TextureShiftYFunction = this.TextureShiftYFunction == null ? null : this.TextureShiftYFunction.Clone();
				Result.LEDClockwiseWinding = this.LEDClockwiseWinding;
				Result.LEDInitialAngle = this.LEDInitialAngle;
				Result.LEDLastAngle = this.LEDLastAngle;
				if (this.LEDVectors != null)
				{
					Result.LEDVectors = new Vector3[this.LEDVectors.Length];
					for (int i = 0; i < this.LEDVectors.Length; i++)
					{
						Result.LEDVectors[i] = this.LEDVectors[i];
					}
				}
				else
				{
					Result.LEDVectors = null;
				}
				Result.LEDFunction = this.LEDFunction == null ? null : this.LEDFunction.Clone();
				Result.RefreshRate = this.RefreshRate;
				Result.SecondsSinceLastUpdate = 0.0;
				for (int i = 0; i < Timetable.CustomObjectsUsed; i++)
				{
					if (Timetable.CustomObjects[i] == this)
					{
						Timetable.AddObjectForCustomTimetable(Result);
					}
				}
				return Result;
			}

			internal void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness)
			{
				int a = AnimatedWorldObjectsUsed;
				if (a >= AnimatedWorldObjects.Length)
				{
					Array.Resize<WorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
				}
				Transformation FinalTransformation = new Transformation(AuxTransformation, BaseTransformation);
				
				//Place track followers if required
				if (TrackFollowerFunction != null)
				{
					var o = this.Clone();
					currentHost.CreateDynamicObject(ref internalObject);
					TrackFollowingObject currentObject = new TrackFollowingObject
					{
						Position = Position,
						Direction = FinalTransformation.Z,
						Up = FinalTransformation.Y,
						Side = FinalTransformation.X,
						Object = o,
						SectionIndex = SectionIndex,
						TrackPosition = TrackPosition,
					};
					
					currentObject.FrontAxleFollower.TrackPosition = TrackPosition + FrontAxlePosition;
					currentObject.RearAxleFollower.TrackPosition = TrackPosition + RearAxlePosition;
					currentObject.FrontAxlePosition = FrontAxlePosition;
					currentObject.RearAxlePosition = RearAxlePosition;
					currentObject.FrontAxleFollower.UpdateWorldCoordinates(false);
					currentObject.RearAxleFollower.UpdateWorldCoordinates(false);
					for (int i = 0; i < currentObject.Object.States.Length; i++)
					{
						if (currentObject.Object.States[i].Object == null)
						{
							currentObject.Object.States[i].Object = new StaticObject(currentHost) { RendererIndex =  -1 };
						}
					}
					double r = 0.0;
					for (int i = 0; i < currentObject.Object.States.Length; i++)
					{
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Materials.Length; j++)
						{
							currentObject.Object.States[i].Object.Mesh.Materials[j].Color *= Brightness;
						}
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Vertices.Length; j++)
						{
							double t = States[i].Object.Mesh.Vertices[j].Coordinates.Norm();
							if (t > r) r = t;
						}
					}
					currentObject.Radius = Math.Sqrt(r);
					currentObject.Visible = false;
					currentObject.Object.Initialize(0, false, false);
					AnimatedWorldObjects[a] = currentObject;
				}
				else
				{
					var o = this.Clone();
					currentHost.CreateDynamicObject(ref o.internalObject);
					AnimatedWorldObject currentObject = new AnimatedWorldObject
					{
						Position = Position,
						Direction = FinalTransformation.Z,
						Up = FinalTransformation.Y,
						Side = FinalTransformation.X,
						Object = o,
						SectionIndex = SectionIndex,
						TrackPosition = TrackPosition,
					};
					for (int i = 0; i < currentObject.Object.States.Length; i++)
					{
						if (currentObject.Object.States[i].Object == null)
						{
							currentObject.Object.States[i].Object = new StaticObject(currentHost) { RendererIndex =  -1 };
						}
					}
					double r = 0.0;
					for (int i = 0; i < currentObject.Object.States.Length; i++)
					{
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Materials.Length; j++)
						{
							currentObject.Object.States[i].Object.Mesh.Materials[j].Color *= Brightness;
						}
						for (int j = 0; j < currentObject.Object.States[i].Object.Mesh.Vertices.Length; j++)
						{
							double t = States[i].Object.Mesh.Vertices[j].Coordinates.Norm();
							if (t > r) r = t;
						}
					}
					currentObject.Radius = Math.Sqrt(r);
					currentObject.Visible = false;
					currentObject.Object.Initialize(0, false, false);
					AnimatedWorldObjects[a] = currentObject;
				}
				AnimatedWorldObjectsUsed++;
			}
		}

		
	}
}
