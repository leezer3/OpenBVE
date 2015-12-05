using System;
using System.Xml;

namespace OpenBve
{
    internal static class Ls3DGrpParser
    {
        internal static ObjectManager.AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding,ObjectManager.ObjectLoadMode LoadMode)
        {
            XmlDocument currentXML = new XmlDocument();
            //May need to be changed to use de-DE
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection();
            Result.Objects = new ObjectManager.AnimatedObject[0];
            int ObjectCount = 0;
            currentXML.Load(FileName);
            string BaseDir = System.IO.Path.GetDirectoryName(FileName);
            //Check for null
            if (currentXML.DocumentElement != null)
            {
                ObjectManager.UnifiedObject[] obj = new OpenBve.ObjectManager.UnifiedObject[0];
                XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/GRUPPENOBJECT");
                if (DocumentNodes != null)
                {
                    foreach (XmlNode outerNode in DocumentNodes)
                    {
                        if (outerNode.HasChildNodes)
                        {
                            foreach (XmlNode node in outerNode.ChildNodes)
                            {
                                if (node.Name == "Object" && node.HasChildNodes)
                                {
                                    foreach (XmlNode childNode in node.ChildNodes)
                                    {
                                        if (childNode.Name == "Props" && childNode.Attributes != null)
                                        {
                                            foreach (XmlAttribute attribute in childNode.Attributes)
                                            {
                                                World.Vector3D position = new World.Vector3D(0,0,0);
                                                switch (attribute.Name)
                                                {
                                                    case "Name":
                                                        string ObjectFile = OpenBveApi.Path.CombineFile(BaseDir,attribute.Value);
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref obj, obj.Length << 1);
                                                        obj[obj.Length -1] = ObjectManager.LoadObject(ObjectFile, Encoding, LoadMode, false, false, false);
                                                        ObjectCount++;
                                                        break;
                                                    case "Position":
                                                        string[] SplitPosition = attribute.Value.Split(';');
                                                        double.TryParse(SplitPosition[0], out position.X);
                                                        double.TryParse(SplitPosition[1], out position.Y);
                                                        double.TryParse(SplitPosition[2], out position.Z);
                                                        break;
                                                    case "Rotation":
                                                        //This will require passing a paramater to the LS3D object reader I think
                                                        //Dynamic rotation doesn't seem to be possible
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                }
            }


            Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, ObjectCount);
            return Result;
        }
    }
}