using System;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope
	{
		private readonly double[] StdDev = { 0, 0, 0 };
		private readonly double[] Avg = { 0, 0, 0 };
		readonly double[] Total = { 0, 0, 0 }, oldTotal = { 0, 0, 0 };
		bool resume = true, backfill = false;
		private ushort timeout, overtime;
		readonly double[] StdSample = { 0, 0, 0};

		void Accrue()
		{
			ushort p;

			if (resume && (0 == Sample % 30 || 180 < ++timeout))
			{
				resume = false;
				timeout = overtime = 0;
				if (0 == Sample)
				{
					for (p = 0; p < 3; p++)
					{
						Avg[p] = x[p,Sample] * 0.999;
						StdDev[p] = 0;
					}
				} else {
					for (p = 0; p < 3; p++) {
						if (!VM.axis[p])
							continue;

						if (Total[p] == oldTotal[p])
							continue;

						oldTotal[p] = Total[p];
						Avg[p] = Total[p] / (Sample);

						double variance = 0;
						for(int j = 0; j < Sample; j++)
						{
							double diff = x[p,j] - Avg[p];
							variance += diff * diff; 
						}
						StdDev[p] = Math.Sqrt(variance / Sample);
					}

					VM.Current = (backfill ? "backfill " : "") + $"count = {Sample};  StdDev = {StdDev[0]:0.0000}";
					if (VM.axis[1])
						VM.Current += $", {StdDev[1]:0.0000}";
					if (VM.axis[2])
						VM.Current += $", {StdDev[2]:0.0000}";
					if (!VM.busy)
						View.Dispatcher.Invoke(() => View.Replot(Sample));
				}
			}	else if (!backfill && 180 < ++overtime) {
				backfill = true;
				SetupIntervals();
			}
			bool keep = false;
			for (p = 0; p < 3; p++)
				if (VM.axis[p])
				{
					double diff = x[p,Sample] - Avg[p];
					StdSample[p] = diff * diff;
					if (StdSample[p] > 2 * StdDev[p])
						keep = true;
				}
			if (keep || (backfill && Interval()))
			{
				resume = true;	// might stick at modulo 30 for awhile
				if (VM.axis[1])
					Total[1] += x[1,Sample];
				if (VM.axis[2])
					Total[2] += x[2,Sample];
				Total[0] += x[0,Sample++];
			}
		}
	}
}
