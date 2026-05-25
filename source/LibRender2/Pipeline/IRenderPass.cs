namespace LibRender2.Pipeline
{
	/// <summary>
	/// Represents a single stage in the rendering pipeline.
	/// </summary>
	public interface IRenderPass
	{
		/// <summary>
		/// Executes the rendering logic for this pass.
		/// </summary>
		/// <param name="context">The current rendering context.</param>
		void Execute(RenderContext context);
	}
}
