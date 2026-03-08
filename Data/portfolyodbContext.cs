using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Portfolyo.Data;
using Portfolyo.Models;

namespace PortfolyoDbContext
{
    public partial class portfolyodbContext : DbContext
    {
        public portfolyodbContext()
        {
        }

        public portfolyodbContext(DbContextOptions<portfolyodbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AboutMe2Table> AboutMe2Tables { get; set; } = null!;
        public virtual DbSet<AboutMeTable> AboutMeTables { get; set; } = null!;
        public virtual DbSet<CategoryTable> CategoryTables { get; set; } = null!;
        public virtual DbSet<HomePage> HomePages { get; set; } = null!;
        public virtual DbSet<ProjectsTable> ProjectsTables { get; set; } = null!;
        public virtual DbSet<ServicesTable> ServicesTables { get; set; } = null!;
        public virtual DbSet<SkillTable> SkillTables { get; set; } = null!;
        public virtual DbSet<TestimonialTable> TestimonialTables { get; set; } = null!;
        public virtual DbSet<MessageTable> MessageTables { get; set; }
        public virtual DbSet<EducationTable> EducationTables { get; set; }
        public DbSet<AboutInfoTable> AboutInfoTables { get; set; }
 






        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // DbContext options are configured in Program.cs.
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AboutMe2Table>(entity =>
            {
                entity.HasKey(e => e.DetailId);

                entity.ToTable("AboutMe2Table");

                entity.Property(e => e.DetailId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("DetailID");

                entity.Property(e => e.AboutId).HasColumnName("AboutID");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.DetailType).HasMaxLength(50);

                entity.Property(e => e.Title).HasMaxLength(200);


                entity.HasOne(d => d.Detail)
                    .WithOne(p => p.AboutMe2Table)
                    .HasForeignKey<AboutMe2Table>(d => d.DetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AboutMe2Table_AboutMeTable");
            });

            modelBuilder.Entity<AboutMeTable>(entity =>
            {
                entity.HasKey(e => e.AboutId);

                entity.ToTable("AboutMeTable");

                entity.Property(e => e.AboutId).HasColumnName("AboutID");

                entity.Property(e => e.ImagePath).HasMaxLength(300);

                entity.Property(e => e.JobTitle).HasMaxLength(100);

                entity.Property(e => e.NameSurname).HasMaxLength(100);

                entity.Property(e => e.ShortDescription).HasMaxLength(600);

            });

            modelBuilder.Entity<CategoryTable>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.ToTable("CategoryTable");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CategoryName).HasMaxLength(100);
            });

            modelBuilder.Entity<HomePage>(entity =>
            {
                entity.HasKey(e => e.HomeId);

                entity.ToTable("HomePage");

                entity.Property(e => e.HomeId).HasColumnName("homeID");

                entity.Property(e => e.ImagePath)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.NameSurname)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Title)
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<ProjectsTable>(entity =>
            {
                entity.HasKey(e => e.ProjectId);

                entity.ToTable("ProjectsTable");

                entity.Property(e => e.ProjectId).HasColumnName("ProjectID");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.Image).HasMaxLength(500);

                entity.Property(e => e.ProjectName).HasMaxLength(200);

                entity.Property(e => e.Title).HasMaxLength(200);
            });

            modelBuilder.Entity<ServicesTable>(entity =>
            {
                entity.HasKey(e => e.ExperinceId)
                    .HasName("PK_Experinces Table");

                entity.ToTable("Services Table");

                entity.Property(e => e.ExperinceId).HasColumnName("ExperinceID");

                entity.Property(e => e.Description).HasMaxLength(400);

                entity.Property(e => e.Icon).HasMaxLength(500);

                entity.Property(e => e.Title).HasMaxLength(100);
            });

            modelBuilder.Entity<SkillTable>(entity =>
            {
                entity.HasKey(e => e.SkilId);

                entity.ToTable("SkillTable");

                entity.Property(e => e.SkilId).HasColumnName("SkilID");

                entity.Property(e => e.Title).HasMaxLength(100);
            });

            modelBuilder.Entity<TestimonialTable>(entity =>
            {
                entity.HasKey(e => e.TestimonialId);

                entity.ToTable("TestimonialTable");

                entity.Property(e => e.TestimonialId)
                    .ValueGeneratedNever()
                    .HasColumnName("TestimonialID");

                entity.Property(e => e.Comment).HasMaxLength(400);

                entity.Property(e => e.CustomerName).HasMaxLength(100);

                entity.Property(e => e.ImagePath).HasMaxLength(200);

                entity.Property(e => e.JobTitle).HasMaxLength(200);
            });

            modelBuilder.Entity<EducationTable>(entity =>
            {
                entity.ToTable("Educations"); // 👈 SQL Server’daki GERÇEK tablo adı
                entity.HasKey(e => e.EducationId);
            });

            modelBuilder.Entity<AboutInfoTable>(entity =>
            {
                entity.ToTable("AboutInfoTable");
                entity.HasKey(e => e.AboutInfoId);
            });







            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
