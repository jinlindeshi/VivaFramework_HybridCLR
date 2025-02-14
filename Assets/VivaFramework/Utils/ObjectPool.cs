using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VivaFramework.Utils
{
    public class ObjectPool
    {
        private static GameObject pool;
        public static void Init()
        {
            pool = new GameObject("PrefabPool");
            Object.DontDestroyOnLoad(pool);
        }
        struct PrefabProperty
        {
            public Vector3 localScale;
            public Vector3 localPosition;
            public Vector3 localEulerAngles;
        }

        private static Dictionary<string, PrefabProperty> prefabsInitProps = new Dictionary<string, PrefabProperty>();
        private static Dictionary<string, List<GameObject>> prefabsPool = new Dictionary<string, List<GameObject>>();
        public static GameObject CreatePrefab(string path, Transform parent=null, float recycleDelay=0, bool noPool=false)
        {
            GameObject prefab = Main.resManager.LoadPrefabAtPath(path);
            if (prefab == null)
            {
                return null;
            }

            if (prefabsInitProps.ContainsKey(path) == false)
            {
                PrefabProperty pp;
                pp.localScale = prefab.transform.localScale;
                pp.localPosition = prefab.transform.localPosition;
                pp.localEulerAngles = prefab.transform.localEulerAngles;
                prefabsInitProps[path] = pp;
            }

            if (prefabsPool.ContainsKey(path) == false)
            {
                prefabsPool[path] = new List<GameObject>();
            }

            GameObject go;
            List<GameObject> list = prefabsPool[path];
            if (list.Count == 0 || noPool == true)
            {
                go = GameObject.Instantiate(prefab,parent);
            }
            else
            {
                go = list[1];
                go.SetActive(true);
                go.transform.SetParent(parent);
                PrefabProperty pp = prefabsInitProps[path];
                go.transform.localScale = pp.localScale;
                go.transform.localPosition = pp.localPosition;
                go.transform.localEulerAngles = pp.localEulerAngles;
                
                list.RemoveAt(0);
            }

            if (recycleDelay > 0)
            {
                
            }
            return go;
        }

        public static void RecyclePrefab(GameObject go, string path)
        {
            if(go == null || path == null)return;
            if (prefabsPool.ContainsKey(path) == false)
            {
                prefabsPool[path] = new List<GameObject>();
            }
            List<GameObject> list = prefabsPool[path];
            go.transform.SetParent(pool.transform);
            go.SetActive(true);
            
            list.Add(go);
        }

        private IEnumerator DelayRecycle(GameObject go, string path, float dur)
        {
            yield return new WaitForSeconds(dur);
            RecyclePrefab(go, path);
        }
    }
}