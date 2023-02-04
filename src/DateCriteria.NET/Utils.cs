namespace DateCriteria.NET;

public static class Utils
{
	/// <summary>
	/// Produces a hash code based on the hash codes of the elements of an <see cref="IEnumerable{T}"/>. Two separate objects
	/// containing elements producing the same hash codes will thus be considered identical, even if their references are not.
	/// </summary>
	/// <param name="enumerable">The enumerable collection to retrieve the hash code for.</param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static int EnumerableHashCode<T>(this IEnumerable<T> enumerable)
	{
		int hashCode = 0;
		foreach (T obj in enumerable)
		{
			if (obj == null) continue;
			hashCode ^= obj.GetHashCode();
		}
		return hashCode;
	}

	public static bool EnumTryParseStrict<TEnum>(string value, out TEnum result) where TEnum : struct
		=> EnumTryParseStrict(value, false, out result);

	public static bool EnumTryParseStrict<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
	{
		if (!int.TryParse(value, out _)) return Enum.TryParse(value, ignoreCase, out result);
		result = default;
		return false;
	}
}