namespace DateCriteria.NET;

public class Rule
{
	internal string RuleText { get; set; } = string.Empty;
	internal Func<DateOnly, bool> RuleAction { get; set; }

	public override string ToString() => RuleText;

	protected bool Equals(Rule other)
	{
		return string.Equals(RuleText, other.RuleText, StringComparison.OrdinalIgnoreCase);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((Rule)obj);
	}

	public override int GetHashCode()
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(RuleText);
	}

	public static bool operator ==(Rule? left, Rule? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(Rule? left, Rule? right)
	{
		return !Equals(left, right);
	}
}