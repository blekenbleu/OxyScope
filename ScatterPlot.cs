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
			PlotModel model = new PlotModel { Title = Model.Title };

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = Model.Yprop,
				Minimum = Ymin,
				Maximum = ymax
			});

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = Model.Xprop,
				Minimum = Xmin,
				Maximum = Xmax
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
			ushort end = (ushort)(Model.start[Model.which] + Model.length);

			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (ushort i = Model.start[Model.which]; i < end; i++)
				scatterSeries.Points.Add(new ScatterPoint(Model.x[i], Model.y[i], size));
			scatterSeries.MarkerFill = OxyColors.Red;
			scatterSeries.Title = title;
			return scatterSeries;
		}

		void RandomPlot()
		{
			// fill the plot with random data
			Random rnd = new Random();
			double xi;
			Ymin = Model.ymin[Model.which];
			ymax = 100 + Ymin;
			Xmin = Model.xmin[Model.which];
			Xmax = 100 + Xmin;
			xi = 100.0 / Model.length;
			for (int i = 0; i < Model.length; i++)	// fill the plot
			{
				Model.y[i] = Ymin + 100 * rnd.NextDouble();
				Model.x[i] = Xmin;
				Xmin += xi;
			}

			Xmin = Model.xmin[Model.which];
			plot.Model = ScatterPlot("Random plot");
		}
	}
}
