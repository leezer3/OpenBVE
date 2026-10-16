using OpenBveApi.Math;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
        /// <summary>A text label widget for the GL control system</summary>
        public class GLLabel : GLControl
        {
                public string Text = "";
                public Color128 TextColor = Color128.White;
                /// <summary>Text alignment within the label's bounds</summary>
                public TextAlignment Alignment = TextAlignment.TopLeft;

                public GLLabel(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(100, 20);
                        BackgroundColor = new Color128(0, 0, 0, 0); // Transparent by default
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        // Draw background only if it has opacity
                        if (BackgroundColor.A > 0.01f)
                        {
                                Renderer.Rectangle.Draw(null, Location, Size, BackgroundColor);
                        }
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Text, Location, Alignment, TextColor);
                }

                /// <summary>Measures the text and updates Size to fit</summary>
                public void AutoSize()
                {
                        Vector2 textSize = Renderer.Fonts.NormalFont.MeasureString(Text);
                        Size = new Vector2((float)textSize.X + 4f, (float)textSize.Y + 2f);
                }
        }
}
