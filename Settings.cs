namespace blekenbleu.OxyScope
{
	/// <summary>
	/// Settings class, make sure it can be correctly serialized using JSON.net
	/// </summary>
	public class Settings
	{
		public double FilterX = 1, FilterY = 1;
		public bool Refresh = true, LinFit = true, Replot = true;
		public string Xprop = "DataCorePlugin.GameData.AccelerationHeave",
					  Yprop = "DataCorePlugin.GameData.GlobalAccelerationG";
	}
}
