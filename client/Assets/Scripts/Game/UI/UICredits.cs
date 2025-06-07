using GameFramework.Core;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏制作人员UI
/// </summary>
public class UICredits : UIBase
{
    // UI元素
    [SerializeField] private Button _closeButton;
    
    public override void OnInit(object userData)
    {
        base.OnInit(userData);
        
        // 初始化UI元素
        InitUI();
    }
    
    /// <summary>
    /// 初始化UI元素
    /// </summary>
    private void InitUI()
    {
        // 设置按钮点击事件
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
        }
    }
    
    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    private void OnCloseButtonClick()
    {
        // 关闭界面
        Close();
    }
    
    public override void OnClose()
    {
        // 移除按钮点击事件
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }
        
        base.OnClose();
    }
} 