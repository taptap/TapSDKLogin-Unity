using System.Threading.Tasks;
using TapSDK.Core;
using TapSDK.Login.Internal;
using TapSDK.Core.Standalone;
using System.Diagnostics;

namespace TapSDK.Login.Standalone
{
    public class TapTapLoginStandalone: ITapTapLoginPlatform
    {
        
        public void Init(string clientId, TapTapRegionType regionType)
        {
            TapLoginStandaloneImpl.Instance.Init(clientId, regionType);
        }

        public Task<TapTapAccount> Login(string[] scopes)
        {
            return TapLoginStandaloneImpl.Instance.Login(scopes);
        }

        public void Logout()
        {
            TapLoginStandaloneImpl.Instance.Logout();
        }

        public Task<TapTapAccount> GetCurrentAccount()
        {
            return TapLoginStandaloneImpl.Instance.GetCurrentAccount();
        }
    }

    public class TapTapLoginOpenIDProvider: IOpenIDProvider {
        public string GetOpenID() {
            return TapLoginStandaloneImpl.Instance.GetCurrentAccount().Result?.openId;
        }
    }
}