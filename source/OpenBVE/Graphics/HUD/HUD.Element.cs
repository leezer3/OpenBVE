using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace OpenBve
{
	internal static partial class HUD
	{
		internal class Element
		{
			internal HUDSubject Subject;
			internal Vector2 Position;
			internal Vector2 Alignment;
			internal Image TopLeft;
			internal Image TopMiddle;
			internal Image TopRight;
			internal Image CenterLeft;
			internal Image CenterMiddle;
			internal Image CenterRight;
			internal Image BottomLeft;
			internal Image BottomMiddle;
			internal Image BottomRight;
			internal Color32 BackgroundColor;
			internal Color32 OverlayColor;
			internal Color32 TextColor;
			internal Vector2 TextPosition;
			internal Vector2 TextAlignment;
			internal OpenGlFont Font;
			internal bool TextShadow;
			internal string Text;
			internal double Value1;
			internal double Value2;
			internal Transition Transition;
			internal Vector2 TransitionVector;
			internal double TransitionState;
			internal Element()
			{
				Subject = HUDSubject.Unknown;
				Position = new Vector2();
				Alignment = new Vector2(-1, -1);
				BackgroundColor = Color32.White;
				OverlayColor = Color32.White;
				TextColor = Color32.White;
				TextPosition = new Vector2();
				TextAlignment = Vector2.Left;
				Font = Program.Renderer.Fonts.VerySmallFont;
				TextShadow = true;
				Text = null;
				Value1 = 0.0f;
				Value2 = 0.0f;
				Transition = Transition.None;
				TransitionState = 1.0;
			}	


			/// <summary>Calculates the viewing plane size for the element</summary>
			/// <param name="leftWidth">The left width of the viewing plane</param>
			/// <param name="rightWidth">The right width of the viewing plane</param>
			/// <param name="lCrH">The center point of the viewing plane</param>
			internal void CalculateViewingPlaneSize(out double leftWidth, out double rightWidth, out double lCrH)
			{
				lCrH = 0.0;
				// left width/height
				leftWidth = 0.0;
				if (Program.CurrentHost.LoadTexture(ref TopLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = TopLeft.BackgroundTexture.Width;
					double v = TopLeft.BackgroundTexture.Height;
					if (u > leftWidth) leftWidth = u;
					if (v > lCrH) lCrH = v;
				}
				if (Program.CurrentHost.LoadTexture(ref CenterLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = CenterLeft.BackgroundTexture.Width;
					double v = CenterLeft.BackgroundTexture.Height;
					if (u > leftWidth) leftWidth = u;
					if (v > lCrH) lCrH = v;
				}
				if (Program.CurrentHost.LoadTexture(ref BottomLeft.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = BottomLeft.BackgroundTexture.Width;
					double v = BottomLeft.BackgroundTexture.Height;
					if (u > leftWidth) leftWidth = u;
					if (v > lCrH) lCrH = v;
				}
				// center height
				if (Program.CurrentHost.LoadTexture(ref TopMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = TopMiddle.BackgroundTexture.Height;
					if (v > lCrH) lCrH = v;
				}
				if (Program.CurrentHost.LoadTexture(ref CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = CenterMiddle.BackgroundTexture.Height;
					if (v > lCrH) lCrH = v;
				}
				if (Program.CurrentHost.LoadTexture(ref BottomMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double v = BottomMiddle.BackgroundTexture.Height;
					if (v > lCrH) lCrH = v;
				}
				// right width/height
				rightWidth = 0.0;
				if (Program.CurrentHost.LoadTexture(ref TopRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = TopRight.BackgroundTexture.Width;
					double v = TopRight.BackgroundTexture.Height;
					if (u > rightWidth) rightWidth = u;
					if (v > lCrH) lCrH = v;
				}
				if (Program.CurrentHost.LoadTexture(ref CenterRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = CenterRight.BackgroundTexture.Width;
					double v = CenterRight.BackgroundTexture.Height;
					if (u > rightWidth) rightWidth = u;
					if (v > lCrH) lCrH = v;
				}
				if (Program.CurrentHost.LoadTexture(ref BottomRight.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					double u = BottomRight.BackgroundTexture.Width;
					double v = BottomRight.BackgroundTexture.Height;
					if (u > rightWidth) rightWidth = u;
					if (v > lCrH) lCrH = v;
				}
			}
		}
	}
}
