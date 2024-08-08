﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PrinceQ.DataAccess.Data.Context;

#nullable disable

namespace PrinceQ.DataAccess.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0-preview.2.24128.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "b8b558cb-8f42-40a8-97f8-2f86d69d5b43",
                            Name = "GenerateNumber",
                            NormalizedName = "GENERATENUMBER"
                        },
                        new
                        {
                            Id = "7143ddd4-7854-4161-b1a1-730fb6185965",
                            Name = "Filling",
                            NormalizedName = "FILLING"
                        },
                        new
                        {
                            Id = "9609ba67-ae84-46cd-916e-23f45a765b4a",
                            Name = "Releasing",
                            NormalizedName = "RELEASING"
                        },
                        new
                        {
                            Id = "f9df0f53-2eb0-4d9e-9e23-7100730d2ff6",
                            Name = "Announcement",
                            NormalizedName = "ANNOUNCEMENT"
                        },
                        new
                        {
                            Id = "9804c22d-cafe-4c03-9af8-210908ee7042",
                            Name = "Videos",
                            NormalizedName = "VIDEOS"
                        },
                        new
                        {
                            Id = "45f6d676-0e09-4ebe-8731-e45fde97b9ef",
                            Name = "Users",
                            NormalizedName = "USERS"
                        },
                        new
                        {
                            Id = "b9eb12b9-d840-472d-9620-790a9f0aa125",
                            Name = "Reports",
                            NormalizedName = "REPORTS"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);

                    b.HasData(
                        new
                        {
                            UserId = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            RoleId = "b8b558cb-8f42-40a8-97f8-2f86d69d5b43"
                        },
                        new
                        {
                            UserId = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            RoleId = "7143ddd4-7854-4161-b1a1-730fb6185965"
                        },
                        new
                        {
                            UserId = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            RoleId = "9609ba67-ae84-46cd-916e-23f45a765b4a"
                        },
                        new
                        {
                            UserId = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            RoleId = "f9df0f53-2eb0-4d9e-9e23-7100730d2ff6"
                        },
                        new
                        {
                            UserId = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            RoleId = "9804c22d-cafe-4c03-9af8-210908ee7042"
                        },
                        new
                        {
                            UserId = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            RoleId = "45f6d676-0e09-4ebe-8731-e45fde97b9ef"
                        },
                        new
                        {
                            UserId = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            RoleId = "b9eb12b9-d840-472d-9620-790a9f0aa125"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Announcement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("Created_At")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Announcement");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created_At")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsActive")
                        .HasColumnType("bit");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            CategoryId = 1,
                            CategoryName = "Category A",
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Trade",
                            IsActive = true
                        },
                        new
                        {
                            CategoryId = 2,
                            CategoryName = "Category B",
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Non-Trade",
                            IsActive = true
                        },
                        new
                        {
                            CategoryId = 3,
                            CategoryName = "Category C",
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Special",
                            IsActive = true
                        },
                        new
                        {
                            CategoryId = 4,
                            CategoryName = "Category D",
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Inquiry",
                            IsActive = true
                        });
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.ClerkIPAddress", b =>
                {
                    b.Property<string>("IPAddress")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ClerkNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("IPAddress");

                    b.HasIndex("UserId");

                    b.ToTable("Device");

                    b.HasData(
                        new
                        {
                            IPAddress = "10.64.14.50",
                            ClerkNumber = "Clerk 1"
                        });
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Queue_Status", b =>
                {
                    b.Property<int>("StatusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StatusId"));

                    b.Property<DateTime>("Created_At")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("StatusName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StatusId");

                    b.ToTable("Queue_Statuses");

                    b.HasData(
                        new
                        {
                            StatusId = 1,
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            StatusName = "waiting"
                        },
                        new
                        {
                            StatusId = 2,
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            StatusName = "serve"
                        },
                        new
                        {
                            StatusId = 3,
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            StatusName = "reserve"
                        },
                        new
                        {
                            StatusId = 4,
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            StatusName = "cancel"
                        },
                        new
                        {
                            StatusId = 5,
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            StatusName = "serving"
                        });
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Queues", b =>
                {
                    b.Property<string>("QueueId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int>("QueueNumber")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Cancelled_At")
                        .HasColumnType("datetime2");

                    b.Property<string>("ClerkId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("ForFilling_end")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ForFilling_start")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ForFilling_start_Backup")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Generate_At")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Releasing_end")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Releasing_start")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Releasing_start_Backup")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Reserve_At")
                        .HasColumnType("datetime2");

                    b.Property<int?>("StageId")
                        .HasColumnType("int");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<int?>("Total_Cheques")
                        .HasColumnType("int");

                    b.HasKey("QueueId", "CategoryId", "QueueNumber");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ClerkId");

                    b.HasIndex("StageId");

                    b.HasIndex("StatusId");

                    b.ToTable("QueueNumbers");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Serve_ForFilling", b =>
                {
                    b.Property<string>("GenerateDate")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ClerkId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int>("QueueNumber")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Serve_end")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Serve_start")
                        .HasColumnType("datetime2");

                    b.HasKey("GenerateDate", "ClerkId", "CategoryId", "QueueNumber");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ClerkId");

                    b.ToTable("Serve_ForFilling");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Serve_Releasing", b =>
                {
                    b.Property<string>("GenerateDate")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ClerkId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int>("QueueNumber")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Serve_end")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Serve_start")
                        .HasColumnType("datetime2");

                    b.HasKey("GenerateDate", "ClerkId", "CategoryId", "QueueNumber");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ClerkId");

                    b.ToTable("Serve_Releasing");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Serving", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int>("QueueNumberServe")
                        .HasColumnType("int");

                    b.Property<DateTime>("Served_At")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("UserId");

                    b.ToTable("Serving");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Stage_Queue", b =>
                {
                    b.Property<int>("StageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("StageId"));

                    b.Property<string>("StageName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StageId");

                    b.ToTable("Stage");

                    b.HasData(
                        new
                        {
                            StageId = 1,
                            StageName = "Filling"
                        },
                        new
                        {
                            StageId = 2,
                            StageName = "Releasing"
                        });
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created_At")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "f626b751-35a0-43df-8173-76cb5b4886fd",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "79681f10-0ee2-4127-9e9c-0eb73ded94fe",
                            Created_At = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "clarky@princeretail.com",
                            EmailConfirmed = false,
                            IsActive = true,
                            LockoutEnabled = true,
                            NormalizedEmail = "CLARKY@PRINCERETAIL.COM",
                            NormalizedUserName = "CLARKY",
                            PasswordHash = "AQAAAAIAAYagAAAAECnnqwWvQxblMxWGHvFRkui6EcfZu6BPqf2MtI8fZS9u6gCf8BWu3bZIc1xF16W6zA==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "GJ636UXBBLKO5JOGP6X3WISMIEBRVVHM",
                            TwoFactorEnabled = false,
                            UserName = "clarky"
                        });
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.User_Category", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "CategoryId");

                    b.HasIndex("CategoryId");

                    b.ToTable("User_Category");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PrinceQ.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.ClerkIPAddress", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Queues", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PrinceQ.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("ClerkId");

                    b.HasOne("PrinceQ.Models.Entities.Stage_Queue", "Stage")
                        .WithMany()
                        .HasForeignKey("StageId");

                    b.HasOne("PrinceQ.Models.Entities.Queue_Status", "QueueStatus")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("QueueStatus");

                    b.Navigation("Stage");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Serve_ForFilling", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PrinceQ.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("ClerkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Serve_Releasing", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PrinceQ.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("ClerkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.Serving", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PrinceQ.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Category");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PrinceQ.Models.Entities.User_Category", b =>
                {
                    b.HasOne("PrinceQ.Models.Entities.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PrinceQ.Models.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
