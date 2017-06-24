using System;
using CSScriptLibrary;
using OpenBveApi.Math;

namespace OpenBve {
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static class ObjectManager
	{
		// unified objects
		internal abstract class UnifiedObject { }
		/// <summary>Interfaces with the CS Script Library </summary>
		public interface AnimationScript
		{
			/// <summary> Call to execute this script </summary>
			/// <param name="Train">A reference to the nearest train</param>
			/// <param name="Position">The object's absolute in world position</param>
			/// <param name="TrackPosition">The object's track position</param>
			/// <param name="SectionIndex"></param>
			/// <param name="IsPartOfTrain">Whether this object forms part of a train</param>
			/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
			double ExecuteScript(TrainManager.Train Train, Vector3 Position, double TrackPosition,int SectionIndex,bool IsPartOfTrain,double TimeElapsed);
		}

		// static objects
		internal class StaticObject : UnifiedObject {
			internal World.Mesh Mesh;
			/// <summary>The index to the Renderer.Object array, plus 1. The value of zero represents that the object is not currently shown by the renderer.</summary>
			internal int RendererIndex;
			/// <summary>The starting track position, for static objects only.</summary>
			internal float StartingDistance;
			/// <summary>The ending track position, for static objects only.</summary>
			internal float EndingDistance;
			/// <summary>The block mod group, for static objects only.</summary>
			internal short GroupIndex;
			/// <summary>Whether the object is dynamic, i.e. not static.</summary>
			internal bool Dynamic;
			/// <summary> Stores the author for this object.</summary>
			internal string Author;
			/// <summary> Stores the copyright information for this object.</summary>
			internal string Copyright;
		}
		internal static StaticObject[] Objects = new StaticObject[16];
		internal static int ObjectsUsed;
		internal static int[] ObjectsSortedByStart = new int[] { };
		internal static int[] ObjectsSortedByEnd = new int[] { };
		internal static int ObjectsSortedByStartPointer = 0;
		internal static int ObjectsSortedByEndPointer = 0;
		internal static double LastUpdatedTrackPosition = 0.0;

		// animated objects
		internal class Damping {
			internal double NaturalFrequency;
			internal double NaturalTime;
			internal double DampingRatio;
			internal double NaturalDampingFrequency;
			internal double OriginalAngle;
			internal double OriginalDerivative;
			internal double TargetAngle;
			internal double CurrentAngle;
			internal double CurrentValue;
			internal double CurrentTimeDelta;
			internal Damping(double NaturalFrequency, double DampingRatio)
			{
				if (NaturalFrequency < 0.0) {
					throw new ArgumentException("NaturalFrequency must be non-negative in the constructor of the Damping class.");
				}
				if (DampingRatio < 0.0) {
					throw new ArgumentException("DampingRatio must be non-negative in the constructor of the Damping class.");
				}
				this.NaturalFrequency = NaturalFrequency;
				this.NaturalTime = NaturalFrequency != 0.0 ? 1.0 / NaturalFrequency : 0.0;
				this.DampingRatio = DampingRatio;
				if (DampingRatio < 1.0) {
					this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(1.0 - DampingRatio * DampingRatio);
				} else if (DampingRatio == 1.0) {
					this.NaturalDampingFrequency = NaturalFrequency;
				} else {
					this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(DampingRatio * DampingRatio - 1.0);
				}
				this.OriginalAngle = 0.0;
				this.OriginalDerivative = 0.0;
				this.TargetAngle = 0.0;
				this.CurrentAngle = 0.0;
				this.CurrentValue = 1.0;
				this.CurrentTimeDelta = 0.0;
			}

			internal Damping Clone() {
				return (Damping)this.MemberwiseClone();
			}
		}
		internal struct AnimatedObjectState {
			internal Vector3 Position;
			internal ObjectManager.StaticObject Object;
		}

		

		internal class AnimatedObject {
			// states
			internal AnimatedObjectState[] States;
			internal FunctionScripts.FunctionScript StateFunction;
			internal int CurrentState;
			internal Vector3 TranslateXDirection;
			internal Vector3 TranslateYDirection;
			internal Vector3 TranslateZDirection;
			internal FunctionScripts.FunctionScript TranslateXFunction;
			internal FunctionScripts.FunctionScript TranslateYFunction;
			internal FunctionScripts.FunctionScript TranslateZFunction;
			
			internal Vector3 RotateXDirection;
			internal Vector3 RotateYDirection;
			internal Vector3 RotateZDirection;
			internal FunctionScripts.FunctionScript RotateXFunction;
			internal FunctionScripts.FunctionScript RotateYFunction;
			internal FunctionScripts.FunctionScript RotateZFunction;
			internal Damping RotateXDamping;
			internal Damping RotateYDamping;
			internal Damping RotateZDamping;
			internal Vector2 TextureShiftXDirection;
			internal Vector2 TextureShiftYDirection;
			internal FunctionScripts.FunctionScript TextureShiftXFunction;
			internal FunctionScripts.FunctionScript TextureShiftYFunction;
			internal bool LEDClockwiseWinding;
			internal double LEDInitialAngle;
			internal double LEDLastAngle;
			/// <summary>If LEDFunction is used, an array of five vectors representing the bottom-left, up-left, up-right, bottom-right and center coordinates of the LED square, or a null reference otherwise.</summary>
			internal Vector3[] LEDVectors;
			internal FunctionScripts.FunctionScript LEDFunction;
			internal double RefreshRate;
			internal double CurrentTrackZOffset;
			internal double SecondsSinceLastUpdate;
			internal int ObjectIndex;

			//This section holds script files executed by CS-Script
			/// <summary>The absolute path to the script file to be evaluated when TranslateXScript is called</summary>
			internal string TranslateXScriptFile;
			internal AnimationScript TranslateXAnimationScript;
			/// <summary>The absolute path to the script file to be evaluated when TranslateYScript is called</summary>
			internal AnimationScript TranslateYAnimationScript;
			internal string TranslateYScriptFile;
			/// <summary>The absolute path to the script file to be evaluated when TranslateZScript is called</summary>
			internal AnimationScript TranslateZAnimationScript;
			internal string TranslateZScriptFile;

			internal FunctionScripts.FunctionScript TrackFollowerFunction;
			//This section holds parameters used by the track following function
			internal double FrontAxlePosition = 1;
			internal double RearAxlePosition = -1;
			// methods
			internal bool IsFreeOfFunctions() {
				if (this.StateFunction != null) return false;
				if (this.TrackFollowerFunction != null) return false;
				if (this.TranslateXFunction != null | this.TranslateYFunction != null | this.TranslateZFunction != null) return false;
				if (this.RotateXFunction != null | this.RotateYFunction != null | this.RotateZFunction != null) return false;
				if (this.TextureShiftXFunction != null | this.TextureShiftYFunction != null) return false;
				if (this.LEDFunction != null) return false;
				if (this.TranslateXScriptFile != null | this.TranslateYScriptFile != null | this.TranslateZScriptFile != null) return false;
				return true;
			}
			internal AnimatedObject Clone() {
				AnimatedObject Result = new AnimatedObject {States = new AnimatedObjectState[this.States.Length]};
				for (int i = 0; i < this.States.Length; i++) {
					Result.States[i].Position = this.States[i].Position;
					Result.States[i].Object = CloneObject(this.States[i].Object);
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
				if (this.LEDVectors != null) {
					Result.LEDVectors = new Vector3[this.LEDVectors.Length];
					for (int i = 0; i < this.LEDVectors.Length; i++) {
						Result.LEDVectors[i] = this.LEDVectors[i];
					}
				} else {
					Result.LEDVectors = null;
				}
				Result.LEDFunction = this.LEDFunction == null ? null : this.LEDFunction.Clone();
				Result.RefreshRate = this.RefreshRate;
				Result.CurrentTrackZOffset = 0.0;
				Result.SecondsSinceLastUpdate = 0.0;
				Result.ObjectIndex = -1;
				return Result;
			}
		}
		internal class AnimatedObjectCollection : UnifiedObject {
			internal AnimatedObject[] Objects;
		}
		internal static void InitializeAnimatedObject(ref AnimatedObject Object, int StateIndex, bool Overlay, bool Show) {
			int i = Object.ObjectIndex;
			Renderer.HideObject(i);
			int t = StateIndex;
			if (t >= 0 && Object.States[t].Object != null) {
				int m = Object.States[t].Object.Mesh.Vertices.Length;
				ObjectManager.Objects[i].Mesh.Vertices = new World.Vertex[m];
				for (int k = 0; k < m; k++) {
					ObjectManager.Objects[i].Mesh.Vertices[k] = Object.States[t].Object.Mesh.Vertices[k];
				}
				m = Object.States[t].Object.Mesh.Faces.Length;
				ObjectManager.Objects[i].Mesh.Faces = new World.MeshFace[m];
				for (int k = 0; k < m; k++) {
					ObjectManager.Objects[i].Mesh.Faces[k].Flags = Object.States[t].Object.Mesh.Faces[k].Flags;
					ObjectManager.Objects[i].Mesh.Faces[k].Material = Object.States[t].Object.Mesh.Faces[k].Material;
					int o = Object.States[t].Object.Mesh.Faces[k].Vertices.Length;
					ObjectManager.Objects[i].Mesh.Faces[k].Vertices = new World.MeshFaceVertex[o];
					for (int h = 0; h < o; h++) {
						ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h] = Object.States[t].Object.Mesh.Faces[k].Vertices[h];
					}
				}
				ObjectManager.Objects[i].Mesh.Materials = Object.States[t].Object.Mesh.Materials;
			} else {
				ObjectManager.Objects[i] = null;
				ObjectManager.Objects[i] = new StaticObject
				{
					Mesh =
					{
						Faces = new World.MeshFace[] {},
						Materials = new World.MeshMaterial[] {},
						Vertices = new World.Vertex[] {}
					}
				};
			}
			Object.CurrentState = StateIndex;
			if (Show) {
				if (Overlay) {
					Renderer.ShowObject(i, Renderer.ObjectType.Overlay);
				} else {
					Renderer.ShowObject(i, Renderer.ObjectType.Dynamic);
				}
			}
		}

		internal static double UpdateTrackFollowerScript(ref AnimatedObject Object, bool IsPartOfTrain, TrainManager.Train Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed)
		{
			double x = 0.0;
			if (Object.TrackFollowerFunction != null)
			{
				if (UpdateFunctions)
				{
					x = Object.TrackFollowerFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain,
						TimeElapsed, Object.CurrentState);
				}
				else
				{
					x = Object.TrackFollowerFunction.LastResult;
				}
			}
			return x;
		}

