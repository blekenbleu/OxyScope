using GameReaderCommon;
using SimHub.Plugins;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;

namespace OxyPlotPlugin
{
	[PluginDescription("XY OxyPlot of paired SimHub properties")]
	[PluginAuthor("blekenbleu")]
	[PluginName("OxyPlot XY")]
	public partial class OxyScope : IPlugin, IDataPlugin, IWPFSettingsV2
	{
		public Settings Settings;
		string CarId = "";
		public bool running = false;
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
		/// raw data are intentionally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
		///
		/// This method is on the critical path; execute as fast as possible, avoid throwing any error
		///
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <param name="data">Current game data, including current and previous data frame.</param>
		double IIRX = 0, IIRY = 0;
		private int i, work;
		public int which;					// which x and y array to plot
		public double[] xmin, ymin, xmax, ymax; // View uses, xmax, ymax for axes scaling
		bool oops = false;
		public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			// Search for X-axis sample batches satisfying < View.lowX && > View.lowY
			if (data.GameRunning && data.OldData != null && data.NewData != null)
			{
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

				if (CarId != data.NewData.CarId)
				{
					CarId = data.NewData.CarId;
					running = true;
               		View.Model.Title =
                    	pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
                	    + ":  " + pluginManager.GetPropertyValue("CarID")?.ToString()
                    	+ "@" + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackId")?.ToString();  
				}

				if (View.Model.ThresVal > xf || 0 > yf)
					return;

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
					int n = 1 - work;

					// Coordination:  View should disable running while loading Plot
					if (running)
					{
						View.Model.XYprop = $"current X high = {xmax[work]:#0.000}"
										  + $", low = {xmin[work]:#0.000}"
										  + $";  current Y high = {ymax[work]:#0.000}"
										  + $", low = {ymin[work]:#0.000}";
					}
					if ((xmax[work] - xmin[work]) > (xmax[n] - xmin[n])
					  && xmin[work] <= xmax[work] * 0.01 * Settings.Low
					  && ymin[work] <= ymax[work] * 0.01 * Settings.Min)
					{	// larger sample volume than in [1 - work]
						which = work;		// plot this buffer
						if (View.Model.Replot)
							View.Dispatcher.Invoke(() => View.Replot());
						else View.Model.RVis = System.Windows.Visibility.Visible;
						work = n;			// refill buffer with smaller range
					}
					i = View.start[work];
				}
			}
		}

		/// <summary>
		/// Called at plugin manager stop, close/dispose anything needed here !
		/// Plugins are rebuilt at game change
		/// </summary>
		/// <param name="pluginManager"></param>
		public void End(PluginManager pluginManager)
		{
			Settings.Min = View.lowY;
			Settings.Low = View.lowX;
			Settings.ThresBool = View.Model.ThresBool;
			Settings.LinFit = View.Model.LinFit;
			View.Model.ThresBool = true;				// set true to get ThresVal
			Settings.ThresVal = View.Model.ThresVal;
			Settings.FilterX = View.Model.FilterX;
			Settings.FilterY = View.Model.FilterY;
			Settings.X = View.Model.Xprop;
			Settings.Y = View.Model.Yprop;
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
		/// Called once after plugins startup
		/// Plugins are rebuilt at game change
		/// </summary>
		/// <param name="pluginManager"></param>
		public void Init(PluginManager pluginManager)
		{
			i = which = work = 0;
			ymin = new double[] {90, 90}; ymax = new double[] {0, 0};
			xmin = new double[] {90, 90}; xmax = new double[] {0, 0};
			SimHub.Logging.Current.Info("Starting " + LeftMenuTitle);

			// Load settings
			Settings = this.ReadCommonSettings<Settings>("GeneralSettings", () => new Settings());
			if (null == Settings)
				Settings = new Settings() {
					Low = 3, Min = 30, ThresVal = 99,
					X = "ShakeITBSV3Plugin.Export.ProxyS.FrontLeft",
					Y = "ShakeITBSV3Plugin.Export.Grip.FrontLeft",
					FilterX = 1, FilterY = 1
				};
			else {
				if (0 == Settings.X.Length)
						Settings.X = "ShakeITBSV3Plugin.Export.ProxyS.FrontLeft";
				if (0 == Settings.Y.Length)
						Settings.Y = "ShakeITBSV3Plugin.Export.Grip.FrontLeft";
				if (1 > Settings.FilterX)
					Settings.FilterX = 1;
                if (1 > Settings.FilterY)
                    Settings.FilterY = 1;
            }

			this.AttachDelegate("Xprop", () => View.Model.Xprop);
			this.AttachDelegate("Yprop", () => View.Model.Yprop);
			this.AttachDelegate("IIRX", () => IIRX);
			this.AttachDelegate("IIRY", () => IIRY);
			this.AttachDelegate("current", () => i);
			// Declare an action which can be called
			this.AddAction("ChangeProperties", (a, b) =>
			{
				running = true;
                View.Model.Title =
					pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
					+ ":  " + pluginManager.GetPropertyValue("CarID")?.ToString()
					+ "@" + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackId")?.ToString();
//				View.Dispatcher.Invoke(() => View.Replot());	// invoke from another thread
				View.Model.XYprop = "property updates waiting...";
			});
		}
	}
}
