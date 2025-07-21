using System;
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
				property = 3;
				start[1] = start[2] = Slength;
			} else {
				property = S.property;
				start[1] = start[2] = S.Slength;
			}
			start[2] <<= 1;
			busy = false;
			Collect = S.Collect;
			AutoPlot = S.AutoPlot;
		}

		readonly PropertyChangedEventArgs Cevent	= new PropertyChangedEventArgs(nameof(Current));
		readonly PropertyChangedEventArgs D3event	= new PropertyChangedEventArgs(nameof(D3vis));
		readonly PropertyChangedEventArgs FVevent	= new PropertyChangedEventArgs(nameof(ForeVS));
		readonly PropertyChangedEventArgs FXevent	= new PropertyChangedEventArgs(nameof(FilterX));
		readonly PropertyChangedEventArgs FYevent	= new PropertyChangedEventArgs(nameof(FilterY));
		readonly PropertyChangedEventArgs LAevent	= new PropertyChangedEventArgs(nameof(LAscale));
		readonly PropertyChangedEventArgs Levent	= new PropertyChangedEventArgs(nameof(Slength));	// VS does not detect change by XAML
		readonly PropertyChangedEventArgs LVevent	= new PropertyChangedEventArgs(nameof(LAscaleVis));
		readonly PropertyChangedEventArgs PVevent	= new PropertyChangedEventArgs(nameof(PVis));
		readonly PropertyChangedEventArgs THevent	= new PropertyChangedEventArgs(nameof(THText));
		readonly PropertyChangedEventArgs TRevent	= new PropertyChangedEventArgs(nameof(TRText));
		readonly PropertyChangedEventArgs TRFevent	= new PropertyChangedEventArgs(nameof(ForePlot));
		readonly PropertyChangedEventArgs XY1event	= new PropertyChangedEventArgs(nameof(XYprop1));
		readonly PropertyChangedEventArgs XY2event	= new PropertyChangedEventArgs(nameof(XYprop2));
		readonly PropertyChangedEventArgs Xevent	= new PropertyChangedEventArgs(nameof(Xprop));
		readonly PropertyChangedEventArgs Y0event	= new PropertyChangedEventArgs(nameof(Y0prop));
		readonly PropertyChangedEventArgs Y1event	= new PropertyChangedEventArgs(nameof(Y1prop));
		readonly PropertyChangedEventArgs Y2event	= new PropertyChangedEventArgs(nameof(Y2prop));

		internal byte		property, plot = 0;
		internal double[][]	min, max;
		internal bool		Restart = true, busy;					// Restart reinitializes work
		internal bool[]		axis = { true, false, false, true };	// which axes have properties assigned
		internal string[]	PropName = { "", "", "", "" };

		internal ushort[]	start = { 0, 60, 120 };						// swap chain buffer
		public ushort Slength	// must be public for xaml TitledSlider Binding
		{
			get => S.Slength;
			set
			{
				if (S.Slength != value)
				{
					start[1] = value;
					start[2] = (ushort)(value + value);
					S.Slength = value;
					Restart = true;
				}
			}
		}

		bool _bfull = false;
		internal bool Bfull
		{   get => _bfull;
			set
			{
				if (value && !_bfull)
				{
					_bfull = value;
					AutoPlot = false;
				} else _bfull = value;
			}
		}

		static readonly string[] refresh = { "more range", "one shot", "grow range" };
		private string _htext = refresh[0];
		public string THText
		{	get => _htext;
			set
			{
				if (_htext != value)
				{
					_htext = value;
					PropertyChanged?.Invoke(this, THevent);
				}
			} 
		}

		internal void SetFore()
		{
			ForeVS = busy ? "White" : "Green";
			ForePlot = S.AutoPlot || 1 == S.Collect || !busy ? "White" : "Red"; 
		}

		internal ushort Collect
		{
			get => S.Collect;
			set
			{
				S.Collect = (byte)(value % 3);
				if (2 == S.Collect)
				{
					PVis = Visibility.Hidden;
					AutoPlot = Restart = true;
				} else if (1 == S.Collect)
				{
					property = 3;
					XYprop2 = "";				// erase Collect 0 range
					PVis = busy ? Visibility.Visible : Visibility.Hidden;
				} else PVis = Visibility.Visible;	// 0 == Collect
				SetFore();
				THText = refresh[S.Collect];
			}
		}

		static internal readonly string[] trtext = { "Auto Replot", "Hold Plot", "Resume Auto", "Restart Auto", "Replot" };
		private string _text = trtext[0];
		public string TRText
		{	get => _text;
			set
			{
				if (_text != value)
				{
					_text = value;
					PropertyChanged?.Invoke(this, TRevent);
				}
			} 
		}

		internal bool AutoPlot
		{
			get => S.AutoPlot;
			set
			{
				S.AutoPlot = value;
				if (value)
				{
					TRText = trtext[0];
					busy = false;
					LAscaleVis = Visibility.Hidden;
					if (Bfull && Visibility.Hidden == PVis)
						Restart = true;
				} else TRText = trtext[(1 == Collect) ? 1 : (Bfull ? 3 : 2)];
				SetFore();
			}
		}

		private Visibility _unseen = Visibility.Hidden;
		public Visibility PVis
		{	get => _unseen;				// PVis
			set
			{
				if (_unseen != value)
				{
					_unseen = value;
					PropertyChanged?.Invoke(this, PVevent);
					SetFore();
				}
			} 
		}

		private string _TRfore = "White";
		public string ForePlot										// OxyScope sets CurrentGame Car@Track
		{	get => _TRfore;
			set
			{
				if (_TRfore != value)
				{
					_TRfore = value;
					PropertyChanged?.Invoke(this, TRFevent);
				}
			}
		}

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

		private Visibility _lav = Visibility.Hidden;
		public Visibility LAscaleVis
		{	get => _lav;
			set
			{
				if (_lav != value)
				{
					_lav = value;
					PropertyChanged?.Invoke(this, LVevent);
				}
			} 
		}

		private Visibility _un3D = Visibility.Hidden;
		public Visibility D3vis
		{	get => _un3D;
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

		public double LAscale
		{	get => (null == S) ? 1 : S.LAscale;
			set
			{
				if (S.LAscale != value)
				{
					S.LAscale = Math.Round(value, 3);
					PropertyChanged?.Invoke(this, LAevent);
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
