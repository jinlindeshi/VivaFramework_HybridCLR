using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivaFramework;

public class Test1
{
    // Start is called before the first frame update
    public static void Start()
    {
        Debug.Log("你妹啊~11 " + Main.resManager);
        GameObject p = Main.resManager.LoadPrefabAtPath("Prefabs/Sphere.prefab");
        Debug.Log("你妹啊~22 " + p);
        GameObject.Instantiate(p);
    }
}
