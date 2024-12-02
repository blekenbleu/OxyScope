using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Linq;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		static double[] c;				// least squares fit coefficient[s]
		string lfs;
		static ushort Count;
		static (double, double, double, double) Ft;

		internal void Plot()
		{
			ymax = 1.3 * (Ymax - Ymin) + Ymin;	// legend space

			PlotModel model = ScatterPlot("60 Hz samples");

			lfs = "";
			if (M.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				double[] xs = SubArray(M.x, M.start[M.which], M.length),
						 ys = SubArray(M.y, M.start[M.which], M.length);
				(double, double)p = Fit.Line(xs, ys);

				B = p.Item1;
				m = p.Item2;
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				M.Current += $";   R-squared = {r2:0.00}";
				model.Series.Add(LineDraw(m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   R-squared = {r2:0.00}";

				// cubic fit https://posts5865.rssing.com/chan-58562618/latest.php
				c = Fit.Polynomial(xs, ys, 3, MathNet.Numerics.LinearRegression.DirectRegressionMethod.QR);

                bool converge = true, m0 = Monotonic(c[1], c[2], c[3]);
				if (m0)
				{
					// https://oxyplot.readthedocs.io/en/latest/models/series/FunctionSeries.html
					model.Series.Add(new FunctionSeries(cubicfit, xmin, xmax,
								(xmax - xmin) / 50, "cubic fit"));              // x increments
					M.XYprop2 = $"cubic:  {c[0]:#0.0000}+{c[1]:#0.0000}*x+{c[2]:#0.0000}*x**2+{c[3]:#0.00000}*x**3 "
								  + $"slopes:  {slope[0]:#0.0000}, {slope[1]:#0.0000}@{inflection:#0.00}, {slope[2]:#0.0000}";
                    M.XYprop3 = "";
                }
				else
//				if (!m0)
				{                               // https://blekenbleu.github.io/static/ImageProcessing/MonotoneCubic.htm
					Count = 0;                  // how many times ConstrainedCubic() invoked?
												// Non-linear least-squares fit arbitrary function y : x -> f(p0, p1, p2, p3, x) to points (x,y),
												// returning best fitting parameter p0, p1, p2 and p3, based on tolerance
												// https://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method#Termination
					try
					{
						Ft = Fit.Curve(xs, ys, CurveFunc, B, m, 0, 0,
						 	0.00001,                                    // tolerance <==== decent match to unforced cubic
							1000);                                       // max iterations, mostly < 300
					}
					// https://numerics.mathdotnet.com/api/MathNet.Numerics.Optimization/MaximumIterationsException.htm
					catch (Exception) { converge = false; }

					if (converge)
					{
						c[0] = Ft.Item1; c[1] = Ft.Item2; c[2] = Ft.Item3; c[3] = Ft.Item4;
						converge = Monotonic(c[1], c[2], c[3]);
						M.XYprop2 = $"constrained cubic:  {c[0]:#0.0000} + {c[1]:#0.000000}*x "
									  + $"+ {c[2]:#0.000000}*x**2 + {c[3]:#0.000000}*x**3;  Count = {Count / M.length}"
									  + $";  slopes:  {slope[0]:#0.00000}, {slope[1]:#0.00000}@{inflection:#0.00}, {slope[2]:#0.00000}";
						model.Series.Add(new FunctionSeries(cubicfit, xmin, xmax,
										 (xmax - xmin) / 50, "constrained cubic fit"));     // x increments
							
					} else M.XYprop3 = "";
					if (!converge)
					{
						lfs += ";  ** Cubic fits failed! **";
						converge = M.AutoPlot = false;
					}
				}
//				else M.XYprop3 = "";

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
					else
					{   // merge old and new curves
						xs = new double[8];
						ys = new double[8];
						ushort k = 0;
						var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
						for (ushort j = 0; j < 5; j++)
						{
							xs[k] = xmin + j * (xmax - xmin) / 4;
							ys[k] = Cubic(xs[k], c);
							scatterSeries.Points.Add(new ScatterPoint(xs[k], ys[k], 2));
							xs[++k] = M.Coef[4] + j * (M.Coef[5] - M.Coef[4]) / 4;
							ys[k] = Cubic(xs[k], M.Coef);
                            scatterSeries.Points.Add(new ScatterPoint(xs[k], ys[k], 3));
                        }
						scatterSeries.Title = "resample cubics";
						scatterSeries.MarkerFill = OxyColors.BlueViolet;
                        model.Series.Add(scatterSeries);
                        c = Fit.Polynomial(xs, ys, 3, MathNet.Numerics.LinearRegression.DirectRegressionMethod.QR);
						Monotonic(c[1], c[2], c[3]);
						model.Series.Add(new FunctionSeries(cubicfit, Xmin, Xmax,
								(Xmax - Xmin) / 50, "expanded cubic fit"));
						M.XYprop3 = $"expanded cubic:  {c[0]:#0.0000} + {c[1]:#0.000000}*x "
									  + $"+ {c[2]:#0.000000}*x**2 + {c[3]:#0.000000}*x**3;  slopes:  {slope[0]:#0.00000}"
									  + $", {slope[1]:#0.00000}@{inflection:#0.00}, {slope[2]:#0.00000}";
						M.Coef = new double[] { c[0], c[1], c[2], c[3], Xmin, Xmax };
					}
				}
                else if (converge && 0 >= M.ymin[M.which] && 0 <= M.ymax[M.which] && 0 >= xmin && 0 <= xmax)
				{
					c = Fit.LinearCombination(xs, ys, x => x);
					model.Series.Add(LineDraw(c[0], 0, "Fit thru origin"));
					lfs += $";  origin slope = {c[0]:#0.0000}";
				}

			} else M.XYprop2 = "";

			M.XYprop = $"{xmin:#0.000} <= X <= "
						 + $"{xmax:#0.000};  "
						 + $"{M.ymin[M.which]:#0.000} <= Y <= "
						 + $"{M.ymax[M.which]:#0.000}" + lfs;

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
