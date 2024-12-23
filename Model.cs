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
		readonly PropertyChangedEventArgs Xevent0 = new PropertyChangedEventArgs(nameof(Xprop0));
		readonly PropertyChangedEventArgs Xevent1 = new PropertyChangedEventArgs(nameof(Xprop1));
		readonly PropertyChangedEventArgs Xevent2 = new PropertyChangedEventArgs(nameof(Xprop2));
		readonly PropertyChangedEventArgs XYevent = new PropertyChangedEventArgs(nameof(XYprop));
		readonly PropertyChangedEventArgs XY2event = new PropertyChangedEventArgs(nameof(XYprop2));
		readonly PropertyChangedEventArgs Yevent = new PropertyChangedEventArgs(nameof(Yprop));

		internal ushort		I, length, Refresh = 0, which = 0;
		internal ushort[]	start;					// split buffer
		internal double		Range, Total;
		internal double[]	Coef; // View uses for axes scaling
		internal double[,]	min, max;
		internal bool LinFit, AutoPlot, Once = true, Restart = true;
		internal bool[] a = { true, false, false, true };	// which axes have properties assigned
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

		private bool _reset = true;
		public bool Reset
		{   get => _reset;
			set
			{
				if (_reset = value)
				{
				   	Total = Range = I = which = 0;	
					Restart = Once = true;
					Coef = null;
					length = (ushort)(2 == Refresh ? 0 : 180);
				}
			}
		}
/* pseudo-array
		internal string Xprop(uint i)
		{
			switch (i)
			{
				case 0:
					return _xprop0;
				case 1:
					return _xprop1;
				case 2:
					return _xprop2;
				default:
					return "";
			}
		}

		internal void Xprop(uint i, string st)
		{
			switch (i)
			{
				case 0:
					Xprop0 = st;
					break;
				case 1:
					Xprop1 = st;
					break;
				case 2:
					Xprop2 = st;
					break;
				default:
					break;
			}
		}
 */
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

		private string _xprop0;
		public string Xprop0
		{	get => _xprop0;
			set
			{
				if (_xprop0 != value)
				{
					_xprop0 = value;
					PropertyChanged?.Invoke(this, Xevent0);
					Reset = true;
				}
			}
		}

		private string _xprop1;
		public string Xprop1
		{	get => _xprop1;
			set
			{
				if (_xprop1 != value)
				{
					_xprop1 = value;
					PropertyChanged?.Invoke(this, Xevent1);
					Reset = true;
				}
			}
		}

		private string _xprop2;
		public string Xprop2
		{	get => _xprop2;
			set
			{
				if (_xprop2 != value)
				{
					_xprop2 = value;
					PropertyChanged?.Invoke(this, Xevent2);
					Reset = true;
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
					Reset = true;
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
