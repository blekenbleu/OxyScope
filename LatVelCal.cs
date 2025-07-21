// recalculate SlipRate and LatVel based on LAscale slider changes
// depends on LatAcc, SwayAcc
using System.Windows;

namespace blekenbleu.OxyScope
{
	public partial class Control
	{
		byte LVi, LAi, SAi;		// property indices
		double original;
		bool angle;				// which set of properties?

		// check for LatVel, LatAcc, SwayAcc properties
		void LAscaleCheck()
		{
			byte pc;
			for (byte b = pc = 0; b < M.axis.Length; b++)
				if (!M.axis[b])
					continue;
				else if (M.PropName[b] == "LatVel")
				{
					LVi = b;
					pc++;
				}
				else if (M.PropName[b] == "LatAcc")
				{
					LAi = b;
					pc++;
				}
				else if (M.PropName[b] == "SwayAcc")
				{
					SAi = b;
					pc++;
				}

			if (angle = 0 == pc)
			{
				SideSlip.Text = "Yaw Rate rescale for Side Slip Angle";
				for (byte b = 0; b < M.axis.Length; b++)
					if (!M.axis[b])
						continue;
					else if (M.PropName[b] == "SideSlipAngleRate")
					{
						LVi = b;
						pc++;
					}
					else if (M.PropName[b] == "YawRate")
					{
						LAi = b;
						pc++;
					}
					else if (M.PropName[b] == "SwayAccAngle")
					{
						SAi = b;
						pc++;
					}
			}
			else SideSlip.Text = "Yaw LatAcc rescale for Side Slip Rate (LatVel)";
					
			M.LAscaleVis = 3 == pc ? Visibility.Visible : Visibility.Hidden;
			if (save && 3 == pc)
			{
				original = O.x[LVi, start];
				save = false;
			}
		}

		void LAscaleValueChanged(object sender, RoutedEventArgs e)
		{
			double LatVel = original, min = 0, max = 0;
			ushort end = (ushort)(Length + start);
			
			for (ushort u = start; u < end; u++)
			{
				// Loaded: LatAcc = 0.0001 * View.Model.LAscale * YawVel * SpeedKmh
				double LatAcc = O.x[LAi, u];
				double SwayAcc = O.x[SAi, u];
				double SlipRate = M.LAscale * LatAcc;
				SlipRate -= SwayAcc;
				LatVel += 0.1 * SlipRate;		// numeric integration
				// damp near zero
				LatVel -= LatVel / (4 + (LatAcc * LatAcc) + (SwayAcc * SwayAcc));
				O.x[LVi, u] = LatVel;
				if (min > LatVel)
					min = LatVel;
				else if (max < LatVel)
					max = LatVel;
			}
			Rmax[LVi] = max;
			Rmin[LVi] = min;
			MinMax();
			Vplot.Model = Plot();
			M.XYprop1 += $";  LatVel range = {max - min}";
		}
	}
}
