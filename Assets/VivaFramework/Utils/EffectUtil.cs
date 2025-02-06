using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using VivaFramework.Components;

namespace VivaFramework.Utils
{
    public class EffectUtil
    {
        private static Dictionary<GameObject, List<Action<PointerEventData>>> _btnInfos =
            new Dictionary<GameObject, List<Action<PointerEventData>>>();
        public static void BtnClickDownUP(GameObject go, Action UpCall = null, Action DownCall = null, bool noEff = false)
        {
            EffectUtil.ClearBtnClick(go);
            Vector3 initScale = go.transform.localScale;
            bool isDown = false;
            PointerHandler component = go.GetComponent<PointerHandler>();
            if(component == null)component = go.AddComponent<PointerHandler>();
            Action<PointerEventData> downHandler = component.AddCall(PointerHandler.DOWN, (data) =>
            {
                if(noEff != true)go.transform.DOScale(initScale * 1.1f, 0.05f);
                isDown = true;
                DownCall?.Invoke();
            });
            Action<PointerEventData> clickHandler = component.AddCall(PointerHandler.CLICK, (data) =>
            {
                if(noEff != true)go.transform.DOScale(initScale, 0.05f);
                isDown = false;
                UpCall?.Invoke();
            });
            Action<PointerEventData> exitHandler = component.AddCall(PointerHandler.EXIT,(data) =>
            {
                if(noEff != true && isDown == true)go.transform.DOScale(initScale, 0.05f);
            });
            component.AddCall(PointerHandler.UP, exitHandler);
            Action<PointerEventData> destroyHandler = component.AddCall(PointerHandler.DESTROY, (data) =>
            {
                EffectUtil.ClearBtnClick(go);
            });
            List<Action<PointerEventData>> list = new List<Action<PointerEventData>>();
            list.Add(downHandler);
            list.Add(clickHandler);
            list.Add(exitHandler);
            list.Add(destroyHandler);
            _btnInfos[go] = list;
        }

        public static void ClearBtnClick(GameObject go)
        {
            if(_btnInfos.ContainsKey(go) == false)return;
            List<Action<PointerEventData>> list = _btnInfos[go];
            PointerHandler component = go.GetComponent<PointerHandler>();
            if (component == null) return;
            component.RemoveCall(PointerHandler.DOWN, list[1]);
            component.RemoveCall(PointerHandler.CLICK, list[2]);
            component.RemoveCall(PointerHandler.EXIT, list[3]);
            component.RemoveCall(PointerHandler.UP, list[3]);
            component.RemoveCall(PointerHandler.DESTROY, list[4]);
        }
    }
}