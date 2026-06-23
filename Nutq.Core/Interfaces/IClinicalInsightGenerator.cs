using System.Collections.Generic;
using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IClinicalInsightGenerator
    {
        PlanClinicalInsights GenerateInsights(
            IReadOnlyList<PlanWordPerformance> words,
            IReadOnlyList<PlanCategoryPerformance> categories,
            IReadOnlyList<SessionClinicalReport> reports,
            PlanStrengthAnalysis strengths,
            PlanWeaknessAnalysis weaknesses);
    }
}
