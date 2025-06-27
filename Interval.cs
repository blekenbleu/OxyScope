using System.Collections.Generic;

namespace blekenbleu.OxyScope
{
	public partial class OxyScope
	{
		double Imin, Imax;
		short bucket;
        //		readonly ushort[] Intervals = new ushort[10];
        // because Intervals may need to be increased after SetupIntervals()
        readonly List<ushort> Intervals = new List<ushort> {0,0,0,0,0,0,0,0,0,0};
		void SetupIntervals()
		{
			Imax = VM.max[work][3]; Imin = VM.min[work][3];
			double Ifac = (Intervals.Count - 0.01) / (Imax - Imin);
			for (ushort i = 0; i < Sample; i++)
				Intervals[(ushort)(Ifac * (x[3, i] -  Imin))]++;
		}

		bool Interval()
		{	// find Intervals index corresponding to current x[3, Sample] value
			double Irange = Imax - Imin;
			double Ifac = ((double)Intervals.Count - 0.01) / Irange;
			bucket = (short)(Ifac *(x[3, Sample] -  Imin) / Irange);

			if (Intervals[bucket] < (double)Sample / Intervals.Count)	// less than average population?
				for (ushort p = 0; p < 3; p++)
					if (VM.axis[p] && StdSample[p] > 0.2 * StdDev[p])	// lower standard for unpopular intervals
						return true;
			return false;
		}

		// preserve current histogram buckets; append new buckets as needed
		void PrefixIntervals(double xValue)
		{
			double bucket_width = (Imax - Imin) / Intervals.Count;

			while (Imin > xValue)
			{
				Intervals.Insert(0, 0);
				Imin -= bucket_width;
			}
		}
		void AppendIntervals(double xValue)
		{
			double bucket_width = (Imax - Imin) / Intervals.Count;

			while (Imax < xValue)
			{
				Intervals.Add(0);
				Imax += bucket_width;
			}
		}
	}
}
