using System.Text.RegularExpressions;
using static System.StringSplitOptions;

namespace DateCriteria.NET;

public static class RuleParser
{
	internal static void ParseRules(string input, Func<Rule, bool> addRuleAction)
	{
		var stringRules = input.Split(";", TrimEntries | RemoveEmptyEntries);
		foreach (var rule in stringRules) addRuleAction(ParseRule(rule));
	}

	private static Rule ParseRule(string input)
	{
		var args = Operators.Comparison.Split(input);
		if (args.Length != 3) throw new Exception($"Invalid rule definition in '{input}'.");
		return HandleOperation(args);
	}

	private static Rule HandleOperation(string[] args)
	{
		var lTrim = args[0].Trim();
		var op = args[1].Trim();
		var rTrim = args[2].Trim();
		var ruleText = $"{lTrim}{op}{rTrim}";
		Func<DateOnly, ValueObject> left = GetValueFunction(lTrim, out Token token);
		Func<DateOnly, ValueObject> right = GetValueFunction(rTrim, out _);


		return token switch
		{
			Token.Date => new Rule
				{ RuleAction = x => DateComparers[op](left(x).Date.Value, right(x).Date.Value), RuleText = ruleText },
			Token.DayOfWeek => new Rule
				{ RuleAction = x => DayOfWeekComparers[op](left(x).DayOfWeek.Value, right(x).DayOfWeek.Value), RuleText = ruleText },
			Token.Day or Token.Month or Token.Year => new Rule
				{ RuleAction = x => ValueComparers[op](left(x).Value.Value, right(x).Value.Value), RuleText = ruleText }
		};
	}

	private static Func<DateOnly, ValueObject> GetValueFunction(string input, out Token token)
	{
		token = 0;
		if (DateOnly.TryParseExact(input, "yyyy-MM-dd", out var date)) return x => new ValueObject { Date = date };
		if (int.TryParse(input, out int result)) return x => new ValueObject { Value = result };
		
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
					Token.Day => x => new ValueObject { Value = x.Day },
					Token.Month => x => new ValueObject { Value = x.Month },
					Token.Year => x => new ValueObject { Value = x.Year },
					Token.EndOfMonth => x => new ValueObject { Date = new DateOnly(x.Year, x.Month, DateTime.DaysInMonth(x.Year, x.Month)) },
					_ => throw new NotImplementedException(),
				};
			}

			if (Enum.TryParse(trimmed, true, out DayOfWeek dow)) return x => new ValueObject { DayOfWeek = dow };
		}

		
		throw new NotImplementedException();
	}
	
	private static class Operators
	{
		internal static readonly Regex Comparison = new Regex(@"(?<![<>=])(!=|[<>]=?|==)(?![<>=])");
		internal static readonly Regex Arithmetic = new Regex(@"[+\-\*%/]|\*\*");
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

	private struct ValueObject
	{
		internal DateOnly? Date;
		internal int? Value;
		internal DayOfWeek? DayOfWeek;
	}

	private static bool IsEasterMonday(DateOnly date) => (date.Year, date.Month, date.Day) is not ((1970, 03, 30) or (1971, 04, 12));
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
	EasterMonday,
	EndOfMonth,
}