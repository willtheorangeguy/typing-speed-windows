namespace WinTypingSpeed.Core;

public sealed class TypingSessionTracker
{
    private readonly object syncRoot = new();

    private DateTimeOffset sessionStartedAt;
    private DateTimeOffset? activeSegmentStartedAt;
    private DateTimeOffset? lastInputAt;
    private TimeSpan accumulatedActiveTime;
    private int typedCharacterCount;
    private int estimatedWordCount;
    private int currentWordCharacterCount;
    private bool isPaused;

    public TypingSessionTracker()
        : this(DateTimeOffset.UtcNow)
    {
    }

    public TypingSessionTracker(DateTimeOffset sessionStartedAt)
    {
        this.sessionStartedAt = sessionStartedAt;
        activeSegmentStartedAt = sessionStartedAt;
    }

    public void RecordCharacter(char character, DateTimeOffset? timestamp = null)
    {
        if (char.IsControl(character) && !char.IsWhiteSpace(character))
        {
            return;
        }

        lock (syncRoot)
        {
            if (isPaused)
            {
                return;
            }

            var now = timestamp ?? DateTimeOffset.UtcNow;

            typedCharacterCount++;
            lastInputAt = now;

            if (char.IsWhiteSpace(character))
            {
                CompleteCurrentWord();
                return;
            }

            currentWordCharacterCount++;
        }
    }

    public void Pause(DateTimeOffset? timestamp = null)
    {
        lock (syncRoot)
        {
            if (isPaused)
            {
                return;
            }

            var now = timestamp ?? DateTimeOffset.UtcNow;
            accumulatedActiveTime += GetCurrentSegmentDuration(now);
            activeSegmentStartedAt = null;
            isPaused = true;
        }
    }

    public void Resume(DateTimeOffset? timestamp = null)
    {
        lock (syncRoot)
        {
            if (!isPaused)
            {
                return;
            }

            var now = timestamp ?? DateTimeOffset.UtcNow;
            activeSegmentStartedAt = now;
            isPaused = false;
        }
    }

    public void Reset(DateTimeOffset? timestamp = null)
    {
        lock (syncRoot)
        {
            var now = timestamp ?? DateTimeOffset.UtcNow;

            sessionStartedAt = now;
            activeSegmentStartedAt = isPaused ? null : now;
            lastInputAt = null;
            accumulatedActiveTime = TimeSpan.Zero;
            typedCharacterCount = 0;
            estimatedWordCount = 0;
            currentWordCharacterCount = 0;
        }
    }

    public TypingSessionSnapshot GetSnapshot(DateTimeOffset? timestamp = null)
    {
        lock (syncRoot)
        {
            var now = timestamp ?? DateTimeOffset.UtcNow;
            var activeTime = accumulatedActiveTime + GetCurrentSegmentDuration(now);
            var effectiveWordCount = estimatedWordCount + (currentWordCharacterCount > 0 ? 1 : 0);
            var currentWpm = activeTime.TotalMinutes > 0
                ? effectiveWordCount / activeTime.TotalMinutes
                : 0;

            return new TypingSessionSnapshot(
                sessionStartedAt,
                isPaused,
                typedCharacterCount,
                estimatedWordCount,
                activeTime,
                currentWpm,
                lastInputAt);
        }
    }

    private void CompleteCurrentWord()
    {
        if (currentWordCharacterCount <= 0)
        {
            return;
        }

        estimatedWordCount++;
        currentWordCharacterCount = 0;
    }

    private TimeSpan GetCurrentSegmentDuration(DateTimeOffset now)
    {
        if (isPaused || activeSegmentStartedAt is null)
        {
            return TimeSpan.Zero;
        }

        return now - activeSegmentStartedAt.Value;
    }
}
