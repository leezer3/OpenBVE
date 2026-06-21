using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
        public class GLLinkButton : GLControl
        {
                public string Text = "";
                public Color128 NormalColor = new Color128(0.2f, 0.6f, 1f, 1f);
                public Color128 HoverColor = new Color128(0.4f, 0.8f, 1f, 1f);
                public Color128 PressColor = new Color128(0.15f, 0.5f, 0.85f, 1f);
                public EventHandler OnClick;

                public GLLinkButton(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(100, 20);
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        Color128 color = NormalColor;
                        if (IsPressed && CurrentlySelected)
                        {
                                color = PressColor;
                        }
                        else if (CurrentlySelected)
                        {
                                color = HoverColor;
                        }
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Text, Location, TextAlignment.TopLeft, color);
                        Vector2 textSize = Renderer.Fonts.NormalFont.MeasureString(Text);
                        Renderer.Rectangle.Draw(null, new Vector2(Location.X, Location.Y + textSize.Y), new Vector2(textSize.X, 1f), color);
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible) return;
                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                IsPressed = true;
                        }
                }

                public override void MouseUp(int x, int y)
                {
                        if (!IsVisible) return;
                        if (IsPressed && x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                OnClick?.Invoke(this, EventArgs.Empty);
                        }
                        IsPressed = false;
                }
        }
}
