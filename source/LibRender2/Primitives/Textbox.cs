using System;
using System.Collections.Generic;
using System.Drawing;
using LibRender2.Texts;
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
		/// <summary>The string contents of the textbox</summary>
		public string Text;
		/// <summary>The border width of the textbox </summary>
		public readonly int Border;

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
					if (myFont.MeasureString(currentLine).Width > width)
					{
						// Exceeded length, back up to last space
						int moveback = 0;
						while (currentChar != ' ')
						{
							moveback++;
							i--;
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

		public Textbox(BaseRenderer Renderer, OpenGlFont Font)
		{
			renderer = Renderer;
			myFont = Font;
			Border = 5;
		}

		public void Draw(Texture texture, Vector2 point, Vector2 size, Color128? color)
		{
			renderer.LastBoundTexture = null;
			renderer.Rectangle.Draw(texture, point, size, color); //Draw the backing rectangle first
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}

			List<string> splitString = WrappedLines((int)size.Y - Border * 2);
			if (splitString.Count == 1)
			{
				//DRAW SINGLE LINE
				renderer.OpenGlString.Draw(myFont, Text, new Point((int)point.X + Border, (int)point.Y + Border), TextAlignment.TopLeft, Color128.White);
			}
			else
			{
				//DRAW SPLIT LINES
				for (int i = 0; i < splitString.Count; i++)
				{
					renderer.OpenGlString.Draw(myFont, splitString[i], new Point((int)point.X + Border, (int)(point.Y + Border + myFont.FontSize * i)), TextAlignment.TopLeft, Color128.White);
				}
				
			}
		}

		
	}
}
