using System;
using LBoL.Base;
using LBoL.Core.Attributes;
namespace LBoL.Core.GapOptions
{
	[Localizable]
	public abstract class GapOption
	{
		public abstract GapOptionType Type { get; }
		protected string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<GapOption>.LocalizeProperty(this.Type.ToString(), key, decorated, required);
		}
		public string Name
		{
			get
			{
				return this.LocalizeProperty("Name", false, true);
			}
		}
		protected string BaseDescription
		{
			get
			{
				return this.LocalizeProperty("Description", true, true);
			}
		}
		protected virtual string GetBaseDescription()
		{
			return this.BaseDescription;
		}
		public string Description
		{
			get
			{
				string baseDescription = this.GetBaseDescription();
				return ((baseDescription != null) ? baseDescription.RuntimeFormat(this) : null) ?? ("<" + base.GetType().Name + ".Description>");
			}
		}
	}
}
