using System;
using System.Collections.Generic;
using System.Text;

namespace ZendeskBackup.Model
{
    public class Category
    {
        public UInt64 Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String Locale { get; set; }
        public String SourceLocale { get; set; }
        public String Url { get; set; }
        public String HtmlUrl { get; set; }
        public Boolean Outdated { get; set; }
        public UInt64 Position { get; set; }
        public String CreatedAt { get; set; }
        public String UpdatedAt { get; set; }
    }
}
