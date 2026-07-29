namespace Events.Application.Caching;

public class CacheOptions
{
    public int EventTtlSeconds { get; set; } = 300;
    public int TopEventsTtlSeconds { get; set; } = 60;
}
