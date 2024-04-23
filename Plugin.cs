using GameReaderCommon;
using SimHub.Plugins;
using SimHub.Plugins.DataPlugins.ShakeItV3;
using System;
using System.Windows.Media;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace OxyPlotPlugin
{
	[PluginDescription("An XY OxyPlot of 2 SimHub properties")]
	[PluginAuthor("blekenbleu")]
	[PluginName("OxyPlot plugin")]
	public class Plugin : IPlugin, IDataPlugin, IWPFSettingsV2
	{
		public Settings Settings;
		internal string gname = "", cname="";

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
		public string LeftMenuTitle => "OxyPlot plugin";

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
		public double[] xmin, xmax, ymin, ymax;
        public string xprop = "ShakeITBSV3Plugin.Export.ProxyS.FrontLeft";
        public string yprop = "ShakeITBSV3Plugin.Export.Grip.FrontLeft";

        public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			// Define the value of our property (declared in init)
			if (data.GameRunning)
			{
				if (data.OldData != null && data.NewData != null)
				{
					float xf = 0, yf = 0;
					bool fail = false;
					var xp = pluginManager.GetPropertyValue(xprop);
					if (null == xp || !float.TryParse(xp.ToString(), out xf))
					{
						that.PluginViewModel.XYprop = "invalid X property:  " + xprop;
						fail = true;
                    }
					var yp = pluginManager.GetPropertyValue(yprop);
					if (null == yp || !float.TryParse(yp.ToString(), out yf))
                    {
						that.PluginViewModel.XYprop = "invalid Y property:  " + yprop;
						fail = true;
                    }
					if (fail)
						return;

					that.PluginViewModel.XYprop = "working...";
                    that.x[work][i] = xf;
                    that.y[work][i] = yf;
					if (0 == i)
					{
						xmin[work] = xmax[work] = that.x[work][i];
						ymin[work] = ymax[work] = that.y[work][i];
					}
					else	// volume of sample values
					{
						if (xmin[work] > that.x[work][i])
							xmin[work] = that.x[work][i];
						else if (xmax[work] < that.x[work][i])
							xmax[work] = that.x[work][i];
						if (ymin[work] > that.y[work][i])
							ymin[work] = that.y[work][i];
						else if (ymax[work] < that.y[work][i])
							ymax[work] = that.y[work][i];
					}
					if (++i >= that.y[which].Length)	// filled?
					{
						i = 0;
						int n = 1 - work;

						if (that.lowval > xmin[work] && that.minval < xmax[work]
							&& (xmax[work] - xmin[work]) > (xmax[n] - xmin[n]))
//						if ((ymax[work] - ymin[work]) * (xmax[work] - xmin[work]) >
//							(ymax[n] - ymin[n]) * (xmax[n] - xmin[n]))
						{	// larger sample volume than in [1 - work]
							which = work;		// plot this buffer
							that.PluginViewModel.OxyButVis = System.Windows.Visibility.Visible;
							that.PluginViewModel.YXclick = "click";
							work = n;	// refill buffer with smaller range
						}
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
			// Save settings
			this.SaveCommonSettings("GeneralSettings", Settings);
		}

		/// <summary>
		/// Returns the settings control, return null if no settings control is required
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <returns></returns>
		private Control that;
		public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
		{
			return that = new Control(this);
		}

		/// <summary>
		/// Called once after plugins startup
		/// Plugins are rebuilt at game change
		/// </summary>
		/// <param name="pluginManager"></param>
		public void Init(PluginManager pluginManager)
        {
			i = which = work = 0;
			xmin = new double[] {0, 0}; xmax = new double[] {0, 0};
			ymin = new double[] {0, 0}; ymax = new double[] {0, 0};
			SimHub.Logging.Current.Info("Starting " + LeftMenuTitle);

            // Load settings
            Settings = this.ReadCommonSettings<Settings>("GeneralSettings", () => new Settings());

			// Declare an action which can be called
			this.AddAction("ChangeProperties", (a, b) =>
			{
                that.variables = "Grip.FrontLeft vs ProxyS.FrontLeft";
				gname = pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString();
				cname = pluginManager.GetPropertyValue("CarID")?.ToString();
			});
		}
	}
}
