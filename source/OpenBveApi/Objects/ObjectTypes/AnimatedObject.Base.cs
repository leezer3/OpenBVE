using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>The base type for an animated object</summary>
	public abstract class AnimatedObjectBase
	{
		/// <summary>The array of states</summary>
		public AnimatedObjectState[] States;
		/// <summary>The function script controlling state changes</summary>
		public FunctionScript StateFunction;
		/// <summary>The index of the current state</summary>
		public int CurrentState;
		/// <summary>A 3D vector describing the direction taken when TranslateXFunction is called</summary>
		public Vector3 TranslateXDirection;
		/// <summary>A 3D vector describing the direction taken when TranslateXYFunction is called</summary>
		public Vector3 TranslateYDirection;
		/// <summary>A 3D vector describing the direction taken when TranslateZFunction is called</summary>
		public Vector3 TranslateZDirection;
		/// <summary>The function script controlling translation in the X direction</summary>
		/// <remarks>X is an arbitrary 3D direction, nominally 1,0,0</remarks>
		public FunctionScript TranslateXFunction;
		/// <summary>The function script controlling translation in the Y direction</summary>
		/// <remarks>Y is an arbitrary 3D direction, nominally 0,1,0</remarks>
		public FunctionScript TranslateYFunction;
		/// <summary>The function script controlling translation in the Z direction</summary>
		/// <remarks>Z is an arbitrary 3D direction, nominally 0,0,1</remarks>
		public FunctionScript TranslateZFunction;
		/// <summary>A 3D vector describing the rotation performed when RotateXFunction is called</summary>
		public Vector3 RotateXDirection;
		/// <summary>A 3D vector describing the rotation performed when RotateYFunction is called</summary>
		public Vector3 RotateYDirection;
		/// <summary>A 3D vector describing the rotation performed when RotateZFunction is called</summary>
		public Vector3 RotateZDirection;
		/// <summary>The function script controlling translation in the X direction</summary>
		/// <remarks>X is an arbitrary 3D direction, nominally 1,0,0</remarks>
		public FunctionScript RotateXFunction;
		/// <summary>The function script controlling translation in the Y direction</summary>
		/// <remarks>Y is an arbitrary 3D direction, nominally 0,1,0</remarks>
		public FunctionScript RotateYFunction;
		/// <summary>The function script controlling translation in the Z direction</summary>
		/// <remarks>Z is an arbitrary 3D direction, nominally 0,0,1</remarks>
		public FunctionScript RotateZFunction;
		/// <summary>The damping (if any) to perform when RotateXFunction is called</summary>
		public Damping RotateXDamping;
		/// <summary>The damping (if any) to perform when RotateYFunction is called</summary>
		public Damping RotateYDamping;
		/// <summary>The damping (if any) to perform when RotateZFunction is called</summary>
		public Damping RotateZDamping;
		/// <summary>A 2D vector describing the texture shifting performed when TextureShiftXFunction is called</summary>
		public Vector2 TextureShiftXDirection;
		/// <summary>A 2D vector describing the texture shifting performed when TextureShiftYFunction is called</summary>
		public Vector2 TextureShiftYDirection;
		/// <summary>The function script controlling texture shifting in the X direction</summary>
		/// <remarks>X is an arbitrary 2D direction, nominally 1,0</remarks>
		public FunctionScript TextureShiftXFunction;
		/// <summary>The function script controlling texture shifting in the Y direction</summary>
		/// <remarks>X is an arbitrary 2D direction, nominally 0,1</remarks>
		public FunctionScript TextureShiftYFunction;
		/// <summary>If the LED function is used, this controls whether the winding is clockwise or anti-clockwise</summary>
		public bool LEDClockwiseWinding;
		/// <summary>The initial angle of the LED function</summary>
		public double LEDInitialAngle;
		/// <summary>The final angle of the LED function</summary>
		public double LEDLastAngle;
		/// <summary>If LEDFunction is used, an array of five vectors representing the bottom-left, up-left, up-right, bottom-right and center coordinates of the LED square, or a null reference otherwise.</summary>
		public Vector3[] LEDVectors;
		/// <summary>The function script controlling the LED square</summary>
		public FunctionScript LEDFunction;
		/// <summary>The refresh rate in seconds</summary>
		public double RefreshRate;
		/// <summary>The time since the last update of this object</summary>
		public double SecondsSinceLastUpdate;
		
		//This section holds script files executed by CS-Script
		/// <summary>The absolute path to the script file to be evaluated when TranslateXScript is called</summary>
		public string TranslateXScriptFile;
		/// <summary>The actual script interface</summary>
		public AnimationScript TranslateXAnimationScript;
		/// <summary>The absolute path to the script file to be evaluated when TranslateYScript is called</summary>
		public AnimationScript TranslateYAnimationScript;
		/// <summary>The actual script interface</summary>
		public string TranslateYScriptFile;
		/// <summary>The absolute path to the script file to be evaluated when TranslateZScript is called</summary>
		public AnimationScript TranslateZAnimationScript;
		/// <summary>The actual script interface</summary>
		public string TranslateZScriptFile;
		/// <summary>The function script controlling movement along the track</summary>
		public FunctionScript TrackFollowerFunction;
		/// <summary>The front axle position if TrackFollowerFunction is used</summary>
		public double FrontAxlePosition = 1;
		/// <summary>The rear axle position if TrackFollowerFunction is used</summary>
		public double RearAxlePosition = -1;
		/// <summary>Holds a reference to the internal static object used for display</summary>
		/// <remarks>This is a fully transformed deep copy of the current state</remarks>
		public StaticObject internalObject;
		/// <summary>Holds a reference to the host interface of the current application</summary>
		public Hosts.HostInterface currentHost;

		/// <summary>Checks whether this object contains any functions</summary>
		public bool IsFreeOfFunctions()
		{
			if (this.StateFunction != null) return false;
			if (this.TrackFollowerFunction != null) return false;
			if (this.TranslateXFunction != null | this.TranslateYFunction != null | this.TranslateZFunction != null) return false;
			if (this.RotateXFunction != null | this.RotateYFunction != null | this.RotateZFunction != null) return false;
			if (this.TextureShiftXFunction != null | this.TextureShiftYFunction != null) return false;
			if (this.LEDFunction != null) return false;
			if (this.TranslateXScriptFile != null | this.TranslateYScriptFile != null | this.TranslateZScriptFile != null) return false;
			if (this.TrackFollowerFunction != null) return false;
			return true;
		}
		/// <summary>Initialises the object</summary>
		/// <param name="StateIndex">The state to show</param>
		/// <param name="Overlay">Whether this object is in overlay mode</param>
		/// <param name="Show">Whether the object should be shown immediately on initialisation</param>
		public void Initialize(int StateIndex, bool Overlay, bool Show)
		{
			currentHost.HideObject(ref internalObject);
			int t = StateIndex;
			if (t >= 0 && States[t].Object != null)
			{
				int m = States[t].Object.Mesh.Vertices.Length;
				internalObject.Mesh.Vertices = new VertexTemplate[m];
				for (int k = 0; k < m; k++)
				{
					if (States[t].Object.Mesh.Vertices[k] is ColoredVertex)
					{
						internalObject.Mesh.Vertices[k] = new ColoredVertex((ColoredVertex)States[t].Object.Mesh.Vertices[k]);
					}
					else
					{
						internalObject.Mesh.Vertices[k] = new Vertex((Vertex)States[t].Object.Mesh.Vertices[k]);
					}
						
				}
				m = States[t].Object.Mesh.Faces.Length;
				internalObject.Mesh.Faces = new MeshFace[m];
				for (int k = 0; k < m; k++)
				{
					internalObject.Mesh.Faces[k].Flags = States[t].Object.Mesh.Faces[k].Flags;
					internalObject.Mesh.Faces[k].Material = States[t].Object.Mesh.Faces[k].Material;
					int o = States[t].Object.Mesh.Faces[k].Vertices.Length;
					internalObject.Mesh.Faces[k].Vertices = new MeshFaceVertex[o];
					for (int h = 0; h < o; h++)
					{
						internalObject.Mesh.Faces[k].Vertices[h] = States[t].Object.Mesh.Faces[k].Vertices[h];
					}
				}
				internalObject.Mesh.Materials = States[t].Object.Mesh.Materials;
			}
			else
			{
				internalObject = new StaticObject(currentHost);
			}
			CurrentState = StateIndex;
			if (Show)
			{
				if (Overlay)
				{
					currentHost.ShowObject(internalObject, ObjectType.Overlay);
				}
				else
				{
					currentHost.ShowObject(internalObject, ObjectType.Dynamic);
				}
			}
		}
	}
}
