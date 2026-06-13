using WinTypingSpeed.Core;

namespace WinTypingSpeed.Core.Tests;

public class TypingSessionTrackerTests
{
    [Fact]
    public void RecordCharacter_CountsCharactersAndWhitespaceDelimitedWords()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);
        var sample = "hello world ";

        for (var index = 0; index < sample.Length; index++)
        {
            tracker.RecordCharacter(sample[index], sessionStart.AddSeconds(index));
        }

        var snapshot = tracker.GetSnapshot(sessionStart.AddMinutes(1));

        Assert.Equal(sample.Length, snapshot.TypedCharacterCount);
        Assert.Equal(2, snapshot.EstimatedWordCount);
        Assert.Equal(TimeSpan.FromMinutes(1), snapshot.ActiveTime);
        Assert.Equal(2d, snapshot.CurrentWpm, 3);
    }

    [Fact]
    public void CurrentWpm_IncludesTheInProgressWord()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.RecordCharacter('h', sessionStart.AddSeconds(1));
        tracker.RecordCharacter('i', sessionStart.AddSeconds(2));

        var snapshot = tracker.GetSnapshot(sessionStart.AddMinutes(1));

        Assert.Equal(0, snapshot.EstimatedWordCount);
        Assert.Equal(1d, snapshot.CurrentWpm, 3);
    }

    [Fact]
    public void Pause_StopsActiveTimeAndIgnoresNewCharacters()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.RecordCharacter('a', sessionStart.AddSeconds(10));
        tracker.Pause(sessionStart.AddSeconds(30));
        tracker.RecordCharacter('b', sessionStart.AddSeconds(40));

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(90));

        Assert.True(snapshot.IsPaused);
        Assert.Equal(1, snapshot.TypedCharacterCount);
        Assert.Equal(TimeSpan.FromSeconds(30), snapshot.ActiveTime);
    }

    [Fact]
    public void Resume_RestartsActiveTimeFromTheResumeMoment()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.Pause(sessionStart.AddSeconds(20));
        tracker.Resume(sessionStart.AddSeconds(50));
        tracker.RecordCharacter('a', sessionStart.AddSeconds(55));
        tracker.RecordCharacter(' ', sessionStart.AddSeconds(56));

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(80));

        Assert.False(snapshot.IsPaused);
        Assert.Equal(2, snapshot.TypedCharacterCount);
        Assert.Equal(1, snapshot.EstimatedWordCount);
        Assert.Equal(TimeSpan.FromSeconds(50), snapshot.ActiveTime);
        Assert.Equal(1.2d, snapshot.CurrentWpm, 3);
    }

    [Fact]
    public void Reset_ClearsCountsAndStartsANewSessionWindow()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.RecordCharacter('t', sessionStart.AddSeconds(1));
        tracker.RecordCharacter('e', sessionStart.AddSeconds(2));
        tracker.RecordCharacter('s', sessionStart.AddSeconds(3));
        tracker.RecordCharacter('t', sessionStart.AddSeconds(4));
        tracker.RecordCharacter(' ', sessionStart.AddSeconds(5));
        tracker.Reset(sessionStart.AddSeconds(30));

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(90));

        Assert.Equal(sessionStart.AddSeconds(30), snapshot.SessionStartedAt);
        Assert.Equal(0, snapshot.TypedCharacterCount);
        Assert.Equal(0, snapshot.EstimatedWordCount);
        Assert.Equal(TimeSpan.FromSeconds(60), snapshot.ActiveTime);
        Assert.Equal(0d, snapshot.CurrentWpm, 3);
    }

    [Fact]
    public void GetSnapshot_OnFreshTracker_ReturnsZeroedTrackingState()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        var snapshot = tracker.GetSnapshot(sessionStart);

        Assert.Equal(sessionStart, snapshot.SessionStartedAt);
        Assert.False(snapshot.IsPaused);
        Assert.Equal(0, snapshot.TypedCharacterCount);
        Assert.Equal(0, snapshot.EstimatedWordCount);
        Assert.Equal(TimeSpan.Zero, snapshot.ActiveTime);
        Assert.Equal(0d, snapshot.CurrentWpm, 3);
        Assert.Null(snapshot.LastInputAt);
    }

    [Fact]
    public void RecordCharacter_IgnoresNonWhitespaceControlCharacters()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.RecordCharacter('\b', sessionStart.AddSeconds(1)); // backspace
        tracker.RecordCharacter('\u001b', sessionStart.AddSeconds(2)); // escape
        tracker.RecordCharacter('a', sessionStart.AddSeconds(3));

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(10));

        Assert.Equal(1, snapshot.TypedCharacterCount);
        Assert.Equal(sessionStart.AddSeconds(3), snapshot.LastInputAt);
    }

    [Fact]
    public void RecordCharacter_CollapsesConsecutiveWhitespaceIntoNoExtraWords()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);
        var sample = "  a   b  ";

        for (var index = 0; index < sample.Length; index++)
        {
            tracker.RecordCharacter(sample[index], sessionStart.AddSeconds(index));
        }

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(30));

        Assert.Equal(sample.Length, snapshot.TypedCharacterCount);
        Assert.Equal(2, snapshot.EstimatedWordCount);
    }

    [Fact]
    public void RecordCharacter_UpdatesLastInputTimestamp()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.RecordCharacter('a', sessionStart.AddSeconds(5));
        tracker.RecordCharacter('b', sessionStart.AddSeconds(9));

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(30));

        Assert.Equal(sessionStart.AddSeconds(9), snapshot.LastInputAt);
    }

    [Fact]
    public void Pause_WhenAlreadyPaused_IsIdempotent()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.Pause(sessionStart.AddSeconds(30));
        tracker.Pause(sessionStart.AddSeconds(60)); // should not accumulate more active time

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(90));

        Assert.True(snapshot.IsPaused);
        Assert.Equal(TimeSpan.FromSeconds(30), snapshot.ActiveTime);
    }

    [Fact]
    public void Resume_WhenNotPaused_DoesNotRestartTheActiveSegment()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.Resume(sessionStart.AddSeconds(40)); // no-op: never paused

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(60));

        Assert.False(snapshot.IsPaused);
        Assert.Equal(TimeSpan.FromSeconds(60), snapshot.ActiveTime);
    }

    [Fact]
    public void Reset_WhilePaused_KeepsActiveTimeAtZeroUntilResumed()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.Pause(sessionStart.AddSeconds(10));
        tracker.Reset(sessionStart.AddSeconds(20));

        var pausedSnapshot = tracker.GetSnapshot(sessionStart.AddSeconds(50));
        Assert.True(pausedSnapshot.IsPaused);
        Assert.Equal(sessionStart.AddSeconds(20), pausedSnapshot.SessionStartedAt);
        Assert.Equal(TimeSpan.Zero, pausedSnapshot.ActiveTime);

        tracker.Resume(sessionStart.AddSeconds(50));
        var resumedSnapshot = tracker.GetSnapshot(sessionStart.AddSeconds(80));
        Assert.Equal(TimeSpan.FromSeconds(30), resumedSnapshot.ActiveTime);
    }

    [Fact]
    public void RecordCharacter_WhilePaused_IsIgnored()
    {
        var sessionStart = new DateTimeOffset(2026, 3, 11, 12, 0, 0, TimeSpan.Zero);
        var tracker = new TypingSessionTracker(sessionStart);

        tracker.Pause(sessionStart.AddSeconds(5));
        tracker.RecordCharacter('a', sessionStart.AddSeconds(6));
        tracker.RecordCharacter(' ', sessionStart.AddSeconds(7));

        var snapshot = tracker.GetSnapshot(sessionStart.AddSeconds(10));

        Assert.Equal(0, snapshot.TypedCharacterCount);
        Assert.Equal(0, snapshot.EstimatedWordCount);
        Assert.Null(snapshot.LastInputAt);
    }
}
