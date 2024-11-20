// https://stackoverflow.com/questions/12946341/algorithm-for-scatter-plot-best-fit-line
using System;
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
		public List<XYvalue> LinearBestFit(double[] x, double[] y,
												  out double m, out double b)
		{
            List<XYvalue> samples = Enumerable.Range(0, x.Length).Select
							(i => new XYvalue { Y = y[i], X = x[i] }).ToList();
			int n = samples.Count;
			double meanX = samples.Average(point => point.X);
			double meanY = samples.Average(point => point.Y);
			double sumXX = samples.Sum(point => point.X * point.X);
			double sumXY = samples.Sum(point => point.X * point.Y);

			m = (sumXY / n - meanX * meanY) / (sumXX / n - meanX * meanX);
			b = (meanY - m * meanX);
/*
			double a1 = m;
			double b1 = b;

			return samples.Select(point => new XYvalue()
			{ X = point.X, Y = a1 * point.X + b1 }).ToList();
 */
			return samples;
		}

		double[] SubArray(double[] din, int offset, int length)
		{
			double[] result = new double[length];
			Array.Copy(din, offset, result, 0, length);
			return result;
		}

/*      float[] x = new float[] { 1, 2, 3, 4, 5 },
			    y = new float[] { 12, 16, 34, 45, 47 };
        public static void Show(float[] x, float[] y)
		{
            List<XYvalue> samples = Enumerable.Range(0, x.Length).Select
							(i => new XYvalue { Y = y[i], X = x[i] }).ToList();
            double m, b;

			List<XYvalue> Fit = LinearBestFit(samples, out m, out b);

			Console.WriteLine($"y = {m:#0.####}x {b:+#0.####;-#0.####}");

			for(int i = 0; i < samples.Count; i++)
				Console.WriteLine("X = {0}, Y = {1}, Fit = {2:#.###}",
								  samples[i].X, samples[i].Y, Fit[i].Y);
		}	*/
	}
}
