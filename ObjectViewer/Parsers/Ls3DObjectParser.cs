using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using OpenBveApi.Math;

namespace OpenBve
{
    /// <summary>Parses a Loksim3D xml format object</summary>
    internal static class Ls3DObjectParser
    {
        // structures
        private class Material
        {
            internal World.ColorRGBA Color;
            internal World.ColorRGB EmissiveColor;
            internal bool EmissiveColorUsed;
            internal World.ColorRGB TransparentColor;
            internal bool TransparentColorUsed;
            internal string DaytimeTexture;
            internal string NighttimeTexture;
            internal World.MeshMaterialBlendMode BlendMode;
            internal ushort GlowAttenuationData;
            internal Material()
            {
                this.Color = new World.ColorRGBA(255, 255, 255, 255);
                this.EmissiveColor = new World.ColorRGB(0, 0, 0);
                this.EmissiveColorUsed = false;
                this.TransparentColor = new World.ColorRGB(0, 0, 0);
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
        // read object
        /// <summary>Loads a Loksim3D object from a file.</summary>
        /// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
        /// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
        /// <param name="LoadMode">The texture load mode.</param>
        /// <param name="ForceTextureRepeatX">Whether to force TextureWrapMode.Repeat for the X-axis</param>
        /// /// <param name="ForceTextureRepeatY">Whether to force TextureWrapMode.Repeat for the Y-axis</param>
        /// <returns>The object loaded.</returns>
        internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding,ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY)
        {
            XmlDocument currentXML = new XmlDocument();
            //May need to be changed to use de-DE
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            //Initialise the object
            ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Faces = new World.MeshFace[] { };
			Object.Mesh.Materials = new World.MeshMaterial[] { };
			Object.Mesh.Vertices = new World.Vertex[] { };
            MeshBuilder Builder = new MeshBuilder();
			World.Vector3Df[] Normals = new World.Vector3Df[4];
            bool PropertiesFound = false;

            World.Vertex[] tempVertices = new World.Vertex[0];
            World.Vector3Df[] tempNormals = new World.Vector3Df[0];
            World.ColorRGB transparentColor = new World.ColorRGB();
            string tday = null;
            string tnight = null;
            bool TransparencyUsed = false;
            bool Face2 = false;
            int TextureWidth = 0;
            int TextureHeight = 0;
            
            currentXML.Load(FileName);
            //Check for null
            if (currentXML.DocumentElement != null)
            {
                XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/OBJECT");
                //Check this file actually contains Loksim3D object nodes
                if (DocumentNodes != null)
                {
                    foreach (XmlNode outerNode in DocumentNodes)
                    {
                        if (outerNode.HasChildNodes)
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
                                                case "Texture":
                                                    tday = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), attribute.Value);
                                                    if (File.Exists(tday))
                                                    {
                                                        try
                                                        {
                                                            using (Bitmap TextureInformation = new Bitmap(tday))
                                                            {
                                                                TextureWidth = TextureInformation.Width;
                                                                TextureHeight = TextureInformation.Height;
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            tday = null;
                                                        }
                                                    }
                                                    break;
                                                case "Transparent":
                                                    //If transparent is set to true, then the texture appears to have a transparent part
                                                    //However, we also appear to be able to set a transparent color and not the Transparent attribute
                                                    if (attribute.Value == "TRUE")
                                                    {
                                                        TransparencyUsed = true;
                                                    }
                                                    else
                                                    {
                                                        TransparencyUsed = false;
                                                    }
                                                    break;
                                                case "TransparentTyp":
                                                    //Sets the transparency type
                                                    //This must be color key transparency, as BMP file can't store an alpha channel
                                                    //A value of 1 would appear to be solid black???
                                                    TransparencyUsed = true;
                                                    switch (attribute.Value)
                                                    {
                                                        case "0":
                                                            break;
                                                        case "1":
                                                            //Would appear to be 0,0,0
                                                            transparentColor = new World.ColorRGB((byte)0, (byte)0, (byte)0);
                                                            break;
                                                        case "2":
                                                            //Would appear to be 0,0,255
                                                            transparentColor = new World.ColorRGB((byte)0, (byte)0, (byte)255);
                                                            break;
                                                    }
                                                    break;
                                                case "Drawrueckseiten":
                                                    Face2 = true;
                                                    break;

                                                /*
                                         * MISSING PROPERTIES:
                                         * AutoRotate - Rotate with tracks?? LS3D presumably uses a 3D world system.
                                         * Beleuchtet- Translates as illuminated. Presume something to do with lighting?
                                         * Drawrueckseiten- Translates as draw backs. Equivilant of Face2 usage?
                                         * FileAuthor
                                         * FileInfo
                                         * FilePicture
                                         */
                                            }
                                        }
                                    }
                                }
                                //The point command would appear to be equivilant to a vertex
                                else if (node.Name == "Point" && node.HasChildNodes)
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
                                                    case "Normal":
                                                        string[] NormalPoints = attribute.Value.Split(';');
                                                        double.TryParse(NormalPoints[0], out nx);
                                                        double.TryParse(NormalPoints[1], out ny);
                                                        double.TryParse(NormalPoints[2], out nz);
                                                        break;
                                                    case "Vekt":
                                                        string[] VertexPoints = attribute.Value.Split(';');
                                                        double.TryParse(VertexPoints[0], out vx);
                                                        double.TryParse(VertexPoints[1], out vy);
                                                        double.TryParse(VertexPoints[2], out vz);
                                                        break;
                                                }
                                            }
                                            World.Normalize(ref nx, ref ny, ref nz);

                                            {
                                                //Resize temp arrays
                                                Array.Resize<World.Vertex>(ref tempVertices, tempVertices.Length + 1);
                                                Array.Resize<World.Vector3Df>(ref tempNormals, tempNormals.Length + 1);
                                                //Add vertex and normals to temp array
                                                tempVertices[tempVertices.Length - 1].Coordinates = new World.Vector3D(vx, vy, vz);
                                                tempNormals[tempNormals.Length - 1] = new World.Vector3Df((float)nx, (float)ny, (float)nz);
                                            }

                                            Array.Resize<World.Vertex>(ref Builder.Vertices, Builder.Vertices.Length + 1);
                                            while (Builder.Vertices.Length >= Normals.Length)
                                            {
                                                Array.Resize<World.Vector3Df>(ref Normals, Normals.Length << 1);
                                            }
                                            Builder.Vertices[Builder.Vertices.Length - 1].Coordinates = new World.Vector3D(vx, vy, vz);
                                            Normals[Builder.Vertices.Length - 1] = new World.Vector3Df((float) nx, (float) ny, (float) nz);
                                        }
                                    }
                                }
                                //The Flaeche command would appear to be equal to a face
                                else if (node.Name == "Flaeche" && node.HasChildNodes)
                                {
                                    foreach (XmlNode childNode in node.ChildNodes)
                                    {
                                        if (childNode.Name == "Props" && childNode.Attributes != null)
                                        {
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
                                                    Array.Resize<World.Vector3Df>(ref Normals,
                                                        Normals.Length << 1);
                                                }
                                                //Run through the vertices list and grab from the temp array
                                                for (int j = 0; j < Verticies.Length; j++)
                                                {
                                                    //This is the position of the vertex in the temp array
                                                    int currentVertex;
                                                    int.TryParse(Verticies[j], out currentVertex);
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
                                                        World.Vector2Df currentCoords;
                                                        float OpenBVEWidth;
                                                        float OpenBVEHeight;
                                                        string[] splitCoords = TextureCoords[j].Split(',');
                                                        float.TryParse(splitCoords[0], out OpenBVEWidth);
                                                        float.TryParse(splitCoords[1], out OpenBVEHeight);
                                                        if (TextureWidth != 0 && TextureHeight != 0)
                                                        {
                                                            currentCoords.X = (OpenBVEWidth / TextureWidth);
                                                            currentCoords.Y = (OpenBVEHeight / TextureHeight);
                                                        }
                                                        else
                                                        {
                                                            currentCoords.X = 0;
                                                            currentCoords.Y = 0;
                                                        }
                                                        Builder.Vertices[Builder.Vertices.Length - 1].TextureCoordinates = currentCoords;
                                                    }
                                                    if (Face2)
                                                    {
                                                        Builder.Faces[f].Flags = (byte) World.MeshFace.Face2Mask;
                                                    }



                                                }
                                            }

                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                
                
                //These files appear to only have one texture defined
                //Therefore import later- May have to change
                if (File.Exists(tday))
                {
                    for (int j = 0; j < Builder.Materials.Length; j++)
                    {
                        Builder.Materials[j].DaytimeTexture = tday;
                        Builder.Materials[j].NighttimeTexture = tnight;
                    }
                }
                if (TransparencyUsed == true)
                {
                    for (int j = 0; j < Builder.Materials.Length; j++)
                    {
                        Builder.Materials[j].TransparentColor = transparentColor;
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
                    TextureManager.TextureWrapMode WrapX, WrapY;
                    if (ForceTextureRepeatX)
                    {
                        WrapX = TextureManager.TextureWrapMode.Repeat;
                    }
                    else
                    {
                        WrapX = TextureManager.TextureWrapMode.ClampToEdge;
                    }
                    if (ForceTextureRepeatY)
                    {
                        WrapY = TextureManager.TextureWrapMode.Repeat;
                    }
                    else
                    {
                        WrapY = TextureManager.TextureWrapMode.ClampToEdge;
                    }
                    if (WrapX != TextureManager.TextureWrapMode.Repeat | WrapY != TextureManager.TextureWrapMode.Repeat)
                    {
                        for (int j = 0; j < Builder.Vertices.Length; j++)
                        {
                            if (Builder.Vertices[j].TextureCoordinates.X < 0.0 | Builder.Vertices[j].TextureCoordinates.X > 1.0)
                            {
                                WrapX = TextureManager.TextureWrapMode.Repeat;
                            }
                            if (Builder.Vertices[j].TextureCoordinates.Y < 0.0 | Builder.Vertices[j].TextureCoordinates.Y > 1.0)
                            {
                                WrapY = TextureManager.TextureWrapMode.Repeat;
                            }
                        }
                    }
                    if (Builder.Materials[i].DaytimeTexture != null)
                    {
                        int tday = TextureManager.RegisterTexture(Builder.Materials[i].DaytimeTexture, Builder.Materials[i].TransparentColor, Builder.Materials[i].TransparentColorUsed ? (byte)1 : (byte)0, WrapX, WrapY, LoadMode != ObjectManager.ObjectLoadMode.Normal);
                        Object.Mesh.Materials[mm + i].DaytimeTextureIndex = tday;
                    }
                    else
                    {
                        Object.Mesh.Materials[mm + i].DaytimeTextureIndex = -1;
                    }
                    Object.Mesh.Materials[mm + i].EmissiveColor = Builder.Materials[i].EmissiveColor;
                    if (Builder.Materials[i].NighttimeTexture != null)
                    {
                        int tnight = TextureManager.RegisterTexture(Builder.Materials[i].NighttimeTexture, Builder.Materials[i].TransparentColor, Builder.Materials[i].TransparentColorUsed ? (byte)1 : (byte)0, WrapX, WrapY, LoadMode != ObjectManager.ObjectLoadMode.Normal);
                        Object.Mesh.Materials[mm + i].NighttimeTextureIndex = tnight;
                    }
                    else
                    {
                        Object.Mesh.Materials[mm + i].NighttimeTextureIndex = -1;
                    }
                    Object.Mesh.Materials[mm + i].DaytimeNighttimeBlend = 0;
                    Object.Mesh.Materials[mm + i].BlendMode = Builder.Materials[i].BlendMode;
                    Object.Mesh.Materials[mm + i].GlowAttenuationData = Builder.Materials[i].GlowAttenuationData;
                }
            }
        }
    }
}
