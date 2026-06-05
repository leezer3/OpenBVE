using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>The type of the dynamic light source</summary>
	public enum SceneLightType
	{
		/// <summary>Point light source emitting light in all directions</summary>
		Point = 0,
		/// <summary>Spot light source emitting a cone of light in a specific direction</summary>
		Spot = 1
	}

	/// <summary>Represents a dynamic light source in the rendering scene</summary>
	public class SceneLight
	{
		/// <summary>The light source type</summary>
		public SceneLightType Type;
		/// <summary>Position in world coordinates or relative to parent object</summary>
		public Vector3 Position;
		/// <summary>Direction vector for spot lights</summary>
		public Vector3 Direction;
		/// <summary>Light color</summary>
		public Color128 Color;
		/// <summary>Maximum range of the light in meters</summary>
		public float Range;
		/// <summary>Squared range of the light</summary>
		public float RangeSquared;
		/// <summary>Linear attenuation factor</summary>
		public float AttenuationLinear;
		/// <summary>Quadratic attenuation factor</summary>
		public float AttenuationQuadratic;
		/// <summary>Spotlight cutoff cosine (pre-calculated on CPU)</summary>
		public float SpotCutoff;
		/// <summary>Spotlight exponent concentration factor</summary>
		public float SpotExponent;
		/// <summary>Whether to show a visual helper for this light</summary>
		public bool Visual;
 
 		/// <summary>Creates a default scene light</summary>
 		public SceneLight()
 		{
 			Type = SceneLightType.Point;
 			Position = Vector3.Zero;
 			Direction = Vector3.Forward;
 			Color = Color128.White;
 			Range = 10.0f;
 			RangeSquared = 100.0f;
 			AttenuationLinear = 1.0f;
 			AttenuationQuadratic = 0.0f;
 			SpotCutoff = 0.5f; // cos(45 deg)
 			SpotExponent = 1.0f;
			Visual = false;
 		}
 
 		/// <summary>Clones the light source</summary>
 		public SceneLight Clone()
 		{
 			return new SceneLight
 			{
 				Type = this.Type,
 				Position = this.Position,
 				Direction = this.Direction,
 				Color = this.Color,
 				Range = this.Range,
 				RangeSquared = this.RangeSquared,
 				AttenuationLinear = this.AttenuationLinear,
 				AttenuationQuadratic = this.AttenuationQuadratic,
 				SpotCutoff = this.SpotCutoff,
 				SpotExponent = this.SpotExponent,
				Visual = this.Visual
 			};
 		}
	}
}
