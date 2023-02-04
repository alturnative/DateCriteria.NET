using DateCriteria.NET;

namespace DateCriteria.NET.Tests;

public class DateTests
{
	[Fact]
	public void SpecifiedDatesTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("date != 2022-12-19;date < 2022-12-19;date <= 2022-12-19");
		criteria.AddRule("date == 2022-12-25");
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 15)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 25)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 19)));
	}

	[Fact]
	public void DayOfWeekTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("dayofweek != wednesday;dayofweek == friday");
		criteria.AddRule("dayofweek == sunday");
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 16)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 17)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 18)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 19)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 20)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 21)));
	}

	[Fact]
	public void DayOfWeekNegateTest()
	{
		var criteria = new DateCriteria { Negate = true };
		criteria.AddRule("dayofweek != wednesday;dayofweek == friday");
		criteria.AddRule("dayofweek == sunday");
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 16)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 17)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 18)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 19)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 20)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 21)));
	}

	[Fact]
	public void DayMonthYearTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("day==4;month==12;year==2022");
		criteria.AddRule("year<=2010");
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 21)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 04)));
		Assert.True(criteria.Contains(new DateOnly(2002, 12, 04)));
		Assert.False(criteria.Contains(new DateOnly(2011, 01, 01)));
		Assert.True(criteria.Contains(new DateOnly(2010, 12, 31)));
	}

	[Fact]
	public void EndOfMonthTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("date == endofmonth");
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 31)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 30)));
	}

	[Fact]
	public void DocumentationTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("Date == EndOfMonth; DayOfWeek != Wednesday");
		criteria.AddRule("DayOfWeek == Saturday");
		criteria.AddRule("DayOfWeek == Sunday");

		Assert.False(criteria.Contains(new DateOnly(2022, 11, 30)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 17)));
		Assert.True(criteria.Contains(new DateOnly(2023, 01, 31)));
	}

	[Fact]
	public void EasterTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("Date == Easter");
		Assert.False(criteria.Contains(new DateOnly(1900, 04, 14)));
		Assert.True(criteria.Contains(new DateOnly(1900, 04, 15)));
	}

	[Fact]
	public void EasterMondayTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("Date == Easter + 1");
		Assert.False(criteria.Contains(new DateOnly(1900, 04, 14)));
		Assert.True(criteria.Contains(new DateOnly(1900, 04, 16)));
		Assert.Throws<Exception>(() => criteria.Contains(new DateOnly(3000, 01, 01)));
	}

	[Fact]
	public void ArithmeticTest()
	{
		var criteria = new DateCriteria();
		criteria.AddRule("Date == EndOfMonth - 3");
		criteria.AddRule("Date < EndOfMonth - 10");
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 31)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 28)));
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 21)));
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 20)));
	}

	[Fact]
	public void EmptyTest()
	{
		var criteria = new DateCriteria();
		Assert.False(criteria.Contains(new DateOnly(2022, 12, 10)));
		criteria = new DateCriteria { Negate = true };
		Assert.True(criteria.Contains(new DateOnly(2022, 12, 10)));
	}
}

// Want to create a Criteria Set, which can be named. E.g. business calendar days, friend's birthdays, etc.
// The set will contain criteria against which we can pass in a DateOnly and be told whether that date matches any criteria in the set.
// Usage within the Criteria Set would look like criterion.Matches(date)
// Usage of the set would look like set.Contains(date)
// A Criterion would contain Rules against which ALL rules must match (i.e. AND comparison) for the criterion to be met, whereas
//		matching within a set would only require one criterion to be met (i.e. OR comparison).
//
// Examples of syntax we'd like to support:
//		{{date}} != {{EasterMonday}}		Retrieve token dates, check in/equality
//		{{date}} == {{2022-09-08}}			Retrieve token date, generate specified date, check in/equality
//		{{DayOfWeek}} != {{Sunday}}
//		{{day}} != {{4}}
//		{{date}} + 4 != {{EasterMonday}}
//		{{date}} + 4 != {{EndOfMonth}}
//