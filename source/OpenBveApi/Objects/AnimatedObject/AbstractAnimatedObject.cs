using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>Represents an abstract animated object to be fully implemented by the host application</summary>
	public abstract class AbstractAnimatedObject
	{
		/// <summary>A reference to the callback for the host application</summary>
		public Hosts.HostInterface CurrentHost;
		/// <summary>The available states to be switched through if using the StateFunction</summary>
		public AnimatedObjectState[] States;
		/// <summary>The function script controlling state switching</summary>
		public FunctionScript StateFunction;
		/// <summary>The index of the current state</summary>
		public int CurrentState;
		/// <summary>A 3D vector describing the translation performed by the TranslateXFunction</summary>
		public Vector3 TranslateXDirection;
		/// <summary>A 3D vector describing the translation performed by the TranslateYFunction</summary>
		public Vector3 TranslateYDirection;
		/// <summary>A 3D vector describing the translation performed by the TranslateZFunction</summary>
		public Vector3 TranslateZDirection;
		/// <summary>The function script controling translation in the X vector</summary>
		public FunctionScript TranslateXFunction;
		/// <summary>The function script controling translation in the Y vector</summary>
		public FunctionScript TranslateYFunction;
		/// <summary>The function script controling translation in the Z vector</summary>
		public FunctionScript TranslateZFunction;
		/// <summary>A 3D vector describing the rotation performed by the RotateXFunction</summary>
		public Vector3 RotateXDirection;
		/// <summary>A 3D vector describing the rotation performed by the RotateYFunction</summary>
		public Vector3 RotateYDirection;
		/// <summary>A 3D vector describing the rotation performed by the RotateZFunction</summary>
		public Vector3 RotateZDirection;
		/// <summary>The function script controling rotation in the X vector</summary>
		public FunctionScript RotateXFunction;
		/// <summary>The function script controling rotation in the Y vector</summary>
		public FunctionScript RotateYFunction;
		/// <summary>The function script controling rotation in the Z vector</summary>
		public FunctionScript RotateZFunction;
		/// <summary>The damping applied to the X vector</summary>
		public Damping RotateXDamping;
		/// <summary>The damping applied to the Y vector</summary>
		public Damping RotateYDamping;
		/// <summary>The damping applied to the Z vector</summary>
		public Damping RotateZDamping;
		/// <summary>A 2D vector describing the texture shift direction performed by TextureShiftXFunction</summary>
		public Vector2 TextureShiftXDirection;
		/// <summary>A 2D vector describing the texture shift direction performed by TextureShiftYFunction</summary>
		public Vector2 TextureShiftYDirection;
		/// <summary>The function script controlling texture shifting in the X axis</summary>
		public FunctionScript TextureShiftXFunction;
		/// <summary>The function script controlling texture shifting in the Y axis</summary>
		public FunctionScript TextureShiftYFunction;
		/// <summary>Whether the LED function uses clockwise vertex winding</summary>
		public bool LEDClockwiseWinding;
		/// <summary>The starting angle for the LEDFunction</summary>
		public double LEDInitialAngle;
		/// <summary>The final angle for the LEDFunction</summary>
		public double LEDLastAngle;
		/// <summary>If LEDFunction is used, an array of five vectors representing the bottom-left, up-left, up-right, bottom-right and center coordinates of the LED square, or a null reference otherwise.</summary>
		public Vector3[] LEDVectors;
		/// <summary>The function script controlling LED</summary>
		public FunctionScript LEDFunction;
		/// <summary>The refresh rate in seconds</summary>
		public double RefreshRate;
		/// <summary>The absolute number of seconds since the object was last updated</summary>
		public double SecondsSinceLastUpdate;
		/// <summary>The index of the renderer object</summary>
		public int ObjectIndex;
		/// <summary>The absolute path to the script file to be evaluated when TranslateXScript is called</summary>
		public string TranslateXScriptFile;
		/// <summary>The animation script for translation in the X axis</summary>
		public AnimationScript TranslateXAnimationScript;
		/// <summary>The absolute path to the script file to be evaluated when TranslateYScript is called</summary>
		public AnimationScript TranslateYAnimationScript;
		/// <summary>The animation script for translation in the Y axis</summary>
		public string TranslateYScriptFile;
		/// <summary>The animation script for translation in the Z axis</summary>
		public AnimationScript TranslateZAnimationScript;
		/// <summary>The absolute path to the script file to be evaluated when TranslateZScript is called</summary>
		public string TranslateZScriptFile;
		/// <summary>The function script controlling movement along the Rail0 axis</summary>
		public FunctionScript TrackFollowerFunction;
		/// <summary>The position of the front axle</summary>
		public double FrontAxlePosition = 1;
		/// <summary>The position of the rear axle</summary>
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
			return true;
		}
	}
}
