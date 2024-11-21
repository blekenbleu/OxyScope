// https://stackoverflow.com/questions/12946341/algorithm-for-scatter-plot-best-fit-line
using System.Collections.Generic;
using System.Linq;

namespace OxyPlotPlugin
{
	public class XYvalue
	{
		public double X;
		public double Y;
	}

    public partial class Control
    {
		List<XYvalue> samples;

		public void LinearBestFit(out double m, out double b)
		{
			int n = length >> 1;
			double meanX = samples.Average(point => point.X);
			double meanY = samples.Average(point => point.Y);
			double sumXX = samples.Sum(point => point.X * point.X);
			double sumXY = samples.Sum(point => point.X * point.Y);

			m = (sumXY / n - meanX * meanY) / (sumXX / n - meanX * meanX);
			b = (meanY - m * meanX);
		}
	}
}
