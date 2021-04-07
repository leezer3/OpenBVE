namespace Train.OpenBve.PanelCfg
{
	/// <summary>The different sections available for a Panel.cfg</summary>
	internal enum Section
	{
		/// <summary>The panel bitmap</summary>
		Panel = 0,
		/// <summary>Sets up the pitch and yaw of the panel</summary>
		View = 1,
		/// <summary>A dial type gauge indicating brake pressure</summary>
		PressureGauge = 2,
		PressureMeter = 2,
		PressureIndicator = 2,
		圧力計 = 2,
		/// <summary>A dial type gauge indicating speed</summary>
		Speedometer = 3,
		SpeedIndicator = 3,
		速度計 = 3,
		/// <summary>A digital-strip gauge bound to an available subject</summary>
		DigitalIndicator = 4,
		デジタル速度計 = 4,
		/// <summary>A lamp lit when the train is ready to start</summary>
		PilotLamp = 5,
		知らせ灯 = 5,
		/// <summary>A dial type gauge indicating time</summary>
		Watch = 6,
		時計 = 7,
		/// <summary>A digital-strip gauge indicating the current brake value</summary>
		BrakeIndicator = 7,
		ハンドルの段表示 = 7
	}
}
