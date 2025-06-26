using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;
using System.Windows;		   // Visibility
using System.Linq;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		static double[] coef;				// least squares fit coefficient[s]
		double[] xs, ys;				// Fit.Polynomial(), Fit.Curve(), Fit.Line()
		string lfs;

		double[] GetRow(double[,] twoD, ushort row, ushort start, ushort length)
		{
			return Enumerable.Range(start, length).Select(x => twoD[row, x]).ToArray();	
		}

		internal PlotModel Plot()
		{
			p = M.LinFit % 3;
			ymax = 1.2 * (Ymax - Ymin) + Ymin;		// legend space
			PlotModel model = ScopeModel();
			model.Series.Add(Scatter(0));
			if (M.axis[1])
				model.Series.Add(Scatter(1));
			if (M.axis[2])
				model.Series.Add(Scatter(2));

			if (3 > M.LinFit && min[p] < max[p])	// curve fit?
			{
				// https://numerics.mathdotnet.com/Regression
				ys = GetRow(O.x, M.LinFit, start, Length);
				xs = GetRow(O.x, 3, start, Length);
				(double, double)fl = Fit.Line(xs, ys);
				B = fl.Item1;
				m = fl.Item2;
				double slope = m * (max[3] - min[3]) / (max[M.LinFit] - min[M.LinFit]);
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				M.Current += $";   R-squared = {r2:0.00}";
				model.Series.Add(LineDraw(m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   Slope = {slope:0.00}, R-squared = {r2:0.00}";
				M.XYprop2 = Curve(min[3], max[3], model);
			}
			else M.XYprop2 = lfs = "";

			M.XYprop1 = $"{min[p]:#0.000} <= Y <= {max[p]:#0.000};  "
					 + $"{min[3]:#0.000} <= X <= {max[3]:#0.000}" + lfs;

			if (M.axis[1] || M.axis[2])								// 2 or 3 Y properties
				M.D3vis = Visibility.Visible;
			return model;
		}

		// draw line fit
		LineSeries LineDraw(double m, double B, string title)
		{
			LineSeries line = new LineSeries { Color = Ycolor[p] };
			// min X value, Y value calculated from X value
			line.Points.Add(new DataPoint(min[3], B + m * min[3]));
			// max X value, Y value calculated from X value
			line.Points.Add(new DataPoint(max[3], B + m * max[3]));
			line.Title = title;
			return line;
		}
	}
}
