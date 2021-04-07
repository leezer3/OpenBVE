namespace Train.OpenBve.Panel2Cfg
{
	/// <summary>The different sections available for a Panel2.cfg</summary>
	internal enum Section
	{
		/// <summary>The panel bitmap</summary>
		This,
		/// <summary>An on-off lamp bound to an available subject</summary>
		PilotLamp,
		/// <summary>A needle type gauge bound to an available subject</summary>
		Needle,
		/// <summary>A linear type gauge bound to an available subject</summary>
		LinearGauge,
		/// <summary>A digital-strip gauge bound to an available subject</summary>
		DigitalNumber,
		/// <summary>A digital type gauge bound to an available subject</summary>
		DigitalGauge,
		/// <summary>A timetable bitmap overlay</summary>
		Timetable,
		/// <summary>The windscreen and wipers overlays</summary>
		Windscreen
	}
}
