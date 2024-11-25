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
	}
}
