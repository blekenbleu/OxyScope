using System.ComponentModel;
using System.Windows;

namespace OxyPlotPlugin
{
	
	// https://intellitect.com/blog/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
	public class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs myevent) => PropertyChanged?.Invoke(this, myevent);

		readonly PropertyChangedEventArgs BVevent = new PropertyChangedEventArgs(nameof(Vis));
		readonly PropertyChangedEventArgs TVevent = new PropertyChangedEventArgs(nameof(THvis));
		readonly PropertyChangedEventArgs XYevent = new PropertyChangedEventArgs(nameof(XYprop));
		readonly PropertyChangedEventArgs THevent = new PropertyChangedEventArgs(nameof(Threshold));
		readonly PropertyChangedEventArgs LFevent = new PropertyChangedEventArgs(nameof(LinFit));
		readonly PropertyChangedEventArgs THVevent = new PropertyChangedEventArgs(nameof(ThresVal));
		readonly PropertyChangedEventArgs Yevent = new PropertyChangedEventArgs(nameof(Yprop));
		readonly PropertyChangedEventArgs Xevent = new PropertyChangedEventArgs(nameof(Xprop));
		readonly PropertyChangedEventArgs Tevent = new PropertyChangedEventArgs(nameof(Title));

		private string _title;
		public string Title { get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					PropertyChanged?.Invoke(this, Tevent);
				}
			}
		}

		private Visibility _thvis;
		public Visibility THvis
		{ 	get => _thvis;
			set
			{
				if (_thvis != value)
				{
					_thvis = value;
					PropertyChanged?.Invoke(this, TVevent);
				}
			} 
		}

		private Visibility _unseen;
		public Visibility Vis
		{ 	get => _unseen;
			set
			{
				if (_unseen != value)
				{
					_unseen = value;
					PropertyChanged?.Invoke(this, BVevent);
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

		private bool _thresh = true;
		public bool Threshold
		{	get => _thresh;
			set
			{
				if (_thresh != value)
				{
					_thresh = value;
					PropertyChanged?.Invoke(this, THevent);
					if (_thresh)
						_thval = _thsave;	// restore saved ThresVal
				}
			}
		}

		private double _thval, _thsave;
		public double ThresVal
		{	get => _thval;
			set
			{
				if (_thval != value)
				{
					_thval = value;
					PropertyChanged?.Invoke(this, THVevent);
					if (_thresh)
						_thsave = _thval;
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
					PropertyChanged?.Invoke(this, LFevent);
				}
			}
		}
	}	// class Model
}
