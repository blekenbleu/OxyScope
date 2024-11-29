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
		readonly Func<double, double> cubicfit = (x) => c[0] + x * (c[1] + x * (c[2] +  x * c[3]));

		readonly Func<double, double> ccubicfit = (x) => Ft.Item1 + Rx(x) * (Ft.Item2 + Rx(x) * (Ft.Item3 +  Rx(x) * Ft.Item4));

		double[] SubArray(double[] din, ushort offset, ushort length)
		{
			double[] result = new double[length];
			Array.Copy(din, offset, result, 0, length);
			return result;
		}

		// y = c[0] + c[1]*x + c[2]*x*x + c[3]*x*x*x
		// y' = c[1] + 2*c[2]*x + 3*c[3]*x*x
/*		double Slope(double x, double[] c)
		{
			return c[1] + 2*c[2]*x + 3*c[3]*x*x;
		}

		// force zero Slope at x
		double[] ZeroSlope(double x, double[] c)
		{
			if(0 == x)
				c[1] = 0;
			else c[3] = - ((2* c[2] / x) + (c[1] / (x * x))) / 3;
			return c;
		}

		double[] TweakCubic(double[] cubic)
		{
			double[] mm = new double[] { Plugin.xmin[Model.which], Plugin.xmax[Model.which] }; 
			double smin = Slope(mm[0], cubic);
			double smax = Slope(mm[1], cubic);
			double[] d;
			string[] sx = new string[] { "in", "ax" };
			string fs = ";  forced @xm", rs =  ";  reforced @xm";
			int i;

			if (0 == Model.m || (0 < Model.m && 0 <= smin && 0 <= smax)
			 || (0 > Model.m && 0 >= smin && 0 >= smax))
				return cubic;	// probably ZeroSlope..

			i = 0 < Model.m ? 0 : 1;
			i = smin < smax ? i : 1 - i;
			d  = ZeroSlope(mm[i], cubic);

			if (0 < Model.m)
			{
				if (0 <= Slope(mm[1 - i], d))
				{
					lfs += fs + sx[i];
					return d;
				}
			}
			else if (0 >= Slope(mm[1 - i], d))
			{
				lfs += fs + sx[i];
				return d;
			}
			lfs += rs + sx[1 - i];
			return ZeroSlope(mm[1 - i], cubic);
		}	*/

		readonly Func<double, double, double, double, double, double> CurveFunc
								= (p0, p1, p2, p3, x) => ConstrainedCubic(p0, p1, p2, p3, x);
		static double Rx(double x) => x1 * (x - xmin);
		static double CubicSlope(double p1, double p2, double p3, double x)
		{
			return p1 + x * (2 * p2 + 3 * x * p3); 
		}

		static bool Monotonic(double p1, double p2, double p3)
		{
			slope[0] = CubicSlope(p1, p2, p3, xmin);
			slope[2] = CubicSlope(p1, p2, p3, xmax);
			inflection = - p2 / (3 * p3);
			slope[1] = (xmax < inflection || inflection < xmin) ? 0 : CubicSlope(p1, p2, p3, inflection);

			return 0 <= slope[0] * slope[2] && 0 <= slope[0] * slope[1];
		}

		static double ConstrainedCubic(double p0, double p1, double p2, double p3, double x)
		{
			Count++; 
			x = Rx(x);
			double y = p0 + x * (p1 + x * (p2 + x * p3));
			if (0 <= CubicSlope(p1, p2, p3, xmin) * CubicSlope(p1, p2, p3, xmax)
			&& ((0 < p2 / p3 && p2 / p3 < -3) || 0 <= CubicSlope(p1, p2, p3, xmin) * CubicSlope(p1, p2, p3, p2 / (3 * p3)))) 
				return y;	// not constrained
			else return 2 * (0 > m ? ymax : ymin); 
		}
	}
}
