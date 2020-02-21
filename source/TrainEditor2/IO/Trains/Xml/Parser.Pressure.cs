using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		private static Pressure ParsePressureNode(string fileName, XElement parent)
		{
			Pressure pressure = new Pressure();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "compressor":
						pressure.Compressor = ParseCompressorNode(fileName, keyNode);
						break;
					case "mainreservoir":
						pressure.MainReservoir = ParseMainReservoirNode(fileName, keyNode);
						break;
					case "auxiliaryreservoir":
						pressure.AuxiliaryReservoir = ParseAuxiliaryReservoirNode(fileName, keyNode);
						break;
					case "equalizingreservoir":
						pressure.EqualizingReservoir = ParseEqualizingReservoirNode(fileName, keyNode);
						break;
					case "brakepipe":
						pressure.BrakePipe = ParseBrakePipeNode(fileName, keyNode);
						break;
					case "straightairpipe":
						pressure.StraightAirPipe = ParseStraightAirPipeNode(fileName, keyNode);
						break;
					case "brakecylinder":
						pressure.BrakeCylinder = ParseBrakeCylinderNode(fileName, keyNode);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return pressure;
		}

		private static Compressor ParseCompressorNode(string fileName, XElement parent)
		{
			Compressor compressor = new Compressor();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "rate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								compressor.Rate = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return compressor;
		}

		private static MainReservoir ParseMainReservoirNode(string fileName, XElement parent)
		{
			MainReservoir mainReservoir = new MainReservoir();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "minimumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								mainReservoir.MinimumPressure = result;
							}
						}
						break;
					case "maximumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								mainReservoir.MaximumPressure = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return mainReservoir;
		}

		private static AuxiliaryReservoir ParseAuxiliaryReservoirNode(string fileName, XElement parent)
		{
			AuxiliaryReservoir auxiliaryReservoir = new AuxiliaryReservoir();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "chargerate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								auxiliaryReservoir.ChargeRate = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return auxiliaryReservoir;
		}

		private static EqualizingReservoir ParseEqualizingReservoirNode(string fileName, XElement parent)
		{
			EqualizingReservoir equalizingReservoir = new EqualizingReservoir();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "chargerate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								equalizingReservoir.ChargeRate = result;
							}
						}
						break;
					case "servicerate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								equalizingReservoir.ServiceRate = result;
							}
						}
						break;
					case "emergencyrate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								equalizingReservoir.EmergencyRate = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return equalizingReservoir;
		}

		private static BrakePipe ParseBrakePipeNode(string fileName, XElement parent)
		{
			BrakePipe brakePipe = new BrakePipe();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "normalpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakePipe.NormalPressure = result;
							}
						}
						break;
					case "chargerate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakePipe.ChargeRate = result;
							}
						}
						break;
					case "servicerate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakePipe.ServiceRate = result;
							}
						}
						break;
					case "emergencyrate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakePipe.EmergencyRate = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return brakePipe;
		}

		private static StraightAirPipe ParseStraightAirPipeNode(string fileName, XElement parent)
		{
			StraightAirPipe straightAirPipe = new StraightAirPipe();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "servicerate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								straightAirPipe.ServiceRate = result;
							}
						}
						break;
					case "emergencyrate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								straightAirPipe.EmergencyRate = result;
							}
						}
						break;
					case "releaserate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								straightAirPipe.ReleaseRate = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return straightAirPipe;
		}

		private static BrakeCylinder ParseBrakeCylinderNode(string fileName, XElement parent)
		{
			BrakeCylinder brakeCylinder = new BrakeCylinder();

			CultureInfo culture = CultureInfo.InvariantCulture;

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "servicemaximumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakeCylinder.ServiceMaximumPressure = result;
							}
						}
						break;
					case "emergencymaximumpressure":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakeCylinder.EmergencyMaximumPressure = result;
							}
						}
						break;
					case "emergencyrate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakeCylinder.EmergencyRate = result;
							}
						}
						break;
					case "releaserate":
						if (value.Any())
						{
							double result;

							if (!NumberFormats.TryParseDoubleVb6(value, out result) || result <= 0.0)
							{
								Interface.AddMessage(MessageType.Error, false, $"Value must be a positive floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
							}
							else
							{
								brakeCylinder.ReleaseRate = result;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return brakeCylinder;
		}
	}
}
