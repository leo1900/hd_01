using System.Collections.Generic;
using System.IO;
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        static List<ResPublicLibraryCommit> GetCommits(BuildTarget target)
        {
            if (null == ResPublicLibraryController.Instance)
            {
                return new List<ResPublicLibraryCommit>();
            }
            
            if (target == BuildTarget.iOS)
            {
                return ResPublicLibraryController.Instance.Commit_iOS;
            }
            else if(target == BuildTarget.Android)
            {
                return ResPublicLibraryController.Instance.Commit_Android;
            }
            else if(target == BuildTarget.StandaloneOSX)
            {
                return ResPublicLibraryController.Instance.Commit_Mac;
            }
            else
            {
                return ResPublicLibraryController.Instance.Commit_Win;
            }
        }

        static ResPublicLibraryCommit GetCommit(string name, BuildTarget target)
        {
            List<ResPublicLibraryCommit> commits = GetCommits(target);
            return commits.Find(x => x.CustomName.Equals(name));
        }
        
        static void RequestResPublicLibraryVersionFile(ResPublicLibraryCommit commit, BuildTarget target, ref VersionInfo info, int tryTime = 0)
        {
            string fileUrl = commit.GetVersionFileUrl(true);
            
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(fileUrl);
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

                string text = Encoding.Default.GetString(outStream.ToArray());
                info = JsonConvert.DeserializeObject<VersionInfo>(text);
            }
            catch (Exception ex)
            {
                if (tryTime >= 5)
                {
                    throw new Exception(string.Format("can not download file : {0} in DownLoadResPublicLibraryVersionFile", fileUrl));
                }
                else
                {
                    RequestResPublicLibraryVersionFile(commit, target, ref info, ++tryTime);
                }
            }
        }
        
        static void DownloadResPublicLibraryAssetBundles(ResPublicLibraryCommit commit, BuildTarget target, BundleState item, string save_root_path, string md5, int tryTime = 0)
        {
            string fileUrl = $"{commit.GetResServerURL(true)}/{(string.IsNullOrEmpty(item.PathOutside) ? item.Path : item.PathOutside).ToLower() + ".ab"}"; 
            string savePath = save_root_path + "/" + item.Path.ToLower() + ".ab";
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            FilePathTools.CreateFolderByFilePath(savePath);
            
            try
            {
                Stream outStream;
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(fileUrl);
                WebResponse response = request.GetResponse();
                Stream inStream = response.GetResponseStream(); //获取http
                byte[] b = new byte[1024 * 1024]; //每一次获取的长度
                FileInfo fi = new FileInfo(savePath);
                outStream = fi.Create(); //创建文件
                int readCount = inStream.Read(b, 0, b.Length); //读流
                while (readCount > 0)
                {
                    outStream.Write(b, 0, readCount); //写流
                    readCount = inStream.Read(b, 0, b.Length); //再读流
                }

                outStream.Close();
                inStream.Close();
                response.Close();
                
                string localMd5 = AssetUtils.BuildFileMd5(savePath);
                if (localMd5.Trim() != md5)
                {
                    DebugUtil.LogError(string.Format("md5 error : {0} in DownloadResPublicLibraryAssetBundles. local : {1} ; remote : {2}", fileUrl, localMd5, md5));
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                if (tryTime >= 5)
                {
                    throw new Exception(string.Format("can not request file : {0} in DownloadResPublicLibraryAssetBundles", fileUrl));
                }
                else
                {
                    DownloadResPublicLibraryAssetBundles(commit, target, item, save_root_path, md5, ++tryTime);
                }
            }
        }

        public static void PatchResPubLibraryCommit()
        {
            string commitValueFilePath = $"{FilePathTools.assetbundlePatchPath}/CommitValue.txt";
            if (!File.Exists(commitValueFilePath))
            {
                throw new Exception("can not find CommitValue.txt");
            }

            string commitValue = File.ReadAllText(commitValueFilePath);
            if (string.IsNullOrEmpty(commitValue))
            {
                throw new Exception("CommitValue is null");
            }
            
            string filePath = $"{FilePathTools.assetbundlePatchPath}/ResPubLibraryCommit.json";
            string value = "{}";
            if (File.Exists(filePath))
            {
                value = File.ReadAllText(filePath);
            }
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(value);
            JsonTextReader jtr = new JsonTextReader(tr);
            JObject root = serializer.Deserialize(jtr) as JObject;
            if (root.GetValue(commitValue) != null)
            {
                throw new Exception($"{commitValue} CommitValue is already exist!!!");
            }
            JObject detaile = new JObject();
            detaile["time"] = System.DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
            root.Add(commitValue, detaile);
            File.WriteAllText(filePath, root.ToString());
            
            Debug.Log("PatchResPubLibraryCommit finish");
        }
    }
}