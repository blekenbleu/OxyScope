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

		internal void Plot()
		{
			ymax = 1.3 * (Plugin.ymax[Model.which] - Plugin.ymin[Model.which])
				 + Plugin.ymin[Model.which];	// legend space
			xmax = Plugin.xmax[Model.which];

			PlotModel model = ScatterPlot(Model.which, "60 Hz samples");

			lfs = "";
			if (Model.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				double[] xs = SubArray(x, start[Model.which], ln2),
						 ys = SubArray(y, start[Model.which], ln2);
				(double, double)p = Fit.Line(xs, ys);
				Model.B = p.Item1;
				Model.m = p.Item2;
                double r2 = GoodnessOfFit.RSquared(xs.Select(x => Model.B + Model.m * x), ys);
				Model.Current += $";   R-squared = {r2:0.00}";
				if (2 == Model.Refresh && r2 < Model.R2)
					return;

				Model.R2 = r2;	
				model.Series.Add(LineDraw(Plugin.xmin[Model.which], Model.m, Model.B, "linear fit"));
                lfs = $";   linear:  {Model.B:#0.0000} + {Model.m:#0.0000}*x;   R-squared = {r2:0.00}";

                // cubic fit https://posts5865.rssing.com/chan-58562618/latest.php
                c = Fit.Polynomial(xs, ys, 3, MathNet.Numerics.LinearRegression.DirectRegressionMethod.QR);
				// cubic slopes signs should match Model.m at xmin, xmax
				c = TweakCubic(c);

                // https://oxyplot.readthedocs.io/en/latest/models/series/FunctionSeries.html
                model.Series.Add(new FunctionSeries(cubicfit, Plugin.xmin[Model.which], Plugin.xmax[Model.which],
								(Plugin.xmax[Model.which]-Plugin.xmin[Model.which])/50, "cubic fit"));
				Model.XYprop2 = $"  cubic:  {c[0]:#0.0000} + {c[1]:#0.000000}*x + {c[2]:#0.000000}*x**2 + {c[3]:#0.000000}*x**3";

                if (0 >= Plugin.ymin[Model.which] && 0 <= ymax
				 && 0 >= Plugin.xmin[Model.which] && 0 <= xmax)
				{
					c = Fit.LinearCombination(xs, ys, x => x);
					model.Series.Add(LineDraw(Plugin.xmin[Model.which], c[0], 0, "Fit thru origin"));
					Model.XYprop2 += $";       origin slope = {c[0]:#0.0000}";
				}

			} else Model.XYprop2 = "";
			Model.XYprop = $"{Plugin.xmin[Model.which]:#0.000} <= X <= "
					     + $"{xmax:#0.000};  "
					     + $"{Plugin.ymin[Model.which]:#0.000} <= Y <= "
					     + $"{Plugin.ymax[Model.which]:#0.000}" + lfs;

			plot.Model = model;								// OxyPlot
			Plugin.xmax[Model.which] = Plugin.xmin[Model.which] = 0;	// free which buffer for reuse
		}

		LineSeries LineDraw(double start, double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(start, B + m * start));
			line.Points.Add(new DataPoint(Plugin.xmax[Model.which],
						 				  B + m * Plugin.xmax[Model.which]));
			line.Title = title;
			return line;
		}
	}
}
