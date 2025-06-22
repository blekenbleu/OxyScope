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
		/// Gets axis short plugin title to show in left menu.
		/// Return null if you want to use the title as defined in PluginName attribute.
		/// </summary>
		public string LeftMenuTitle => "OxyScope " + PluginVersion;

		PluginManager PM;
        bool ValidateProp(int i, string prop)
		{
			var yp = PM.GetPropertyValue(prop);
			VM.axis[i] = null != yp && float.TryParse(yp.ToString(), out f[i]);
			if (!VM.axis[i])
			{
				string foo = 3 > i ? "Y" + i : "X";

                VM.XYprop2 += $"invalid {foo} property:  " + prop;
				oops = true;
			}
			return VM.axis[i];
		}

		/// <summary>
		/// Called one time per game data update, contains all normalized game data,
		/// raw data are intentionally "hidden" under axis generic object type
		/// (A plugin SHOULD NOT USE IT)
		///
		/// This method is on the critical path; execute as fast as possible,
		/// avoid throwing any error
		///
		/// </summary>
		/// <param name="pluginManager"></param>
		/// <param name="data">Current game data, including current and previous data frame.</param>
		readonly float[] f = { 0, 0, 0, 0 };		// float values from properties
		readonly double[] IIR = { 0, 0, 0, 0 };		// filtered property values from f[]
		internal double[,] x;						// plot samples from IIR[]
		private ushort work;						// arrays currently being sampled
		private ushort timeout;						// for Accrue
		bool oops = false;
		int m = 0;									// current LinFit
		string CarId = "";
		double current;
		public void DataUpdate(PluginManager pluginManager, ref GameData data)
		{
			if (!data.GameRunning || null == data.OldData || null == data.NewData)
				return;

			if (current == data.NewData.CarSettings_CurrentDisplayedRPMPercent)
				return;
			current = data.NewData.CarSettings_CurrentDisplayedRPMPercent;

            PM = pluginManager;
			VM.XYprop2 = "";
            if (!ValidateProp(0, VM.Y0prop) || !ValidateProp(3, VM.Xprop))
				return;

			ValidateProp(1, VM.Y1prop);
			ValidateProp(2, VM.Y2prop);

			if (oops)
			{
				oops = false;
				VM.XYprop2 += ";  continuing...";
			}

			if (0 == data.NewData.CarId.Length)
			{
				VM.XYprop1 = "waiting for CarId...";
				return;
			}

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
				restart = true;				// restart Accrue()
				for (int i = 0; i < 4; i++)
					IIR[i] = f[i];
				VM.start[work] = VM.I;
				m = (0 == VM.LinFit) ? 0 : VM.LinFit - 1;
			} else {	// check for redundant samples
			  	if (1 > (double)pluginManager.GetPropertyValue("DataCorePlugin.GameData.SpeedKmh")
				 || VM.I >= 1200)
					return; 	// Restart sample may have been before car moved

				int i;
				for (i = 0; i < 3; i++)
					if(VM.axis[i] && System.Math.Abs(f[i] - x[i,VM.I]) > 0.02 * (VM.max[i,work] - VM.min[i,work]))
						break;
				if (3 == i)		// differed from previous by < 2% ?
					return;
			}

			for (int i = 0; i < 4; i++)
				if (VM.axis[i])
				{
					IIR[i] += (f[i] - IIR[i]) / VM.FilterX;
					x[i,VM.I] = IIR[i];
					if (VM.start[work] == VM.I)
						VM.min[i,work] = VM.max[i,work] = x[i,VM.I];
					else if (VM.min[i,work] > x[i,VM.I])	// volume of sample values
						VM.min[i,work] = x[i,VM.I];
					else if (VM.max[i,work] < x[i,VM.I])
						VM.max[i,work] = x[i,VM.I];
				}

			if (2 == VM.Refresh)
			{
				if (VM.I < x.Length)
					Accrue();
			}
			else if ((++VM.I - VM.start[work]) >= VM.length)	// filled?
			{
				VM.Current = $"{VM.min[m,work]:#0.000} <= Y <= {VM.max[m,work]:#0.000};  "
						   + $"{VM.min[3,work]:#0.000} <= X <= {VM.max[3,work]:#0.000}";
				// Refresh: 0 = max range, 1 = 3 second, 2 = cumulative range
				if ( 1 == VM.Refresh
				 || (0 == VM.Refresh && (VM.max[m,work] - VM.min[m,work]) > VM.Range))
				{
					View.Dispatcher.Invoke(() => View.Replot(work));
					work = (ushort)(1 - work);					// switch buffers
				}
				VM.I = VM.start[work];
			}
		}														// DataUpdate()

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
			VM = Control.M;
			return View;
		}

		public Settings Settings;
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
		/// Called one time after plugin instance
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
					Y0prop = where+sy,
					Y1prop = "", Y2prop = "",
					Xprop = where+sx,
					FilterX = 1, FilterY = 1, Refresh = 1, LinFit = 1
				};
			else {
				if (0 == Settings.Y0prop.Length)
						Settings.Y0prop = where+sy;
				if (null == Settings.Y1prop)
					Settings.Y1prop = "";
				if (null == Settings.Y2prop)
					Settings.Y2prop = "";
				if (0 == Settings.Xprop.Length)
						Settings.Xprop = where+sx;
				if (1 > Settings.FilterX)
					Settings.FilterX = 1;
				if (1 > Settings.FilterY)
					Settings.FilterY = 1;
			}

			this.AttachDelegate("IIRY0", () => IIR[0]);
			this.AttachDelegate("IIRY1", () => IIR[1]);
			this.AttachDelegate("IIRY2", () => IIR[2]);
			this.AttachDelegate("IIRX",	 () => IIR[3]);
		}
	}
}
