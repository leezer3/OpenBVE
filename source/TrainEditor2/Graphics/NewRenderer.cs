using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenTK.Graphics.OpenGL;

namespace TrainEditor2.Graphics
{
	internal class NewRenderer : BaseRenderer
	{
		public override void Initialize()
		{
			base.Initialize();
			GL.Disable(EnableCap.CullFace);
		}

		public NewRenderer(HostInterface CurrentHost, BaseOptions CurrentOptions, FileSystem FileSystem) : base(CurrentHost, CurrentOptions, FileSystem)
		{
		}
	}
}
