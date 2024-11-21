using System.ComponentModel;
using System.Windows;

namespace OxyPlotPlugin
{
	
	// https://intellitect.com/blog/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
	public class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs myevent) => PropertyChanged?.Invoke(this, myevent);

		readonly PropertyChangedEventArgs FXevent = new PropertyChangedEventArgs(nameof(FilterX));
		readonly PropertyChangedEventArgs FYevent = new PropertyChangedEventArgs(nameof(FilterY));
		readonly PropertyChangedEventArgs Levent = new PropertyChangedEventArgs(nameof(LinFit));
		readonly PropertyChangedEventArgs Revent = new PropertyChangedEventArgs(nameof(Replot));
		readonly PropertyChangedEventArgs RVevent = new PropertyChangedEventArgs(nameof(RVis));
		readonly PropertyChangedEventArgs TBevent = new PropertyChangedEventArgs(nameof(ThresBool));
		readonly PropertyChangedEventArgs THevent = new PropertyChangedEventArgs(nameof(ThresVal));
		readonly PropertyChangedEventArgs TIevent = new PropertyChangedEventArgs(nameof(Title));
		readonly PropertyChangedEventArgs TVevent = new PropertyChangedEventArgs(nameof(TVis));
		readonly PropertyChangedEventArgs Xevent = new PropertyChangedEventArgs(nameof(Xprop));
		readonly PropertyChangedEventArgs XYevent = new PropertyChangedEventArgs(nameof(XYprop));
		readonly PropertyChangedEventArgs Yevent = new PropertyChangedEventArgs(nameof(Yprop));

		public double m, B;		// for linear least-squares fit
		private string _title;
		public string Title { get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					PropertyChanged?.Invoke(this, TIevent);
				}
			}
		}

		private Visibility _tvis;
		public Visibility TVis
		{ 	get => _tvis;
			set
			{
				if (_tvis != value)
				{
					_tvis = value;
					PropertyChanged?.Invoke(this, TVevent);
				}
			} 
		}

		private Visibility _unseen;
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

		private string _xprop;
		public string Xprop
		{	get => _xprop;
			set
			{
				if (_xprop != value)
				{
					_xprop = value;
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
					PropertyChanged?.Invoke(this, Yevent);
				}
			}
		}

		private string _xyprop;
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
		public bool ThresBool
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

		private double _thval;
		public double ThresVal
		{	get => _tbool ? _thval : 0;
			set
			{
				if (_thval != value)
				{
					_thval = value;
					PropertyChanged?.Invoke(this, THevent);
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
		public bool Replot
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
