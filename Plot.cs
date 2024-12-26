using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;
using System.Windows;           // Visibility
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
		double[] xs, ys;

		double[] GetRow(double[,] twoD, ushort row, ushort start, ushort length)
		{
			return Enumerable.Range(start, length).Select(x => twoD[row, x]).ToArray();	
		}

		internal void Plot()
		{
			p = (0 < M.LinFit) ? (M.LinFit - 1) : 0;
			ymax = 1.2 * (Ymax - Ymin) + Ymin;		// legend space
			M.XYprop2 = "";
			model = ScatterPlot("Xprop0", 0);
			if (M.a[1])
				model.Series.Add(Scatter("Xprop1", 1));
            if (M.a[2])
                model.Series.Add(Scatter("Xprop2", 2));

            if (0 < M.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				xs = GetRow(O.x, (ushort)(M.LinFit - 1), M.start[M.which], M.length);
				ys = GetRow(O.x, 3, M.start[M.which], M.length);
				(double, double)fl = Fit.Line(xs, ys);
				B = fl.Item1;
				m = fl.Item2;
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				M.Current += $";   R-squared = {r2:0.00}";
				model.Series.Add(LineDraw(m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   R-squared = {r2:0.00}";
				M.XYprop2 = Curve(M.min[p,M.which], M.max[p,M.which], "");
			}
			else lfs = "";

			M.XYprop = $"{M.min[p,M.which]:#0.000} <= X <= {M.max[p,M.which]:#0.000};  "
					 + $"{M.min[3,M.which]:#0.000} <= Y <= {M.max[3,M.which]:#0.000}" + lfs;

			plot.Model = model;											// OxyPlot
			if (M.a[1] && M.a[2])
				M.D3vis = Visibility.Visible;
		}

		LineSeries LineDraw(double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(M.min[p,M.which], B + m * M.min[p,M.which]));
			line.Points.Add(new DataPoint(M.max[p,M.which], B + m * M.max[p,M.which]));
			line.Title = title;
			return line;
		}
	}
}