		/// <summary> Updates the position and state of the specified animated object</summary>
		/// <param name="Object">The object to update</param>
		/// <param name="IsPartOfTrain">Whether this object forms part of a train</param>
		/// <param name="Train">The train, or a null reference otherwise</param>
		/// <param name="CarIndex">If this object forms part of a train, the car index it refers to</param>
		/// <param name="SectionIndex">If this object has been placed via Track.Sig, the index of the section it is attached to</param>
		/// <param name="TrackPosition"></param>
		/// <param name="Position"></param>
		/// <param name="Direction"></param>
		/// <param name="Up"></param>
		/// <param name="Side"></param>
		/// <param name="Overlay">Whether this object should be overlaid over the other objects on-screen (Forms part of the cab etc.)</param>
		/// <param name="UpdateFunctions">Whether the functions associated with this object should be re-evaluated</param>
		/// <param name="Show"></param>
		/// <param name="TimeElapsed">The time elapsed since this object was last updated</param>
		/// <param name="EnableDamping">Whether damping is to be applied for this call</param>
		internal static void UpdateAnimatedObject(ref AnimatedObject Object, bool IsPartOfTrain, TrainManager.Train Train, int CarIndex, 
			int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping) {
			int s = Object.CurrentState;
			int i = Object.ObjectIndex;
			// state change
			if (Object.StateFunction != null & UpdateFunctions) {
				double sd = Object.StateFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				int si = (int)Math.Round(sd);
				int sn = Object.States.Length;
				if (si < 0 | si >= sn) si = -1;
				if (s != si) {
					InitializeAnimatedObject(ref Object, si, Overlay, Show);
					s = si;
				}
			}
			if (s == -1) return;
			// translation
			if (Object.TranslateXFunction != null) {
				double x;
				if (UpdateFunctions) {
					x = Object.TranslateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else  {
					x = Object.TranslateXFunction.LastResult;
				}
				double rx = Object.TranslateXDirection.X, ry = Object.TranslateXDirection.Y, rz = Object.TranslateXDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
				Position.X += x * rx;
				Position.Y += x * ry;
				Position.Z += x * rz;
			}
			else if (Object.TranslateXScriptFile != null)
			{
				//Translate X Script
				if (Object.TranslateXAnimationScript == null)
				{
					//Load the script if required
					try
					{
						CSScript.GlobalSettings.TargetFramework = "v4.0";
						Object.TranslateXAnimationScript = CSScript.LoadCodeFrom(Object.TranslateXScriptFile)
							.CreateObject("OpenBVEScript")
							.AlignToInterface<AnimationScript>(true);
					}
					catch
					{
						Interface.AddMessage(Interface.MessageType.Error, false,
							"An error occcured whilst parsing script " + Object.TranslateXScriptFile);
						Object.TranslateXScriptFile = null;
						return;
					}
				}
				double x = Object.TranslateXAnimationScript.ExecuteScript(Train, Position, TrackPosition, SectionIndex,
					IsPartOfTrain, TimeElapsed);
				double rx = Object.TranslateXDirection.X, ry = Object.TranslateXDirection.Y, rz = Object.TranslateXDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y,
					Side.Z);
				Position.X += x*rx;
				Position.Y += x*ry;
				Position.Z += x*rz;
			}


			if (Object.TranslateYFunction != null) {
				double y;
				if (UpdateFunctions) {
					y = Object.TranslateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					y = Object.TranslateYFunction.LastResult;
				}
				double rx = Object.TranslateYDirection.X, ry = Object.TranslateYDirection.Y, rz = Object.TranslateYDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
				Position.X += y * rx;
				Position.Y += y * ry;
				Position.Z += y * rz;
			}
			else if (Object.TranslateYScriptFile != null)
			{
				//Translate X Script
				if (Object.TranslateYAnimationScript == null)
				{
					//Load the script if required
					try
					{
						CSScript.GlobalSettings.TargetFramework = "v4.0";
						Object.TranslateYAnimationScript = CSScript.LoadCodeFrom(Object.TranslateYScriptFile)
							.CreateObject("OpenBVEScript")
							.AlignToInterface<AnimationScript>(true);
					}
					catch
					{
						Interface.AddMessage(Interface.MessageType.Error, false,
							"An error occcured whilst parsing script " + Object.TranslateYScriptFile);
						Object.TranslateYScriptFile = null;
						return;
					}
				}
				double y = Object.TranslateYAnimationScript.ExecuteScript(Train, Position, TrackPosition, SectionIndex,
					IsPartOfTrain, TimeElapsed);
				double rx = Object.TranslateYDirection.X, ry = Object.TranslateYDirection.Y, rz = Object.TranslateYDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y,
					Side.Z);
				Position.X += y * rx;
				Position.Y += y * ry;
				Position.Z += y * rz;
			}

