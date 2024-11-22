using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MathNet.Numerics;

namespace blekenbleu.OxyScope
{

	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public OxyScope Plugin { get; }
		public Model Model;
		public int lowX, lowY;			// plot control lime sliders
		public double[] x, y;			// plot samples
		public int length, ln2;
		public int[] start;				// circular buffer
		private double xmax, ymax;		// somewhat arbitrary axis limits
		double[] c;

		public Control()
		{
			DataContext = Model = new Model();
			Model.RVis = Visibility.Hidden;
			Model.XYprop = "Property plots require some X and Y values < 'X Below' and 'Y Below'";
			InitializeComponent();
			lowX = 50; lowY = 10;		// default minimum plotable interval range 
			ymax = 15;
			length = 360;
			ln2 = length >> 1; 
			start = new int[] { 0, ln2 };
			x = new double[length];
			y = new double[length];
			Random rnd = new Random();
			for (int i = 0; i < ln2 ;)	// fill the plot
			{
				y[i] = ymax * rnd.NextDouble();
				x[i] = ++i;
			}
		}

		public Control(OxyScope plugin) : this()
		{
			Plugin = plugin;
			Model.Title = "launch a game or Replay to start property XY plots";
			TBL.Text = "X Below " + (SL.Value = lowX = plugin.Settings.Low) + "%";
			TBR.Text = "Y Below " + (SR.Value = lowY = plugin.Settings.Min) + "%";

			Model.ThresBool = true;		// to restore Model.ThresVal
			TBT.Text = "X Threshold " + (ST.Value = Model.ThresVal = Plugin.Settings.ThresVal);
			Model.ThresBool = Plugin.Settings.ThresBool;
			Model.TVis = Model.ThresBool ? Visibility.Visible : Visibility.Hidden;
			TH.Text = Model.ThresBool ? "Disable Threshold" : "Enable Threshold";
			Model.LinFit = Plugin.Settings.LinFit;
			LF.Text = "Linear Fit " + (Model.LinFit ? "enabled" : "disabled");
			Model.Xprop = Model.Yprop = "random";
			Model.FilterX = Plugin.Settings.FilterX;
			Model.FilterY = Plugin.Settings.FilterY;

			plot.Model = ScatterPlot(0, null);
			if (null != plugin.Settings)
			{
				Model.Xprop = plugin.Settings.X;
				Model.Yprop = plugin.Settings.Y;
			}
		}

        private void Hyperlink_RequestNavigate(object sender,
                                       System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        internal void Replot()
		{
			Plugin.running = false;		// disable OxyScope updates
			ymax = 1.2 * Plugin.ymax[Plugin.which];	// legend space
			xmax = 1.1 * Plugin.xmax[Plugin.which];

			// ScatterPlot() also collects samples for LinearBestFit()
			PlotModel model = ScatterPlot(Plugin.which, "60 Hz samples");
			if (Model.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				double[] xs = SubArray(x, start[Plugin.which], ln2),
						 ys = SubArray(y, start[Plugin.which], ln2);
				(double, double)p = Fit.Line(xs, ys);
				Model.B = p.Item1;
				Model.m = p.Item2;
				model.Series.Add(BestFit(Model.m, Model.B, "Fit.Line"));

				c = Fit.LinearCombination(xs, ys, x => x);
				model.Series.Add(BestFit(c[0], 0, "Fit thru origin"));

			// 	LinearBestFit(out Model.m, out Model.B);
			//	model.Series.Add(BestFit(Model.m, Model.B, "LinearBestFit"));
				Model.XYprop += $";  intercept = {Model.B:#0.0000}, "
					+ $"slope = {Model.m:#0.0000}, origin slope = {c[0]:#0.0000}"; 
			}
			plot.Model = model;					// OxyPlot
			Plugin.ymax[Plugin.which] = Plugin.xmax[Plugin.which] = 0;
			Plugin.running = true;				// enable OxyScope updates
			Model.RVis = Visibility.Hidden;		// Replot button
		}

		private void SSclick(object sender, RoutedEventArgs e)	// Replot button
		{
			Replot();
		}

		private void THclick(object sender, RoutedEventArgs e)	// Threshold button
		{
			Model.ThresBool = !Model.ThresBool;
			if (Model.ThresBool)
			{
				Model.TVis = Visibility.Visible;
				TH.Text = "Disable Threshold";
				TBT.Text = "X Threshold " + Model.ThresVal;
			}
			else {
				Model.TVis = Visibility.Hidden;
				TH.Text = "Enable Threshold";
			}
		}

		private void ARclick(object sender, RoutedEventArgs e)	// Linear Fit button
		{
			Model.Replot = !Model.Replot;
			TR.Text = (Model.Replot ? "Auto" : "Manual") + " Replot";
		}

		private void LFclick(object sender, RoutedEventArgs e)	// Linear Fit button
		{
			Model.LinFit = !Model.LinFit;
			LF.Text = "Linear Fit " + (Model.LinFit ? "enabled" : "disabled");
		}

		PlotModel ScatterPlot(int which, string title)
		{
			PlotModel model = new PlotModel { Title = Model.Title };

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = Model.Yprop,
				Minimum = 0,
				Maximum = ymax
			});

			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = Model.Xprop,
				Minimum = 0,
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
			int size = 3;	// plot dot size
			int end = (start[which] <= ln2) ? start[which] + ln2 : length;

//			samples = new List<XYvalue> ();
			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (int i = start[which]; i < end; i++)
			{
//				samples.Add(new XYvalue { X = x[i], Y = y[i] });
				scatterSeries.Points.Add(new ScatterPoint(x[i], y[i], size));
			}
			scatterSeries.MarkerFill = OxyColors.Red;
			if (null != title)
				scatterSeries.Title = title;
			return scatterSeries;
		}

		LineSeries BestFit(double m, double B, string title)
		{
			LineSeries line = new LineSeries();
			line.Points.Add(new DataPoint(0, B));
			line.Points.Add(new DataPoint(Plugin.xmax[Plugin.which],
							 m * Plugin.xmax[Plugin.which]));
			line.Title = title;
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
