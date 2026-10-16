using OpenBveApi.Math;
using OpenBveApi.Colors;
using System;
using OpenBveApi.Graphics;

namespace LibRender2.Primitives
{
        public class GLNumericUpDown : GLControl
        {
                public float Value = 0f;
                public float Minimum = 0f;
                public float Maximum = 100f;
                public float Increment = 1f;
                /// <summary>Numeric format string for the displayed and edited value</summary>
                public string ValueFormat = "0.##";
                public EventHandler ValueChanged;

                private bool upPressed = false;
                private bool downPressed = false;
                public bool IsEditing = false;
                private string currentInputString = "";

                public GLNumericUpDown(BaseRenderer renderer) : base(renderer)
                {
                        IsVisible = true;
                        Size = new Vector2(120, 24);
                        BackgroundColor = new Color128(0.15f, 0.15f, 0.15f, 1f);
                }

                /// <summary>Wide enough for the value text plus the +/- buttons</summary>
                public override float DesiredWidth
                {
                        get
                        {
                                var font = Renderer.Fonts.NormalFont;
                                float w = (float)font.MeasureString(Value.ToString(ValueFormat)).X;
                                w = System.Math.Max(w, (float)font.MeasureString(Minimum.ToString(ValueFormat)).X);
                                w = System.Math.Max(w, (float)font.MeasureString(Maximum.ToString(ValueFormat)).X);
                                return 8f + w + 8f + 34f;
                        }
                }

                public override void Draw()
                {
                        if (!IsVisible) return;
                        bool dimmed = !Enabled;
                        Color128 fieldBg = dimmed ? new Color128(0.08f, 0.08f, 0.08f, 1f)
                                : IsPressed ? new Color128(0.22f, 0.22f, 0.26f, 1f) : BackgroundColor;
                        Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X - 34, Size.Y), fieldBg);

