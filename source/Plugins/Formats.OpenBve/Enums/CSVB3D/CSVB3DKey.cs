namespace Formats.OpenBve
{
	public enum CSVB3DKey
	{
		/// <summary>Adds a new vertex</summary>
		Vertex = int.MinValue,
		AddVertex = Vertex,
		/// <summary>Adds a new 1-sided face</summary>
		Face,
		AddFace = Face,
		/// <summary>Adds a new 2-sided face</summary>
		Face2,
		AddFace2 = Face2,
		/// <summary>Sets the color of all preceding faces in the MeshBuilder</summary>
		Color,
		SetColor = Color,
		/// <summary>Sets the color of all preceding faces in the object</summary>
		ColorAll,
		SetColorAll = ColorAll,
		/// <summary>Sets the emissive color of all preceding faces in the MeshBuilder</summary>
		EmissiveColor,
		SetEmissiveColor = EmissiveColor,
		/// <summary>Sets the emissive color for all preceding faces in the object</summary>
		EmissiveColorAll,
		SetEmissiveColorAll = EmissiveColorAll,
		/// <summary>Sets the texture co-ordinates for vertex N</summary>
		Coordinates,
		SetTextureCoordinates = Coordinates,
		/// <summary>Sets the light map co-ordinates for vertex N</summary>
		LightMapCoordinates,
		/// <summary>Sets the transparent color for the current texture</summary>
		Transparent,
		SetDecalTransparentColor = Transparent,
		/// <summary>Sets the texture blending mode for the current face</summary>
		BlendMode,
		BlendingMode = BlendMode,
		SetBlendMode = BlendMode,
		SetBlendingMode = BlendMode,
		/// <summary>Sets the texture wrapping mode for the current face</summary>
		WrapMode,
		SetWrapMode = WrapMode,
		/// <summary>Sets the light map texture for the current face</summary>
		LightMap,
		LoadLightMap = LightMap,
		/// <summary>Sets the text to be drawn on the current face</summary>
		Text,
		SetText = Text,
		/// <summary>Sets the color of the text to be drawn</summary>
		TextColor,
		SetTextColor = TextColor,
		/// <summary>Sets the background color of the text to be drawn</summary>
		BackgroundColor,
		SetBackgroundColor = BackgroundColor,
		/// <summary>Sets the padding of the text to be drawn</summary>
		TextPadding,
		SetTextPadding = TextPadding,
		/// <summary>Sets whether texture cross-fading is enabled on this face</summary>
		CrossFading,
		EnableCrossFading = CrossFading,
		/// <summary>Sets whether shadow casting is disabled for this face</summary>
		DisableShadowCasting,
		/// <summary>Loads a texture</summary>
		Load,
		LoadTexture = Load,
		/// <summary>Creates a cylinder</summary>
		Cylinder,
		/// <summary>Creates a cube</summary>
		Cube,
		/// <summary>Applies translation to all preceding faces in the MeshBuilder</summary>
		Translate,
		/// <summary>Applies translation to all preceding faces in the object</summary>
		TranslateAll,
		/// <summary>Applies rotation to all preceding faces in the MeshBuilder</summary>
		Rotate,
		/// <summary>Applies rotation to all preceding faces in the object</summary>
		RotateAll,
		/// <summary>Applies scale to all preceding faces in the MeshBuilder</summary>
		Scale,
		/// <summary>Applies scale to all preceding faces in the object</summary>
		ScaleAll,
		/// <summary>Applies shear to all preceding faces in the MeshBuilder</summary>
		Shear,
		/// <summary>Applies shear to all preceding faces in the object</summary>
		ShearAll,
		/// <summary>The direction to apply shear</summary>
		ShearDirection,
		/// <summary>The sheer stress vector to apply</summary>
		ShearStress,
		/// <summary>Mirrors all preceding faces in the MeshBuilder</summary>
		Mirror,
		/// <summary>Mirrors all preceding faces in the object</summary>
		MirrorAll,
		/// <summary>Whether vertices are to be mirrored</summary>
		MirrorVertices,
		/// <summary>Whether normals are to be mirrored</summary>
		MirrorNormals,
		/// <summary>Unused by OpenBVE</summary>
		GenerateNormals,
		
	}
}
