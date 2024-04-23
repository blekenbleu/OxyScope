using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OxyPlotPlugin
{
	// https://intellitect.com/blog/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
	public class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(PropertyChangedEventArgs myevent) => PropertyChanged?.Invoke(this, myevent);

		readonly PropertyChangedEventArgs BVevent = new PropertyChangedEventArgs(nameof(OxyButVis));
		readonly PropertyChangedEventArgs XYevent = new PropertyChangedEventArgs(nameof(XYprop));
		readonly PropertyChangedEventArgs Yevent = new PropertyChangedEventArgs(nameof(Yprop));
		readonly PropertyChangedEventArgs Xevent = new PropertyChangedEventArgs(nameof(Xprop));

		private Visibility _unseen;
		public Visibility OxyButVis
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
	}	// class ViewModel

	/// <summary>
	/// Control.xaml interaction logic
	/// </summary>
	public partial class Control : UserControl
	{
		public Plugin Plugin { get; }
		public ViewModel Model;
		public int lowval, minval;
		public Control()
		{
			Model = new ViewModel();
			Model.OxyButVis = Visibility.Hidden;
			Model.XYprop = "All plots must include Low > values > Min";
			DataContext = Model;
			InitializeComponent();
			Random rnd=new Random();
			lowval = 1; minval = 40;	// minimum plotable interval range 
			x = new double[2][];
			y = new double[2][];
			ymax = 15;
			x[0] = new double[180];
			x[1] = new double[x[0].Length];
			y[0] = new double[x[0].Length];
			y[1] = new double[x[0].Length];
			for (int i = 0; i < x[0].Length;)
			{
				y[0][i] = ymax * rnd.NextDouble();
				x[0][i] = ++i;
			}
		}

		public Control(Plugin plugin) : this()
		{
			this.Plugin = plugin;
			ScatterPlot(0, "launch a game or Replay to enable Y vs X property plots");
		}

		private void ScatterSeries_Click(object sender, RoutedEventArgs e)
		{
			Plugin.yprop = Yprop.Text;
			Plugin.xprop = Xprop.Text;
			Plugin.running = true;		// prevent Plugin overwriting slider prompt until first click
			ymax = 1 + Plugin.ymax[Plugin.which];
			ScatterPlot(Plugin.which, Plugin.gname + " " + Plugin.cname + "@" + Plugin.tname);
			// force which refill
			Plugin.ymax[Plugin.which] = Plugin.xmax[Plugin.which] = Plugin.ymin[Plugin.which] = Plugin.xmin[Plugin.which];
			Model.OxyButVis = Visibility.Hidden;
		}

		public double[][] x;
		public double[][] y;
		private double ymax;
		public string variables;
		private PlotModel model;

		public void ScatterPlot(int which, string title)
		{
			model = new PlotModel { Title = title };
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Left,
				Title = Plugin.yprop,
				Minimum = 0,
				Maximum = ymax
			});
			model.Axes.Add(new LinearAxis
			{
				Position = AxisPosition.Bottom,
				Title = Plugin.xprop,
				Minimum = 0,
				Maximum = 100
			});
			model = AddScatter(model, x[which], y[which], 3, OxyColors.Red);
			model.Series[model.Series.Count - 1].Title = "60Hz sample";
			model.LegendPosition = LegendPosition.TopRight;
			model.LegendFontSize = 12;
			model.LegendBorder = OxyColors.Black;

			model.LegendBorderThickness = 1;
			plot.Model = model;
		}

		private PlotModel AddScatter(PlotModel model, double[] x, double[] y, int size, OxyColor color)
		{
			var scatterSeries = new ScatterSeries { MarkerType = MarkerType.Circle };
			for (int i = 0; i < x.Length; i++)
				scatterSeries.Points.Add(new ScatterPoint(x[i], y[i], size));
			scatterSeries.MarkerFill = color;
			model.Series.Add(scatterSeries);
			return model;
		}

		// handle slider changes
		private void SLslider_DragCompleted(object sender, MouseButtonEventArgs e)
		{
			TBL.Text = "Low " + (lowval = (int)((Slider)sender).Value);
		}

		private void SRslider_DragCompleted(object sender, MouseButtonEventArgs e)
		{
			TBR.Text = "Min " + (minval = (int)((Slider)sender).Value);
		}
	}	// class
}
