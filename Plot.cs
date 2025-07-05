using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;
using System.Windows;		   // Visibility
using System.Linq;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		static double[] coef;			// least squares fit coefficient[s]
		static double ymax;				// static for ConstrainedCubic()
		static byte Yf;					// current Y property to fit curves
		double[] xs, ys;				// Fit.Polynomial(), Fit.Curve(), Fit.Line()
		string lfs;

		double[] GetRow(double[,] twoD, ushort row, ushort start, ushort length)
		{
			return Enumerable.Range(start, length).Select(x => twoD[row, x]).ToArray();	
		}

		internal PlotModel Plot()
		{
			M.ForeVS = "White";						// disable VSclick()
			Yf = (byte)(property % 3);
			ymax = 1.2 * (Ymax - Ymin) + Ymin;		// legend space
			PlotModel model = ScopeModel();
			for (byte b = 1; b < xmap.Count; b++)
				model.Series.Add(Scatter(xmap[b]));

			if (3 > property && min[Yf] < max[Yf])	// curve fit?
			{
				// https://numerics.mathdotnet.com/Regression
				ys = GetRow(O.x, property, start, Length);
				xs = GetRow(O.x, xmap[0], start, Length);
				(double, double)fl = Fit.Line(xs, ys);
				B = fl.Item1;
				m = fl.Item2;
				double slope = m * (max[xmap[0]] - min[xmap[0]]) / (max[property] - min[property]);
				double r2 = GoodnessOfFit.RSquared(xs.Select(x => B + m * x), ys);
				M.Current += $";   R-squared = {r2:0.00}";
				model.Series.Add(LineDraw(m, B, "line fit"));
				lfs = $";   line:  {B:#0.0000} + {m:#0.00000}*x;   slope = {slope:0.00}, R-squared = {r2:0.00}";
				M.XYprop2 = Curve(min[xmap[0]], max[xmap[0]], model);
			}
			else M.XYprop2 = lfs = "";

			M.XYprop1 = $"{min[Yf]:#0.000} <= Y <= {max[Yf]:#0.000};  "
					  + $"{min[ xmap[0]]:#0.000} <= X <= {max[ xmap[0]]:#0.000}" + lfs;

			if (M.axis[1] || M.axis[2])								// 2 or 3 Y properties
				M.D3vis = Visibility.Visible;
			if (Visibility.Visible == M.PVis)
				M.ForeVS = "Green";
			return model;
		}

		// draw line fit
		LineSeries LineDraw(double m, double B, string title)
		{
			LineSeries line = new LineSeries { Color = Ycolor[Yf] };
			// min X value, Y value calculated from X value
			line.Points.Add(new DataPoint(min[xmap[0]], B + m * min[xmap[0]]));
			// max X value, Y value calculated from X value
			line.Points.Add(new DataPoint(max[xmap[0]], B + m * max[xmap[0]]));
			line.Title = title;
			return line;
		}
	}
}
