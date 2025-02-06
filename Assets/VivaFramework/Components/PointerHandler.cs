using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace VivaFramework.Components
{
    public class PointerHandler : OperateHandler, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        
		public const string ENTER = "PointerEnter";
		public const string EXIT = "PointerExit";
		public const string DOWN = "PointerDown";
		public const string UP = "PointerUp";
		public const string CLICK = "PointerClick";
        public const string DOUBLE_CLICK = "PointerDoubleClick";

        
        
		public void OnPointerEnter (PointerEventData eventData)
		{
            TakeCall(ENTER, eventData);
            
        }

		public void OnPointerExit (PointerEventData eventData)
        {
            TakeCall(EXIT, eventData);
        }

		public void OnPointerDown (PointerEventData eventData)
        {
            TakeCall(DOWN, eventData);
            // print("木哈哈哈 - OnPointerDown");
        }

		public void OnPointerUp (PointerEventData eventData)
        {
            TakeCall(UP, eventData);
        }

		public void OnPointerClick (PointerEventData eventData)
        {
            TakeCall(CLICK, eventData);
            CheckDoubleClick(eventData);
        }

        private float _lastClickTime = 0;
        //验证双击
        private void CheckDoubleClick(PointerEventData eventData)
        {
//            Debug.Log("CheckDoubleClick " + _lastClickTime + " ^ " + Time.time);
            if (_lastClickTime != 0 && Time.time - _lastClickTime < 1)
            {
                _lastClickTime = 0;
                TakeCall(DOUBLE_CLICK, eventData);
//                Debug.Log("CheckDoubleClick1");
            }
            else
            {
                _lastClickTime = Time.time;
//                Debug.Log("CheckDoubleClick2");
            }
        }

        void OnDestroy()
        {
            funDic = null;
        }

    }
}
