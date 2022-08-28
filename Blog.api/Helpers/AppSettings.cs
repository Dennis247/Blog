namespace Blog.api.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public int TokenExpirationInMinutes { get; set; }

        public string BlogImportUrl { get; set; }   

    }
}