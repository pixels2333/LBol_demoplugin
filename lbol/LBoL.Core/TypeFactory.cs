using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000071 RID: 113
	internal static class TypeFactory<T> where T : class
	{
		// Token: 0x06000516 RID: 1302 RVA: 0x00010BD3 File Offset: 0x0000EDD3
		private static T InternalCreateInstance(Type type)
		{
			T t = (T)((object)Activator.CreateInstance(type));
			IInitializable initializable = t as IInitializable;
			if (initializable != null)
			{
				initializable.Initialize();
			}
			IVerifiable verifiable = t as IVerifiable;
			if (verifiable == null)
			{
				return t;
			}
			verifiable.Verify();
			return t;
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x00010C0C File Offset: 0x0000EE0C
		public static void RegisterAssembly(Assembly assembly)
		{
			Type baseType = typeof(T);
			foreach (Type type2 in Enumerable.ToList<Type>(Enumerable.Where<Type>(assembly.GetExportedTypes(), (Type type) => type.IsSealed && type.IsSubclassOf(baseType))))
			{
				TypeFactory<T>.FullNameTypeDict.Add(type2.FullName, type2);
				TypeFactory<T>.TypeDict.TryAdd(type2.Name, type2);
			}
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x00010CA8 File Offset: 0x0000EEA8
		public static async UniTask ReloadLocalizationTableAsync()
		{
			try
			{
				UniTask<Dictionary<string, Dictionary<string, object>>>.Awaiter awaiter = Localization.LoadTypeLocalizationTableAsync(typeof(T), TypeFactory<T>.AllTypes).GetAwaiter();
				if (!awaiter.IsCompleted)
				{
					await awaiter;
					UniTask<Dictionary<string, Dictionary<string, object>>>.Awaiter awaiter2;
					awaiter = awaiter2;
					awaiter2 = default(UniTask<Dictionary<string, Dictionary<string, object>>>.Awaiter);
				}
				TypeFactory<T>._typeLocalizers = awaiter.GetResult();
			}
			catch (Exception ex)
			{
				TypeFactory<T>._typeLocalizers = new Dictionary<string, Dictionary<string, object>>();
				Debug.LogError("[Localization] Failed to load localization for " + typeof(T).Name);
				Debug.LogException(ex);
			}
			TypeFactory<T>._failureTable = new Dictionary<string, string>();
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x00010CE4 File Offset: 0x0000EEE4
		internal static string LocalizeProperty(string id, string key, bool decorated, bool required)
		{
			Dictionary<string, object> dictionary;
			object obj;
			if (TypeFactory<T>._typeLocalizers.TryGetValue(id, ref dictionary) && dictionary.TryGetValue(key, ref obj))
			{
				string text = obj as string;
				if (text == null)
				{
					string text2;
					if (!TypeFactory<T>._failureTable.TryGetValue(id + "." + key, ref text2))
					{
						text2 = string.Concat(new string[] { "<", id, ".", key, ">(Error)" });
						TypeFactory<T>._failureTable.Add(id + "." + key, text2);
						Debug.LogError(string.Concat(new string[]
						{
							"[Localization:",
							typeof(T).Name,
							"] ",
							id,
							".",
							key,
							" (",
							obj.GetType().Name,
							") is not string"
						}));
					}
					return text2;
				}
				if (!decorated)
				{
					return text;
				}
				return StringDecorator.Decorate(text);
			}
			else
			{
				if (required)
				{
					string text3;
					if (!TypeFactory<T>._failureTable.TryGetValue(id + "." + key, ref text3))
					{
						text3 = string.Concat(new string[] { "<", id, ".", key, ">" });
						TypeFactory<T>._failureTable.Add(id + "." + key, text3);
						Debug.LogError(string.Concat(new string[]
						{
							"[Localization:",
							typeof(T).Name,
							"] ",
							id,
							".",
							key,
							" not found"
						}));
					}
					return text3;
				}
				return null;
			}
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x00010EA0 File Offset: 0x0000F0A0
		internal static IReadOnlyList<string> LocalizeListProperty(string id, string key, bool required)
		{
			Dictionary<string, object> dictionary;
			object obj;
			if (TypeFactory<T>._typeLocalizers.TryGetValue(id, ref dictionary) && dictionary.TryGetValue(key, ref obj))
			{
				IReadOnlyList<string> readOnlyList = obj as IReadOnlyList<string>;
				if (readOnlyList != null)
				{
					return readOnlyList;
				}
				string text;
				if (!TypeFactory<T>._failureTable.TryGetValue(id + "." + key, ref text))
				{
					text = "<Error>";
					TypeFactory<T>._failureTable.Add(id + "." + key, text);
					Debug.LogError(string.Concat(new string[]
					{
						"[Localization:",
						typeof(T).Name,
						"] ",
						id,
						".",
						key,
						" (",
						obj.GetType().Name,
						") is not list of string"
					}));
				}
				return FaultTolerantArray<string>.Empty(text, string.Concat(new string[]
				{
					"[Localization:",
					typeof(T).Name,
					"] ",
					id,
					".",
					key,
					" type error."
				}));
			}
			else
			{
				if (required)
				{
					string text2;
					if (!TypeFactory<T>._failureTable.TryGetValue(id + "." + key, ref text2))
					{
						text2 = "<Null>";
						TypeFactory<T>._failureTable.Add(id + "." + key, text2);
						Debug.LogError(string.Concat(new string[]
						{
							"[Localization:",
							typeof(T).Name,
							"] ",
							id,
							".",
							key,
							" not found"
						}));
					}
					return FaultTolerantArray<string>.Empty("<null>", string.Concat(new string[]
					{
						"[Localization:",
						typeof(T).Name,
						"] ",
						id,
						".",
						key,
						" not found."
					}));
				}
				return null;
			}
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x00011098 File Offset: 0x0000F298
		public static Type TryGetType(string id)
		{
			Type type;
			if (TypeFactory<T>.TypeDict.TryGetValue(id, ref type) || TypeFactory<T>.FullNameTypeDict.TryGetValue(id, ref type))
			{
				return type;
			}
			return null;
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x000110C8 File Offset: 0x0000F2C8
		public static Type GetType(string id)
		{
			Type type = TypeFactory<T>.TryGetType(id);
			if (type == null)
			{
				throw new ArgumentException(string.Concat(new string[]
				{
					"Cannot find ",
					typeof(T).Name,
					" type '",
					id,
					"'"
				}));
			}
			return type;
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x00011120 File Offset: 0x0000F320
		public static T TryCreateInstance(string id)
		{
			Type type = TypeFactory<T>.TryGetType(id);
			if (!(type == null))
			{
				return TypeFactory<T>.InternalCreateInstance(type);
			}
			return default(T);
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x00011150 File Offset: 0x0000F350
		public static T CreateInstance(string id)
		{
			T t = TypeFactory<T>.TryCreateInstance(id);
			if (t != null)
			{
				return t;
			}
			throw new ArgumentException("Cannot create instance " + id + " of type " + typeof(T).Name);
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x00011194 File Offset: 0x0000F394
		public static T TryCreateInstance(Type type)
		{
			if (!type.IsSubclassOf(typeof(T)))
			{
				throw new ArgumentException("Type " + type.Name + " is not sub-class of " + typeof(T).Name);
			}
			return TypeFactory<T>.InternalCreateInstance(type);
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x000111E4 File Offset: 0x0000F3E4
		public static T CreateInstance(Type type)
		{
			if (!type.IsSubclassOf(typeof(T)))
			{
				throw new ArgumentException("Type " + type.Name + " is not sub-class of " + typeof(T).Name);
			}
			T t = TypeFactory<T>.TryCreateInstance(type);
			if (t != null)
			{
				return t;
			}
			throw new ArgumentException("Cannot create instance " + type.Name + " of type " + typeof(T).Name);
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x00011267 File Offset: 0x0000F467
		public static TDerived TryCreateInstance<TDerived>() where TDerived : T
		{
			return (TDerived)((object)TypeFactory<T>.TryCreateInstance(typeof(TDerived)));
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x00011282 File Offset: 0x0000F482
		public static TDerived CreateInstance<TDerived>() where TDerived : T
		{
			return (TDerived)((object)TypeFactory<T>.CreateInstance(typeof(TDerived)));
		}

		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06000523 RID: 1315 RVA: 0x0001129D File Offset: 0x0000F49D
		public static IEnumerable<Type> AllTypes
		{
			get
			{
				return TypeFactory<T>.TypeDict.Values;
			}
		}

		// Token: 0x04000297 RID: 663
		private static readonly Dictionary<string, Type> FullNameTypeDict = new Dictionary<string, Type>();

		// Token: 0x04000298 RID: 664
		private static readonly Dictionary<string, Type> TypeDict = new Dictionary<string, Type>();

		// Token: 0x04000299 RID: 665
		private static Dictionary<string, Dictionary<string, object>> _typeLocalizers;

		// Token: 0x0400029A RID: 666
		private static Dictionary<string, string> _failureTable;
	}
}
