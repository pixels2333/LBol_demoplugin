using System;
using LBoL.Base;
namespace LBoL.Core
{
	public class KeywordDisplayWord : IDisplayWord, IEquatable<IDisplayWord>
	{
		internal KeywordDisplayWord(Keyword keyword, string name, string description, bool isAutoAppend, bool isVerbose, bool isHidden)
		{
			this.Keyword = keyword;
			this.Name = name;
			this.Description = description;
			this.IsAutoAppend = isAutoAppend;
			this.IsVerbose = isVerbose;
			this.IsHidden = isHidden;
		}
		public Keyword Keyword { get; }
		public string Name { get; }
		public string Description { get; }
		public bool IsAutoAppend { get; }
		public bool IsVerbose { get; }
		public bool IsHidden { get; }
		public bool Equals(IDisplayWord other)
		{
			KeywordDisplayWord keywordDisplayWord = other as KeywordDisplayWord;
			return keywordDisplayWord != null && this.Keyword == keywordDisplayWord.Keyword;
		}
	}
}
