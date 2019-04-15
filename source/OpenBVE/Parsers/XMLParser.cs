using System;
using System.Text;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
    internal static class XMLParser
    {
        //The XML Parser Class will allow loading of an object with more advanced
        //properties than are currently available with the CSV and B3D formats, whilst
        //not requiring backwards incompatible changes

        public static UnifiedObject ReadObject(string fileName, Encoding encoding)
        {
            //The current XML file to load
            XmlDocument currentXML = new XmlDocument();
            ObjectManager.StaticObject Object = null;
            //Load the object's XML file 
            currentXML.Load(fileName);
            //Check for null
            if (currentXML.DocumentElement != null)
            {
                XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openbve/object");
                //Check this file actually contains OpenBVE object nodes
                if (DocumentNodes != null)
                    foreach (XmlNode node in DocumentNodes)
                    {
                        string objectPath;
                        try
                        {
                            var fn = System.IO.Path.GetDirectoryName(fileName);
							var InnerNode = node.SelectSingleNode("filename").InnerText;
                            InnerNode = InnerNode.Trim();
                            objectPath = OpenBveApi.Path.CombineFile(fn, InnerNode);
                        }
                        catch (Exception)
                        {
                            Interface.AddMessage(MessageType.Error, false,
                                "The XML does not contain a valid object path: " + fileName);
                            return null;
                        }
                        if (objectPath != null && System.IO.File.Exists(objectPath))
                        {
                            switch (System.IO.Path.GetExtension(objectPath).ToLowerInvariant())
                            {
                                case ".csv":
                                case ".b3d":
                                    Object = CsvB3dObjectParser.ReadObject(objectPath, encoding);
                                    break;
                                case ".x":
                                    Object = XObjectParser.ReadObject(objectPath, encoding);
                                    break;
                                case ".animated":
                                    //Not currently working.
                                    //Object = AnimatedObjectParser.ReadObject(objectPath, encoding, LoadMode);
                                    break;                                  
                            }
                            try
                            {
                                var BoundingBoxUpper = node.SelectSingleNode("boundingboxupper").InnerText;
                                var BoundingBoxLower = node.SelectSingleNode("boundingboxlower").InnerText;
                                Object.Mesh.BoundingBox = new Vector3[2];
                                var splitStrings = BoundingBoxUpper.Split(',');
                                if (splitStrings.Length != 3)
                                {
                                    //Throw exception, as this isn't a valid 3D point
                                    throw new Exception();
                                }
                                Object.Mesh.BoundingBox[0].X = Double.Parse(splitStrings[0]);
                                Object.Mesh.BoundingBox[0].Y = Double.Parse(splitStrings[1]);
                                Object.Mesh.BoundingBox[0].Z = Double.Parse(splitStrings[2]);
                                splitStrings = BoundingBoxLower.Split(',');
                                if (splitStrings.Length != 3)
                                {
                                    //Throw exception, as this isn't a valid 3D point
                                    throw new Exception();
                                }
                                Object.Mesh.BoundingBox[1].X = Double.Parse(splitStrings[0]);
                                Object.Mesh.BoundingBox[1].Y = Double.Parse(splitStrings[1]);
                                Object.Mesh.BoundingBox[1].Y = Double.Parse(splitStrings[2]);
                            }
                            catch (Exception)
                            {
                                Interface.AddMessage(MessageType.Error, false,
                                "The XML contained an invalid bounding box entry: " + fileName);
                            }
                            var selectSingleNode = node.SelectSingleNode("author");
                            if (selectSingleNode != null)
                            {
                                //Attempt to load author information from XML
								Object.Author = selectSingleNode.InnerText.Trim();
                            }
                            selectSingleNode = node.SelectSingleNode("copyright");
                            if (selectSingleNode != null)
                            {
                                //Attempt to load copyright information from XML
                                Object.Copyright = selectSingleNode.InnerText.Trim();
                            }
                            return Object;
                        }
                        Interface.AddMessage(MessageType.Error, false,
                                        "The file extension is not supported: " + objectPath);
                        return null;

                    }
            }
            //We couldn't find any valid XML, so return a null object
            return null;
        }
    }
}
