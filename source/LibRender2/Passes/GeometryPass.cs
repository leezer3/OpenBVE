using System.Collections.Generic;
using System.Linq;
using LibRender2.Objects;
using LibRender2.Pipeline;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Passes
{
	/// <summary>
	/// Renders the opaque and alpha-tested world geometry.
	/// </summary>
	public class GeometryPass : IRenderPass
	{
		public void Execute(RenderContext context)
		{
			BaseRenderer renderer = context.Renderer;

			// 1. Setup lighting and fog for world geometry
			if (renderer.AvailableNewRenderer)
			{
				if (renderer.OptionLighting)
				{
					renderer.DefaultShader.SetIsLight(true);
					renderer.DefaultShader.SetLightPosition(renderer.TransformedLightPosition);
					renderer.DefaultShader.SetLightAmbient(renderer.Lighting.OptionAmbientColor);
					renderer.DefaultShader.SetLightDiffuse(renderer.Lighting.OptionDiffuseColor);
					renderer.DefaultShader.SetLightSpecular(renderer.Lighting.OptionSpecularColor);
					renderer.DefaultShader.SetLightModel(renderer.Lighting.LightModel);
				}
				renderer.Fog.Set();
				renderer.DefaultShader.SetTexture(0);
				renderer.CurrentProjectionMatrix = context.ProjectionMatrix;
				renderer.CurrentViewMatrix = context.ViewMatrix;
				renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
				renderer.DefaultShader.SetCurrentViewMatrix(renderer.CurrentViewMatrix);
				renderer.BindCSMToDefaultShader();
			}

			renderer.ResetOpenGlState();
			GL.DepthFunc(DepthFunction.Lequal);

			if (renderer.OptionWireFrame)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			}

			List<FaceState> alphaFaces;
			lock (renderer.Scene.VisibleObjects.LockObject)
			{
				// 2. Render Opaque Faces with Batching
				int opaqueCount = renderer.Scene.VisibleObjects.OpaqueFaces.Count;
				for (int i = 0; i < opaqueCount; i++)
				{
					FaceState firstFace = renderer.Scene.VisibleObjects.OpaqueFaces[i];
					int batchCount = firstFace.Face.Vertices.Length;
					int j = i + 1;

					// Try to batch contiguous faces from the same object and material
					while (j < opaqueCount)
					{
						FaceState nextFace = renderer.Scene.VisibleObjects.OpaqueFaces[j];
						if (nextFace.Object == firstFace.Object && nextFace.Face.Material == firstFace.Face.Material)
						{
							// Check if they are contiguous in the index buffer
							if (nextFace.Face.IboStartIndex == firstFace.Face.IboStartIndex + batchCount)
							{
								batchCount += nextFace.Face.Vertices.Length;
								j++;
								continue;
							}
						}
						break;
					}

					if (j > i + 1)
					{
						// Draw the batch
						renderer.RenderFace(renderer.DefaultShader, firstFace.Object, firstFace.Face, vertexCount: batchCount);
						i = j - 1;
					}
					else
					{
						firstFace.Draw();
					}
				}
				alphaFaces = renderer.Scene.VisibleObjects.GetSortedPolygons();
			}


			// 3. Render Alpha Faces
			renderer.ResetOpenGlState();
			GL.DepthFunc(DepthFunction.Lequal);

			if (renderer.currentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				renderer.SetBlendFunc();
				renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				renderer.SetDepthMask(false);

				foreach (FaceState face in alphaFaces)
				{
					face.Draw();
				}
			}
			else
			{
				// Quality Transparency Mode
				renderer.UnsetBlendFunc();
				renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				renderer.SetDepthMask(true);

				foreach (FaceState face in alphaFaces)
				{
					var material = face.Object.Prototype.Mesh.Materials[face.Face.Material];
					if (material.BlendMode == MeshMaterialBlendMode.Normal && material.GlowAttenuationData == 0)
					{
						if (material.Color.A == 255)
						{
							face.Draw();
						}
					}
				}

				renderer.SetBlendFunc();
				renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				renderer.SetDepthMask(false);
				bool additive = false;

				foreach (FaceState face in alphaFaces)
				{
					var material = face.Object.Prototype.Mesh.Materials[face.Face.Material];
					if (material.BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							renderer.UnsetAlphaFunc();
							additive = true;
						}
					}
					else
					{
						if (additive)
						{
							renderer.SetAlphaFunc();
							additive = false;
						}
					}
					face.Draw();
				}
			}
			
			// Restore default depth mask
			renderer.SetDepthMask(true);

			if (renderer.OptionWireFrame)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
		}
	}
}
