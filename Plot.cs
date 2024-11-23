using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

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
				Minimum = Plugin.ymin[Plugin.which],
				Maximum = ymax
			});

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = Model.Xprop,
				Minimum = Plugin.xmin[Plugin.which],
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

		LineSeries BestFit(double start, double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(start, B + m * start));
			line.Points.Add(new DataPoint(Plugin.xmax[Plugin.which],
						 				  B + m * Plugin.xmax[Plugin.which]));
			line.Title = title;
			return line;
		}
	}
}
