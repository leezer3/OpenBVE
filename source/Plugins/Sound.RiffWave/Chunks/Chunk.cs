using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    internal abstract class Chunk
    {
	    internal byte[][] Buffers;

	    internal Chunk(int numSamples)
	    {
		    Buffers = new byte[numSamples][];
	    }
    }
}
