using System;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;

namespace LeanCloud.Play {
    internal class PlayRouter {
        readonly string appId;
        readonly string playServer;

        string url;
        long serverValidTimestamp;

        readonly object lockObj = new object();

        internal PlayRouter(string appId, string playServer) {
            this.appId = appId;
            this.playServer = playServer;
            url = null;
            serverValidTimestamp = 0;
        }

        internal Task<string> Fetch() {
            if (playServer != null) {
                return Task.FromResult(string.Format("{0}/router", playServer));
            }
            var now = DateTimeUtils.Now;
            if (now < serverValidTimestamp) {
                Logger.Debug("Get server from cache");
                return Task.FromResult(url);
            }
            return FetchFromServer();
        }

        Task<string> FetchFromServer() {
            var tcs = new TaskCompletionSource<string>();
            Task.Run(() => {
                try {
                    var client = new WebClient();
                    client.QueryString.Add("appId", appId);
                    var content = client.DownloadString("https://app-router.leancloud.cn/2/route");
                    Logger.Debug(content);
                    var response = Json.Parse(content) as Dictionary<string, object>;
                    string primaryServer = null;
                    string secondaryServer = null;
                    if (response.TryGetValue("multiplayer_router_server", out object primaryServerObj)) {
                        primaryServer = primaryServerObj.ToString();
                    }
                    if (response.TryGetValue("play_server", out object secondaryServerObj)) {
                        secondaryServer = secondaryServerObj.ToString();
                    }
                    var routerServer = primaryServer ?? secondaryServer;
                    if (routerServer == null) {
                        tcs.SetException(new ArgumentNullException(nameof(routerServer)));
                    } else {
                        lock (lockObj) {
                            if (response.TryGetValue("ttl", out object ttlObj)) {
                                var ttl = int.Parse(response["ttl"].ToString());
                                serverValidTimestamp = DateTimeUtils.Now + ttl * 1000;
                            }
                            url = string.Format("https://{0}/1/multiplayer/router/router", routerServer);
                        }
                        tcs.SetResult(url);
                    }
                } catch (Exception e) {
                    Logger.Error(e.Message);
                    tcs.SetException(e);
                }
            });
            return tcs.Task;
        }
    }
}
