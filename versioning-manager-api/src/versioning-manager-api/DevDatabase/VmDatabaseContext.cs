using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using versioning_manager_api.StaticStorages;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable InconsistentNaming

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace versioning_manager_api.DevDatabase;

public class VmDatabaseContext : DbContext
{
    public VmDatabaseContext(DbContextOptions<VmDatabaseContext> opts) : base(opts)
    {
    }

    public VmDatabaseContext()
    {
    }

    public virtual DbSet<DbUser> Users { get; set; }
    public virtual DbSet<DbRole> Roles { get; set; }
    public virtual DbSet<DbProject> Projects { get; set; }
    public virtual DbSet<DbProjectEntry> ProjectEntries { get; set; }
    public virtual DbSet<DbImageInfo> Images { get; set; }
    public virtual DbSet<DbDevice> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbUser>(usr =>
        {
            usr.HasKey(u => u.Id);
            usr.HasIndex(u => u.Username).IsUnique();
            usr.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
            usr.HasMany(u => u.Projects).WithOne(p => p.Creator).HasForeignKey(p => p.CreatorId);
        });

        modelBuilder.Entity<DbRole>(role =>
        {
            role.HasKey(r => r.Id);
            role.HasIndex(r => r.Name).IsUnique();
            role.HasMany(r => r.Users).WithOne(u => u.Role);
        });

        modelBuilder.Entity<DbProject>(proj =>
        {
            proj.HasKey(p => p.Id);
            proj.HasIndex(p => p.Name).IsUnique();
            proj.HasMany(p => p.Entries).WithOne(e => e.Project).HasForeignKey(p => p.ProjectId);
        });

        modelBuilder.Entity<DbProjectEntry>(ent =>
        {
            ent.HasKey(e => e.Id);
            ent.HasOne(e => e.Project).WithMany(p => p.Entries).HasForeignKey(e => e.ProjectId);
            ent.HasMany(e => e.Images).WithOne(i => i.Project).HasForeignKey(i => i.ProjectId);
        });

        modelBuilder.Entity<DbDevice>(dev =>
        {
            dev.HasKey(d => d.Id);
            dev.HasIndex(d => d.SourceHash); //mb source to another table...
            dev.HasOne(d => d.Creator).WithMany(u => u.CreatedDevices).HasForeignKey(d => d.CreatorId);
        });

        modelBuilder.Entity<DbImageInfo>(img =>
        {
            img.HasKey(i => i.Id);
            img.HasOne(i => i.Project).WithMany(p => p.Images).HasForeignKey(i => i.ProjectId);
            img.HasOne(i => i.Creator).WithMany(d => d.CreatedImages).HasForeignKey(i => i.CreatorId);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql();
        base.OnConfiguring(optionsBuilder);
    }
}

[Table("users", Schema = DbSchemas.UsersSchema)]
public class DbUser
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("username")]
    [MaxLength(FieldsLimits.MaxUsernameLength)]
    public required string Username { get; set; }

    [Column("password")]
    [MaxLength(FieldsLimits.MaxPasswordLength + 100)]
    [MinLength(FieldsLimits.MinPasswordLength)]
    public required string Password { get; set; }

    [Column("salt")] public required string Salt { get; set; }
    [ForeignKey(nameof(RoleId))] public DbRole? Role { get; set; }
    [Column("role")] public int RoleId { get; set; }
    [Column("creation_utc")] public DateTimeOffset CreationUtc { get; set; }
    [Column("last_update_utc")] public DateTimeOffset LastUpdateUtc { get; set; }
    [Column("is_active")] public bool IsActive { get; set; } = true;

    public ICollection<DbDevice>? CreatedDevices { get; set; }

    public ICollection<DbProject>? Projects { get; set; }
}

[Table("roles", Schema = DbSchemas.UsersSchema)]
public class DbRole
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("name")]
    [MaxLength(FieldsLimits.MaxRoleName)]
    public required string Name { get; set; }

    [Column("roles")] public required string[] Roles { get; set; }
    public ICollection<DbUser>? Users { get; set; }
}

[Table("projects", Schema = DbSchemas.ProjectSchemas)]
public class DbProject
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("name")] public required string Name { get; set; }

    [ForeignKey(nameof(CreatorId))] public required DbUser Creator { get; set; }

    [Column("creator_user_id")] public int CreatorId { get; set; }

    [Column("available_sources")] public required string[] AvailableSources { get; set; }

    [Column("creation_utc")] public DateTimeOffset CreationUTC { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<DbProjectEntry>? Entries { get; set; }
}

[Table("project_entries", Schema = DbSchemas.ProjectSchemas)]
public class DbProjectEntry
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(ProjectId))] public required DbProject Project { get; set; }

    [Column("project")] public int ProjectId { get; set; }

    [Column("version")] public required string Version { get; set; }

    [Column("last_update_utc")] public DateTimeOffset LastUpdateUTC { get; set; }

    [Column("is_actual")] public bool IsActual { get; set; }

    public ICollection<DbImageInfo> Images { get; set; } = [];
}

[Table("images", Schema = DbSchemas.ProjectSchemas)]
public class DbImageInfo
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("service_name")] public required string ServiceName { get; set; }

    [Column("version")] public required string Version { get; set; }

    [Column("image_tag")] public required string ImageTag { get; set; }

    [Column("docker_compose_file")] public required string DockerCompose { get; set; }

    [ForeignKey(nameof(ProjectId))] public required DbProjectEntry Project { get; set; }

    [Column("project_entry")] public int ProjectId { get; set; }

    [ForeignKey(nameof(CreatorId))] public required DbDevice Creator { get; set; }

    [Column("creator_device_id")] public Guid CreatorId { get; set; }

    [Column("is_active")] public bool IsActive { get; set; }

    [Column("creation_utc")] public DateTimeOffset CreationUTC { get; set; }
}

[Table("device", Schema = DbSchemas.DeviceSchema)]
public class DbDevice
{
    [Key] [Column("id")] public Guid Id { get; set; }

    [Column("key")] public required string KeyHash { get; set; }

    [Column("source")] [MaxLength(128)] public required string SourceHash { get; set; }

    [Column("salt")] public required string Salt { get; set; }

    [Column("key_expires_utc")] public DateTimeOffset ExpireUTC { get; set; }

    [Column("creation_utc")] public DateTimeOffset CreationUTC { get; set; }

    [Column("is_active")] public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CreatorId))] public required DbUser Creator { get; set; }

    [Column("creator_id")] public int CreatorId { get; set; }

    public ICollection<DbImageInfo>? CreatedImages { get; set; }
}

public static class DbSchemas
{
    public const string UsersSchema = "users";
    public const string ProjectSchemas = "projects";
    public const string DeviceSchema = "devices";
}