using System.Collections.Concurrent;

namespace DateCriteria.NET;

public class DateCriteria : IDateCriteria
{
	private ConcurrentDictionary<DateOnly, bool> _cache = new ConcurrentDictionary<DateOnly, bool>();

	public bool Negate { get; set; } = false;
	public IList<IDateRule> Rules { get; } = new List<IDateRule>();

	public void AddRule(string input, bool negate = false)
	{
		Rules.Add(new DateRule(input, negate));
	}

	public bool Contains(DateOnly date) => _cache.GetOrAdd(date, ContainsPrivate(date));

	public void RefreshCache()
	{
		foreach (DateOnly date in _cache.Keys) _cache[date] = ContainsPrivate(date);
	}

	private bool ContainsPrivate(DateOnly date) => Rules.Any(x => x.Matches(date)) ^ Negate;
}