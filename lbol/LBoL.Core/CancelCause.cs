using System;
namespace LBoL.Core
{
	public enum CancelCause
	{
		None,
		Failure,
		InvalidTarget,
		HandFull,
		ZoneFull,
		EmptyDraw,
		AlreadyExiled,
		DollSlotFull,
		DollSlotEmpty,
		UserCanceled = 10,
		Reaction = 4096
	}
}
