using System.ComponentModel;
using System.Windows;

namespace blekenbleu.OxyScope
{
	// elements must be public
	// https://intellitect.com/blog/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
	public class Model : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs myevent) => PropertyChanged?.Invoke(this, myevent);
		static Settings S;

		public Model(OxyScope os)
		{
			S = os.Settings;
			if (null == S)
			{
				Refresh = 1;
				property = 3;
				AutoPlot = false;
			} else {
				Refresh = S.Refresh;
				property = S.property;
				AutoPlot = S.AutoPlot;
			}
		}

		readonly PropertyChangedEventArgs Cevent	= new PropertyChangedEventArgs(nameof(Current));
		readonly PropertyChangedEventArgs D3event	= new PropertyChangedEventArgs(nameof(D3vis));
		readonly PropertyChangedEventArgs FVevent	= new PropertyChangedEventArgs(nameof(ForeVS));
		readonly PropertyChangedEventArgs FXevent	= new PropertyChangedEventArgs(nameof(FilterX));
		readonly PropertyChangedEventArgs FYevent	= new PropertyChangedEventArgs(nameof(FilterY));
		readonly PropertyChangedEventArgs PVevent	= new PropertyChangedEventArgs(nameof(PVis));
		readonly PropertyChangedEventArgs XY1event	= new PropertyChangedEventArgs(nameof(XYprop1));
		readonly PropertyChangedEventArgs XY2event	= new PropertyChangedEventArgs(nameof(XYprop2));
		readonly PropertyChangedEventArgs Xevent	= new PropertyChangedEventArgs(nameof(Xprop));
		readonly PropertyChangedEventArgs Y0event	= new PropertyChangedEventArgs(nameof(Y0prop));
		readonly PropertyChangedEventArgs Y1event	= new PropertyChangedEventArgs(nameof(Y1prop));
		readonly PropertyChangedEventArgs Y2event	= new PropertyChangedEventArgs(nameof(Y2prop));

		internal readonly ushort length = 300;
		internal ushort		Refresh, property;
		internal ushort[]	start;									// split buffer
		internal double[][]	min, max;
		internal bool		AutoPlot, Restart = true;				// work gets reinitialed by Restart
		internal bool[]		axis = { true, false, false, true };	// which axes have properties assigned
		internal string[]	PropName = { "", "", "", "" };

		private string _current = "waiting for property values...";
		public string Current										// OxyScope sets CurrentGame Car@Track
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

		private string _title = "launch game or Replay to collect and plot property samples";
		public string Title { get => _title;						// for PlotModel
			set
			{
				if (_title != value)
				{
					_title = value;
					Restart = true;									// Restart OxyScope
				}
			}
		}

		private Visibility _unseen = Visibility.Hidden;
		public Visibility PVis
		{ 	get => _unseen;				// PVis
			set
			{
				if (_unseen != value)
				{
					_unseen = value;
					PropertyChanged?.Invoke(this, PVevent);
					ForeVS = (Visibility.Hidden == _unseen) ? "White" : "Green";
				}
			} 
		}

		private Visibility _un3D = Visibility.Hidden;
		public Visibility D3vis
		{ 	get => _un3D;
			set
			{
				if (_un3D != value)
				{
					_un3D = value;
					PropertyChanged?.Invoke(this, D3event);
				}
			} 
		}

		public string Y0prop
		{	get => (null == S) ? "random" : S.Y0prop;
			set
			{
				if (S.Y0prop != value)
				{
					S.Y0prop = value;
					PropertyChanged?.Invoke(this, Y0event);
					Restart = true;
				}
			}
		}

		public string Y1prop
		{	get => S.Y1prop;
			set
			{
				if (S.Y1prop != value)
				{
					S.Y1prop = value;
					PropertyChanged?.Invoke(this, Y1event);
					Restart = true;
				}
			}
		}

		public string Y2prop
		{	get => S.Y2prop;
			set
			{
				if (S.Y2prop != value)
				{
					S.Y2prop = value;
					PropertyChanged?.Invoke(this, Y2event);
					Restart = true;
				}
			}
		}

		public string Xprop
		{	get => (null == S) ? "random" : S.Xprop;
			set
			{
				if (S.Xprop != value)
				{
					S.Xprop = value;
					Restart = true;
					PropertyChanged?.Invoke(this, Xevent);
				}
			}
		}

		private string _xyprop1 = "Random data";
		public string XYprop1
		{	get => _xyprop1;
			set
			{
				if (_xyprop1 != value)
				{
					_xyprop1 = value;
					PropertyChanged?.Invoke(this, XY1event);
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

		public double FilterX
		{	get => (null == S) ? 1 : S.FilterX;
			set
			{
				if (S.FilterX != value)
				{
					S.FilterX = value;
					PropertyChanged?.Invoke(this, FXevent);
				}
			}
		}

		public double FilterY
		{	get => (null == S) ? 1 : S.FilterY;
			set
			{
				if (S.FilterY != value)
				{
					S.FilterY = value;
					PropertyChanged?.Invoke(this, FYevent);
				}
			}
		}

		private string _foreVS = "White";	// VS TextBox converted to Button
		public string ForeVS
		{	get => _foreVS;
			set
			{
				if (_foreVS != value)
				{
					_foreVS = value;
					PropertyChanged?.Invoke(this, FVevent);
				}
			}
		}
	}	// class Model
}
