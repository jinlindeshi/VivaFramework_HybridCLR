using UnityEngine;

namespace VivaFramework.Components
{
    public class PointerHandler3D : OperateHandler 
    {
        
        public const string ENTER = "PointerEnter";
        public const string EXIT = "PointerExit";
        public const string DOWN = "PointerDown";
        public const string UP = "PointerUp";
        public const string CLICK = "PointerClick";
        public const string DOUBLE_CLICK = "PointerDoubleClick";
        public const string ENABLE = "Enable";
        public const string DISABLE = "Disable";

        private bool _isEnter = false;
        
        private void OnDisable()
        {
            _isEnter = false;
            TakeCall(DISABLE);
        }

        private void OnEnable()
        {
            TakeCall(ENABLE);
        }
        private void OnMouseEnter()
        {
            if (enabled == false) return;
            _isEnter = true;
            TakeCall(ENTER);
        }
        private void OnMouseExit()
        {
            if (enabled == false) return;
            _isEnter = false;
            TakeCall(EXIT);
        }

        private void OnMouseUp()
        {
            if (enabled == false) return;
            // Debug.Log("OnMouseUp");
            TakeCall(UP);
            if (_isEnter == true)
            {
                TakeCall(CLICK);
                CheckDoubleClick();
            }
        }
        
        private float _lastClickTime = 0;
        //验证双击
        private void CheckDoubleClick()
        {
//            Debug.Log("CheckDoubleClick " + _lastClickTime + " ^ " + Time.time);
            if (_lastClickTime != 0 && Time.time - _lastClickTime < 1)
            {
                _lastClickTime = 0;
                TakeCall(DOUBLE_CLICK);
//                Debug.Log("CheckDoubleClick1");
            }
            else
            {
                _lastClickTime = Time.time;
//                Debug.Log("CheckDoubleClick2");
            }
        }
        
        private void OnMouseDown()
        {
            if (enabled == false) return;
            TakeCall(DOWN);
        }
        
    }
}