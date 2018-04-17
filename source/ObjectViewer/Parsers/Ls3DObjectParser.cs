using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Linq;
using OpenBveApi.Textures;

namespace OpenBve
{
	/// <summary>Parses a Loksim3D xml format object</summary>
    internal static class Ls3DObjectParser
    {
        // structures
        private class Material
        {
            internal Color32 Color;
            internal Color24 EmissiveColor;
            internal bool EmissiveColorUsed;
            internal Color24 TransparentColor;
            internal bool TransparentColorUsed;
            internal string DaytimeTexture;
	        internal string TransparencyTexture;
            internal string NighttimeTexture;
            internal World.MeshMaterialBlendMode BlendMode;
            internal ushort GlowAttenuationData;
            internal Material()
            {
                this.Color = new Color32(255, 255, 255, 255);
                this.EmissiveColor = new Color24(0, 0, 0);
                this.EmissiveColorUsed = false;
                this.TransparentColor = new Color24(0, 0, 0);
                this.TransparentColorUsed = false;
                this.DaytimeTexture = null;
                this.NighttimeTexture = null;
                this.BlendMode = World.MeshMaterialBlendMode.Normal;
                this.GlowAttenuationData = 0;
            }
            internal Material(Material Prototype)
            {
                this.Color = Prototype.Color;
                this.EmissiveColor = Prototype.EmissiveColor;
                this.EmissiveColorUsed = Prototype.EmissiveColorUsed;
                this.TransparentColor = Prototype.TransparentColor;
                this.TransparentColorUsed = Prototype.TransparentColorUsed;
                this.DaytimeTexture = Prototype.DaytimeTexture;
                this.NighttimeTexture = Prototype.NighttimeTexture;
                this.BlendMode = Prototype.BlendMode;
                this.GlowAttenuationData = Prototype.GlowAttenuationData;
            }
        }
        private class MeshBuilder
        {
            internal World.Vertex[] Vertices;
            internal World.MeshFace[] Faces;
            internal Material[] Materials;
            internal MeshBuilder()
            {
                this.Vertices = new World.Vertex[] { };
                this.Faces = new World.MeshFace[] { };
                this.Materials = new Material[] { new Material() };
            }
        }

