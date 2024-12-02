using System.Windows;			// Visibility
using System.Windows.Controls;

namespace blekenbleu.OxyScope
{

	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public Model Model;
		static double Xmax, Ymax, Xmin, Ymin, xmin, xmax, ymax,	// axes limits
						m, B, inflection;
		static readonly double[] slope = new double[] { 0, 0, 0 };

		public Control()
		{
			DataContext = Model = new Model();
			InitializeComponent();
			Model.length = 360;				// 2 x 3 seconds of 60 Hz samples
			Model.x = new double[Model.length];
			Model.y = new double[Model.length];
			Model.length /= 2; 
			Model.start = new ushort[] { 0, Model.length };
		}

		public Control(OxyScope plugin) : this()
		{
			Settings S;

			if (null != (S = plugin.Settings))
			{
				Model.FilterX = S.FilterX;
				Model.FilterY = S.FilterY;
				Model.Refresh = S.Refresh;
				Model.AutoPlot = S.Plot;
				Model.LinFit = S.LinFit;
				Model.Xprop = S.Xprop;
				Model.Yprop = S.Yprop;
			} else {
				Model.Refresh = 1;
				Model.LinFit = Model.AutoPlot = true;
				Model.Xprop = Model.Yprop = "random";
				Model.FilterY = Model.FilterX = 1;
			}

			ButtonUpdate();
			Model.ymin = new double[] {0, 0}; Model.ymax = new double[] {0, 0};
			Model.xmin = new double[] {0, 0}; Model.xmax = new double[] {0, 0};
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
			Model.Done = false;
			xmin = Model.xmin[Model.which];
			xmax = Model.xmax[Model.which];
			Model.Range = 0 < Model.Refresh ? 0 : xmax - xmin;
			if (2 == Model.Refresh && null != Model.Coef)		// cumulative Range?
			{
		 		if (Xmin < xmin && Xmax > xmax)
					return;

				if (Xmin > xmin)
					Xmin = xmin;
				if (Xmax < xmax)
					Xmax = xmax;
				if (Ymin > Model.ymin[Model.which])
					Ymin = Model.ymin[Model.which];
				if (Ymax < Model.ymax[Model.which])
					Ymax = Model.ymax[Model.which];
			}
			else {
				Xmax = xmax;
				Xmin = xmin;
				Ymax = Model.ymax[Model.which];
				Ymin = Model.ymin[Model.which];
			}

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
			"Cumulative X range"
		};
		void ButtonUpdate()
		{
			TH.Text = refresh[Model.Refresh];
			LF.Text = "Fit Curves " + (Model.LinFit ? "enabled" : "disabled");
			TR.Text = (Model.AutoPlot ? "Auto" : "Manual") + " Replot";
		}
	}	// class
}
