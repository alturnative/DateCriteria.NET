namespace DateCriteria.NET;

public class DateConstraint
{
	internal string RuleText { get; set; } = string.Empty;
	internal Func<DateOnly, bool> RuleAction { get; set; } = null!;

	public override string ToString() => RuleText;

	protected bool Equals(DateConstraint other)
	{
		return string.Equals(RuleText, other.RuleText, StringComparison.OrdinalIgnoreCase);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((DateConstraint)obj);
	}

	public override int GetHashCode()
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(RuleText);
	}

	public static bool operator ==(DateConstraint? left, DateConstraint? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(DateConstraint? left, DateConstraint? right)
	{
		return !Equals(left, right);
	}
}