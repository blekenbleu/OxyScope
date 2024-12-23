using System.Drawing;
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
		public OxyScope O;
		static double Xmax, Ymax, Xmin, Ymin, xmin, xmax, ymax,	// axes limits
						m, B, inflection;
		static readonly double[] slope = new double[] { 0, 0, 0 };

		public Control()
		{
			DataContext = M = new Model();
			InitializeComponent();
		}

		public Control(OxyScope plugin) : this()
		{
			Settings S;

			O = plugin;
			if (null != (S = plugin.Settings))
			{
				M.FilterX = S.FilterX;
				M.FilterY = S.FilterY;
				M.Refresh = S.Refresh;
				M.AutoPlot = S.Plot;
				M.LinFit = S.LinFit;
				M.Xprop0 = S.Xprop;
				M.Xprop1 = S.Xprop1;
				M.Xprop2 = S.Xprop2;
				M.Yprop = S.Yprop;
			} else {
				M.Refresh = M.LinFit = 1;
				M.AutoPlot = true;
				M.Xprop0 = M.Yprop = "random";
				M.FilterY = M.FilterX = 1;
			}
			O.x = new double[4, M.length = 901];
			M.length /= 5; 
			M.start = new ushort[] { 0, M.length };

			ButtonUpdate();
			M.min = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, {0, 0} };
			M.max = new double[,] {{0, 0}, {0, 0}, {0, 0}, {0, 0} };
			RandomPlot();
		}

		private void Hyperlink_RequestNavigate(object sender,
									System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		internal void Replot(ushort choose)
		{
			int m = (0 < M.LinFit) ? (M.LinFit - 1) : 0;
			M.which = choose;
			xmin = M.min[m,M.which];	// for curve fit
			xmax = M.max[m,M.which];
			M.Range = 0 < M.Refresh ? 0 : xmax - xmin;

			double Nmax = M.max[0,M.which];    // plot range for up to 3 Xprops
			double Nmin = M.min[0,M.which];
			if (M.a[1])
			{
				if (Nmin > M.min[1,M.which])
					Nmin = M.min[1,M.which];
				if (Nmax < M.max[1,M.which])
					Nmax = M.max[1,M.which];
			}
			if (M.a[2])
			{
				if (Nmin > M.min[2,M.which])
					Nmin = M.min[2,M.which];
				if (Nmax < M.max[2,M.which])
					Nmax = M.max[2,M.which];
			}
			if (1 == M.Refresh || M.Reset)		// first time or 3 second
			{
				Xmax = Nmax;
				Xmin = Nmin;
				Ymax = M.max[3,M.which];
				Ymin = M.min[3,M.which];
			} else {
		 		if (0 == M.Refresh && Xmin < Nmin && Xmax > Nmax)
					return;

				if (Xmin > Nmin)
					Xmin = Nmin;
				if (Xmax < Nmax)
					Xmax = Nmax;
				if (Ymin > M.min[3,M.which])
					Ymin = M.min[3,M.which];
				if (Ymax < M.max[3,M.which])
					Ymax = M.max[3,M.which];
			}
			M.Reset = false;
			if (M.AutoPlot)
				Plot();
			else M.PVis = Visibility.Visible;
		}

		private void PBclick(object sender, RoutedEventArgs e)		// Plot button
		{
			M.PVis = Visibility.Hidden;
			Plot();
		}

		private void RBclick(object sender, RoutedEventArgs e)		// Refresh button
		{
			M.Refresh = (ushort)((++M.Refresh) % 3);
			M.Reset = true;
			ButtonUpdate();
		}

		private void APclick(object sender, RoutedEventArgs e)		// AutoPlot
		{
			M.AutoPlot = !M.AutoPlot;
			if (M.AutoPlot && Visibility.Visible == M.PVis)
			{
				M.PVis = Visibility.Hidden;
				Plot();
			}
			ButtonUpdate();
		}

		private void LFclick(object sender, RoutedEventArgs e)		// Line Fit button
		{
			if (0 == M.LinFit)
				M.LinFit++;
			else if (3 == M.LinFit)
				M.LinFit = 0;
			else if (1 == M.LinFit)
				M.LinFit = (ushort)(M.a[1] ? 2 : M.a[2] ? 3 : 0);
			else M.LinFit = (ushort)(M.a[2] ? 3 : 0);
			M.Reset = true;
			ButtonUpdate();	
		}

		readonly string[] refresh = new string[]
		{ 	"Hold max  X range",
			"3 second refresh",
			"Cumulative X range"
		};

		void ButtonUpdate()
		{
            System.Windows.Media.Brush[] color =
			{ System.Windows.Media.Brushes.White,
			  System.Windows.Media.Brushes.Red,
			  System.Windows.Media.Brushes.Green,
			  System.Windows.Media.Brushes.Cyan
			};
			TH.Text = refresh[M.Refresh];
			LF.Foreground = color[M.LinFit];	// 0: white

            LF.Text = "Fit Curves " + ((0 < M.LinFit) ? ("Xprop"+(M.LinFit - 1)) : "disabled");
			TR.Text = (M.AutoPlot ? "Auto" : "Manual") + " Replot";
		}
	}	// class
}
