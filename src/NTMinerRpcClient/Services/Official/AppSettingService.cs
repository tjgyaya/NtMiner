﻿using NTMiner.Controllers;
using NTMiner.Core;
using NTMiner.Core.MinerServer;
using NTMiner.ServerNode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NTMiner.Services.Official {
    public class AppSettingService {
        private readonly string _controllerName = ControllerUtil.GetControllerName<IAppSettingController>();

        internal AppSettingService() {
        }

        #region GetJsonFileVersionAsync
        public void GetJsonFileVersionAsync(NTMinerAppType appType, string key, Action<ServerStateResponse> callback) {
            HashSet<string> macAddresses = new HashSet<string>();
            foreach (var localIp in VirtualRoot.LocalIpSet.AsEnumerable().ToArray()) {
                macAddresses.Add(localIp.MACAddress);
            }
            JsonFileVersionRequest request = new JsonFileVersionRequest {
                Key = key,
                ClientId = NTMinerRegistry.GetClientId(appType),
                MACAddress = macAddresses.ToArray()
            };
            RpcRoot.JsonRpc.PostAsync(
                RpcRoot.OfficialServerHost,
                RpcRoot.OfficialServerPort,
                _controllerName,
                nameof(IAppSettingController.GetServerState),
                request,
                callback: (ServerStateResponse response, Exception e) => {
                    if (e != null) {
                        Logger.ErrorDebugLine(e);
                    }
                    if (response == null) {
                        response = ServerStateResponse.Empty;
                        Logger.ErrorWriteLine("询问服务器状态失败，请检查网络。");
                    }
                    if (response.NeedReClientId) {
                        NTMinerRegistry.ReClientId(ClientAppType.AppType);
                        RpcRoot.Client.NTMinerDaemonService.ReClientIdAsync(appType);
                        Logger.InfoDebugLine("检测到本机标识存在重复，已重新生成");
                    }
                    callback?.Invoke(response);
                }, timeountMilliseconds: 10 * 1000);
        }
        #endregion

        #region SetAppSettingAsync
        public void SetAppSettingAsync(AppSettingData entity, Action<ResponseBase, Exception> callback) {
            DataRequest<AppSettingData> request = new DataRequest<AppSettingData>() {
                Data = entity
            };
            RpcRoot.JsonRpc.SignPostAsync(
                RpcRoot.OfficialServerHost, 
                RpcRoot.OfficialServerPort, 
                _controllerName, 
                nameof(IAppSettingController.SetAppSetting), 
                data: request, 
                callback);
        }
        #endregion
    }
}
