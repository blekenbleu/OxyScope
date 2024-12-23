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

		double[] GetRow(double[,] twoD, ushort row, ushort start, ushort length)
		{
			return Enumerable.Range(start, length).Select(x => twoD[row, x]).ToArray();	
		}

		internal void Plot()
		{
			ymax = 1.2 * (Ymax - Ymin) + Ymin;		// legend space
			model = ScatterPlot("60 Hz samples");

			if (M.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				xs = GetRow(O.x, 0, M.start[M.which], M.length);
				ys = GetRow(O.x, 3, M.start[M.which], M.length);
				(double, double)p = Fit.Line(xs, ys);
				B = p.Item1;
				m = p.Item2;
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				M.Current += $";   R-squared = {r2:0.00}";
				model.Series.Add(LineDraw(m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   R-squared = {r2:0.00}";
				M.XYprop2 = Curve(xmin, xmax, "");
			}
			else lfs = "";

			M.XYprop = $"{xmin:#0.000} <= X <= {xmax:#0.000};  "
					 + $"{M.min[3,M.which]:#0.000} <= Y <= {M.max[3,M.which]:#0.000}" + lfs;

			plot.Model = model;											// OxyPlot
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
