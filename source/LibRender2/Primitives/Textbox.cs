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

		private List<string> WrappedLines(int width)
		{
			string[] firstSplit = Text.Split(new[] {"\r\n"}, StringSplitOptions.None);
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

			List<string> splitString = WrappedLines((int)Size.Y - Border * 2);
			if (splitString.Count == 1)
			{
				//DRAW SINGLE LINE
				renderer.OpenGlString.Draw(myFont, Text, new Vector2(Location.X + Border, Location.Y + Border), TextAlignment.TopLeft, myFontColor);
			}
			else
			{
				int maxFittingLines = (int)(Size.Y / myFont.MeasureString(Text).Y);
				if (topLine + maxFittingLines > splitString.Count)
				{
					topLine = Math.Max(0, splitString.Count - maxFittingLines);
				}
				//DRAW SPLIT LINES
				int currentLine = topLine;
				for (int i = 0; i < Math.Min(maxFittingLines, splitString.Count); i++)
				{
					renderer.OpenGlString.Draw(myFont, splitString[currentLine], new Vector2(Location.X + Border, Location.Y + Border + myFont.FontSize * i), TextAlignment.TopLeft, myFontColor);
					currentLine++;
				}
				
			}
		}

		
	}
}
