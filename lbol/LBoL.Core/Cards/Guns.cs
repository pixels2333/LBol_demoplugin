using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LBoL.Core.Cards
{
	// Token: 0x02000130 RID: 304
	public class Guns
	{
		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06000BDC RID: 3036 RVA: 0x000212CC File Offset: 0x0001F4CC
		private bool HasGun
		{
			get
			{
				List<string> gunNames = this._gunNames;
				return gunNames != null && gunNames.Count > 0;
			}
		}

		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x06000BDD RID: 3037 RVA: 0x000212EE File Offset: 0x0001F4EE
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

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x06000BDE RID: 3038 RVA: 0x00021308 File Offset: 0x0001F508
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

		// Token: 0x06000BDF RID: 3039 RVA: 0x00021385 File Offset: 0x0001F585
		public Guns()
		{
			this._gunNames = new List<string>();
		}

		// Token: 0x06000BE0 RID: 3040 RVA: 0x00021398 File Offset: 0x0001F598
		public Guns(string gunName)
		{
			List<string> list = new List<string>();
			list.Add(gunName);
			this._gunNames = list;
		}

		// Token: 0x06000BE1 RID: 3041 RVA: 0x000213B4 File Offset: 0x0001F5B4
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

		// Token: 0x06000BE2 RID: 3042 RVA: 0x0002140C File Offset: 0x0001F60C
		public Guns(IEnumerable<string> gunNames)
		{
			this._gunNames = new List<string>();
			foreach (string text in gunNames)
			{
				this._gunNames.Add(text);
			}
		}

		// Token: 0x06000BE3 RID: 3043 RVA: 0x0002146C File Offset: 0x0001F66C
		public void Add(string gunName)
		{
			this._gunNames.Add(gunName);
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x0002147C File Offset: 0x0001F67C
		public void Add(string gunName, int times)
		{
			for (int i = 0; i < times; i++)
			{
				this._gunNames.Add(gunName);
			}
		}

		// Token: 0x04000560 RID: 1376
		private readonly List<string> _gunNames;
	}
}
