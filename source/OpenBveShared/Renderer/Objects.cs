using OpenBveApi.Graphics;

namespace OpenBveShared
{
	public static partial class Renderer
	{
		/// <summary>Re-adds all objects within the world, for example after a screen resolution change</summary>
		public static void ReAddObjects(TransparencyMode Mode)
		{
			RendererObject[] list = new RendererObject[Renderer.ObjectCount];
			for (int i = 0; i < Renderer.ObjectCount; i++)
			{
				list[i] = Renderer.Objects[i];
			}
			for (int i = 0; i < list.Length; i++)
			{
				HideObject(list[i].ObjectIndex);
			}
			for (int i = 0; i < list.Length; i++)
			{
				ShowObject(list[i].ObjectIndex, list[i].Type, Mode);
			}
		}
	}
}
