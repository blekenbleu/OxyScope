using System.Collections.Generic;
using System.Windows;				// Visibility
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
		static double Ymax, Ymin;				// axes limits - consolidated for all Y properties
		static double[] Xmax, Xmin;				// axes limits = unique for configured Xprop
		ushort start, Length, property = 3;
		byte currentX, highY, currentP;			// index Xmin[] and Xmax[] for plot axes rotations
		static double[] min, max;				// static required for CubicSlope()
		List<byte> xmap;						// M.PropName[] indices for rotating thru plot axes

		public Control() => InitializeComponent();

		public Control(OxyScope plugin) : this()
		{
			DataContext = M = new Model(O = plugin);
			O.x = new double[4, 1 + 5 * M.length];
			M.start = new ushort[] { 0, M.length };
			Xmax = new double[] { 0, 0 }; Xmin = new double[] { 0, 0 }; Ymax = 0; Ymin = 0;

			ButtonUpdate();
			M.min = new double[][] { new double[] { 0, 0, 0, 0 }, new double[] { 0, 0, 0, 0 } };
			M.max = new double[][] { new double[] { 0, 0, 0, 0 }, new double[] { 0, 0, 0, 0 } };
			min = M.min[0];  max = M.max[0];
			xmap = new List<byte> { 3, 0 };
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
			currentX = 0;
			M.ForeVS = "White";

			if (!M.AutoPlot)
			{
				if (Visibility.Hidden == M.PVis)
					property = M.property;			// Accrue buffer full hint: switch to curve fit property selection
				M.PVis = Visibility.Visible;		// no more updates;  manual plot
				ButtonUpdate();
			}


			double Nmax = max[0];					// plot range for up to 3 Yprops
			double Nmin = min[0];
			xmap = new List<byte> { 3, 0 };
			highY = 0;

			for (byte i = 1; i < 3; i++)
				if (M.axis[i])
				{
					highY = i;
					xmap.Add(i);
					if (Nmin > min[i])
						Nmin = min[i];
					if (Nmax < max[i])
						Nmax = max[i];
				}

			Xmax[1] = Ymax = Nmax;
			// move plot points inside limits
			Xmin[1] = Ymin = Nmin - 0.01 * (Nmax - Nmin);
			Xmax[0] = max[3] + (Xmin[0] = 0.01 * (max[3] - min[3]));
			Xmin[0] = min[3] - Xmin[0];
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

		// VS TextBox converted to Button
		private void VSclick(object sender, RoutedEventArgs e)		// rotate thru properties as X-axis
		{
			byte currentY = xmap[0];

			if (Visibility.Hidden == M.PVis || "White" == M.ForeVS)
				return;												// not while in RePlot()

			if (3 == currentY)
			{
				xmap.RemoveAt(0);
				currentX = 1;										// Use Ymax for Xmax, Ymin for Xmin
			} else {
				xmap.Add(currentY);
				if (highY == currentY) {
					xmap[0] = 3;									// restore configured X axis
					currentX = 0;
				} else xmap.RemoveAt(0);
			}
			currentP = 0;											// curve fitting property index
			property = 3;											// disable curve fit until user selects
			plot.Model = Plot();
			ButtonUpdate();
		}

		private void RefreshMode(object sender, RoutedEventArgs e)		// Refresh button
		{
			// 0 = max range, 1 = snapshot, 2 = Accrue
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
			else {	// just property for curve fitting
				currentP++;
				if (currentP >= xmap.Count)
				{
					currentP = 0;
					property = 3;
				}
				else property = xmap[currentP];
			}
			ButtonUpdate();
			plot.Model = Plot();
		}

		internal void ButtonUpdate()
		{
			string[] refresh = new string[]
			{ 	"Hold max range",
				"snapshot",
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
