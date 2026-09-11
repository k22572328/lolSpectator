namespace LolSpector.Core.RiotApi;

/// <summary>
/// Client-side throttle matching a Riot Development API Key's limits
/// (20 req/1s, 100 req/2min) with a small safety margin, so lolSpector
/// stays under the limit by pacing requests instead of hitting 429s.
/// </summary>
public class RiotRateLimiter(int maxPerSecond = 18, int maxPerTwoMinutes = 95)
{
    private readonly Lock _lock = new();
    private readonly Queue<DateTime> _perSecond = new();
    private readonly Queue<DateTime> _perTwoMinutes = new();

    public async Task WaitAsync(CancellationToken ct = default)
    {
        while (true)
        {
            TimeSpan delay;
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                Prune(_perSecond, now, TimeSpan.FromSeconds(1));
                Prune(_perTwoMinutes, now, TimeSpan.FromMinutes(2));

                if (_perSecond.Count < maxPerSecond && _perTwoMinutes.Count < maxPerTwoMinutes)
                {
                    _perSecond.Enqueue(now);
                    _perTwoMinutes.Enqueue(now);
                    return;
                }

                var waitForSecond = _perSecond.Count >= maxPerSecond
                    ? _perSecond.Peek() + TimeSpan.FromSeconds(1) - now
                    : TimeSpan.Zero;
                var waitForTwoMin = _perTwoMinutes.Count >= maxPerTwoMinutes
                    ? _perTwoMinutes.Peek() + TimeSpan.FromMinutes(2) - now
                    : TimeSpan.Zero;

                delay = waitForSecond > waitForTwoMin ? waitForSecond : waitForTwoMin;
                if (delay < TimeSpan.FromMilliseconds(10)) delay = TimeSpan.FromMilliseconds(10);
            }
            await Task.Delay(delay, ct);
        }
    }

    private static void Prune(Queue<DateTime> queue, DateTime now, TimeSpan window)
    {
        while (queue.Count > 0 && now - queue.Peek() > window) queue.Dequeue();
    }
}
