using System;
using System.Linq;
using System.Linq.Expressions;
using imgurplusbot.dal.EF;
using imgurplusbot.dal.Models;

namespace imgurplusbot.dal.Helpers
{
    public static class DbUtils
    {
        public static User AddUser(User user)
        {
            User retUser = null;
            try
            {
                using (ImgurPlusContext dbContext = new ImgurPlusContext())
                {
                    retUser = dbContext.Users.Add(user).Entity;
                    dbContext.SaveChanges();
                }
            }
            catch(Exception ex) { return null; }
            return retUser;
        }
        public static User GetUser(Expression<Func<User, bool>> searchFunc)
        {
            User retUser = null;
            try
            {
                using (ImgurPlusContext dbContext = new ImgurPlusContext())
                    retUser = dbContext.Users.FirstOrDefault(searchFunc.Compile());
                
            }
            catch { return null; }
            return retUser;
        }
        public static UserUpload AddUserUpload(UserUpload userUpload)
        {
            UserUpload retUserUpload = null;
            try
            {
                using (ImgurPlusContext dbContext = new ImgurPlusContext())
                {
                    retUserUpload = dbContext.UserUploads.Add(userUpload).Entity;
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex) { return null; }
            return retUserUpload;
        }
        public static UserUpload GetUserUpload(Expression<Func<UserUpload, bool>> searchFunc)
        {
            UserUpload retUserUpload = null;
            try
            {
                using (ImgurPlusContext dbContext = new ImgurPlusContext())
                    retUserUpload = dbContext.UserUploads.FirstOrDefault(searchFunc.Compile());
            }
            catch { return null; }
            return retUserUpload;
        }
        public static int DeteleUserUpload(Expression<Func<UserUpload, bool>> searchFunc)
        {
            int retUserUpload = 0;
            try
            {
                using (ImgurPlusContext dbContext = new ImgurPlusContext())
                {
                    UserUpload userUpd = GetUserUpload(searchFunc);
                    userUpd.DeleteDate = DateTimeOffset.Now;
                    dbContext.UserUploads.Update(userUpd);
                    retUserUpload = dbContext.SaveChanges();
                }
            }
            catch { return 0; }
            return retUserUpload;
        }
    }
}
