using System;
using UnityEngine;
using VivaFramework.Components;

namespace VivaFramework.Utils
{
    public class EffectUtil
    {
        private struct BtnInfo
        {
            public GameObject go;
            public Action UpCall;
            public Action DownCall;
            public bool noEffect;
        }
        public static void BtnClickDownUP(GameObject go, Action UpCall, Action DownCall, bool noEffect = false)
        {
            EffectUtil.ClearBtnClick(go);
            BtnInfo info = new BtnInfo{go=go,UpCall=UpCall,DownCall=DownCall,noEffect=noEffect};
            if (go.GetComponent<PointerHandler>() == null)go.AddComponent<PointerHandler>();
            PointerHandler component = go.GetComponent<PointerHandler>();
            // component.AddCall(PointerHandler.DOWN, data => go.transform.)
        }

        public static void ClearBtnClick(GameObject go)
        {
            
        }
    }
}