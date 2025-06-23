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
			p = M.LinFit % 3;
			ymax = 1.2 * (Ymax - Ymin) + Ymin;		// legend space
			model = ScatterPlot(M.PropName[0], 0);
			if (M.axis[1])
				model.Series.Add(Scatter(1));
            if (M.axis[2])
                model.Series.Add(Scatter(2));

            if (3 > M.LinFit && M.min[p,M.which] < M.max[p,M.which])	// curve fit?
			{
				// https://numerics.mathdotnet.com/Regression
				ys = GetRow(O.x, (ushort)(M.LinFit), M.start[M.which], M.length);
				xs = GetRow(O.x, 3, M.start[M.which], M.length);
				(double, double)fl = Fit.Line(xs, ys);
				B = fl.Item1;
				m = fl.Item2;
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				M.Current += $";   R-squared = {r2:0.00}";
				model.Series.Add(LineDraw(m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   R-squared = {r2:0.00}";
				M.XYprop2 = Curve(M.min[3,M.which], M.max[3,M.which]);
			}
			else lfs = "";

			M.XYprop1 = $"{M.min[p,M.which]:#0.000} <= Y <= {M.max[p,M.which]:#0.000};  "
					 + $"{M.min[3,M.which]:#0.000} <= X <= {M.max[3,M.which]:#0.000}" + lfs;

			plot.Model = model;											// OxyPlot
			if (M.axis[1] && M.axis[2])
				M.D3vis = Visibility.Visible;
		}

		// draw line fit
		LineSeries LineDraw(double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			// min X value, Y value calculated from X value
			line.Points.Add(new DataPoint(M.min[3,M.which], B + m * M.min[3,M.which]));
			// max X value, Y value calculated from X value
			line.Points.Add(new DataPoint(M.max[3,M.which], B + m * M.max[3,M.which]));
			line.Title = title;
			return line;
		}
	}
}
