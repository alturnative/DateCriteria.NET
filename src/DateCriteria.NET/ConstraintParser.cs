using System.Text.RegularExpressions;
using static System.StringSplitOptions;

namespace DateCriteria.NET;

public static class ConstraintParser
{
	internal static void ParseConstraints(string input, Func<DateConstraint, bool> addRuleAction)
	{
		var stringRules = input.Split(";", TrimEntries | RemoveEmptyEntries);
		foreach (var rule in stringRules) addRuleAction(ParseConstraint(rule));
	}

	private static DateConstraint ParseConstraint(string input)
	{
		var args = Operators.Comparison.Split(input);
		if (args.Length != 3) throw new Exception($"Invalid rule definition in '{input}'.");
		return HandleOperation(args);
	}

	private static DateConstraint HandleOperation(string[] args)
	{
		var lTrim = args[0].Trim();
		var op = args[1].Trim();
		var rTrim = args[2].Trim();
		var ruleText = $"{lTrim}{op}{rTrim}";
		Func<DateOnly, ValueObject> left = GetValueFunction(lTrim, out Token lToken);
		Func<DateOnly, ValueObject> right = GetValueFunction(rTrim, out Token rToken);

		var lComparison = ComparisonTypes[lToken];
		var rComparison = ComparisonTypes[rToken];

		if (lComparison != rComparison)
			throw new Exception($"Incompatible operands in rule - \"{lTrim}\" ({lComparison}) is not comparable with \"{rTrim}\" ({rComparison}).");

		DateConstraint dateConstraint = lComparison switch
		{
			ComparisonType.Date => new() { RuleAction = x => DateComparers[op](left(x).Date!.Value, right(x).Date!.Value) },
			ComparisonType.DayOfWeek => new() { RuleAction = x => DayOfWeekComparers[op](left(x).DayOfWeek!.Value, right(x).DayOfWeek!.Value) },
			ComparisonType.Value => new() { RuleAction = x => ValueComparers[op](left(x).Value!.Value, right(x).Value!.Value) },
			_ => throw new NotImplementedException()
		};
		dateConstraint.RuleText = ruleText;

		return dateConstraint;
	}

