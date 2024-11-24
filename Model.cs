using System.ComponentModel;
using System.Windows;

namespace blekenbleu.OxyScope
{
	
	// https://intellitect.com/blog/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
	public class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs myevent) => PropertyChanged?.Invoke(this, myevent);

		readonly PropertyChangedEventArgs Cevent = new PropertyChangedEventArgs(nameof(Current));
		readonly PropertyChangedEventArgs FXevent = new PropertyChangedEventArgs(nameof(FilterX));
		readonly PropertyChangedEventArgs FYevent = new PropertyChangedEventArgs(nameof(FilterY));
		readonly PropertyChangedEventArgs Levent = new PropertyChangedEventArgs(nameof(LinFit));
		readonly PropertyChangedEventArgs Revent = new PropertyChangedEventArgs(nameof(Plot));
		readonly PropertyChangedEventArgs RVevent = new PropertyChangedEventArgs(nameof(RVis));
		readonly PropertyChangedEventArgs TBevent = new PropertyChangedEventArgs(nameof(Refresh));
		readonly PropertyChangedEventArgs TIevent = new PropertyChangedEventArgs(nameof(Title));
		readonly PropertyChangedEventArgs Xevent = new PropertyChangedEventArgs(nameof(Xprop));
		readonly PropertyChangedEventArgs XRevent = new PropertyChangedEventArgs(nameof(Xrange));
		readonly PropertyChangedEventArgs XYevent = new PropertyChangedEventArgs(nameof(XYprop));
		readonly PropertyChangedEventArgs Yevent = new PropertyChangedEventArgs(nameof(Yprop));

		public double m, B;		// for linear least-squares fit
		private string _title = "launch a game or Replay to collect XY property samples";
		public string Title { get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					Xrange = 0;
					PropertyChanged?.Invoke(this, TIevent);
				}
			}
		}

		private Visibility _unseen = Visibility.Hidden;
		public Visibility RVis
		{ 	get => _unseen;
			set
			{
				if (_unseen != value)
				{
					_unseen = value;
					PropertyChanged?.Invoke(this, RVevent);
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
					Xrange = 0;	
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
					Xrange = 0;
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

		private bool _tbool = true;
		public bool Refresh
		{	get => _tbool;
			set
			{
				if (_tbool != value)
				{
					_tbool = value;
					PropertyChanged?.Invoke(this, TBevent);
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

		private bool _replot = true;
		public bool Plot
		{	get => _replot;
			set
			{
				if (_replot != value)
				{
					_replot = value;
					PropertyChanged?.Invoke(this, Revent);
				}
			}
		}
	}	// class Model
}
