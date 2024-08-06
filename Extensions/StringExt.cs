using System;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Sapientia.Extensions
{
	public static class StringExt
	{
		public const string NULL = "NULL";
		public const string EMPTY = "EMPTY";

		public static string Remove(this string str, string value)
		{
			return str.Replace(value, string.Empty);
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		public static string Format(this string str, params object[] args)
		{
			return string.Format(str, args);
		}

		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}

		public static string GetCompositeString<T>(this IEnumerable<T> collection, bool vertical = true, Func<T, string> getter = null,
			bool numerate = true,
			string separator = "")
		{
			if (collection == null) return string.Empty;

			return GetCompositeString(new List<T>(collection), vertical, getter, numerate, separator);
		}

		public static string GetCompositeString<T>(this List<T> items, bool vertical = true, Func<T, string> getter = null,
			bool numerate = true,
			string separator = "")
		{
			if (items == null) return NULL;
			if (items.Count == 0) return EMPTY;

			using (StringBuilderPool.Get(out var sb))
			{
				for (int i = 0; i < items.Count; i++)
				{
					T item = items[i];

					string value =
						item == null ? NULL :
						getter != null ? getter.Invoke(item) :
						item.ToString();

					string prefix = !separator.IsNullOrEmpty() ? separator :
						numerate ? $"{i + 1}. " :
						null;

					string next = vertical ? $"\n{prefix}{value}" : $"{prefix}{value} ";

					sb.Append(next);
				}

				return sb.ToString();
			}
		}

		public static string GetCompositeString<T>(IList<T> items, Func<T, int, string> getter,
			bool vertical = true,
			bool numerate = true,
			string separator = "")
		{
			if (items == null) return NULL;
			if (items.Count == 0) return EMPTY;

			using (StringBuilderPool.Get(out var sb))
			{
				for (int i = 0; i < items.Count; i++)
				{
					T item = items[i];

					string value =
						item == null ? getter != null ? getter.Invoke(item, i) + " " + NULL : NULL
						: getter != null ? getter.Invoke(item, i) : item.ToString();

					string prefix = !separator.IsNullOrEmpty() ? separator :
						numerate ? $"{i + 1}. " :
						null;

					string next = vertical ? $"\n{prefix}{value}" : $"{prefix}{value} ";

					sb.Append(next);
				}

				return sb.ToString();
			}
		}
	}
}
