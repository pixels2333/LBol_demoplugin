using System;
namespace LBoL.Core
{
	public interface INotifyChanged
	{
		event Action PropertyChanged;
		void NotifyChanged();
	}
}
