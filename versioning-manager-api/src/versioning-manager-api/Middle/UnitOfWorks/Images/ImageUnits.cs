using Microsoft.EntityFrameworkCore;
using versioning_manager_api.DevDatabase;
using versioning_manager_api.Middle.Docker;
using versioning_manager_api.Middle.DockerCompose;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Responses.Images;
using versioning_manager_api.SystemObjects;

namespace versioning_manager_api.Middle.UnitOfWorks.Images;

/// <summary>
/// The image units.
/// </summary>
/// <param name="db">The database context.</param>
public class ImageUnits(VmDatabaseContext db, DockerController docker, DockerComposeHelper composeHelper)
{
    /// <summary>
    /// Gets the image file.
    /// </summary>
    /// <param name="id">The image id.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns><see cref="Stream"/> with image tar archive if exists; otherwise <c>null</c>.</returns>
    public async Task<Stream?> GetImageFileAsync(int id, CancellationToken token = default)
    {
        DbImageInfo? image = await db.Images.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, token);
        if (image == null)
        {
            return null;
        }

        if (!await docker.IsImageExistsAsync(image.ImageTag, token))
        {
            await docker.PullImageFromGitlabAsync(image.ImageTag, token);
        }

        return await docker.GetImageFileAsync(image.ImageTag, token);
    }

    /// <summary>
    /// Uploads the image info to database.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="creatorId">The creator id.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<OperationResult> UploadImageInfoAsync(UploadImageInfoModel model, Guid creatorId,
        CancellationToken token = default)
    {
        DbDevice? device = await db.Devices.FirstOrDefaultAsync(d => d.Id == creatorId, token);
        if (device == null)
        {
            return OperationResult.Failure;
        }

        DbProject? project =
            await db.Projects.Include(p => p.Entries)
                .FirstOrDefaultAsync(p => p.Name == model.ProjectName.ToLowerInvariant(), token);

        DbProjectEntry? entry = project?.Entries?.FirstOrDefault(e => e.IsActual);
        if (entry == null)
        {
            return OperationResult.NotFound;
        }

        IEnumerable<DbImageInfo> imagesFromService = await db.Images
            .Where(i => i.ProjectId == entry.Id && i.ServiceName == model.ServiceName).ToListAsync(token);
        foreach (DbImageInfo imageInfo in imagesFromService)
        {
            imageInfo.IsActive = false;
        }

        DbImageInfo? image =
            imagesFromService.FirstOrDefault(i => i.ImageTag == model.ImageTag);

        await docker.PullImageFromGitlabAsync(model.ImageTag, token);

        if (image == null)
        {
            image = new DbImageInfo
            {
                Project = entry,
                ImageTag = model.ImageTag,
                CreationUTC = DateTimeOffset.UtcNow,
                DockerCompose = model.DockerCompose,
                ServiceName = model.ServiceName,
                Version = model.Version,
                IsActive = true,
                Creator = device
            };
            await db.Images.AddAsync(image, token);
            await db.SaveChangesAsync(token);
            return OperationResult.Success;
        }

        image.ImageTag = model.ImageTag;
        image.CreationUTC = DateTimeOffset.UtcNow;
        image.DockerCompose = model.DockerCompose;
        image.ServiceName = model.ServiceName;
        image.Version = model.Version;
        image.IsActive = true;
        image.Creator = device;
        await db.SaveChangesAsync(token);
        return OperationResult.Success;
    }

    /// <summary>
    /// Gets the project info for device.
    /// </summary>
    /// <param name="projectName">The project name.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    public async Task<DeviceProjectInfoResponse> GetProjectInfoAsync(string projectName,
        CancellationToken token = default)
    {
        List<DbProjectEntry> entry = await db.ProjectEntries
            .Include(e => e.Project)
            .Include(e => e.Images.Where(i => i.IsActive))
            .AsNoTracking()
            .Where(e => e.Project.Name == projectName.ToLowerInvariant() && e.IsActual).ToListAsync(token);
        return new DeviceProjectInfoResponse
        {
            Name = projectName,
            ActualEntries = entry.Select(e => new DeviceProjectEntryInfo
            {
                Id = e.Id,
                Version = e.Version,
                Images = e.Images?.Select(i => new DeviceImageInfoResponse
                {
                    Id = i.Id,
                    Tag = i.ImageTag
                }).ToArray() ?? []
            }).ToArray()
        };
    }

    /// <summary>
    /// Gets the project docker-compose file.
    /// </summary>
    /// <param name="projectEntryId">The project entry id.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns><see cref="Stream"/> of docker-compose file if project entry exists; otherwise <c>null</c>.</returns>
    public async Task<Stream?> GetProjectDockerComposeAsync(int projectEntryId, CancellationToken token = default)
    {
        List<DbImageInfo> images = await db.Images
            .Include(i => i.Project)
            .AsNoTracking()
            .Where(i => i.ProjectId == projectEntryId && i.Project.IsActual && i.IsActive)
            .ToListAsync(token);
        return images.Count > 0 ? composeHelper.GetTotalCompose(images.Select(i => i.DockerCompose)) : null;
    }
}