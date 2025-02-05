using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using HybridCLR;
using VivaFramework;

namespace VivaFramework
{
    public class Main : MonoBehaviour
    {
        private static List<string> AOTMetaAssemblyFiles { get; } = new List<string>()
        {
            "mscorlib.dll.bytes",
            "System.dll.bytes",
            "System.Core.dll.bytes",
        };

        private static Dictionary<string, byte[]> _dllDatas = new Dictionary<string, byte[]>();

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(gameObject); //防止销毁自己
            CheckExtractResource();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = AppConst.GameFrameRate;
        }

        /// <summary>
        /// 释放资源到设备可读写目录
        /// </summary>
        public void CheckExtractResource()
        {
            print("CheckExtractResource " + Util.DataPath);
            print("Version Check - " + PlayerPrefs.GetString("lastVersion") + " - " + Application.version);
            //更新模式从网上下资源
            if (AppConst.UpdateMode == true)
            {
                StartCoroutine(OnUpdateResource());
            }
            //为了测试方便 兼容本地模式 从本地解压资源
            else if (AppConst.UseBundle == true)
            {
                bool isExists = Directory.Exists(Util.DataPath) && File.Exists(Util.DataPath + "files.txt");
                Debug.Log("文件是否已解压 - " + isExists);
                if (isExists && PlayerPrefs.GetString("lastVersion") == Application.version)
                {
                    OnResourceInited();
                    return; //文件已经解压过了，自己可添加检查文件列表逻辑
                }

                if (Directory.Exists(Util.DataPath) == true)
                {
                    Directory.Delete(Util.DataPath, true);
                }

                StartCoroutine(OnExtractResource()); //启动释放协成
            }
            else
            {
                OnResourceInited();
            }
        }


        /// <summary>
        /// 创建进度条
        /// </summary>
        public IEnumerator CreateProgressUI()
        {

            ResourceRequest req = Resources.LoadAsync<GameObject>("UpdateCanvas");
            yield return req;
            //                        print("UpdateCanvas加载完毕" + req.asset + " " +req.isDone);
            GameObject canvas = GameObject.Instantiate((GameObject)req.asset);
            _updateLoadingUI = canvas.transform.Find("UpdateLoading").gameObject;
            _updateLoadingUI.SetActive(true);
        }

        IEnumerator OnExtractResource()
        {
            string dataPath = Util.DataPath; //数据目录
            string resPath = Util.AppContentPath(); //游戏包资源目录

            print("OnExtractResource " + dataPath + " " + resPath);

            if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
            Directory.CreateDirectory(dataPath);

            string infile = resPath + "files.txt";
            string outfile = dataPath + "files.txt";
            if (File.Exists(outfile)) File.Delete(outfile);

            Debug.Log(infile);
            Debug.Log(outfile);
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                UnityWebRequest www = UnityWebRequest.Get(infile);
                yield return www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    File.WriteAllBytes(outfile, www.downloadHandler.data);
                }

