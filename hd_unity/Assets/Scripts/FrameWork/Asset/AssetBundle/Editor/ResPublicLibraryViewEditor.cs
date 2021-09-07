using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

namespace DragonU3DSDK.Asset
{
    public class ResPublicLibraryViewEditor : EditorWindow
    {
        public enum Platform
        {
            android,
            iphone,
            mac,
            win,
        }

        public enum PublicType
        {
            None,
            Cooking,
            Home
        }

        static ResPublicLibraryViewEditor m_EditorWindow;

        [MenuItem("AssetBundle/公共库浏览", false, 1)]
        public static void ViewResPublicLibrary()
        {
            m_EditorWindow = (ResPublicLibraryViewEditor) EditorWindow.GetWindowWithRect(typeof(ResPublicLibraryViewEditor), new Rect(0, 0, 400, 400), true, "公共库浏览");
            m_EditorWindow.Show();
        }

        static ResPublicLibraryDetialEditor m_DetailWindow;

        static void ViewResPublicLibraryDetia(string name, string url, Platform platform, PublicType publicType)
        {
            if (null != m_DetailWindow)
            {
                m_DetailWindow.Close();
            }

            m_DetailWindow = (ResPublicLibraryDetialEditor) EditorWindow.GetWindowWithRect(typeof(ResPublicLibraryDetialEditor), new Rect(0, 0, 800, 800), true, name + " 详情");
            m_DetailWindow.Init(url);
            m_DetailWindow.platform = platform;
            m_DetailWindow.publicType = publicType;
        }

        private Vector2 scrollPosition;
        private Platform selectPlatform = Platform.android;

        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Android"))
            {
                selectPlatform = Platform.android;
            }
            if (GUILayout.Button("iOS"))
            {
                selectPlatform = Platform.iphone;
            }
            if (GUILayout.Button("Mac"))
            {
                selectPlatform = Platform.mac;
            }
            if (GUILayout.Button("Win"))
            {
                selectPlatform = Platform.win;
            }
            GUILayout.EndHorizontal();

            List<ResPublicLibraryCommit> selectCommit = null;
            if (selectPlatform == Platform.android)
            {
                GUILayout.Label("当前选择：Android");
                selectCommit = ResPublicLibraryController.Instance.Commit_Android;
            }
            else if (selectPlatform == Platform.iphone)
            {
                GUILayout.Label("当前选择：iOS");
                selectCommit = ResPublicLibraryController.Instance.Commit_iOS;
            }
            else if (selectPlatform == Platform.mac)
            {
                GUILayout.Label("当前选择：Mac");
                selectCommit = ResPublicLibraryController.Instance.Commit_Mac;
            }
            else if (selectPlatform == Platform.win)
            {
                GUILayout.Label("当前选择：Mac");
                selectCommit = ResPublicLibraryController.Instance.Commit_Win;
            }
            
            if (null != ResPublicLibraryController.Instance)
            {
                foreach (var p in selectCommit)
                {
                    if (GUILayout.Button(p.CustomName, GUILayout.Width(320)))
                    {
                        var resUrl = ConfigurationController.Instance.version != VersionStatus.RELEASE ? p.Res_Server_URL_Beta : p.Res_Server_URL_Release;
                        var libType = GetTypeFromName(p.CustomName);
                        ViewResPublicLibraryDetia(p.CustomName, $"{resUrl}/{selectPlatform.ToString()}", selectPlatform, libType);
                    }
                }
            }

            GUILayout.EndScrollView();
        }


