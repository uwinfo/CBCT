using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Core.Ef;

public partial class CBCTContext : DbContext
{
    public CBCTContext(DbContextOptions<CBCTContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminMenu> AdminMenus { get; set; }

    public virtual DbSet<AdminPermission> AdminPermissions { get; set; }

    public virtual DbSet<AdminRole> AdminRoles { get; set; }

    public virtual DbSet<AdminRoleAdminPermission> AdminRoleAdminPermissions { get; set; }

    public virtual DbSet<AdminRoleAdminUser> AdminRoleAdminUsers { get; set; }

    public virtual DbSet<AdminUser> AdminUsers { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<SysConfig> SysConfigs { get; set; }

    public virtual DbSet<SysLog> SysLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminMenu>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("admin_menu_pkey");

            entity.ToTable("admin_menu", tb => tb.HasComment("後台目錄"));

            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Link).HasColumnName("link");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierUid).HasColumnName("modifier_uid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ParentUid)
                .HasComment("第一層為#，後面的接對應的id")
                .HasColumnName("parent_uid");
            entity.Property(e => e.PermissionUid).HasColumnName("permission_uid");
            entity.Property(e => e.Sort)
                .HasDefaultValue(500L)
                .HasColumnName("sort");
        });

        modelBuilder.Entity<AdminPermission>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("admin_permission_group_pkey");

            entity.ToTable("admin_permission", tb => tb.HasComment("權限"));

            entity.Property(e => e.Uid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("uid");
            entity.Property(e => e.Code)
                .HasComment("英文，權限")
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasComment("中文，權限的描述")
                .HasColumnName("description");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierUid).HasColumnName("modifier_uid");
            entity.Property(e => e.Sort)
                .HasDefaultValue(5000L)
                .HasColumnName("sort");
        });

        modelBuilder.Entity<AdminRole>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("admin_role_pkey");

            entity.ToTable("admin_role", tb => tb.HasComment("後台管理角色"));

            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierUid).HasColumnName("modifier_uid");
            entity.Property(e => e.Name)
                .HasComment("群組名稱")
                .HasColumnName("name");
            entity.Property(e => e.Sort).HasColumnName("sort");
        });

        modelBuilder.Entity<AdminRoleAdminPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_role_admin_permission_pkey");

            entity.ToTable("admin_role_admin_permission", tb => tb.HasComment("權限對應角色"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AdminRoleUid).HasColumnName("admin_role_uid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierUid).HasColumnName("modifier_uid");
            entity.Property(e => e.PermissionUid).HasColumnName("permission_uid");
        });

        modelBuilder.Entity<AdminRoleAdminUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_role_admin_user_pkey");

            entity.ToTable("admin_role_admin_user", tb => tb.HasComment("角色管理員對應"));

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AdminRoleUid).HasColumnName("admin_role_uid");
            entity.Property(e => e.AdminUserUid).HasColumnName("admin_user_uid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierUid).HasColumnName("modifier_uid");
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("admin_user_pkey");

            entity.ToTable("admin_user");

            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.BackMemo).HasColumnName("back_memo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.EnStatus)
                .HasComment("管理員狀態: 正常=100, 鎖定=-100")
                .HasColumnName("en_status");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierUid).HasColumnName("modifier_uid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.OtpConfirm).HasColumnName("otp_confirm");
            entity.Property(e => e.OtpSecret).HasColumnName("otp_secret");
            entity.Property(e => e.ResetToken).HasColumnName("reset_token");
            entity.Property(e => e.ResetTokenExpiration)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reset_token_expiration");
            entity.Property(e => e.Salt).HasColumnName("salt");
            entity.Property(e => e.SecretHash).HasColumnName("secret_hash");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("doctor_pkey");

            entity.ToTable("doctor");

            entity.Property(e => e.Uid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("uid");
            entity.Property(e => e.BackMemo).HasColumnName("back_memo");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.EnStatus)
                .HasComment("狀態: 正常=100, 鎖定=-100")
                .HasColumnName("en_status");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Mobile).HasColumnName("mobile");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierUid).HasColumnName("modifier_uid");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.OtpConfirm).HasColumnName("otp_confirm");
            entity.Property(e => e.OtpSecret).HasColumnName("otp_secret");
            entity.Property(e => e.ResetToken).HasColumnName("reset_token");
            entity.Property(e => e.ResetTokenExpiration)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reset_token_expiration");
            entity.Property(e => e.Salt).HasColumnName("salt");
            entity.Property(e => e.SecretHash).HasColumnName("secret_hash");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.VerifyCode)
                .HasMaxLength(10)
                .HasColumnName("verify_code");
        });

        modelBuilder.Entity<SysConfig>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("sys_config_pkey");

            entity.ToTable("sys_config", tb => tb.HasComment("系統參數"));

            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.Content)
                .HasComment("參數內容")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("LOCALTIMESTAMP")
                .HasComment("建立日")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorName)
                .HasComment("建立人名稱(admin_user.name)")
                .HasColumnName("creator_name");
            entity.Property(e => e.CreatorUid)
                .HasComment("建立人(admin_user.uid)")
                .HasColumnName("creator_uid");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description)
                .HasComment("參數描述")
                .HasColumnName("description");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("LOCALTIMESTAMP")
                .HasComment("修改日")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifierName)
                .HasComment("修改人名稱(admin_user.name)")
                .HasColumnName("modifier_name");
            entity.Property(e => e.ModifierUid)
                .HasComment("修改人(admin_user.uid)")
                .HasColumnName("modifier_uid");
            entity.Property(e => e.Name)
                .HasComment("參數名稱")
                .HasColumnName("name");
        });

        modelBuilder.Entity<SysLog>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("sys_log_pkey");

            entity.ToTable("sys_log", tb => tb.HasComment("更動歷程"));

            entity.Property(e => e.Uid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("uid");
            entity.Property(e => e.AfterAdditionalJson)
                .HasComment("額外資料更動後")
                .HasColumnName("after_additional_json");
            entity.Property(e => e.AfterJson)
                .HasComment("變更後內容")
                .HasColumnName("after_json");
            entity.Property(e => e.BeforeAdditionalJson)
                .HasComment("額外資料更動前")
                .HasColumnName("before_additional_json");
            entity.Property(e => e.BeforeJson)
                .HasComment("變更前內容")
                .HasColumnName("before_json");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("LOCALTIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatorName).HasColumnName("creator_name");
            entity.Property(e => e.CreatorUid).HasColumnName("creator_uid");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.RecordUid)
                .HasComment("被更動的資料Uid")
                .HasColumnName("record_uid");
            entity.Property(e => e.TableName)
                .HasComment("資料表名稱")
                .HasColumnName("table_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
