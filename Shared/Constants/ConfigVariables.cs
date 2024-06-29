using Microsoft.Extensions.Configuration;

namespace Shared.Constants
{
    public static class ConfigVariables
    {
        public static string fileUrl = "";
        public static string redisConnectionString = "";
        public static string filePath = "";
        public static bool isRedisEnabled = false;
        static ConfigVariables()
        {
            IConfiguration configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables()
           .Build();

            redisConnectionString = configuration.GetConnectionString("Redis");

            fileUrl = configuration.GetValue<string>("FileUrl");

            filePath = configuration.GetValue<string>("FilePath");

            isRedisEnabled = configuration.GetValue<bool>("IsRedisEnabled");
        }
    }
}
