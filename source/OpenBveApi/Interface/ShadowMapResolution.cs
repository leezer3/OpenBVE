namespace OpenBveApi.Interface
{
    /// <summary>Available shadow map resolution presets.</summary>
    public enum ShadowMapResolution
    {
        /// <summary>Shadows disabled entirely.</summary>
        Off = 0,
        /// <summary>512×512 per cascade — minimal quality, best performance.</summary>
        Low = 512,
        /// <summary>1024×1024 per cascade — balanced.</summary>
        Medium = 1024,
        /// <summary>2048×2048 per cascade — high quality.</summary>
        High = 2048,
        /// <summary>4096×4096 per cascade — ultra quality, GPU-heavy.</summary>
        Ultra = 4096
    }

    /// <summary>Available shadow distance presets.</summary>
    public enum ShadowDistance
    {
        /// <summary>Matches the camera viewing distance.</summary>
        ViewingDistance = -1,
        /// <summary>150m — close shadows only, best performance.</summary>
        Near = 150,
        /// <summary>300m — balanced distance.</summary>
        Medium = 300,
        /// <summary>500m — far shadows, higher GPU cost.</summary>
        Far = 500,
        /// <summary>800m — maximum distance.</summary>
        VeryFar = 800
    }

    /// <summary>Available shadow cascade counts.</summary>
    public enum ShadowCascadeCount
    {
        /// <summary>2 cascades — fastest.</summary>
        Two = 2,
        /// <summary>3 cascades — balanced quality/performance.</summary>
        Three = 3,
        /// <summary>4 cascades — best quality gradient.</summary>
        Four = 4
    }
}
