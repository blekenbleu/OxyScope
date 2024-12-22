using System;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope
	{
		double StdDev, Avg;
		bool once = true;

		void Accrue()
		{
			if (once && (0 == VM.I % 30 || 180 < ++timeout))
			{
				timeout = 0;
				once = false;
				if (0 == VM.I)
				{
					StdDev = 0;
					Avg = x[0,VM.I] * 0.999;
				} else {
					double variance = 0;

					Avg = VM.Total / (VM.length = VM.I);

					for(int j = 0; j < VM.length; j++)
					{
						double diff = x[0,j] - Avg;
						variance += diff * diff; 
					}
					StdDev = Math.Sqrt(variance / VM.length);
					VM.Current = $"length = {VM.length};  StdDev = {StdDev:0.0000}";
					View.Dispatcher.Invoke(() => View.Replot(work));
				}
			}
			if(x[0,VM.I] > Avg + 2 * StdDev || x[0,VM.I] < Avg - 2 * StdDev)
			{
				if (VM.I > x.Length - 1)
					return;
				once = true;	// might stick at modulo 60 for awhile
				VM.Total += x[0,VM.I++];
			}
		}
	}
}
