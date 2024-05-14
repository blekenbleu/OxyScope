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
		private int i, j, work;
		public int which;	// which x and y array for OxyPlot to use
		public double[] xmin, xmax, ymin, ymax;

		public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			// Define the value of our property (declared in init)
			if (data.GameRunning && data.OldData != null && data.NewData != null)
			{
				float xf = 0, yf = 0;
				bool fail = false;
				var xp = pluginManager.GetPropertyValue(Settings.X);
				if (null == xp || !float.TryParse(xp.ToString(), out xf))
				{
					View.Model.XYprop = "invalid X property:  " + Settings.X;
					fail = true;
				}
				var yp = pluginManager.GetPropertyValue(Settings.Y);
				if (null == yp || !float.TryParse(yp.ToString(), out yf))
				{
					View.Model.XYprop = "invalid Y property:  " + Settings.Y;
					fail = true;
				}
				if (fail)
				{
					View.Model.OxyButVis = System.Windows.Visibility.Visible;
					return;
				}

				if (running)
					View.Model.XYprop = "working...";
				View.x[j + i] = xf;
				View.y[j + i] = yf;
				if (0 == i)
				{
					xmin[work] = xmax[work] = View.x[j];
					ymin[work] = ymax[work] = View.y[j];
				}
				else	// volume of sample values
				{
					if (xmin[work] > View.x[j + i])
						xmin[work] = View.x[j + i];
					else if (xmax[work] < View.x[j + i])
						xmax[work] = View.x[j + i];
					if (ymin[work] > View.y[j + i])
						ymin[work] = View.y[j + i];
					else if (ymax[work] < View.y[j + i])
						ymax[work] = View.y[j + i];
				}
				if (++i >= View.length >> 1)	// filled?
				{
					i = 0;
					int n = 1 - work;

					if (View.lowval > xmin[work] && View.minval < xmax[work]
						&& (xmax[work] - xmin[work]) > (xmax[n] - xmin[n]))
//					if ((ymax[work] - ymin[work]) * (xmax[work] - xmin[work]) >
//						(ymax[n] - ymin[n]) * (xmax[n] - xmin[n]))
					{	// larger sample volume than in [1 - work]
						which = work;		// plot this buffer
						View.Model.OxyButVis = System.Windows.Visibility.Visible;
						work = n;			// refill buffer with smaller range
						j = View.start[work];
					}
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
			i = j = which = work = 0;
			xmin = new double[] {0, 0}; xmax = new double[] {0, 0};
			ymin = new double[] {0, 0}; ymax = new double[] {0, 0};
			SimHub.Logging.Current.Info("Starting " + LeftMenuTitle);

			// Load settings
			Settings = this.ReadCommonSettings<Settings>("GeneralSettings", () => new Settings());
			if (null == Settings)
				Settings = new Settings() {
					Low = 3, Min = 30,
					X = "ShakeITBSV3Plugin.Export.ProxyS.FrontLeft",
					Y = "ShakeITBSV3Plugin.Export.Grip.FrontLeft"
				};

			// Declare an action which can be called
			this.AddAction("ChangeProperties", (a, b) =>
			{
                View.Model.Title =
					pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
					+ ":  " + pluginManager.GetPropertyValue("CarID")?.ToString()
					+ "@" + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackId")?.ToString();
			});
		}
	}
}
