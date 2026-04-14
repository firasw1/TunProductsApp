using Microsoft.EntityFrameworkCore;
using SimilarProducts.Domain.Entities;
using SimilarProducts.Domain.Enums;
using SimilarProducts.Domain.Interfaces.Repositories;
using SimilarProducts.Infrastructure.Data;

namespace SimilarProducts.Infrastructure.Repositories;

public class ThemeRepository : GenericRepository<Theme>, IThemeRepository
{
    public ThemeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Theme?> GetByIdWithTagsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(t => t.ThemeTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Theme>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbSet
            .AsNoTracking()
            .Where(t =>
                t.IsActive &&
                (
                    t.Type != ThemeType.Event ||
                    (
                        (!t.StartDate.HasValue || t.StartDate <= now) &&
                        (!t.EndDate.HasValue || t.EndDate >= now)
                    )
                ))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Theme>> GetByTypeAsync(
        ThemeType themeType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.Type == themeType)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Theme>> GetSystemThemesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(t => t.Type == ThemeType.System)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        name = name.Trim();

        return await _dbSet
            .AsNoTracking()
            .AnyAsync(t => t.Name == name, cancellationToken);
    }
}