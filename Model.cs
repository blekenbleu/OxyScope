using System.ComponentModel;
using System.Windows;

namespace blekenbleu.OxyScope
{
	
	// https://intellitect.com/blog/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
	public class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs myevent) => PropertyChanged?.Invoke(this, myevent);

		readonly PropertyChangedEventArgs APevent = new PropertyChangedEventArgs(nameof(AutoPlot));
		readonly PropertyChangedEventArgs Cevent = new PropertyChangedEventArgs(nameof(Current));
		readonly PropertyChangedEventArgs FXevent = new PropertyChangedEventArgs(nameof(FilterX));
		readonly PropertyChangedEventArgs FYevent = new PropertyChangedEventArgs(nameof(FilterY));
		readonly PropertyChangedEventArgs Levent = new PropertyChangedEventArgs(nameof(LinFit));
		readonly PropertyChangedEventArgs PVevent = new PropertyChangedEventArgs(nameof(PVis));
		readonly PropertyChangedEventArgs TBevent = new PropertyChangedEventArgs(nameof(Refresh));
		readonly PropertyChangedEventArgs TIevent = new PropertyChangedEventArgs(nameof(Title));
		readonly PropertyChangedEventArgs Xevent = new PropertyChangedEventArgs(nameof(Xprop));
		readonly PropertyChangedEventArgs XRevent = new PropertyChangedEventArgs(nameof(Xrange));
		readonly PropertyChangedEventArgs XYevent = new PropertyChangedEventArgs(nameof(XYprop));
		readonly PropertyChangedEventArgs XY2event = new PropertyChangedEventArgs(nameof(XYprop2));
		readonly PropertyChangedEventArgs Yevent = new PropertyChangedEventArgs(nameof(Yprop));

		internal OxyScope Plugin;
		public double m, B, R2;     // for linear least-squares fit
		internal int which = 0;     // which samples to plot
        private string _title = "launch a game or Replay to collect XY property samples";
		public string Title { get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					R2 = Xrange = 0;
					PropertyChanged?.Invoke(this, TIevent);
				}
			}
		}

		private Visibility _unseen = Visibility.Hidden;
		public Visibility PVis
		{ 	get => _unseen;
			set
			{
				if (_unseen != value)
				{
					_unseen = value;
					PropertyChanged?.Invoke(this, PVevent);
				}
			} 
		}

		private string _current = "waiting for property values...";
		public string Current
		{	get => _current;
			set
			{
				if (_current != value)
				{
					_current = value;
					PropertyChanged?.Invoke(this, Cevent);
				}
			}
		}


		private string _xprop;
		public string Xprop
		{	get => _xprop;
			set
			{
				if (_xprop != value)
				{
					_xprop = value;
					R2 = Plugin.xmax[which] = Plugin.xmin[which] = Xrange = 0;	
					PropertyChanged?.Invoke(this, Xevent);
				}
			}
		}

		private string _yprop;
		public string Yprop
		{	get => _yprop;
			set
			{
				if (_yprop != value)
				{
					_yprop = value;
					R2 = Plugin.xmax[which] = Plugin.xmin[which] = Xrange = 0;	
					PropertyChanged?.Invoke(this, Yevent);
				}
			}
		}

		private string _xyprop = "Random data";
		public string XYprop
		{	get => _xyprop;
			set
			{
				if (_xyprop != value)
				{
					_xyprop = value;
					PropertyChanged?.Invoke(this, XYevent);
				}
			}
		}

		private string _xyprop2 = "";
		public string XYprop2
		{	get => _xyprop2;
			set
			{
				if (_xyprop2 != value)
				{
					_xyprop2 = value;
					PropertyChanged?.Invoke(this, XY2event);
				}
			}
		}

		private uint _ref = 0;
		public uint Refresh
		{	get => _ref;
			set
			{
				if (_ref != value)
				{
					_ref = value;
					PropertyChanged?.Invoke(this, TBevent);
					R2 = Plugin.xmin[which] = Plugin.xmax[which] = Xrange = 0;
				}
			}
		}

		private double _filtx;
		public double FilterX
		{	get => _filtx;
			set
			{
				if (_filtx != value)
				{
					_filtx = value;
					PropertyChanged?.Invoke(this, FXevent);
				}
			}
		}

		private double _filty;
		public double FilterY
		{	get => _filty;
			set
			{
				if (_filty != value)
				{
					_filty = value;
					PropertyChanged?.Invoke(this, FYevent);
				}
			}
		}

		private double _xrange = 0;
		public double Xrange
		{	get => _xrange;
			set
			{
				if (_xrange != value)
				{
					_xrange = value;
					PropertyChanged?.Invoke(this, XRevent);
				}
			}
		}

		private bool _linfit = true;
		public bool LinFit
		{	get => _linfit;
			set
			{
				if (_linfit != value)
				{
					_linfit = value;
					PropertyChanged?.Invoke(this, Levent);
				}
			}
		}

		private bool _aplot = true;
		public bool AutoPlot
		{	get => _aplot;
			set
			{
				if (_aplot != value)
				{
					_aplot = value;
					PropertyChanged?.Invoke(this, APevent);
					if (_aplot && Visibility.Visible == _unseen)
					{
						PVis = Visibility.Hidden;
						Plugin.View.Plot();
					}
				}
			}
		}
	}	// class Model
}
