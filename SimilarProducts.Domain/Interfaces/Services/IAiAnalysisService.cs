using SimilarProducts.Domain.Entities;

namespace SimilarProducts.Domain.Interfaces.Services;

public interface IAiAnalysisService
{
    Task<ProductAiAnalysis> AnalyzeProductAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnalysisAsync(
        int productId,
        CancellationToken cancellationToken = default);
}