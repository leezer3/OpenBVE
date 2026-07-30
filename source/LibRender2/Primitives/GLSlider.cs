using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;

namespace LibRender2.Primitives
{
        public class GLSlider : GLControl
        {
                public double Value = 0.0;
                public double Minimum = 0.0;
                public double Maximum = 100.0;
                public EventHandler ValueChanged;

                private bool dragging = false;

                public GLSlider(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(160, 20);
                        BackgroundColor = new Color128(0.2f, 0.2f, 0.2f, 1f);
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        double centerY = Location.Y + Size.Y / 2.0 - 2.0;
                        // Draw track background
                        Renderer.Rectangle.Draw(null, new Vector2(Location.X, centerY), new Vector2(Size.X, 4.0), BackgroundColor);
                        // Draw filled portion
                        double ratio = (Value - Minimum) / (Maximum - Minimum);
                        double fillWidth = ratio * Size.X;
                        Renderer.Rectangle.Draw(null, new Vector2(Location.X, centerY), new Vector2(fillWidth, 4.0), Color128.Orange);
                        // Draw knob
                        double knobX = Location.X + ratio * (Size.X - 12.0);
                        Color128 knobColor = dragging ? new Color128(1.0f, 0.7f, 0.2f, 1f) : Color128.Orange;
                        Renderer.Rectangle.Draw(null, new Vector2(knobX, Location.Y + 2.0), new Vector2(12.0, Size.Y - 4.0), knobColor);
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);
                        if (dragging)
                        {
                                double localX = System.Math.Max(0.0, System.Math.Min(Size.X - 12.0, x - Location.X - 6.0));
                                double r = localX / (Size.X - 12.0);
                                Value = Minimum + r * (Maximum - Minimum);
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                        }
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible) return;
                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                dragging = true;
                                IsPressed = true;
                                MouseMove(x, y);
                        }
                }

                public override void MouseUp(int x, int y)
                {
                        dragging = false;
                        IsPressed = false;
                }
        }
}
