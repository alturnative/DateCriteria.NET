namespace DateCriteria.NET;

public interface IDateCriteria
{
	bool Negate { get; set; }
	IList<IDateRule> Rules { get; }
	void AddRule(string input, bool negate);

	bool Contains(DateOnly date);
}