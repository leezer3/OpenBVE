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
		Spot = 1,
		/// <summary>Area light source emitting light from a 2D surface</summary>
		Area = 2
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
		/// <summary>Spotlight cutoff cosine (pre-calculated on CPU)</summary>
		public float SpotCutoff;
		/// <summary>Whether to show a visual helper for this light</summary>
		public bool Visual;
		/// <summary>Power of the light in Watts</summary>
		public float Power;
		/// <summary>Exposure multiplier</summary>
		public float Exposure;
		/// <summary>Whether to normalize power</summary>
		public bool NormalizeCone;
		/// <summary>Source radius (size) of the light source</summary>
		public float Radius;
		/// <summary>Whether to use soft falloff</summary>
		public bool SoftFalloff;
		/// <summary>Spot angle in degrees</summary>
		public float Angle;
		/// <summary>Spot softness factor (0 to 1)</summary>
		public float Softness;
		/// <summary>Whether to show the cone helper</summary>
		public bool ShowCone;
		/// <summary>Width and Height of the area light source</summary>
		public Vector2 AreaSize;
 
 		/// <summary>Creates a default scene light</summary>
 		public SceneLight()
 		{
 			Type = SceneLightType.Point;
 			Position = Vector3.Zero;
 			Direction = Vector3.Forward;
 			Color = Color128.White;
 			Range = 10.0f;
 			RangeSquared = 100.0f;
 			SpotCutoff = 0.5f; // cos(45 deg)
			Visual = false;
			Power = 12.5663706f; // 4 * PI (so default multiplier is 1.0)
			Exposure = 0.0f;
			NormalizeCone = true;
			Radius = 0.0f;
			SoftFalloff = true;
			Angle = 45.0f;
			Softness = 1.0f;
			ShowCone = true;
			AreaSize = new Vector2(1.0f, 1.0f);
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
 				SpotCutoff = this.SpotCutoff,
				Visual = this.Visual,
				Power = this.Power,
				Exposure = this.Exposure,
				NormalizeCone = this.NormalizeCone,
				Radius = this.Radius,
				SoftFalloff = this.SoftFalloff,
				Angle = this.Angle,
				Softness = this.Softness,
				ShowCone = this.ShowCone,
				AreaSize = this.AreaSize
 			};
 		}
	}
}
