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
			xmin = Plugin.xmin[Model.which];
			PlotModel model = new PlotModel { Title = Model.Title };

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = Model.Yprop,
				Minimum = ymin,
				Maximum = ymax
			});

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = Model.Xprop,
				Minimum = xmin,
				Maximum = xmax
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
			ushort end = (start[Model.which] <= ln2) ? (ushort)(start[Model.which] + ln2) : length;

			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (ushort i = start[Model.which]; i < end; i++)
				scatterSeries.Points.Add(new ScatterPoint(x[i], y[i], size));
			scatterSeries.MarkerFill = OxyColors.Red;
			scatterSeries.Title = title;
			return scatterSeries;
		}

		void RandomPlot()
		{
			// fill the plot with random data
			Random rnd = new Random();
			double xi;
			ymin = Plugin.ymin[Model.which];
			ymax = 100 + Plugin.ymin[Model.which];
			xmin = Plugin.xmin[Model.which];
			xmax = 100 + xmin;
			xi = 100.0 / ln2;
			for (int i = 0; i < ln2; i++)	// fill the plot
			{
				y[i] = ymin + 100 * rnd.NextDouble();
				x[i] = xmin;
				xmin += xi;
			}

			plot.Model = ScatterPlot("Random plot");
		}
	}
}
