namespace DateCriteria.NET;

public interface IDateCriterion
{
	bool Negate { get; }
	ISet<Rule> Rules { get; }

	bool Matches(DateOnly date);
}