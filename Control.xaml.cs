using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SimHub.Plugins;
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
		public int lowX, lowY;			// plot control lime sliders
		public double[] x, y;			// plot samples
		public int length;
		public int[] start;				// circular buffer
		private double xmax, ymax;		// somewhat arbitrary axis limits

		public Control()
		{
			DataContext = Model = new Model();
			Model.Vis = Visibility.Hidden;
			Model.XYprop = "Property plots require some X and Y values < 'X Below' and 'Y Below'";
			InitializeComponent();
			lowX = 50; lowY = 10;		// default minimum plotable interval range 
			ymax = 15;
			length = 360;
			start = new int[2]; start[0] = 0; start[1] = length >> 1;
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
			Model.Title = "launch a game or Replay to start property XY plots";
			TBL.Text = "X Below " + (SL.Value = lowX = plugin.Settings.Low) + "%";
			TBR.Text = "Y Below " + (SR.Value = lowY = plugin.Settings.Min) + "%";

			Model.Threshold = true;		// to restore Model.ThresVal
			TBT.Text = "X Threshold " + (ST.Value = Model.ThresVal = Plugin.Settings.ThresVal);
			Model.Threshold = Plugin.Settings.Threshold;
			Model.THvis = Model.Threshold ? Visibility.Visible : Visibility.Hidden;
			TH.Text = Model.Threshold ? "Disable Threshold" : "Enable Threshold";
			Model.LinFit = Plugin.Settings.LinFit;
			LF.Text = "Linear Fit " + (Model.LinFit ? "disable" : "enable");
			Xprop.Text = Yprop.Text = "random";
			
			plot.Model = ScatterPlot(0);
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
			ymax = 1.15 * Plugin.ymax[Plugin.which];	// legend space
			xmax = 1.1 * Plugin.xmax[Plugin.which];
			PlotModel model = ScatterPlot(Plugin.which);
			if (Model.LinFit)
			{
				double m, B;
				LinearBestFit(SubArray(x, start[Plugin.which], length >> 1),
							  SubArray(y, start[Plugin.which], length >> 1), out m, out B);
				model.Series.Add(BestFit(m, B));
			}
			plot.Model = model;
			// enable which refill
			Plugin.ymax[Plugin.which] = Plugin.xmax[Plugin.which] = 0;
			Plugin.running = true;		// enable Plugin updates
			Model.Vis = Visibility.Hidden;		// Refresh button
		}

		private void THclick(object sender, RoutedEventArgs e)	// Threshold button
		{
			Model.Threshold = !Model.Threshold;
			if (Model.Threshold)
			{
				Model.THvis = Visibility.Visible;
				TH.Text = "Disable Threshold";
				TBT.Text = "X Threshold " + Model.ThresVal;
			}
			else {
				Model.THvis = Visibility.Hidden;
				TH.Text = "Enable Threshold";
			}
		}

		private void LFclick(object sender, RoutedEventArgs e)	// Linear Fit button
		{
			Model.LinFit = !Model.LinFit;
			LF.Text = "Linear Fit " + (Model.LinFit ? "disable" : "enable");
		}

		public PlotModel ScatterPlot(int which)
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
				Maximum = xmax
			});

			model.Series.Add(Scatter(which));
			model.Series[model.Series.Count - 1].Title = "60Hz sample";
			model.LegendPosition = LegendPosition.TopRight;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;
			model.LegendBorderThickness = 1;
			return model;
		}

		private ScatterSeries Scatter(int which)
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
			return scatterSeries;
		}

		LineSeries BestFit(double m, double B)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(0, B));
			line.Points.Add(new DataPoint(Plugin.xmax[Plugin.which],
							 m * Plugin.xmax[Plugin.which]));
			return line;
		}

		// handle slider changes
		private void SLdone(object sender, MouseButtonEventArgs e)
		{
			TBL.Text = "X Below " + (lowX = (int)((Slider)sender).Value) + "%";
		}

		private void SRdone(object sender, MouseButtonEventArgs e)
		{
			TBR.Text = "Y Below " + (lowY = (int)((Slider)sender).Value) + "%";
		}

		private void STdone(object sender, MouseButtonEventArgs e)
		{
			TBT.Text = "X Threshold " + (Model.ThresVal = (int)((Slider)sender).Value);
		}
	}	// class
}
