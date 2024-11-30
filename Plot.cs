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
			ymin = Plugin.ymin[Model.which];
			ymax = 1.3 * (Plugin.ymax[Model.which] - ymin) + ymin;	// legend space
			xmax = Plugin.xmax[Model.which];

			PlotModel model = ScatterPlot("60 Hz samples");

			lfs = "";
			if (Model.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				double[] xs = SubArray(x, start[Model.which], ln2),
						 ys = SubArray(y, start[Model.which], ln2);
				(double, double)p = Fit.Line(xs, ys);

				B = p.Item1;
				m = p.Item2;
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				Model.Current += $";   R-squared = {r2:0.00}";
				if (2 == Model.Refresh && r2 < Model.R2)
					return;

				Model.R2 = r2;	
				model.Series.Add(LineDraw(xmin, m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   R-squared = {r2:0.00}";

				// cubic fit https://posts5865.rssing.com/chan-58562618/latest.php
				c = Fit.Polynomial(xs, ys, 3, MathNet.Numerics.LinearRegression.DirectRegressionMethod.QR);
				// cubic slopes signs should match m at xmin, xmax
				//c = TweakCubic(c);

				bool m0 = Monotonic(c[1], c[2], c[3]);
			//	if (m0)
				{
					// https://oxyplot.readthedocs.io/en/latest/models/series/FunctionSeries.html
					model.Series.Add(new FunctionSeries(cubicfit, xmin, xmax,
								(xmax - xmin) / 50, "cubic fit"));				// x increments
					Model.XYprop2 = $"cubic:  {c[0]:#0.0000}+{c[1]:#0.0000}*x+{c[2]:#0.0000}*x**2+{c[3]:#0.00000}*x**3 "
								  + $"slopes:  {slope[0]:#0.0000}, {slope[1]:#0.0000}@{inflection:#0.00}, {slope[2]:#0.0000}";
				}
			//	else
				if (!m0)
				{								// https://blekenbleu.github.io/static/ImageProcessing/MonotoneCubic.htm
					Count = 0;					// how many times ConstrainedCubic() invoked?
					bool converge = true;
					// Non-linear least-squares fit arbitrary function y : x -> f(p0, p1, p2, p3, x) to points (x,y),
					// returning best fitting parameter p0, p1, p2 and p3, based on tolerance
					// https://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method#Termination
					try
					{
						Ft = Fit.Curve(xs, ys, CurveFunc, B, m, 0, 0,
						 	0.00001,									 // tolerance <==== decent match to unforced cubic
							1000);										 // max iterations, mostly < 300
					}
					// https://numerics.mathdotnet.com/api/MathNet.Numerics.Optimization/MaximumIterationsException.htm
					catch (Exception) { converge = false; }

					if (converge && (Monotonic(Ft.Item2, Ft.Item3, Ft.Item4) || true))
					{
						Model.XYprop3 = $"constrained cubic:  {Ft.Item1:#0.0000} + {Ft.Item2:#0.000000}*x "
									  + $"+ {Ft.Item3:#0.000000}*x**2 + {Ft.Item4:#0.000000}*x**3;  Count = {Count / 180}"
								  + $";  slopes:  {slope[0]:#0.00000}, {slope[1]:#0.00000}@{inflection:#0.00}, {slope[2]:#0.00000}";
						model.Series.Add(new FunctionSeries(ccubicfit, xmin, xmax,
										 (xmax - xmin) / 50, "constrained cubic fit"));		// x increments
					}
					else Model.XYprop3 = "cubic fits failed!";
				}	else Model.XYprop3 = "";

				if (0 >= ymin && 0 <= ymax
				 && 0 >= xmin && 0 <= xmax)
				{
					c = Fit.LinearCombination(xs, ys, x => x);
					model.Series.Add(LineDraw(xmin, c[0], 0, "Fit thru origin"));
					lfs += $";  origin slope = {c[0]:#0.0000}";
				}

			} else Model.XYprop2 = "";

			Model.XYprop = $"{xmin:#0.000} <= X <= "
						 + $"{xmax:#0.000};  "
						 + $"{ymin:#0.000} <= Y <= "
						 + $"{Plugin.ymax[Model.which]:#0.000}" + lfs;

			plot.Model = model;											// OxyPlot
		}

		LineSeries LineDraw(double start, double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(start, B + m * start));
			line.Points.Add(new DataPoint(xmax,
						 				  B + m * xmax));
			line.Title = title;
			return line;
		}
	}
}
