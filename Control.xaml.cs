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
		internal static Model M;
		internal OxyScope O;
		static double Xmax, Ymax, Xmin, Ymin;	// axes limits
		ushort start, Length, property = 3;
		static double[] min, max;				// static required for CubicSlope()

		public Control() => InitializeComponent();

		public Control(OxyScope plugin) : this()
		{
			DataContext = M = new Model(O = plugin);
			O.x = new double[4, 1 + 5 * M.length];
			M.start = new ushort[] { 0, M.length };

			ButtonUpdate();
			M.min = new double[][] { new double[] { 0, 0, 0, 0 }, new double[] { 0, 0, 0, 0 } };
			M.max = new double[][] { new double[] { 0, 0, 0, 0 }, new double[] { 0, 0, 0, 0 } };
			min = M.min[0];  max = M.max[0];
			start = 0;
			RandomPlot();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
		}

		internal void Replot(ushort rs, ushort rl, double[] rmin, double[] rmax)
		{
			start = rs; min = rmin; max = rmax; Length = rl;

			if (!M.AutoPlot)
			{
				if (Visibility.Hidden == M.PVis)
					property = M.property;			// Accrue buffer full hint: switch to curve fit property selection
				M.PVis = Visibility.Visible;		// no more updates;  manual plot
				ButtonUpdate();
			}


			double Nmax = max[0];					// plot range for up to 3 Yprops
			double Nmin = min[0];
			for (int i = 1; i < 3; i++)
				if (M.axis[i])
				{
					if (Nmin > min[i])
						Nmin = min[i];
					if (Nmax < max[i])
						Nmax = max[i];
				}

			Ymax = Nmax;
			// move plot points inside limits
			Ymin = Nmin - 0.01 * (Nmax - Nmin);
			Xmax = max[3] + (Xmin = 0.01 * (max[3] - min[3]));
			Xmin = min[3] - Xmin;
			plot.Model = Plot();
		}

		private void D3click(object sender, RoutedEventArgs e)		// 3D visualize button
		{
			D3();
			M.D3vis = Visibility.Hidden;
		}

		private void PBclick(object sender, RoutedEventArgs e)		// Plot Button (for manual Refresh)
		{
			M.PVis = Visibility.Hidden;
			if (2 == M.Refresh)
				M.AutoPlot = M.Restart = true;						// Restart Accrue
			ButtonUpdate();
			plot.Model = Plot();
		}

		private void RefreshMode(object sender, RoutedEventArgs e)		// Refresh button
		{
			// 0 = max range, 1 = 3 second, 2 = Accrue
			// Accrue() now always maximizes StdDev for all Yprops
			if (2 == M.Refresh)
				M.Restart = true;
			M.Refresh = (ushort)((++M.Refresh) % 3);
			if (2 == M.Refresh)
			{
				M.AutoPlot = M.Restart = true;
				M.PVis = Visibility.Hidden;
			}
			else if (!M.AutoPlot && !M.Restart)
					M.PVis = Visibility.Visible;
			ButtonUpdate();
		}

		// inaccessible when 2 == M.Refresh
		private void PlotMode(object sender, RoutedEventArgs e)		// AutoPlot
		{
			M.AutoPlot = !M.AutoPlot;
			if (M.AutoPlot)
			{
				if (Visibility.Visible == M.PVis)
				{
					plot.Model = Plot();
					M.PVis = Visibility.Hidden;
				}
			} else M.PVis = Visibility.Visible;
			ButtonUpdate();
		}

		private void PropertySelect(object sender, RoutedEventArgs e)
		{
			if (Visibility.Hidden == M.PVis && 1 != M.Refresh)
			{
				M.Restart = true;				// restart sampling with newly selected property
				for (byte b = 0; b < 3; b++)	// change M.Refresh Y property
					if (M.axis[M.property = (ushort)((1 + M.property) % 4)])
						break;
			}
			else for (byte b = 0; b < 3; b++)	// just property for curve fitting
				if (M.axis[property = (ushort)((1 + property) % 4)])
					break;

			ButtonUpdate();
			plot.Model = Plot();
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

			TH.Text = refresh[M.Refresh];
			if (Visibility.Hidden == M.PVis && 1 != M.Refresh)
			{
				LF.Text = M.PropName[M.property] + " selected";
				LF.Foreground = color[M.property];	// 3: white
			} else {
				LF.Text = "Fit " + ((3 > property) ? (M.PropName[property]) : "disabled");
				LF.Foreground = color[property];	// 3: white
			}

			BTR.Visibility = (2 == M.Refresh) ? Visibility.Hidden : Visibility.Visible;

			if (M.AutoPlot)
			{
				M.PVis = Visibility.Hidden;
				TR.Text = "Auto Replot";
			}
			else TR.Text = "Manual Replot";
		}
	}	// class Control
}
