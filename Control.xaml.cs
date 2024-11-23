using OxyPlot;
using System;
using System.Windows;
using System.Windows.Controls;
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
		public double[] x, y;			// plot samples
		public int length, ln2;
		public int[] start;				// circular buffer
		private double xmax, ymax;		// somewhat arbitrary axis limits
		double[] c;

		public Control()
		{
			DataContext = Model = new Model();
			InitializeComponent();
			length = 360;
			ln2 = length >> 1; 
			start = new int[] { 0, ln2 };
			x = new double[length];
			y = new double[length];
		}

		public Control(OxyScope plugin) : this()
		{
			Plugin = plugin;
			if (null != Plugin.Settings)
			{
				Model.FilterX = Plugin.Settings.FilterX;
				Model.FilterY = Plugin.Settings.FilterY;
				Model.Refresh = !Plugin.Settings.Refresh;
				Model.Replot = !Plugin.Settings.Replot;
				Model.LinFit = !Plugin.Settings.LinFit;
				Model.Refresh = Plugin.Settings.Refresh;
				Model.Replot = Plugin.Settings.Replot;
				Model.LinFit = Plugin.Settings.LinFit;
				Model.Xprop = plugin.Settings.Xprop;
				Model.Yprop = plugin.Settings.Yprop;
			} else Model.Xprop = Model.Yprop = "random";

			LF.Text = "Linear Fit " + (Model.LinFit ? "enabled" : "disabled");
			// fill the plot with random data
			Random rnd = new Random();
			double xp, xi, yp, ymin  = Plugin.ymin[Plugin.which];
			ymax = Plugin.ymax[Plugin.which];
			xmax = Plugin.xmax[Plugin.which];
			yp = ymax - ymin;
			xp = Plugin.xmin[Plugin.which];
			xi = (xmax - xp) / ln2;
			for (int i = 0; i < ln2; i++)	// fill the plot
			{
				y[i] = ymin + yp * rnd.NextDouble();
				x[i] = xp;
				xp += xi;
			}

			plot.Model = ScatterPlot(0, null);
			Plugin.running = true;				// enable OxyScope updates
		}

        private void Hyperlink_RequestNavigate(object sender,
                                       System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        internal void Replot()
		{
			if (Model.Xrange > Plugin.xmax[Plugin.work] - Plugin.xmin[Plugin.work] && !Model.Refresh)
				return;

			if (!Model.Replot)
			{
				Model.RVis = Visibility.Visible;
				return;
			}

			Plugin.running = false;		// disable OxyScope updates
			ymax = 1.2 * (Plugin.ymax[Plugin.which] - Plugin.ymin[Plugin.which])
				 + Plugin.ymin[Plugin.which];	// legend space
			xmax = Plugin.xmax[Plugin.which];
			Model.Xrange = Model.Refresh ? 0 : xmax - Plugin.xmin[Plugin.which];

			PlotModel model = ScatterPlot(Plugin.which, "60 Hz samples");
			Model.XYprop = Model.Current;
			if (Model.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				double[] xs = SubArray(x, start[Plugin.which], ln2),
						 ys = SubArray(y, start[Plugin.which], ln2);
				(double, double)p = Fit.Line(xs, ys);
				Model.B = p.Item1;
				Model.m = p.Item2;
				model.Series.Add(BestFit(Plugin.xmin[Plugin.which], Model.m, Model.B, "Fit.Line"));
				Model.XYprop += $";  intercept = {Model.B:#0.0000}, slope = {Model.m:#0.0000}";

				// 	LinearBestFit(out Model.m, out Model.B);
				//	model.Series.Add(BestFit(Model.m, Model.B, "LinearBestFit"));

				if (0 >= Plugin.ymin[Plugin.which] && 0 <= Plugin.ymax[Plugin.which])
				{
					c = Fit.LinearCombination(xs, ys, x => x);
					model.Series.Add(BestFit(0, c[0], 0, "Fit thru origin"));
                    Model.XYprop += $", origin slope = {c[0]:#0.0000}"; 
				}

			}
			plot.Model = model;					// OxyPlot
			Plugin.ymax[Plugin.which] = Plugin.xmax[Plugin.which]		// free this buffer for reuse
									  = Plugin.xmin[Plugin.which] = 0;
			Model.RVis = Visibility.Hidden;								// Replot button
			Plugin.running = true;										// enable OxyScope updates
		}

		private void SSclick(object sender, RoutedEventArgs e)	// Replot button
		{
			Model.Replot = true;
			Replot();
			Model.Replot = false;
			Model.RVis = Visibility.Hidden;
		}

		private void THclick(object sender, RoutedEventArgs e)	// Refresh button
		{
			Model.Refresh = !Model.Refresh;
			if (Model.Refresh)
			{
				TH.Text = "3 second refresh";
				Model.Xrange = 0;
				Model.RVis = Visibility.Hidden;		// Replot button
			} else TH.Text = "Hold max  X range";
		}

		private void ARclick(object sender, RoutedEventArgs e)	// Auto Replot
		{
			Model.Replot = !Model.Replot;
			if (Model.Replot)
			{
				TR.Text = "Auto";
				Model.RVis = Visibility.Hidden;
			} else TR.Text = "Manual";
			TR.Text += " Replot";
		}

		private void LFclick(object sender, RoutedEventArgs e)	// Linear Fit button
		{
			Model.LinFit = !Model.LinFit;
			LF.Text = "Linear Fit " + (Model.LinFit ? "enabled" : "disabled");
		}
	}	// class
}
