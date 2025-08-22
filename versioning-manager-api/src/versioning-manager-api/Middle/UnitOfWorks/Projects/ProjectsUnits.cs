using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.Middle.Docker;
using versioning_manager_api.SystemObjects;

namespace versioning_manager_api.Middle.UnitOfWorks.Projects;

/// <summary>
///     The project units of work.
/// </summary>
/// <param name="db">The database context.</param>
public class ProjectsUnits(VmDatabaseContext db, IDockerController docker)
{
    /// <summary>
    ///     Creates a new project.
    /// </summary>
    /// <param name="creatorName">The creator username.</param>
    /// <param name="projectName">The new project name.</param>
    /// <param name="availableSources">The list of available sources.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> CreateProjectAsync(string creatorName, string projectName,
        IEnumerable<string> availableSources, CancellationToken token = default)
    {
        DbUser? creator = await db.Users.FirstOrDefaultAsync(u => u.Username == creatorName.ToLowerInvariant(), token);
        if (creator == null) return OperationResult.NotFound;

        DbProject? project =
            await db.Projects.FirstOrDefaultAsync(p => p.Name == projectName.ToLowerInvariant(), token);
        if (project != null) return OperationResult.Conflict;

        project = new DbProject
        {
            Name = projectName.ToLowerInvariant(),
            Creator = creator,
            AvailableSources = availableSources.Select(s => s.ToLowerInvariant()).ToArray()
        };
        await db.Projects.AddAsync(project, token);
        await db.SaveChangesAsync(token);
        return OperationResult.Success;
    }

    /// <summary>
    ///     Creates a new project entry.
    /// </summary>
    /// <param name="creatorUsername">The creator username.</param>
    /// <param name="projectName">The project name.</param>
    /// <param name="version">The version.</param>
    /// <param name="isActual">Is actual entry.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> CreateProjectEntryAsync(string creatorUsername, string projectName,
        string version, bool isActual, CancellationToken token = default)
    {
        DbUser? user = await db.Users.FirstOrDefaultAsync(u => u.Username == creatorUsername.ToLowerInvariant(), token);
        if (user == null) return OperationResult.NotFound;

        DbProject? project = await db.Projects.Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.Name == projectName.ToLowerInvariant(), token);
        if (project == null) return OperationResult.NotFound;

        if (project.Entries?.Any(e => e.Version == version.ToLowerInvariant()) ?? false)
            return OperationResult.Conflict;

        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (isActual && project.Entries != null)
            foreach (DbProjectEntry existingEntry in project.Entries)
            {
                existingEntry.IsActual = false;
                existingEntry.LastUpdateUTC = now;
            }

        DbProjectEntry entry = new()
        {
            Project = project,
            Version = version.ToLowerInvariant(),
            IsActual = isActual,
            LastUpdateUTC = now
        };
        await db.ProjectEntries.AddAsync(entry, token);
        await db.SaveChangesAsync(token);
        return OperationResult.Success;
    }

    /// <summary>
    ///     Gets all project.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<IEnumerable<DbProject>> GetAllProjectsAsync(CancellationToken token = default)
    {
        return await db.Projects.AsNoTracking().ToListAsync(token);
    }

    /// <summary>
    ///     Gets all project entries.
    /// </summary>
    /// <param name="projectName">The project name.</param>
    /// <param name="onlyActual">Return only actual entries.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult<IEnumerable<DbProjectEntry>>> GetAllProjectEntriesAsync(string projectName,
        bool onlyActual,
        CancellationToken token = default)
    {
        DbProject? project = await db.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == projectName.ToLowerInvariant(), token);
        if (project == null) return OperationResult<IEnumerable<DbProjectEntry>>.NotFoundResult(null);

        IEnumerable<DbProjectEntry> entries = await db.ProjectEntries.Include(e => e.Images)
            .Where(e => (e.ProjectId == project.Id && !onlyActual) || e.IsActual).AsNoTracking().ToListAsync(token);

        return OperationResult<IEnumerable<DbProjectEntry>>.SuccessResult(entries);
    }

    /// <summary>
    ///     Changes project entry actuality.
    /// </summary>
    /// <param name="projectEntryId">The project entry id.</param>
    /// <param name="newStatus">The new actuality status.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> ChangeProjectEntryActualityAsync(int projectEntryId, bool newStatus,
        CancellationToken token = default)
    {
        DbProjectEntry? entry = await db.ProjectEntries.FirstOrDefaultAsync(e => e.Id == projectEntryId, token);
        if (entry == null) return OperationResult.NotFound;

        DateTimeOffset now = DateTimeOffset.UtcNow;
        entry.IsActual = newStatus;
        entry.LastUpdateUTC = now;

        if (newStatus)
            await db.ProjectEntries.Where(e => e.ProjectId == entry.ProjectId && e.Id != entry.Id && e.IsActual)
                .ForEachAsync(e =>
                {
                    e.IsActual = false;
                    e.LastUpdateUTC = now;
                }, token);

        await db.SaveChangesAsync(token);
        return OperationResult.Success;
    }

    /// <summary>
    ///     Migrates all project entry images to new entry with new image.
    /// </summary>
    /// <param name="projectEntryId">The old project entry id.</param>
    /// <param name="newVersion">The new version.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> MigrateProjectEntryToNewWithNewVersion(int projectEntryId, string newVersion,
        CancellationToken token = default)
    {
        DbProjectEntry? entry = await db.ProjectEntries.Include(e => e.Project).Include(e => e.Images)
            .FirstOrDefaultAsync(p => p.Id == projectEntryId, token);
        if (entry == null) return OperationResult.NotFound;

        if (await db.ProjectEntries.AnyAsync(
                p => p.ProjectId == entry.ProjectId && p.Version == newVersion.ToLowerInvariant(), token))
            return OperationResult.Conflict;

        DbProjectEntry newEntry = new()
        {
            LastUpdateUTC = DateTimeOffset.UtcNow,
            Project = entry.Project,
            Version = newVersion,
            IsActual = false
        };

        if (entry.Images?.Count > 0)
            foreach (DbImageInfo image in entry.Images)
                image.Project = newEntry;

        await db.ProjectEntries.AddAsync(newEntry, token);
        await db.SaveChangesAsync(token);
        return OperationResult.Success;
    }

    /// <summary>
    ///     Copy images to new project entry.
    /// </summary>
    /// <param name="images">The images.</param>
    /// <param name="projectEntryId">The new project entry id.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> CopyImagesToNewProjectEntry(int[] images, int projectEntryId,
        CancellationToken token = default)
    {
        DbProjectEntry? project = await db.ProjectEntries.FirstOrDefaultAsync(p => p.Id == projectEntryId, token);
        if (project == null) return OperationResult.NotFound;

        List<DbImageInfo> imagesToMigrate = await db.Images.Where(i => images.Contains(i.Id)).ToListAsync(token);
        if (imagesToMigrate.Count != images.Length) return OperationResult.Failure;

        List<DbImageInfo> newImages = [];
        imagesToMigrate.ForEach(i =>
        {
            newImages.Add(new DbImageInfo
            {
                Creator = i.Creator,
                DockerCompose = i.DockerCompose,
                ImageTag = i.ImageTag,
                ServiceName = i.ServiceName,
                Version = i.Version,
                Project = project,
                IsActive = true,
                CreationUTC = DateTimeOffset.UtcNow
            });
        });
        await db.Images.AddRangeAsync(newImages, token);
        await db.SaveChangesAsync(token);
        return OperationResult.Success;
    }

    /// <summary>
    ///     Gets the image infos by project entry.
    /// </summary>
    /// <param name="projectId">The project entry id.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<IEnumerable<DbImageInfo>> GetImageInfosAsync(int projectId, CancellationToken token = default)
    {
        return await db.Images.Include(i => i.Creator).AsNoTracking().Where(i => i.ProjectId == projectId)
            .ToListAsync(token);
    }

    /// <summary>
    ///     Updates project available sources.
    /// </summary>
    /// <param name="projectId">The project id.</param>
    /// <param name="newSources">The new sources.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> UpdateProjectAvailableSources(int projectId, IEnumerable<string> newSources,
        CancellationToken token = default)
    {
        string[] hashedSources = newSources.Select(newS => newS.ToLowerInvariant()).ToArray();
        int count = await db.Projects.Where(p => p.Id == projectId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.AvailableSources,
                    hashedSources),
                token);
        return count > 0 ? OperationResult.Success : OperationResult.NotFound;
    }

    /// <summary>
    ///     Changes the image activity.
    /// </summary>
    /// <param name="id">The image id.</param>
    /// <param name="newState">The new state.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> ChangeImageActivityAsync(int id, bool newState,
        CancellationToken token = default)
    {
        DbImageInfo? image = await db.Images.FirstOrDefaultAsync(i => i.Id == id, token);
        if (image == null) return OperationResult.NotFound;

        image.IsActive = newState;

        if (!newState)
            try
            {
                await docker.RemoveImageAsync(image.ImageTag, token);
            }
            catch (DockerImageNotFoundException)
            {
            }

        await db.SaveChangesAsync(token);
        return OperationResult.Success;
    }
}