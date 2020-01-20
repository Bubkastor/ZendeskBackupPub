using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using ZendeskBackup.Model;
using ZendeskBackup.RestApi;
using ZendeskBackup.Template;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace ZendeskBackup
{

    public class ZendeskBackup
    {
        private readonly ZendeskConfig Config;
        private readonly ZendeskRestApi RestApi;

        private const double MAX_ALLOW_DAY_BACKUP = 30;
        private const int TIMEOUT_DOWNLOAD_MINUTES = 1;

        private Regex IllegalInFileName = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);

        private List<Category> Categories { get; set; } = new List<Category>();
        private List<Section> Sections { get; set; } = new List<Section>();
        private List<Article> Articles { get; set; } = new List<Article>();
        private List<Attachment> Attachments { get; set; } = new List<Attachment>();
        private List<ImgFile> ImgFiles { get; set; } = new List<ImgFile>();
        private Dictionary<UInt64, string> GetPathById { get; set; } = new Dictionary<UInt64, string>();
        private string BackupPath { get; set; }
        private string BackupPathZip { get; set; }
        private string CurrentDate { get; set; }
        private string CurrentBackupPath { get; set; }



        public ZendeskBackup(ZendeskConfig config, string backupFolder)
        {
            this.Config = config;
            this.RestApi = new ZendeskRestApi(this.Config);
            BackupPathZip = backupFolder;
            CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
            BackupPath = $"{BackupPathZip}{CurrentDate}\\";
        }

        public void Run()
        {
            GetData();
            CreateStructure();
            FillStructure();
            CreateBackup();
            DeleteOldBackup();
            PrintDebug();

        }
        private void PrintDebug()
        {
            Console.WriteLine($"Articles count: {Articles.Count}");
            Console.WriteLine($"Categories count: {Categories.Count}");
            Console.WriteLine($"Sections count: {Sections.Count}");
            Console.WriteLine($"Attachments count: {Attachments.Count}");
            Console.WriteLine($"Backup create. path: {CurrentBackupPath}");
            
        }

        private void DeleteOldBackup()
        {
            var backups = Directory.GetFiles(BackupPathZip, "*.zip");
            foreach(var path in backups)
            {
                var time = File.GetCreationTime(path);
                var daysDifference = DateTime.Now.Subtract(time);
                if(daysDifference.TotalDays > MAX_ALLOW_DAY_BACKUP)
                {
                    File.Delete(path);
                }
            }
        }


        private void FillStructure()
        {
            CreateMainIndex();
            CreateSectionIndex();
            CreateSectionArticlesIndex();
            foreach (var article in Articles)
            {
                CreateArticleHtml(article);
            }
            var urlsNotLoaded = RestApi.DownloadFiles(ImgFiles).Result;
            var notLoadedImg = (from item in urlsNotLoaded
                                where item.IsDownload == false
                                select item).ToList();
            while(notLoadedImg.Count() > 0)
            {
                Thread.Sleep(TimeSpan.FromMinutes(TIMEOUT_DOWNLOAD_MINUTES));
                urlsNotLoaded = RestApi.DownloadFiles(notLoadedImg).Result;
                notLoadedImg = (from item in urlsNotLoaded
                                where item.IsDownload == false
                                select item).ToList();
            }
        }

        private void CreateMainIndex()
        {
            HtmlTemplate.CreateIndex(BackupPath, Categories,  GetPathById);
        }

        private void CreateSectionIndex()
        {
            HtmlTemplate.CreateSection(Categories, Sections, GetPathById);
        }

        private void CreateSectionArticlesIndex()
        {
            HtmlTemplate.CreateSectionArticlesIndex(Sections, Articles, GetPathById);
        }
        private void CreateArticleHtml(Article article)
        {
            var path = GetPathById[article.Id];

            List<Attachment> attachment = (from it in Attachments
                                           where it.ArticleId == article.Id
                                           select it).ToList();
            foreach (var item in attachment)
            {
                if (item.ContentType.StartsWith("image"))
                {
                    Directory.CreateDirectory(path + "\\image");

                    var imagePath = $".\\image\\{item.Id}-{item.FileName}";

                    var savePath = $"{path}\\image\\{item.Id}-{item.FileName}";
                    var replaceString = article.Body.Replace(item.ContentUrl, imagePath);
                    
                    if (!replaceString.Equals(article.Body))
                    {
                        article.Body = article.Body.Replace(item.ContentUrl, imagePath);
                        ImgFiles.Add(new ImgFile()
                        {
                            Id = item.Id,
                            SourceUrl = item.ContentUrl,
                            DestPath = savePath,
                            IsDownload = false
                        });
                    }

                }
            }
            HtmlTemplate.CreateArticle(article, GetPathById);

        }

        private void CreateStructure()
        {
            if (Directory.Exists(BackupPath))
            {
                Directory.Delete(BackupPath, true);
            }
            Directory.CreateDirectory(BackupPath);

            foreach (var category in Categories)
            {
                var path = $"{BackupPath}{IllegalInFileName.Replace(category.Name, "")}";
                path = path.Trim();
                Directory.CreateDirectory(path);
                GetPathById.Add(category.Id, path);
            }

            var innerSections = new List<Section>();
            foreach (var section in Sections)
            {
                if (section.ParentSectionId == null)
                {
                    var path = $"{GetPathById[section.CategoryId]}\\{IllegalInFileName.Replace(section.Name, " ")}";
                    path = path.Trim();
                    Directory.CreateDirectory(path);
                    GetPathById.Add(section.Id, path);
                }
                else
                {
                    innerSections.Add(section);
                }
            }

            foreach (var article in Articles)
            {
                var path = BackupPath + IllegalInFileName.Replace(article.Title, " ");

                if (article.SectionId != null)
                {
                    var nameFolder = IllegalInFileName.Replace(article.Title, " ");

                    path = $"{ GetPathById[(UInt64)article.SectionId] }\\{nameFolder}";
                    path = path.Replace(".", " ");
                    path = path.Trim();
                }
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch(Exception ex)
                { 
                    Console.WriteLine(ex.Message);
                }
                
                GetPathById.Add(article.Id, path);
            }
        }

        #region getInformation
        private void GetData()
        {
            GetCategories();
            GetSections();
            GetArticles();
            GetAttachments();
        }

        private void GetAttachments()
        {
            List<ListAttachments> listAttachments = new List<ListAttachments>();
            var listIds = from item in Articles
                          select item.Id;
            listAttachments.AddRange(RestApi.MultiplyRequestAttachment<ListAttachments>(ApiRequest.Attachments, listIds).Result);

            foreach (var item in listAttachments)
            {
                Attachments.AddRange(item.ArticleAttachments);
            }

        }

        private void GetArticles()
        {
            var listArticles = RestApi.Request<ListArticles>(ApiRequest.Articles).Result;
            this.Articles.AddRange(listArticles.Articles);
            List<String> requestList = new List<string>();
            for (var i = 2; i <= listArticles.PageCount; i++)
            {
                var url = $"?page={i}&per_page=30";
                requestList.Add(url);
            }

            var list = RestApi.MultiplyRequest<ListArticles>(ApiRequest.Articles, requestList).Result;

            foreach (var item in list)
            {
                Articles.AddRange(item.Articles);
            }
        }
        private void GetCategories()
        {
            var listCategories = RestApi.Request<ListCategories>(ApiRequest.Categories).Result;
            this.Categories.AddRange(listCategories.Categories);
            List<String> requestList = new List<string>();
            for (var i = 2; i <= listCategories.PageCount; i++)
            {
                var url = $"?page={i}&per_page=30";
                requestList.Add(url);
            }

            var list = RestApi.MultiplyRequest<ListCategories>(ApiRequest.Categories, requestList).Result;

            foreach (var item in list)
            {
                Categories.AddRange(item.Categories);
            }
        }
        private void GetSections()
        {
            var listSections = RestApi.Request<ListSections>(ApiRequest.Sections).Result;
            this.Sections.AddRange(listSections.Sections);
            List<String> requestList = new List<string>();
            for (var i = 2; i <= listSections.PageCount; i++)
            {
                var url = $"?page={i}&per_page=30";
                requestList.Add(url);
            }

            var list = RestApi.MultiplyRequest<ListSections>(ApiRequest.Sections, requestList).Result;

            foreach (var item in list)
            {
                Sections.AddRange(item.Sections);
            }
        }

        #endregion

        public void CreateBackup()
        {
            CurrentBackupPath = BackupPathZip + $"{CurrentDate}.zip";
            try
            {
                if (File.Exists(CurrentBackupPath))
                {
                    File.Delete(CurrentBackupPath);
                }
                ZipFile.CreateFromDirectory(BackupPath, CurrentBackupPath);
                Directory.Delete(BackupPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
