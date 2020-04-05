using Dev_Blog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Data
{
    public class BlogDBContext : IdentityDbContext<AspUser>
    {
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<User> Authors { get; set; }

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
        }

    }
}