                        // Draw active/edit border
                        Color128 borderColor = dimmed ? new Color128(0.25f, 0.25f, 0.25f, 1f)
                                : IsEditing ? Color128.Orange : new Color128(0.0f, 0.47f, 0.83f, 1f);
                        Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X - 34, 1.0), borderColor);

                        string textToDraw = IsEditing ? currentInputString + "|" : Value.ToString(ValueFormat);
                        Color128 numTextColor = dimmed ? new Color128(0.45f, 0.45f, 0.45f, 1f) : Color128.White;
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, textToDraw, new Vector2(Location.X + 8, Location.Y + 2), TextAlignment.TopLeft, numTextColor);

                        Vector2 upLoc = new Vector2(Location.X + Size.X - 32, Location.Y);
                        Vector2 downLoc = new Vector2(Location.X + Size.X - 16, Location.Y);

                        // Draw Up (+) button
                        Color128 upBg = dimmed ? new Color128(0.12f, 0.12f, 0.12f, 1f) : upPressed ? Color128.Orange : new Color128(0.25f, 0.25f, 0.25f, 1f);
                        Renderer.Rectangle.Draw(null, upLoc, new Vector2(15, Size.Y), upBg);
                        float plusX = (float)(upLoc.X + (15 - 7) / 2);
                        float plusY = (float)(upLoc.Y + (Size.Y - 7) / 2);
                        Renderer.Rectangle.Draw(null, new Vector2(plusX, plusY + 3), new Vector2(7, 1), numTextColor);
                        Renderer.Rectangle.Draw(null, new Vector2(plusX + 3, plusY), new Vector2(1, 7), numTextColor);

                        // Draw Down (-) button
                        Color128 downBg = dimmed ? new Color128(0.12f, 0.12f, 0.12f, 1f) : downPressed ? Color128.Orange : new Color128(0.25f, 0.25f, 0.25f, 1f);
                        Renderer.Rectangle.Draw(null, downLoc, new Vector2(15, Size.Y), downBg);
                        float minusX = (float)(downLoc.X + (15 - 7) / 2);
                        float minusY = (float)(downLoc.Y + (Size.Y - 1) / 2);
                        Renderer.Rectangle.Draw(null, new Vector2(minusX, minusY), new Vector2(7, 1), numTextColor);

                        if (!dimmed) DrawFocusRing();
                }

                public override void OnActivate()
                {
                        if (!Enabled) return;
                        if (!IsEditing)
                        {
                                IsEditing = true;
                                currentInputString = Value.ToString(ValueFormat);
                                FocusedControl = this;
                        }
                }

                public override bool OnAdjust(int direction)
                {
                        if (!Enabled) return false;
                        CommitEdit();
                        Value = System.Math.Max(Minimum, System.Math.Min(Maximum, Value + direction * Increment));
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                        return true;
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible || !Enabled) return;
                        Vector2 upLoc = new Vector2(Location.X + Size.X - 32, Location.Y);
                        Vector2 downLoc = new Vector2(Location.X + Size.X - 16, Location.Y);

                        if (x >= upLoc.X && x <= upLoc.X + 15 && y >= upLoc.Y && y <= upLoc.Y + Size.Y)
                        {
                                CommitEdit();
                                upPressed = true;
                                IsPressed = true;
                                Value = System.Math.Min(Maximum, Value + Increment);
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                                OnClick?.Invoke(this, EventArgs.Empty);
                        }
                        else if (x >= downLoc.X && x <= downLoc.X + 15 && y >= downLoc.Y && y <= downLoc.Y + Size.Y)
                        {
                                CommitEdit();
                                downPressed = true;
                                IsPressed = true;
                                Value = System.Math.Max(Minimum, Value - Increment);
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                                OnClick?.Invoke(this, EventArgs.Empty);
                        }
                        else if (x >= Location.X && x < Location.X + Size.X - 32 && y >= Location.Y && y <= Location.Y + Size.Y)
                        {
                                if (!IsEditing)
                                {
                                        if (FocusedControl != null && FocusedControl != this && FocusedControl is GLNumericUpDown otherNum)
                                        {
                                                otherNum.CommitEdit();
                                        }
                                        IsEditing = true;
                                        currentInputString = Value.ToString(ValueFormat);
                                        FocusedControl = this;
                                }
                        }
                        else
                        {
                                CommitEdit();
                        }
                }

                public override void MouseUp(int x, int y)
                {
                        upPressed = false;
                        downPressed = false;
                        IsPressed = false;
                }

                public void CommitEdit()
                {
                        if (!IsEditing) return;
                        if (float.TryParse(currentInputString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                        {
                                Value = System.Math.Max(Minimum, System.Math.Min(Maximum, parsed));
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                                OnClick?.Invoke(this, EventArgs.Empty);
                        }
                        else if (float.TryParse(currentInputString.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsedAlt))
                        {
                                Value = System.Math.Max(Minimum, System.Math.Min(Maximum, parsedAlt));
                                ValueChanged?.Invoke(this, EventArgs.Empty);
                                OnClick?.Invoke(this, EventArgs.Empty);
                        }
                        IsEditing = false;
                        if (FocusedControl == this)
                        {
                                FocusedControl = null;
                        }
                }

                public override void KeyDown(OpenTK.Input.Key key)
                {
                        if (!IsEditing) return;

                        if (key == OpenTK.Input.Key.Escape)
                        {
                                IsEditing = false;
                                if (FocusedControl == this) FocusedControl = null;
                                return;
                        }

                        if (key == OpenTK.Input.Key.Enter || key == OpenTK.Input.Key.KeypadEnter)
                        {
                                CommitEdit();
                                return;
                        }

                        if (key == OpenTK.Input.Key.BackSpace)
                        {
                                if (currentInputString.Length > 0)
                                {
                                        currentInputString = currentInputString.Substring(0, currentInputString.Length - 1);
                                }
                                return;
                        }

                        string charToAdd = "";
                        if (key >= OpenTK.Input.Key.Number0 && key <= OpenTK.Input.Key.Number9)
                        {
                                charToAdd = ((int)(key - OpenTK.Input.Key.Number0)).ToString();
                        }
                        else if (key >= OpenTK.Input.Key.Keypad0 && key <= OpenTK.Input.Key.Keypad9)
                        {
                                charToAdd = ((int)(key - OpenTK.Input.Key.Keypad0)).ToString();
                        }
                        else if (key == OpenTK.Input.Key.Period || key == OpenTK.Input.Key.KeypadPeriod || key == OpenTK.Input.Key.Comma)
                        {
                                charToAdd = ".";
                        }
                        else if (key == OpenTK.Input.Key.Minus || key == OpenTK.Input.Key.KeypadMinus)
                        {
                                charToAdd = "-";
                        }

                        if (!string.IsNullOrEmpty(charToAdd))
                        {
                                currentInputString += charToAdd;
                        }
                }
        }
}