	private static Func<DateOnly, ValueObject> GetValueFunction(string input, out Token token)
	{
		if (DateOnly.TryParseExact(input, "yyyy-MM-dd", out var date))
		{
			token = Token.Date;
			return x => new ValueObject { Date = date };
		}
		if (int.TryParse(input, out int value))
		{
			token = Token.Day;
			return x => new ValueObject { Value = value };
		}
		
		var expression = Operators.Arithmetic.Split(input);
		if (expression.Length is not (1 or 3)) throw new Exception($"Invalid expression in comparison: '{input}'.");
		if (expression.Length == 1) // nice, simple, single token :)
		{
			var trimmed = expression[0].Trim();
			if (Enum.TryParse(trimmed, true, out token))
			{	// we got a nice token
				return token switch
				{
					Token.Date => x => new ValueObject { Date = x },
					Token.DayOfWeek => x => new ValueObject { DayOfWeek = x.DayOfWeek },
					Token.Easter => x => new ValueObject { Date = Easter(x) },
					Token.Day => x => new ValueObject { Value = x.Day },
					Token.Month => x => new ValueObject { Value = x.Month },
					Token.Year => x => new ValueObject { Value = x.Year },
					Token.EndOfMonth => x => new ValueObject { Date = new DateOnly(x.Year, x.Month, DateTime.DaysInMonth(x.Year, x.Month)) },
					_ => throw new NotImplementedException(),
				};
			}

			if (Enum.TryParse(trimmed, true, out DayOfWeek dow)) return x => new ValueObject { DayOfWeek = dow };
		}
		else // horrible, complex, arithmetic expression :(
		{
			var lTrim = expression[0].Trim();
			var op = expression[1].Trim();
			var rTrim = expression[2].Trim();

			if (!int.TryParse(lTrim, out _) && Enum.TryParse<DayOfWeek>(lTrim, out _) || !int.TryParse(rTrim, out _) && Enum.TryParse<DayOfWeek>(rTrim, out _))
				throw new Exception("Why are you doing arithmetic with a day of the week??");

			// try parse as date => RHS should be raw numeric value

			if (DateOnly.TryParseExact(lTrim, "yyyy-MM-dd", out var lDate))
			{
				if (!int.TryParse(rTrim, out var rNum))
					throw new Exception("RHS of arithmetic operation must be a numeric value when LHS is a specified date");

				return x => new ValueObject { Date = lDate.AddDays(op == "+" ? rNum : - rNum) };
			}

			// else try parse token => RHS depends on token

		if (Enum.TryParse(lTrim, out token))
		{
			GetRnum(token, rTrim, out var rNum);
			return token switch
			{
				Token.Date => x => new ValueObject { Date = x.AddDays(op == "+" ? rNum : -rNum) },
				Token.Day => x => new ValueObject { Value = x.Day + op == "+" ? rNum : -rNum },
				Token.Month => x => new ValueObject { Value = x.Month + op == "+" ? rNum : -rNum },
				Token.Year => x => new ValueObject { Value = x.Year + op == "+" ? rNum : -rNum },
				Token.DayNumber => x => new ValueObject { Value = x.DayNumber + op == "+" ? rNum : -rNum },
				Token.DayOfYear => x => new ValueObject { Value = x.DayOfYear + op == "+" ? rNum : -rNum },
				Token.Easter => x => new ValueObject { Date = Easter(x).AddDays(op == "+" ? rNum : -rNum) },
				Token.EndOfMonth => x => new ValueObject
					{ Date = new DateOnly(x.Year, x.Month, DateTime.DaysInMonth(x.Year, x.Month)).AddDays(op == "+" ? rNum : -rNum) },
				Token.DayOfWeek => throw new Exception("This path shouldn't be possible!"),
				_ => throw new NotImplementedException(),
			};
		}

			// else try parse raw numeric value => RHS should be date or token TODO less work if we require numerics to be RHS of op

			void GetRnum(Token token, string trimmed, out int num)
			{
				num = 0;
				if (!int.TryParse(trimmed, out num))
					throw new Exception($"RHS of arithmetic operation ({trimmed}) must be a numeric value when LHS token is \"{token}\".");
			}
		}

		
		throw new NotImplementedException();
	}
	
	private static class Operators
	{
		internal static readonly Regex Comparison = new Regex(@"(?<![<>=])(!=|[<>]=?|==)(?![<>=])");
		internal static readonly Regex Arithmetic = new Regex(@"(?<![+\-\*%/])([+\-\*%/]|\*\*)(?![+\-\*%/])"); // +, -, *, %, /, **
	}

	private static Dictionary<string, Func<DateOnly, DateOnly, bool>> DateComparers { get; } = new ()
	{
		{ "==", (x, y) => x == y },
		{ "!=", (x, y) => x != y },
		{ "<", (x, y) => x < y },
		{ "<=", (x, y) => x <= y },
		{ ">", (x, y) => x > y },
		{ ">=", (x, y) => x >= y },
	};

	private static Dictionary<string, Func<DayOfWeek, DayOfWeek, bool>> DayOfWeekComparers { get; } = new ()
	{
		{ "==", (x, y) => x == y },
		{ "!=", (x, y) => x != y },
	};

	private static Dictionary<string, Func<int, int, bool>> ValueComparers { get; } = new ()
	{
		{ "==", (x, y) => x == y },
		{ "!=", (x, y) => x != y },
		{ "<", (x, y) => x < y },
		{ "<=", (x, y) => x <= y },
		{ ">", (x, y) => x > y },
		{ ">=", (x, y) => x >= y },
	};

	private static Dictionary<Token, ComparisonType> ComparisonTypes { get; } = new()
	{
		{Token.Date, ComparisonType.Date},
		{Token.Day, ComparisonType.Value},
		{Token.Month, ComparisonType.Value},
		{Token.Year, ComparisonType.Value},
		{Token.DayOfWeek, ComparisonType.DayOfWeek},
		{Token.DayNumber, ComparisonType.Value},
		{Token.DayOfYear, ComparisonType.Value},
		{Token.Easter, ComparisonType.Date},
		{Token.EndOfMonth, ComparisonType.Date},
	};

	private struct ValueObject
	{
		internal DateOnly? Date;
		internal int? Value;
		internal DayOfWeek? DayOfWeek;
	}

