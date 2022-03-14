using System;

static class DateTimeRoundingExtensions
{
    public static DateTimeOffset RoundUp(this DateTimeOffset instance, TimeSpan period)
    {
        return new DateTimeOffset((instance.Ticks + period.Ticks - 1) / period.Ticks * period.Ticks, instance.Offset);
    }

    public static DateTimeOffset RoundDown(this DateTimeOffset instance, TimeSpan period)
    {
        var delta = instance.Ticks % period.Ticks;
        return new DateTimeOffset(instance.Ticks - delta, instance.Offset);
    }

    public static DateTimeOffset Round(this DateTimeOffset value, TimeSpan period, MidpointRounding style = default)
    {
        if (period <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(period), "value must be positive");

        var units = (decimal)value.Ticks / period.Ticks; // Conversion to decimal not to loose precision
        var roundedUnits = Math.Round(units, style);
        var roundedTicks = (long)roundedUnits * period.Ticks;
        var instance = new DateTimeOffset(roundedTicks,value.Offset);
        return instance;
    }

    public static TimeSpan RoundDown(this TimeSpan instance, TimeSpan period)
    {
        var delta = instance.Ticks % period.Ticks;
        return new TimeSpan(instance.Ticks - delta);
    }

    public static TimeSpan RoundUp(this TimeSpan instance, TimeSpan period)
    {
        return new TimeSpan((instance.Ticks + period.Ticks - 1) / period.Ticks * period.Ticks);
    }

    public static TimeSpan Round(this TimeSpan instance, TimeSpan period)
    {
        if (period == TimeSpan.Zero) return instance;

        var rndTicks = period.Ticks;
        var ansTicks = instance.Ticks + Math.Sign(instance.Ticks) * rndTicks / 2;
        return TimeSpan.FromTicks(ansTicks - ansTicks % rndTicks);
    }
}