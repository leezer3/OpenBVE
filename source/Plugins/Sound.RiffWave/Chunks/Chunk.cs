namespace Plugin
{
    internal abstract class Chunk
    {
	    internal byte[][] Buffers;

		internal int BytesPerSample;
	    internal Chunk(int numSamples)
	    {
		    Buffers = new byte[numSamples][];
	    }
    }
}
