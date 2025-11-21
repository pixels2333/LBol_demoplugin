using System;
using LBoL.Core;
using UnityEngine;

namespace LBoL.Presentation
{
	// Token: 0x0200000D RID: 13
	public class PlatformHandlerRunner : MonoBehaviour
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600015A RID: 346 RVA: 0x000075B6 File Offset: 0x000057B6
		// (set) Token: 0x0600015B RID: 347 RVA: 0x000075BE File Offset: 0x000057BE
		public PlatformHandler PlatformHandler { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600015C RID: 348 RVA: 0x000075C7 File Offset: 0x000057C7
		// (set) Token: 0x0600015D RID: 349 RVA: 0x000075CE File Offset: 0x000057CE
		public static PlatformHandlerRunner Instance { get; private set; }

		// Token: 0x0600015E RID: 350 RVA: 0x000075D6 File Offset: 0x000057D6
		private void Awake()
		{
			PlatformHandlerRunner.Instance = this;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x000075DE File Offset: 0x000057DE
		private void OnDestroy()
		{
			PlatformHandler platformHandler = this.PlatformHandler;
			if (platformHandler == null)
			{
				return;
			}
			platformHandler.Shutdown();
		}

		// Token: 0x06000160 RID: 352 RVA: 0x000075F0 File Offset: 0x000057F0
		private void Update()
		{
			PlatformHandler platformHandler = this.PlatformHandler;
			if (platformHandler == null)
			{
				return;
			}
			platformHandler.Update();
		}
	}
}
