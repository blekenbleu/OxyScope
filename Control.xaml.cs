using System.Windows;			// Visibility
using System.Windows.Controls;

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
				Model.AutoPlot = Plugin.Settings.Plot;
				Model.LinFit = Plugin.Settings.LinFit;
				Model.Xprop = plugin.Settings.Xprop;
				Model.Yprop = plugin.Settings.Yprop;
			} else {
				Model.Xprop = Model.Yprop = "random";
				Model.FilterY = Model.FilterX = 1;
			}

			ButtonUpdate();
			RandomPlot();
		}

		private void Hyperlink_RequestNavigate(object sender,
									   System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		internal void Replot(int choose)
		{
			which = choose;

			if (Model.Xrange > Plugin.xmax[which] - Plugin.xmin[which] && !Model.Refresh)
				return;

			if (!Model.AutoPlot)
			{
				Model.PVis = Visibility.Visible;
				return;
			}

			Plot();
		}

        private void PBclick(object sender, RoutedEventArgs e)			// Plot button
		{
			Plot();
            Model.PVis = Visibility.Hidden;
        }

        private void RBclick(object sender, RoutedEventArgs e)			// Refresh button
		{
			Model.Refresh = !Model.Refresh;
			if (Model.Refresh)
				Plugin.xmax[which] = Plugin.xmin[which] = 0;
			ButtonUpdate();
		}

		private void APclick(object sender, RoutedEventArgs e)          // AutoPlot
		{
            Model.AutoPlot = !Model.AutoPlot;
            if (Model.AutoPlot)
                Plugin.xmax[which] = Plugin.xmin[which] = 0;
            ButtonUpdate();
        }

        private void LFclick(object sender, RoutedEventArgs e)			// Linear Fit button
		{
			Model.LinFit = !Model.LinFit;
			ButtonUpdate();	
		}

		void ButtonUpdate()
		{
			LF.Text = "Linear Fit " + (Model.LinFit ? "enabled" : "disabled");
			TH.Text = Model.Refresh ? "3 second refresh" : "Hold max  X range";
        
            if (Model.AutoPlot)
			{
				TR.Text = "Auto";
				Model.PVis = Visibility.Hidden;
			}
			else TR.Text = "Manual";
            TR.Text += " Replot";
        }
	}	// class
}
