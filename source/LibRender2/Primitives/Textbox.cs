using System;
using System.Collections.Generic;
using LibRender2.Text;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Textures;

namespace LibRender2.Primitives
{
	public class Textbox
	{
		/// <summary>Holds a reference to the base renderer</summary>
		private readonly BaseRenderer renderer;
		/// <summary>The font the items in this textbox are to be drawn with</summary>
		private readonly OpenGlFont myFont;
		/// <summary>The font color</summary>
		private readonly Color128 myFontColor;
		/// <summary>The color of the scrollbar handle</summary>
		private readonly Color128 myScrollbarColor;
		/// <summary>The string contents of the textbox</summary>
		public string Text
		{
			get
			{
				return myText;
			}
			set
			{
				myText = value;
				//reset the scroll value
				topLine = 0;
			}
		}
		/// <summary>The background texture</summary>
		public Texture BackgroundTexture;
		/// <summary>The background color</summary>
		public Color128 BackgroundColor;
		/// <summary>Backing property for the textbox text</summary>
		private string myText;

		/// <summary>The border width of the textbox </summary>
		public readonly int Border;
		/// <summary>The top line to be renderered</summary>
		private int topLine;
		/// <summary>The stored location for the textbox</summary>
		public Vector2 Location;
		/// <summary>The stored size for the textbox</summary>
		public Vector2 Size;
		/// <summary>Whether the textbox is currently selected by the mouse</summary>
		public bool CurrentlySelected;
		/// <summary>Whether the textbox can scroll</summary>
		public bool CanScroll;
		/// <summary>Used for internal size calculations</summary>
		private Vector2 internalSize
		{
			get
			{
				if (CanScroll)
				{
					return new Vector2(Size.X, Size.Y - 12);
				}
				return Size;
			}
		}

		private List<string> WrappedLines(int width)
		{
			string[] firstSplit = Text.Split(new[] {"\r\n", @"\r\n"}, StringSplitOptions.None);
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
						// Exceeded length, back up to last space
						int moveback = 0;
						int lastChar = i - 1;
						while (currentChar != ' ')
						{
							moveback++;
							i--;
							if (i == 0)
							{
								//No spaces found, so just drop back one char
								i = lastChar;
								moveback = 1;
								break;
							}
							currentChar = firstSplit[j][i];
						}
						string lineToAdd = currentLine.Substring(0, currentLine.Length - moveback);
						wrappedLines.Add(lineToAdd.TrimStart());
						currentLine = string.Empty;
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

		public Textbox(BaseRenderer Renderer, OpenGlFont Font, Color128 FontColor, Color128 backgroundColor)
		{
			renderer = Renderer;
			myFont = Font;
			myFontColor = FontColor;
			Border = 5;
			topLine = 0;
			BackgroundTexture = null;
			BackgroundColor = backgroundColor;
			myScrollbarColor = new Color128(1.0f, 0.69f, 0.0f, 1.0f);
		}

		public void VerticalScroll(int numberOfLines)
		{
			topLine += numberOfLines;
			if (topLine < 0)
			{
				topLine = 0;
			}
		}

		public void Draw()
		{
			renderer.Rectangle.Draw(BackgroundTexture, Location, Size, BackgroundColor); //Draw the backing rectangle first
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}

			List<string> splitString = WrappedLines((int)internalSize.Y - Border * 2);
			if (splitString.Count == 1)
			{
				//DRAW SINGLE LINE
				renderer.OpenGlString.Draw(myFont, Text, new Vector2(Location.X + Border, Location.Y + Border), TextAlignment.TopLeft, myFontColor);
				CanScroll = false;
			}
			else
			{
				int maxFittingLines = (int)(internalSize.Y / myFont.MeasureString(Text).Y);
				if (topLine + maxFittingLines > splitString.Count)
				{
					topLine = Math.Max(0, splitString.Count - maxFittingLines);
				}
				CanScroll = maxFittingLines < splitString.Count;
				if (CanScroll)
				{
					renderer.Rectangle.Draw(null, new Vector2(Location.X + Size.X - 12, Location.Y + 2), new Vector2(8, Size.Y - 4), Color128.Grey); //Backing rectangle
				}
				//DRAW SPLIT LINES
				int currentLine = topLine;
				int bottomLine = Math.Min(maxFittingLines, splitString.Count);
				for (int i = 0; i < bottomLine; i++)
				{
					renderer.OpenGlString.Draw(myFont, splitString[currentLine], new Vector2(Location.X + Border, Location.Y + Border + myFont.FontSize * i), TextAlignment.TopLeft, myFontColor);
					currentLine++;
				}

				if (CanScroll)
				{
					double percentageScroll = topLine / (double)(splitString.Count - maxFittingLines);
					renderer.Rectangle.Draw(null, new Vector2(Location.X + Size.X - 13, Location.Y + (Size.Y - 14) * percentageScroll), new Vector2(10, 10), myScrollbarColor);
				}

			}
		}

		
	}
}
