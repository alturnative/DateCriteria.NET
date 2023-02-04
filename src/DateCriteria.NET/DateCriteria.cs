using System.Collections.Concurrent;

namespace DateCriteria.NET;

public class DateCriteria : IDateCriteria
{
	private readonly ConcurrentDictionary<DateOnly, bool> _cache = new();
	private bool _negate = false;

	public bool Negate
	{
		get => _negate;
		set
		{
			_negate = value;
			foreach (var pair in _cache) _cache[pair.Key] = !pair.Value;
		}
	}

	public bool RefreshCacheOnAdd { get; set; } = true;

	public ISet<IDateRule> Rules { get; } = new HashSet<IDateRule>();

	public void AddRule(string input, bool negate = false, string name = "")
	{
		Rules.Add(new DateRule(input, negate, name));
		AfterRulesAdded();
	}

	public void AddRules(params (string input, bool negate, string name)[] rules)
	{
		foreach (var (input, negate, name) in rules) Rules.Add(new DateRule(input, negate, name));
		AfterRulesAdded();
	}

	public bool Contains(DateOnly date) => _cache.GetOrAdd(date, ContainsPrivate(date));

	public bool ContainsAny(params DateOnly[] dates) => dates.Any(Contains);
	
	public bool ContainsAll(params DateOnly[] dates) => dates.All(Contains);

	public void RefreshCache()
	{
		foreach (DateOnly date in _cache.Keys) _cache[date] = ContainsPrivate(date);
	}

	private bool ContainsPrivate(DateOnly date) => Rules.Any(x => x.Matches(date)) ^ Negate;

	private void AfterRulesAdded()
	{
		if (RefreshCacheOnAdd) RefreshCache();
	}
}