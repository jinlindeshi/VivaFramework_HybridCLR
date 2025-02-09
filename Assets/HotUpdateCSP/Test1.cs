using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VivaFramework;
using VivaFramework.Utils;

public class Test1
{
    // Start is called before the first frame update
    public static void Start()
    {
        GameObject p1 = Main.resManager.LoadPrefabAtPath("Prefabs/UICanvas.prefab");
        GameObject p2 = Main.resManager.LoadPrefabAtPath("Prefabs/Bg.prefab");
        GameObject uiCanvas = GameObject.Instantiate(p1);
        GameObject bg = GameObject.Instantiate(p2, uiCanvas.transform);
        
        EffectUtil.BtnClickDownUP(bg.transform.Find("Button").gameObject, () =>
            {
                Debug.Log("UP");
                Main.sceneManager.LoadSceneAsync("Battle", ChangeScene, LoadSceneMode.Single);
            }
            ,() => { Debug.Log("DOWN");});
    }

    private static void ChangeScene(Scene s)
    {
        Debug.Log("你妹啊~");
        GameObject p = Main.resManager.LoadPrefabAtPath("Prefabs/Battle/TestAvatar.prefab");
        GameObject.Instantiate(p);
    }
}
