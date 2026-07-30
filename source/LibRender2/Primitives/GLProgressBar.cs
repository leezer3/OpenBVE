using OpenBveApi.Math;
using OpenBveApi.Colors;

namespace LibRender2.Primitives
{
        public class GLProgressBar : GLControl
        {
                public float Value = 0f; // 0 to 1
                public Color128 FillColor = Color128.Orange;
                public Color128 TrackColor;

                public GLProgressBar(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        BackgroundColor = new Color128(0.2f, 0.2f, 0.2f, 1f);
                        TrackColor = BackgroundColor;
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        // Draw track background
                        Renderer.Rectangle.Draw(null, Location, Size, TrackColor);
                        // Draw fill
                        double fillWidth = Size.X * System.Math.Max(0.0, System.Math.Min(1.0, Value));
                        if (fillWidth > 0)
                        {
                                Renderer.Rectangle.Draw(null, Location, new Vector2(fillWidth, Size.Y), FillColor);
                        }
                }
        }
}
