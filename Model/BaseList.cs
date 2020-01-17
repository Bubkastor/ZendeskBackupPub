using System;
using System.Collections.Generic;
using System.Text;

namespace ZendeskBackup.Model
{
    abstract public class BaseList
    {
        public int Count { get; set; }
        public String NextPage { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public int PerPage { get; set; }
        public string PreviousPage { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }
}
