using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		PlotModel ScatterPlot()
		{
			PlotModel model = new PlotModel { Title = M.Title };
			string at = M.PropName[0];
			if (M.axis[1])
				at += ", " + M.PropName[1];
			if (M.axis[2])
				at += ", " + M.PropName[2];
			if (null == at || 0 == at.Length)
				at = "watch this space";

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = at,
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

			model.LegendPosition = LegendPosition.TopLeft;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;
			model.LegendBorderThickness = 1;
			return model;
		}

		private ScatterSeries Scatter(uint Yprop) => Scatter(M.PropName[Yprop], Yprop);
		readonly OxyColor[] color = { OxyColors.Red, OxyColors.Green, OxyColors.Cyan };
		
		private ScatterSeries Scatter(string title, uint Yprop)	// 3 possible
		{
			int size = 2;	// plot dot size
			ushort end = (ushort)(M.start[M.which] + M.length);

			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (ushort i = M.start[M.which]; i < end; i++)
				scatterSeries.Points.Add(new ScatterPoint(O.x[3,i], O.x[Yprop,i], size));
			scatterSeries.MarkerFill = color[Yprop];
			scatterSeries.Title = title;
			return scatterSeries;
		}

		void RandomPlot()
		{
			// fill the plot with random data
			Random rnd = new Random();
			double xi;
			Xmin = M.min[0,M.which];	 // RandomPlot()
			Xmax = 100 + Xmin;
			Ymin = M.min[3,M.which];
			ymax = 100 + Ymin;
			xi = 100.0 / M.length;
			for (int i = 0; i < M.length; i++)	// fill the plot
			{
				O.x[3,i] = Ymin + 100 * rnd.NextDouble();
				O.x[0,i] = Xmin;
				Xmin += xi;
			}

			Xmin = M.min[0,M.which];
			model = ScatterPlot();
			model.Series.Add(Scatter("random", 0));
			plot.Model = model;
		}
	}
}
