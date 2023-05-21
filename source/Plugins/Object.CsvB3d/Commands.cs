namespace Object.CsvB3d
{
	/// <summary>The commands found in a B3D / CSV file</summary>
	internal enum B3DCsvCommands
	{
		Unknown = 0,
		// B3D COMMANDS
		MeshBuilder,
		Vertex,
		Face,
		Face2,
		Texture,
		Color,
		ColorAll,
		EmissiveColor,
		EmissiveColorAll,
		Transparent,
		BlendingMode,
		BlendMode,
		WrapMode,
		Load,
		LightMap,
		Text,
		TextColor,
		BackgroundColor,
		TextPadding,
		Font,
		Coordinates,
		CrossFading,

		// CSV COMMANDS
		CreateMeshBuilder = 100,
		AddVertex,
		AddFace,
		AddFace2,
		// no equivilant of the [Texture] section in csv format
		SetColor,
		SetColorAll,
		SetEmissiveColor,
		SetEmissiveColorAll,
		SetDecalTransparentColor,
		SetBlendingMode,
		SetBlendMode,
		SetWrapMode,
		LoadTexture,
		LoadLightMap,
		SetText,
		SetTextColor,
		SetBackgroundColor,
		SetTextPadding,
		SetFont,
		SetTextureCoordinates,
		EnableCrossFading,

		// COMMON COMMANDS
		Cube = 200,
		Cylinder,
		Translate,
		TranslateAll,
		Scale,
		ScaleAll,
		Rotate,
		RotateAll,
		Shear,
		ShearAll,
		Mirror,
		MirrorAll,

		// JUNK COMMANDS
		GenerateNormals = 300, // required by legacy DirectX API, does nothing in OpenBVE
	}
}
