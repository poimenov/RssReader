using Microsoft.EntityFrameworkCore;
using RssReader.MVVM.DataAccess.Models;

namespace RssReader.MVVM.DataAccess;

internal class Database : DbContext
{
    public const string DB_FILE_NAME = "RssReader.db";

    public Database()
    {
        this.Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DB_FILE_NAME}");
    }

    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Channel> Channels { get; set; }
    public virtual DbSet<ChannelItem> ChannelItems { get; set; }
    public virtual DbSet<ChannelsGroup> ChannelsGroups { get; set; }
    public virtual DbSet<ItemCategory> ItemCategories { get; set; }
}
