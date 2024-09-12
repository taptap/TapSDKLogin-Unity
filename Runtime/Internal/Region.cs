namespace TapSDK.Login.Internal
{
    public abstract class Region
    {
        protected static bool isRND = false;
        
        public abstract string WebHost();

        public abstract string ApiHost();

        public abstract string AccountHost();

        public string CodeUrl()
        {
            return WebHost() + "/oauth2/v1/device/code";
        }

        public string TokenUrl()
        {
            return WebHost() + "/oauth2/v1/token";
        }

        public string ProfileUrl(bool havePublicProfile = true)
        {
            if (havePublicProfile)
            {
                return ApiHost() + "/account/profile/v1?client_id=";
            }
            else
            {
                return ApiHost() + "/account/basic-info/v1?client_id=";
            }
        }

        public string AccountUrl()
        {
            return AccountHost() + "/authorize?";
        }
    }

    public class RegionCN : Region {
        
        private static string webHost = "https://accounts.tapapis.cn";
        
        private static string apiHost = "https://open.tapapis.cn";
        
        private static string accountHost = "https://accounts.taptap.cn";

        private static string webHostRND = "https://oauth.api.xdrnd.cn";

        private static string apiHostRND = "https://open.api.xdrnd.cn";
        
        private static string accountHostRND = "https://accounts-beta.xdrnd.cn";
        
        public override string WebHost()
        {
            return isRND ? webHostRND : webHost;
        }

        public override string ApiHost()
        {
            return isRND ? apiHostRND : apiHost;
        }

        public override string AccountHost()
        {
            return isRND ? accountHostRND : accountHost;
        }
        
    }

    public class RegionIO : Region
    {
        private static string webHost = "https://accounts.tapapis.com";
        private static string apiHost = "https://open.tapapis.com";
        private static string accountHost = "https://accounts.taptap.io";
        
        private static string webHostRND = "https://oauth.api.xdrnd.com";
        private static string apiHostRND = "https://open.api.xdrnd.com";
        private static string accountHostRND = "https://accounts-io-beta.xdrnd.com";
        
        public override string WebHost()
        {
            return isRND ? webHostRND : webHost;
        }

        public override string ApiHost()
        {
            return isRND ? apiHostRND : apiHost;
        }

        public override string AccountHost()
        {
            return isRND ? accountHostRND : accountHost;
        }
        
    }
}