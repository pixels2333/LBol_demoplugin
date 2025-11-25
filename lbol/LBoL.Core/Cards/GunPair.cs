using System;
namespace LBoL.Core.Cards
{
	public class GunPair
	{
		public GunPair(string gunName, GunType gunType)
		{
			this.GunName = gunName;
			this.GunType = gunType;
		}
		public string GunName { get; }
		public GunType GunType { get; set; }
	}
}
