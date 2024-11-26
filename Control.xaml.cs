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
			Model.Plugin = Plugin = plugin;
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
			Model.which = choose;

			if (Model.Xrange > (Plugin.xmax[Model.which] - Plugin.xmin[Model.which]) && 1 > Model.Refresh)
				return;

			Model.Xrange = 0 < Model.Refresh ? 0 : Plugin.xmax[Model.which] - Plugin.xmin[Model.which];
			if (!Model.AutoPlot)
			{
				Model.PVis = Visibility.Visible;
				return;
			}

			Plot();
		}

        private void PBclick(object sender, RoutedEventArgs e)			// Plot button
		{
            Model.PVis = Visibility.Hidden;
			Plot();
        }

        private void RBclick(object sender, RoutedEventArgs e)			// Refresh button
		{
			Model.Refresh = (++Model.Refresh) % 3;
			ButtonUpdate();
		}

		private void APclick(object sender, RoutedEventArgs e)          // AutoPlot
		{
            Model.AutoPlot = !Model.AutoPlot;
            ButtonUpdate();
        }

        private void LFclick(object sender, RoutedEventArgs e)			// Line Fit button
		{
			Model.LinFit = !Model.LinFit;
			ButtonUpdate();	
		}

		void ButtonUpdate()
		{
			string[] refresh = new string[]
			{ 	"Hold max  X range",
				"3 second refresh",
				"Hold max R-squared"
			};

			TH.Text = refresh[Model.Refresh];
			LF.Text = "Fit Lines " + (Model.LinFit ? "enabled" : "disabled");
            TR.Text = (Model.AutoPlot ? "Auto" : "Manual") + " Replot";
        }
	}	// class
}
