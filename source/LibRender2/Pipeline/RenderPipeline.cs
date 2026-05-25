using System.Collections.Generic;

namespace LibRender2.Pipeline
{
	/// <summary>
	/// Orchestrates the execution of multiple rendering passes.
	/// </summary>
	public class RenderPipeline
	{
		/// <summary>
		/// The list of passes in this pipeline, executed in order.
		/// </summary>
		public readonly List<IRenderPass> Passes = new List<IRenderPass>();

		/// <summary>
		/// Executes all registered passes in the pipeline.
		/// </summary>
		/// <param name="context">The current rendering context.</param>
		public void Execute(RenderContext context)
		{
			foreach (var pass in Passes)
			{
				pass.Execute(context);
			}
		}

		/// <summary>
		/// Adds a pass to the end of the pipeline.
		/// </summary>
		public void AddPass(IRenderPass pass)
		{
			Passes.Add(pass);
		}

		/// <summary>
		/// Clears all passes from the pipeline.
		/// </summary>
		public void Clear()
		{
			Passes.Clear();
		}
	}
}
