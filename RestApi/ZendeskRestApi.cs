using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ZendeskBackup.Helper;

namespace ZendeskBackup.RestApi
{
    public struct ImgFile
    {
        public UInt64 Id { get; set; }
        public string SourceUrl { get; set; }
        public string DestPath { get; set; }
        public bool IsDownload { get; set; }
    }
    public class ZendeskConfig
    {
        public String Login { get; set; }
        public String Password { get; set; }
        public String Url { get; set; }

    }

    public enum ApiRequest
    {
        Categories,
        Sections,
        Articles,
        Attachments
    }

    public class ZendeskRestApi
    {
        private HttpClient HttpClient;
        readonly private ZendeskConfig Config;
        readonly private DefaultContractResolver ContractResolver;
        readonly private JsonSerializerSettings JsonSerializerSettings;
        readonly private int COUNT_THREAD = 20;


        private Dictionary<ApiRequest, String> mapRequest = new Dictionary<ApiRequest, string>(){
            { ApiRequest.Categories , "/api/v2/help_center/categories.json" },
            { ApiRequest.Sections, "/api/v2/help_center/sections.json" },
            { ApiRequest.Articles, "/api/v2/help_center/articles.json" },
            { ApiRequest.Attachments, "/api/v2/help_center/articles/{article_id}/attachments/inline.json" }
        };

        private static ThrottlingHelper ThrottlingHelper;
        public ZendeskRestApi(ZendeskConfig zendeskConfig)
        {            
            this.Config = zendeskConfig;
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            JsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = ContractResolver
            };
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
            };
            ThrottlingHelper = new ThrottlingHelper(699, TimeSpan.FromMinutes(1));
            var byteArray = Encoding.ASCII.GetBytes(Config.Login + ':' + Config.Password);
            HttpClient = new HttpClient(handler);
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<ImgFile> RequestFile(ImgFile imgFile)
        {
            /*while (!ThrottlingHelper.RequestAllowed)
            {
                await Task.Delay(100);
            }

            ThrottlingHelper.StartRequest();*/
            var response = await HttpClient.GetAsync(imgFile.SourceUrl).ConfigureAwait(false);
            Byte[] result = await response.Content.ReadAsByteArrayAsync();

            //ThrottlingHelper.EndRequest();
            if(response.StatusCode == System.Net.HttpStatusCode.OK) 
            {
                try
                {
                    using var sw = File.Create(imgFile.DestPath);
                    await sw.WriteAsync(result);

                    imgFile.IsDownload = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine(response.StatusCode);
            }


            return imgFile;
        }

        public async Task<T> Request<T>(ApiRequest apiRequest, UInt64? idArticle = null, String query = null)
        {
            T result = default(T);
            
            var url = Config.Url + mapRequest[apiRequest];
            if (query != null)
            {
                url += query;
            }
            if (apiRequest == ApiRequest.Attachments)
            {
                url = url.Replace("{article_id}", idArticle.ToString());
            }
            while (!ThrottlingHelper.RequestAllowed)
            {
                await Task.Delay(100);
            }
            ThrottlingHelper.StartRequest();
            var response = await HttpClient.GetAsync(url).ConfigureAwait(false);
            var responseResult = await response.Content.ReadAsStringAsync();
            ThrottlingHelper.EndRequest();

            try
            {
                result = JsonConvert.DeserializeObject<T>(responseResult, JsonSerializerSettings);
            }
            catch(Exception ex)
            {
                Console.WriteLine(responseResult);
                Console.WriteLine(ex.Message);
            }
            
            return result;
        }
        public async Task<IEnumerable<T>> MultiplyRequest<T>(ApiRequest apiRequest,  IEnumerable<String> queries)
        {
            var tasks = queries.Select(query => Request<T>(apiRequest, null, query));
            var result = await Task.WhenAll(tasks);
            return result;
        }
        public async Task<IEnumerable<T>> MultiplyRequestAttachment<T>(ApiRequest apiRequest, IEnumerable<UInt64> idsArticle)
        {
            var result = new List<T>();
            var tasks = idsArticle
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / COUNT_THREAD)
                .Select(x => x.Select(v => Request<T>(apiRequest, v.Value, "")));
            foreach(var taskPart in tasks)
            {
                result.AddRange(await Task.WhenAll(taskPart));
            }
            
            return result;
        }

        public async Task<List<ImgFile>> DownloadFiles(List<ImgFile> urlsFile)
        {
            var result = new List<ImgFile>();
            var tasks = urlsFile
               .Select((x, i) => new { Index = i, Value = x })
               .GroupBy(x => x.Index / COUNT_THREAD)
               .Select(x => x.Select(v => RequestFile(v.Value)));
            foreach (var taskPart in tasks)
            {
                result.AddRange(await Task.WhenAll(taskPart));
            }
            return result;
        }
    }
}
