using SimHub.Plugins;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;

namespace blekenbleu.OxyScope
{
	[PluginDescription("scatter Vplot one SimHub property vs others")]
	[PluginAuthor("blekenbleu")]
	[PluginName("OxyScope")]
	public partial class OxyScope : IPlugin
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
			Settings.property = VM.property;
			this.SaveCommonSettings("GeneralSettings", Settings);
		}

		/// <summary>
		/// Called one time after plugin instance
		/// Plugins are reinstanced at game change
		/// </summary>
		/// <param name="pluginManager"></param>
		public void Init(PluginManager pluginManager)
		{
			work = 0;													// Init()
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
					FilterX = 1, FilterY = 1, Refresh = 1, property = 3
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

			this.AttachDelegate("backfill",() => backfill);
			this.AttachDelegate("bucket",() => bucket);
			this.AttachDelegate("IIRY0-2,X", () => $"{IIR[0]:#.00},{IIR[1]:#.00},{IIR[2]:#.00},{IIR[3]:#.00}");
			this.AttachDelegate("Ifac",() => Ifac);
			this.AttachDelegate("Imax",() => Imax);
			this.AttachDelegate("Imin",() => Imin);
			this.AttachDelegate("Intervals.Count",() => Intervals.Count);
			this.AttachDelegate("overtime",() => overtime);
			this.AttachDelegate("resume",() => resume);
			this.AttachDelegate("Sample",() => Sample);
			this.AttachDelegate("StdDev",() => $"{StdDev[0]:#0.00},{StdDev[1]:#0.00},{StdDev[2]:#0.00}");
			this.AttachDelegate("StdSample",() => $"{StdSample[0]:#0.00},{StdSample[1]:#0.00},{StdSample[2]:#0.00}");
			this.AttachDelegate("timeout",() => timeout);
		}
	}
}
