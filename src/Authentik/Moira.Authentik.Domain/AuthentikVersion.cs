namespace Moira.Authentik.Domain;

public readonly record struct AuthentikVersion(int Year, int Month, int Patch) : IComparable<AuthentikVersion>
{
    public static AuthentikVersion Parse(string version)
    {
        var parts = version.Split('.');
        return new AuthentikVersion(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }

    public int CompareTo(AuthentikVersion other)
    {
        var yearCmp = Year.CompareTo(other.Year);
        if (yearCmp != 0) return yearCmp;
        var monthCmp = Month.CompareTo(other.Month);
        if (monthCmp != 0) return monthCmp;
        return Patch.CompareTo(other.Patch);
    }

    public static bool operator <(AuthentikVersion left, AuthentikVersion right) => left.CompareTo(right) < 0;
    public static bool operator >(AuthentikVersion left, AuthentikVersion right) => left.CompareTo(right) > 0;
    public static bool operator <=(AuthentikVersion left, AuthentikVersion right) => left.CompareTo(right) <= 0;
    public static bool operator >=(AuthentikVersion left, AuthentikVersion right) => left.CompareTo(right) >= 0;
}
