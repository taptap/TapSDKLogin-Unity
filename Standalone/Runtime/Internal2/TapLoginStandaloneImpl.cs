using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TapSDK.Core;
using TapSDK.Core.Internal.Utils;
using TapSDK.Login.Internal.Http;
using TapSDK.Login.Standalone;
using TapSDK.Login.Standalone.Internal;
using UnityEngine;

namespace TapSDK.Login.Internal
{
    public class TapLoginStandaloneImpl
    {
        private static TapLoginStandaloneImpl instance;

        private TapLoginStandaloneImpl()
        {
            
        }

        public static TapLoginStandaloneImpl Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TapLoginStandaloneImpl();
                }
                return instance;
            }
        }
        
        public void Init(string clientId, TapTapRegionType regionType)
        {
            TapTapSdk.SDKInitialize(clientId, regionType == TapTapRegionType.CN);
            AccountManager.Instance.Init();
            CheckAndRefreshToken();
            TapLoginTracker.Instance.TrackInit();
        }

        public Task<TapTapAccount> Login(string[] scopes)
        {
            string sessionId = Guid.NewGuid().ToString();
            TapLoginTracker.Instance.TrackStart("loginWithScopes", sessionId);

            if (!scopes.Contains(TapTapLogin.TAP_LOGIN_SCOPE_PUBLIC_PROFILE))
            {
                scopes = scopes.Append(TapTapLogin.TAP_LOGIN_SCOPE_PUBLIC_PROFILE).ToArray();
            }
            IComplianceProvider provider = BridgeUtils.CreateBridgeImplementation(typeof(IComplianceProvider),
                "TapSDK.Compliance") as IComplianceProvider;
            String complianceScope = provider?.GetAgeRangeScope();
            if (complianceScope != null)
            {
                scopes = scopes.Append(complianceScope).ToArray();
            }
            TaskCompletionSource<TapTapAccount> tcs = new TaskCompletionSource<TapTapAccount>();
            LoginPanelController.OpenParams openParams = new LoginPanelController.OpenParams {
                ClientId = TapTapSdk.ClientId,
                Scopes = scopes,
                OnAuth = async (tokenData, loginType) => {
                    if (tokenData == null) {
                        TapLoginTracker.Instance.TrackFailure("loginWithScopes", sessionId, loginType, (int) TapErrorCode.ERROR_CODE_UNDEFINED, "UnKnow Error");
                        tcs.TrySetException(new TapException((int) TapErrorCode.ERROR_CODE_UNDEFINED, "UnKnow Error"));
                    } else {
                        // 将 TokenData 转化为 AccessToken
                        AccessToken refreshToken = new AccessToken {
                            kid = tokenData.Kid,
                            tokenType = tokenData.TokenType,
                            macKey = tokenData.MacKey,
                            macAlgorithm = tokenData.MacAlgorithm,
                            scopeSet = tokenData.Scopes
                        };
                        ProfileData profileData = await LoginService.GetProfile(TapTapSdk.ClientId, refreshToken);
                        if (profileData != null)
                        {
                            TapLoginTracker.Instance.TrackSuccess("loginWithScopes", sessionId, loginType);
                            AccountManager.Instance.Account = new TapTapAccount(
                                refreshToken, profileData.OpenId, profileData.UnionId, profileData.Name, profileData.Avatar,
                                profileData.Email);
                            tcs.TrySetResult(AccountManager.Instance.Account);
                        }
                        else
                        {
                            TapLoginTracker.Instance.TrackFailure("loginWithScopes", sessionId, loginType, (int) TapErrorCode.ERROR_CODE_UNDEFINED, "UnKnow Error");
                            tcs.TrySetException(new TapException((int) TapErrorCode.ERROR_CODE_UNDEFINED, "UnKnow Error"));
                        }
                    }
                },
                OnError = (e, loginType) => {
                    TapLoginTracker.Instance.TrackFailure("loginWithScopes", sessionId, loginType, e.Code, e.Message);
                    tcs.TrySetException(e);
                },
                OnClose = () => {
                    TapLoginTracker.Instance.TrackCancel("loginWithScopes", sessionId);
                    tcs.TrySetCanceled();
                }
            };
            TapSDK.UI.UIManager.Instance.OpenUI<LoginPanelController>("Prefabs/TapLogin/LoginPanel", openParams);
            return tcs.Task;
        }
        
        public Task<AccessToken> Authorize(string[] scopes = null)
        {
            TaskCompletionSource<AccessToken> tcs = new TaskCompletionSource<AccessToken>();
            LoginPanelController.OpenParams openParams = new LoginPanelController.OpenParams {
                ClientId = TapTapSdk.ClientId,
                Scopes = new HashSet<string>(scopes).ToArray(),
                OnAuth = (tokenData, loginType) => {
                    if (tokenData == null) {
                        tcs.TrySetException(new TapException((int) TapErrorCode.ERROR_CODE_UNDEFINED, "UnKnow Error"));
                    } else {
                        // 将 TokenData 转化为 AccessToken
                        AccessToken accessToken = new AccessToken {
                            kid = tokenData.Kid,
                            tokenType = tokenData.TokenType,
                            macKey = tokenData.MacKey,
                            macAlgorithm = tokenData.MacAlgorithm,
                            scopeSet = tokenData.Scopes
                        };
                        tcs.TrySetResult(accessToken);
                    }
                },
                OnError = (e, loginType) => {
                    tcs.TrySetException(e);
                },
                OnClose = () => {
                    tcs.TrySetException(
                        new TapException((int) TapErrorCode.ERROR_CODE_LOGIN_CANCEL, "Login Cancel"));
                }
            };
            TapSDK.UI.UIManager.Instance.OpenUI<LoginPanelController>("Prefabs/TapLogin/LoginPanel", openParams);
            return tcs.Task;
        }

        public void Logout()
        {
            AccountManager.Instance.ClearCache();
        }

        public Task<TapTapAccount> GetCurrentAccount()
        {
            var tcs = new TaskCompletionSource<TapTapAccount>();
            tcs.TrySetResult(AccountManager.Instance.Account);
            return tcs.Task;
        }
        
        private async Task CheckAndRefreshToken(){
            try
            {
                AccessToken accessToken = AccountManager.Instance.Account?.accessToken;
                if(accessToken != null){
                    TokenData tokenData = null;
                    try{
                        tokenData =  await LoginService.RefreshToken(TapTapSdk.ClientId, accessToken.kid);
                    }catch(TapException e){
                        //清除本地缓存
                        if(e.code < 0 ){
                            Logout();
                        }
                        return;
                    }

                    if (tokenData == null)
                    {
                        return;
                    }
                    AccessToken refreshToken = new AccessToken {
                        kid = tokenData.Kid,
                        tokenType = tokenData.TokenType,
                        macKey = tokenData.MacKey,
                        macAlgorithm = tokenData.MacAlgorithm,
                        scopeSet = tokenData.Scopes
                    };
                    ProfileData profileData = await LoginService.GetProfile(TapTapSdk.ClientId, refreshToken);
                    if (profileData != null)
                    {
                        AccountManager.Instance.Account = new TapTapAccount(
                            refreshToken, profileData.OpenId, profileData.UnionId, profileData.Name, profileData.Avatar,
                            profileData.Email);
                    }
                }
            }catch(Exception e){
                Debug.Log("refresh TapToken fail reason : " + e.Message + "\n stack = " + e.StackTrace);
            }
        }
    }
}