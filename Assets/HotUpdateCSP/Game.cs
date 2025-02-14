using TMPro;
using UnityEngine;
using VivaFramework.Utils;

public class Game
{
    public static Canvas uiCanvas;
    public static void Start()
    {
        ObjectPool.Init();
        uiCanvas = ObjectPool.CreatePrefab("Prefabs/UICanvas.prefab").GetComponent<Canvas>();
        GameObject btn = ObjectPool.CreatePrefab("Prefabs/Bg.prefab", uiCanvas.transform).transform.Find("Button").gameObject;
        TextMeshProUGUI btnLb = btn.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        btnLb.text = "ohehehe";
        EffectUtil.BtnClickDownUP(btn, () => { Debug.Log("你妹啊~~~");});
    }
}