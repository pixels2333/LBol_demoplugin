using System;
namespace LBoL.Base
{
	[Flags]
	public enum Keyword : ulong
	{
		None = 0UL,
		[Keyword(AutoAppend = false)]
		Basic = 1UL,
		[Keyword(AutoAppend = false)]
		Yinyang = 2UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Ability = 4UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Misfortune = 8UL,
		Unremovable = 16UL,
		Tool = 32UL,
		[Keyword(AutoAppend = false)]
		Gadgets = 64UL,
		Copy = 128UL,
		[Keyword(IsVerbose = true)]
		Accuracy = 256UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Block = 512UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Shield = 1024UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Philosophy = 2048UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		HybridCost = 4096UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Upgrade = 8192UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Power = 16384UL,
		Forbidden = 32768UL,
		Exile = 65536UL,
		Echo = 131072UL,
		EternalEcho = 262144UL,
		Ethereal = 524288UL,
		Initial = 1048576UL,
		Retain = 2097152UL,
		TempRetain = 4194304UL,
		Replenish = 8388608UL,
		Purified = 16777216UL,
		AutoExile = 33554432UL,
		FollowCard = 67108864UL,
		DreamCard = 134217728UL,
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Debut = 268435456UL,
		[Keyword(AutoAppend = false)]
		Instinct = 536870912UL,
		[Keyword(AutoAppend = false)]
		XCost = 1073741824UL,
		[Keyword(AutoAppend = false)]
		Synergy = 2147483648UL,
		[Keyword(AutoAppend = false)]
		Scry = 68719476736UL,
		[Keyword(AutoAppend = false)]
		Purify = 137438953472UL,
		[Keyword(AutoAppend = false)]
		Expel = 274877906944UL,
		[Keyword(AutoAppend = false)]
		Morph = 549755813888UL,
		[Keyword(AutoAppend = false)]
		TempMorph = 1099511627776UL,
		[Keyword(AutoAppend = false)]
		Overdrive = 2199023255552UL,
		[Keyword(AutoAppend = false)]
		Grow = 4398046511104UL,
		[Keyword(AutoAppend = false)]
		Plentiful = 8796093022208UL,
		[Keyword(AutoAppend = false)]
		Overdraft = 17592186044416UL,
		[Keyword(AutoAppend = false)]
		CopyHint = 35184372088832UL,
		[Keyword(AutoAppend = false)]
		Battlefield = 70368744177664UL,
		[Keyword(AutoAppend = false)]
		NaturalTurn = 140737488355328UL,
		[Keyword(AutoAppend = false)]
		FriendCard = 281474976710656UL,
		[Keyword(AutoAppend = false)]
		Friend = 562949953421312UL,
		[Keyword(AutoAppend = false)]
		FriendSummoned = 1125899906842624UL,
		[Keyword(AutoAppend = false)]
		Loyalty = 2251799813685248UL,
		[Keyword(AutoAppend = false)]
		Mood = 4503599627370496UL,
		[Keyword(AutoAppend = false)]
		FollowAttack = 9007199254740992UL,
		[Keyword(AutoAppend = false)]
		Dream = 18014398509481984UL,
		[Keyword(AutoAppend = false)]
		Deploy = 72057594037927936UL,
		[Keyword(AutoAppend = false)]
		Promote = 144115188075855872UL,
		[Keyword(AutoAppend = false)]
		Kicker = 288230376151711744UL,
		[Keyword(AutoAppend = false, Hidden = true)]
		GamerunInitial = 9223372036854775808UL
	}
}
