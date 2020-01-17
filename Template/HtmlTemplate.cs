using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZendeskBackup.Model;

namespace ZendeskBackup.Template
{
    public class HtmlTemplate
    {
        private const string TEMPLATE = @"
<title>%TITLE%</title>
<style>article{max-width: 1200px; margin: 0 auto; padding: 20 50px;}</style>
<article>
<h1>%H1%</h1>
%CONTENT%
</article>
";
        private const string TITLE_REPLACE = "%TITLE%";
        private const string H1_REPLACE = "%H1%";
        private const string CONTENT_REPLACE = "%CONTENT%";

        public static void CreateArticle(Article article, Dictionary<UInt64, string> pathById)
        {
            var path = pathById[article.Id];

            StringBuilder file = new StringBuilder(TEMPLATE);
            StringBuilder content = new StringBuilder();


            

            content.AppendLine("<a href='../index.html'>назад к статьям</a>");
            content.AppendLine("</br>");
            content.AppendLine($"<a href='{article.HtmlUrl}'>оригинальная статья</a>");

            content.AppendLine(article.Body);
            file.Replace(TITLE_REPLACE, article.Title);
            file.Replace(H1_REPLACE, article.Title);
            file.Replace(CONTENT_REPLACE, content.ToString());

            File.WriteAllText(path + "/index.html", file.ToString());
        }
        public static void CreateSectionArticlesIndex(List<Section> sections, List<Article> articles, Dictionary<UInt64, string> pathById)
        {

            foreach (var section in sections)
            {
                var findArticles = from item in articles
                               where item.SectionId == section.Id
                               select item;

                
                StringBuilder content = new StringBuilder();
                content.AppendLine("<a href='../index.html'>назад к рубрикам</a>");
                content.AppendLine($"<ul>");
                foreach (var article in findArticles)
                {
                    content.AppendLine($"<li>");
                    content.AppendLine($"<a href='{pathById[article.Id]}/index.html'>{article.Title}</a>");
                    content.AppendLine($"</li>");
                }
                content.AppendLine($"</ul>");
                StringBuilder file = new StringBuilder(TEMPLATE);
                file.Replace(TITLE_REPLACE, section.Name);
                file.Replace(H1_REPLACE, $"Статьи рубрики {section.Name}");
                file.Replace(CONTENT_REPLACE, content.ToString());
                
                File.WriteAllText(pathById[section.Id] + "/index.html", file.ToString());
            }

        }
        public static void CreateSection(List<Category> categories, List<Section> sections, Dictionary<UInt64, string> pathById)
        {
            foreach (var category in categories)
            {
                var finSections = from item in sections
                               where item.CategoryId == category.Id
                               select item;
                StringBuilder content = new StringBuilder();
                content.AppendLine("<a href='../index.html'>назад к категориям</a>");
                
                content.AppendLine($"<ul>");
                foreach (var section in finSections)
                {
                    content.AppendLine($"<li>");
                    content.AppendLine($"<a href='{pathById[section.Id]}/index.html'>{section.Name}</a>");
                    content.AppendLine($"</li>");
                }

                content.AppendLine($"</ul>");
                StringBuilder file = new StringBuilder(TEMPLATE);
                file.Replace(TITLE_REPLACE, category.Name);
                file.Replace(H1_REPLACE, $"Категории {category.Name}");
                file.Replace(CONTENT_REPLACE, content.ToString());

                File.WriteAllText(pathById[category.Id] + "/index.html", file.ToString());
            }

        }
        public static void CreateIndex(String path, List<Category> categories, Dictionary<UInt64, string> pathById)
        {
            StringBuilder content = new StringBuilder();
            content.AppendLine($"<ul>");
            foreach (var category in categories)
            {
                content.AppendLine($"<li>");
                content.AppendLine($"<a href='{pathById[category.Id]}/index.html'>{category.Name}</a>");
                content.AppendLine($"</li>");
            }
            content.AppendLine($"</ul>");

            StringBuilder file = new StringBuilder(TEMPLATE);
            file.Replace(TITLE_REPLACE, "База знайний Travelline");
            file.Replace(H1_REPLACE, "Категории");
            file.Replace(CONTENT_REPLACE, content.ToString());

            File.WriteAllText(path + "/index.html", file.ToString());
        }
        
    }
}
