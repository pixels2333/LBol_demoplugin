using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LBoL.Core.Cards
{
	public class Guns
	{
		private bool HasGun
		{
			get
			{
				List<string> gunNames = this._gunNames;
				return gunNames != null && gunNames.Count > 0;
			}
		}
		public int Count
		{
			get
			{
				if (this.HasGun)
				{
					return this._gunNames.Count;
				}
				return 0;
			}
		}
		public List<GunPair> GunPairs
		{
			get
			{
				if (this.HasGun)
				{
					List<GunPair> list = Enumerable.ToList<GunPair>(Enumerable.Select<string, GunPair>(this._gunNames, (string gun) => new GunPair(gun, GunType.Middle)));
					int count = list.Count;
					if (count <= 1)
					{
						if (count == 1)
						{
							Enumerable.First<GunPair>(list).GunType = GunType.Single;
						}
					}
					else
					{
						Enumerable.First<GunPair>(list).GunType = GunType.First;
						Enumerable.Last<GunPair>(list).GunType = GunType.Last;
					}
					return list;
				}
				return null;
			}
		}
		public Guns()
		{
			this._gunNames = new List<string>();
		}
		public Guns(string gunName)
		{
			List<string> list = new List<string>();
			list.Add(gunName);
			this._gunNames = list;
		}
		public Guns(string gunName, int times, bool multiGun = true)
		{
			if (times < 1)
			{
				Debug.LogError("Creating Guns with times blow 0.");
			}
			List<string> list = new List<string>();
			list.Add(gunName);
			this._gunNames = list;
			for (int i = 1; i < times; i++)
			{
				this._gunNames.Add(multiGun ? gunName : "Instant");
			}
		}
		public Guns(IEnumerable<string> gunNames)
		{
			this._gunNames = new List<string>();
			foreach (string text in gunNames)
			{
				this._gunNames.Add(text);
			}
		}
		public void Add(string gunName)
		{
			this._gunNames.Add(gunName);
		}
		public void Add(string gunName, int times)
		{
			for (int i = 0; i < times; i++)
			{
				this._gunNames.Add(gunName);
			}
		}
		private readonly List<string> _gunNames;
	}
}
