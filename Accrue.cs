using System;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope
	{
		private readonly double[] StdDev = { 0, 0, 0 };
		private readonly double[] Avg = { 0, 0, 0 };
		bool restart = true;

		void Accrue()
		{
			ushort p;

			if (restart && (0 == VM.I % 30 || 180 < ++timeout))
			{
				timeout = 0;
				restart = false;
				if (0 == VM.I)
				{
					for (p = 0; p < 3; p++)
					{
						Avg[p] = x[p,VM.I] * 0.999;
						StdDev[p] = 0;
					}
				} else {					
					for (p = 0; p < 3; p++) {
						if (!VM.axis[p])
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
					if (VM.axis[1])
						VM.Current += $", {StdDev[1]:0.0000}";
					if (VM.axis[2])
						VM.Current += $", {StdDev[2]:0.0000}";
					View.Dispatcher.Invoke(() => View.Replot(work));
				}
			}
			for (p = 0; p < 3; p++)
				if (VM.axis[p] && Math.Abs(x[p,VM.I] - Avg[p]) > 2 * StdDev[p])
					break;
			if (3 > p)
			{
				restart = true;	// might stick at modulo 30 for awhile
				if (VM.axis[1])
					VM.Total[1] += x[1,VM.I];
				if (VM.axis[2])
					VM.Total[2] += x[2,VM.I];
				VM.Total[0] += x[0,VM.I++];
			}
		}
	}
}