                yield return 0;
            }
            else File.Copy(infile, outfile, true);

            yield return new WaitForEndOfFrame();

            //释放所有文件到数据目录
            string[] files = File.ReadAllLines(outfile);

            Debug.Log("要拷贝的文件数量：" + files.Length);
            yield return CreateProgressUI();
            for (int i = 0; i < files.Length; i++)
            {
                string[] fs = files[i].Split('|');
                infile = resPath + fs[0]; //
                outfile = dataPath + fs[0];

                _extractProgress = (float)i / (float)files.Length;
                _extractFileName = fs[0];

                Debug.Log("正在解包文件:>" + infile + " - " + i + " - " + files.Length + " - " + _extractProgress);

                string dir = Path.GetDirectoryName(outfile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    UnityWebRequest www = UnityWebRequest.Get(infile);
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        File.WriteAllBytes(outfile, www.downloadHandler.data);
                    }

                    yield return 0;
                }
                else
                {
                    if (File.Exists(outfile))
                    {
                        File.Delete(outfile);
                    }

                    File.Copy(infile, outfile, true);
                }


                yield return new WaitForEndOfFrame();
            }

            _extractProgress = 1;
            yield return new WaitForSeconds(0.2f);

            if (_updateLoadingUI != null)
            {
                _updateLoadingUI.transform.parent.gameObject.SetActive(false);
                _updateLoadingUI = null;
            }

            PlayerPrefs.SetString("lastVersion", Application.version);
            PlayerPrefs.Save();
            OnResourceInited();
        }

        /// <summary>
        /// 启动更新下载
        /// </summary>
        IEnumerator OnUpdateResource()
        {
            if (!AppConst.UseBundle)
            {
                OnResourceInited();
                yield break;
            }

            string dataPath = Util.DataPath; //数据目录
            string url = "http://" + AppConst.GameServerIP + "/";
#if UNITY_EDITOR
            url = "http://127.0.0.1/";
#endif
            string random = DateTime.Now.ToString("yyyymmddhhmmss");
            // string listUrl = url + "files.txt?v=" + random;
            string listUrl = url + "files.txt";
            Debug.Log("LoadUpdate---->>>" + listUrl);

            // WWW www = new WWW(listUrl);
            UnityWebRequest uwr = UnityWebRequest.Get(listUrl);
            uwr.timeout = 3;
            yield return uwr.SendWebRequest();
            if (uwr.isHttpError || uwr.isNetworkError)
            {
                Debug.LogError("OnUpdateResource更新失败!>" + uwr.error);
                yield break;
            }

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            // Debug.LogWarning("下载完毕 " + uwr.isDone + " - " + uwr.downloadHandler);
            File.WriteAllBytes(dataPath + "files.txt", uwr.downloadHandler.data);
            string filesText = uwr.downloadHandler.text;
            string[] files = filesText.Split('\n');

            //TEST 测试更新界面的可见
            //            ResourceRequest reqq = Resources.LoadAsync<GameObject>("UpdateCanvas");
            //            yield return reqq;
            //            GameObject canvass = GameObject.Instantiate((GameObject)reqq.asset);
            //            GameObject g = canvass.transform.Find("UpdateLoading").gameObject;
            //            g.SetActive(true);
            //            yield return new WaitForSeconds(200);
            //TEST


            for (int i = 0; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i])) continue;
                string[] keyValue = files[i].Split('|');
                string f = keyValue[0];
                string localfile = (dataPath + f).Trim();
                string path = Path.GetDirectoryName(localfile);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileUrl = url + f + "?v=" + random;
                bool canUpdate = !File.Exists(localfile);
                if (!canUpdate)
                {
                    string remoteMd5 = keyValue[1].Trim();
                    string localMd5 = Util.md5file(localfile);
                    canUpdate = !remoteMd5.Equals(localMd5);
                    if (canUpdate) File.Delete(localfile);
                    Debug.Log("OnUpdateResource - " + f + " - " + remoteMd5 + " - " + localMd5);
                }

                if (canUpdate)
                {
                    if (_updateLoadingUI == null)
                    {
                        yield return CreateProgressUI();
                    }

                    //本地缺少文件
                    Debug.Log("OnUpdateResource更新的文件 - " + fileUrl + " - " + localfile);

                    _updateRequest = new WWW(fileUrl);
                    _updateFileName = f;
                    yield return _updateRequest;
                    if (_updateRequest.error != null)
                    {
                        Debug.LogError("OnUpdateResource更新失败!>" + path);
                        yield break;
                    }

                    File.WriteAllBytes(localfile, _updateRequest.bytes);
                    _updateRequest = null;
                    Debug.Log("OnUpdateResource更新完毕 - " + fileUrl + " - " + localfile);

                }
            }

            yield return new WaitForEndOfFrame();

            if (_updateLoadingUI != null)
            {
                _updateLoadingUI.transform.parent.gameObject.SetActive(false);
                _updateLoadingUI = null;
            }

            OnResourceInited();
        }


        private WWW _updateRequest;
        private string _updateFileName;
        private GameObject _updateLoadingUI;

        private string _extractFileName;
        private float _extractProgress = 0;

        private void Update()
        {
            if (_updateLoadingUI == null) return;

            //            print("你妹啊 "+ _extractFileName + _extractProgress);
            if (_updateRequest != null)
            {
                Text infoText = _updateLoadingUI.transform.Find("bg/text").gameObject.GetComponent<Text>();
                Image barImg = _updateLoadingUI.transform.Find("bg/bar").gameObject.GetComponent<Image>();
                barImg.fillAmount = _updateRequest.progress;
                infoText.text = _updateFileName + " " + Math.Floor(_updateRequest.progress * 100) + "%";
            }
            else if (_extractProgress > 0)
            {
                Text infoText = _updateLoadingUI.transform.Find("bg/text").gameObject.GetComponent<Text>();
                Image barImg = _updateLoadingUI.transform.Find("bg/bar").gameObject.GetComponent<Image>();
                barImg.fillAmount = _extractProgress;
                infoText.text = "解压中：" + _extractFileName + " " + Math.Floor(_extractProgress * 100) + "%";
            }


        }


        /// <summary>
        /// 资源初始化结束
        /// </summary>
        public void OnResourceInited()
        {
            if (AppConst.UseBundle == true)
            {
                StartCoroutine(LoadDlls(StartGame));
            }
            else
            {
                StartGame();
            }
        }

        public static ResourceManager resManager;
        public static SceneManager sceneManager;
        public static AudioManager audioManager;

        private void InitManagers()
        {
            resManager = gameObject.AddComponent<ResourceManager>();
        }

        private string GetWebRequestPath(string asset)
        {
            var path = $"{Util.DataPath}{asset}";
            if (!path.Contains("://") && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                path = "file://" + path;
            }

            return path;
        }

        IEnumerator LoadDlls(Action onDownloadComplete)
        {
            Debug.Log("你妹啊~LoadDlls");
            var assets = new List<string>
            {
                "HotUpdateTest.dll.bytes",
            }.Concat(AOTMetaAssemblyFiles);

            foreach (var asset in assets)
            {
                string dllPath = GetWebRequestPath(asset);
                Debug.Log($"start download asset:{dllPath}");
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    
                    byte[] hehe = File.ReadAllBytes(dllPath);
                    yield return null;
                    _dllDatas[asset] = hehe;
                
                    // Task<byte[]> readTask = File.ReadAllBytesAsync(dllPath);
                    // while (!readTask.IsCompleted)
                    // {
                    //     yield return null; // 等待一帧
                    // }
                    // _dllDatas[asset] = readTask.Result;
                }
                else
                {
                
                    UnityWebRequest www = UnityWebRequest.Get(dllPath);
                    yield return www.SendWebRequest();
                
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        // Or retrieve results as binary data
                        byte[] assetData = www.downloadHandler.data;
                        // Debug.Log($"dll:{asset}  size:{assetData.Length}");
                        _dllDatas[asset] = assetData;
                    }
                }
                
            }

            onDownloadComplete();
        }

        /// <summary>
        /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
        /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
        /// </summary>
        private static void InitDlls()
        {
            /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in AOTMetaAssemblyFiles)
            {
                byte[] dllBytes = _dllDatas[aotDllName];
                // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }

        private static Assembly _hotUpdateAss;

        private void StartGame()
        {
            if (AppConst.UseBundle == true)
            {
                InitDlls();
                _hotUpdateAss = Assembly.Load(_dllDatas["HotUpdateTest.dll.bytes"]);
            }
            else
            {
                _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies()
                    .First(a => a.GetName().Name == "HotUpdateTest");
            }

            InitManagers();
            StartCoroutine( Test());
        }

        IEnumerator Test()
        {
            yield return new WaitForSeconds(1);
            Type entryType = _hotUpdateAss.GetType("Test1");
            entryType.GetMethod("Start").Invoke(null, null);
        }
    }
}