	private static DateOnly Easter(DateOnly date) => EasterSundays.TryGetValue(date.Year, out var easter)
		? easter
		: throw new Exception($"Couldn't get easter for year {date.Year}.") ;

	private static Dictionary<int, DateOnly> EasterSundays { get; } = new Dictionary<int, DateOnly>{
		// 1900-1999
		{ 1900, new DateOnly(1900, 04, 15) }, { 1901, new DateOnly(1901, 04, 07) }, { 1902, new DateOnly(1902, 03, 30) },
		{ 1903, new DateOnly(1903, 04, 12) }, { 1904, new DateOnly(1904, 04, 03) }, { 1905, new DateOnly(1905, 04, 23) },
		{ 1906, new DateOnly(1906, 04, 15) }, { 1907, new DateOnly(1907, 03, 31) }, { 1908, new DateOnly(1908, 04, 19) },
		{ 1909, new DateOnly(1909, 04, 11) }, { 1910, new DateOnly(1910, 03, 27) }, { 1911, new DateOnly(1911, 04, 16) },
		{ 1912, new DateOnly(1912, 04, 07) }, { 1913, new DateOnly(1913, 03, 23) }, { 1914, new DateOnly(1914, 04, 12) },
		{ 1915, new DateOnly(1915, 04, 04) }, { 1916, new DateOnly(1916, 04, 23) }, { 1917, new DateOnly(1917, 04, 08) },
		{ 1918, new DateOnly(1918, 03, 31) }, { 1919, new DateOnly(1919, 04, 20) }, { 1920, new DateOnly(1920, 04, 04) },
		{ 1921, new DateOnly(1921, 03, 27) }, { 1922, new DateOnly(1922, 04, 16) }, { 1923, new DateOnly(1923, 04, 01) },
		{ 1924, new DateOnly(1924, 04, 20) }, { 1925, new DateOnly(1925, 04, 12) }, { 1926, new DateOnly(1926, 04, 04) },
		{ 1927, new DateOnly(1927, 04, 17) }, { 1928, new DateOnly(1928, 04, 08) }, { 1929, new DateOnly(1929, 03, 31) },

		{ 1930, new DateOnly(1930, 04, 20) }, { 1931, new DateOnly(1931, 04, 05) }, { 1932, new DateOnly(1932, 03, 27) },
		{ 1933, new DateOnly(1933, 04, 16) }, { 1934, new DateOnly(1934, 04, 01) }, { 1935, new DateOnly(1935, 04, 21) },
		{ 1936, new DateOnly(1936, 04, 12) }, { 1937, new DateOnly(1937, 03, 28) }, { 1938, new DateOnly(1938, 04, 17) },
		{ 1939, new DateOnly(1939, 04, 09) }, { 1940, new DateOnly(1940, 03, 24) }, { 1941, new DateOnly(1941, 04, 13) },
		{ 1942, new DateOnly(1942, 04, 05) }, { 1943, new DateOnly(1943, 04, 25) }, { 1944, new DateOnly(1944, 04, 09) },
		{ 1945, new DateOnly(1945, 04, 01) }, { 1946, new DateOnly(1946, 04, 21) }, { 1947, new DateOnly(1947, 04, 06) },
		{ 1948, new DateOnly(1948, 03, 28) }, { 1949, new DateOnly(1949, 04, 17) }, { 1950, new DateOnly(1950, 04, 09) },
		{ 1951, new DateOnly(1951, 03, 25) }, { 1952, new DateOnly(1952, 04, 13) }, { 1953, new DateOnly(1953, 04, 05) },
		{ 1954, new DateOnly(1954, 04, 18) }, { 1955, new DateOnly(1955, 04, 10) }, { 1956, new DateOnly(1956, 04, 01) },
		{ 1957, new DateOnly(1957, 04, 21) }, { 1958, new DateOnly(1958, 04, 06) }, { 1959, new DateOnly(1959, 03, 29) },

		{ 1960, new DateOnly(1960, 04, 17) }, { 1961, new DateOnly(1961, 04, 02) }, { 1962, new DateOnly(1962, 04, 22) },
		{ 1963, new DateOnly(1963, 04, 14) }, { 1964, new DateOnly(1964, 03, 29) }, { 1965, new DateOnly(1965, 04, 18) },
		{ 1966, new DateOnly(1966, 04, 10) }, { 1967, new DateOnly(1967, 03, 26) }, { 1968, new DateOnly(1968, 04, 14) },
		{ 1969, new DateOnly(1969, 04, 06) }, { 1970, new DateOnly(1970, 03, 29) }, { 1971, new DateOnly(1971, 04, 11) },
		{ 1972, new DateOnly(1972, 04, 02) }, { 1973, new DateOnly(1973, 04, 22) }, { 1974, new DateOnly(1974, 04, 14) },
		{ 1975, new DateOnly(1975, 03, 30) }, { 1976, new DateOnly(1976, 04, 18) }, { 1977, new DateOnly(1977, 04, 10) },
		{ 1978, new DateOnly(1978, 03, 26) }, { 1979, new DateOnly(1979, 04, 15) }, { 1980, new DateOnly(1980, 04, 06) },
		{ 1981, new DateOnly(1981, 04, 19) }, { 1982, new DateOnly(1982, 04, 11) }, { 1983, new DateOnly(1983, 04, 03) },
		{ 1984, new DateOnly(1984, 04, 22) }, { 1985, new DateOnly(1985, 04, 07) }, { 1986, new DateOnly(1986, 03, 30) },
		{ 1987, new DateOnly(1987, 04, 19) }, { 1988, new DateOnly(1988, 04, 03) }, { 1989, new DateOnly(1989, 03, 26) },

		{ 1990, new DateOnly(1990, 04, 15) }, { 1991, new DateOnly(1991, 03, 31) }, { 1992, new DateOnly(1992, 04, 19) },
		{ 1993, new DateOnly(1993, 04, 11) }, { 1994, new DateOnly(1994, 04, 03) }, { 1995, new DateOnly(1995, 04, 16) },
		{ 1996, new DateOnly(1996, 04, 07) }, { 1997, new DateOnly(1997, 03, 30) }, { 1998, new DateOnly(1998, 04, 12) },
		{ 1999, new DateOnly(1999, 04, 04) },
		
		// 2000-2099
		{ 2000, new DateOnly(2000, 04, 23) }, { 2001, new DateOnly(2001, 04, 15) }, { 2002, new DateOnly(2002, 03, 31) },
		{ 2003, new DateOnly(2003, 04, 20) }, { 2004, new DateOnly(2004, 04, 11) }, { 2005, new DateOnly(2005, 03, 27) },
		{ 2006, new DateOnly(2006, 04, 16) }, { 2007, new DateOnly(2007, 04, 08) }, { 2008, new DateOnly(2008, 03, 23) },
		{ 2009, new DateOnly(2009, 04, 12) }, { 2010, new DateOnly(2010, 04, 04) }, { 2011, new DateOnly(2011, 04, 24) },
		{ 2012, new DateOnly(2012, 04, 08) }, { 2013, new DateOnly(2013, 03, 31) }, { 2014, new DateOnly(2014, 04, 20) },
		{ 2015, new DateOnly(2015, 04, 05) }, { 2016, new DateOnly(2016, 03, 27) }, { 2017, new DateOnly(2017, 04, 16) },
		{ 2018, new DateOnly(2018, 04, 01) }, { 2019, new DateOnly(2019, 04, 21) }, { 2020, new DateOnly(2020, 04, 12) },
		{ 2021, new DateOnly(2021, 04, 04) }, { 2022, new DateOnly(2022, 04, 17) }, { 2023, new DateOnly(2023, 04, 09) },
		{ 2024, new DateOnly(2024, 03, 31) }, { 2025, new DateOnly(2025, 04, 20) }, { 2026, new DateOnly(2026, 04, 05) },
		{ 2027, new DateOnly(2027, 03, 28) }, { 2028, new DateOnly(2028, 04, 16) }, { 2029, new DateOnly(2029, 04, 01) },

		{ 2030, new DateOnly(2030, 04, 21) }, { 2031, new DateOnly(2031, 04, 13) }, { 2032, new DateOnly(2032, 03, 28) },
		{ 2033, new DateOnly(2033, 04, 17) }, { 2034, new DateOnly(2034, 04, 09) }, { 2035, new DateOnly(2035, 03, 25) },
		{ 2036, new DateOnly(2036, 04, 13) }, { 2037, new DateOnly(2037, 04, 05) }, { 2038, new DateOnly(2038, 04, 25) },
		{ 2039, new DateOnly(2039, 04, 10) }, { 2040, new DateOnly(2040, 04, 01) }, { 2041, new DateOnly(2041, 04, 21) },
		{ 2042, new DateOnly(2042, 04, 06) }, { 2043, new DateOnly(2043, 03, 29) }, { 2044, new DateOnly(2044, 04, 17) },
		{ 2045, new DateOnly(2045, 04, 09) }, { 2046, new DateOnly(2046, 03, 25) }, { 2047, new DateOnly(2047, 04, 14) },
		{ 2048, new DateOnly(2048, 04, 05) }, { 2049, new DateOnly(2049, 04, 18) }, { 2050, new DateOnly(2050, 04, 10) },
		{ 2051, new DateOnly(2051, 04, 02) }, { 2052, new DateOnly(2052, 04, 21) }, { 2053, new DateOnly(2053, 04, 06) },
		{ 2054, new DateOnly(2054, 03, 29) }, { 2055, new DateOnly(2055, 04, 18) }, { 2056, new DateOnly(2056, 04, 02) },
		{ 2057, new DateOnly(2057, 04, 22) }, { 2058, new DateOnly(2058, 04, 14) }, { 2059, new DateOnly(2059, 03, 30) },

		{ 2060, new DateOnly(2060, 04, 18) }, { 2061, new DateOnly(2061, 04, 10) }, { 2062, new DateOnly(2062, 03, 26) },
		{ 2063, new DateOnly(2063, 04, 15) }, { 2064, new DateOnly(2064, 04, 06) }, { 2065, new DateOnly(2065, 03, 29) },
		{ 2066, new DateOnly(2066, 04, 11) }, { 2067, new DateOnly(2067, 04, 03) }, { 2068, new DateOnly(2068, 04, 22) },
		{ 2069, new DateOnly(2069, 04, 14) }, { 2070, new DateOnly(2070, 03, 30) }, { 2071, new DateOnly(2071, 04, 19) },
		{ 2072, new DateOnly(2072, 04, 10) }, { 2073, new DateOnly(2073, 03, 26) }, { 2074, new DateOnly(2074, 04, 15) },
		{ 2075, new DateOnly(2075, 04, 07) }, { 2076, new DateOnly(2076, 04, 19) }, { 2077, new DateOnly(2077, 04, 11) },
		{ 2078, new DateOnly(2078, 04, 03) }, { 2079, new DateOnly(2079, 04, 23) }, { 2080, new DateOnly(2080, 04, 07) },
		{ 2081, new DateOnly(2081, 03, 30) }, { 2082, new DateOnly(2082, 04, 19) }, { 2083, new DateOnly(2083, 04, 04) },
		{ 2084, new DateOnly(2084, 03, 26) }, { 2085, new DateOnly(2085, 04, 15) }, { 2086, new DateOnly(2086, 03, 31) },
		{ 2087, new DateOnly(2087, 04, 20) }, { 2088, new DateOnly(2088, 04, 11) }, { 2089, new DateOnly(2089, 04, 03) },

		{ 2090, new DateOnly(2090, 04, 16) }, { 2091, new DateOnly(2091, 04, 08) }, { 2092, new DateOnly(2092, 03, 30) },
		{ 2093, new DateOnly(2093, 04, 12) }, { 2094, new DateOnly(2094, 04, 04) }, { 2095, new DateOnly(2095, 04, 24) },
		{ 2096, new DateOnly(2096, 04, 15) }, { 2097, new DateOnly(2097, 03, 31) }, { 2098, new DateOnly(2098, 04, 20) },
		{ 2099, new DateOnly(2099, 04, 12) },
	};
}

enum Token
{
	Date,
	Day,
	Month,
	Year,
	DayOfWeek,
	DayNumber,
	DayOfYear,
	Easter,
	EndOfMonth,
}

enum ComparisonType
{
	Date,
	DayOfWeek,
	Value,
}