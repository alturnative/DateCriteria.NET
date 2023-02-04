namespace DateCriteria.NET;

public class DateRule : IDateRule
{
	public bool Negate { get; } = false;
	public ISet<DateConstraint> Constraints { get; } = new HashSet<DateConstraint>();

	public DateRule(string rulesString, bool negate = false)
	{
		Negate = negate;
		ConstraintParser.ParseConstraints(rulesString, Constraints.Add);
	}

	public bool Matches(DateOnly date) => Constraints.All(x => x.RuleAction(date)) ^ Negate;
}