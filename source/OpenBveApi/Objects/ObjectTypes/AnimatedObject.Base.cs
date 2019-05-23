using OpenBveApi.FunctionScripting;
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
		/// <summary>The object index</summary>
		public int ObjectIndex;

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

		
	}
}
