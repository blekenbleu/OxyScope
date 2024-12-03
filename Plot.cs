using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;
using System.Linq;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		static double[] c;				// least squares fit coefficient[s]
		string lfs;
		static ushort Count;
		static (double, double, double, double) Ft;
		PlotModel model;
		bool converge = true;
		double[] xs, ys;

		internal void Plot()
		{
			ymax = 1.3 * (Ymax - Ymin) + Ymin;	// legend space

			model = ScatterPlot("60 Hz samples");

			lfs = "";
			if (M.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				xs = SubArray(O.x, M.start[M.which], M.length);
				ys = SubArray(O.y, M.start[M.which], M.length);
				(double, double)p = Fit.Line(xs, ys);

				B = p.Item1;
				m = p.Item2;
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				M.Current += $";   R-squared = {r2:0.00}";
				model.Series.Add(LineDraw(m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   R-squared = {r2:0.00}";

				M.XYprop2 = Curve(xmin, xmax, "");
			/*
				if (2 == M.Refresh)
				{
					if (!converge)
					{
						c[0] = B; c[1] = m; c[2] = c[3] = 0;
					}
					if (null == M.Coef)
					{
						M.Coef = new double[] { c[0], c[1], c[2], c[3], xmin, xmax };
					}
					else if (converge)
					{   // merge old and new curves
						xs = new double[8];
						ys = new double[8];
						ushort k = 0;
						var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
						for (ushort j = 0; j < 5; j++)
						{
							xs[k] = xmin + j * (xmax - xmin) / 4;
							ys[k] = Cubic(xs[k], c);
							scatterSeries.Points.Add(new ScatterPoint(xs[k], ys[k], 4));
							xs[++k] = M.Coef[4] + j * (M.Coef[5] - M.Coef[4]) / 4;
							ys[k] = Cubic(xs[k], M.Coef);
							scatterSeries.Points.Add(new ScatterPoint(xs[k], ys[k], 3));
						}
						scatterSeries.Title = "resample cubics";
						scatterSeries.MarkerFill = OxyColors.BlueViolet;
						model.Series.Add(scatterSeries);
						M.XYprop3 = Curve(Xmin, Xmax, "expanded ");
						M.Coef = new double[] { c[0], c[1], c[2], c[3], Xmin, Xmax };
					}
					else if ((!converge) && 0 >= M.ymin[M.which] && 0 <= M.ymax[M.which] && 0 >= xmin && 0 <= xmax)
					{
						c = Fit.LinearCombination(xs, ys, x => x);
						model.Series.Add(LineDraw(c[0], 0, "Fit thru origin"));
						lfs += $";  origin slope = {c[0]:#0.0000}";
					}
				}
			 */
			}

			M.XYprop = $"{xmin:#0.000} <= X <= {xmax:#0.000};  "
					 + $"{M.ymin[M.which]:#0.000} <= Y <= {M.ymax[M.which]:#0.000}" + lfs;

			plot.Model = model;											// OxyPlot
			M.Done = true;
		}

		LineSeries LineDraw(double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(xmin, B + m * xmin));
			line.Points.Add(new DataPoint(xmax, B + m * xmax));
			line.Title = title;
			return line;
		}
	}
}
