using System.Text.Json.Serialization;

namespace Nutq.Core.Models
{
    public class SessionDataModel
    {
        [JsonPropertyName("exerciseType")]
        public string? ExerciseType { get; set; }

        [JsonPropertyName("startedAt")]
        public string? StartedAt { get; set; }

        [JsonPropertyName("completedAt")]
        public string? CompletedAt { get; set; }

        [JsonPropertyName("totalDurationSeconds")]
        public int TotalDurationSeconds { get; set; }

        [JsonPropertyName("overallAccuracyPercent")]
        public double OverallAccuracyPercent { get; set; }

        [JsonPropertyName("repetitions")]
        public List<RepetitionDataModel> Repetitions { get; set; } = new();
    }

    public class RepetitionDataModel
    {
        [JsonPropertyName("repetitionNumber")]
        public int RepetitionNumber { get; set; }

        [JsonPropertyName("completedAt")]
        public string? CompletedAt { get; set; }

        [JsonPropertyName("words")]
        public List<WordDataModel> Words { get; set; } = new();

        [JsonPropertyName("accuracyPercent")]
        public double AccuracyPercent { get; set; }

        [JsonPropertyName("durationSeconds")]
        public int DurationSeconds { get; set; }
    }

    public class WordDataModel
    {
        [JsonPropertyName("wordId")]
        public int WordId { get; set; }

        [JsonPropertyName("wordEnglish")]
        public string WordEnglish { get; set; } = string.Empty;

        [JsonPropertyName("wordArabic")]
        public string WordArabic { get; set; } = string.Empty;

        [JsonPropertyName("attempts")]
        public int Attempts { get; set; }

        [JsonPropertyName("audioPlays")]
        public int AudioPlays { get; set; }

        [JsonPropertyName("firstTryCorrect")]
        public bool FirstTryCorrect { get; set; }

        [JsonPropertyName("timeSpentSeconds")]
        public int TimeSpentSeconds { get; set; }

        [JsonPropertyName("speechAttempts")]
        public List<SpeechAttemptDataModel>? SpeechAttempts { get; set; }
    }

    public class SpeechAttemptDataModel
    {
        [JsonPropertyName("attemptNumber")]
        public int AttemptNumber { get; set; }

        [JsonPropertyName("expectedWord")]
        public string ExpectedWord { get; set; } = string.Empty;

        [JsonPropertyName("recognizedWord")]
        public string RecognizedWord { get; set; } = string.Empty;

        [JsonPropertyName("similarityScore")]
        public double SimilarityScore { get; set; }

        [JsonPropertyName("isCorrect")]
        public bool IsCorrect { get; set; }

        [JsonPropertyName("audioDurationSeconds")]
        public double AudioDurationSeconds { get; set; }

        [JsonPropertyName("attemptedAt")]
        public string? AttemptedAt { get; set; }
    }

    public record ParsedSpeechAttempt(
        int? VocabularyId,
        string ExpectedWord,
        string RecognizedWord,
        double SimilarityScore,
        int AttemptNumber,
        bool IsCorrect,
        double AudioDurationSeconds,
        string? Category,
        DateTime AttemptedAt,
        bool FirstTryCorrectForWord);
}
