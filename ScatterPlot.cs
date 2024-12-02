using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		PlotModel ScatterPlot(string title)
		{
			PlotModel model = new PlotModel { Title = M.Title };

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = M.Yprop,
				Minimum = Ymin,
				Maximum = ymax
			});

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = M.Xprop,
						Minimum = Xmin - 0.005 * (Xmax - Xmin),	// space for dot@Xmin
						Maximum = Xmax + 0.005 * (Xmax - Xmin)
            });

			model.Series.Add(Scatter(title));
			model.LegendPosition = LegendPosition.TopLeft;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;
			model.LegendBorderThickness = 1;
			return model;
		}

		private ScatterSeries Scatter(string title)
		{
			int size = 2;	// plot dot size
			ushort end = (ushort)(M.start[M.which] + M.length);

			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (ushort i = M.start[M.which]; i < end; i++)
				scatterSeries.Points.Add(new ScatterPoint(O.x[i], O.y[i], size));
			scatterSeries.MarkerFill = OxyColors.Red;
			scatterSeries.Title = title;
			return scatterSeries;
		}

		void RandomPlot()
		{
			// fill the plot with random data
			Random rnd = new Random();
			double xi;
			Ymin = M.ymin[M.which];
			ymax = 100 + Ymin;
			Xmin = M.xmin[M.which];
			Xmax = 100 + Xmin;
			xi = 100.0 / M.length;
			for (int i = 0; i < M.length; i++)	// fill the plot
			{
				O.y[i] = Ymin + 100 * rnd.NextDouble();
				O.x[i] = Xmin;
				Xmin += xi;
			}

			Xmin = M.xmin[M.which];
			plot.Model = ScatterPlot("Random plot");
		}
	}
}
