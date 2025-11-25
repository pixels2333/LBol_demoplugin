using System;
namespace LBoL.Core
{
	public interface IDisplayWord : IEquatable<IDisplayWord>
	{
		string Name { get; }
		string Description { get; }
		bool IsVerbose { get; }
	}
}
