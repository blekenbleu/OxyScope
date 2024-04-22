using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OxyPlotPlugin
{
	// https://intellitect.com/blog/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
	public class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
	    public void OnPropertyChanged(PropertyChangedEventArgs myevent) => PropertyChanged?.Invoke(this, myevent);

		readonly PropertyChangedEventArgs BVevent = new PropertyChangedEventArgs(nameof(OxyButVis));
		private Visibility _unseen;
		public Visibility OxyButVis
		{ 	get => _unseen;
			set
			{
				if (_unseen != value)
				{
					_unseen = value;
					PropertyChanged?.Invoke(this, BVevent);
				}
			} 
		}
	}	// class ViewModel

	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public Plugin Plugin { get; }

		public ViewModel PluginViewModel;

		public Control()
		{
			PluginViewModel = new ViewModel();
			PluginViewModel.OxyButVis = Visibility.Hidden;
			DataContext = PluginViewModel;
			InitializeComponent();
			Random rnd=new Random();
			x = new double[2][];
			y = new double[2][];
			ymax = 15;
			x[0] = new double[180];
			x[1] = new double[x[0].Length];
			y[0] = new double[x[0].Length];
			y[1] = new double[x[0].Length];
			for (int i = 0; i < x[0].Length;)
			{
				y[0][i] = ymax * rnd.NextDouble();
				x[0][i] = ++i;
			}
			ScatterPlot(0);
		}

		public Control(Plugin plugin) : this()
		{
			this.Plugin = plugin;
		}

		private void ScatterSeries_Click(object sender, RoutedEventArgs e)
		{
			ymax = 1 + Plugin.ymax[Plugin.which];
			ScatterPlot(Plugin.which);
			// force which refill
			Plugin.ymax[Plugin.which] = Plugin.xmax[Plugin.which] = Plugin.ymin[Plugin.which] = Plugin.xmin[Plugin.which];
			PluginViewModel.OxyButVis = Visibility.Hidden;
		}

		public double[][] x;
		public double[][] y;
		private double ymax;
		public string variables = "OxyPlot - Scatter Series";
		private PlotModel model;

        public void ScatterPlot(int which)
		{
			model = new PlotModel { Title = variables };
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = "Grip",
				Minimum = 0,
				Maximum = ymax
			});
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = "Slip",
				Minimum = 0,
				Maximum = 100
			});
			model = AddScatter(model, x[which], y[which], 3, OxyColors.Red);
			model.Series[model.Series.Count - 1].Title = "60Hz sample";
			model.LegendPosition = LegendPosition.TopRight;
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