        public static PublicType GetTypeFromName(string libName)
        {
            var libType = PublicType.None;
            if (libName.ToLower().StartsWith("cooking")) libType = PublicType.Cooking;
            if (libName.ToLower().StartsWith("home")) libType = PublicType.Home;

            return libType;
        }
    }

    public class ResPublicLibraryDetialEditor : EditorWindow
    {
        private string url;
        private List<VersionInfo> _versionList = new List<VersionInfo>();

        public ResPublicLibraryViewEditor.Platform platform;
        public ResPublicLibraryViewEditor.PublicType publicType;

        class VersionInfo
        {
            public VersionInfo(DateTime time, string version)
            {
                this.time = time;
                this.version = version;
            }

            public DateTime time;
            public string version;
        }

        public void Init(string _url)
        {
            url = _url;
            detail = RequestDetail($"{url}/ResPubLibraryCommit.json");

            var table = JsonConvert.DeserializeObject<Hashtable>(detail);

            _versionList.Clear();
            foreach (var key in table.Keys)
            {
                var timeObj = JsonConvert.DeserializeObject<Hashtable>(table[key].ToString());
                var timeStr = timeObj["time"] as string;
                timeStr = timeStr.Remove(10, 1);
                timeStr = timeStr.Insert(10, " ");
                var dateTime = Convert.ToDateTime(timeStr);

                _versionList.Add(new VersionInfo(dateTime, key as string));
            }

            _versionList.Sort((a, b) => a.time < b.time ? 1 : -1);
        }


        private string detialCommit = "";

        private string compareCommitA = "";
        private string compareCommitB = "";

        private string detail = "";

        private Vector2 scrollPosition;

        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.Label("commit :");
            detialCommit = GUILayout.TextField(detialCommit, 32);
            if (GUILayout.Button("详细", GUILayout.Width(60)) && !string.IsNullOrEmpty(detialCommit))
            {
                string data = RequestDetail($"{url}/{detialCommit}/Version.10000.txt");
                TextEditor t = new TextEditor();
                t.text = Utils.ConvertJsonString(data);
                t.OnFocus();
                t.Copy();

                EditorUtility.DisplayDialog("Info", "Version详细信息已复制到剪贴板。", "ok");
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("commit A:");
            compareCommitA = GUILayout.TextField(compareCommitA, 32);
            GUILayout.Label("commit B:");
            compareCommitB = GUILayout.TextField(compareCommitB, 32);
            if (GUILayout.Button("比较", GUILayout.Width(60)) && !string.IsNullOrEmpty(compareCommitA) && !string.IsNullOrEmpty(compareCommitB))
            {
                string detailA = RequestDetail($"{url}/{compareCommitA}/Version.10000.txt");
                string detailB = RequestDetail($"{url}/{compareCommitB}/Version.10000.txt");

                Asset.VersionInfo versionA = JsonConvert.DeserializeObject<Asset.VersionInfo>(detailA);
                Asset.VersionInfo versionB = JsonConvert.DeserializeObject<Asset.VersionInfo>(detailB);

                string onlyA = "";
                string onlyB = "";
                string diff = "";

                //A独有
                foreach (var groupA in versionA.ResGroups)
                {
                    string temp = "";
                    if (!versionB.ResGroups.ContainsKey(groupA.Key))
                    {
                        foreach (var ab in groupA.Value.AssetBundles)
                        {
                            temp += $"{ab.Key}\n";
                        }
                    }
                    else
                    {
                        foreach (var ab in groupA.Value.AssetBundles)
                        {
                            if (!versionB.ResGroups[groupA.Key].AssetBundles.ContainsKey(ab.Key))
                            {
                                temp += $"{ab.Key}\n";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(temp))
                    {
                        onlyA += $"{groupA.Key} : \n";
                        onlyA += temp;
                    }
                }

                //B独有
                foreach (var groupB in versionB.ResGroups)
                {
                    string temp = "";
                    if (!versionA.ResGroups.ContainsKey(groupB.Key))
                    {
                        foreach (var ab in groupB.Value.AssetBundles)
                        {
                            temp += $"{ab.Key}\n";
                        }
                    }
                    else
                    {
                        foreach (var ab in groupB.Value.AssetBundles)
                        {
                            if (!versionA.ResGroups[groupB.Key].AssetBundles.ContainsKey(ab.Key))
                            {
                                temp += $"{ab.Key}\n";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(temp))
                    {
                        onlyB += $"{groupB.Key} : \n";
                        onlyB += temp;
                    }
                }

                //差异
                foreach (var groupA in versionA.ResGroups)
                {
                    if (versionB.ResGroups.ContainsKey(groupA.Key))
                    {
                        foreach (var ab in groupA.Value.AssetBundles)
                        {
                            if (versionB.ResGroups[groupA.Key].AssetBundles.ContainsKey(ab.Key))
                            {
                                bool hash = ab.Value.HashString != versionB.ResGroups[groupA.Key].AssetBundles[ab.Key].HashString;
                                bool md5 = ab.Value.Md5 != versionB.ResGroups[groupA.Key].AssetBundles[ab.Key].Md5;
                                if (hash || md5)
                                {
                                    diff += ab.Key;
                                    if (hash)
                                    {
                                        diff += " -Hash";
                                    }

                                    if (md5)
                                    {
                                        diff += " -Md5";
                                    }

                                    diff += "\n";
                                }
                            }
                        }
                    }
                }

                string all = $"A独有:\n{onlyA}\nB独有:\n{onlyB}\nAB差异:\n{diff}";

                TextEditor t = new TextEditor();
                t.text = all;
                t.OnFocus();
                t.Copy();

                EditorUtility.DisplayDialog("Info", "对比结果已复制到剪贴板。", "ok");
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("最新version写入配置文件", GUILayout.Width(200)))
            {
                List<ResPublicLibraryCommit> listPublickLibrary = null;
                if (platform == ResPublicLibraryViewEditor.Platform.android)
                {
                    listPublickLibrary = ResPublicLibraryController.Instance.Commit_Android;
                }
                else if (platform == ResPublicLibraryViewEditor.Platform.iphone)
                {
                    listPublickLibrary = ResPublicLibraryController.Instance.Commit_iOS;
                }
                else if (platform == ResPublicLibraryViewEditor.Platform.mac)
                {
                    listPublickLibrary = ResPublicLibraryController.Instance.Commit_Mac;
                }
                else if (platform == ResPublicLibraryViewEditor.Platform.win)
                {
                    listPublickLibrary = ResPublicLibraryController.Instance.Commit_Win;
                }
                
                ResPublicLibraryCommit commit = null;
                if (publicType == ResPublicLibraryViewEditor.GetTypeFromName(listPublickLibrary[0].CustomName))
                {
                    commit = listPublickLibrary[0];
                }
                else if (publicType == ResPublicLibraryViewEditor.GetTypeFromName(listPublickLibrary[1].CustomName))
                {
                    commit = listPublickLibrary[1];
                }

                if (commit != null)
                {
                    commit.Commit = _versionList[0].version;

                    EditorUtility.SetDirty(ResPublicLibraryController.Instance);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Success", "写入成功 [ " + commit.Commit + " ]", "ok");
                }
                else
                {
                    EditorUtility.DisplayDialog("Fail", "写入失败 [ 未匹配到库名 ]", "ok");
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            // GUILayout.TextArea(Utils.ConvertJsonString(detail));
            foreach (var v in _versionList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{v.time} version:{v.version}");
                if (GUILayout.Button("复制version到剪贴板", GUILayout.Width(200)))
                {
                    GUIUtility.systemCopyBuffer = v.version;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        private string RequestDetail(string url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream inStream = response.GetResponseStream(); //获取http
            byte[] b = new byte[1024 * 1024]; //每一次获取的长度
            List<byte> outStream = new List<byte>();
            int readCount = inStream.Read(b, 0, b.Length); //读流
            while (readCount > 0)
            {
                byte[] temp = new byte[readCount];
                Array.Copy(b, temp, readCount);
                outStream.AddRange(temp);
                readCount = inStream.Read(b, 0, b.Length); //再读流
            }

            inStream.Close();
            response.Close();

            return Encoding.Default.GetString(outStream.ToArray());
        }
    }
}