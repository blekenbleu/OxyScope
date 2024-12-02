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
		double IIRX = 0, IIRY = 0;				// filtered property values
		private ushort i, work;					// arrays currently being sampled
		public double[] xmin, ymin, xmax, ymax; // View uses for axes scaling
		bool oops = false;
		public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			if (!data.GameRunning || null == data.OldData || null == data.NewData)
				return;

			var xp = pluginManager.GetPropertyValue(VM.Xprop);

			if (null == xp || !float.TryParse(xp.ToString(), out float xf))
			{
				VM.XYprop = "invalid X property:  " + VM.Xprop;
				oops = true;
				return;
			}

			var yp = pluginManager.GetPropertyValue(VM.Yprop);
			if (null == yp || !float.TryParse(yp.ToString(), out float yf))
			{
				VM.XYprop = "invalid Y property:  " + VM.Yprop;
				oops = true;
				return;
			}

			if (oops)
			{
				oops = false;
				VM.XYprop = "continuing...";
			}

			if (0 == data.NewData.CarId.Length)
				return;

			if (CarId != data.NewData.CarId)
			{
				CarId = data.NewData.CarId;
				IIRX = IIRY = 0;
				xmin[0] = xmin[1] = ymin[0] = ymin[1] = xmax[0] = ymax[0] = xmax[1] = ymax[1] = 0;
		   		VM.Title =
						pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
						+ ":  " + pluginManager.GetPropertyValue("DataCorePlugin.GameData.CarModel")?.ToString()
						+ "@" + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackName")?.ToString();  
			}

			IIRX += (xf - IIRX) / VM.FilterX;
			IIRY += (yf - IIRY) / VM.FilterY;
			View.x[i] = IIRX;									// why can DataUpdate() set View values??
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
				VM.Current = $"{xmin[work]:#0.000} <= X <= "
								   + $"{xmax[work]:#0.000};  "
								   + $"{ymin[work]:#0.000} <= Y <= "
								   + $"{ymax[work]:#0.000}";
				// Refresh: 0 = max range, 1 = 3 second, 2 = cumulative range
				if ( 1 == VM.Refresh || (VM.Done && 2 == VM.Refresh)
				 || (0 == VM.Refresh && (xmax[work] - xmin[work]) > VM.Range))
				{
					View.Dispatcher.Invoke(() => View.Replot(work));
					work = (ushort)(1 - work);					// switch buffers
				}
				i = View.start[work];
			}
		}														// DataUpdate()

		/// <summary>
		/// Called at plugin manager stop, close/dispose anything needed here !
		/// Plugins are rebuilt at game change
		/// </summary>
		/// <param name="pluginManager"></param>
		public void End(PluginManager pluginManager)
		{
			Settings.FilterX = VM.FilterX;
			Settings.FilterY = VM.FilterY;
			Settings.Xprop = VM.Xprop;
			Settings.Yprop = VM.Yprop;
			Settings.LinFit = VM.LinFit;
			Settings.Refresh = VM.Refresh;
			Settings.Plot = VM.AutoPlot;
			// Save settings
			this.SaveCommonSettings("GeneralSettings", Settings);
		}

		/// <summary>
		/// Returns UI control instance or null if not required
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <returns></returns>
		Control View;
		Model VM;
		public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
		{
			View = new Control(this);
			VM = View.Model;
			return View;
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

			ymin = new double[] {0, 0}; ymax = new double[] {0, 0};
			xmin = new double[] {0, 0}; xmax = new double[] {0, 0};
			SimHub.Logging.Current.Info("Starting " + LeftMenuTitle);

			// Load settings
			Settings = this.ReadCommonSettings<Settings>("GeneralSettings", () => new Settings());
			if (null == Settings)
				Settings = new Settings() {
					Xprop = where+x,
					Yprop = where+y,
					FilterX = 1, FilterY = 1, Refresh = 1, LinFit = true
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

			this.AttachDelegate("IIRX", () => IIRX);
			this.AttachDelegate("IIRY", () => IIRY);
		}
	}
}
