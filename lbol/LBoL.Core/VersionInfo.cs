using System;
using UnityEngine;
using YamlDotNet.Serialization;

namespace LBoL.Core
{
	// Token: 0x02000079 RID: 121
	[Serializable]
	public sealed class VersionInfo
	{
		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000551 RID: 1361 RVA: 0x00011DD5 File Offset: 0x0000FFD5
		// (set) Token: 0x06000552 RID: 1362 RVA: 0x00011DDD File Offset: 0x0000FFDD
		public string Revision { get; set; }

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000553 RID: 1363 RVA: 0x00011DE6 File Offset: 0x0000FFE6
		// (set) Token: 0x06000554 RID: 1364 RVA: 0x00011DEE File Offset: 0x0000FFEE
		public string Version { get; set; }

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06000555 RID: 1365 RVA: 0x00011DF7 File Offset: 0x0000FFF7
		// (set) Token: 0x06000556 RID: 1366 RVA: 0x00011DFF File Offset: 0x0000FFFF
		public bool IsBeta { get; set; }

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000557 RID: 1367 RVA: 0x00011E08 File Offset: 0x00010008
		// (set) Token: 0x06000558 RID: 1368 RVA: 0x00011E10 File Offset: 0x00010010
		public string BuildDateTime { get; set; }

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000559 RID: 1369 RVA: 0x00011E19 File Offset: 0x00010019
		public static VersionInfo Current { get; } = new Deserializer().Deserialize<VersionInfo>(Resources.Load<TextAsset>("VersionInfo").text);
	}
}
