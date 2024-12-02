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
		internal double[] x, y;                 // plot samples
		private ushort i, work;					// arrays currently being sampled
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
				IIRX = IIRY = i = 0;
				VM.xmin[0] = VM.xmin[1] = VM.ymin[0] = VM.ymin[1] = VM.xmax[0] = VM.ymax[0]
						   = VM.xmax[1] = VM.ymax[1] = VM.Total = 0;
		   		VM.Title = pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
						 + ":  " + pluginManager.GetPropertyValue("DataCorePlugin.GameData.CarModel")?.ToString()
						 + "@" + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackName")?.ToString();
				VM.length = (ushort)(2 == VM.Refresh ? 0 : 180);
			}

			IIRX += (xf - IIRX) / VM.FilterX;
			IIRY += (yf - IIRY) / VM.FilterY;
			x[i] = IIRX;									// why can DataUpdate() set View values??
			y[i] = IIRY;

			if (VM.start[work] == i)
			{
				VM.xmin[work] = VM.xmax[work] = x[i];
				VM.ymin[work] = VM.ymax[work] = y[i];
			}
			else	// volume of sample values
			{
				if (VM.xmin[work] > x[i])
					VM.xmin[work] = x[i];
				else if (VM.xmax[work] < x[i])
					VM.xmax[work] = x[i];

				if (VM.ymax[work] < y[i])
					VM.ymax[work] = y[i];
				else if (VM.ymin[work] > y[i])
					VM.ymin[work] = y[i];
			}
			if (2 == VM.Refresh)
			{
				if (1 < (double)pluginManager.GetPropertyValue("DataCorePlugin.GameData.SpeedKmh"))
					Accrue();
				return;
			}

			if ((++i - VM.start[work]) >= VM.length)	// filled?
			{
				VM.Current = $"{VM.xmin[work]:#0.000} <= X <= "
								   + $"{VM.xmax[work]:#0.000};  "
								   + $"{VM.ymin[work]:#0.000} <= Y <= "
								   + $"{VM.ymax[work]:#0.000}";
				// Refresh: 0 = max range, 1 = 3 second, 2 = cumulative range
				if ( 1 == VM.Refresh || (VM.Done && 2 == VM.Refresh)
				 || (0 == VM.Refresh && (VM.xmax[work] - VM.xmin[work]) > VM.Range))
				{
					View.Dispatcher.Invoke(() => View.Replot(work));
					work = (ushort)(1 - work);					// switch buffers
				}
				i = VM.start[work];
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
			VM = View.M;
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
