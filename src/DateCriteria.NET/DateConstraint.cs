namespace DateCriteria.NET;

public class DateConstraint : IEquatable<DateConstraint>
{
	internal string Text { get; init; } = string.Empty;
	internal Func<DateOnly, bool> Action { get; init; } = null!;

	public override string ToString() => Text;

	#region Equality Members
	
	public bool Equals(DateConstraint? other)
	{
		return string.Equals(Text, other?.Text, StringComparison.OrdinalIgnoreCase);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((DateConstraint)obj);
	}

	public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Text);

	public static bool operator ==(DateConstraint? left, DateConstraint? right) => Equals(left, right);

	public static bool operator !=(DateConstraint? left, DateConstraint? right) => !Equals(left, right);

	#endregion
}