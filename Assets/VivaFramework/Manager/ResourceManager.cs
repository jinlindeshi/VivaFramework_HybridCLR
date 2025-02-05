
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UObject = UnityEngine.Object;

namespace VivaFramework
{
    public class ResourceManager : MonoBehaviour
    {
        private string[] m_Variants = { };
        private AssetBundleManifest manifest;
        private AssetBundle shared, assetbundle;
        
        //已经加载好的bundle
        private Dictionary<string, AssetBundle> bundles;
        //正在卸载的bundle
        private Dictionary<string, AssetBundle> unloadingBundles;

        
        private static ResourceManager _instance;
        public static ResourceManager Instance
        {
            get{
                return _instance;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Start()
        {
            _instance = this;
            string uri = string.Empty;
            bundles = new Dictionary<string, AssetBundle>();
            unloadingBundles = new Dictionary<string, AssetBundle>();
            uri = Util.DataPath + AppConst.AssetDir;
            Debug.Log("ResourceManager - " + uri);
            if (AppConst.UseBundle == false || !File.Exists(uri)) return;
            // var fileStream = new FileStream(uri, FileMode.Open, FileAccess.Read);
            // assetbundle = AssetBundle.LoadFromStream(fileStream); //关联数据的素材绑定
            assetbundle = AssetBundle.LoadFromFile(uri);
            manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            assetbundle.Unload(false);
            
        }

        public T LoadAnythingAtPath<T>(string assetPath) where T:UnityEngine.Object
        {
            if (AppConst.UseBundle == true)
            {
                return LoadAsset<T>(assetPath);
            }
            else
            {
#if UNITY_EDITOR
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>("Assets/" + AppConst.ResPathHead + assetPath);
#else
                return null;
#endif
            }
        }

        public GameObject LoadPrefabAtPath(string assetPath)
        {
            return LoadAnythingAtPath<GameObject>(assetPath);
        }
        public void LoadAssetAtPathAsync(string assetPath, Action callBack)
        {
            if (AppConst.UseBundle != true)
            {
                return;
            }
            string buldleName = assetPath.Replace('\\', '^').Replace('/', '^');
            buldleName = buldleName.Substring(0, buldleName.LastIndexOf("^"));
            LoadAssetBundle(buldleName.ToLower(), true, callBack);
        }

        public UObject LoadAssetAtPath(string assetPath)
        {
            return LoadAnythingAtPath<UObject>(assetPath);
        }

        public Material LoadMaterialAtPath(string assetPath)
        {
            return LoadAnythingAtPath<Material>(assetPath);
        }
        public Sprite LoadSpriteAtPath(string assetPath)
        {
            return LoadAnythingAtPath<Sprite>(assetPath);
        }
        /// <summary>
        /// 载入素材
        /// </summary>
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            string buldleName = assetPath.Replace('\\', '^').Replace('/', '^');
            string assetName = buldleName.Substring(buldleName.LastIndexOf("^") + 1);
            assetName = assetName.Substring(0, assetName.IndexOf("."));
            buldleName = buldleName.Substring(0, buldleName.LastIndexOf("^"));
            buldleName = buldleName.ToLower();
            // Debug.Log("LoadAsset 你妹啊 " + assetPath + " - " + buldleName);
            AssetBundle bundle = LoadAssetBundle(buldleName, false);
            T obj = bundle.LoadAsset<T>(assetName);
            return obj;
        }

        
        /// <summary>
        /// 卸载资源
        /// </summary>
        public bool UnLoadBundleByAssetPath(string assetPath, bool withDepends = false)
        {
            string buldleName = assetPath.Replace('\\', '^').Replace('/', '^');
            buldleName = buldleName.Substring(0, buldleName.LastIndexOf("^"));
            buldleName = buldleName.ToLower();
            if (!buldleName.EndsWith(AppConst.ExtName))
            {
                buldleName += AppConst.ExtName;
            }
            if (bundles.ContainsKey(buldleName))
            {
                Debug.Log("UnLoadBundleByAssetPath1 - " + assetPath + " - " + buldleName);
                StartCoroutine(UnLoadAssetBundleAsync(buldleName));
                if (withDepends == true)
                {
                    string[] dependencies = GetDependencies(buldleName);
                    
                    if (dependencies != null)
                    {
                        // Record and load all dependencies.
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            string dependName = dependencies[i];
                            if (!dependName.EndsWith(AppConst.ExtName))
                            {
                                dependName += AppConst.ExtName;
                            }
                            if (bundles.ContainsKey(dependName))
                            {
                                Debug.Log("UnLoadBundleByAssetPath2 - " + dependName);
                                StartCoroutine(UnLoadAssetBundleAsync(dependName));
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        
        /// <summary>
        /// 依照模糊关键字 卸载资源
        /// </summary>
        public void UnLoadBundleByFuzzyKey(string key)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, AssetBundle> kvp in bundles)
            {
                if (kvp.Key.Contains(key) == true)
                {
                    Debug.Log("UnLoadBundleByFuzzyKey - " + kvp.Key);
                    list.Add(kvp.Key);
                    // StartCoroutine(UnLoadAssetBundleAsync(kvp.Key));
                }
            }

            for (int i = 0; i < list.Count - 1; i++)
            {
                StartCoroutine(UnLoadAssetBundleAsync(list[i]));
            }
        }

        private Dictionary<string, List<Action>> ABCallBacks = new Dictionary<string, List<Action>>();
        /// <summary>
        /// 载入AssetBundle
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string abname, bool async, Action callBack = null)
        {
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }

            AssetBundle bundle = null;
            // Debug.Log("你妹啊~2 " + bundles);
            if (!bundles.ContainsKey(abname))
            {
                
                if (async == true)
                {

                    if (callBack != null)
                    {
                        if (ABCallBacks.ContainsKey(abname) == false)
                        {
                            ABCallBacks[abname] = new List<Action>();
                        }
                        ABCallBacks[abname].Add(callBack);
                    }

                    if (_waitingDepends.ContainsKey(abname) == true || _loadingAssets.ContainsKey(abname) == true)
                    {
                        return null;
                    }

                    Debug.Log("LoadAssetBundle " + abname + " " + async);
                    StartCoroutine(LoadAssetBundleAsync(abname));
                }
                else
                {
                    //本身在异步加载中，直接底层异步转同步
                    if (_waitingDepends.ContainsKey(abname) == true || _loadingAssets.ContainsKey(abname) == true)
                    {
                        CompleteAsyncLoading(abname);
                        bundles.TryGetValue(abname, out bundle);
                        if (callBack != null)
                        {
                            callBack();
                        }
                        return bundle;
                    }
                    
                    Debug.Log("LoadAssetBundle 你妹啊 " + abname + " " + async);
                    string uri = Util.DataPath + abname;

                    bundle = AssetBundle.LoadFromFile(uri); //关联数据的素材绑定
                    bundles[abname] = bundle;
                    // StartCoroutine(UnLoadAssetBundleAsync(abname));
                    
                    string[] dependencies = GetDependencies(abname);

                    // Debug.Log("你妹啊2~ " + abname + " - " + uri);
                    if (dependencies != null)
                    {
                        // Record and load all dependencies.
                        for (int i = 0; i < dependencies.Length; i++)
                        {
                            LoadAssetBundle(dependencies[i], false);
                        }
                    }
                    
                    if (_waitingDepends.ContainsKey(abname) == true || _loadingAssets.ContainsKey(abname) == true)
                    {
                        CheckWaitingDepends();
                    }
                }
            }
            else
            {
                bundles.TryGetValue(abname, out bundle);
                // Debug.Log("LoadAssetBundle 资源已经存在" + abname + " " + async + " " + bundle);
                if (callBack != null)
                {
                    callBack();
                }
            }

            return bundle;
        }

        /// <summary>
        /// 获取AB包的加载中信息
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public AssetBundleCreateRequest GetLoadingRequestInfo(string abname)
        {
            if (_loadingAssets.ContainsKey(abname) == false)
            {
                return null;
            }

            return _loadingAssets[abname];
        }

        /// <summary>
        /// 获取依赖包的加载中信息
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public string[] GetLoadingDependsList(string abname)
        {
            
            if (_depends.ContainsKey(abname) == false)
            {
                return null;
            }

            return _depends[abname];
        }

        IEnumerator UnLoadAssetBundleAsync(string abname)
        {
            if (!abname.EndsWith(AppConst.ExtName))
            {
                abname += AppConst.ExtName;
            }
            unloadingBundles[abname] = bundles[abname];
            bundles.Remove(abname);
            yield return new WaitForEndOfFrame();
            unloadingBundles[abname].UnloadAsync(false);
            unloadingBundles.Remove(abname);
            yield return null;
        }

        private Dictionary<string, AssetBundleCreateRequest> _loadingAssets = new Dictionary<string, AssetBundleCreateRequest>();
        private Dictionary<string, AssetBundleCreateRequest> _waitingDepends = new Dictionary<string, AssetBundleCreateRequest>();
        private Dictionary<string, string[]> _depends = new Dictionary<string, string[]>();

        IEnumerator LoadAssetBundleAsync(string abname)
        {
            string uri = Util.DataPath + abname;
            AssetBundleCreateRequest abRequest = AssetBundle.LoadFromFileAsync(uri); //关联数据的素材绑定
            
            _loadingAssets[abname] = abRequest;
            
            string[] dependencies = GetDependencies(abname);

            if (dependencies != null)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    LoadAssetBundle(dependencies[i], true);
                } 
            }
            
            yield return abRequest;
            _loadingAssets.Remove(abname);
            
            
            bool dependsOK = true;
            if (dependencies != null)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (bundles.ContainsKey(dependencies[i]) == false)
                    {
                        dependsOK = false;
                        break;
                    }
                }
            }

