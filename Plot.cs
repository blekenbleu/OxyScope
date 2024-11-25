using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		static double[] c;				// least squares fit coefficient[s]
		bool plotting = false;			// don't replot when already busy

		void Plot()
		{
			if (plotting)
				return;
			plotting = true;			// disable OxyScope multiple invocations

			ymax = 1.2 * (Plugin.ymax[which] - Plugin.ymin[which])
				 + Plugin.ymin[which];	// legend space
			xmax = Plugin.xmax[which];
			Model.Xrange = Model.Refresh ? 0 : xmax - Plugin.xmin[which];

			PlotModel model = ScatterPlot(which, "60 Hz samples");
			Model.XYprop = $"{Plugin.xmin[which]:#0.000} <= X <= "
					     + $"{xmax:#0.000};  "
					     + $"{Plugin.ymin[which]:#0.000} <= Y <= "
					     + $"{Plugin.ymax[which]:#0.000}";

			if (Model.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				double[] xs = SubArray(x, start[which], ln2),
						 ys = SubArray(y, start[which], ln2);
				(double, double)p = Fit.Line(xs, ys);
				Model.B = p.Item1;
				Model.m = p.Item2;
				model.Series.Add(LineDraw(Plugin.xmin[which], Model.m, Model.B, "linear fit"));
				Model.XYprop += $";  intercept = {Model.B:#0.0000}, slope = {Model.m:#0.0000}";

                // cubic fit https://posts5865.rssing.com/chan-58562618/latest.php
                c = Fit.Polynomial(xs, ys, 3, MathNet.Numerics.LinearRegression.DirectRegressionMethod.QR);

                // https://oxyplot.readthedocs.io/en/latest/models/series/FunctionSeries.html
                model.Series.Add(new FunctionSeries(cubicfit, Plugin.xmin[which], Plugin.xmax[which], 0.005, "cubic fit"));
				Model.XYprop += $";  {c[0]:#0.0000} + {c[1]:#0.000000}*x + {c[2]:#0.000000}*x**2 + {c[3]:#0.000000}*x**3";

                if (0 >= Plugin.ymin[which] && 0 <= Plugin.ymax[which])
				{
					c = Fit.LinearCombination(xs, ys, x => x);
					model.Series.Add(LineDraw(0, c[0], 0, "Fit thru origin"));
					Model.XYprop += $", origin slope = {c[0]:#0.0000}";
				}

			}

			plot.Model = model;								// OxyPlot
			Plugin.xmax[which] = Plugin.xmin[which] = 0;	// free which buffer for reuse
            plotting = false;								// enable OxyScope updates
		}

		LineSeries LineDraw(double start, double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(start, B + m * start));
			line.Points.Add(new DataPoint(Plugin.xmax[which],
						 				  B + m * Plugin.xmax[which]));
			line.Title = title;
			return line;
		}
	}
}
