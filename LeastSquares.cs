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
/*		List<XYvalue> samples;

		void LinearBestFit(out double m, out double b)
		{
			double meanX = samples.Average(point => point.X);
			double meanY = samples.Average(point => point.Y);
			double sumXX = samples.Sum(point => point.X * point.X);
			double sumXY = samples.Sum(point => point.X * point.Y);

			m = (sumXY / ln2 - meanX * meanY) / (sumXX / ln2 - meanX * meanX);
			b = (meanY - m * meanX);
		}
 */
		double[] SubArray(double[] din, int offset, int length)
		{
			double[] result = new double[length];
			Array.Copy(din, offset, result, 0, length);
			return result;
		}
	}
}
