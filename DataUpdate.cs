using GameReaderCommon;
using SimHub.Plugins;
using System.Collections.Generic;
using System.Windows;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope : IPlugin, IDataPlugin, IWPFSettingsV2
	{
		PluginManager PM;

		string Last(string[] split) => split[split.Length - 1];	// last substring

		bool ValidateProp(int i, string prop)
		{
			var yp = PM.GetPropertyValue(prop);
			VM.axis[i] = null != yp && float.TryParse(yp.ToString(), out f[i]);
			if (!VM.axis[i])
			{
				if (VM.Restart)
				{
					string foo = 3 > i ? "Y" + i : "X";

					if (oops)
						VM.XYprop2 += $"invalid {foo} property:  " + prop;
					else VM.XYprop2 = $"invalid {foo} property:  " + prop;
					oops = true;
				}
			} else VM.PropName[i] = Last(prop.Split('.'));
			return VM.axis[i];
		}

		/// <summary>
		/// Called one time per game data update, contains all normalized game data,
		/// raw data are intentionally "hidden" under axis generic object type
		/// (A plugin SHOULD NOT USE IT)
		///
		/// This method is on the critical path; execute as fast as possible,
		/// avoid throwing any error
		///
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <param name="data">Current game data, including current and previous data frame.</param>
		readonly float[] f = { 0, 0, 0, 0 };		// float values from properties
		readonly double[] IIR = { 0, 0, 0, 0 };		// filtered property values from f[]
		internal double[,] x;						// Vplot samples from IIR[]
		private byte work, other;					// buffers currently sampling, other non-plot buffer
		private ushort Sample;						// which x[,] is currently being worked
		bool oops = false, available = false;
		int clf = 0;								// current Y property
		string CarId = "";
		double current;
		uint WaitCt = 0;
		public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			PM = pluginManager;		// ValidateProp() uses
			oops = false;
			if (!ValidateProp(0, VM.Y0prop) || !ValidateProp(3, VM.Xprop))
				return;

			ValidateProp(1, VM.Y1prop); ValidateProp(2, VM.Y2prop);
			if (VM.Restart)
				View.Dispatcher.Invoke(() => View.ButtonUpdate());

			if (oops)
			{
				oops = false;
				VM.XYprop2 += ";  continuing...";
			}

			if (2 == VM.Refresh && !VM.AutoPlot && !VM.Restart)
				return;

			if (!data.GameRunning || null == data.OldData || null == data.NewData)
			{
				if (0 == CarId.Length)	// avoid wiping XYprop1 if already past this;  replays can get here
					VM.XYprop1 = "waiting for valid data";
				return;
			}

			if (0 == data.NewData.CarId.Length)
			{
				VM.XYprop1 = "waiting for CarId...";
				return;
			}

			if (CarId != data.NewData.CarId)
			{
				CarId = data.NewData.CarId;
				// Title change sets Restart
				VM.Title = pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
						 + ":  " + pluginManager.GetPropertyValue("DataCorePlugin.GameData.CarModel")?.ToString()
						 + "@"	 + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackName")?.ToString();
			}

			if (1 > (double)pluginManager.GetPropertyValue("DataCorePlugin.GameData.SpeedKmh")
				|| current == data.NewData.CarSettings_CurrentDisplayedRPMPercent)
			{
				if (!VM.Bfull && 30 < ++WaitCt)
					VM.XYprop1 = "waiting for action";
				return;
			}

			current = data.NewData.CarSettings_CurrentDisplayedRPMPercent;

			int i;
			if (VM.Restart)
			{
				VM.Restart = VM.Bfull = false;
				VM.XYprop1 = "Restart";
				for (i = 0; i < 4; i++)
					IIR[i] = f[i];
				if (2 == VM.Refresh)
				{
					VM.plot = work = 0;				// Accrue() uses the full buffer
					Total[0] = Total[1] = Total[2] = oldTotal[0] = oldTotal[1] = oldTotal[2] = 0;
					Intervals = new List<ushort> {0,0,0,0,0,0,0,0,0,0};
					backfill = false;
					resume = true;
					VM.AutoPlot = true;
				} else {
					work = (byte)((1 + VM.plot) % 3);
					other = (byte)((2 + VM.plot) % 3);
				}
				Sample = VM.start[work];
				clf = VM.property % 3;
			} else {
				// check for full buffer, redundant samples
				if (Sample >= x.Length >> 2)
				{							// this should occur only for accumulations (2 == VM.Refresh)
					if (!VM.Bfull)
					{						// victory lap
						VM.Bfull = true;
						VM.XYprop1 = $"sample buffer full;  {Intervals.Count} histogram buckets";
						VM.AutoPlot = false;			// update Control property
						VM.TRText = "Restart Auto";
						View.Dispatcher.Invoke(() => View.Replot((ushort)(x.Length >> 2)));
					}
					return;				// Restart sample may have been before car moved
				}

				for (i = 0; i < 3; i++)
					if(VM.axis[i] && System.Math.Abs(f[i] - IIR[i]) > 0.02 * (VM.max[work][i] - VM.min[work][i]))
						break;
				if (3 == i)					// differed from previous by < 2% ?
					return;

				if (30 < WaitCt)
					WaitCt = 0;
			}

			bool[] mm = { false, false };	// remember whether min or max change
			for (i = 0; i < 4; i++)
				if (VM.axis[i])
				{
					IIR[i] += (f[i] - IIR[i]) / VM.FilterX;
					x[i,Sample] = IIR[i];
					if (VM.start[work] == Sample)
						VM.min[work][i] = VM.max[work][i] = x[i,Sample];
					else if (VM.min[work][i] > x[i,Sample])	// volume of sample values
					{
						if (i == VM.property)
							mm[0] = true;
						VM.min[work][i] = x[i,Sample];
					}
					else if (VM.max[work][i] < x[i,Sample])
					{
						if (i == VM.property)
							mm[1] = true;
						VM.max[work][i] = x[i,Sample];
					}
				}

			if (2 == VM.Refresh)		// Accrue processes all buffered samples
			{
				if (backfill)
				{
					if (mm[0])
						PrefixIntervals(x[VM.property, Sample]);
					else if (mm[1])
						AppendIntervals(x[VM.property, Sample]);
				}
				Accrue();				// runs until buffer is full; restart by changing Refresh mode
				return;
			}
			else if ((++Sample - VM.start[work]) >= VM.Slength)	// filled?
			{
				// swap chain buffer:  buffer setup in Control.xaml.cs Control() and Model.Slength
				// https://blekenbleu.github.io/SimHub/swapchain.htm
				double work_range = Drange(work, VM.property);

				VM.Current = $"{VM.min[work][clf]:#0.000} <= Y <= {VM.max[work][clf]:#0.000};  "
						   + ((1 == VM.Refresh) ? $"{VM.min[work][3]:#0.000} <= X <= {VM.max[work][3]:#0.000}"
												: $"{VM.PropName[VM.property]} range {work_range}");

				// Refresh: 0 = greater range, 1 = snapshot, 2 = grow range (not handled here)
				// property: 3 == no curve fitting; 0-2 correspond to Y0-Y2
				if (1 == VM.Refresh || Drange(other, VM.property) < work_range)
				{
					other = work;
					work = (byte)(3 - (other + VM.plot));
 				}
				available = 1 == VM.Refresh || Drange(VM.plot, VM.property) < Drange(other, VM.property);
				Sample = VM.start[work];
			}

			if (available && !VM.busy)
			{
				available = false;
				VM.busy = true;
				VM.plot = other;
				other = (byte)(3 - (VM.plot + work));
				View.Dispatcher.Invoke(() => View.Replot(VM.Slength));
			}
		}														// DataUpdate()

		double Drange(byte b, byte c) => (VM.max[b][c] - VM.min[b][c]);
	}
}
