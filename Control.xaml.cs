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
		byte currentX, highY, currentP;			// index Xmin[] and Xmax[] for Vplot axes rotations
		static double[] Rmin, Rmax;				// static required for CubicSlope()
		List<byte> xmap;						// M.PropName[] indices for rotating thru Vplot axes

		public Control() => InitializeComponent();

		public Control(OxyScope plugin) : this()
		{
			DataContext = M = new Model(O = plugin);
			O.x = new double[4, 1501];			// based on samples per shot TitledSlider max 500
			Xmax = new double[] { 0, 0 }; Xmin = new double[] { 0, 0 }; Ymax = 0; Ymin = 0;

			if (1 == M.Refresh)
				M.property = 3;
			ButtonUpdate();
			M.min = new double[][] { new double[] { 0, 0, 0, 0 }, new double[] { 0, 0, 0, 0 } };
			M.max = new double[][] { new double[] { 0, 0, 0, 0 }, new double[] { 0, 0, 0, 0 } };
			Rmin = M.min[0];  Rmax = M.max[0];
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
			start = rs; Rmin = rmin; Rmax = rmax; Length = rl;
			currentX = 0;
			M.ForeVS = "White";

            if (!M.AutoPlot && Visibility.Hidden == M.PVis)
			{
				if (2 != M.Refresh)
					M.PVis = Visibility.Visible;	// no more updates;  hold Vplot
				if (1 != M.Refresh)
					property = M.property;			// switch curve fit property selection
				ButtonUpdate();						// color coding
			}

			Ymax = Rmax[0];					// Vplot range for up to 3 Yprops
			Ymin = Rmin[0];
			xmap = new List<byte> { 3, 0 };
			highY = 0;

			for (byte i = 1; i < 3; i++)
				if (M.axis[i])
				{
					highY = i;
					xmap.Add(i);
					if (Ymin > Rmin[i])
						Ymin = Rmin[i];
					if (Ymax < Rmax[i])
						Ymax = Rmax[i];
				}

			// move Vplot points inside limits
			Ymin -= 0.01 * (Ymax - Ymin);
			Xmin[1] = Ymin;
			Xmax[1] = Ymax;
			Xmax[0] = Rmax[3] + (Xmin[0] = 0.01 * (Rmax[3] - Rmin[3]));
			Xmin[0] = Rmin[3] - Xmin[0];
			Vplot.Model = Plot();
		}

		private void D3click(object sender, RoutedEventArgs e)		// 3D visualize button
		{
			D3();
			M.D3vis = Visibility.Hidden;
		}

		private void REPLOTclick(object sender, RoutedEventArgs e)		// REPLOT Button
		{
			M.PVis = Visibility.Hidden;
			ButtonUpdate();
		}

		// VS TextBox converted to Button
		private void VSclick(object sender, RoutedEventArgs e)		// rotate thru properties as X-axis
		{
			if ("white" == M.ForeVS)
				return;												// not while in RePlot()

			byte currentY = xmap[0];

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
			Ymax = Rmax[xmap[1]];
			Ymin = Rmin[xmap[1]];
			for (byte b = 2; b < xmap.Count; b++)					// set Y-axis range for current properties
			{
				if (Ymin > Rmin[xmap[b]])
					Ymin = Rmin[xmap[b]];
				if (Ymax < Rmax[xmap[b]])
					Ymax = Rmax[xmap[b]];
			}
			Ymin -= 0.01 * (Ymax - Ymin);
			Xmax[1] = Rmax[xmap[0]];
			Xmin[1] = Rmin[xmap[0]];								// set X-axis range for current Y property
			Xmin[1] -= 0.01 * (Xmax[1] - Xmin[1]);
			Vplot.Model = Plot();
			ButtonUpdate();
		}

		private void Refreshclick(object sender, RoutedEventArgs e)		// Refresh button
		{
			// 0 = max range, 1 = one shot, 2 = grow
			// Accrue() now always maximizes StdDev for all Yprops
			if (2 == M.Refresh)
				M.Restart = true;
			M.Refresh++;
			if (2 == M.Refresh)
			{
				M.AutoPlot = M.Restart = true;
				M.PVis = Visibility.Hidden;
			} else if (1 == M.Refresh)
				M.property = 3;
			ButtonUpdate();
		}

		// M.AutoPlot false stalls 2 == M.Refresh
		private void Plotclick(object sender, RoutedEventArgs e)		// AutoPlot
		{
			M.AutoPlot = !M.AutoPlot;
			if (M.AutoPlot && Visibility.Visible == M.PVis)
			{
				Vplot.Model = Plot();
				M.PVis = Visibility.Hidden;
			}
			ButtonUpdate();
		}

		private void Propertyclick(object sender, RoutedEventArgs e)
		{
			if (1 == M.Refresh || !M.AutoPlot)
			{									// just property for curve fitting
				currentP++;
				if (currentP >= xmap.Count)
				{
					currentP = 0;
					property = 3;
				}
				else property = xmap[currentP];
				Vplot.Model = Plot();
			} else for (byte b = 0; b < 3; b++)	// change M.Refresh Y property
				if (M.axis[M.property = (ushort)((1 + M.property) % 4)])
				{
					M.Restart = true;			// restart sampling with newly selected property
					break;
				}

			ButtonUpdate();
		}
	}	// class Control
}
