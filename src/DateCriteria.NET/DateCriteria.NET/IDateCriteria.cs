namespace DateCriteria.NET;

public interface IDateCriteria
{
	bool Negate { get; set; }
	IList<IDateCriterion> Criteria { get; }
	void AddCriterion(string input, bool negate);

	bool Contains(DateOnly date);
}