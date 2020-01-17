using System;
using System.Collections.Generic;
using System.Text;

namespace ZendeskBackup.Model
{
    public class ListArticles : BaseList
    {
        public List<Article> Articles { get; set; }
    }
}
