using System;
using System.Collections.Generic;
using System.Text;

namespace imgurplusbot.dal.Models
{
    public class User
    {
        public long Id { get; set; }
        public int TgId { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
    }
}
