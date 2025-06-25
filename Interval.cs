using System;

namespace blekenbleu.OxyScope
{
    public partial class OxyScope
    {
		double Irange;
        readonly ushort[] Intervals = new ushort[10];
		void SetupIntervals()
		{
			Irange = VM.max[3,work] - VM.min[3,work];
			for (ushort i = 0; i < Sample; i++)
				Intervals[(ushort)(9.99 * (x[3, i] -  VM.min[3,work]) / Irange)]++;
		}

		bool Interval()
		{	// find Intervals index corresponding to current x[3, Sample] value
			short j = (short)(9.99 *(x[3, Sample] -  VM.min[3,work]) / (VM.max[3,work] - VM.min[3,work]));

			if (Intervals[j] < 0.1 * Sample)	// less than average population?
				for (ushort p = 0; p < 3; p++)
					if (VM.axis[j] && dev[p] > 0.1 * StdDev[p])	// lower standard for unpopular intervals
						return true;
			return false;
		}
	}
}
