using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;

namespace LibRender2.Primitives
{
	public class Textbox : GLControl
	{
		/// <summary>The font the items in this textbox are to be drawn with</summary>
		private readonly OpenGlFont myFont;
		/// <summary>The font color</summary>
		private readonly Color128 myFontColor;
		/// <summary>The color of the scrollbar handle</summary>
		private readonly Color128 myScrollbarColor;
		/// <summary>The string contents of the textbox</summary>
		public string Text
		{
			get => myText;
			set
			{
				myText = value;
				//reset the scroll value
				topLine = 0;
			}
		}
		/// <summary>Backing property for the textbox text</summary>
		private string myText;

		/// <summary>The border width of the textbox </summary>
		public readonly int Border;
		/// <summary>The top line to be renderered</summary>
		private int topLine;
		/// <summary>Whether the textbox can scroll</summary>
		public bool CanScroll;
		/// <summary>Used for internal size calculations</summary>
		private Vector2 internalSize => CanScroll ? new Vector2(Size.X, Size.Y - 12) : Size;

		private List<string> WrappedLines(int width)
		{
			width = 200;
			// string literal as well as escaped character as we may have loaded from language file
			string[] firstSplit = Text.Split(new[] {"\r\n", "\n", @"\r\n"}, StringSplitOptions.None);
			List<string> wrappedLines = new List<string>();
			string currentLine = string.Empty;
			for(int j = 0; j < firstSplit.Length; j++)
			{
				for (int i = 0; i < firstSplit[j].Length; i++)
				{
					char currentChar = firstSplit[j][i];
					currentLine += currentChar;
					if (myFont.MeasureString(currentLine).X > width)
					{
						if (currentLine.Any(char.IsWhiteSpace))
						{
							// Exceeded length, back up to last space
							int moveback = 1;
							while (!char.IsWhiteSpace(currentLine[currentLine.Length - moveback]))
							{
								moveback++;
								i--;
							}
							string lineToAdd = currentLine.Substring(0, currentLine.Length - moveback);
							wrappedLines.Add(lineToAdd.TrimStart());
							currentLine = string.Empty;
						}
						else
						{
							i--;
							string lineToAdd = currentLine.Substring(0, currentLine.Length - 1);
							wrappedLines.Add(lineToAdd.TrimStart());
							currentLine = string.Empty;
						}
						
					}
				}
				wrappedLines.Add(currentLine.TrimStart());
				currentLine = string.Empty;
			}
			
			if (currentLine.Length > 0)
			{
				wrappedLines.Add(currentLine.TrimStart());
			}
			return wrappedLines;
		}

		public Textbox(BaseRenderer Renderer, OpenGlFont Font, Color128 FontColor, Color128 backgroundColor) : base(Renderer)
		{
			myFont = Font;
			myFontColor = FontColor;
			Border = 5;
			topLine = 0;
			Texture = null;
			BackgroundColor = backgroundColor;
			myScrollbarColor = Color128.Orange;
		}

		public void VerticalScroll(int numberOfLines)
		{
			topLine += numberOfLines;
			if (topLine < 0)
			{
				topLine = 0;
			}
		}

		public override void Draw()
		{
			Renderer.Rectangle.Draw(Texture, Location, Size, BackgroundColor); //Draw the backing rectangle first
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}

			List<string> splitString = WrappedLines((int)internalSize.Y - Border * 2);
			if (splitString.Count == 1)
			{
				//DRAW SINGLE LINE
				Renderer.OpenGlString.Draw(myFont, Text, new Vector2(Location.X + Border, Location.Y + Border), TextAlignment.TopLeft, myFontColor);
				CanScroll = false;
			}
			else
			{
				int maxFittingLines = (int)(internalSize.Y / myFont.FontSize);
				if (topLine + maxFittingLines > splitString.Count)
				{
					topLine = Math.Max(0, splitString.Count - maxFittingLines);
				}
				CanScroll = maxFittingLines < splitString.Count;
				if (CanScroll)
				{
					Renderer.Rectangle.Draw(null, new Vector2(Location.X + Size.X - 12, Location.Y + 2), new Vector2(8, Size.Y - 4), Color128.Grey); //Backing rectangle
				}
				//DRAW SPLIT LINES
				int currentLine = topLine;
				int bottomLine = Math.Min(maxFittingLines, splitString.Count);
				for (int i = 0; i < bottomLine; i++)
				{
					Renderer.OpenGlString.Draw(myFont, splitString[currentLine], new Vector2(Location.X + Border, Location.Y + Border + myFont.FontSize * i), TextAlignment.TopLeft, myFontColor);
					currentLine++;
				}

				if (CanScroll)
				{
					double scrollBarHeight = (Size.Y - 4) * maxFittingLines / splitString.Count;
					double percentageScroll = topLine / (double)(splitString.Count - maxFittingLines);
					Renderer.Rectangle.Draw(null, new Vector2(Location.X + Size.X - 13, Location.Y + (Size.Y - scrollBarHeight) * percentageScroll), new Vector2(10, scrollBarHeight), myScrollbarColor);
				}

			}
		}

		public override void MouseMove(int x, int y)
		{
			if (x > Location.X && x < Location.X + Size.X && y > Location.Y && y < Location.Y + Size.Y)
			{
				CurrentlySelected = true;
				Renderer.SetCursor(CanScroll ? AvailableCursors.ScrollCursor : OpenTK.MouseCursor.Default); 
			}
			else
			{
				Renderer.SetCursor(OpenTK.MouseCursor.Default);
				CurrentlySelected = false;
			}
		}
	}
}
