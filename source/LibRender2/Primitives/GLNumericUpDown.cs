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

                public override void Draw()
                {
                        if (!IsVisible) return;
                        Color128 fieldBg = IsPressed ? new Color128(0.22f, 0.22f, 0.26f, 1f) : BackgroundColor;
                        Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X - 34, Size.Y), fieldBg);

                        // Draw active/edit border
                        Color128 borderColor = IsEditing ? Color128.Orange : new Color128(0.0f, 0.47f, 0.83f, 1f);
                        Renderer.Rectangle.Draw(null, Location, new Vector2(Size.X - 34, 1.0), borderColor);

                        string textToDraw = IsEditing ? currentInputString + "|" : Value.ToString("0.##");
                        Renderer.OpenGlString.Draw(Renderer.Fonts.NormalFont, textToDraw, new Vector2(Location.X + 8, Location.Y + 2), TextAlignment.TopLeft, Color128.White);

                        Vector2 upLoc = new Vector2(Location.X + Size.X - 32, Location.Y);
                        Vector2 downLoc = new Vector2(Location.X + Size.X - 16, Location.Y);

                        // Draw Up (+) button
                        Color128 upBg = upPressed ? Color128.Orange : new Color128(0.25f, 0.25f, 0.25f, 1f);
                        Renderer.Rectangle.Draw(null, upLoc, new Vector2(15, Size.Y), upBg);
                        float plusX = (float)(upLoc.X + (15 - 7) / 2);
                        float plusY = (float)(upLoc.Y + (Size.Y - 7) / 2);
                        Renderer.Rectangle.Draw(null, new Vector2(plusX, plusY + 3), new Vector2(7, 1), Color128.White);
                        Renderer.Rectangle.Draw(null, new Vector2(plusX + 3, plusY), new Vector2(1, 7), Color128.White);

                        // Draw Down (-) button
                        Color128 downBg = downPressed ? Color128.Orange : new Color128(0.25f, 0.25f, 0.25f, 1f);
                        Renderer.Rectangle.Draw(null, downLoc, new Vector2(15, Size.Y), downBg);
                        float minusX = (float)(downLoc.X + (15 - 7) / 2);
                        float minusY = (float)(downLoc.Y + (Size.Y - 1) / 2);
                        Renderer.Rectangle.Draw(null, new Vector2(minusX, minusY), new Vector2(7, 1), Color128.White);
                }

                public override void MouseMove(int x, int y)
                {
                        if (!IsVisible) return;
                        CurrentlySelected = (x >= Location.X && x <= Location.X + Size.X && y >= Location.Y && y <= Location.Y + Size.Y);
                }

                public override void MouseDown(int x, int y)
                {
                        if (!IsVisible) return;
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
                                        currentInputString = Value.ToString("0.##");
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
