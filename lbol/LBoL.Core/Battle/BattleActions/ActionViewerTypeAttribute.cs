using System;
namespace LBoL.Core.Battle.BattleActions
{
	internal class ActionViewerTypeAttribute : Attribute
	{
		public Type Type { get; }
		public ActionViewerTypeAttribute(Type type)
		{
			this.Type = type;
		}
	}
}
