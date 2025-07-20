namespace blekenbleu.OxyScope
{
	/// <summary>
	/// Settings class, make sure it can be correctly serialized using JSON.net
	/// elements must be public for Model : INotifyPropertyChanged class
	/// </summary>
	public class Settings
	{
		public bool AutoPlot = true;
		public byte Collect = 1, property = 3;
		public ushort Slength = 60;
		public double FilterX = 1, FilterY = 1, LAscale = 1;
		public string Y0prop = "DataCorePlugin.GameData.AccelerationHeave",
					  Y1prop = "", Y2prop = "",
					  Xprop = "DataCorePlugin.GameData.GlobalAccelerationG";
	}
}
