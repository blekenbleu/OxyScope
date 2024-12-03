using MathNet.Numerics;
using OxyPlot.Series;
using System;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		string Curve(double left, double right, string es)
		{
			// cubic fit https://posts5865.rssing.com/chan-58562618/latest.php
			c = Fit.Polynomial(xs, ys, 3, MathNet.Numerics.LinearRegression.DirectRegressionMethod.QR);

			bool m0 = Monotonic(c[1], c[2], c[3]);
			if (m0)
			{
				// https://oxyplot.readthedocs.io/en/latest/models/series/FunctionSeries.html
				model.Series.Add(new FunctionSeries(cubicfit, left, right,
								(right - left) / 50, es + "cubic fit"));			  // x increments
				return es + $"cubic:  {c[0]:#0.0000}+{c[1]:#0.0000}*x+{c[2]:#0.0000}*x**2+{c[3]:#0.00000}*x**3 "
					 + $"slopes:  {slope[0]:#0.0000}, {slope[1]:#0.0000}@{inflection:#0.00}, {slope[2]:#0.0000}";
			}
			else
			{							// https://blekenbleu.github.io/static/ImageProcessing/MonotoneCubic.htm
				Count = 0;				// how many times ConstrainedCubic() invoked?
										// Non-linear least-squares fit y : x -> f(p0, p1, p2, p3, x) to points (x,y),
										// returning best fitting parameter p0, p1, p2 and p3, based on tolerance
										// https://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method#Termination
				try
				{
					Ft = Fit.Curve(xs, ys, CurveFunc, B, m, 0, 0,
					 	0.00001,		// tolerance <==== decent match to unforced cubic
						1000);			// max iterations, mostly < 300
				}
				// https://numerics.mathdotnet.com/api/MathNet.Numerics.Optimization/MaximumIterationsException.htm
				catch (Exception) { converge = false; }

				if (converge)
				{
					c[0] = Ft.Item1; c[1] = Ft.Item2; c[2] = Ft.Item3; c[3] = Ft.Item4;
					if (converge = Monotonic(c[1], c[2], c[3]))
					{
						model.Series.Add(new FunctionSeries(cubicfit, left, right, (right - left) / 50,	// x increments
															es + "constrained cubic fit"));
						return es + $"constrained cubic:  {c[0]:#0.0000} + {c[1]:#0.000000}*x "
							 + $"+ {c[2]:#0.000000}*x**2 + {c[3]:#0.000000}*x**3;  Count = {Count / M.length}"
							 + $";  slopes:  {slope[0]:#0.00000}, {slope[1]:#0.00000}@{inflection:#0.00}, {slope[2]:#0.00000}";
					}
				}
				converge = M.AutoPlot = false;
				return "	** Cubic fits failed! **";
			}
		}

	}
}
