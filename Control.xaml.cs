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
		public ushort length, ln2;
		public ushort[] start;						// circular buffer
		public double[] x, y;						// plot samples
		static double xmax, ymax, xmin, ymin,		// axis limits
				 m, B, inflection;
		static readonly double[] slope = new double[] { 0, 0, 0 };

		public Control()
		{
			DataContext = Model = new Model();
			InitializeComponent();
			length = 360;				// 2 x 3 seconds of 60 Hz samples
			ln2 = (ushort)(length >> 1); 
			start = new ushort[] { 0, ln2 };
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

		internal void Replot(ushort choose)
		{
			Model.which = choose;
			Model.Xrange = 0 < Model.Refresh ? 0 : Plugin.xmax[Model.which] - Plugin.xmin[Model.which];
			if (Model.AutoPlot)
				Plot();
			else Model.PVis = Visibility.Visible;
		}

		private void PBclick(object sender, RoutedEventArgs e)			// Plot button
		{
			Model.PVis = Visibility.Hidden;
			Plot();
		}

		private void RBclick(object sender, RoutedEventArgs e)			// Refresh button
		{
			Model.Refresh = (ushort)((++Model.Refresh) % 3);
			ButtonUpdate();
		}

		private void APclick(object sender, RoutedEventArgs e)		  // AutoPlot
		{
			Model.AutoPlot = !Model.AutoPlot;
			if (Model.AutoPlot && Visibility.Visible == Model.PVis)
			{
				Model.PVis = Visibility.Hidden;
				Plot();
			}
			ButtonUpdate();
		}

		private void LFclick(object sender, RoutedEventArgs e)			// Line Fit button
		{
			Model.LinFit = !Model.LinFit;
			ButtonUpdate();	
		}

		readonly string[] refresh = new string[]
		{ 	"Hold max  X range",
			"3 second refresh",
			"Hold max R-squared"
		};
		void ButtonUpdate()
		{
			TH.Text = refresh[Model.Refresh];
			LF.Text = "Fit Lines " + (Model.LinFit ? "enabled" : "disabled");
			TR.Text = (Model.AutoPlot ? "Auto" : "Manual") + " Replot";
		}
	}	// class
}
