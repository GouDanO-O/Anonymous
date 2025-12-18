using GDFramework.Models;
using GDFramework.Utility;
using GDFrameworkCore;
using GDFrameworkExtend.Data;
using GDFrameworkExtend.LogKit;
using GDFrameworkExtend.PoolKit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GDFramework.Input
{
    #region Mouse

    public struct SInputEvent_MouseDrag
    {
        public Vector2 mousePos;
    }

    public struct SInputEvent_MouseLeftClick
    {
        
    }

    public struct SInputEvent_MouseRightClick
    {
        
    }

    public struct SInputEvent_MouseMiddleDown
    {
        
    }

    public struct SInputEvent_MouseMiddleUp
    {
        
    }

    public struct SInputEvent_MouseMiddleScroll
    {
        public Vector2 scrollValue;
    }

    #endregion

    /// <summary>
    /// 新输入系统
    /// </summary>
    public class NewInputManager : AbstractSystem
    {
        private InputActionAsset _actionAsset;

        #region Mouse

        private InputActionMap _playerMouseMap;

        private InputAction _mouseDrag;

        private InputAction _mouseLeftClick;

        private InputAction _mouseRightClick;

        private InputAction _mouseMiddleClick;

        private InputAction _mouseMiddleScroll;

        #endregion

        protected override void OnInit()
        {
        }

        protected override void OnDeinit()
        {
            UnregisterInputCallbacks();
            if (_actionAsset != null) 
                _actionAsset.Disable();
        }

        /// <summary>
        /// 初始化输入
        /// </summary>
        public void InitActionAsset()
        {
            _actionAsset = this.GetModel<GameDataModel>().InputActionAsset;

            _playerMouseMap = _actionAsset.FindActionMap("PlayerMouseMap");
            _mouseDrag = _actionAsset.FindAction("MouseDrag");
            _mouseLeftClick = _actionAsset.FindAction("MouseLeftClick");
            _mouseRightClick = _actionAsset.FindAction("MouseRightClick");
            
            _mouseMiddleClick = _actionAsset.FindAction("MouseMiddleClick");
            _mouseMiddleScroll = _actionAsset.FindAction("MouseMiddleScroll");
            
            CheckMouseMap(true);
            RegisterInputCallbacks();
        }

        public void CheckMouseMap(bool enable)
        {
            if (enable)
            {
                _playerMouseMap.Enable();
            }
            else
            {
                _playerMouseMap.Disable();
            }
        }

        /// <summary>
        /// 注册输入回调
        /// </summary>
        private void RegisterInputCallbacks()
        {
            if (_playerMouseMap != null)
            {
                _mouseDrag.performed += HandleMouseDrag;
                _mouseDrag.canceled += HandleMouseDragCancel;
            }

            if (_mouseLeftClick != null)
            {
                _mouseLeftClick.performed += HandleMouseLeftClick;
            }
            
            if (_mouseRightClick != null)
            {
                _mouseRightClick.performed += HandleMouseRightClick;
            }
            
            if (_mouseMiddleClick != null)
            {
                _mouseMiddleClick.performed += HandleMouseMiddleDown;
                _mouseMiddleClick.canceled += HandleMouseMiddleUp;
            }
            
            if (_mouseMiddleScroll != null)
            {
                _mouseMiddleScroll.performed += HandleMouseMiddleScroll;
            }
        }

        /// <summary>
        /// 注销输入回调
        /// </summary>
        private void UnregisterInputCallbacks()
        {
            if (_mouseDrag != null)
            {
                _mouseDrag.performed -= HandleMouseDrag;
                _mouseDrag.canceled -= HandleMouseDragCancel;
            }

            if (_mouseLeftClick != null) 
                _mouseLeftClick.performed -= HandleMouseLeftClick;

            if (_mouseRightClick != null) 
                _mouseRightClick.performed -= HandleMouseRightClick;
        }

        #region Mouse

        /// <summary>
        /// 是否显示鼠标在游戏内
        /// </summary>
        public void WillShowMouse()
        {
        }

        /// <summary>
        /// 是否限制鼠标在游戏内的移动(不让其超出操作界面)
        /// </summary>
        public void WillImposeMouse()
        {
        }

        /// <summary>
        /// 处理输入--鼠标移动（更新当前鼠标位置）
        /// </summary>
        /// <param name="context"></param>
        private void HandleMouseDrag(InputAction.CallbackContext context)
        {
            this.SendEvent(new SInputEvent_MouseDrag(){mousePos = context.ReadValue<Vector2>()});
        }

        /// <summary>
        /// 清空鼠标输入（鼠标停止时）
        /// </summary>
        /// <param name="context"></param>
        private void HandleMouseDragCancel(InputAction.CallbackContext context)
        {
            this.SendEvent(new SInputEvent_MouseDrag(){mousePos = Vector2.zero});
        }
        
        /// <summary>
        /// 处理输入--鼠标左键
        /// </summary>
        private void HandleMouseLeftClick(InputAction.CallbackContext context)
        {
            this.SendEvent<SInputEvent_MouseLeftClick>();
        }

        /// <summary>
        /// 处理输入--鼠标右键
        /// </summary>
        private void HandleMouseRightClick(InputAction.CallbackContext context)
        {
            this.SendEvent<SInputEvent_MouseRightClick>();
        }

        /// <summary>
        /// 处理输入--鼠标中键按住不放
        /// </summary>
        /// <param name="context"></param>
        private void HandleMouseMiddleDown(InputAction.CallbackContext context)
        {
            LogKit.Log("按住鼠标中键");
            this.SendEvent<SInputEvent_MouseMiddleDown>();
        }
        
        /// <summary>
        /// 处理输入--鼠标中键松开
        /// </summary>
        /// <param name="context"></param>
        private void HandleMouseMiddleUp(InputAction.CallbackContext context)
        {
            LogKit.Log("松开鼠标中键");
            this.SendEvent<SInputEvent_MouseMiddleUp>();
        }

        /// <summary>
        /// 处理输入--鼠标滚轮
        /// </summary>
        /// <param name="context"></param>
        private void HandleMouseMiddleScroll(InputAction.CallbackContext context)
        {
            this.SendEvent<SInputEvent_MouseMiddleScroll>(new SInputEvent_MouseMiddleScroll()
            {
                scrollValue = context.ReadValue<Vector2>()
            });
        }
        #endregion

        /// <summary>
        /// 改变输入键值
        /// </summary>
        public void ChangeInputKey()
        {
        }
    }
}