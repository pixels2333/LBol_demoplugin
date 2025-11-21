using System;

namespace LBoL.Base
{
	// Token: 0x0200000E RID: 14
	[Flags]
	public enum Keyword : ulong
	{
		// Token: 0x04000039 RID: 57
		None = 0UL,
		// Token: 0x0400003A RID: 58
		[Keyword(AutoAppend = false)]
		Basic = 1UL,
		// Token: 0x0400003B RID: 59
		[Keyword(AutoAppend = false)]
		Yinyang = 2UL,
		// Token: 0x0400003C RID: 60
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Ability = 4UL,
		// Token: 0x0400003D RID: 61
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Misfortune = 8UL,
		// Token: 0x0400003E RID: 62
		Unremovable = 16UL,
		// Token: 0x0400003F RID: 63
		Tool = 32UL,
		// Token: 0x04000040 RID: 64
		[Keyword(AutoAppend = false)]
		Gadgets = 64UL,
		// Token: 0x04000041 RID: 65
		Copy = 128UL,
		// Token: 0x04000042 RID: 66
		[Keyword(IsVerbose = true)]
		Accuracy = 256UL,
		// Token: 0x04000043 RID: 67
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Block = 512UL,
		// Token: 0x04000044 RID: 68
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Shield = 1024UL,
		// Token: 0x04000045 RID: 69
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Philosophy = 2048UL,
		// Token: 0x04000046 RID: 70
		[Keyword(AutoAppend = false, IsVerbose = true)]
		HybridCost = 4096UL,
		// Token: 0x04000047 RID: 71
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Upgrade = 8192UL,
		// Token: 0x04000048 RID: 72
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Power = 16384UL,
		// Token: 0x04000049 RID: 73
		Forbidden = 32768UL,
		// Token: 0x0400004A RID: 74
		Exile = 65536UL,
		// Token: 0x0400004B RID: 75
		Echo = 131072UL,
		// Token: 0x0400004C RID: 76
		EternalEcho = 262144UL,
		// Token: 0x0400004D RID: 77
		Ethereal = 524288UL,
		// Token: 0x0400004E RID: 78
		Initial = 1048576UL,
		// Token: 0x0400004F RID: 79
		Retain = 2097152UL,
		// Token: 0x04000050 RID: 80
		TempRetain = 4194304UL,
		// Token: 0x04000051 RID: 81
		Replenish = 8388608UL,
		// Token: 0x04000052 RID: 82
		Purified = 16777216UL,
		// Token: 0x04000053 RID: 83
		AutoExile = 33554432UL,
		// Token: 0x04000054 RID: 84
		FollowCard = 67108864UL,
		// Token: 0x04000055 RID: 85
		DreamCard = 134217728UL,
		// Token: 0x04000056 RID: 86
		[Keyword(AutoAppend = false, IsVerbose = true)]
		Debut = 268435456UL,
		// Token: 0x04000057 RID: 87
		[Keyword(AutoAppend = false)]
		Instinct = 536870912UL,
		// Token: 0x04000058 RID: 88
		[Keyword(AutoAppend = false)]
		XCost = 1073741824UL,
		// Token: 0x04000059 RID: 89
		[Keyword(AutoAppend = false)]
		Synergy = 2147483648UL,
		// Token: 0x0400005A RID: 90
		[Keyword(AutoAppend = false)]
		Scry = 68719476736UL,
		// Token: 0x0400005B RID: 91
		[Keyword(AutoAppend = false)]
		Purify = 137438953472UL,
		// Token: 0x0400005C RID: 92
		[Keyword(AutoAppend = false)]
		Expel = 274877906944UL,
		// Token: 0x0400005D RID: 93
		[Keyword(AutoAppend = false)]
		Morph = 549755813888UL,
		// Token: 0x0400005E RID: 94
		[Keyword(AutoAppend = false)]
		TempMorph = 1099511627776UL,
		// Token: 0x0400005F RID: 95
		[Keyword(AutoAppend = false)]
		Overdrive = 2199023255552UL,
		// Token: 0x04000060 RID: 96
		[Keyword(AutoAppend = false)]
		Grow = 4398046511104UL,
		// Token: 0x04000061 RID: 97
		[Keyword(AutoAppend = false)]
		Plentiful = 8796093022208UL,
		// Token: 0x04000062 RID: 98
		[Keyword(AutoAppend = false)]
		Overdraft = 17592186044416UL,
		// Token: 0x04000063 RID: 99
		[Keyword(AutoAppend = false)]
		CopyHint = 35184372088832UL,
		// Token: 0x04000064 RID: 100
		[Keyword(AutoAppend = false)]
		Battlefield = 70368744177664UL,
		// Token: 0x04000065 RID: 101
		[Keyword(AutoAppend = false)]
		NaturalTurn = 140737488355328UL,
		// Token: 0x04000066 RID: 102
		[Keyword(AutoAppend = false)]
		FriendCard = 281474976710656UL,
		// Token: 0x04000067 RID: 103
		[Keyword(AutoAppend = false)]
		Friend = 562949953421312UL,
		// Token: 0x04000068 RID: 104
		[Keyword(AutoAppend = false)]
		FriendSummoned = 1125899906842624UL,
		// Token: 0x04000069 RID: 105
		[Keyword(AutoAppend = false)]
		Loyalty = 2251799813685248UL,
		// Token: 0x0400006A RID: 106
		[Keyword(AutoAppend = false)]
		Mood = 4503599627370496UL,
		// Token: 0x0400006B RID: 107
		[Keyword(AutoAppend = false)]
		FollowAttack = 9007199254740992UL,
		// Token: 0x0400006C RID: 108
		[Keyword(AutoAppend = false)]
		Dream = 18014398509481984UL,
		// Token: 0x0400006D RID: 109
		[Keyword(AutoAppend = false)]
		Deploy = 72057594037927936UL,
		// Token: 0x0400006E RID: 110
		[Keyword(AutoAppend = false)]
		Promote = 144115188075855872UL,
		// Token: 0x0400006F RID: 111
		[Keyword(AutoAppend = false)]
		Kicker = 288230376151711744UL,
		// Token: 0x04000070 RID: 112
		[Keyword(AutoAppend = false, Hidden = true)]
		GamerunInitial = 9223372036854775808UL
	}
}
