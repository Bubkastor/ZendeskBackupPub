using System;
using System.Collections.Generic;
using System.Text;

namespace ZendeskBackup.Model
{
    public class ListCategories : BaseList
    {
        public List<Category> Categories { get; set; }
    }
}
