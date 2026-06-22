using System.Globalization;
using System.Text.Json;
using Nutq.Core.Models;

namespace Nutq.Core.Services
{
    public static class SessionDataParser
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static SessionDataModel? Parse(string? sessionDataJson)
        {
            if (string.IsNullOrWhiteSpace(sessionDataJson)) return null;

            try
            {
                return JsonSerializer.Deserialize<SessionDataModel>(sessionDataJson, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public static List<ParsedSpeechAttempt> ExtractAttempts(
            SessionDataModel? sessionData,
            DateTime sessionStart,
            DateTime sessionEnd,
            string? defaultCategory)
        {
            if (sessionData?.Repetitions == null || sessionData.Repetitions.Count == 0)
                return new List<ParsedSpeechAttempt>();

            var results = new List<ParsedSpeechAttempt>();
            var sessionDefaultTime = sessionEnd;

            foreach (var rep in sessionData.Repetitions)
            {
                var repTime = ParseDateTime(rep.CompletedAt) ?? sessionDefaultTime;

                foreach (var word in rep.Words)
                {
                    var category = AnalyticsHelpers.NormalizeCategory(defaultCategory);
                    var expected = !string.IsNullOrWhiteSpace(word.WordArabic) ? word.WordArabic : word.WordEnglish;

                    if (word.SpeechAttempts != null && word.SpeechAttempts.Count > 0)
                    {
                        foreach (var attempt in word.SpeechAttempts)
                        {
                            var expectedWord = !string.IsNullOrWhiteSpace(attempt.ExpectedWord)
                                ? attempt.ExpectedWord
                                : expected;

                            results.Add(new ParsedSpeechAttempt(
                                word.WordId > 0 ? word.WordId : null,
                                expectedWord,
                                attempt.RecognizedWord ?? string.Empty,
                                attempt.SimilarityScore,
                                attempt.AttemptNumber > 0 ? attempt.AttemptNumber : 1,
                                attempt.IsCorrect,
                                attempt.AudioDurationSeconds,
                                category,
                                ParseDateTime(attempt.AttemptedAt) ?? repTime,
                                attempt.AttemptNumber <= 1 && attempt.IsCorrect));
                        }
                    }
                    else if (word.Attempts > 0)
                    {
                        var isCorrect = word.FirstTryCorrect;
                        var similarity = word.FirstTryCorrect ? 100.0 : Math.Max(0, 100.0 - (word.Attempts - 1) * 15.0);

                        results.Add(new ParsedSpeechAttempt(
                            word.WordId > 0 ? word.WordId : null,
                            expected,
                            isCorrect ? expected : string.Empty,
                            similarity,
                            1,
                            isCorrect,
                            word.TimeSpentSeconds,
                            category,
                            repTime,
                            word.FirstTryCorrect));

                        for (var i = 2; i <= word.Attempts; i++)
                        {
                            results.Add(new ParsedSpeechAttempt(
                                word.WordId > 0 ? word.WordId : null,
                                expected,
                                string.Empty,
                                Math.Max(0, similarity - (i - 1) * 10),
                                i,
                                i == word.Attempts && isCorrect,
                                0,
                                category,
                                repTime,
                                false));
                        }
                    }
                }
            }

            return results;
        }

        private static DateTime? ParseDateTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                return dt;
            return DateTime.TryParse(value, out dt) ? dt : null;
        }
    }
}
