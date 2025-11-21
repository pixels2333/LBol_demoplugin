using System;
using LBoL.Base;

namespace LBoL.Core
{
	// Token: 0x02000056 RID: 86
	public class KeywordDisplayWord : IDisplayWord, IEquatable<IDisplayWord>
	{
		// Token: 0x0600039C RID: 924 RVA: 0x0000BBB7 File Offset: 0x00009DB7
		internal KeywordDisplayWord(Keyword keyword, string name, string description, bool isAutoAppend, bool isVerbose, bool isHidden)
		{
			this.Keyword = keyword;
			this.Name = name;
			this.Description = description;
			this.IsAutoAppend = isAutoAppend;
			this.IsVerbose = isVerbose;
			this.IsHidden = isHidden;
		}

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x0600039D RID: 925 RVA: 0x0000BBEC File Offset: 0x00009DEC
		public Keyword Keyword { get; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x0600039E RID: 926 RVA: 0x0000BBF4 File Offset: 0x00009DF4
		public string Name { get; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x0600039F RID: 927 RVA: 0x0000BBFC File Offset: 0x00009DFC
		public string Description { get; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x060003A0 RID: 928 RVA: 0x0000BC04 File Offset: 0x00009E04
		public bool IsAutoAppend { get; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x060003A1 RID: 929 RVA: 0x0000BC0C File Offset: 0x00009E0C
		public bool IsVerbose { get; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x060003A2 RID: 930 RVA: 0x0000BC14 File Offset: 0x00009E14
		public bool IsHidden { get; }

		// Token: 0x060003A3 RID: 931 RVA: 0x0000BC1C File Offset: 0x00009E1C
		public bool Equals(IDisplayWord other)
		{
			KeywordDisplayWord keywordDisplayWord = other as KeywordDisplayWord;
			return keywordDisplayWord != null && this.Keyword == keywordDisplayWord.Keyword;
		}
	}
}
