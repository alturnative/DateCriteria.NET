namespace DateCriteria.NET;

public class DateRule : IDateRule, IEquatable<DateRule>
{
	public string Name { get; }
	public bool Negate { get; } = false;
	public ISet<DateConstraint> Constraints { get; } = new HashSet<DateConstraint>();

	public DateRule(string rulesString, bool negate = false, string name = "")
	{
		Name = name;
		Negate = negate;
		ConstraintParser.ParseConstraints(rulesString, Constraints.Add);
	}

	public bool Matches(DateOnly date) => Constraints.All(x => x.Action(date)) ^ Negate;

	#region Equality Members
	
	public bool Equals(DateRule? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Name == other.Name && Negate == other.Negate && Constraints.Equals(other.Constraints);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((DateRule)obj);
	}

	public override int GetHashCode() => HashCode.Combine(Name, Negate, Constraints);

	public static bool operator ==(DateRule? left, DateRule? right) => Equals(left, right);

	public static bool operator !=(DateRule? left, DateRule? right) => !Equals(left, right);

	#endregion
}