using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NewsFeedService.WebAPI.Data
{
    public class NewsFeedContext : DbContext
    {
        public NewsFeedContext(DbContextOptions<NewsFeedContext> options)
            : base(options)
        { }

        public DbSet<NewsFeedItem> NewsFeedItems { get; set; }
    }

    public class NewsFeedItem
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string AuthorName { get; set; }

        public string Body { get; set; }

        public DateTime DateCreated { get; set; }

        public bool AllowComments { get; set; }
    }
}
