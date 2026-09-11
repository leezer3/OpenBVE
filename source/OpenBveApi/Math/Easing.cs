namespace OpenBveApi.Math
{
	/// <summary>Provides standard easing and interpolation functions</summary>
	/// <remarks>Easing curves follow the reference implementations by Robert Penner and operate on a normalized input in the range 0..1</remarks>
	public static class Easing
	{
		/// <summary>Linearly interpolates between two values</summary>
		/// <param name="a">The first value</param>
		/// <param name="b">The second value</param>
		/// <param name="t">The interpolation factor</param>
		public static double Lerp(double a, double b, double t)
		{
			return a + (b - a) * t;
		}

		/// <summary>Performs a Hermite (smoothstep) interpolation between two values</summary>
		/// <param name="a">The first value</param>
		/// <param name="b">The second value</param>
		/// <param name="x">The interpolation input</param>
		public static double Smoothstep(double a, double b, double x)
		{
			if (a == b)
			{
				return a;
			}
			double t = (x - a) / (b - a);
			return t * t * (3.0 - 2.0 * t);
		}

		/// <summary>Spherically (shortest-arc) interpolates between two angles</summary>
		/// <param name="a">The first angle</param>
		/// <param name="b">The second angle</param>
		/// <param name="t">The interpolation factor</param>
		public static double Slerp(double a, double b, double t)
		{
			double delta = b - a;
			delta = (delta + System.Math.PI) % (2.0 * System.Math.PI);
			if (delta < 0.0) delta += 2.0 * System.Math.PI;
			delta -= System.Math.PI;
			return a + delta * t;
		}

		/// <summary>Eases the input using a sine curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInSine(double t)
		{
			return 1.0 - System.Math.Cos(t * System.Math.PI / 2.0);
		}

		/// <summary>Eases the input using a sine curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutSine(double t)
		{
			return System.Math.Sin(t * System.Math.PI / 2.0);
		}

		/// <summary>Eases the input using a sine curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutSine(double t)
		{
			return -(System.Math.Cos(System.Math.PI * t) - 1.0) / 2.0;
		}

		/// <summary>Eases the input using a quadratic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInQuad(double t)
		{
			return t * t;
		}

		/// <summary>Eases the input using a quadratic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutQuad(double t)
		{
			double u = 1.0 - t;
			return 1.0 - u * u;
		}

		/// <summary>Eases the input using a quadratic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutQuad(double t)
		{
			if (t < 0.5)
			{
				return 2.0 * t * t;
			}
			double v = -2.0 * t + 2.0;
			return 1.0 - v * v / 2.0;
		}

		/// <summary>Eases the input using a cubic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInCubic(double t)
		{
			return t * t * t;
		}

		/// <summary>Eases the input using a cubic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutCubic(double t)
		{
			double u = 1.0 - t;
			return 1.0 - u * u * u;
		}

		/// <summary>Eases the input using a cubic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutCubic(double t)
		{
			if (t < 0.5)
			{
				return 4.0 * t * t * t;
			}
			double v = -2.0 * t + 2.0;
			return 1.0 - v * v * v / 2.0;
		}

		/// <summary>Eases the input using a quartic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInQuart(double t)
		{
			double s = t * t;
			return s * s;
		}

		/// <summary>Eases the input using a quartic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutQuart(double t)
		{
			double u = 1.0 - t;
			double s = u * u;
			return 1.0 - s * s;
		}

		/// <summary>Eases the input using a quartic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutQuart(double t)
		{
			if (t < 0.5)
			{
				double s = t * t;
				return 8.0 * s * s;
			}
			double v = -2.0 * t + 2.0;
			double s2 = v * v;
			return 1.0 - s2 * s2 / 2.0;
		}

		/// <summary>Eases the input using a quintic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInQuint(double t)
		{
			double s = t * t;
			return s * s * t;
		}

		/// <summary>Eases the input using a quintic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutQuint(double t)
		{
			double u = 1.0 - t;
			double s = u * u;
			return 1.0 - s * s * u;
		}

		/// <summary>Eases the input using a quintic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutQuint(double t)
		{
			if (t < 0.5)
			{
				double s = t * t;
				return 16.0 * s * s * t;
			}
			double v = -2.0 * t + 2.0;
			double s2 = v * v;
			return 1.0 - s2 * s2 * v / 2.0;
		}

		/// <summary>Eases the input using an exponential curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInExpo(double t)
		{
			return t == 0.0 ? 0.0 : System.Math.Pow(2.0, 10.0 * t - 10.0);
		}

		/// <summary>Eases the input using an exponential curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutExpo(double t)
		{
			return t == 1.0 ? 1.0 : 1.0 - System.Math.Pow(2.0, -10.0 * t);
		}

		/// <summary>Eases the input using an exponential curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutExpo(double t)
		{
			if (t == 0.0)
			{
				return 0.0;
			}
			if (t == 1.0)
			{
				return 1.0;
			}
			if (t < 0.5)
			{
				return System.Math.Pow(2.0, 20.0 * t - 10.0) / 2.0;
			}
			return (2.0 - System.Math.Pow(2.0, -20.0 * t + 10.0)) / 2.0;
		}

		/// <summary>Eases the input using a circular curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInCirc(double t)
		{
			return 1.0 - Extensions.SqrtC(1.0 - t * t);
		}

		/// <summary>Eases the input using a circular curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutCirc(double t)
		{
			double u = t - 1.0;
			return Extensions.SqrtC(1.0 - u * u);
		}

		/// <summary>Eases the input using a circular curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutCirc(double t)
		{
			if (t < 0.5)
			{
				return (1.0 - Extensions.SqrtC(1.0 - 4.0 * t * t)) / 2.0;
			}
			double v = -2.0 * t + 2.0;
			return (Extensions.SqrtC(1.0 - v * v) + 1.0) / 2.0;
		}

		/// <summary>Eases the input using a back curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInBack(double t)
		{
			const double c1 = 1.70158;
			const double c3 = c1 + 1.0;
			return c3 * t * t * t - c1 * t * t;
		}

		/// <summary>Eases the input using a back curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutBack(double t)
		{
			const double c1 = 1.70158;
			const double c3 = c1 + 1.0;
			double u = t - 1.0;
			return 1.0 + c3 * u * u * u + c1 * u * u;
		}

		/// <summary>Eases the input using a back curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutBack(double t)
		{
			const double c1 = 1.70158;
			const double c2 = c1 * 1.525;
			if (t < 0.5)
			{
				double v = 2.0 * t;
				return (v * v * ((c2 + 1.0) * v - c2)) / 2.0;
			}
			double w = 2.0 * t - 2.0;
			return (w * w * ((c2 + 1.0) * w + c2) + 2.0) / 2.0;
		}

		/// <summary>Eases the input using an elastic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInElastic(double t)
		{
			const double c4 = (2.0 * System.Math.PI) / 3.0;
			if (t == 0.0)
			{
				return 0.0;
			}
			if (t == 1.0)
			{
				return 1.0;
			}
			return -System.Math.Pow(2.0, 10.0 * t - 10.0) * System.Math.Sin((t * 10.0 - 10.75) * c4);
		}

		/// <summary>Eases the input using an elastic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutElastic(double t)
		{
			const double c4 = (2.0 * System.Math.PI) / 3.0;
			if (t == 0.0)
			{
				return 0.0;
			}
			if (t == 1.0)
			{
				return 1.0;
			}
			return System.Math.Pow(2.0, -10.0 * t) * System.Math.Sin((t * 10.0 - 0.75) * c4) + 1.0;
		}

		/// <summary>Eases the input using an elastic curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutElastic(double t)
		{
			const double c5 = (2.0 * System.Math.PI) / 4.5;
			if (t == 0.0)
			{
				return 0.0;
			}
			if (t == 1.0)
			{
				return 1.0;
			}
			if (t < 0.5)
			{
				return -(System.Math.Pow(2.0, 20.0 * t - 10.0) * System.Math.Sin((20.0 * t - 11.125) * c5)) / 2.0;
			}
			return (System.Math.Pow(2.0, -20.0 * t + 10.0) * System.Math.Sin((20.0 * t - 11.125) * c5)) / 2.0 + 1.0;
		}

		/// <summary>Eases the input using a bounce curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInBounce(double t)
		{
			return 1.0 - EaseOutBounce(1.0 - t);
		}

		/// <summary>Eases the input using a bounce curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseOutBounce(double t)
		{
			const double n1 = 7.5625;
			const double d1 = 2.75;
			if (t < 1.0 / d1)
			{
				return n1 * t * t;
			}
			if (t < 2.0 / d1)
			{
				double u = t - 1.5 / d1;
				return n1 * u * u + 0.75;
			}
			if (t < 2.5 / d1)
			{
				double u = t - 2.25 / d1;
				return n1 * u * u + 0.9375;
			}
			double v = t - 2.625 / d1;
			return n1 * v * v + 0.984375;
		}

		/// <summary>Eases the input using a bounce curve</summary>
		/// <param name="t">The input, in the range 0..1</param>
		public static double EaseInOutBounce(double t)
		{
			if (t < 0.5)
			{
				return (1.0 - EaseOutBounce(1.0 - 2.0 * t)) / 2.0;
			}
			return (1.0 + EaseOutBounce(2.0 * t - 1.0)) / 2.0;
		}
	}
}
