using System;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope
	{
		double StdDev, Avg;
		bool once = true;

		void Accrue()
		{
			if (0 == i % 50 && once)
			{
				once = false;
				if (0 == i)
				{
					StdDev = 0;
					Avg = x[i] * 0.999;
				} else {
					double variance = 0;

					Avg = VM.Total / (VM.length = i);

					for(int j = 0; j < VM.length; j++)
					{
						double diff = x[j] - Avg;
						variance += diff * diff; 
					}
					StdDev = Math.Sqrt(variance / VM.length);
					VM.Current = $"length = {VM.length};  StdDev = {StdDev}";
					View.Dispatcher.Invoke(() => View.Replot(work));
				}
			}
			if(x[i] > Avg + 2 * StdDev || x[i] < Avg - 2 * StdDev)
			{
				if (i > x.Length - 1)
					return;
				once = true;	// might stick at modulo 60 for awhile
				VM.Total += x[i++];
			}
		}
	}
}
