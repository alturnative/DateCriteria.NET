namespace DateCriteria.NET;

public interface IDateCriteria
{
	bool Negate { get; set; }
	ISet<IDateRule> Rules { get; }
	void AddRule(string input, bool negate, string name);
	void AddRules(params (string input, bool negate, string name)[] rules);

	bool Contains(DateOnly date);
	bool ContainsAny(params DateOnly[] date);
	bool ContainsAll(params DateOnly[] date);
	void RefreshCache();
}