            if (dependsOK == true)
            {
                // print("LoadAssetBundleAsync 依赖已经加载好了" + abname);
                if (bundles.ContainsKey(abname) == false)
                {
                    bundles[abname] = abRequest.assetBundle;
                }
                if (ABCallBacks.ContainsKey(abname) == true)
                {
                    for (int i = 0; i < ABCallBacks[abname].Count; i++)
                    {
                        // T obj = abRequest.assetBundle.LoadAsset<T>(assetName);
                        ABCallBacks[abname][i]();
                    }

                    ABCallBacks.Remove(abname);
                }
            }
            else
            {
                // print("LoadAssetBundleAsync 还要等待依赖资源的加载" + abname + "  " + dependencies.Length);
                _waitingDepends[abname] = abRequest;
                _depends[abname] = dependencies;
            }
            
            CheckWaitingDepends();
            yield return null;

        }

        /*
         * 同步进来直接完成正在加载的异步
         */
        private void CompleteAsyncLoading(string abname)
        {
            if(_waitingDepends.ContainsKey(abname) == true)
            {
                bundles[abname] = _waitingDepends[abname].assetBundle;
                _waitingDepends.Remove(abname);
            }
            else
            {
                bundles[abname] = _loadingAssets[abname].assetBundle;
                _loadingAssets.Remove(abname);
            }

            if (_depends.ContainsKey(abname) == true)
            {
                string[] dependencies = _depends[abname];
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (bundles.ContainsKey(dependencies[i]) == false )
                    {
                        CompleteAsyncLoading(dependencies[i]);
                    }
                }
            }
            
            if (ABCallBacks.ContainsKey(abname) == true)
            {
                for (int j = 0; j < ABCallBacks[abname].Count; j++)
                {
                    ABCallBacks[abname][j]();
                }

                ABCallBacks.Remove(abname);
            }
            Debug.Log("CompleteAsyncLoading - " + abname);
        }

        /*
         * 每次有异步加载完成 判断是否有父级完成
         */
        private void CheckWaitingDepends()
        {
            List<string> okList = new List<string>();
            foreach (KeyValuePair<string, AssetBundleCreateRequest> kvp in _waitingDepends)
            {
                string[] dependencies = _depends[kvp.Key];
                bool dependsOK = true;
                for (int i = 0; i < dependencies.Length; i++)
                {
                    if (bundles.ContainsKey(dependencies[i]) == false && _waitingDepends.ContainsKey(dependencies[i]) == false)
                    {
                        dependsOK = false;
                        break;
                    }
                }

                if (dependsOK == true)
                {
                    okList.Add(kvp.Key);
                    bundles[kvp.Key] = kvp.Value.assetBundle;
                }
            }

            for (int i = 0; i < okList.Count; i++)
            {
                string abname = okList[i];
                _waitingDepends.Remove(abname);
                _depends.Remove(abname);
                
                if (ABCallBacks.ContainsKey(abname) == true)
                {
                    for (int j = 0; j < ABCallBacks[abname].Count; j++)
                    {
                        ABCallBacks[abname][j]();
                    }

                    ABCallBacks.Remove(abname);
                }
            }
        }

        /// <summary>
        /// 载入依赖
        /// </summary>
        /// <param name="name"></param>
        string[] GetDependencies(string name)
        {
            if (manifest == null)
            {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return null;
            }

            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = manifest.GetAllDependencies(name);
            // Debug.Log("你妹啊~ " + name);
            if (dependencies.Length == 0) return null;

            for (int i = 0; i < dependencies.Length; i++)
            {
                dependencies[i] = RemapVariantName(dependencies[i]);
            }


            return dependencies;
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        string RemapVariantName(string assetBundleName)
        {
            string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

            // If the asset bundle doesn't have variant, simply return.
            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
                return assetBundleName;

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] != split[0])
                    continue;

                int found = System.Array.IndexOf(m_Variants, curSplit[1]);
                if (found != -1 && found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if (bestFitIndex != -1)
                return bundlesWithVariant[bestFitIndex];
            else
                return assetBundleName;
        }

        /// <summary>
        /// 卸载所有bundle
        /// </summary>
        public void UnloadAllBundles()
        {
            if (bundles != null)
            {
                foreach (KeyValuePair<string, AssetBundle> kvp in bundles)
                {
                    if (kvp.Value)
                    {
                        kvp.Value.Unload(true);
                    }
                }
            }
        }

        /// <summary>
        /// 销毁资源
        /// </summary>
        void OnDestroy()
        {
            if (shared != null) shared.Unload(true);
            if (manifest != null) manifest = null;
            Debug.Log("~ResourceManager was destroy!");
        }
    }
}