		/// <summary>Loads a Loksim3D object from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <param name="LoadMode">The texture load mode.</param>
		/// <param name="ForceTextureRepeatX">Whether to force TextureWrapMode.Repeat for the X-axis</param>
		/// <param name="ForceTextureRepeatY">Whether to force TextureWrapMode.Repeat for the Y-axis</param>
		/// <param name="Rotation">A three-dimemsional vector describing the rotation to be applied</param>
		/// <returns>The object loaded.</returns>
		internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding,ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY, Vector3 Rotation)
        {
	        string BaseDir = System.IO.Path.GetDirectoryName(FileName);
			XmlDocument currentXML = new XmlDocument();
            //Initialise the object
            ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Faces = new World.MeshFace[] { };
			Object.Mesh.Materials = new World.MeshMaterial[] { };
			Object.Mesh.Vertices = new World.Vertex[] { };
            MeshBuilder Builder = new MeshBuilder();
			Vector3[] Normals = new Vector3[4];
            bool PropertiesFound = false;

            World.Vertex[] tempVertices = new World.Vertex[0];
            Vector3[] tempNormals = new Vector3[0];
            Color24 transparentColor = new Color24();
            string tday = null;
	        string transtex = null;
            string tnight = null;
            bool TransparencyUsed = false;
	        bool TransparentTypSet = false;
	        bool FirstPxTransparent = false;
	        Color24 FirstPxColor = new Color24();
            bool Face2 = false;
            int TextureWidth = 0;
            int TextureHeight = 0;
            if (File.Exists(FileName))
            {
	            try
	            {
					currentXML.Load(FileName);
	            }
	            catch
	            {
		            return null;
	            }
                
            }
            else
            {
	            Interface.AddMessage(Interface.MessageType.Error, false, "Loksim3D object " + FileName + " does not exist.");
				return null;
            }
            //Check for null
            if (currentXML.DocumentElement != null)
            {
                XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/OBJECT");
                //Check this file actually contains Loksim3D object nodes
                if (DocumentNodes != null)
                {
                    foreach (XmlNode outerNode in DocumentNodes)
                    {
                        if (outerNode.ChildNodes.OfType<XmlElement>().Any())
                        {
                            foreach (XmlNode node in outerNode.ChildNodes)
                            {
                                //I think there should only be one properties node??
                                //Need better format documentation
                                if (node.Name == "Props" && PropertiesFound == false)
                                {
                                    if (node.Attributes != null)
                                    {
                                        //Our node has child nodes, therefore this properties node should be valid
                                        //Needs better validation
                                        PropertiesFound = true;
                                        foreach (XmlAttribute attribute in node.Attributes)
                                        {
                                            switch (attribute.Name)
                                            {
												//Sets the texture
												//Loksim3D objects only support daytime textures
												case "Texture":
													tday = OpenBveApi.Path.Loksim3D.CombineFile(BaseDir, attribute.Value, Program.FileSystem.LoksimPackageInstallationDirectory);
													if (!File.Exists(tday))
													{
														Interface.AddMessage(Interface.MessageType.Warning, true, "Ls3d Texture file " + attribute.Value + " not found.");
														break;
													}
													try
													{
														using (Bitmap TextureInformation = new Bitmap(tday))
														{
															TextureWidth = TextureInformation.Width;
															TextureHeight = TextureInformation.Height;
															Color color = TextureInformation.GetPixel(0, 0);
															FirstPxColor = new Color24(color.R, color.G, color.B);
														}
													}
													catch
													{
														Interface.AddMessage(Interface.MessageType.Error, true,
															"An error occured loading daytime texture " + tday +
															" in file " + FileName);
														tday = null;
													}
													break;
												//Defines whether the texture uses transparency
												//May be omitted
												case "Transparent":
													if (TransparentTypSet)
													{
														//Appears to be ignored with TransparentTyp set
														continue;
													}
													if (attribute.Value == "TRUE")
													{
														TransparencyUsed = true;
														transparentColor = new Color24(0,0,0);
													}
                                                    break;
	                                            case "TransTexture":
		                                            if (string.IsNullOrEmpty(attribute.Value))
		                                            {
			                                            //Empty....
			                                            continue;
		                                            }
		                                            transtex = OpenBveApi.Path.Loksim3D.CombineFile(BaseDir, attribute.Value, Program.FileSystem.LoksimPackageInstallationDirectory);
		                                            if (!File.Exists(transtex))
		                                            {
			                                            Interface.AddMessage(Interface.MessageType.Error, true, "AlphaTexture " + transtex + " could not be found in file " + FileName);
			                                            transtex = null;
													}
		                                            break;
												//Sets the transparency type
												case "TransparentTyp":
	                                                TransparentTypSet = true;
                                                    switch (attribute.Value)
                                                    {
                                                        case "0":
                                                            //Transparency is disabled
                                                            TransparencyUsed = false;
                                                            break;
                                                        case "1":
                                                            //Transparency is solid black
                                                            TransparencyUsed = true;
                                                            transparentColor = new Color24(0,0,0);
	                                                        FirstPxTransparent = false;
                                                            break;
                                                        case "2":
                                                            //Transparency is the color at Pixel 1,1
                                                            TransparencyUsed = true;
	                                                        FirstPxTransparent = true;
                                                            break;
                                                        case "3":
														case "4":
                                                            //This is used when transparency is used with an alpha bitmap
                                                            TransparencyUsed = false;
	                                                        FirstPxTransparent = false;
                                                            break;
														case "5":
															//Use the alpha channel from the image, so we don't need to do anything fancy
															//TODO: (Low priority) Check what happens in Loksim itself when an image uses the Alpha channel, but doesn't actually specify type 5
															break;
														default:
															Interface.AddMessage(Interface.MessageType.Error, false, "Unrecognised transparency type " + attribute.Value + " detected in " + attribute.Name + " in Loksim3D object file " + FileName);
															break;
                                                    }
                                                    break;
                                                //Sets whether the rears of the faces are to be drawn
                                                case "Drawrueckseiten":
													if (attribute.Value == "TRUE" || string.IsNullOrEmpty(attribute.Value))
													{
														Face2 = true;
													}
													else
													{
														Face2 = false;
													}
													break;

                                        /*
                                         * MISSING PROPERTIES:
                                         * AutoRotate - Rotate with tracks?? LS3D presumably uses a 3D world system.
                                         * Beleuchtet- Translates as illuminated. Presume something to do with lighting? - What emissive color?
                                         * FileAuthor
                                         * FileInfo
                                         * FilePicture
                                         */
                                            }
                                        }
                                    }
                                }
                                //The point command is eqivilant to a vertex
                                else if (node.Name == "Point" && node.ChildNodes.OfType<XmlElement>().Any())
                                {
                                    foreach (XmlNode childNode in node.ChildNodes)
                                    {
                                        if (childNode.Name == "Props" && childNode.Attributes != null)
                                        {
                                            //Vertex
                                            double vx = 0.0, vy = 0.0, vz = 0.0;
                                            //Normals
                                            double nx = 0.0, ny = 0.0, nz = 0.0;
                                            foreach (XmlAttribute attribute in childNode.Attributes)
                                            {
                                                switch (attribute.Name)
                                                {
                                                    //Sets the vertex normals
                                                    case "Normal":
                                                        string[] NormalPoints = attribute.Value.Split(';');
														if (!double.TryParse(NormalPoints[0], out nx))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument nX in " + attribute.Name + " in Loksim3D object file " + FileName);
														}
														if (!double.TryParse(NormalPoints[1], out ny))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument nY in " + attribute.Name + " in Loksim3D object file " + FileName);
														}
														if (!double.TryParse(NormalPoints[2], out nz))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument nZ in " + attribute.Name + " in Loksim3D object file " + FileName);
														}
                                                        break;
                                                    //Sets the vertex 3D co-ordinates
                                                    case "Vekt":
                                                        string[] VertexPoints = attribute.Value.Split(';');
														if (!double.TryParse(VertexPoints[0], out vx))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument vX in " + attribute.Name + " in Loksim3D object file " + FileName);
														}
														if (!double.TryParse(VertexPoints[1], out vy))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument yY in " + attribute.Name + " in Loksim3D object file " + FileName);
														}
														if (!double.TryParse(VertexPoints[2], out vz))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument vZ in " + attribute.Name + " in Loksim3D object file " + FileName);
														}
                                                        break;
                                                }
                                            }
                                            World.Normalize(ref nx, ref ny, ref nz);
											//Resize temp arrays
                                            Array.Resize<World.Vertex>(ref tempVertices, tempVertices.Length + 1);
                                            Array.Resize<Vector3>(ref tempNormals, tempNormals.Length + 1);
                                            //Add vertex and normals to temp array
                                            tempVertices[tempVertices.Length - 1].Coordinates = new Vector3(vx, vy, vz);
                                            tempNormals[tempNormals.Length - 1] = new Vector3((float)nx, (float)ny, (float)nz);
                                            Array.Resize<World.Vertex>(ref Builder.Vertices, Builder.Vertices.Length + 1);
                                            while (Builder.Vertices.Length >= Normals.Length)
                                            {
                                                Array.Resize<Vector3>(ref Normals, Normals.Length << 1);
                                            }
                                            Builder.Vertices[Builder.Vertices.Length - 1].Coordinates = new Vector3(vx, vy, vz);
                                            Normals[Builder.Vertices.Length - 1] = new Vector3((float) nx, (float) ny, (float) nz);
                                        }
                                    }
                                }
                                //The Flaeche command creates a face
                                else if (node.Name == "Flaeche" && node.ChildNodes.OfType<XmlElement>().Any())
                                {
									foreach (XmlNode childNode in node.ChildNodes)
                                    {
                                        if (childNode.Name == "Props" && childNode.Attributes != null)
                                        {
                                            //Defines the verticies in this face
                                            //**NOTE**: A vertex may appear in multiple faces with different texture co-ordinates
                                            if (childNode.Attributes["Points"] != null)
                                            {
                                                string[] Verticies = childNode.Attributes["Points"].Value.Split(';');
                                                int f = Builder.Faces.Length;
                                                //Add 1 to the length of the face array
                                                Array.Resize<World.MeshFace>(ref Builder.Faces, f + 1);
                                                Builder.Faces[f] = new World.MeshFace();
                                                //Create the vertex array for the face
                                                Builder.Faces[f].Vertices = new World.MeshFaceVertex[Verticies.Length];
                                                while (Builder.Vertices.Length > Normals.Length)
                                                {
                                                    Array.Resize<Vector3>(ref Normals,
                                                        Normals.Length << 1);
                                                }
                                                //Run through the vertices list and grab from the temp array

	                                            int smallestX = TextureWidth;
	                                            int smallestY = TextureHeight;
                                                for (int j = 0; j < Verticies.Length; j++)
                                                {
                                                    //This is the position of the vertex in the temp array
                                                    int currentVertex;
													if (!int.TryParse(Verticies[j], out currentVertex))
													{
														Interface.AddMessage(Interface.MessageType.Error, false, Verticies[j] + " does not parse to a valid Vertex in " + node.Name + " in Loksim3D object file " + FileName);
														continue;
													}
                                                    //Add one to the actual vertex array
                                                    Array.Resize<World.Vertex>(ref Builder.Vertices, Builder.Vertices.Length + 1);
                                                    //Set coordinates
                                                    Builder.Vertices[Builder.Vertices.Length - 1].Coordinates = tempVertices[currentVertex].Coordinates;
                                                    //Set the vertex index
                                                    Builder.Faces[f].Vertices[j].Index = (ushort)(Builder.Vertices.Length - 1);
                                                    //Set the normals
                                                    Builder.Faces[f].Vertices[j].Normal = tempNormals[currentVertex];
                                                    //Now deal with the texture
                                                    //Texture mapping points are in pixels X,Y and are relative to the face in question rather than the vertex
                                                    if (childNode.Attributes["Texture"] != null)
                                                    {
                                                        string[] TextureCoords = childNode.Attributes["Texture"].Value.Split(';');
                                                        Vector2 currentCoords;
                                                        float OpenBVEWidth;
                                                        float OpenBVEHeight;
                                                        string[] splitCoords = TextureCoords[j].Split(',');
														if (!float.TryParse(splitCoords[0], out OpenBVEWidth))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid texture width specified in " + node.Name + " in Loksim3D object file " + FileName);
															continue;
														}
														if (!float.TryParse(splitCoords[1], out OpenBVEHeight))
														{
															Interface.AddMessage(Interface.MessageType.Error, false, "Invalid texture height specified in " + node.Name + " in Loksim3D object file " + FileName);
															continue;
														}
														if (OpenBVEWidth <= smallestX && OpenBVEHeight <= smallestY)
														{
															//Clamp texture width and height
															smallestX = (int)OpenBVEWidth;
															smallestY = (int)OpenBVEHeight;
														}
														if (TextureWidth != 0 && TextureHeight != 0)
                                                        {
															//Calculate openBVE co-ords
                                                            currentCoords.X = (OpenBVEWidth / TextureWidth);
                                                            currentCoords.Y = (OpenBVEHeight / TextureHeight);

                                                        }
                                                        else
                                                        {
															//Invalid, so just return zero
                                                            currentCoords.X = 0;
                                                            currentCoords.Y = 0;
                                                        }
                                                        Builder.Vertices[Builder.Vertices.Length - 1].TextureCoordinates = currentCoords;
	                                                    
	                                                    
                                                    }
                                                }
	                                            if (Face2)
	                                            {
													//Add face2 flag if required
		                                            Builder.Faces[f].Flags = (byte)World.MeshFace.Face2Mask;
	                                            }
											}

                                        }
                                    }
                                }

                            }
                        }
                    }
                }

				//Apply rotation
				/*
                 * NOTES:
                 * No rotation order is specified
                 * The rotation string in a .l3dgrp file is ordered Y, Z, X    ??? Can't find a good reason for this ???
                 * Rotations must still be performed in X,Y,Z order to produce correct results
                 */

				if (Rotation.Z != 0.0)
				{
					//Convert to radians
					Rotation.Z *= 0.0174532925199433;
					//Apply rotation
					ApplyRotation(Builder, 1, 0, 0, Rotation.Z);
				}


				if (Rotation.X != 0.0)
                {
                    //This is actually the Y-Axis rotation
                    //Convert to radians
                    Rotation.X *= 0.0174532925199433;
                    //Apply rotation
                    ApplyRotation(Builder, 0, 1, 0, Rotation.X);
                }
                if (Rotation.Y != 0.0)
                {
                    //This is actually the X-Axis rotation
                    //Convert to radians
                    Rotation.Y *= 0.0174532925199433;
                    //Apply rotation
                    ApplyRotation(Builder, 0, 0, 1, Rotation.Y);
                }
                
                
                //These files appear to only have one texture defined
                //Therefore import later- May have to change
                if (File.Exists(tday))
                {
                    for (int j = 0; j < Builder.Materials.Length; j++)
                    {
                        Builder.Materials[j].DaytimeTexture = tday;
	                    Builder.Materials[j].TransparencyTexture = transtex;
                        Builder.Materials[j].NighttimeTexture = tnight;
                    }
                }
                if (TransparencyUsed == true)
                {
                    for (int j = 0; j < Builder.Materials.Length; j++)
                    {
	                    Builder.Materials[j].TransparentColor = FirstPxTransparent ? FirstPxColor : transparentColor;
	                    Builder.Materials[j].TransparentColorUsed = true;
                    }
                }
                 
            }
            ApplyMeshBuilder(ref Object, Builder, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
            World.CreateNormals(ref Object.Mesh);
            return Object;
        }
        private static void ApplyMeshBuilder(ref ObjectManager.StaticObject Object, MeshBuilder Builder, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY)
		{
			if (Builder.Faces.Length != 0)
			{
				int mf = Object.Mesh.Faces.Length;
				int mm = Object.Mesh.Materials.Length;
				int mv = Object.Mesh.Vertices.Length;
				Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + Builder.Faces.Length);
				Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Builder.Materials.Length);
				Array.Resize<World.Vertex>(ref Object.Mesh.Vertices, mv + Builder.Vertices.Length);
				for (int i = 0; i < Builder.Vertices.Length; i++)
				{
					Object.Mesh.Vertices[mv + i] = Builder.Vertices[i];
				}
				for (int i = 0; i < Builder.Faces.Length; i++)
				{
					Object.Mesh.Faces[mf + i] = Builder.Faces[i];
					for (int j = 0; j < Object.Mesh.Faces[mf + i].Vertices.Length; j++)
					{
						Object.Mesh.Faces[mf + i].Vertices[j].Index += (ushort)mv;
					}
					Object.Mesh.Faces[mf + i].Material += (ushort)mm;
				}
				for (int i = 0; i < Builder.Materials.Length; i++)
				{
					Object.Mesh.Materials[mm + i].Flags = (byte)((Builder.Materials[i].EmissiveColorUsed ? World.MeshMaterial.EmissiveColorMask : 0) | (Builder.Materials[i].TransparentColorUsed ? World.MeshMaterial.TransparentColorMask : 0));
					Object.Mesh.Materials[mm + i].Color = Builder.Materials[i].Color;
					Object.Mesh.Materials[mm + i].TransparentColor = Builder.Materials[i].TransparentColor;
					Textures.OpenGlTextureWrapMode WrapX, WrapY;
					if (ForceTextureRepeatX)
					{
						WrapX = Textures.OpenGlTextureWrapMode.RepeatRepeat;
					}
					else
					{
						WrapX = Textures.OpenGlTextureWrapMode.ClampClamp;
					}
					if (ForceTextureRepeatY)
					{
						WrapY = Textures.OpenGlTextureWrapMode.RepeatRepeat;
					}
					else
					{
						WrapY = Textures.OpenGlTextureWrapMode.ClampClamp;
					}
					if (WrapX != Textures.OpenGlTextureWrapMode.RepeatRepeat | WrapY != Textures.OpenGlTextureWrapMode.RepeatRepeat)
					{
						for (int j = 0; j < Builder.Vertices.Length; j++)
						{
							if (Builder.Vertices[j].TextureCoordinates.X < 0.0 | Builder.Vertices[j].TextureCoordinates.X > 1.0)
							{
								WrapX = Textures.OpenGlTextureWrapMode.RepeatRepeat;
							}
							if (Builder.Vertices[j].TextureCoordinates.Y < 0.0 | Builder.Vertices[j].TextureCoordinates.Y > 1.0)
							{
								WrapY = Textures.OpenGlTextureWrapMode.RepeatRepeat;
							}
						}
					}
					if (Builder.Materials[i].DaytimeTexture != null)
					{
						Textures.Texture tday;
						if (Builder.Materials[i].TransparencyTexture != null)
						{
							Bitmap Main = new Bitmap(Builder.Materials[i].DaytimeTexture);
							Main = ResizeImage(Main, Main.Size.Width, Main.Size.Height);
							Bitmap Alpha = new Bitmap(Builder.Materials[i].TransparencyTexture);
							if (Alpha.Size != Main.Size)
							{
								Alpha = ResizeImage(Alpha, Main.Size.Width, Main.Size.Height);
							}
							Bitmap texture = MergeAlphaBitmap(Main, Alpha);
							//Dispose of both main and alpha
							Main.Dispose();
							Alpha.Dispose();
							tday = Textures.RegisterTexture(texture);
						}
						else
						{
							Textures.RegisterTexture(Builder.Materials[i].DaytimeTexture, new TextureParameters(null, Builder.Materials[i].TransparentColor), out tday);
						}
						Object.Mesh.Materials[mm + i].DaytimeTexture = tday;
					}
					else
					{
						Object.Mesh.Materials[mm + i].DaytimeTexture = null;
					}
					Object.Mesh.Materials[mm + i].EmissiveColor = Builder.Materials[i].EmissiveColor;
					if (Builder.Materials[i].NighttimeTexture != null)
					{
						Textures.Texture tnight;
						Textures.RegisterTexture(Builder.Materials[i].NighttimeTexture, new TextureParameters(null, Builder.Materials[i].TransparentColor), out tnight);
						Object.Mesh.Materials[mm + i].DaytimeTexture = tnight;
					}
					else
					{
						Object.Mesh.Materials[mm + i].NighttimeTexture = null;
					}
					Object.Mesh.Materials[mm + i].DaytimeNighttimeBlend = 0;
					Object.Mesh.Materials[mm + i].BlendMode = Builder.Materials[i].BlendMode;
					Object.Mesh.Materials[mm + i].GlowAttenuationData = Builder.Materials[i].GlowAttenuationData;
				}
			}
		}

	    private static Bitmap MergeAlphaBitmap(Bitmap Main, Bitmap Alpha)
	    {
		    Bitmap Output = new Bitmap(Main.Width, Main.Height, PixelFormat.Format32bppArgb);

		    Rectangle rect = new Rectangle(0, 0, Main.Width, Main.Height);

		    BitmapData bmp1Data = Main.LockBits(rect, ImageLockMode.ReadOnly, Main.PixelFormat);
		    BitmapData bmp2Data = Alpha.LockBits(rect, ImageLockMode.ReadOnly, Alpha.PixelFormat);
		    BitmapData bmp3Data = Output.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

		    int size1 = bmp1Data.Stride * bmp1Data.Height;
		    int size2 = bmp2Data.Stride * bmp2Data.Height;
		    int size3 = bmp3Data.Stride * bmp3Data.Height;
		    byte[] data1 = new byte[size1];
		    byte[] data2 = new byte[size2];
		    byte[] data3 = new byte[size3];
		    Marshal.Copy(bmp1Data.Scan0, data1, 0, size1);
		    Marshal.Copy(bmp2Data.Scan0, data2, 0, size2);
		    Marshal.Copy(bmp3Data.Scan0, data3, 0, size3);

			for (int y = 0; y < Main.Height; y++)
			{
				for (int x = 0; x < Main.Width; x++)
				{
					int index1 = y * bmp1Data.Stride + x * 3;
					int index2;
					int index3 = y * bmp3Data.Stride + x * 4;
					//Copy alpha pixel data
					switch (Alpha.PixelFormat)
					{
						case PixelFormat.Format1bppIndexed:
							//Find the index in the bitmap's data
							var idx = y* bmp2Data.Stride + (x >> 3);
							var mask = (byte)(0x80 >> (x & 0x7));
							//Use mask to find whether this pixel is trans (8 pixels per byte)
							data3[index3 + 3] = (data2[idx] & mask) == 0 ? (byte)0 : (byte)255;
							break;
						case PixelFormat.Format8bppIndexed:
							//Likely 256 color grayscale so just copy the raw bytes
							index2 = y * bmp2Data.Stride + x;
							data3[index3 + 3] = data2[index2];
							break;
						case PixelFormat.Format24bppRgb:
							//This is a full-color RGB bitmap, so cast our color to grayscale first
							index2 = y * bmp2Data.Stride + x * 3;
							var c2 = Color.FromArgb(255, data2[index2 + 2], data2[index2 + 1], data2[index2 + 0]);
							data3[index3 + 3] = (byte)(255 * c2.GetBrightness());
							break;
						default:
							//Not supported, so set our bitmap to fully opaque
							data3[index3 + 3] = 255;
							break;
					}
					var c1 = Color.FromArgb(255, data1[index1 + 2], data1[index1 + 1], data1[index1 + 0]);
					data3[index3 + 0] = c1.B;
					data3[index3 + 1] = c1.G;
					data3[index3 + 2] = c1.R;
				}
			}

			Marshal.Copy(data3, 0, bmp3Data.Scan0, data3.Length);
		    Main.UnlockBits(bmp1Data);
		    Alpha.UnlockBits(bmp2Data);
		    Output.UnlockBits(bmp3Data);
		    return Output;
		}


	    private static Bitmap ResizeImage(Image image, int width, int height)
	    {
		    var destRect = new Rectangle(0, 0, width, height);
		    var destImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);

		    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

		    using (var graphics = Graphics.FromImage(destImage))
		    {
			    graphics.CompositingMode = CompositingMode.SourceCopy;
			    graphics.CompositingQuality = CompositingQuality.HighQuality;
			    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			    graphics.SmoothingMode = SmoothingMode.HighQuality;
			    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			    using (var wrapMode = new ImageAttributes())
			    {
				    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
				    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
			    }
		    }
		    return destImage;
	    }

		private static void ApplyRotation(MeshBuilder Builder, double x, double y, double z, double a)
        {
            double cosa = Math.Cos(a);
            double sina = Math.Sin(a);
            for (int i = 0; i < Builder.Vertices.Length; i++)
            {
                World.Rotate(ref Builder.Vertices[i].Coordinates.X, ref Builder.Vertices[i].Coordinates.Y, ref Builder.Vertices[i].Coordinates.Z, x, y, z, cosa, sina);
            }
            for (int i = 0; i < Builder.Faces.Length; i++)
            {
                for (int j = 0; j < Builder.Faces[i].Vertices.Length; j++)
                {
                    World.Rotate(ref Builder.Faces[i].Vertices[j].Normal.X, ref Builder.Faces[i].Vertices[j].Normal.Y, ref Builder.Faces[i].Vertices[j].Normal.Z, x, y, z, cosa, sina);
                }
            }
        }
    }
}
