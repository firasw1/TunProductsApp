using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;

namespace SimilarProducts.Domain.Interfaces.Repositories;

public interface IThemeRepository : IGenericRepository<Theme>
{
    Task<Theme?> GetByIdWithTagsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Theme>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Theme>> GetByTypeAsync(
        ThemeType themeType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Theme>> GetSystemThemesAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}