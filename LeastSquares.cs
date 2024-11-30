// https://stackoverflow.com/questions/12946341/algorithm-for-scatter-plot-best-fit-line
// alternatively:  https://christoph.ruegg.name/blog/linear-regression-mathnet-numerics
using System;
using System.Collections.Generic;
using System.Linq;

namespace blekenbleu.OxyScope
{
	public class XYvalue
	{
		public double X;
		public double Y;
	}

	public partial class Control
	{
		// cubic functions for OxyPlot FunctionSeries()
		readonly Func<double, double> cubicfit = (x) => c[0] + x * (c[1] + x * (c[2] +  x * c[3]));
		readonly Func<double, double> ccubicfit = (x) => Ft.Item1 + x * (Ft.Item2 + x * (Ft.Item3 +  x * Ft.Item4));

		double[] SubArray(double[] din, ushort offset, ushort length)
		{
			double[] result = new double[length];
			Array.Copy(din, offset, result, 0, length);
			return result;
		}

		static double CubicSlope(double p1, double p2, double p3, double x)
		{
			return p1 + x * (2 * p2 + 3 * x * p3); 
		}

		// https://blekenbleu.github.io/static/ImageProcessing/MonotoneCubic.htm
		static bool Monotonic(double p1, double p2, double p3)
		{
			slope[0] = CubicSlope(p1, p2, p3, xmin);
			slope[2] = CubicSlope(p1, p2, p3, xmax);
			if (0 <= slope[0] * slope[2])
			{
				inflection = - p2 / (3 * p3);
				slope[1] = (xmax < inflection || inflection < xmin) ? 0
						 : CubicSlope(p1, p2, p3, inflection);
				return 0 <= slope[0] * slope[1];
			} else inflection = slope[1] = 0;
			return false;
		}

		// iteratively invoked for NelderMeadSimplex solution to monotonic cubic Fit.Curve
		readonly Func<double, double, double, double, double, double>
					CurveFunc = (p0, p1, p2, p3, x) => ConstrainedCubic(p0, p1, p2, p3, x);
		static double ConstrainedCubic(double p0, double p1, double p2, double p3, double x)
		{
			double sx, inflection;
			Count++; 
			if (0 <= CubicSlope(p1, p2, p3, xmin) * (sx = CubicSlope(p1, p2, p3, xmax))
			 && (xmax < (inflection = - p2 / (3 *p3)) || xmin > inflection
			 || 0 <= sx * CubicSlope(p1, p2, p3, inflection))) 
				return p0 + x * (p1 + x * (p2 + x * p3));	// unconstrained
			else return 2 * (0 > m ? ymax : ymin); 	// tell NelderMeadSimplex slope changes are bad
		}
	}
}
