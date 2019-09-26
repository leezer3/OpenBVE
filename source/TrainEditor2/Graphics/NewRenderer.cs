using LibRender2;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenTK.Graphics.OpenGL;

namespace TrainEditor2.Graphics
{
	internal class NewRenderer : BaseRenderer
	{
		public override void Initialize(HostInterface CurrentHost, BaseOptions CurrentOptions)
		{
			base.Initialize(CurrentHost, CurrentOptions);

			GL.Disable(EnableCap.CullFace);
		}
	}
}
