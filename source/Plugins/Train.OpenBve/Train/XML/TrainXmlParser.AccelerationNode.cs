using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using TrainManager.Power;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private AccelerationCurve[] ParseAccelerationNode(XmlNode c, string fileName)
		{
			if (c.ChildNodes.OfType<XmlElement>().Any())
			{
				List<AccelerationCurve> accelerationCurves = new List<AccelerationCurve>();
				foreach (XmlNode cc in c.ChildNodes)
				{
					switch (cc.Name.ToLowerInvariant())
					{
						case "openbve": // don't support legacy BVE2 curves in XML, but at the same time specify that this is deliberately BVE4 / OpenBVE format
							BveAccelerationCurve curve = new BveAccelerationCurve();
							foreach (XmlNode sc in cc.ChildNodes)
							{
								switch (sc.Name.ToLowerInvariant())
								{
									case "stagezeroacceleration":
										if (!NumberFormats.TryParseDoubleVb6(sc.InnerText, out curve.StageZeroAcceleration))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Stage zero acceleration was invalid for curve " + accelerationCurves.Count + " in XML file " + fileName);
										}

										curve.StageZeroAcceleration *= 0.277777777777778;
										break;
									case "stageoneacceleration":
										if (!NumberFormats.TryParseDoubleVb6(sc.InnerText, out curve.StageOneAcceleration))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Stage one acceleration was invalid for curve " + accelerationCurves.Count + " in XML file " + fileName);
										}

										curve.StageOneAcceleration *= 0.277777777777778;
										break;
									case "stageonespeed":
										if (!NumberFormats.TryParseDoubleVb6(sc.InnerText, out curve.StageOneSpeed))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Stage one speed was invalid for curve " + accelerationCurves.Count + " in XML file " + fileName);
										}

										curve.StageOneSpeed *= 0.277777777777778;
										break;
									case "stagetwospeed":
										if (!NumberFormats.TryParseDoubleVb6(sc.InnerText, out curve.StageTwoSpeed))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Stage two speed was invalid for curve " + accelerationCurves.Count + " in XML file " + fileName);
										}

										curve.StageTwoSpeed *= 0.277777777777778;
										break;
									case "stagetwoexponent":
										if (!NumberFormats.TryParseDoubleVb6(sc.InnerText, out curve.StageTwoExponent))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Stage two exponent was invalid for curve " + accelerationCurves.Count + " in XML file " + fileName);
										}
										break;
									case "multiplier":
										if (!NumberFormats.TryParseDoubleVb6(sc.InnerText, out double multiplier) || multiplier <= 0 || multiplier > 50)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Multiplier was invalid for curve " + accelerationCurves.Count + " in XML file " + fileName);
											multiplier = Plugin.AccelerationCurves[0].Multiplier;
										}
										curve.Multiplier = multiplier;
										break;
								}
							}
							accelerationCurves.Add(curve);
							break;
					}
				}
				return accelerationCurves.ToArray();
			}
			Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "An empty list of acceleration curves was provided in XML file " + fileName);
			return new AccelerationCurve[] { };
		}
	}
}
