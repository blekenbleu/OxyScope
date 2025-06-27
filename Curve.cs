using MathNet.Numerics;
using OxyPlot;
using OxyPlot.Series;
using System;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		static (double, double, double, double) Ft;
		static double m, B, inflection;
		static ushort Count;

		string Poly(double left, double right, PlotModel model)
		{
			// https://oxyplot.readthedocs.io/en/latest/models/series/FunctionSeries.html
			try {	// CubicFit() gets declared as FunctionSeries Func in LeastSquares.cs
				double incr = (right - left) / 50;	// 50 x increments
				FunctionSeries function = new FunctionSeries(CubicFit, left, right, incr, "cubic fit")
				{ Color = OxyColors.Orange };
				model.Series.Add(function);
			} catch (Exception e) {
				M.LinFit = 0;
				M.XYprop2 = "Poly(): " + e?.ToString();
			}
			return $"cubic:  {coef[0]:#0.0000}+{coef[1]:#0.0000}*x+{coef[2]:#0.0000}*x**2+{coef[3]:#0.00000}*x**3"
				 + $";  Slopes:  {Slope[0]:#0.0000}, {Slope[1]:#0.0000}@{inflection:#0.00}, {Slope[2]:#0.0000}";
		}

		string Curve(double left, double right, PlotModel model)
		{
			// cubic fit https://posts5865.rssing.com/chan-58562618/latest.php
			coef = Fit.Polynomial(xs, ys, 3, MathNet.Numerics.LinearRegression.DirectRegressionMethod.QR);

			if (Monotonic(coef))
				return Poly(left, right, model);

			else
			{							// https://blekenbleu.github.io/static/ImageProcessing/MonotoneCubic.htm
										// Non-linear least-squares fit y : x -> f(p0, p1, p2, p3, x) to points (x,y),
										// returning best fitting parameter p0, p1, p2 and p3, based on tolerance
										// https://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method#Termination
				bool converge = true;

				Count = 0;				// Fit.Curve may invoke CurveFunc => ConstrainedCubic() multiple times;
				try
				{
					Ft = Fit.Curve(xs, ys, CurveFunc, B, m, 0, 0,	// LeastSquares.cs
					 	0.00001,									// tolerance <==== decent match to unforced coef
						1000);										// max iterations, mostly < 300
				}
				// https://numerics.mathdotnet.com/api/MathNet.Numerics.Optimization/MaximumIterationsException.htm
				catch (Exception) { converge = false; }

				if (converge)
				{
					coef[0] = Ft.Item1; coef[1] = Ft.Item2; coef[2] = Ft.Item3; coef[3] = Ft.Item4;
					if (Monotonic(coef))
					{
						FunctionSeries function = new FunctionSeries(CubicFit, left, right,
								 (right - left) / 50, "constrained coef fit") { Color = OxyColors.Orange };	// x increments
						model.Series.Add(function);
						return $"constrained coef:  {coef[0]:#0.0000} + {coef[1]:#0.000000}*x;  Count = {Count}"
							 + $"+ {coef[2]:#0.000000}*x**2 + {coef[3]:#0.000000}*x**3"
							 + $";  Slopes:  {Slope[0]:#0.00000}, {Slope[1]:#0.00000}@{inflection:#0.00}, {Slope[2]:#0.00000}";
					}
				}
				return "** non-monotonic! ** " + Poly(left, right, model);
			}
		}
	}
}
