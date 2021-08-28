namespace LibRender2.Screens
{
	/// <summary>A screen resolution</summary>
	public class ScreenResolution
	{
		/// <summary>The width</summary>
		public readonly int Width;
		/// <summary>The height</summary>
		public readonly int Height;

		public ScreenResolution(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public override string ToString()
		{
			return Width + " x " + Height;
		}
	}
}
