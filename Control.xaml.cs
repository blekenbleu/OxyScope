using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OxyPlotPlugin
{

	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public Plugin Plugin { get; }
		public Model Model;
		public int lowval, minval;	// plot control lime sliders
		public double[] x;			// plot samples
		public double[] y;
		public int length;
		public int[] start;			// circular buffer  
		private double ymax;		// somewhat arbitrary Y axis limit

		public Control()
		{
			Model = new Model();
			Model.Vis = Visibility.Hidden;
			Model.XYprop = "Plots require some values:  Below > values and other values:  Above < values";
			DataContext = Model;
			InitializeComponent();
			lowval = 50; minval = 10;	// default minimum plotable interval range 
			ymax = 15;
			length = 360;
			start = new int[2]; start[0] = 0; start[1] =  length >> 1;
			x = new double[length];
			y = new double[length];
			int l = length >> 1;
			Random rnd=new Random();
			for (int i = 0; i < l ;)	// something to fill the plot
			{
				y[i] = ymax * rnd.NextDouble();
				x[i] = ++i;
			}
		}

		public Control(Plugin plugin) : this()
		{
			this.Plugin = plugin;
			Model.Title = "launch a game or Replay to enable Y vs X property plots";
			TBL.Text = "Below " + (SL.Value = lowval = plugin.Settings.Low);
			TBR.Text = "Above " + (SR.Value = minval = plugin.Settings.Min);
			Xprop.Text = Yprop.Text = "random";
			ScatterPlot(0);
			if (null != plugin.Settings)
			{
				Xprop.Text = plugin.Settings.X;
				Yprop.Text = plugin.Settings.Y;
			}
		}

		private void SSclick(object sender, RoutedEventArgs e)	// Refresh button
		{
			if (0 < Yprop.Text.Length)
				Plugin.Settings.Y = Yprop.Text;
			else Yprop.Text = Plugin.Settings.Y;
			if (0 < Xprop.Text.Length)
				Plugin.Settings.X = Xprop.Text;
			else Xprop.Text = Plugin.Settings.X;

			Plugin.running = false;		// disable Plugin updates
			Model.XYprop = "property updates paused...";
			ymax = 1 + Plugin.ymax[Plugin.which];
			ScatterPlot(Plugin.which);
			// enable which refill
			Plugin.ymax[Plugin.which] = Plugin.xmax[Plugin.which] = 1 + lowval;
			Plugin.ymax[Plugin.which] = 0;
			Plugin.running = true;		// enable Plugin updates
			Model.XYprop = "property updates waiting...";
			Model.Vis = Visibility.Hidden;
		}

		public void ScatterPlot(int which)
		{
			PlotModel model = new PlotModel { Title = Model.Title };

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = Yprop.Text,
				Minimum = 0,
				Maximum = ymax
			});

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = Xprop.Text,
				Minimum = 0,
				Maximum = 100
			});

			model = AddScatter(model, which);
			model.Series[model.Series.Count - 1].Title = "60Hz sample";
			model.LegendPosition = LegendPosition.TopRight;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;
			model.LegendBorderThickness = 1;
			plot.Model = model;
		}

		private PlotModel AddScatter(PlotModel model, int which)
		{
			int size = 3;	// plot dot size
			int end = (start[which] <= length >> 1) ? start[which] + length >> 1 : length;

			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (int i = start[which]; i < end; i++)
				scatterSeries.Points.Add(new ScatterPoint(x[i], y[i], size));
			if (start[which] > length >> 1)
			{
				end = start[which] - length >> 1;	// wrap "circular" buffer
				for (int i = 0; i < end; i++)
					scatterSeries.Points.Add(new ScatterPoint(x[i], y[i], size));
			}
			scatterSeries.MarkerFill = OxyColors.Red;
			model.Series.Add(scatterSeries);
			return model;
		}

		// handle slider changes
		private void SLdone(object sender, MouseButtonEventArgs e)
		{
			TBL.Text = "Below " + (lowval = (int)((Slider)sender).Value);
		}

		private void SRdone(object sender, MouseButtonEventArgs e)
		{
			TBR.Text = "Above " + (minval = (int)((Slider)sender).Value);
		}
	}	// class
}
