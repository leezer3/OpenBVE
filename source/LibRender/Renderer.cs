namespace LibRender
{
    public class Renderer
    {
		/// <summary>Holds the lock for GDI Plus functions</summary>
		public static readonly object gdiPlusLock = new object();
    }
}
