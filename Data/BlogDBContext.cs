using Dev_Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Data
{
    public class BlogDBContext : IdentityDbContext<User>
    {
        public override DbSet<User> Users { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<BlogFile> PostFiles { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<PostTag> PostTags { get; set; }
        public virtual DbSet<Subscriber> Subscribers { get; set; }
        public virtual DbSet<SubscriberTag> SubscriberTags { get; set; }
        public virtual DbSet<Ban> Bans { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectURL = Environment.GetEnvironmentVariable("SQLSERVERURL");
            if (String.IsNullOrWhiteSpace(connectURL))
            {
                connectURL = "server=localhost;database=blog;username=root;password=12345";
            }
            optionsBuilder.UseMySql(connectURL);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.DisplayName)
                .IsUnique();

            modelBuilder.Entity<Post>()
                .HasIndex(p => p.Stub)
                .IsUnique();

            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostID, pt.TagID });

            modelBuilder.Entity<Subscriber>()
                .HasIndex(s => s.ValidationToken)
                .IsUnique();

            modelBuilder.Entity<SubscriberTag>()
                .HasKey(st => new { st.SubscriberID, st.TagID });
        }
    }
}
