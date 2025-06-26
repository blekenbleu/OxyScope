// https://stackoverflow.com/questions/12946341/algorithm-for-scatter-plot-best-fit-line
// alternatively:  https://christoph.ruegg.name/blog/linear-regression-mathnet-numerics
using System;

namespace blekenbleu.OxyScope
{
	public class XYvalue
	{
		public double X;
		public double Y;
	}

	public partial class Control
	{
		static readonly double[] Slope = new double[] { 0, 0, 0 }; // LeastSquares inflections

		// Cubic functions for OxyPlot FunctionSeries()
		readonly Func<double, double> CubicFit = (x) => coef[0] + x * (coef[1] + x * (coef[2] +  x * coef[3]));

		static double CubicSlope(double p1, double p2, double p3, double x)
		{
			return p1 + x * (2 * p2 + 3 * x * p3); 
		}

		// https://blekenbleu.github.io/static/ImageProcessing/MonotoneCubic.htm
		static bool Monotonic(double p1, double p2, double p3)
		{
			Slope[0] = CubicSlope(p1, p2, p3, min[p]);
			Slope[2] = CubicSlope(p1, p2, p3, max[p]);
			if (0 <= Slope[0] * Slope[2])
			{
				inflection = - p2 / (3 * p3);
				Slope[1] = (max[p] < inflection || inflection < min[p]) ? 0
						 : CubicSlope(p1, p2, p3, inflection);
				return 0 <= Slope[0] * Slope[1];
			} else inflection = Slope[1] = 0;
			return false;
		}

		// iteratively invoked for NelderMeadSimplex solution to monotonic coef Fit.Curve
		readonly Func<double, double, double, double, double, double>
					CurveFunc = (p0, p1, p2, p3, x) => ConstrainedCubic(p0, p1, p2, p3, x);

		static double ConstrainedCubic(double p0, double p1, double p2, double p3, double x)
		{
			double sx, inflection;
			Count++; 
			if (0 <= CubicSlope(p1, p2, p3, min[p]) * (sx = CubicSlope(p1, p2, p3, max[p]))
			 && (max[p] < (inflection = - p2 / (3 *p3)) || min[p] > inflection
			 || 0 <= sx * CubicSlope(p1, p2, p3, inflection))) 
				return p0 + x * (p1 + x * (p2 + x * p3));	// unconstrained
			else return 2 * (0 > m ? ymax : Ymin); 			// penalize NelderMeadSimplex non-monotonic solutions
		}
	}
}
