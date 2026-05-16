using Microsoft.EntityFrameworkCore;
using auth_dotnet_api.Models;
using auth_dotnet_api.Enum;
using System.Security.Cryptography;
using System.Text;

namespace auth_dotnet_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
                _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }

        // public override int SaveChanges()
        // {
        //     // SetAuditFields();
        //     return base.SaveChanges();
        // }

        // public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        // {
        //     // SetAuditFields();
        //     return await base.SaveChangesAsync(cancellationToken);
        // }
        private void SetAuditFields()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? "system";

            foreach (var entry in ChangeTracker.Entries<Base>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            string password = "1234";
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Username = "admin",
                    Password = hash,
                    Firstname = "Admin",
                    Lastname = "User",
                    Email = "admin@gmail.com",
                    Role = RoleType.Admin,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    Username = "user",
                    Password = hash,
                    Firstname = "User",
                    Lastname = "User",
                    Email = "user@gmail.com",
                    Role = RoleType.User,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}