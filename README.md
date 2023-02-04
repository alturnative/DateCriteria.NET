# DateCriteria.NET

A date flag calculator that accepts very basic but dynamic input criteria.
The constraints within a rule must *all* be satisfied for the rule to be
considered a match for a given date, and *any* of the rules can match
for a date to be considered satisfying that criteria as a whole. This can be
used in any situation where calendar dates need to be flagged as meeting a
specific criteria, such as: business v.s. non-business days.

The status of a particular date is only calculated once, after which the
value is cached and subsequently retrieved directly.

Example:

```csharp
var criteria = new DateCriteria();
criteria.AddRule("Date == EndOfMonth; DayOfWeek != Wednesday");
criteria.AddRule("DayOfWeek == Saturday");
criteria.AddRule("DayOfWeek == Sunday");

criteria.Contains(new DateOnly(2022, 11, 30); // False, end of month falls on a Wednesday
criteria.Contains(new DateOnly(2022, 12, 17); // True, weekend
criteria.Contains(new DateOnly(2023, 01, 31); // True, non-Wednesday end-of-month
```

Supported tokens:
* `Date`
* `Day`
* `Month`
* `Year`
* Days of the week
* `DayOfWeek`
* `DayNumber`
* `DayOfYear`
* `Easter`
* `EndOfMonth`

Supported comparison operators:
* `<`
* `>`
* `<=`
* `>=`
* `==`
* `!=`

Supported arithmetic operators:
* `+`
* `-`

May potentially add some form of manual override for specific dates (if one were to reach
a point where it was less expensive to, say, call a database rather than perform excessive calculations).