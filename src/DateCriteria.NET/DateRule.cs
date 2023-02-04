namespace DateCriteria.NET;

public class DateRule : IDateRule
{
	public string Name { get; } = string.Empty;
	public bool Negate { get; } = false;
	public ISet<DateConstraint> Constraints { get; } = new HashSet<DateConstraint>();

	public DateRule(string rulesString, bool negate = false, string name = "")
	{
		Name = name;
		Negate = negate;
		ConstraintParser.ParseConstraints(rulesString, Constraints.Add);
	}

	public bool Matches(DateOnly date) => Constraints.All(x => x.RuleAction(date)) ^ Negate;
}