using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OxyPlotPlugin
{
	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public Plugin Plugin { get; }

		public Control()
		{
			InitializeComponent();
		}

		public Control(Plugin plugin) : this()
		{
			this.Plugin = plugin;
		}

		private void ScatterSeries_Click(object sender, RoutedEventArgs e)
		{
			Random rnd=new Random();
			double[] x = { 1.2, 2.1, 2.9, 4.5, 5, 5.7, 6.9, 8.3, 9.12, 10.0 };
			double[] y = new double[x.Length].Select(v => 10 * rnd.NextDouble()).ToArray();
			var model = new PlotModel { Title = "OxyPlot - Scatter Series" };
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = "Y",
			});
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = "Number of items",
			});
			model = AddScatter(model, x, y, 5, OxyColors.Red);
			model.Series[model.Series.Count - 1].Title = "X2 Data";
			model.LegendPosition = LegendPosition.TopLeft;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;

			model.LegendBorderThickness = 1;
			plot.Model = model;
		}

		private PlotModel AddScatter(PlotModel model, double[] x, double[] y, int size, OxyColor color)
		{
			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (int i = 0; i < x.Length; i++)
				scatterSeries.Points.Add(new ScatterPoint(x[i], y[i], size));
			scatterSeries.MarkerFill = color;
			model.Series.Add(scatterSeries);
			return model;
		} 
	}	// class
}
