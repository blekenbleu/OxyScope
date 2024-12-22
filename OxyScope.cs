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
		double IIRX = 0, IIRX1 = 0, IIRX2 = 0, IIRY = 0;	// filtered property values
		internal double[,] x;								// plot samples
		internal double[] y;								// plot samples
		private ushort work, timeout;						// arrays currently being sampled
		float xf1 = 0, xf2 = 0;
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

			if (0 < VM.Xprop1.Length)
			{
				var xp1 = pluginManager.GetPropertyValue(VM.Xprop1);
				if (null == xp1 || !float.TryParse(xp1.ToString(), out xf1))
				{
					VM.XYprop = "invalid X1 property:  " + VM.Xprop1;
					oops = true;
					VM.X1 = false;
					return;
				}
				VM.X1 = true;
			}
			if (0 < VM.Xprop2.Length)
			{
				var xp2 = pluginManager.GetPropertyValue(VM.Xprop2);
				if (null == xp2 || !float.TryParse(xp2.ToString(), out xf2))
				{
					VM.XYprop = "invalid X2 property:  " + VM.Xprop2;
					oops = true;
                    VM.X2 = false;
					return;
				}
				VM.X2 = true;
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
				// implicit Reset
		   		VM.Title = pluginManager.GetPropertyValue("DataCorePlugin.CurrentGame")?.ToString()
						 + ":  " + pluginManager.GetPropertyValue("DataCorePlugin.GameData.CarModel")?.ToString()
						 + "@"	 + pluginManager.GetPropertyValue("DataCorePlugin.GameData.TrackName")?.ToString();
			}

			if (VM.Restart)
			{
				VM.Restart = false;
				IIRY = yf;
				IIRX = xf;
				IIRX1 = xf1;
				IIRX2 = xf2;
				VM.start[work] = VM.I;
			}

			IIRX += (xf - IIRX) / VM.FilterX;
			IIRX1 += (xf1 - IIRX1) / VM.FilterX;
			IIRX2 += (xf2 - IIRX2) / VM.FilterX;
			IIRY += (yf - IIRY) / VM.FilterY;
			x[0,VM.I] = IIRX;
			if (VM.X1)
				x[1,VM.I] = IIRX1;
			if (VM.X2)
				x[2,VM.I] = IIRX2;
			y[VM.I] = IIRY;

			if (VM.start[work] == VM.I)
			{
				VM.xmin[0,work] = VM.xmax[0,work] = x[0,VM.I];
				if (VM.X1)
					VM.xmin[1,work] = VM.xmax[1,work] = x[1,VM.I];
				if (VM.X2)
					VM.xmin[2,work] = VM.xmax[2,work] = x[2,VM.I];
				VM.ymin[work] = VM.ymax[work] = y[VM.I];
			}
			else	// volume of sample values
			{
				if (VM.xmin[0,work] > x[0,VM.I])
					VM.xmin[0,work] = x[0,VM.I];
				else if (VM.xmax[0,work] < x[0,VM.I])
					VM.xmax[0,work] = x[0,VM.I];
				if (VM.X1)
				{
					if (VM.xmin[1,work] > x[1,VM.I])
						VM.xmin[1,work] = x[1,VM.I];
					else if (VM.xmax[1,work] < x[1,VM.I])
						VM.xmax[1,work] = x[1,VM.I];
				}
				if (VM.X2)
				{
					if (VM.xmin[2,work] > x[2,VM.I])
						VM.xmin[2,work] = x[2,VM.I];
					else if (VM.xmax[2,work] < x[2,VM.I])
						VM.xmax[2,work] = x[2,VM.I];
				}
				if (VM.ymax[work] < y[VM.I])
					VM.ymax[work] = y[VM.I];
				else if (VM.ymin[work] > y[VM.I])
					VM.ymin[work] = y[VM.I];
			}

			if (2 == VM.Refresh)
			{
				if (1 < (double)pluginManager.GetPropertyValue("DataCorePlugin.GameData.SpeedKmh"))
					Accrue();
			}

			else if ((++VM.I - VM.start[work]) >= VM.length)	// filled?
			{
				VM.Current = $"{VM.xmin[0,work]:#0.000} <= X <= {VM.xmax[0,work]:#0.000};  "
						   + $"{VM.ymin[work]:#0.000} <= Y <= {VM.ymax[work]:#0.000}";
				// Refresh: 0 = max range, 1 = 3 second, 2 = cumulative range
				if ( 1 == VM.Refresh
				 || (0 == VM.Refresh && (VM.xmax[0,work] - VM.xmin[0,work]) > VM.Range))
				{
					View.Dispatcher.Invoke(() => View.Replot(work));
					work = (ushort)(1 - work);					// switch buffers
				}
				VM.I = VM.start[work];
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
			Settings.Xprop1 = VM.Xprop1;
			Settings.Xprop2 = VM.Xprop2;
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
					sx = "AccelerationHeave", sy = "GlobalAccelerationG";

			SimHub.Logging.Current.Info("Starting " + LeftMenuTitle);

			// Load settings
			Settings = this.ReadCommonSettings<Settings>("GeneralSettings", () => new Settings());
			if (null == Settings)
				Settings = new Settings() {
					Xprop = where+sx,
					Xprop1 = "", Xprop2 = "",
					Yprop = where+sy,
					FilterX = 1, FilterY = 1, Refresh = 1, LinFit = true
				};
			else {
				if (0 == Settings.Xprop.Length)
						Settings.Xprop = where+x;
				if (null == Settings.Xprop1)
					Settings.Xprop1 = "";
				if (null == Settings.Xprop2)
					Settings.Xprop2 = "";
				if (0 == Settings.Yprop.Length)
						Settings.Yprop = where+y;
				if (1 > Settings.FilterX)
					Settings.FilterX = 1;
				if (1 > Settings.FilterY)
					Settings.FilterY = 1;
			}

			this.AttachDelegate("IIRX", () => IIRX);
			this.AttachDelegate("IIRX1", () => IIRX1);
			this.AttachDelegate("IIRX2", () => IIRX2);
			this.AttachDelegate("IIRY", () => IIRY);
		}
	}
}
