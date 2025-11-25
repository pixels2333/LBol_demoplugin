using System;
namespace LBoL.Core
{
	public sealed class PuzzleFlagDisplayWord : IDisplayWord, IEquatable<IDisplayWord>
	{
		internal PuzzleFlagDisplayWord(PuzzleFlag flag, string name, string description)
		{
			this._flag = flag;
			this.Name = name;
			this.Description = description;
		}
		public bool Equals(IDisplayWord other)
		{
			PuzzleFlagDisplayWord puzzleFlagDisplayWord = other as PuzzleFlagDisplayWord;
			return puzzleFlagDisplayWord != null && puzzleFlagDisplayWord._flag == this._flag;
		}
		public string Name { get; }
		public string Description { get; }
		public bool IsVerbose
		{
			get
			{
				return false;
			}
		}
		private readonly PuzzleFlag _flag;
	}
}
