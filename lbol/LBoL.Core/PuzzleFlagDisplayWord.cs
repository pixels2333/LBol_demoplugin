using System;

namespace LBoL.Core
{
	// Token: 0x02000064 RID: 100
	public sealed class PuzzleFlagDisplayWord : IDisplayWord, IEquatable<IDisplayWord>
	{
		// Token: 0x0600044A RID: 1098 RVA: 0x0000EAA3 File Offset: 0x0000CCA3
		internal PuzzleFlagDisplayWord(PuzzleFlag flag, string name, string description)
		{
			this._flag = flag;
			this.Name = name;
			this.Description = description;
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x0000EAC0 File Offset: 0x0000CCC0
		public bool Equals(IDisplayWord other)
		{
			PuzzleFlagDisplayWord puzzleFlagDisplayWord = other as PuzzleFlagDisplayWord;
			return puzzleFlagDisplayWord != null && puzzleFlagDisplayWord._flag == this._flag;
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x0600044C RID: 1100 RVA: 0x0000EAE7 File Offset: 0x0000CCE7
		public string Name { get; }

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x0600044D RID: 1101 RVA: 0x0000EAEF File Offset: 0x0000CCEF
		public string Description { get; }

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x0600044E RID: 1102 RVA: 0x0000EAF7 File Offset: 0x0000CCF7
		public bool IsVerbose
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000254 RID: 596
		private readonly PuzzleFlag _flag;
	}
}
