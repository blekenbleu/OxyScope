using GameReaderCommon;
using SimHub.Plugins;
using SimHub.Plugins.DataPlugins.ShakeItV3;
using System;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace OxyPlotPlugin
{
	[PluginDescription("XY OxyPlot of paired SimHub properties")]
	[PluginAuthor("blekenbleu")]
	[PluginName("OxyPlot XY plugin")]
	public class Plugin : IPlugin, IDataPlugin, IWPFSettingsV2
	{
		public Settings Settings;
		public bool running = false;

		/// <summary>
		/// Instance of the current plugin manager
		/// </summary>
		public PluginManager PluginManager { get; set; }

		/// <summary>
		/// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
		/// </summary>
		public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

		/// <summary>
		/// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
		/// </summary>
		public string LeftMenuTitle => "OxyPlot XY plugin";

		/// <summary>
		/// Called one time per game data update, contains all normalized game data,
		/// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
		///
		/// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
		///
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <param name="data">Current game data, including current and previous data frame.</param>
		private int i, work;
		public int which;	// which x and y array for OxyPlot to use
		public double[] xmin, xmax, ymax;	// View uses ymax for Y axis scaling

		public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			// Search for X-axis sample batches satisfying < View.lowval && > View.minval
			if (data.GameRunning && data.OldData != null && data.NewData != null)
			{
				float xf = 0, yf = 0;
				bool fail = false;
				var xp = pluginManager.GetPropertyValue(Settings.X);
				if (null == xp || !float.TryParse(xp.ToString(), out xf))
				{
					View.VMod.XYprop = "invalid X property:  " + Settings.X;
					fail = true;
				}
				var yp = pluginManager.GetPropertyValue(Settings.Y);
				if (null == yp || !float.TryParse(yp.ToString(), out yf))
				{
					View.VMod.XYprop = "invalid Y property:  " + Settings.Y;
					fail = true;
				}
				if (fail)
				{
					View.VMod.Vis = System.Windows.Visibility.Visible;
					return;
				}

				if (1 > xf)
					return;

				View.x[i] = xf;
				View.y[i] = yf;
				if (View.start[work] == i)
				{
					xmin[work] = xmax[work] = 0.5 * (View.lowval + View.minval); // View.x[i];
				}
				else	// volume of sample values
				{
					if (xmin[work] > View.x[i])
						xmin[work] = View.x[i];
					else if (xmax[work] < View.x[i])
						xmax[work] = View.x[i];
					if (ymax[work] < View.y[i])
						ymax[work] = View.y[i];
				}
				if ((++i - View.start[work]) >= View.length >> 1)	// filled?
				{
					int n = 1 - work;

					// Coordination:  View should disable running while loading Plot
					if (running)
					{
						View.VMod.XYprop = "working..." + " current high = " + xmax[work] + ", low = " + xmin[work];
						if (View.minval < xmax[work])
						{
//							View.VMod.XYprop += " current high = " + xmax[work];
							if (View.lowval > xmin[work])
							{
//								View.VMod.XYprop += ", low = " + xmin[work];
								if ((xmax[work] - xmin[work]) > (xmax[n] - xmin[n]))
								{	// larger sample volume than in [1 - work]
									which = work;		// plot this buffer
									View.VMod.Vis = System.Windows.Visibility.Visible;
									work = n;			// refill buffer with smaller range
								}
							}
						}
					}
					i = View.start[work];
					ymax[work] = 0;
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
			Settings.Min = View.minval;
			Settings.Low = View.lowval;
			Settings.X = View.Xprop.Text;
			Settings.Y = View.Yprop.Text;
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
			i = which = work = 0;	ymax = new double[] {0, 0};
			xmin = new double[] {90, 90}; xmax = new double[] {0, 0};
			SimHub.Logging.Current.Info("Starting " + LeftMenuTitle);

			// Load settings
			Settings = this.ReadCommonSettings<Settings>("GeneralSettings", () => new Settings());
			if (null == Settings)
				Settings = new Settings() {
					Low = 3, Min = 30,
					X = "ShakeITBSV3Plugin.Export.ProxyS.FrontLeft",
					Y = "ShakeITBSV3Plugin.Export.Grip.FrontLeft"
				};
			else {
				if (0 == Settings.X.Length)
						Settings.X = "ShakeITBSV3Plugin.Export.ProxyS.FrontLeft";
				if (0 == Settings.Y.Length)
						Settings.Y = "ShakeITBSV3Plugin.Export.Grip.FrontLeft";
			}

			// Declare an action which can be called
			this.AddAction("ChangeProperties", (a, b) =>
			{
				running = true;
                View.VMod.Title =
					pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
					+ ":  " + pluginManager.GetPropertyValue("CarID")?.ToString()
					+ "@" + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackId")?.ToString();
				View.Dispatcher.Invoke(() => View.ScatterPlot(which));	// invoke from another thread
				View.VMod.XYprop = "property updates waiting...";
			});
		}
	}
}
