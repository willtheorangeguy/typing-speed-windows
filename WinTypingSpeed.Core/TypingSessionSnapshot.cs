namespace WinTypingSpeed.Core;

public sealed record TypingSessionSnapshot(
    DateTimeOffset SessionStartedAt,
    bool IsPaused,
    int TypedCharacterCount,
    int EstimatedWordCount,
    TimeSpan ActiveTime,
    double CurrentWpm,
    DateTimeOffset? LastInputAt);