			if (Object.TranslateZFunction != null) {
				double z;
				if (UpdateFunctions) {
					z = Object.TranslateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					z = Object.TranslateZFunction.LastResult;
				}
				double rx = Object.TranslateZDirection.X, ry = Object.TranslateZDirection.Y, rz = Object.TranslateZDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
				Position.X += z * rx;
				Position.Y += z * ry;
				Position.Z += z * rz;
			}
			else if (Object.TranslateZScriptFile != null)
			{
				//Translate X Script
				if (Object.TranslateZAnimationScript == null)
				{
					//Load the script if required
					try
					{
						CSScript.GlobalSettings.TargetFramework = "v4.0";
						Object.TranslateZAnimationScript = CSScript.LoadCodeFrom(Object.TranslateZScriptFile)
							.CreateObject("OpenBVEScript")
							.AlignToInterface<AnimationScript>(true);
					}
					catch
					{
						Interface.AddMessage(Interface.MessageType.Error, false,
							"An error occcured whilst parsing script " + Object.TranslateZScriptFile);
						Object.TranslateZScriptFile = null;
						return;
					}
				}
				double z = Object.TranslateZAnimationScript.ExecuteScript(Train, Position, TrackPosition, SectionIndex,
					IsPartOfTrain, TimeElapsed);
				double rx = Object.TranslateZDirection.X, ry = Object.TranslateZDirection.Y, rz = Object.TranslateZDirection.Z;
				World.Rotate(ref rx, ref ry, ref rz, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y,
					Side.Z);
				Position.X += z * rx;
				Position.Y += z * ry;
				Position.Z += z * rz;
			}
			// rotation
			bool rotateX = Object.RotateXFunction != null;
			bool rotateY = Object.RotateYFunction != null;
			bool rotateZ = Object.RotateZFunction != null;
			double cosX, sinX;
			if (rotateX) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					a = Object.RotateXFunction.LastResult;
				}
				ObjectManager.UpdateDamping(ref Object.RotateXDamping, TimeElapsed, ref a, EnableDamping);
				cosX = Math.Cos(a);
				sinX = Math.Sin(a);
			} else {
				cosX = 0.0; sinX = 0.0;
			}
			double cosY, sinY;
			if (rotateY) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					a = Object.RotateYFunction.LastResult;
				}
				ObjectManager.UpdateDamping(ref Object.RotateYDamping, TimeElapsed, ref a, EnableDamping);
				cosY = Math.Cos(a);
				sinY = Math.Sin(a);
			} else {
				cosY = 0.0; sinY = 0.0;
			}
			double cosZ, sinZ;
			if (rotateZ) {
				double a;
				if (UpdateFunctions) {
					a = Object.RotateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					a = Object.RotateZFunction.LastResult;
				}
				ObjectManager.UpdateDamping(ref Object.RotateZDamping, TimeElapsed, ref a, EnableDamping);
				cosZ = Math.Cos(a);
				sinZ = Math.Sin(a);
			} else {
				cosZ = 0.0; sinZ = 0.0;
			}
			// texture shift
			bool shiftx = Object.TextureShiftXFunction != null;
			bool shifty = Object.TextureShiftYFunction != null;
			if ((shiftx | shifty) & UpdateFunctions) {
				for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++) {
					ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates = Object.States[s].Object.Mesh.Vertices[k].TextureCoordinates;
				}
				if (shiftx) {
					double x = Object.TextureShiftXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
					x -= Math.Floor(x);
					for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++) {
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(x * Object.TextureShiftXDirection.X);
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(x * Object.TextureShiftXDirection.Y);
					}
				}
				if (shifty) {
					double y = Object.TextureShiftYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
					y -= Math.Floor(y);
					for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++) {
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(y * Object.TextureShiftYDirection.X);
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(y * Object.TextureShiftYDirection.Y);
					}
				}
			}
			// led
			bool led = Object.LEDFunction != null;
			double ledangle;
			if (led) {
				if (UpdateFunctions) {
					ledangle = Object.LEDFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
				} else {
					ledangle = Object.LEDFunction.LastResult;
				}
			} else {
				ledangle = 0.0;
			}
			// null object
			if (Object.States[s].Object == null) {
				return;
			}
			// initialize vertices
			for (int k = 0; k < Object.States[s].Object.Mesh.Vertices.Length; k++) {
				ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates = Object.States[s].Object.Mesh.Vertices[k].Coordinates;
			}
			// led
			if (led) {
				/*
				 * Edges:         Vertices:
				 * 0 - bottom     0 - bottom-left
				 * 1 - left       1 - top-left
				 * 2 - top        2 - top-right
				 * 3 - right      3 - bottom-right
				 *                4 - center
				 * */
				int v = 1;
				if (Object.LEDClockwiseWinding) {
					/* winding is clockwise*/
					if (ledangle < Object.LEDInitialAngle) {
						ledangle = Object.LEDInitialAngle;
					}
					if (ledangle < Object.LEDLastAngle) {
						double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
						int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
						double lastEdgeFloat = Math.Floor(0.636619772367582 * (Object.LEDLastAngle + 0.785398163397449));
						int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
						if (lastEdge < currentEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0) {
							lastEdge += 4;
						}
						if (currentEdge == lastEdge) {
							/* current angle to last angle */
							{
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
								v++;
							}
							{
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
								v++;
							}
						} else {
							{
								/* current angle to square vertex */
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(cx, cy, cz);
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[currentEdge];
								v += 2;
							}
							for (int j = currentEdge + 1; j < lastEdge; j++) {
								/* square-vertex to square-vertex */
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(j + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[j % 4];
								v += 2;
							}
							{
								/* square vertex to last angle */
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge % 4].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge % 4].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge % 4].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(lastEdge + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(lx, ly, lz);
								v += 2;
							}
						}
					}
				} else {
					/* winding is counter-clockwise*/
					if (ledangle > Object.LEDInitialAngle) {
						ledangle = Object.LEDInitialAngle;
					}
					if (ledangle > Object.LEDLastAngle) {
						double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
						int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
						double lastEdgeFloat = Math.Floor(0.636619772367582 * (Object.LEDLastAngle + 0.785398163397449));
						int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
						if (currentEdge < lastEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0) {
							currentEdge += 4;
						}
						if (currentEdge == lastEdge) {
							/* current angle to last angle */
							{
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
								v++;
							}
							{
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = t - Math.Floor(t);
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
								v++;
							}
						} else {
							{
								/* current angle to square vertex */
								double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge % 4].X;
								double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge % 4].Y;
								double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge % 4].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(currentEdge + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(cx, cy, cz);
								v += 2;
							}
							for (int j = currentEdge - 1; j > lastEdge; j--) {
								/* square-vertex to square-vertex */
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(j + 3) % 4];
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[j % 4];
								v += 2;
							}
							{
								/* square vertex to last angle */
								double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
								if (t < 0.0) {
									t = 0.0;
								} else if (t > 1.0) {
									t = 1.0;
								}
								t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
								double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
								double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
								double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
								Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(lx, ly, lz);
								Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[lastEdge % 4];
								v += 2;
							}
						}
					}
				}
				for (int j = v; v < 11; v++) {
					Object.States[s].Object.Mesh.Vertices[j].Coordinates = Object.LEDVectors[4];
				}
			}
			// update vertices
			for (int k = 0; k < Object.States[s].Object.Mesh.Vertices.Length; k++) {
				// rotate
				if (rotateX) {
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates, Object.RotateXDirection, cosX, sinX);
				}
				if (rotateY) {
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates, Object.RotateYDirection, cosY, sinY);
				}
				if (rotateZ) {
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates, Object.RotateZDirection, cosZ, sinZ);
				}
				// translate
				if (Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable) {
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += Object.States[s].Position.X - Position.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += Object.States[s].Position.Y - Position.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += Object.States[s].Position.Z - Position.Z;
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z, World.AbsoluteCameraDirection.X, World.AbsoluteCameraDirection.Y, World.AbsoluteCameraDirection.Z, World.AbsoluteCameraUp.X, World.AbsoluteCameraUp.Y, World.AbsoluteCameraUp.Z, World.AbsoluteCameraSide.X, World.AbsoluteCameraSide.Y, World.AbsoluteCameraSide.Z);
					double dx = -Math.Tan(World.CameraCurrentAlignment.Yaw) - World.CameraCurrentAlignment.Position.X;
					double dy = -Math.Tan(World.CameraCurrentAlignment.Pitch) - World.CameraCurrentAlignment.Position.Y;
					double dz = -World.CameraCurrentAlignment.Position.Z;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += World.AbsoluteCameraPosition.X + dx * World.AbsoluteCameraSide.X + dy * World.AbsoluteCameraUp.X + dz * World.AbsoluteCameraDirection.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += World.AbsoluteCameraPosition.Y + dx * World.AbsoluteCameraSide.Y + dy * World.AbsoluteCameraUp.Y + dz * World.AbsoluteCameraDirection.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += World.AbsoluteCameraPosition.Z + dx * World.AbsoluteCameraSide.Z + dy * World.AbsoluteCameraUp.Z + dz * World.AbsoluteCameraDirection.Z;
				} else {
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += Object.States[s].Position.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += Object.States[s].Position.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += Object.States[s].Position.Z;
					World.Rotate(ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y, ref ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.X += Position.X;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Y += Position.Y;
					ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Z += Position.Z;
				}
			}
			// update normals
			for (int k = 0; k < Object.States[s].Object.Mesh.Faces.Length; k++) {
				for (int h = 0; h < Object.States[s].Object.Mesh.Faces[k].Vertices.Length; h++) {
					ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal = Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal;
				}
				for (int h = 0; h < Object.States[s].Object.Mesh.Faces[k].Vertices.Length; h++) {
					if (!Vector3.IsZero(Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal))
					{
						if (rotateX) {
							World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal, Object.RotateXDirection, cosX, sinX);
						}
						if (rotateY) {
							World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal, Object.RotateYDirection, cosY, sinY);
						}
						if (rotateZ) {
							World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal, Object.RotateZDirection, cosZ, sinZ);
						}
						World.Rotate(ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.X, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Y, ref ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
					}
				}
				// visibility changed
				if (Show) {
					if (Overlay) {
						Renderer.ShowObject(i, Renderer.ObjectType.Overlay);
					} else {
						Renderer.ShowObject(i, Renderer.ObjectType.Dynamic);
					}
				} else {
					Renderer.HideObject(i);
				}
			}
		}

		// update damping
		internal static void UpdateDamping(ref Damping Damping, double TimeElapsed, ref double Angle, bool Enable) {
			if (Damping != null && Enable == false)
			{
				Damping.CurrentValue = 1.0;
				Damping.CurrentAngle = Angle;
				Damping.OriginalAngle = Angle;
				Damping.TargetAngle = Angle;
				return;
			}
			if (TimeElapsed < 0.0) {
				TimeElapsed = 0.0;
			} else if (TimeElapsed > 1.0) {
				TimeElapsed = 1.0;
			}
			if (Damping != null) {
				if (Damping.CurrentTimeDelta > Damping.NaturalTime) {
					// update
					double newDerivative;
					if (Damping.NaturalFrequency == 0.0) {
						newDerivative = 0.0;
					} else if (Damping.DampingRatio == 0.0) {
						newDerivative = Damping.OriginalDerivative * Math.Cos(Damping.NaturalFrequency * Damping.CurrentTimeDelta) - Damping.NaturalFrequency * Math.Sin(Damping.NaturalFrequency * Damping.CurrentTimeDelta);
					} else if (Damping.DampingRatio < 1.0) {
						newDerivative = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Damping.NaturalDampingFrequency * Damping.OriginalDerivative * Math.Cos(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) - (Damping.NaturalDampingFrequency * Damping.NaturalDampingFrequency + Damping.DampingRatio * Damping.NaturalFrequency * (Damping.DampingRatio * Damping.NaturalFrequency + Damping.OriginalDerivative)) * Math.Sin(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta)) / Damping.NaturalDampingFrequency;
					} else if (Damping.DampingRatio == 1.0) {
						newDerivative = Math.Exp(-Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Damping.OriginalDerivative - Damping.NaturalFrequency * (Damping.NaturalFrequency + Damping.OriginalDerivative) * Damping.CurrentTimeDelta);
					} else {
						newDerivative = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Damping.NaturalDampingFrequency * Damping.OriginalDerivative * Math.Cosh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) + (Damping.NaturalDampingFrequency * Damping.NaturalDampingFrequency - Damping.DampingRatio * Damping.NaturalFrequency * (Damping.DampingRatio * Damping.NaturalFrequency + Damping.OriginalDerivative)) * Math.Sinh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta)) / Damping.NaturalDampingFrequency;
					}
					double a = Damping.TargetAngle - Damping.OriginalAngle;
					Damping.OriginalAngle = Damping.CurrentAngle;
					Damping.TargetAngle = Angle;
					double b = Damping.TargetAngle - Damping.OriginalAngle;
					double r = b == 0.0 ? 1.0 : a / b;
					Damping.OriginalDerivative = newDerivative * r;
					if (Damping.NaturalTime > 0.0) {
						Damping.CurrentTimeDelta = Damping.CurrentTimeDelta % Damping.NaturalTime;
					}
				}
				{
					// perform
					double newValue;
					if (Damping.NaturalFrequency == 0.0) {
						newValue = 1.0;
					} else if (Damping.DampingRatio == 0.0) {
						newValue = Math.Cos(Damping.NaturalFrequency * Damping.CurrentTimeDelta) + Damping.OriginalDerivative * Math.Sin(Damping.NaturalFrequency * Damping.CurrentTimeDelta) / Damping.NaturalFrequency;
					} else if (Damping.DampingRatio < 1.0) {
						double n = (Damping.OriginalDerivative + Damping.NaturalFrequency * Damping.DampingRatio) / Damping.NaturalDampingFrequency;
						newValue = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Math.Cos(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) + n * Math.Sin(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta));
					} else if (Damping.DampingRatio == 1.0) {
						newValue = Math.Exp(-Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (1.0 + (Damping.OriginalDerivative + Damping.NaturalFrequency) * Damping.CurrentTimeDelta);
					} else {
						double n = (Damping.OriginalDerivative + Damping.NaturalFrequency * Damping.DampingRatio) / Damping.NaturalDampingFrequency;
						newValue = Math.Exp(-Damping.DampingRatio * Damping.NaturalFrequency * Damping.CurrentTimeDelta) * (Math.Cosh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta) + n * Math.Sinh(Damping.NaturalDampingFrequency * Damping.CurrentTimeDelta));
					}
					Damping.CurrentValue = newValue;
					Damping.CurrentAngle = Damping.TargetAngle * (1.0 - newValue) + Damping.OriginalAngle * newValue;
					Damping.CurrentTimeDelta += TimeElapsed;
					Angle = Damping.CurrentAngle;
				}
			}
		}

		// animated world object
		internal class AnimatedWorldObject
		{
			internal bool FollowsTrack = false;
			internal TrackManager.TrackFollower FrontAxleFollower;
			internal TrackManager.TrackFollower RearAxleFollower;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
		/// <summary>Holds the properties for an animated object within the simulation world</summary>
			internal Vector3 Position;
			/// <summary>The object's relative track position</summary>
			internal double TrackPosition;
			internal Vector3 Direction;
			internal Vector3 Up;
			internal Vector3 Side;
			/// <summary>The actual animated object</summary>
			internal AnimatedObject Object;
			/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
			internal int SectionIndex;
			/// <summary>The curve radius at the object's track position</summary>
			internal double Radius;
			/// <summary>Whether the object is currently visible</summary>
			internal bool Visible;
			/*
			 * NOT IMPLEMENTED, BUT REQUIRED LATER
			 */
			internal double CurrentRollDueToTopplingAngle = 0;
			internal double CurrentRollDueToCantAngle = 0;
		}
		internal static AnimatedWorldObject[] AnimatedWorldObjects = new AnimatedWorldObject[4];
		internal static int AnimatedWorldObjectsUsed = 0;
		internal static void CreateAnimatedWorldObjects(AnimatedObject[] Prototypes, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			bool[] free = new bool[Prototypes.Length];
			bool anyfree = false;
			bool allfree = true;
			for (int i = 0; i < Prototypes.Length; i++) {
				free[i] = Prototypes[i].IsFreeOfFunctions();
				if (free[i])
				{
					anyfree = true;
				}
				else
				{
					allfree = false;
				}
			}
			if (anyfree && !allfree && Prototypes.Length > 1)
			{
				var X = new Vector3(1.0, 0.0, 0.0);
				var Y = new Vector3(0.0, 1.0, 0.0);
				var Z = new Vector3(0.0, 0.0, 1.0);
				//Optimise a little: If *all* are free of functions, this can safely be converted into a static object without regard to below
				if (AuxTransformation.X != X|| AuxTransformation.Y != Y || AuxTransformation.Z != Z)
				{
					//HACK:
					//An animated object containing a mix of functions and non-functions and using yaw, pitch or roll must not be converted into a mix
					//of animated and static objects, as this causes rounding differences....
					anyfree = false;
				}
			}
			if (anyfree) {
				for (int i = 0; i < Prototypes.Length; i++) {
					if (Prototypes[i].States.Length != 0) {
						if (free[i]) {
							Vector3 p = Position;
							World.Transformation t = new OpenBve.World.Transformation(BaseTransformation, AuxTransformation);
							Vector3 s = t.X;
							Vector3 u = t.Y;
							Vector3 d = t.Z;
							p.X += Prototypes[i].States[0].Position.X * s.X + Prototypes[i].States[0].Position.Y * u.X + Prototypes[i].States[0].Position.Z * d.X;
							p.Y += Prototypes[i].States[0].Position.X * s.Y + Prototypes[i].States[0].Position.Y * u.Y + Prototypes[i].States[0].Position.Z * d.Y;
							p.Z += Prototypes[i].States[0].Position.X * s.Z + Prototypes[i].States[0].Position.Y * u.Z + Prototypes[i].States[0].Position.Z * d.Z;
							double zOffset = Prototypes[i].States[0].Position.Z;
							CreateStaticObject(Prototypes[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
						} else {
							CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
						}
					}
				}
			} else {
				for (int i = 0; i < Prototypes.Length; i++) {
					if (Prototypes[i].States.Length != 0) {
						CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
					}
				}
			}
		}
		internal static int CreateAnimatedWorldObject(AnimatedObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness) {
			int a = AnimatedWorldObjectsUsed;
			if (a >= AnimatedWorldObjects.Length) {
				Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
			}
			World.Transformation FinalTransformation = new World.Transformation(AuxTransformation, BaseTransformation);
			AnimatedWorldObjects[a] = new AnimatedWorldObject
			{
				Position = Position,
				Direction = FinalTransformation.Z,
				Up = FinalTransformation.Y,
				Side = FinalTransformation.X,
				Object = Prototype.Clone()
			};
			AnimatedWorldObjects[a].Object.ObjectIndex = CreateDynamicObject();
			AnimatedWorldObjects[a].SectionIndex = SectionIndex;
			AnimatedWorldObjects[a].TrackPosition = TrackPosition;
			//Place track followers if required
			if (Prototype.TrackFollowerFunction != null)
			{
				AnimatedWorldObjects[a].FollowsTrack = true;
				AnimatedWorldObjects[a].FrontAxleFollower.TrackPosition = TrackPosition + Prototype.FrontAxlePosition;
				AnimatedWorldObjects[a].RearAxleFollower.TrackPosition = TrackPosition + Prototype.RearAxlePosition;
				AnimatedWorldObjects[a].FrontAxlePosition = Prototype.FrontAxlePosition;
				AnimatedWorldObjects[a].RearAxlePosition = Prototype.RearAxlePosition;
				AnimatedWorldObjects[a].FrontAxleFollower.UpdateWorldCoordinates(false);
				AnimatedWorldObjects[a].RearAxleFollower.UpdateWorldCoordinates(false);
				
			}
			for (int i = 0; i < AnimatedWorldObjects[a].Object.States.Length; i++) {
				if (AnimatedWorldObjects[a].Object.States[i].Object == null) {
					AnimatedWorldObjects[a].Object.States[i].Object = new StaticObject
					{
						Mesh =
						{
							Faces = new World.MeshFace[] {},
							Materials = new World.MeshMaterial[] {},
							Vertices = new World.Vertex[] {}
						},
						RendererIndex = -1
					};
				}
			}
			double r = 0.0;
			for (int i = 0; i < AnimatedWorldObjects[a].Object.States.Length; i++) {
				for (int j = 0; j < AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials.Length; j++) {
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.R = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.R * Brightness);
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.G = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.G * Brightness);
					AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.B = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.B * Brightness);
				}
				for (int j = 0; j < AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Vertices.Length; j++) {
					double x = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.X;
					double y = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.Y;
					double z = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.Z;
					double t = x * x + y * y + z * z;
					if (t > r) r = t;
				}
			}
			AnimatedWorldObjects[a].Radius = Math.Sqrt(r);
			AnimatedWorldObjects[a].Visible = false;
			InitializeAnimatedObject(ref AnimatedWorldObjects[a].Object, 0, false, false);
			AnimatedWorldObjectsUsed++;
			return a;
		}
		internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate) {
			for (int i = 0; i < AnimatedWorldObjectsUsed; i++) {
				const double extraRadius = 10.0;
				double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z - World.BackgroundImageDistance - World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z + World.BackgroundImageDistance + World.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate) {
					if (AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate >= AnimatedWorldObjects[i].Object.RefreshRate | ForceUpdate) {
						double timeDelta = AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate + TimeElapsed;
						AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate = 0.0;
						TrainManager.Train train = null;
						double trainDistance = double.MaxValue;
						for (int j = 0; j < TrainManager.Trains.Length; j++) {
							if (TrainManager.Trains[j].State == TrainManager.TrainState.Available) {
								double distance;
								if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < AnimatedWorldObjects[i].TrackPosition) {
									distance = AnimatedWorldObjects[i].TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
								} else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > AnimatedWorldObjects[i].TrackPosition) {
									distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - AnimatedWorldObjects[i].TrackPosition;
								} else {
									distance = 0;
								}
								if (distance < trainDistance) {
									train = TrainManager.Trains[j];
									trainDistance = distance;
								}
							}
						}
						if (AnimatedWorldObjects[i].FollowsTrack)
						{
							if (AnimatedWorldObjects[i].Visible)
							{
								//Calculate the distance travelled
								double delta = UpdateTrackFollowerScript(ref AnimatedWorldObjects[i].Object, false, train, train == null ? 0 : train.DriverCar, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].TrackPosition, AnimatedWorldObjects[i].Position, AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true, timeDelta);

								//Update the front and rear axle track followers
								TrackManager.UpdateTrackFollower(ref AnimatedWorldObjects[i].FrontAxleFollower, (AnimatedWorldObjects[i].TrackPosition + AnimatedWorldObjects[i].FrontAxlePosition) + delta, true, true);
								TrackManager.UpdateTrackFollower(ref AnimatedWorldObjects[i].RearAxleFollower, (AnimatedWorldObjects[i].TrackPosition + AnimatedWorldObjects[i].RearAxlePosition) + delta, true, true);
								//Update the base object position
								AnimatedWorldObjects[i].FrontAxleFollower.UpdateWorldCoordinates(false);
								AnimatedWorldObjects[i].RearAxleFollower.UpdateWorldCoordinates(false);
								UpdateTrackFollowingObject(ref AnimatedWorldObjects[i]);
								
							}
							//Update the actual animated object- This must be done last in case the user has used Translation or Rotation
							UpdateAnimatedObject(ref AnimatedWorldObjects[i].Object, false, train, train == null ? 0 : train.DriverCar, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].FrontAxleFollower.TrackPosition, AnimatedWorldObjects[i].FrontAxleFollower.WorldPosition, AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true, timeDelta, true);
							
						}
						else
						{
							UpdateAnimatedObject(ref AnimatedWorldObjects[i].Object, false, train, train == null ? 0 : train.DriverCar,AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].TrackPosition, AnimatedWorldObjects[i].Position,
								AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true,timeDelta, true);
						}

					} else {
						AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
					}
					if (!AnimatedWorldObjects[i].Visible) {
						Renderer.ShowObject(AnimatedWorldObjects[i].Object.ObjectIndex, Renderer.ObjectType.Dynamic);
						AnimatedWorldObjects[i].Visible = true;
					}
				} else {
					AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
					if (AnimatedWorldObjects[i].Visible) {
						Renderer.HideObject(AnimatedWorldObjects[i].Object.ObjectIndex);
						AnimatedWorldObjects[i].Visible = false;
					}
				}
			}
		}

		/// <summary>Updates the position and rotation of an animated object which follows a track</summary>
		/// <param name="Object">The animated object to update</param>
		internal static void UpdateTrackFollowingObject(ref AnimatedWorldObject Object)
		{
			//Get vectors
			double dx, dy, dz;
			double ux, uy, uz;
			double sx, sy, sz;
			{
				dx = Object.FrontAxleFollower.WorldPosition.X -
					 Object.RearAxleFollower.WorldPosition.X;
				dy = Object.FrontAxleFollower.WorldPosition.Y -
					 Object.RearAxleFollower.WorldPosition.Y;
				dz = Object.FrontAxleFollower.WorldPosition.Z -
					 Object.RearAxleFollower.WorldPosition.Z;
				double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
				dx *= t;
				dy *= t;
				dz *= t;
				t = 1.0 / Math.Sqrt(dx * dx + dz * dz);
				double ex = dx * t;
				double ez = dz * t;
				sx = ez;
				sy = 0.0;
				sz = -ex;
				World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
			}
			
			// apply position due to cant/toppling
			{
				double a = Object.CurrentRollDueToTopplingAngle +
						   Object.CurrentRollDueToCantAngle;
				double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
				double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
				double cx = sx * x + ux * y;
				double cy = sy * x + uy * y;
				double cz = sz * x + uz * y;
				Object.FrontAxleFollower.WorldPosition.X += cx;
				Object.FrontAxleFollower.WorldPosition.Y += cy;
				Object.FrontAxleFollower.WorldPosition.Z += cz;
				Object.RearAxleFollower.WorldPosition.X += cx;
				Object.RearAxleFollower.WorldPosition.Y += cy;
				Object.RearAxleFollower.WorldPosition.Z += cz;
			}
			// apply rolling
			{
				double a = Object.CurrentRollDueToTopplingAngle -
						   Object.CurrentRollDueToCantAngle;
				double cosa = Math.Cos(a);
				double sina = Math.Sin(a);
				World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
				World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
				Object.Up.X = ux;
				Object.Up.Y = uy;
				Object.Up.Z = uz;
			}
			Object.Direction.X = dx;
			Object.Direction.Y = dy;
			Object.Direction.Z = dz;
			Object.Side.X = sx;
			Object.Side.Y = sy;
			Object.Side.Z = sz;
		}

		// load object
		internal enum ObjectLoadMode { Normal, DontAllowUnloadOfTextures }
		internal static UnifiedObject LoadObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			if (String.IsNullOrEmpty(FileName))
			{
				return null;
			}
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						var f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".xml");
						if (System.IO.File.Exists(f))
						{
							FileName = f;
						}
						break;
					}
				}
				UnifiedObject Result;
				TextEncoding.Encoding newEncoding = TextEncoding.GetEncodingFromFile(FileName);
				if (newEncoding != TextEncoding.Encoding.Unknown)
				{
					switch (newEncoding)
					{
						case TextEncoding.Encoding.Utf7:
							Encoding = System.Text.Encoding.UTF7;
							break;
						case TextEncoding.Encoding.Utf8:
							Encoding = System.Text.Encoding.UTF8;
							break;
						case TextEncoding.Encoding.Utf16Le:
							Encoding = System.Text.Encoding.Unicode;
							break;
						case TextEncoding.Encoding.Utf16Be:
							Encoding = System.Text.Encoding.BigEndianUnicode;
							break;
						case TextEncoding.Encoding.Utf32Le:
							Encoding = System.Text.Encoding.UTF32;
							break;
						case TextEncoding.Encoding.Utf32Be:
							Encoding = System.Text.Encoding.GetEncoding(12001);
							break;
						case TextEncoding.Encoding.Shift_JIS:
							Encoding = System.Text.Encoding.GetEncoding(932);
							break;
						case TextEncoding.Encoding.Windows1252:
							Encoding = System.Text.Encoding.GetEncoding(1252);
							break;
						case TextEncoding.Encoding.Big5:
							Encoding = System.Text.Encoding.GetEncoding(950);
							break;
					}
				}
			switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
						Result = CsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".x":
						Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".animated":
						Result = AnimatedObjectParser.ReadObject(FileName, Encoding, LoadMode);
						break;
					case ".xml":
						Result = XMLParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					default:
						Interface.AddMessage(Interface.MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}
				OptimizeObject(Result, PreserveVertices);
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}
		internal static StaticObject LoadStaticObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			if (String.IsNullOrEmpty(FileName))
			{
				return null;
			}
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
						}
						break;
					}
				}
				StaticObject Result;
				switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
						Result = CsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".x":
						Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
					case ".animated":
						Interface.AddMessage(Interface.MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
						return null;
						/*
						 * This will require implementing a specific static object load function- Leave alone for the moment
						 * 
					case ".xml":
						Result = XMLParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
						break;
						 */
					default:
						Interface.AddMessage(Interface.MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}
				OptimizeObject(Result, PreserveVertices);
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}

		// optimize object
		internal static void OptimizeObject(UnifiedObject Prototype, bool PreserveVertices) {
			if (Prototype is StaticObject) {
				StaticObject s = (StaticObject)Prototype;
				OptimizeObject(s, PreserveVertices);
			} else if (Prototype is AnimatedObjectCollection) {
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				for (int i = 0; i < a.Objects.Length; i++) {
					for (int j = 0; j < a.Objects[i].States.Length; j++) {
						OptimizeObject(a.Objects[i].States[j].Object, PreserveVertices);
					}
				}
			}
		}
		internal static void OptimizeObject(StaticObject Prototype, bool PreserveVertices) {
			if (Prototype == null)
			{
				return;
			}
			int v = Prototype.Mesh.Vertices.Length;
			int m = Prototype.Mesh.Materials.Length;
			int f = Prototype.Mesh.Faces.Length;
			if (f >= Interface.CurrentOptions.ObjectOptimizationBasicThreshold)
			{
				return;
			}
			// eliminate invalid faces and reduce incomplete faces
			for (int i = 0; i < f; i++) {
				int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
				bool keep;
				if (type == World.MeshFace.FaceTypeTriangles) {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 3;
					if (keep) {
						int n = (Prototype.Mesh.Faces[i].Vertices.Length / 3) * 3;
						if (Prototype.Mesh.Faces[i].Vertices.Length != n) {
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n);
						}
					}
				} else if (type == World.MeshFace.FaceTypeQuads) {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 4;
					if (keep) {
						int n = Prototype.Mesh.Faces[i].Vertices.Length & ~3;
						if (Prototype.Mesh.Faces[i].Vertices.Length != n) {
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n);
						}
					}
				} else if (type == World.MeshFace.FaceTypeQuadStrip) {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 4;
					if (keep) {
						int n = Prototype.Mesh.Faces[i].Vertices.Length & ~1;
						if (Prototype.Mesh.Faces[i].Vertices.Length != n) {
							Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, n);
						}
					}
				} else {
					keep = Prototype.Mesh.Faces[i].Vertices.Length >= 3;
				}
				if (!keep) {
					for (int j = i; j < f - 1; j++) {
						Prototype.Mesh.Faces[j] = Prototype.Mesh.Faces[j + 1];
					}
					f--;
					i--;
				}
			}
			// eliminate unused materials
			bool[] materialUsed = new bool[m];
			for (int i = 0; i < f; i++) {
				materialUsed[Prototype.Mesh.Faces[i].Material] = true;
			}
			for (int i = 0; i < m; i++) {
				if (!materialUsed[i]) {
					for (int j = 0; j < f; j++) {
						if (Prototype.Mesh.Faces[j].Material > i) {
							Prototype.Mesh.Faces[j].Material--;
						}
					}
					for (int j = i; j < m - 1; j++) {
						Prototype.Mesh.Materials[j] = Prototype.Mesh.Materials[j + 1];
						materialUsed[j] = materialUsed[j + 1];
					}
					m--;
					i--;
				}
			}
			// eliminate duplicate materials
			for (int i = 0; i < m - 1; i++) {
				for (int j = i + 1; j < m; j++) {
					if (Prototype.Mesh.Materials[i] == Prototype.Mesh.Materials[j]) {
						for (int k = 0; k < f; k++) {
							if (Prototype.Mesh.Faces[k].Material == j) {
								Prototype.Mesh.Faces[k].Material = (ushort)i;
							} else if (Prototype.Mesh.Faces[k].Material > j) {
								Prototype.Mesh.Faces[k].Material--;
							}
						}
						for (int k = j; k < m - 1; k++) {
							Prototype.Mesh.Materials[k] = Prototype.Mesh.Materials[k + 1];
						}
						m--;
						j--;
					}
				}
			}
			/* TODO:
			 * Use a hash based technique
			 */ 
			// Cull vertices based on hidden option.
			// This is disabled by default because it adds a lot of time to the loading process.
			if (!PreserveVertices && Interface.CurrentOptions.ObjectOptimizationVertexCulling)
			{
				// eliminate unused vertices
				for (int i = 0; i < v; i++)
				{
					bool keep = false;
					for (int j = 0; j < f; j++)
					{
						for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++)
						{
							if (Prototype.Mesh.Faces[j].Vertices[k].Index == i)
							{
								keep = true;
								break;
							}
						}
						if (keep)
						{
							break;
						}
					}
					if (!keep)
					{
						for (int j = 0; j < f; j++)
						{
							for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++)
							{
								if (Prototype.Mesh.Faces[j].Vertices[k].Index > i)
								{
									Prototype.Mesh.Faces[j].Vertices[k].Index--;
								}
							}
						}
						for (int j = i; j < v - 1; j++)
						{
							Prototype.Mesh.Vertices[j] = Prototype.Mesh.Vertices[j + 1];
						}
						v--;
						i--;
					}
				}
				
				// eliminate duplicate vertices
				for (int i = 0; i < v - 1; i++)
				{
					for (int j = i + 1; j < v; j++)
					{
						if (Prototype.Mesh.Vertices[i] == Prototype.Mesh.Vertices[j])
						{
							for (int k = 0; k < f; k++)
							{
								for (int h = 0; h < Prototype.Mesh.Faces[k].Vertices.Length; h++)
								{
									if (Prototype.Mesh.Faces[k].Vertices[h].Index == j)
									{
										Prototype.Mesh.Faces[k].Vertices[h].Index = (ushort)i;
									}
									else if (Prototype.Mesh.Faces[k].Vertices[h].Index > j)
									{
										Prototype.Mesh.Faces[k].Vertices[h].Index--;
									}
								}
							}
							for (int k = j; k < v - 1; k++)
							{
								Prototype.Mesh.Vertices[k] = Prototype.Mesh.Vertices[k + 1];
							}
							v--;
							j--;
						}
					}
				}
			}
			// structure optimization
			// Trangularize all polygons and quads into triangles
			for (int i = 0; i < f; ++i)
			{
				byte type = (byte) (Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask);
				// Only transform quads and polygons
				if (type == World.MeshFace.FaceTypeQuads || type == World.MeshFace.FaceTypePolygon)
				{
					int staring_vertex_count = Prototype.Mesh.Faces[i].Vertices.Length;

					// One triange for the first three points, then one for each vertex
					// Wind order is maintained.
					// Ex: 0, 1, 2; 0, 2, 3; 0, 3, 4; 0, 4, 5; 
					int tri_count = (staring_vertex_count - 2);
					int vertex_count = tri_count * 3;

					// Copy old array for use as we work
					World.MeshFaceVertex[] original_poly = (World.MeshFaceVertex[]) Prototype.Mesh.Faces[i].Vertices.Clone();

					// Resize new array
					Array.Resize(ref Prototype.Mesh.Faces[i].Vertices, vertex_count);

					// Reference to output vertices
					World.MeshFaceVertex[] out_verts = Prototype.Mesh.Faces[i].Vertices;

					// Triangularize
					for (int tri_index = 0, vert_index = 0, old_vert = 2; tri_index < tri_count; ++tri_index, ++old_vert)
					{
						// First vertex is always the 0th
						out_verts[vert_index] = original_poly[0];
						vert_index += 1;

						// Second vertex is one behind the current working vertex
						out_verts[vert_index] = original_poly[old_vert - 1];
						vert_index += 1;

						// Third vertex is current working vertex
						out_verts[vert_index] = original_poly[old_vert];
						vert_index += 1;
					}

					// Mark as triangle
					unchecked
					{
						Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
						Prototype.Mesh.Faces[i].Flags |= World.MeshFace.FaceTypeTriangles;
					}
				}
			}

			// decomposit TRIANGLES and QUADS
			for (int i = 0; i < f; i++)
			{
				int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
				int face_count = 0;
				byte face_bit = 0;
				if (type == World.MeshFace.FaceTypeTriangles)
				{
					face_count = 3;
					face_bit = World.MeshFace.FaceTypeTriangles;
				}
				else if (type == World.MeshFace.FaceTypeQuads)
				{
					face_count = 4;
					face_bit = World.MeshFace.FaceTypeQuads;
				}
				if (face_count == 3 || face_count == 4)
				{
					if (Prototype.Mesh.Faces[i].Vertices.Length > face_count)
					{
						int n = (Prototype.Mesh.Faces[i].Vertices.Length - face_count) / face_count;
						while (f + n > Prototype.Mesh.Faces.Length)
						{
							Array.Resize<World.MeshFace>(ref Prototype.Mesh.Faces, Prototype.Mesh.Faces.Length << 1);
						}
						for (int j = 0; j < n; j++)
						{
							Prototype.Mesh.Faces[f + j].Vertices = new World.MeshFaceVertex[face_count];
							for (int k = 0; k < face_count; k++)
							{
								Prototype.Mesh.Faces[f + j].Vertices[k] = Prototype.Mesh.Faces[i].Vertices[face_count + face_count * j + k];
							}
							Prototype.Mesh.Faces[f + j].Material = Prototype.Mesh.Faces[i].Material;
							Prototype.Mesh.Faces[f + j].Flags = Prototype.Mesh.Faces[i].Flags;
							unchecked
							{
								Prototype.Mesh.Faces[i].Flags &= (byte)~World.MeshFace.FaceTypeMask;
								Prototype.Mesh.Faces[i].Flags |= face_bit;
							}
						}
						Array.Resize<World.MeshFaceVertex>(ref Prototype.Mesh.Faces[i].Vertices, face_count);
						f += n;
					}
				}
			}

			// Squish faces that have the same material.
			{
				bool[] can_merge = new bool[f];
				for (int i = 0; i < f - 1; ++i)
				{
					int merge_vertices = 0;

					// Type of current face
					int type = Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask;
					int face = Prototype.Mesh.Faces[i].Flags & World.MeshFace.Face2Mask;

					// Find faces that can be merged
					for (int j = i + 1; j < f; ++j)
					{
						int type2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.FaceTypeMask;
						int face2 = Prototype.Mesh.Faces[j].Flags & World.MeshFace.Face2Mask;

						// Conditions for face merger
						bool mergeable = (type == World.MeshFace.FaceTypeTriangles) &&
										 (type == type2) &&
										 (face == face2) &&
										 (Prototype.Mesh.Faces[i].Material == Prototype.Mesh.Faces[j].Material);

						can_merge[j] = mergeable;
						merge_vertices += mergeable ? Prototype.Mesh.Faces[j].Vertices.Length : 0;
					}

					if (merge_vertices == 0)
					{
						continue;
					}

					// Current end of array index
					int last_vertex_it = Prototype.Mesh.Faces[i].Vertices.Length;

					// Resize current face's vertices to have enough room
					Array.Resize(ref Prototype.Mesh.Faces[i].Vertices, last_vertex_it + merge_vertices);

					// Merge faces
					for (int j = i + 1; j < f; ++j)
					{
						if (can_merge[j])
						{
							// Copy vertices
							Prototype.Mesh.Faces[j].Vertices.CopyTo(Prototype.Mesh.Faces[i].Vertices, last_vertex_it);

							// Adjust index
							last_vertex_it += Prototype.Mesh.Faces[j].Vertices.Length;
						}
					}

					// Remove now unused faces
					int jump = 0;
					for (int j = i + 1; j < f; ++j)
					{
						if (can_merge[j])
						{
							jump += 1;
						}
						else if (jump > 0)
						{
							Prototype.Mesh.Faces[j - jump] = Prototype.Mesh.Faces[j];
						}
					}
					// Remove faces removed from face count
					f -= jump;
				}
			}
			// finalize arrays
			if (v != Prototype.Mesh.Vertices.Length) {
				Array.Resize<World.Vertex>(ref Prototype.Mesh.Vertices, v);
			}
			if (m != Prototype.Mesh.Materials.Length) {
				Array.Resize<World.MeshMaterial>(ref Prototype.Mesh.Materials, m);
			}
			if (f != Prototype.Mesh.Faces.Length) {
				Array.Resize<World.MeshFace>(ref Prototype.Mesh.Faces, f);
			}
		}

		// join objects
		internal static void JoinObjects(ref StaticObject Base, StaticObject Add)
		{
			if (Base == null & Add == null) {
				return;
			}
			if (Base == null) {
				Base = CloneObject(Add);
			} else if (Add != null) {
				int mf = Base.Mesh.Faces.Length;
				int mm = Base.Mesh.Materials.Length;
				int mv = Base.Mesh.Vertices.Length;
				Array.Resize<World.MeshFace>(ref Base.Mesh.Faces, mf + Add.Mesh.Faces.Length);
				Array.Resize<World.MeshMaterial>(ref Base.Mesh.Materials, mm + Add.Mesh.Materials.Length);
				Array.Resize<World.Vertex>(ref Base.Mesh.Vertices, mv + Add.Mesh.Vertices.Length);
				for (int i = 0; i < Add.Mesh.Faces.Length; i++) {
					Base.Mesh.Faces[mf + i] = Add.Mesh.Faces[i];
					for (int j = 0; j < Base.Mesh.Faces[mf + i].Vertices.Length; j++) {
						Base.Mesh.Faces[mf + i].Vertices[j].Index += (ushort)mv;
					}
					Base.Mesh.Faces[mf + i].Material += (ushort)mm;
				}
				for (int i = 0; i < Add.Mesh.Materials.Length; i++) {
					Base.Mesh.Materials[mm + i] = Add.Mesh.Materials[i];
				}
				for (int i = 0; i < Add.Mesh.Vertices.Length; i++) {
					Base.Mesh.Vertices[mv + i] = Add.Mesh.Vertices[i];
				}
			}
		}

		// create object
		internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
		}
		internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			if (Prototype is StaticObject) {
				StaticObject s = (StaticObject)Prototype;
				CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			} else if (Prototype is AnimatedObjectCollection) {
				AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
				CreateAnimatedWorldObjects(a.Objects, Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			}
		}

		// create static object
		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition) {
			return CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
		}
		internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials) {
			if (Prototype == null)
			{
				return -1;
			}
			int a = ObjectsUsed;
			if (a >= Objects.Length) {
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			ApplyStaticObjectData(ref Objects[a], Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
			for (int i = 0; i < Prototype.Mesh.Faces.Length; i++) {
				switch (Prototype.Mesh.Faces[i].Flags & World.MeshFace.FaceTypeMask) {
					case World.MeshFace.FaceTypeTriangles:
						Game.InfoTotalTriangles++;
						break;
					case World.MeshFace.FaceTypeTriangleStrip:
						Game.InfoTotalTriangleStrip++;
						break;
					case World.MeshFace.FaceTypeQuads:
						Game.InfoTotalQuads++;
						break;
					case World.MeshFace.FaceTypeQuadStrip:
						Game.InfoTotalQuadStrip++;
						break;
					case World.MeshFace.FaceTypePolygon:
						Game.InfoTotalPolygon++;
						break;
				}
			}
			ObjectsUsed++;
			return a;
		}
		internal static void ApplyStaticObjectData(ref StaticObject Object, StaticObject Prototype, Vector3 Position, World.Transformation BaseTransformation, World.Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
		{
			//Object is not actually overwritten by this call
			Object = new StaticObject
			{
				StartingDistance = float.MaxValue,
				EndingDistance = float.MinValue,
				Mesh = {Vertices = new World.Vertex[Prototype.Mesh.Vertices.Length]}
			};
			// vertices
			for (int j = 0; j < Prototype.Mesh.Vertices.Length; j++) {
				Object.Mesh.Vertices[j] = Prototype.Mesh.Vertices[j];
				if (AccurateObjectDisposal) {
					World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, AuxTransformation);
					if (Object.Mesh.Vertices[j].Coordinates.Z < Object.StartingDistance) {
						Object.StartingDistance = (float)Object.Mesh.Vertices[j].Coordinates.Z;
					}
					if (Object.Mesh.Vertices[j].Coordinates.Z > Object.EndingDistance) {
						Object.EndingDistance = (float)Object.Mesh.Vertices[j].Coordinates.Z;
					}
					Object.Mesh.Vertices[j].Coordinates = Prototype.Mesh.Vertices[j].Coordinates;
				}
				World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, AuxTransformation);
				World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, BaseTransformation);
				Object.Mesh.Vertices[j].Coordinates.X += Position.X;
				Object.Mesh.Vertices[j].Coordinates.Y += Position.Y;
				Object.Mesh.Vertices[j].Coordinates.Z += Position.Z;
			}
			if (AccurateObjectDisposal) {
				Object.StartingDistance += (float)AccurateObjectDisposalZOffset;
				Object.EndingDistance += (float)AccurateObjectDisposalZOffset;
			}
			// faces
			Object.Mesh.Faces = new World.MeshFace[Prototype.Mesh.Faces.Length];
			for (int j = 0; j < Prototype.Mesh.Faces.Length; j++) {
				Object.Mesh.Faces[j].Flags = Prototype.Mesh.Faces[j].Flags;
				Object.Mesh.Faces[j].Material = Prototype.Mesh.Faces[j].Material;
				Object.Mesh.Faces[j].Vertices = new World.MeshFaceVertex[Prototype.Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
					Object.Mesh.Faces[j].Vertices[k] = Prototype.Mesh.Faces[j].Vertices[k];
					double nx = Object.Mesh.Faces[j].Vertices[k].Normal.X;
					double ny = Object.Mesh.Faces[j].Vertices[k].Normal.Y;
					double nz = Object.Mesh.Faces[j].Vertices[k].Normal.Z;
					if (nx * nx + ny * ny + nz * nz != 0.0) {
						World.Rotate(ref Object.Mesh.Faces[j].Vertices[k].Normal.X, ref Object.Mesh.Faces[j].Vertices[k].Normal.Y, ref Object.Mesh.Faces[j].Vertices[k].Normal.Z, AuxTransformation);
						World.Rotate(ref Object.Mesh.Faces[j].Vertices[k].Normal.X, ref Object.Mesh.Faces[j].Vertices[k].Normal.Y, ref Object.Mesh.Faces[j].Vertices[k].Normal.Z, BaseTransformation);
					}
				}
			}
			// materials
			Object.Mesh.Materials = new World.MeshMaterial[Prototype.Mesh.Materials.Length];
			for (int j = 0; j < Prototype.Mesh.Materials.Length; j++) {
				Object.Mesh.Materials[j] = Prototype.Mesh.Materials[j];
				Object.Mesh.Materials[j].Color.R = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.R * Brightness);
				Object.Mesh.Materials[j].Color.G = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.G * Brightness);
				Object.Mesh.Materials[j].Color.B = (byte)Math.Round((double)Prototype.Mesh.Materials[j].Color.B * Brightness);
			}
			const double minBlockLength = 20.0;
			if (BlockLength < minBlockLength) {
				BlockLength = BlockLength * Math.Ceiling(minBlockLength / BlockLength);
			}
			if (AccurateObjectDisposal) {
				Object.StartingDistance += (float)TrackPosition;
				Object.EndingDistance += (float)TrackPosition;
				double z = BlockLength * Math.Floor(TrackPosition / BlockLength);
				StartingDistance = Math.Min(z - BlockLength, (double)Object.StartingDistance);
				EndingDistance = Math.Max(z + 2.0 * BlockLength, (double)Object.EndingDistance);
				Object.StartingDistance = (float)(BlockLength * Math.Floor(StartingDistance / BlockLength));
				Object.EndingDistance = (float)(BlockLength * Math.Ceiling(EndingDistance / BlockLength));
			} else {
				Object.StartingDistance = (float)StartingDistance;
				Object.EndingDistance = (float)EndingDistance;
			}
			if (BlockLength != 0.0) {
				checked {
					Object.GroupIndex = (short)Mod(Math.Floor(Object.StartingDistance / BlockLength), Math.Ceiling(Interface.CurrentOptions.ViewingDistance / BlockLength));
				}
			}
		}
		
		private static double Mod(double a, double b) {
			return a - b * Math.Floor(a / b);
		}

		// create dynamic object
		internal static int CreateDynamicObject() {
			int a = ObjectsUsed;
			if (a >= Objects.Length) {
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			Objects[a] = new StaticObject
			{
				Mesh =
				{
					Faces = new World.MeshFace[] {},
					Materials = new World.MeshMaterial[] {},
					Vertices = new World.Vertex[] {}
				},
				Dynamic = true
			};
			ObjectsUsed++;
			return a;
		}

		/// <summary>Creates a clone of the specified object.</summary>
		/// <param name="Prototype">The prototype.</param>
		internal static StaticObject CloneObject(StaticObject Prototype) {
			if (Prototype == null) return null;
			return CloneObject(Prototype, null, null);
		}
		/// <summary>Creates a clone of the specified object.</summary>
		/// <param name="Prototype">The prototype.</param>
		/// <param name="DaytimeTexture">The replacement daytime texture, or a null reference to keep the texture of the prototype.</param>
		/// <param name="NighttimeTexture">The replacement nighttime texture, or a null reference to keep the texture of the prototype.</param>
		/// <returns></returns>
		internal static StaticObject CloneObject(StaticObject Prototype, Textures.Texture DaytimeTexture, Textures.Texture NighttimeTexture) {
			if (Prototype == null) return null;
			StaticObject Result = new StaticObject
			{
				StartingDistance = Prototype.StartingDistance,
				EndingDistance = Prototype.EndingDistance,
				Dynamic = Prototype.Dynamic,
				Mesh = {Vertices = new World.Vertex[Prototype.Mesh.Vertices.Length]}
			};
			// vertices
			for (int j = 0; j < Prototype.Mesh.Vertices.Length; j++) {
				Result.Mesh.Vertices[j] = Prototype.Mesh.Vertices[j];
			}
			// faces
			Result.Mesh.Faces = new World.MeshFace[Prototype.Mesh.Faces.Length];
			for (int j = 0; j < Prototype.Mesh.Faces.Length; j++) {
				Result.Mesh.Faces[j].Flags = Prototype.Mesh.Faces[j].Flags;
				Result.Mesh.Faces[j].Material = Prototype.Mesh.Faces[j].Material;
				Result.Mesh.Faces[j].Vertices = new World.MeshFaceVertex[Prototype.Mesh.Faces[j].Vertices.Length];
				for (int k = 0; k < Prototype.Mesh.Faces[j].Vertices.Length; k++) {
					Result.Mesh.Faces[j].Vertices[k] = Prototype.Mesh.Faces[j].Vertices[k];
				}
			}
			// materials
			Result.Mesh.Materials = new World.MeshMaterial[Prototype.Mesh.Materials.Length];
			for (int j = 0; j < Prototype.Mesh.Materials.Length; j++) {
				Result.Mesh.Materials[j] = Prototype.Mesh.Materials[j];
				if (DaytimeTexture != null) {
					Result.Mesh.Materials[j].DaytimeTexture = DaytimeTexture;
				} else {
					Result.Mesh.Materials[j].DaytimeTexture = Prototype.Mesh.Materials[j].DaytimeTexture;
				}
				if (DaytimeTexture != null) {
					Result.Mesh.Materials[j].NighttimeTexture = NighttimeTexture;
				} else {
					Result.Mesh.Materials[j].NighttimeTexture = Prototype.Mesh.Materials[j].NighttimeTexture;
				}
			}
			return Result;
		}

		// finish creating objects
		internal static void FinishCreatingObjects() {
			Array.Resize<StaticObject>(ref Objects, ObjectsUsed);
			Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjectsUsed);
		}

		// initialize visibility
		internal static void InitializeVisibility() {
			// sort objects
			ObjectsSortedByStart = new int[ObjectsUsed];
			ObjectsSortedByEnd = new int[ObjectsUsed];
			double[] a = new double[ObjectsUsed];
			double[] b = new double[ObjectsUsed];
			int n = 0;
			for (int i = 0; i < ObjectsUsed; i++) {
				if (!Objects[i].Dynamic) {
					ObjectsSortedByStart[n] = i;
					ObjectsSortedByEnd[n] = i;
					a[n] = Objects[i].StartingDistance;
					b[n] = Objects[i].EndingDistance;
					n++;
				}
			}
			Array.Resize<int>(ref ObjectsSortedByStart, n);
			Array.Resize<int>(ref ObjectsSortedByEnd, n);
			Array.Resize<double>(ref a, n);
			Array.Resize<double>(ref b, n);
			Array.Sort<double, int>(a, ObjectsSortedByStart);
			Array.Sort<double, int>(b, ObjectsSortedByEnd);
			ObjectsSortedByStartPointer = 0;
			ObjectsSortedByEndPointer = 0;
			// initial visiblity
			double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z;
			for (int i = 0; i < ObjectsUsed; i++) {
				if (!Objects[i].Dynamic) {
					if (Objects[i].StartingDistance <= p + World.ForwardViewingDistance & Objects[i].EndingDistance >= p - World.BackwardViewingDistance) {
						Renderer.ShowObject(i, Renderer.ObjectType.Static);
					}
				}
			}
		}

		// update visibility
		internal static void UpdateVisibility(double TrackPosition, bool ViewingDistanceChanged) {
			if (ViewingDistanceChanged) {
				UpdateVisibility(TrackPosition);
				UpdateVisibility(TrackPosition - 0.001);
				UpdateVisibility(TrackPosition + 0.001);
				UpdateVisibility(TrackPosition);
			} else {
				UpdateVisibility(TrackPosition);
			}
		}
		internal static void UpdateVisibility(double TrackPosition) {
			double d = TrackPosition - LastUpdatedTrackPosition;
			int n = ObjectsSortedByStart.Length;
			double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z;
			if (d < 0.0) {
				if (ObjectsSortedByStartPointer >= n) ObjectsSortedByStartPointer = n - 1;
				if (ObjectsSortedByEndPointer >= n) ObjectsSortedByEndPointer = n - 1;
				// dispose
				while (ObjectsSortedByStartPointer >= 0) {
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance > p + World.ForwardViewingDistance) {
						Renderer.HideObject(o);
						ObjectsSortedByStartPointer--;
					} else {
						break;
					}
				}
				// introduce
				while (ObjectsSortedByEndPointer >= 0) {
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance >= p - World.BackwardViewingDistance) {
						if (Objects[o].StartingDistance <= p + World.ForwardViewingDistance) {
							Renderer.ShowObject(o, Renderer.ObjectType.Static);
						}
						ObjectsSortedByEndPointer--;
					} else {
						break;
					}
				}
			} else if (d > 0.0) {
				if (ObjectsSortedByStartPointer < 0) ObjectsSortedByStartPointer = 0;
				if (ObjectsSortedByEndPointer < 0) ObjectsSortedByEndPointer = 0;
				// dispose
				while (ObjectsSortedByEndPointer < n) {
					int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
					if (Objects[o].EndingDistance < p - World.BackwardViewingDistance) {
						Renderer.HideObject(o);
						ObjectsSortedByEndPointer++;
					} else {
						break;
					}
				}
				// introduce
				while (ObjectsSortedByStartPointer < n) {
					int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
					if (Objects[o].StartingDistance <= p + World.ForwardViewingDistance) {
						if (Objects[o].EndingDistance >= p - World.BackwardViewingDistance) {
							Renderer.ShowObject(o, Renderer.ObjectType.Static);
						}
						ObjectsSortedByStartPointer++;
					} else {
						break;
					}
				}
			}
			LastUpdatedTrackPosition = TrackPosition;
		}

	}
}