using OpenBveApi.Math;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace LibRender2.Primitives
{
        /// <summary>A reusable Blender-style color picker: hue wheel, value slider, RGB 0-255 and hex readout</summary>
        public class GLColorPicker : GLControl
        {
                /// <summary>The currently selected color</summary>
                public Color24 SelectedColor;
                /// <summary>Whether the picker popup is currently expanded</summary>
                public bool Expanded = false;
                /// <summary>Fired when the selected color changes</summary>
                public EventHandler SelectionChanged;
                /// <summary>The picker whose popup is currently open, if any</summary>
                public static GLColorPicker ActivePicker;

                private const float Pad = 6f;
                private const float WheelD = 150f;
                private const float SliderW = 18f;
                private const float Gap = 8f;
                private const float RowH = 26f;
                private float PopupW => Pad + WheelD + Gap + SliderW + Pad;
                private float PopupH => Pad + WheelD + Gap + 3 * RowH + 4 + 20 + Pad;

                private double hue;
                private double sat;
                private double val = 1.0;
                private bool draggingWheel;
                private bool draggingSlider;
                /// <summary>Cached static hue wheel texture (full value, built once)</summary>
                private Texture wheelTexture;

                public GLColorPicker(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(120, 24);
                        BackgroundColor = new Color128(0.15f, 0.15f, 0.15f, 1f);
                }

                /// <summary>Wide enough for the swatch, the hex readout and the arrow indicator</summary>
                public override float DesiredWidth
                {
                        get
                        {
                                float hexW = (float)Renderer.Fonts.NormalFont.MeasureString("#FFFFFF").X;
                                return 8f + 22f + 6f + hexW + 6f + 12f + 8f;
                        }
                }

                private static Color128 ToColor128(Color24 color)
                {
                        return new Color128(color.R / 255f, color.G / 255f, color.B / 255f, 1f);
                }

                private static string ToHex(Color24 color)
                {
                        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                }

                private static byte ToByte(double d)
                {
                        if (d <= 0.0) return 0;
                        if (d >= 1.0) return 255;
                        return (byte)(d * 255.0 + 0.5);
                }

                private static void RgbToHsv(Color24 color, out double h, out double s, out double v)
                {
                        double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
                        double max = Math.Max(r, Math.Max(g, b));
                        double min = Math.Min(r, Math.Min(g, b));
                        v = max;
                        double d = max - min;
                        s = max <= 0.0 ? 0.0 : d / max;
                        if (d <= 0.0)
                        {
                                h = 0.0;
                        }
                        else if (max == r)
                        {
                                h = (g - b) / d + (g < b ? 6.0 : 0.0);
                                h /= 6.0;
                        }
                        else if (max == g)
                        {
                                h = ((b - r) / d + 2.0) / 6.0;
                        }
                        else
                        {
                                h = ((r - g) / d + 4.0) / 6.0;
                        }
                }

                private static Color24 HsvToRgb(double h, double s, double v)
                {
                        h -= Math.Floor(h);
                        if (s <= 0.0) return new Color24(ToByte(v), ToByte(v), ToByte(v));
                        double c = v * s;
                        double hh = h * 6.0;
                        double x = c * (1.0 - Math.Abs(hh % 2.0 - 1.0));
                        double r = 0.0, g = 0.0, b = 0.0;
                        switch ((int)hh)
                        {
                                case 0: r = c; g = x; break;
                                case 1: r = x; g = c; break;
                                case 2: g = c; b = x; break;
                                case 3: g = x; b = c; break;
                                case 4: r = x; b = c; break;
                                default: r = c; b = x; break;
                        }
                        double m = v - c;
                        return new Color24(ToByte(r + m), ToByte(g + m), ToByte(b + m));
                }

                /// <summary>Sets the selected color without notifying (e.g. initial value from options)</summary>
                public void SetColor(Color24 color)
                {
                        ApplyColor(color, false);
                }

                /// <summary>Builds the static hue wheel bitmap once and registers it (1 draw call, no shader)</summary>
                private void EnsureWheelTexture()
                {
                        if (wheelTexture != null) return;
                        int d = (int)WheelD;
                        Bitmap bitmap = new Bitmap(d, d, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, d, d), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        byte[] pixels = new byte[data.Stride * d];
                        for (int j = 0; j < d; j++)
                        {
                                for (int i = 0; i < d; i++)
                                {
                                        double dx = i + 0.5 - d / 2.0, dy = j + 0.5 - d / 2.0;
                                        double r = Math.Sqrt(dx * dx + dy * dy) / (d / 2.0);
                                        int o = j * data.Stride + i * 4;
                                        if (r > 1.0)
                                        {
                                                pixels[o] = pixels[o + 1] = pixels[o + 2] = pixels[o + 3] = 0;
                                        }
                                        else
                                        {
                                                double h = Math.Atan2(-dy, dx) / (2.0 * Math.PI);
                                                if (h < 0.0) h += 1.0;
                                                Color24 col = HsvToRgb(h, Math.Min(1.0, r), 1.0);
                                                // 1px feathered rim (bitmap rows map top-to-top, no flip)
                                                double rim = (1.0 - r) * (d / 2.0);
                                                pixels[o] = col.B;
                                                pixels[o + 1] = col.G;
                                                pixels[o + 2] = col.R;
                                                pixels[o + 3] = rim >= 1.0 ? (byte)255 : (byte)(rim * 255.0);
                                        }
                                }
                        }
                        Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                        bitmap.UnlockBits(data);
                        wheelTexture = Renderer.TextureManager.RegisterTexture(bitmap);
                }

                private void ApplyColor(Color24 color, bool notify)
                {
                        SelectedColor = color;
                        RgbToHsv(color, out hue, out sat, out val);
                        if (notify) SelectionChanged?.Invoke(this, EventArgs.Empty);
                }

                private void AdjustChannel(int channel, int delta)
                {
                        int r = SelectedColor.R, g = SelectedColor.G, b = SelectedColor.B;
                        switch (channel)
                        {
                                case 0: r = Math.Max(0, Math.Min(255, r + delta)); break;
                                case 1: g = Math.Max(0, Math.Min(255, g + delta)); break;
                                default: b = Math.Max(0, Math.Min(255, b + delta)); break;
                        }
                        ApplyColor(new Color24((byte)r, (byte)g, (byte)b), true);
                        OnClick?.Invoke(this, EventArgs.Empty);
                }

                /// <summary>Computes the popup position and size, right-aligned to the control so it never covers the sidebar toggle</summary>
                private void OverlayGeometry(out Vector2 overlayLoc, out Vector2 overlaySize)
                {
                        float w = PopupW, h = PopupH;
                        float ox = (float)(Location.X + Size.X - w);
                        if (ox < 0) ox = 0;
                        float oy = (float)(Location.Y + Size.Y + 2);
                        if (oy + h > Renderer.Screen.Height) oy = (float)(Location.Y - h - 2);
                        if (oy < 0) oy = 0;
                        overlayLoc = new Vector2(ox, oy);
                        overlaySize = new Vector2(w, h);
                }

                private void WheelGeometry(out float cx, out float cy, out float radius)
                {
                        OverlayGeometry(out Vector2 overlayLoc, out Vector2 overlaySize);
                        cx = (float)(overlayLoc.X + Pad + WheelD / 2);
                        cy = (float)(overlayLoc.Y + Pad + WheelD / 2);
                        radius = WheelD / 2;
                }

                private void SliderGeometry(out Vector2 sliderLoc, out Vector2 sliderSize)
                {
                        OverlayGeometry(out Vector2 overlayLoc, out Vector2 overlaySize);
                        sliderLoc = new Vector2(overlayLoc.X + Pad + WheelD + Gap, overlayLoc.Y + Pad);
                        sliderSize = new Vector2(SliderW, WheelD);
                }

                private void StepperGeometry(int channel, out Vector2 minusLoc, out Vector2 plusLoc, out float rowY)
                {
                        OverlayGeometry(out Vector2 overlayLoc, out Vector2 overlaySize);
                        rowY = (float)(overlayLoc.Y + Pad + WheelD + Gap + channel * RowH);
                        minusLoc = new Vector2(overlayLoc.X + PopupW - 58, rowY + 2);
                        plusLoc = new Vector2(overlayLoc.X + PopupW - 30, rowY + 2);
                }

                private bool IsInOverlay(int x, int y)
                {
                        OverlayGeometry(out Vector2 overlayLoc, out Vector2 overlaySize);
                        return x >= overlayLoc.X && x <= overlayLoc.X + overlaySize.X && y >= overlayLoc.Y && y <= overlayLoc.Y + overlaySize.Y;
                }

                private void SetFromWheel(int x, int y)
                {
                        WheelGeometry(out float cx, out float cy, out float radius);
                        double dx = x - cx, dy = y - cy;
                        double r = Math.Sqrt(dx * dx + dy * dy);
                        if (r > radius && r > 0.0)
                        {
                                dx *= radius / r;
                                dy *= radius / r;
                                r = radius;
                        }
                        double h = Math.Atan2(-dy, dx) / (2.0 * Math.PI);
                        if (h < 0.0) h += 1.0;
                        hue = h;
                        sat = radius > 0.0 ? r / radius : 0.0;
                        ApplyColor(HsvToRgb(hue, sat, val), true);
                }

                private void SetFromSlider(int y)
                {
                        SliderGeometry(out Vector2 sliderLoc, out Vector2 sliderSize);
                        double v = 1.0 - (y - sliderLoc.Y) / sliderSize.Y;
                        val = Math.Max(0.0, Math.Min(1.0, v));
                        ApplyColor(HsvToRgb(hue, sat, val), true);
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        bool dimmed = !Enabled;
                        // Draw main box
                        Color128 boxBg = dimmed ? new Color128(0.08f, 0.08f, 0.08f, 1f)
                                : Expanded ? new Color128(0.22f, 0.22f, 0.26f, 1f) : BackgroundColor;
                        Renderer.Rectangle.DrawRounded(null, Location, Size, 6f, boxBg);
                        // Draw border
                        Color128 border = dimmed ? new Color128(0.25f, 0.25f, 0.25f, 1f) : new Color128(0.0f, 0.47f, 0.83f, 1f);
                        Renderer.Rectangle.DrawRounded(null, Location, new Vector2(Size.X, 1.0), 6f, border);
                        // Draw swatch
                        float swatchY = (float)(Location.Y + (Size.Y - 18) / 2);
                        Color128 swatchColor = dimmed ? new Color128(0.25f, 0.25f, 0.25f, 1f) : ToColor128(SelectedColor);
                        Renderer.Rectangle.DrawRounded(null, new Vector2(Location.X + 8, swatchY), new Vector2(22, 18), 3f, swatchColor);
                        // Draw arrow indicator
                        Color128 arrowColor = dimmed ? new Color128(0.45f, 0.45f, 0.45f, 1f) : Color128.White;
                        float arrowX = (float)(Location.X + Size.X - 18);
                        float arrowY = (float)(Location.Y + (Size.Y - 5) / 2);
                        if (Expanded)
                        {
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 4, arrowY), new Vector2(1, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 3, arrowY + 1), new Vector2(3, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 2, arrowY + 2), new Vector2(5, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 1, arrowY + 3), new Vector2(7, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX, arrowY + 4), new Vector2(9, 1), arrowColor);
                        }
                        else
                        {
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX, arrowY), new Vector2(9, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 1, arrowY + 1), new Vector2(7, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 2, arrowY + 2), new Vector2(5, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 3, arrowY + 3), new Vector2(3, 1), arrowColor);
                                Renderer.Rectangle.Draw(null, new Vector2(arrowX + 4, arrowY + 4), new Vector2(1, 1), arrowColor);
                        }

                        Color128 textColor = dimmed ? new Color128(0.45f, 0.45f, 0.45f, 1f) : Color128.White;
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, ToHex(SelectedColor), new Vector2(Location.X + 8 + 22 + 6, Location.Y + 2), TextAlignment.TopLeft, textColor);

                        if (!dimmed) DrawFocusRing();
                }

                /// <summary>Draws the expanded wheel / slider / RGB popup above all tab contents</summary>
                public void DrawOverlay()
                {
                        if (!IsVisible || !Expanded) return;
                        OverlayGeometry(out Vector2 overlayLoc, out Vector2 overlaySize);
                        Renderer.Rectangle.DrawRounded(null, overlayLoc, overlaySize, 6f, new Color128(0.1f, 0.1f, 0.1f, 0.97f));

                        // Hue wheel (hue = angle, saturation = radius), one cached textured quad
                        WheelGeometry(out float cx, out float cy, out float radius);
                        EnsureWheelTexture();
                        if (wheelTexture != null)
                        {
                                Renderer.Rectangle.Draw(wheelTexture, new Vector2(overlayLoc.X + Pad, overlayLoc.Y + Pad), new Vector2(WheelD, WheelD), Color128.White);
                        }
                        // Wheel position indicator
                        double angle = hue * 2.0 * Math.PI;
                        float dotX = (float)(cx + Math.Cos(angle) * sat * radius);
                        float dotY = (float)(cy - Math.Sin(angle) * sat * radius);
                        Renderer.Rectangle.DrawRounded(null, new Vector2(dotX - 4, dotY - 4), new Vector2(8, 8), 4f, Color128.White);
                        Renderer.Rectangle.DrawRounded(null, new Vector2(dotX - 3, dotY - 3), new Vector2(6, 6), 3f, ToColor128(SelectedColor));

                        // Value slider
                        SliderGeometry(out Vector2 sliderLoc, out Vector2 sliderSize);
                        int bands = 30;
                        float bandH = (float)(sliderSize.Y / bands);
                        for (int i = 0; i < bands; i++)
                        {
                                double v = 1.0 - (i + 0.5) / bands;
                                Renderer.Rectangle.Draw(null, new Vector2(sliderLoc.X, sliderLoc.Y + i * bandH), new Vector2(sliderSize.X, bandH + 0.5), ToColor128(HsvToRgb(hue, sat, v)));
                        }
                        float markY = (float)(sliderLoc.Y + (1.0 - val) * sliderSize.Y);
                        Renderer.Rectangle.Draw(null, new Vector2(sliderLoc.X - 1, markY - 1), new Vector2(sliderSize.X + 2, 2), Color128.White);

                        // RGB steppers (0-255)
                        string[] names = { "Red", "Green", "Blue" };
                        int[] values = { SelectedColor.R, SelectedColor.G, SelectedColor.B };
                        for (int k = 0; k < 3; k++)
                        {
                                StepperGeometry(k, out Vector2 minusLoc, out Vector2 plusLoc, out float rowY);
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, names[k], new Vector2(overlayLoc.X + 8, rowY + 4), TextAlignment.TopLeft, Color128.White);
                                Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, values[k].ToString(), new Vector2(overlayLoc.X + 64, rowY + 4), TextAlignment.TopLeft, Color128.White);
                                Renderer.Rectangle.DrawRounded(null, minusLoc, new Vector2(24, 22), 3f, new Color128(0.25f, 0.25f, 0.25f, 1f));
                                Renderer.Rectangle.DrawRounded(null, plusLoc, new Vector2(24, 22), 3f, new Color128(0.25f, 0.25f, 0.25f, 1f));
                                DrawCentered("+", plusLoc, 24, 22);
                                DrawCentered("-", minusLoc, 24, 22);
                        }

                        // Hex readout
                        float hexY = (float)(overlayLoc.Y + Pad + WheelD + Gap + 3 * RowH + 4);
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, "Hex  " + ToHex(SelectedColor), new Vector2(overlayLoc.X + 8, hexY + 2), TextAlignment.TopLeft, Color128.White);
                }

                private void DrawCentered(string text, Vector2 boxLoc, float boxW, float boxH)
                {
                        Vector2 textSize = Renderer.Fonts.NormalFont.MeasureString(text);
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, text, new Vector2(boxLoc.X + (boxW - textSize.X) / 2f, boxLoc.Y + (boxH - textSize.Y) / 2f), TextAlignment.TopLeft, Color128.White);
                }

                public override bool HitTest(int x, int y)
                {
                        if (!IsVisible || !Enabled) return false;
                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y) return true;
                        if (Expanded && IsInOverlay(x, y)) return true;
                        return false;
                }

                public override void OnActivate()
                {
                        if (!Enabled) return;
                        Expanded = !Expanded;
                        if (Expanded)
                        {
                                if (ActivePicker != null && ActivePicker != this) ActivePicker.Expanded = false;
                                ActivePicker = this;
                                if (GLDropdown.ActiveDropdown != null)
                                {
                                        GLDropdown.ActiveDropdown.Expanded = false;
                                        GLDropdown.ActiveDropdown = null;
                                }
                        }
                        else if (ActivePicker == this)
                        {
                                ActivePicker = null;
                        }
                        draggingWheel = false;
                        draggingSlider = false;
                }

                public override bool OnAdjust(int direction)
                {
                        if (!Enabled) return false;
                        val = Math.Max(0.0, Math.Min(1.0, val + direction * 0.05));
                        ApplyColor(HsvToRgb(hue, sat, val), true);
                        return true;
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);
                        if (draggingWheel)
                        {
                                SetFromWheel(x, y);
                        }
                        else if (draggingSlider)
                        {
                                SetFromSlider(y);
                        }
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        if (Expanded)
                        {
                                WheelGeometry(out float cx, out float cy, out float radius);
                                double dx = x - cx, dy = y - cy;
                                if (Math.Sqrt(dx * dx + dy * dy) <= radius)
                                {
                                        draggingWheel = true;
                                        SetFromWheel(x, y);
                                        return;
                                }
                                SliderGeometry(out Vector2 sliderLoc, out Vector2 sliderSize);
                                if (x >= sliderLoc.X - 2 && x <= sliderLoc.X + sliderSize.X + 2 && y >= sliderLoc.Y && y <= sliderLoc.Y + sliderSize.Y)
                                {
                                        draggingSlider = true;
                                        SetFromSlider(y);
                                        return;
                                }
                                for (int k = 0; k < 3; k++)
                                {
                                        StepperGeometry(k, out Vector2 minusLoc, out Vector2 plusLoc, out float rowY);
                                        if (x >= minusLoc.X && x <= minusLoc.X + 24 && y >= minusLoc.Y && y <= minusLoc.Y + 22)
                                        {
                                                AdjustChannel(k, -1);
                                                return;
                                        }
                                        if (x >= plusLoc.X && x <= plusLoc.X + 24 && y >= plusLoc.Y && y <= plusLoc.Y + 22)
                                        {
                                                AdjustChannel(k, 1);
                                                return;
                                        }
                                }
                        }
                        if (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                OnActivate();
                        }
                        else
                        {
                                Expanded = false;
                                if (ActivePicker == this) ActivePicker = null;
                                draggingWheel = false;
                                draggingSlider = false;
                        }
                }

                public override void MouseUp(int x, int y)
                {
                        draggingWheel = false;
                        draggingSlider = false;
                        base.MouseUp(x, y);
                }
        }
}
