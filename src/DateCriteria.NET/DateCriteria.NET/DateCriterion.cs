namespace DateCriteria.NET;

public class DateCriterion : IDateCriterion
{
	public bool Negate { get; } = false;
	public ISet<Rule> Rules { get; } = new HashSet<Rule>();

	public DateCriterion(string rulesString, bool negate = false)
	{
		Negate = negate;
		RuleParser.ParseRules(rulesString, Rules.Add);
	}

	public bool Matches(DateOnly date) => Rules.All(x => x.RuleAction(date)) ^ Negate;
}