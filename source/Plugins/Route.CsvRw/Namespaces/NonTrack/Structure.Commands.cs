namespace CsvRwRouteParser
{
	internal enum StructureCommand
	{
		/// <summary>The object used for RailType N</summary>
		Rail,
		/// <summary>The object used for BeaconType N</summary>
		Beacon,
		/// <summary>The object used for PoleType N</summary>
		Pole,
		/// <summary>The object used for Ground N</summary>
		Ground,
		/// <summary>The left Wall object for type N</summary>
		WallL,
		/// <summary>The right Wall object for type N</summary>
		WallR,
		/// <summary>The left Dike object for type N</summary>
		DikeL,
		/// <summary>The right Dike object for type N</summary>
		DikeR,
		/// <summary>The left Form object for type N</summary>
		FormL,
		/// <summary>The right Form object for type N</summary>
		FormR,
		/// <summary>The left FormCenter object for type N</summary>
		FormCL,
		/// <summary>The right FormCenter object for type N</summary>
		FormCR,
		/// <summary>The left FormRoof object for type N</summary>
		RoofL,
		/// <summary>The right FormRoof object for type N</summary>
		RoofR,
		/// <summary>The left FormRoofCenter object for type N</summary>
		RoofCL,
		/// <summary>The right FormRoofCenter object for type N</summary>
		RoofCR,
		/// <summary>The left Crack object for type N</summary>
		CrackL,
		/// <summary>The right Crack object for type N</summary>
		CrackR,
		/// <summary>The object used for FreeObject N</summary>
		FreeObj,
		/// <summary>The image / object used for Background N</summary>
		Background,
		/// <summary>The image / object used for Background N</summary>
		Back,
		/// <summary>The image / object used for Background N</summary>
		BackgroundX,
		/// <summary>The image / object used for Background N</summary>
		BackX,
		/// <summary>If Background N is an image, sets the aspect ratio used in wrapping</summary>
		BackgroundAspect,
		/// <summary>If Background N is an image, sets the aspect ratio used in wrapping</summary>
		BackAspect,
		/// <summary>The object used for RainType N</summary>
		Weather,
		/// <summary>Loads a dynamic lighting set</summary>
		DynamicLight,

		/*
		 * HMMSIM
		 */
		/// <summary>The object used for Object N</summary>
		/// <remarks>Equivilant to .FreeObj</remarks>
		Object

	}
}
