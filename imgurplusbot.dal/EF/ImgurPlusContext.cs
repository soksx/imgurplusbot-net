using System;
using imgurplusbot.dal.Models;
using Microsoft.EntityFrameworkCore;

namespace imgurplusbot.dal.EF
{
    public class ImgurPlusContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserUpload> UserUploads { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=imgurplus.db");
        public ImgurPlusContext() : base()
        {
            this.Database.Migrate();
        }
    }
}
