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