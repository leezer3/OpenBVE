namespace Plugin
{
    internal class SilentChunk : Chunk
    {
	    internal int NumSamples;
		
	    public SilentChunk(uint numSamples, WaveFormatEx format) : base(format.Channels)
	    {
		    NumSamples = (int)numSamples;
		    int bytesPerSample;

		    if (!(format is WaveFormatAdPcm))
		    {
			    bytesPerSample = format.BitsPerSample / 8;
		    }
		    else
		    {
			    bytesPerSample = 2;
		    }
			
		    for (int i = 0; i < format.Channels; i++)
		    {
			    Buffers[i] = new byte[numSamples * bytesPerSample];
		    }
		}
    }
}
