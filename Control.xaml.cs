using System.Windows;			// Visibility
using System.Windows.Controls;

namespace blekenbleu.OxyScope
{

	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public Model M;
		static double Xmax, Ymax, Xmin, Ymin, xmin, xmax, ymax,	// axes limits
						m, B, inflection;
		static readonly double[] slope = new double[] { 0, 0, 0 };

		public Control()
		{
			DataContext = M = new Model();
			InitializeComponent();
			M.length = 360;				// 2 x 3 seconds of 60 Hz samples
			M.x = new double[M.length];
			M.y = new double[M.length];
			M.length /= 2; 
			M.start = new ushort[] { 0, M.length };
		}

		public Control(OxyScope plugin) : this()
		{
			Settings S;

			if (null != (S = plugin.Settings))
			{
				M.FilterX = S.FilterX;
				M.FilterY = S.FilterY;
				M.Refresh = S.Refresh;
				M.AutoPlot = S.Plot;
				M.LinFit = S.LinFit;
				M.Xprop = S.Xprop;
				M.Yprop = S.Yprop;
			} else {
				M.Refresh = 1;
				M.LinFit = M.AutoPlot = true;
				M.Xprop = M.Yprop = "random";
				M.FilterY = M.FilterX = 1;
			}

			ButtonUpdate();
			M.ymin = new double[] {0, 0}; M.ymax = new double[] {0, 0};
			M.xmin = new double[] {0, 0}; M.xmax = new double[] {0, 0};
			RandomPlot();
		}

		private void Hyperlink_RequestNavigate(object sender,
									   System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		internal void Replot(ushort choose)
		{
			M.which = choose;
			xmin = M.xmin[M.which];
			xmax = M.xmax[M.which];
			M.Range = 0 < M.Refresh ? 0 : xmax - xmin;
			if (2 == M.Refresh && null != M.Coef)		// cumulative Range?
			{
		 		if (Xmin < xmin && Xmax > xmax)
					return;

				if (Xmin > xmin)
					Xmin = xmin;
				if (Xmax < xmax)
					Xmax = xmax;
				if (Ymin > M.ymin[M.which])
					Ymin = M.ymin[M.which];
				if (Ymax < M.ymax[M.which])
					Ymax = M.ymax[M.which];
                M.Done = false;
            }
			else {
				Xmax = xmax;
				Xmin = xmin;
				Ymax = M.ymax[M.which];
				Ymin = M.ymin[M.which];
			}

			if (M.AutoPlot)
				Plot();
			else M.PVis = Visibility.Visible;
		}

		private void PBclick(object sender, RoutedEventArgs e)			// Plot button
		{
			M.PVis = Visibility.Hidden;
			Plot();
		}

		private void RBclick(object sender, RoutedEventArgs e)			// Refresh button
		{
			M.Refresh = (ushort)((++M.Refresh) % 3);
			ButtonUpdate();
		}

		private void APclick(object sender, RoutedEventArgs e)        // AutoPlot
		{
			M.AutoPlot = !M.AutoPlot;
			M.Done = true;
			if (M.AutoPlot && Visibility.Visible == M.PVis)
			{
				M.PVis = Visibility.Hidden;
				Plot();
			}
			ButtonUpdate();
		}

		private void LFclick(object sender, RoutedEventArgs e)			// Line Fit button
		{
			M.LinFit = !M.LinFit;
			ButtonUpdate();	
		}

		readonly string[] refresh = new string[]
		{ 	"Hold max  X range",
			"3 second refresh",
			"Cumulative X range"
		};
		void ButtonUpdate()
		{
			TH.Text = refresh[M.Refresh];
			LF.Text = "Fit Curves " + (M.LinFit ? "enabled" : "disabled");
			TR.Text = (M.AutoPlot ? "Auto" : "Manual") + " Replot";
		}
	}	// class
}
