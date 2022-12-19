namespace DateCriteria.NET;

public class DateCriteria : IDateCriteria
{
	public bool Negate { get; set; } = false;
	public IList<IDateCriterion> Criteria { get; } = new List<IDateCriterion>();

	public void AddCriterion(string input, bool negate = false)
	{
		Criteria.Add(new DateCriterion(input, negate));
	}

	public bool Contains(DateOnly date) => Criteria.Any(x => x.Matches(date)) ^ Negate;
}