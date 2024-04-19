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
			Random rnd=new Random();
			x = new double[180];
			y = new double[180];
			for (int i = 0; i < 180;)
			{
				y[i] = 100 * rnd.NextDouble();
				x[i] = ++i;
			}
			ScatterPlot();
		}

		public Control(Plugin plugin) : this()
		{
			this.Plugin = plugin;
		}

		private void ScatterSeries_Click(object sender, RoutedEventArgs e)
		{
			ScatterPlot();
		}

		public double[] x;
		public double[] y;
		public string variables = "OxyPlot - Scatter Series";


        public void ScatterPlot()
		{
			var model = new PlotModel { Title = variables };
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = "Grip",
			});
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = "Slip",
			});
			model = AddScatter(model, x, y, 3, OxyColors.Red);
			model.Series[model.Series.Count - 1].Title = "60Hz sample";
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
