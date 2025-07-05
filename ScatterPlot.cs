using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		PlotModel ScopeModel()
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
				Title = M.PropName[xmap[0]],
						Minimum = Xmin[currentX] - 0.005 * (Xmax[currentX] - Xmin[currentX]),	// space for dot@Xmin
						Maximum = Xmax[currentX] + 0.005 * (Xmax[currentX] - Xmin[currentX])
			});

			model.LegendPosition = LegendPosition.TopLeft;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;
			model.LegendBorderThickness = 1;
			return model;
		}

		private ScatterSeries Scatter(uint Yprop) => Scatter(M.PropName[Yprop], Yprop);
		readonly OxyColor[] Ycolor = { OxyColors.Red, OxyColors.Green, OxyColors.Cyan };
		
		private ScatterSeries Scatter(string title, uint Yprop)	// 3 possible
		{
			int size = 2;	// plot dot size
			ushort end = (ushort)(start + Length);

			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (ushort i = start; i < end; i++)
				scatterSeries.Points.Add(new ScatterPoint(O.x[3,i], O.x[Yprop,i], size));
			scatterSeries.MarkerFill = Ycolor[Yprop];
			scatterSeries.Title = title;
			return scatterSeries;
		}

		void RandomPlot()
		{
			// fill the plot with random data
			Random rnd = new Random();
			double xi;
			Xmin[0] = min[0];								// RandomPlot()
			Xmax[0] = 100 + Xmin[0];						// RandomPlot()
			Ymin = min[3];
			ymax = 100 + Ymin;
			Length = M.length;
			xi = 100.0 / Length;
			for (int i = 0; i < Length; i++)				// fill RandomPlot()
			{
				O.x[3,i] = Ymin + 100 * rnd.NextDouble();	// RandomPlot()
				O.x[0,i] = Xmin[0];							// RandomPlot()
				Xmin[0] += xi;								// RandomPlot()
			}

			Xmin[0] = min[0];								// RandomPlot()
            PlotModel model = ScopeModel();
			model.Series.Add(Scatter("random", 0));
			plot.Model = model;
		}
	}
}
