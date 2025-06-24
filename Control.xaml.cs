using System.Windows;			// Visibility
using System.Windows.Controls;
using System.Windows.Navigation;

namespace blekenbleu.OxyScope
{

	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public static Model M;
		public OxyScope O;
		static double Xmax, Ymax, Xmin, Ymin, ymax,	// axes limits
						m, B, inflection;
		static readonly double[] slope = new double[] { 0, 0, 0 }; // LeastSquares inflections
		static int p;								// current X property to plot

		public Control() => InitializeComponent();

		public Control(OxyScope plugin) : this()
		{
			DataContext = M = new Model(O = plugin);
			O.x = new double[4, 1 + 5 * M.length];
			M.start = new ushort[] { 0, M.length };

			ButtonUpdate();
			M.min = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
			M.max = new double[,] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
			RandomPlot();
		}

		private void Hyperlink_RequestNavigate(object sender,
									RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		internal void Replot(ushort w)
		{
			if (2 != M.Refresh && !M.AutoPlot)		// neither autoplot nor Accrue?
				M.PVis = Visibility.Visible;		// no more updates manual plot
			double Nmax = M.max[0,M.which = w];		// plot range for up to 3 Yprops
			double Nmin = M.min[0,M.which];
			for (int i = 1; i < 3; i++)
				if (M.axis[i])
				{
					if (Nmin > M.min[i,M.which])
						Nmin = M.min[i,M.which];
					if (Nmax < M.max[i,M.which])
						Nmax = M.max[i,M.which];
				}

//			if (1 == M.Refresh || M.Reset)		// first time or 3 second
//			{
				Ymax = Nmax;
				Ymin = Nmin;
				Xmax = M.max[3,M.which];
				Xmin = M.min[3,M.which];
/*			} else {							// remember max/min from previous plots
				if (Ymin > Nmin)
					Ymin = Nmin;
				if (Ymax < Nmax)
					Ymax = Nmax;
				if (Xmin > M.min[3,M.which])
					Xmin = M.min[3,M.which];
				if (Xmax < M.max[3,M.which])
					Xmax = M.max[3,M.which];
			}	*/
			M.Reset = false;
			Plot();
		}

		private void D3click(object sender, RoutedEventArgs e)		// 3D visualize button
		{
			D3();
			M.D3vis = Visibility.Hidden;
		}

		private void PBclick(object sender, RoutedEventArgs e)		// Plot button (for manual Refresh)
		{
			M.PVis = Visibility.Hidden;
			Plot();
		}

		private void RBclick(object sender, RoutedEventArgs e)		// Refresh button
		{
			// 0 = max range, 1 = 3 second, 2 = Accrue
			// Accrue() now always maximizes StdDev for all Yprops
			if (2 == M.Refresh)
				M.Reset = true;
			M.Refresh = (ushort)((++M.Refresh) % 3);
			if (2 == M.Refresh)
			{
				M.Reset = true;
				M.PVis = Visibility.Hidden;
			}
			else if (!M.AutoPlot)
				M.PVis = Visibility.Visible;
			ButtonUpdate();
		}

		// inaccessible when 2 == M.Refresh
		private void APclick(object sender, RoutedEventArgs e)		// AutoPlot
		{
			M.AutoPlot = !M.AutoPlot;
			if (M.AutoPlot)
			{
				if (Visibility.Visible == M.PVis)
				{
					Plot();
					M.PVis = Visibility.Hidden;
				}
			} else M.PVis = Visibility.Visible;
			ButtonUpdate();
		}

		private void LFclick(object sender, RoutedEventArgs e)		// Line Fit button
		{
			if (3 == M.LinFit)
				M.LinFit = 0;
			else if (0 == M.LinFit)
				M.LinFit = (ushort)(M.axis[1] ? 1 : M.axis[2] ? 2 : 3);
			else M.LinFit = (ushort)(M.axis[2] ? 2 : 3);
			ButtonUpdate();
			Plot();
		}

		internal void ButtonUpdate()
		{
			string[] refresh = new string[]
			{ 	"Hold max range",
				"3 second refresh",
				"Cumulative range"
			};

			System.Windows.Media.Brush[] color =
			{ System.Windows.Media.Brushes.Red,
			  System.Windows.Media.Brushes.Green,
			  System.Windows.Media.Brushes.Cyan,
			  System.Windows.Media.Brushes.White
			};

			BTR.Visibility = (2 == M.Refresh) ? Visibility.Hidden : Visibility.Visible;
			TH.Text = refresh[M.Refresh];
			LF.Foreground = color[M.LinFit];	// 3: white
			LF.Text = "Fit Curves " + ((3 > M.LinFit) ? (M.PropName[M.LinFit]) : "disabled");
			TR.Text = (M.AutoPlot ? "Auto" : "Manual") + " Replot";
		}
	}	// class
}
