using System;
using System.Collections.Generic;
using System.Text;

namespace ZendeskBackup.Model
{
    public class Attachment
    {
        public UInt64 Id { get; set; }
        public String Url { get; set; }
        public UInt64 ArticleId { get; set; }
        public String FileName { get; set; }
        public String ContentUrl { get; set; }
        public String ContentType { get; set; }
        public UInt64 Size { get; set; }
        public Boolean Inline { get; set; }
        public String CreatedAt { get; set; }
        public String UpdatedAt { get; set; }
    }
}
