using System;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope
	{
		private readonly double[] StdDev = { 0, 0, 0 };
		private readonly double[] Avg = { 0, 0, 0 };
		double[] Total = { 0, 0, 0 };
		bool resume = true;
		private ushort timeout;

		void Accrue()
		{
			ushort p;

			if (resume && (0 == Sample % 30 || 180 < ++timeout))
			{
				timeout = 0;
				resume = false;
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

						Avg[p] = Total[p] / (Sample);

						double variance = 0;
						for(int j = 0; j < Sample; j++)
						{
							double diff = x[p,j] - Avg[p];
							variance += diff * diff; 
						}
						StdDev[p] = Math.Sqrt(variance / Sample);
					}

					VM.Current = $"count = {Sample};  StdDev = {StdDev[0]:0.0000}";
					if (VM.axis[1])
						VM.Current += $", {StdDev[1]:0.0000}";
					if (VM.axis[2])
						VM.Current += $", {StdDev[2]:0.0000}";
					View.Dispatcher.Invoke(() => View.Replot(work));
				}
			}
			for (p = 0; p < 3; p++)
				if (VM.axis[p] && Math.Abs(x[p,Sample] - Avg[p]) > 2 * StdDev[p])
					break;
			if (3 > p)
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
