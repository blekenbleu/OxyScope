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
		readonly Func<double, double> cubicfit = (x) => c[0] + c[1] * x + c[2] * x * x + c[3] * x * x * x;

		double[] SubArray(double[] din, int offset, int length)
		{
			double[] result = new double[length];
			Array.Copy(din, offset, result, 0, length);
			return result;
		}

		// y = c[0] + c[1]*x + c[2]*x*x + c[3]*x*x*x
		// y' = c[1] + 2*c[2]*x + 3*c[3]*x*x
		double Slope(double x, double[] c)
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
		}
	}
}
