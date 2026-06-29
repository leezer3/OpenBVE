using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
        /// <summary>A modern styled button widget for the GL control system</summary>
        public class GLButton : GLControl
        {
                public string Text = "";
                public EventHandler OnClick;

                /// <summary>The normal background color</summary>
                public Color128 NormalColor = new Color128(0.0f, 0.47f, 0.83f, 1.0f);
                /// <summary>The hover background color</summary>
                public Color128 HoverColor = new Color128(0.1f, 0.55f, 0.93f, 1.0f);
                /// <summary>The pressed background color</summary>
                public Color128 PressColor = new Color128(0.0f, 0.37f, 0.65f, 1.0f);
                /// <summary>The disabled background color</summary>
                public Color128 DisabledColor = new Color128(0.2f, 0.2f, 0.2f, 1.0f);
                /// <summary>Whether the button is enabled</summary>
                public bool Enabled = true;
                /// <summary>Whether this is a flat/minimal style button</summary>
                public bool Flat = false;
                /// <summary>Border radius in pixels (0 = sharp corners)</summary>
                public float CornerRadius = 3f;

                public GLButton(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(100, 28);
                        BackgroundColor = NormalColor;
                }

                public override void Draw()
                {
                        if (!IsVisible) return;

                        if (!Enabled)
                        {
                                Renderer.Rectangle.Draw(null, Location, Size, DisabledColor);
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Text,
                                        new Vector2(Location.X + (Size.X - Renderer.Fonts.NormalFont.MeasureString(Text).X) / 2.0, Location.Y + 4),
                                        TextAlignment.TopLeft, new Color128(0.5f, 0.5f, 0.5f, 1f));
                                return;
                        }

                        Color128 bgColor;
                        if (IsPressed && CurrentlySelected)
                        {
                                bgColor = PressColor;
                        }
                        else if (CurrentlySelected)
                        {
                                bgColor = HoverColor;
                        }
                        else
                        {
                                bgColor = Flat ? BackgroundColor : NormalColor;
                        }

                        // Draw background
                        Renderer.Rectangle.Draw(null, Location, Size, bgColor);

                        // Draw subtle top highlight line (gives 3D feel)
                        if (!Flat)
                        {
                                Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X, 1.0), new Color128(1f, 1f, 1f, 0.15f));
                        }

                        // Draw text centered
                        Vector2 textSize = Renderer.Fonts.NormalFont.MeasureString(Text);
                        Vector2 textPos = new Vector2(Location.X + (Size.X - textSize.X) / 2.0, Location.Y + (Size.Y - textSize.Y) / 2.0);
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Text, textPos, TextAlignment.TopLeft, Color128.White);
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = Enabled && (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                IsPressed = true;
                        }
                }

                public override void MouseUp(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        if (IsPressed && x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                OnClick?.Invoke(this, EventArgs.Empty);
                        }
                        IsPressed = false;
                }
        }
}
