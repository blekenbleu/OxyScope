namespace blekenbleu.OxyScope
{
	/// <summary>
	/// Settings class, make sure it can be correctly serialized using JSON.net
	/// </summary>
	public class Settings
	{
		public double FilterX = 1, FilterY = 1;
		public uint Refresh = 0;
		public bool LinFit = true, Plot = true;
		public string Xprop = "DataCorePlugin.GameData.AccelerationHeave",
					  Yprop = "DataCorePlugin.GameData.GlobalAccelerationG";
	}
}
