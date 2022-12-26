namespace DateCriteria.NET;

public interface IDateRule
{
	bool Negate { get; }
	ISet<DateConstraint> Constraints { get; }

	bool Matches(DateOnly date);
}