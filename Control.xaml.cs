using OxyPlot;
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
		public int length, ln2;
		public int[] start;				// circular buffer
		public double[] x, y;			// plot samples
		int which = 0;					// which samples to plot
		double xmax, ymax;				// somewhat arbitrary axis limits
		double[] c;						// least squares fit coefficient[s]
		bool running = true;			// don't replot when already busy

		public Control()
		{
			DataContext = Model = new Model();
			InitializeComponent();
			length = 360;				// 2 x 3 seconds of 60 Hz samples
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
				Model.Refresh = Plugin.Settings.Refresh;
				Model.Plot = Plugin.Settings.Plot;
				Model.LinFit = Plugin.Settings.LinFit;
				Model.Xprop = plugin.Settings.Xprop;
				Model.Yprop = plugin.Settings.Yprop;
			} else {
				Model.Xprop = Model.Yprop = "random";
				Model.FilterY = Model.FilterX = 1;
			}

			Bupdate();
			Init();								// random plot
		}

		private void Hyperlink_RequestNavigate(object sender,
									   System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		internal void Replot(int choose, bool force)
		{
			which = choose;
			if (!running)
				return;
			if (Model.Xrange > Plugin.xmax[which] - Plugin.xmin[which] && !Model.Refresh)
				return;

			if (!Model.Plot && !force)
			{
				Model.RVis = Visibility.Visible;
				return;
			}

			running = false;		// disable OxyScope updates
			ymax = 1.2 * (Plugin.ymax[which] - Plugin.ymin[which])
				 + Plugin.ymin[which];	// legend space
			xmax = Plugin.xmax[which];
			Model.Xrange = Model.Refresh ? 0 : xmax - Plugin.xmin[which];

			PlotModel model = ScatterPlot(which, "60 Hz samples");
			Model.XYprop = $"{Plugin.xmin[which]:#0.000} <= X <= "
					     + $"{xmax:#0.000};  "
					     + $"{Plugin.ymin[which]:#0.000} <= Y <= "
					     + $"{Plugin.ymax[which]:#0.000}";

			if (Model.LinFit)
			{
				// https://numerics.mathdotnet.com/Regression
				double[] xs = SubArray(x, start[which], ln2),
						 ys = SubArray(y, start[which], ln2);
				(double, double)p = Fit.Line(xs, ys);
				Model.B = p.Item1;
				Model.m = p.Item2;
				model.Series.Add(BestFit(Plugin.xmin[which], Model.m, Model.B, "Fit.Line"));
				Model.XYprop += $";  intercept = {Model.B:#0.0000}, slope = {Model.m:#0.0000}";

				// 	LinearBestFit(out Model.m, out Model.B);
				//	model.Series.Add(BestFit(Model.m, Model.B, "LinearBestFit"));

				if (0 >= Plugin.ymin[which] && 0 <= Plugin.ymax[which])
				{
					c = Fit.LinearCombination(xs, ys, x => x);
					model.Series.Add(BestFit(0, c[0], 0, "Fit thru origin"));
					Model.XYprop += $", origin slope = {c[0]:#0.0000}"; 
				}

			}
			plot.Model = model;											// OxyPlot
			Plugin.ymax[which] = Plugin.xmax[which]		// free this buffer for reuse
									  = Plugin.xmin[which] = 0;
			Model.RVis = Visibility.Hidden;								// Plot button
			running = true;										// enable OxyScope updates
		}

		private void PBclick(object sender, RoutedEventArgs e)			// Plot button
		{
			Replot(which, true);
		}

		private void RBclick(object sender, RoutedEventArgs e)			// Refresh button
		{
			Model.Refresh = !Model.Refresh;
			Bupdate();
		}

		private void APclick(object sender, RoutedEventArgs e)			// Auto Plot
		{
			Model.Plot = !Model.Plot;
			Bupdate();
		}

		private void LFclick(object sender, RoutedEventArgs e)			// Linear Fit button
		{
			Model.LinFit = !Model.LinFit;
			Bupdate();	
		}

		void Bupdate()
		{
			LF.Text = "Linear Fit " + (Model.LinFit ? "enabled" : "disabled");
			TH.Text = Model.Refresh ? "3 second refresh" : "Hold max  X range";
			if (Model.Plot)
			{
				TR.Text = "Auto";
				Model.RVis = Visibility.Hidden;
			}
			else TR.Text = "Manual";
			TR.Text += " Plot";
		}
	}	// class
}
