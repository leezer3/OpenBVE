using OpenBveApi.Math;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using System;

namespace LibRender2.Primitives
{
        public class GLSlider : GLControl
        {
                public double Value = 0.0;
                public double Minimum = 0.0;
                public double Maximum = 100.0;
                public EventHandler ValueChanged;
                /// <summary>When true, the current value is drawn right-aligned inside the control</summary>
                public bool ShowValue = false;
                /// <summary>Numeric format string for the value readout</summary>
                public string ValueFormat = "0";

                private bool dragging = false;

                public GLSlider(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(160, 20);
                        BackgroundColor = new Color128(0.2f, 0.2f, 0.2f, 1f);
                }

                /// <summary>Sliders need enough length to drag precisely</summary>
                public override float DesiredWidth => ShowValue ? 120f + ValueTextWidth() + 8f : 120f;

                /// <summary>Width reserved for the readout, measured from the range ends so the track never jitters while dragging</summary>
                private float ValueTextWidth()
                {
                        var font = Renderer.Fonts.NormalFont;
                        float w = (float)font.MeasureString(Maximum.ToString(ValueFormat)).X;
                        w = System.Math.Max(w, (float)font.MeasureString(Minimum.ToString(ValueFormat)).X);
                        return w;
                }

                /// <summary>Track length after reserving the readout area</summary>
                private double TrackWidth()
                {
                        double trackWidth = Size.X;
                        if (ShowValue)
                        {
                                trackWidth -= ValueTextWidth() + 8.0;
                        }
                        return System.Math.Max(20.0, trackWidth);
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        bool dimmed = !Enabled;
                        double centerY = Location.Y + Size.Y / 2.0 - 2.0;
                        double trackWidth = TrackWidth();
                        // Draw track background
                        Color128 trackBg = dimmed ? new Color128(0.08f, 0.08f, 0.08f, 1f) : BackgroundColor;
                        Renderer.Rectangle.Draw(null, new Vector2(Location.X, centerY), new Vector2(trackWidth, 4.0), trackBg);
                        // Draw filled portion
                        double ratio = (Value - Minimum) / (Maximum - Minimum);
                        double fillWidth = ratio * trackWidth;
                        Color128 fillColor = dimmed ? new Color128(0.4f, 0.4f, 0.4f, 1f) : Color128.Orange;
                        Renderer.Rectangle.Draw(null, new Vector2(Location.X, centerY), new Vector2(fillWidth, 4.0), fillColor);
                        // Draw knob
                        double knobX = Location.X + ratio * (trackWidth - 12.0);
                        Color128 knobColor = dimmed ? new Color128(0.35f, 0.35f, 0.35f, 1f)
                                : dragging ? new Color128(1.0f, 0.7f, 0.2f, 1f) : Color128.Orange;
                        Renderer.Rectangle.Draw(null, new Vector2(knobX, Location.Y + 2.0), new Vector2(12.0, Size.Y - 4.0), knobColor);
                        if (ShowValue)
                        {
                                Color128 valueColor = dimmed ? new Color128(0.45f, 0.45f, 0.45f, 1f) : Color128.White;
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Value.ToString(ValueFormat), new Vector2((float)(Location.X + trackWidth + 4.0), Location.Y + 2), TextAlignment.TopLeft, valueColor);
                        }

                        if (!dimmed) DrawFocusRing();
                }

                public override bool OnAdjust(int direction)
                {
                        if (!Enabled) return false;
                        double step = (Maximum - Minimum) / 100.0;
                        if (step <= 0.0) step = 1.0;
                        Value = System.Math.Max(Minimum, System.Math.Min(Maximum, Value + direction * step));
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                        return true;
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);
                        if (dragging)
                        {
                                double trackWidth = TrackWidth();
                                double localX = System.Math.Max(0.0, System.Math.Min(trackWidth - 12.0, x - Location.X - 6.0));
                                double r = localX / (trackWidth - 12.0);
                                Value = Minimum + r * (Maximum - Minimum);
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                        }
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
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
