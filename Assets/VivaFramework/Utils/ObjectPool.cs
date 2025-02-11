using System.Collections.Generic;
using UnityEngine;

namespace VivaFramework.Utils
{
    public class ObjectPool
    {
        struct PrefabProperty
        {
            public Vector3 localScale;
            public Vector3 localPosition;
            public Vector3 localEulerAngles;
        }

        private static Dictionary<string, PrefabProperty> prefabsInitProps = new Dictionary<string, PrefabProperty>();
        public static GameObject InstanciatePrefab(string path, Transform parent, float recycleDelay, bool noPool)
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
            }
            return null;
        }
    }
}