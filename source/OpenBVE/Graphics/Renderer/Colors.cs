using OpenBveApi.Colors;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>Creates the background color for anti-aliasing text</summary>
		/// <param name="Original">The original background color</param>
		/// <param name="SystemColor">The color of the message text</param>
		/// <param name="R">Receives the value of the red component of SystemColor.</param>
		/// <param name="G">Receives the value of the green component of SystemColor.</param>
		/// <param name="B">Receives the value of the blue component of SystemColor.</param>
		/// <param name="A">Receives the value of the alpha component of SystemColor.</param>
		private static void CreateBackColor(Color32 Original, MessageColor SystemColor, out float R, out float G, out float B, out float A)
		{
			if (Original.R == 0 & Original.G == 0 & Original.B == 0)
			{
				switch (SystemColor)
				{
					case MessageColor.Black:
						R = 0.0f; G = 0.0f; B = 0.0f;
						break;
					case MessageColor.Gray:
						R = 0.4f; G = 0.4f; B = 0.4f;
						break;
					case MessageColor.White:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
					case MessageColor.Red:
						R = 1.0f; G = 0.0f; B = 0.0f;
						break;
					case MessageColor.Orange:
						R = 0.9f; G = 0.7f; B = 0.0f;
						break;
					case MessageColor.Green:
						R = 0.2f; G = 0.8f; B = 0.0f;
						break;
					case MessageColor.Blue:
						R = 0.0f; G = 0.7f; B = 1.0f;
						break;
					case MessageColor.Magenta:
						R = 1.0f; G = 0.0f; B = 0.7f;
						break;
					default:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
				}
			}
			else
			{
				R = inv255 * (float)Original.R;
				G = inv255 * (float)Original.G;
				B = inv255 * (float)Original.B;
			}
			A = inv255 * (float)Original.A;
		}

		/// <summary>Creates the foreground color for anti-aliasing text</summary>
		/// <param name="Original">The original background color</param>
		/// <param name="SystemColor">The color of the message text</param>
		/// <param name="R">Receives the value of the red component of SystemColor.</param>
		/// <param name="G">Receives the value of the green component of SystemColor.</param>
		/// <param name="B">Receives the value of the blue component of SystemColor.</param>
		/// <param name="A">Receives the value of the alpha component of SystemColor.</param>
		private static void CreateTextColor(Color32 Original, MessageColor SystemColor, out float R, out float G, out float B, out float A)
		{
			if (Original.R == 0 & Original.G == 0 & Original.B == 0)
			{
				switch (SystemColor)
				{
					case MessageColor.Black:
						R = 0.0f; G = 0.0f; B = 0.0f;
						break;
					case MessageColor.Gray:
						R = 0.4f; G = 0.4f; B = 0.4f;
						break;
					case MessageColor.White:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
					case MessageColor.Red:
						R = 1.0f; G = 0.0f; B = 0.0f;
						break;
					case MessageColor.Orange:
						R = 0.9f; G = 0.7f; B = 0.0f;
						break;
					case MessageColor.Green:
						R = 0.3f; G = 1.0f; B = 0.0f;
						break;
					case MessageColor.Blue:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
					case MessageColor.Magenta:
						R = 1.0f; G = 0.0f; B = 0.7f;
						break;
					default:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
				}
			}
			else
			{
				R = inv255 * (float)Original.R;
				G = inv255 * (float)Original.G;
				B = inv255 * (float)Original.B;
			}
			A = inv255 * (float)Original.A;
		}
	}
}
