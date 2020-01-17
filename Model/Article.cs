using System;
using System.Collections.Generic;
using System.Text;

namespace ZendeskBackup.Model
{
    public class Article
    {
        public UInt64 Id { get; set; }
        public String Url { get; set; }
        public String HtmlUrl { get; set; }
        public String Title { get; set; }
        public String Body { get; set; }
        public String Locale { get; set; }
        public String SourceLocale { get; set; }
        public UInt64 AuthorId { get; set; }
        public Boolean CommentsDisabled { get; set; }
        public List<String> OutdatedLocales { get; set; }
        public Boolean Outdated { get; set; }
        public List<String> LabelNames { get; set; }
        public Boolean Draft { get; set; }
        public Boolean Promoted { get; set; }
        public int Position { get; set; }
        public int VoteSum { get; set; }
        public int VoteCount { get; set; }
        public UInt64? SectionId { get; set; }
        public UInt64? UserSegmentId { get; set; }
        public UInt64 PermissionGroupId { get; set; }
        public String CreatedAt { get; set; }
        public String EditedAt { get; set; }
        public String UpdatedAt { get; set; }

    }
}
