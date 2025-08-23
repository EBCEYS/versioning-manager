using Microsoft.EntityFrameworkCore;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.Models.Requests.Devices;
using versioning_manager_api.SystemObjects;

namespace versioning_manager_api.Middle.UnitOfWorks.Devices;

public class DeviceUnits(VmDatabaseContext db)
{
    public async Task<OperationResult<DbDevice>> CreateDeviceAsync(Guid id, string creatorUsername, string key,
        CreateDeviceModel model, IHashHelper hasher, CancellationToken token = default)
    {
        creatorUsername = creatorUsername.ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == creatorUsername, token);
        if (user == null) return OperationResult<DbDevice>.NotFoundResult(null);

        var salt = hasher.GenerateSalt();
        DbDevice device = new()
        {
            Id = id,
            Creator = user,
            ExpireUTC = model.ExpiresUtc,
            IsActive = true,
            KeyHash = hasher.Hash(key, salt),
            SourceHash = hasher.Hash(model.Source.ToLowerInvariant(), hasher.DefaultSalt),
            Salt = salt,
            CreationUTC = DateTimeOffset.UtcNow
        };
        var resultDevice = await db.Devices.AddAsync(device, token);
        await db.SaveChangesAsync(token);
        return OperationResult<DbDevice>.SuccessResult(resultDevice.Entity);
    }

    public async Task<IEnumerable<DbDevice>> GetActiveDevicesAsync(CancellationToken token = default)
    {
        return await db.Devices.AsNoTracking().Where(d => d.IsActive).ToListAsync(token);
    }

    public async Task<IEnumerable<DbDevice>> GetAllDevicesAsync(CancellationToken token = default)
    {
        return await db.Devices.AsNoTracking().ToListAsync(token);
    }

    public async Task<DbDevice?> GetDeviceInfo(Guid id, CancellationToken token = default)
    {
        return await db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, token);
    }

    public async Task<OperationResult<DbDevice>> UpdateDeviceAsync(UpdateDeviceModel model, IHashHelper hasher,
        string newKey, CancellationToken token = default)
    {
        var device = await db.Devices.FirstOrDefaultAsync(d => d.Id == model.DeviceKey, token);
        if (device == null) return OperationResult<DbDevice>.NotFoundResult(null);

        var newKeyHash = hasher.Hash(newKey, device.Salt);
        var newSourceHash = hasher.Hash(model.Source, hasher.DefaultSalt);

        device.IsActive = true;
        device.KeyHash = newKeyHash;
        device.SourceHash = newSourceHash;
        device.ExpireUTC = model.ExpiresUtc;

        await db.SaveChangesAsync(token);
        return OperationResult<DbDevice>.SuccessResult(device);
    }

    public async Task<OperationResult<object>> DeleteDeviceAsync(Guid deviceId, CancellationToken token = default)
    {
        var device = await db.Devices.FirstOrDefaultAsync(d => d.Id == deviceId, token);
        if (device == null) return OperationResult<object>.NotFoundResult(null);

        device.IsActive = false;
        await db.SaveChangesAsync(token);
        return OperationResult<object>.SuccessResult(null);
    }

    public async Task<IEnumerable<DbDevice>> GetDevicesBySource(string source, IHashHelper hasher,
        CancellationToken token = default)
    {
        var sourceHash = hasher.Hash(source.ToLowerInvariant(), hasher.DefaultSalt);
        return await db.Devices.AsNoTracking().Where(d => d.SourceHash == sourceHash).ToListAsync(token);
    }
}