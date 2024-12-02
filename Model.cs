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
		readonly PropertyChangedEventArgs PVevent = new PropertyChangedEventArgs(nameof(PVis));
		readonly PropertyChangedEventArgs TIevent = new PropertyChangedEventArgs(nameof(Title));
		readonly PropertyChangedEventArgs Xevent = new PropertyChangedEventArgs(nameof(Xprop));
		readonly PropertyChangedEventArgs XYevent = new PropertyChangedEventArgs(nameof(XYprop));
		readonly PropertyChangedEventArgs XY2event = new PropertyChangedEventArgs(nameof(XYprop2));
		readonly PropertyChangedEventArgs XY3event = new PropertyChangedEventArgs(nameof(XYprop3));
		readonly PropertyChangedEventArgs Yevent = new PropertyChangedEventArgs(nameof(Yprop));

		internal ushort		length, which = 0;	 	// which samples to plot
		internal ushort[]	start;					// split buffer
		internal double		Range, Total;
		internal double[]	Coef,
							xmin, ymin, xmax, ymax; // View uses for axes scaling
		internal bool LinFit, AutoPlot, Done = true;
		private string _title = "launch a game or Replay to collect XY property samples";
		public string Title { get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					Range = 0;
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
		public string Current		// OxyScope sets CurrentGame Car@Track
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
					Range = 0;	
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
					Range = 0;	
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

		private string _xyprop3 = "";
		public string XYprop3
		{	get => _xyprop3;
			set
			{
				if (_xyprop3 != value)
				{
					_xyprop3 = value;
					PropertyChanged?.Invoke(this, XY3event);
				}
			}
		}

//		private ushort _ref = 0;
		public ushort Refresh; /* 0 == max Range; 1 == 3 second, 2 = cumulative Range
		{	get => _ref;
			set
			{
				if (_ref != value)
				{
					_ref = value;
					PropertyChanged?.Invoke(this, TBevent);
					Total = Range = 0;
					Coef = null;
				}
			}
		} */

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
	}	// class M
}
