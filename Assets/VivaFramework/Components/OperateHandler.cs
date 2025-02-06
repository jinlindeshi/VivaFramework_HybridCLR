using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace VivaFramework.Components
{
    public class OperateHandler : MonoBehaviour
    {
        protected Dictionary<string, List<Action<PointerEventData>>> funDic = new Dictionary<string, List<Action<PointerEventData>>>();

        public Action<PointerEventData> AddCall(string type, Action<PointerEventData> call)
        {
            if(funDic.ContainsKey(type) == false)
            {
                funDic[type] = new List<Action<PointerEventData>>();
            }
            funDic[type].Add(call);

            return call;
        }

        public void RemoveCall(string type, Action<PointerEventData> call)
        {
            var removeIndex = -1;
            if (funDic.ContainsKey(type) == false) return;
            List<Action<PointerEventData>> list = funDic[type];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == call)
                {
                    removeIndex = i;
                    break;
                }
            }
            if (removeIndex >= 0)
            {
                funDic[type].RemoveAt(removeIndex);
            }
        }

        protected void TakeCall(string type, PointerEventData eventData)
        {
            if (funDic.ContainsKey(type) == false || funDic[type] == null)
            {
                return;
            }

            List<Action<PointerEventData>> list = new List<Action<PointerEventData>>();
            for (int i = 0; i < funDic[type].Count; i++)
            {
                list.Add(funDic[type][i]);
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i](eventData);
            }
        }

        protected void TakeCall(string type)
        {
            TakeCall(type, null);
        }
    }
}