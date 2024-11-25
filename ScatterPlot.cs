using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		PlotModel ScatterPlot(int which, string title)
		{
			PlotModel model = new PlotModel { Title = Model.Title };

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = Model.Yprop,
				Minimum = Plugin.ymin[which],
				Maximum = ymax
			});

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = Model.Xprop,
				Minimum = Plugin.xmin[which],
				Maximum = xmax
			});

			model.Series.Add(Scatter(which, title));
			model.LegendPosition = LegendPosition.TopLeft;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;
			model.LegendBorderThickness = 1;
			return model;
		}

		private ScatterSeries Scatter(int which, string title)
		{
			int size = 2;	// plot dot size
			int end = (start[which] <= ln2) ? start[which] + ln2 : length;

			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (int i = start[which]; i < end; i++)
				scatterSeries.Points.Add(new ScatterPoint(x[i], y[i], size));
			scatterSeries.MarkerFill = OxyColors.Red;
			if (null != title)
				scatterSeries.Title = title;
			return scatterSeries;
		}

		void Init()
		{
			// fill the plot with random data
			Random rnd = new Random();
			double xp, xi, yp, ymin  = Plugin.ymin[which];
			ymax = Plugin.ymax[which];
			xmax = Plugin.xmax[which];
			yp = ymax - ymin;
			xp = Plugin.xmin[which];
			xi = (xmax - xp) / ln2;
			for (int i = 0; i < ln2; i++)	// fill the plot
			{
				y[i] = ymin + yp * rnd.NextDouble();
				x[i] = xp;
				xp += xi;
			}

			plot.Model = ScatterPlot(0, null);
		}
	}
}
