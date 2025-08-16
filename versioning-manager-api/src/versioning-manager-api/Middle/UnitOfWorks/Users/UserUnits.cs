using Microsoft.EntityFrameworkCore;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.Models.Requests.Users;
using versioning_manager_api.SystemObjects;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace versioning_manager_api.Middle.UnitOfWorks.Users;

public class UserUnits(VmDatabaseContext db)
{
    public async Task<OperationResult<DbUser>> CreateUserIfNotExistsAsync(UserCreationApiModel model,
        IHashHelper hasher, CancellationToken token = default)
    {
        string username = model.Username.ToLowerInvariant();
        DbUser? user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, token);
        if (user != null) return OperationResult<DbUser>.ConflictResult(null);

        string? roleName = model.Role?.ToLowerInvariant();
        DbRole? role = roleName != null
            ? await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, token)
            : null;

        string salt = hasher.GenerateSalt();
        user = new DbUser
        {
            Username = username,
            Password = hasher.Hash(model.Password, salt),
            Salt = salt,
            Role = role,
            CreationUtc = DateTimeOffset.UtcNow,
            LastUpdateUtc = DateTimeOffset.UtcNow,
            IsActive = true
        };

        await db.AddAsync(user, token);
        await db.SaveChangesAsync(token);
        return OperationResult<DbUser>.SuccessResult(user);
    }

    public async Task<OperationResult<DbUser?>> LoginUserAsync(UserLoginModel model, IHashHelper hasher,
        CancellationToken token = default)
    {
        string username = model.Username.ToLowerInvariant();
        DbUser? user = await db.Users.Include(u => u.Role).AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, token);
        if (user == null) return OperationResult<DbUser?>.NotFoundResult(null);

        string hashedPassword = hasher.Hash(model.Password, user.Salt);
        return user.Password == hashedPassword
            ? OperationResult<DbUser?>.SuccessResult(user)
            : OperationResult<DbUser?>.FailureResult(null);
    }

    public async Task<OperationResult<DbRole>> CreateRoleAsync(CreateRoleModel model, CancellationToken token = default)
    {
        string roleName = model.Name.ToLowerInvariant();
        DbRole? role = await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, token);
        if (role != null) return OperationResult<DbRole>.ConflictResult(null);

        role = new DbRole
        {
            Name = roleName,
            Roles = model.Roles
        };

        await db.AddAsync(role, token);
        await db.SaveChangesAsync(token);
        return OperationResult<DbRole>.SuccessResult(role);
    }

    public async Task<OperationResult<object>> ChangePasswordAsync(string username, ChangePasswordModel model,
        IHashHelper hasher, CancellationToken token = default)
    {
        username = username.ToLowerInvariant();
        DbUser? user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, token);
        if (user == null) return OperationResult<object>.NotFoundResult(null);

        if (user.Password != hasher.Hash(model.CurrentPassword, user.Salt))
            return OperationResult<object>.FailureResult(null);

        user.Password = hasher.Hash(model.NewPassword, user.Salt);
        user.LastUpdateUtc = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(token);
        return OperationResult<object>.SuccessResult(null);
    }

    public async Task<IEnumerable<DbRole>> GetAllRolesAsync(CancellationToken token = default)
    {
        return await db.Roles.AsNoTracking().ToListAsync(token);
    }

    public async Task<OperationResult> DeleteRoleAsync(string roleName, CancellationToken token = default)
    {
        roleName = roleName.ToLowerInvariant();
        int count = await db.Roles.Where(r => r.Name == roleName).ExecuteDeleteAsync(token);
        return count > 0 ? OperationResult.Success : OperationResult.NotFound;
    }

    public async Task<OperationResult> UpdateRoleAsync(string roleName, IEnumerable<string> newRoles,
        CancellationToken token = default)
    {
        roleName = roleName.ToLowerInvariant();
        int count = await db.Roles.Where(r => r.Name == roleName)
            .ExecuteUpdateAsync(sp => sp.SetProperty(r => r.Roles, newRoles), token);
        return count > 0 ? OperationResult.Success : OperationResult.NotFound;
    }

    public async Task<OperationResult> ChangeUserRoleAsync(string username, string roleName,
        CancellationToken token = default)
    {
        username = username.ToLowerInvariant();
        roleName = roleName.ToLowerInvariant();
        DbUser? user =
            await db.Users.FirstOrDefaultAsync(
                u => u.Username == username, token);
        if (user == null) return OperationResult.NotFound;

        DbRole? role =
            await db.Roles.FirstOrDefaultAsync(r => r.Name == roleName,
                token);
        if (role == null) return OperationResult.NotFound;

        user.Role = role;
        user.LastUpdateUtc = DateTimeOffset.UtcNow;
        return OperationResult.Success;
    }

    public async Task<OperationResult> UpdateUserIsActiveAsync(string username, bool isActive,
        CancellationToken token = default)
    {
        username = username.ToLowerInvariant();
        int count = await db.Users.Where(u => u.Username == username)
            .ExecuteUpdateAsync(sp => sp.SetProperty(u => u.IsActive, isActive), token);
        return count > 0 ? OperationResult.Success : OperationResult.NotFound;
    }

    public async Task<IEnumerable<DbUser>> GetAllUsersAsync(CancellationToken token = default)
    {
        return await db.Users.Include(u => u.Role).AsNoTracking().ToListAsync(token);
    }

    public async Task<IEnumerable<DbUser>> GetActiveUsersAsync(CancellationToken token = default)
    {
        return await db.Users.Include(u => u.Role).AsNoTracking().Where(u => u.IsActive).ToListAsync(token);
    }

    public async Task<DbUser?> GetUserAsync(string username, CancellationToken token = default)
    {
        return await db.Users.Include(u => u.Role).AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase), token);
    }
}