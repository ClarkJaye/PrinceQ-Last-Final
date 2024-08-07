using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrinceQ.Models.Entities;

namespace PrinceQ.DataAccess.Data.Context
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<ClerkIPAddress> Device { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Queue_Status> Queue_Statuses { get; set; }
        public virtual DbSet<Queues> QueueNumbers { get; set; }
        public virtual DbSet<Serve_ForFilling> Serve_ForFilling { get; set; }
        public virtual DbSet<Serve_Releasing> Serve_Releasing { get; set; }
        public virtual DbSet<User_Category> User_Category { get; set; }
        public virtual DbSet<Serving> Serving { get; set; }
        public virtual DbSet<Announcement> Announcement { get; set; }
        public virtual DbSet<User_Category> User_Categories{ get; set; }
        //public virtual DbSet<IsActive> IsActive{ get; set; }
        public virtual DbSet<Stage_Queue> Stage{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //CONSTANT VALUE
            const string SOFTWAREADMIN = "f626b751-35a0-43df-8173-76cb5b4886fd";
            const string GENID = "b8b558cb-8f42-40a8-97f8-2f86d69d5b43";
            const string FILLINGID = "7143ddd4-7854-4161-b1a1-730fb6185965";
            const string RELEASINGID = "9609ba67-ae84-46cd-916e-23f45a765b4a";
            const string ANNOUNCEMENTID = "f9df0f53-2eb0-4d9e-9e23-7100730d2ff6";
            const string VIDEOID = "9804c22d-cafe-4c03-9af8-210908ee7042";
            const string USERID = "45f6d676-0e09-4ebe-8731-e45fde97b9ef";
            const string REPORTID = "b9eb12b9-d840-472d-9620-790a9f0aa125";

            //Seeding
            modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        Id = SOFTWAREADMIN,
                        UserName = "clarky",
                        NormalizedUserName = "CLARKY",
                        Email = "clarky@princeretail.com",
                        NormalizedEmail = "CLARKY@PRINCERETAIL.COM",
                        EmailConfirmed = false,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                        PasswordHash = "AQAAAAIAAYagAAAAECnnqwWvQxblMxWGHvFRkui6EcfZu6BPqf2MtI8fZS9u6gCf8BWu3bZIc1xF16W6zA==",
                        SecurityStamp = "GJ636UXBBLKO5JOGP6X3WISMIEBRVVHM",
                        ConcurrencyStamp = "79681f10-0ee2-4127-9e9c-0eb73ded94fe",
                        //IsActiveId = 1
                        IsActive = true
                    }                             
                );

            modelBuilder.Entity<IdentityRole>().HasData(
                new  { Id = GENID, Name = "GenerateNumber", NormalizedName = "GENERATENUMBER" },
                new  { Id = FILLINGID, Name = "Filling", NormalizedName = "FILLING" },
                new  { Id = RELEASINGID, Name = "Releasing", NormalizedName = "RELEASING" },
                new  { Id = ANNOUNCEMENTID, Name = "Announcement", NormalizedName = "ANNOUNCEMENT" },
                new  { Id = VIDEOID, Name = "Videos", NormalizedName = "VIDEOS" },
                new  { Id = USERID, Name = "Users", NormalizedName = "USERS" },
                new  { Id = REPORTID, Name = "Reports", NormalizedName = "REPORTS" }
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                   new { UserId = SOFTWAREADMIN, RoleId = GENID },
                   new { UserId = SOFTWAREADMIN, RoleId = FILLINGID },
                   new { UserId = SOFTWAREADMIN, RoleId = RELEASINGID },
                   new { UserId = SOFTWAREADMIN, RoleId = ANNOUNCEMENTID },
                   new { UserId = SOFTWAREADMIN, RoleId = VIDEOID },
                   new { UserId = SOFTWAREADMIN, RoleId = USERID },
                   new { UserId = SOFTWAREADMIN, RoleId = REPORTID }

               );

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Category A", Description = "Trade", IsActive = true },
                new Category { CategoryId = 2, CategoryName = "Category B", Description = "Non-Trade", IsActive = true },
                new Category { CategoryId = 3, CategoryName = "Category C", Description = "Special", IsActive = true },
                new Category { CategoryId = 4, CategoryName = "Category D", Description = "Inquiry" , IsActive = true }
                );

            modelBuilder.Entity<Queue_Status>().HasData(
                 new Queue_Status { StatusId = 1, StatusName = "waiting" },
                 new Queue_Status { StatusId = 2, StatusName = "serve" },
                 new Queue_Status { StatusId = 3, StatusName = "reserve" },
                 new Queue_Status { StatusId = 4, StatusName = "cancel" },
                 new Queue_Status { StatusId = 5, StatusName = "serving" }
                 );

            //modelBuilder.Entity<IsActive>().HasData(
            //   new IsActive { IsActiveId = 1, Name = "Active" },
            //   new IsActive { IsActiveId = 2, Name = "Inactive" }
            //   );

            modelBuilder.Entity<Stage_Queue>().HasData(
               new Stage_Queue { StageId = 1, StageName = "Filling" },
               new Stage_Queue { StageId = 2, StageName = "Releasing" }
               );

            modelBuilder.Entity<ClerkIPAddress>().HasData(
               new ClerkIPAddress { IPAddress = "10.64.14.50", ClerkNumber = "Clerk 1" }
               );

            // For Junction 
            modelBuilder.Entity<Queues>()
                .HasKey(uc => new { uc.QueueId, uc.CategoryId, uc.QueueNumber});
                
            modelBuilder.Entity<User_Category>()
                .HasKey(uc => new { uc.UserId, uc.CategoryId });

            modelBuilder.Entity<User_Category>()
                .HasKey(uc => new { uc.UserId, uc.CategoryId });

            modelBuilder.Entity<Serve_ForFilling>()
                .HasKey(uc => new { uc.GenerateDate, uc.ClerkId, uc.CategoryId, uc.QueueNumber });

            modelBuilder.Entity<Serve_Releasing>()
                .HasKey(uc => new { uc.GenerateDate, uc.ClerkId, uc.CategoryId, uc.QueueNumber });


            // Default Date
            modelBuilder.Entity<User>()
              .Property(u => u.Created_At)
              .HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Category>()
                .Property(u => u.Created_At)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Queue_Status>()
                .Property(u => u.Created_At)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Serving>()
                .Property(u => u.Served_At)
                .HasDefaultValueSql("GETDATE()");
            
            modelBuilder.Entity<Announcement>()
                .Property(u => u.Created_At)
                .HasDefaultValueSql("GETDATE()");

        }

    }

}

