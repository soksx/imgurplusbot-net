using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.dal.Models
{
    public class UserUpload
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string TgFileId { get; set; }
        public DateTimeOffset UploadDate { get; set; }
        public string ImgurLink { get; set; }
        public string ImgurDeleteHash { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
    }
}
