﻿using UnityEngine;
using UnityEngine.UI;
using TapSDK.UI;
using TapSDK.Login.Internal.Http;
using System;
using TapSDK.Core;

namespace TapSDK.Login.Internal {
    public class LoginPanelController : BasePanelController {
        public enum Type {
            Auth,
            Login
        }

        public class OpenParams : AbstractOpenPanelParameter {
            public Type Type { get; set; }
            public string ClientId { get; set; }
            public string[] Scopes { get; set; }
            public Action<TokenData, String> OnAuth { get; set; }
            public Action<TapException, String> OnError { get; set; }
            public Action OnClose { get; set; }
        }

        private TitleController titleController;
        private QRCodeController qrcodeController;
        private WebController webController;

        protected override void BindComponents() {
            base.BindComponents();

            Button closeBtn = transform.Find("TopBar/CloseButton").GetComponent<Button>();
            closeBtn.onClick.AddListener(OnCloseClicked);

            Transform titleTrans = transform.Find("TopBar/Title");
            titleController = new TitleController(titleTrans);

            Transform qrcodeTrans = transform.Find("QRCode");
            qrcodeController = new QRCodeController(qrcodeTrans, OnAuth);

            Transform webTrans = transform.Find("Web");
            webController = new WebController(webTrans, OnAuth);
        }

        protected override void OnLoadSuccess() {
            base.OnLoadSuccess();
            
            OpenParams openParams = openParam as OpenParams;
            titleController.Load();
            qrcodeController.Load(openParams.ClientId, openParams.Scopes);
            webController.Load(openParams.ClientId, openParams.Scopes);
        }

        private void OnDestroy() {
            qrcodeController.Unload();
            webController.Unload();
        }

        private void OnCloseClicked() {
            Close();
            OpenParams openParams = openParam as OpenParams;
            openParams.OnClose.Invoke();
        }

        protected void OnAuth(TokenData tokenData, String loginType) {
            if (tokenData != null) {
                OpenParams openParams = openParam as OpenParams;
                openParams.OnAuth.Invoke(tokenData, loginType);
                Close();           
            }
        }
    }
}