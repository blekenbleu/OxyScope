using GameReaderCommon;
using SimHub.Plugins;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;

namespace blekenbleu.OxyScope
{
	[PluginDescription("XY OxyPlot of paired SimHub properties")]
	[PluginAuthor("blekenbleu")]
	[PluginName("OxyPlot XY")]
	public partial class OxyScope : IPlugin, IDataPlugin, IWPFSettingsV2
	{
		public Settings Settings;
		string CarId = "";
		public static string PluginVersion = FileVersionInfo.GetVersionInfo(
            Assembly.GetExecutingAssembly().Location).FileVersion.ToString();

		/// <summary>
		/// Instance of the current plugin manager
		/// </summary>
		public PluginManager PluginManager { get; set; }

		/// <summary>
		/// Gets the left menu icon. Icon must be 24x24 black and white.
		/// </summary>
		public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

		/// <summary>
		/// Gets a short plugin title to show in left menu.
		/// Return null if you want to use the title as defined in PluginName attribute.
		/// </summary>
		public string LeftMenuTitle => "OxyScope " + PluginVersion;

		/// <summary>
		/// Called one time per game data update, contains all normalized game data,
		/// raw data are intentionally "hidden" under a generic object type
		/// (A plugin SHOULD NOT USE IT)
		///
		/// This method is on the critical path; execute as fast as possible,
		/// avoid throwing any error
		///
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <param name="data">Current game data, including current and previous data frame.</param>
		double IIRX = 0, IIRY = 0;
		private int i, work;					// arrays currently being sampled
		public double[] xmin, ymin, xmax, ymax; // View uses for axes scaling
		bool oops = false;
		public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			if (!data.GameRunning || null == data.OldData || null == data.NewData)
				return;

			float xf, yf;
			var xp = pluginManager.GetPropertyValue(View.Model.Xprop);

			if (null == xp || !float.TryParse(xp.ToString(), out xf))
			{
				View.Model.XYprop = "invalid X property:  " + View.Model.Xprop;
				oops = true;
				return;
			}

			var yp = pluginManager.GetPropertyValue(View.Model.Yprop);
			if (null == yp || !float.TryParse(yp.ToString(), out yf))
			{
				View.Model.XYprop = "invalid Y property:  " + View.Model.Yprop;
				oops = true;
				return;
			}

			if (oops)
			{
				oops = false;
				View.Model.XYprop = "continuing...";
            }

			if (0 == data.NewData.CarId.Length)
				return;

			if (CarId != data.NewData.CarId)
			{
				CarId = data.NewData.CarId;
				IIRX = IIRY = 0;
				xmin[0] = xmin[1] = ymin[0] = ymin[1] = xmax[0] = ymax[0] = xmax[1] = ymax[1] = 0;
           		View.Model.Title =
                    	pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
                	    + ":  " + pluginManager.GetPropertyValue("DataCorePlugin.GameData.CarModel")?.ToString()
                    	+ "@" + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackName")?.ToString();  
			}

			IIRX += (xf - IIRX) / View.Model.FilterX;
			IIRY += (yf - IIRY) / View.Model.FilterY;
			View.x[i] = IIRX;
			View.y[i] = IIRY;
			if (View.start[work] == i)
			{
				xmin[work] = xmax[work] = View.x[i];
				ymin[work] = ymax[work] = View.y[i];
			}
			else	// volume of sample values
			{
				if (xmin[work] > View.x[i])
					xmin[work] = View.x[i];
				else if (xmax[work] < View.x[i])
					xmax[work] = View.x[i];

				if (ymax[work] < View.y[i])
					ymax[work] = View.y[i];
				else if (ymin[work] > View.y[i])
					ymin[work] = View.y[i];
			}
			if ((++i - View.start[work]) >= View.length >> 1)	// filled?
			{
				// Coordination:  View should disable running while loading Plot
				View.Model.Current = $"{xmin[work]:#0.000} <= X <= "
								   + $"{xmax[work]:#0.000};  "
								   + $"{ymin[work]:#0.000} <= Y <= "
								   + $"{ymax[work]:#0.000}";

				int n = 1 - work;
				bool bigger = (xmax[work] - xmin[work]) > (xmax[n] - xmin[n]);
                if (View.Model.Refresh || bigger)
				{
                    View.Dispatcher.Invoke(() => View.Replot(work, false));
					// Replot typically frees buffers
					if((xmax[work] - xmin[work]) > (xmax[n] - xmin[n]))
						work = n;			// refill the buffer with smaller range
				}
				i = View.start[work];
			}
		}

		/// <summary>
		/// Called at plugin manager stop, close/dispose anything needed here !
		/// Plugins are rebuilt at game change
		/// </summary>
		/// <param name="pluginManager"></param>
		public void End(PluginManager pluginManager)
		{
			Settings.FilterX = View.Model.FilterX;
			Settings.FilterY = View.Model.FilterY;
			Settings.Xprop = View.Model.Xprop;
			Settings.Yprop = View.Model.Yprop;
			Settings.LinFit = View.Model.LinFit;
			Settings.Refresh = View.Model.Refresh;
			Settings.Plot = View.Model.Plot;
			// Save settings
			this.SaveCommonSettings("GeneralSettings", Settings);
		}

		/// <summary>
		/// Returns the settings control, return null if no settings control is required
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <returns></returns>
		private Control View;
		public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
		{
			return View = new Control(this);
		}

		/// <summary>
		/// Called once after plugin instance
		/// Plugins are reinstanced at game change
		/// </summary>
		/// <param name="pluginManager"></param>
		public void Init(PluginManager pluginManager)
		{
			work = 0;
			string where = "DataCorePlugin.GameData.",					// defaults
				x = "AccelerationHeave", y = "GlobalAccelerationG";

            ymin = new double[] {-90, -90}; ymax = new double[] {90, 90};
			xmin = new double[] {-90, -90}; xmax = new double[] {90, 90};
			SimHub.Logging.Current.Info("Starting " + LeftMenuTitle);

			// Load settings
			Settings = this.ReadCommonSettings<Settings>("GeneralSettings", () => new Settings());
			if (null == Settings)
				Settings = new Settings() {
					Xprop = where+x,
                    Yprop = where+y,
					FilterX = 1, FilterY = 1, Refresh = true, LinFit = true
                };
			else {
				if (0 == Settings.Xprop.Length)
						Settings.Xprop = where+x;
				if (0 == Settings.Yprop.Length)
						Settings.Yprop = where+y;
				if (1 > Settings.FilterX)
					Settings.FilterX = 1;
                if (1 > Settings.FilterY)
                    Settings.FilterY = 1;
            }

			this.AttachDelegate("Xprop", () => View.Model.Xprop);
			this.AttachDelegate("Yprop", () => View.Model.Yprop);
			this.AttachDelegate("IIRX", () => IIRX);
			this.AttachDelegate("IIRY", () => IIRY);
		}
	}
}
