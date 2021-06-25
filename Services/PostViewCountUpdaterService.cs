using Dev_Blog.Data;
using Dev_Blog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dev_Blog.Services
{
    public class PostViewCountUpdaterService
    {
        private readonly ILogger<PostViewCountUpdaterService> logger;
        private readonly IServiceScopeFactory scopeFactory;

        public PostViewCountUpdaterService(ILogger<PostViewCountUpdaterService> logger, IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
        }

        public async Task IncrementViewCount(long postId)
        {
            await Task.Yield();
            using var scope = scopeFactory.CreateScope();
            using BlogDBContext context = scope.ServiceProvider.GetRequiredService<BlogDBContext>();
            try
            {
                Post p = await context.Posts.Where(p => p.ID == postId).FirstAsync();
                p.Views++;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Couldn't update the view count.");
            }
        }
    }
}
