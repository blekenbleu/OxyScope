using System;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope
	{
        private readonly double[] StdDev = { 0, 0, 0 };
        private readonly double[] Avg = { 0, 0, 0 };
        bool once = true;

		void Accrue()
		{
			ushort p;

			if (once && (0 == VM.I % 30 || 180 < ++timeout))
			{
				timeout = 0;
				once = false;
				if (0 == VM.I)
				{
					for (p = 0; p < 3; p++)
					{
						Avg[p] = x[p,VM.I] * 0.999;
						StdDev[p] = 0;
					}
				} else {
					for (p = 0; p < 3; p++) {
						if (!VM.a[p])
							continue;

						Avg[p] = VM.Total[p] / (VM.length = VM.I);

						double variance = 0;
						for(int j = 0; j < VM.length; j++)
						{
							double diff = x[p,j] - Avg[p];
							variance += diff * diff; 
						}
						StdDev[p] = Math.Sqrt(variance / VM.length);
					}
					VM.Current = $"length = {VM.length};  StdDev = {StdDev[0]:0.0000}";
					if (VM.a[1])
						VM.Current += $", {StdDev[1]:0.0000}";
					if (VM.a[2])
						VM.Current += $", {StdDev[2]:0.0000}";
					View.Dispatcher.Invoke(() => View.Replot(work));
				}
			}
			for (p = 0; p < 3; p++)
				if (VM.a[p] && (x[p,VM.I] > Avg[p] + 2 * StdDev[p] || x[p,VM.I] < Avg[p] - 2 * StdDev[p]))
					break;
			if (3 > p)		
			{
				once = true;	// might stick at modulo 60 for awhile
				VM.Total[0] += x[0,VM.I];
				if (VM.a[1])
					VM.Total[1] += x[1,VM.I];
				if (VM.a[2])
					VM.Total[2] += x[2,VM.I];
				VM.I++;
			}
		}
	}
}
