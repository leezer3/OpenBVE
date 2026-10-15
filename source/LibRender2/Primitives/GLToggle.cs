using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
        public class GLToggle : GLControl
        {
                public bool Checked;
                public string Text = "";
                public EventHandler ValueChanged;

                /// <summary>Color for the toggle track background</summary>
                public Color128 TrackColor = new Color128(0.3f, 0.3f, 0.3f, 1f);
                /// <summary>Color for the toggle knob when checked</summary>
                public Color128 KnobCheckedColor = Color128.Orange;
                /// <summary>Color for the toggle knob when unchecked</summary>
                public Color128 KnobUncheckedColor = new Color128(0.5f, 0.5f, 0.5f, 1f);

                public GLToggle(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(120, 24);
                        BackgroundColor = new Color128(0.1f, 0.1f, 0.1f, 1f);
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        // Draw toggle switch track, right-aligned within the control's bounds
                        Color128 track = CurrentlySelected
                                ? new Color128(0.35f, 0.35f, 0.4f, 1f)
                                : TrackColor;
                        float trackWidth = 36f;
                        float trackHeight = 18f;
                        float trackX = (float)(Location.X + Size.X - trackWidth);
                        float trackY = (float)(Location.Y + (Size.Y - trackHeight) / 2.0);
                        Renderer.Rectangle.Draw(null, new Vector2(trackX, trackY), new Vector2(trackWidth, trackHeight), track);

                        // Draw knob
                        float knobX = Checked ? trackX + 18f : trackX + 3f;
                        Color128 knobColor = Checked ? KnobCheckedColor : KnobUncheckedColor;
                        Renderer.Rectangle.Draw(null, new Vector2(knobX, trackY + 2f), new Vector2(14, 14), knobColor);

                        // Draw label text
                        if (!string.IsNullOrEmpty(Text))
                        {
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, Text, new Vector2(Location.X + 44, Location.Y + 1), TextAlignment.TopLeft, Color128.White);
                        }

                        DrawFocusRing();
                }

                public override void OnActivate()
                {
                        Checked = !Checked;
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                        OnClick?.Invoke(this, EventArgs.Empty);
                }

                public override bool OnAdjust(int direction)
                {
                        OnActivate();
                        return true;
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
                                Checked = !Checked;
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                                OnClick?.Invoke(this, EventArgs.Empty);
                        }
                        IsPressed = false;
                }
        }
}
