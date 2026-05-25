using OpenTK.Graphics.OpenGL;

namespace LibRender2.Pipeline
{
	/// <summary>
	/// Manages and caches the OpenGL state to prevent redundant API calls.
	/// </summary>
	public class GraphicsDevice
	{
		private bool blendEnabled;
		private BlendingFactor blendSrcFactor;
		private BlendingFactor blendDestFactor;

		private bool depthTestEnabled = true;
		private DepthFunction depthFunction = DepthFunction.Lequal;
		private bool depthMask = true;

		private bool cullFaceEnabled = true;
		private CullFaceMode cullFaceMode = CullFaceMode.Front;

		/// <summary>
		/// Sets the blending state.
		/// </summary>
		public void SetBlend(bool enabled, BlendingFactor src = BlendingFactor.SrcAlpha, BlendingFactor dest = BlendingFactor.OneMinusSrcAlpha)
		{
			if (blendEnabled != enabled)
			{
				blendEnabled = enabled;
				if (enabled) GL.Enable(EnableCap.Blend);
				else GL.Disable(EnableCap.Blend);
			}

			if (enabled && (blendSrcFactor != src || blendDestFactor != dest))
			{
				blendSrcFactor = src;
				blendDestFactor = dest;
				GL.BlendFunc(src, dest);
			}
		}

		/// <summary>
		/// Sets the depth test state.
		/// </summary>
		public void SetDepthTest(bool enabled, DepthFunction function = DepthFunction.Lequal)
		{
			if (depthTestEnabled != enabled)
			{
				depthTestEnabled = enabled;
				if (enabled) GL.Enable(EnableCap.DepthTest);
				else GL.Disable(EnableCap.DepthTest);
			}

			if (enabled && depthFunction != function)
			{
				depthFunction = function;
				GL.DepthFunc(function);
			}
		}

		/// <summary>
		/// Sets whether depth writing is enabled.
		/// </summary>
		public void SetDepthMask(bool enabled)
		{
			if (depthMask != enabled)
			{
				depthMask = enabled;
				GL.DepthMask(enabled);
			}
		}

		/// <summary>
		/// Sets the face culling state.
		/// </summary>
		public void SetCullFace(bool enabled, CullFaceMode mode = CullFaceMode.Front)
		{
			if (cullFaceEnabled != enabled)
			{
				cullFaceEnabled = enabled;
				if (enabled) GL.Enable(EnableCap.CullFace);
				else GL.Disable(EnableCap.CullFace);
			}

			if (enabled && cullFaceMode != mode)
			{
				cullFaceMode = mode;
				GL.CullFace(mode);
			}
		}

		/// <summary>
		/// Resets the cached state to match the current OpenGL defaults or a known state.
		/// </summary>
		public void Reset()
		{
			// Reset to OpenGL defaults
			GL.Disable(EnableCap.Blend);
			blendEnabled = false;

			GL.Enable(EnableCap.DepthTest);
			depthTestEnabled = true;
			GL.DepthFunc(DepthFunction.Less);
			depthFunction = DepthFunction.Less;

			GL.DepthMask(true);
			depthMask = true;

			GL.Enable(EnableCap.CullFace);
			cullFaceEnabled = true;
			GL.CullFace(CullFaceMode.Back);
			cullFaceMode = CullFaceMode.Back;
		}
	}
}
