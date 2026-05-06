using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 平台接口 - 定义平台通用功能
/// </summary>
public interface IPlatform
{
    PlatformUserInfo UserInfo { get; set; }
    
    /// <summary>
    /// 初始化平台
    /// </summary>
    void Init();

    /// <summary>
    /// 更新平台
    /// </summary>
    void Update();

    /// <summary>
    /// 释放平台
    /// </summary>
    void Release();

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    /// <param name="getProviderLogin">快联盟登录参数，其他平台忽略该参数</param>
    void Login(Action<string> success, Action<string> fail, Action<ProviderLogin> getProviderLogin = null);

    /// <summary>
    /// 检查用户授权状态
    /// </summary>
    /// <param name="success">成功回调，返回授权设置</param>
    /// <param name="fail">失败回调</param>
    void CheckUserAuthorization(Action<bool> success, Action<string> fail);

    /// <summary>
    /// 获取用户信息（需要先检查授权）
    /// </summary>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    //void GetUserInfo(Action<UserInfoResult> success, Action<string> fail);

    /// <summary>
    /// 创建用户信息授权按钮（透明区域）
    /// </summary>
    /// <param name="x">左上角X坐标</param>
    /// <param name="y">左上角Y坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="onTap">点击回调</param>
    /// <returns>按钮控制器</returns>
    IUserInfoButton CreateUserInfoButton(int x, int y, int width, int height, Action<UserInfoResult> onTap);

    /// <summary>
    /// 创建游戏圈按钮
    /// </summary>
    /// <param name="left">左上角横坐标</param>
    /// <param name="top">左上角纵坐标</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    void CreatGameClubButton(int left, int top, int width, int height);
    
    /// <summary>
    /// 分享
    /// </summary>
    /// <param name="shareInfo">分享信息</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void Share(ShareInfo shareInfo, Action success, Action<string> fail);

    /// <summary>
    /// 显示广告
    /// </summary>
    /// <param name="adType">广告类型</param>
    /// <param name="adId">广告ID</param>
    /// <param name="success">成功回调</param>
    /// <param name="close">关闭广告回调</param>
    /// <param name="fail">失败回调</param>
    void ShowAd(AdType adType, string adId, Action success, Action<AdResult> close, Action<string> fail);

    /// <summary>
    /// 显示激励广告（带服务端验证）
    /// </summary>
    /// <param name="adId">广告ID</param>
    /// <param name="userId">用户ID，用于服务端验证</param>
    /// <param name="success">完整观看成功回调</param>
    /// <param name="close">关闭广告回调</param>
    /// <param name="fail">失败回调</param>
    void ShowRewardAd(string adId, string userId, Action success, Action<AdResult> close, Action<string> fail);

    /// <summary>
    /// 支付
    /// </summary>
    /// <param name="payInfo">支付信息</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void Pay(PayInfo payInfo, Action<PayResult> success, Action<string> fail);

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void GetUserInfo(Action<UserInfoResult> success, Action<string> fail);
    
    /// <summary>
    /// 获取用户手机号码
    /// </summary>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void GetPhoneNumber(Action<string> success, Action<string> fail);

    /// <summary>
    /// 获取系统信息
    /// </summary>
    /// <returns>系统信息</returns>
    SystemInfo GetSystemInfo();

    /// <summary>
    /// 振动
    /// </summary>
    /// <param name="isShort">是否短振动</param>
    void Vibrate(bool isShort);

    /// <summary>
    /// 跳转其他小游戏
    /// </summary>
    /// <param name="appId">小游戏ID</param>
    /// <param name="path">路径</param>
    /// <param name="extraData">额外数据</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void NavigateToMiniGame(string appId, string path, string extraData, Action success, Action<string> fail);

    /// <summary>
    /// 显示消息提示框
    /// </summary>
    /// <param name="title">提示的内容</param>
    /// <param name="icon">图标（success/error/loading/none）</param>
    /// <param name="duration">提示的延迟时间（毫秒），默认1500</param>
    /// <param name="mask">是否显示透明蒙层，防止触摸穿透</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void ShowToast(string title, string icon = "none", int duration = 1500, bool mask = false, Action success = null, Action<string> fail = null);

    /// <summary>
    /// 隐藏消息提示框
    /// </summary>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void HideToast(Action success = null, Action<string> fail = null);

    /// <summary>
    /// 显示模态对话框
    /// </summary>
    /// <param name="title">提示的标题</param>
    /// <param name="content">提示的内容</param>
    /// <param name="showCancel">是否显示取消按钮，默认true</param>
    /// <param name="confirmText">确认按钮的文字，默认"确定"</param>
    /// <param name="cancelText">取消按钮的文字，默认"取消"</param>
    /// <param name="success">成功回调（confirm: 是否点击确定, content: editable时用户输入内容）</param>
    /// <param name="fail">失败回调</param>
    void ShowModal(string title, string content, bool showCancel = true, string confirmText = "确定", string cancelText = "取消", Action<bool, string> success = null, Action<string> fail = null);

    /// <summary>
    /// 显示Loading提示框（需主动调用HideLoading关闭）
    /// </summary>
    /// <param name="title">提示的内容</param>
    /// <param name="mask">是否显示透明蒙层，防止触摸穿透</param>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void ShowLoading(string title, bool mask = false, Action success = null, Action<string> fail = null);

    /// <summary>
    /// 隐藏Loading提示框
    /// </summary>
    /// <param name="success">成功回调</param>
    /// <param name="fail">失败回调</param>
    void HideLoading(Action success = null, Action<string> fail = null);

    /// <summary>
    /// 显示操作菜单
    /// </summary>
    /// <param name="itemList">按钮的文字数组，数组长度最大为6</param>
    /// <param name="itemColor">按钮的文字颜色</param>
    /// <param name="alertText">警示文案</param>
    /// <param name="success">成功回调（tapIndex: 用户点击的按钮序号，从0开始）</param>
    /// <param name="fail">失败回调</param>
    void ShowActionSheet(string[] itemList, string itemColor = "#000000", string alertText = "", Action<int> success = null, Action<string> fail = null);

    /// <summary>
    /// 对用户托管数据进行写数据操作。允许同时写多组 KV 数据。
    /// </summary>
    /// <param name="data"></param>
    /// <param name="success"></param>
    /// <param name="fail"></param>
    void SetUserCloudStorage(UserStorageData[] data, Action success = null, Action<string> fail = null);
    

    // void GetFriendCloudStorage();
}

/// <summary>
/// 登录结果
/// </summary>
public class LoginResult
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Avatar { get; set; }
    public string Token { get; set; }
    public Dictionary<string, object> ExtraData { get; set; }
}


/// <summary>
/// 分享信息
/// </summary>
public class ShareInfo
{
    public string Title { get; set; }
    public string Desc { get; set; }
    public string ImageUrl { get; set; }
    public string Link { get; set; }
}

/// <summary>
/// 广告类型
/// </summary>
public enum AdType
{
    Banner, // 横幅广告
    Interstitial, // 插屏广告
    Reward, // 激励视频
    Native // 原生广告
}

/// <summary>
/// 广告结果
/// </summary>
public class AdResult
{
    public bool IsCompleted { get; set; }
    public string AdId { get; set; }
    public int AdSeq { get; set; }
    public string CustomData { get; set; }
}

/// <summary>
/// 支付信息
/// </summary>
public class PayInfo
{
    public string OrderId { get; set; }
    public string ProductId { get; set; }
    public float Amount { get; set; }
    public string Currency { get; set; }
    public string Desc { get; set; }
}

/// <summary>
/// 支付结果
/// </summary>
public class PayResult
{
    public string OrderId { get; set; }
    public bool IsSuccess { get; set; }
    public string Receipt { get; set; }
}

/// <summary>
/// 系统信息
/// </summary>
public class SystemInfo
{
    public string Platform { get; set; }
    public string Version { get; set; }
    public float ScreenWidth { get; set; }
    public float ScreenHeight { get; set; }
    public string Language { get; set; }
}

/// <summary>
/// 授权结果
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// 授权设置字典，key为权限名称，value为是否已授权
    /// </summary>
    public Dictionary<string, bool> AuthSetting { get; set; }

    /// <summary>
    /// 是否已授权用户信息
    /// </summary>
    public bool HasUserInfoAuth => AuthSetting != null &&
                                   AuthSetting.ContainsKey("scope.userInfo") &&
                                   AuthSetting["scope.userInfo"];
}

/// <summary>
/// 用户信息结果
/// </summary>
public class UserInfoResult
{
    public string RawData { get; set; }
    public int ErrCode { get; set; }
    public string ErrMsg { get; set; }
    public string NickName { get; set; }
    public string AvatarUrl { get; set; }
    public int Gender { get; set; }
    public string Country { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public string Language { get; set; }
    public string EncryptedData { get; set; }
    public string Iv { get; set; }
    public string Signature { get; set; }
    public string CloudID { get; set; }
    public bool IsSuccess => ErrCode == 0;

    public override string ToString()
    {
        return $"RawData = {RawData}, NickName = {NickName}, AvatarUrl = {AvatarUrl}, Gender = {Gender}, Province = {Province}, City = {City}," +
               $" Language = {Language}, EncryptedData = {EncryptedData}, Iv = {Iv}, Signature = {Signature}, CloudID = {CloudID}";
    }
}

/// <summary>
/// 平台用户信息
/// </summary>
public class PlatformUserInfo
{
    public string NickName { get; set; }
    public string AvatarUrl { get; set; }
    public int Gender { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public string Language { get; set; }
}

/// <summary>
/// 用户信息按钮接口
/// </summary>
public interface IUserInfoButton
{
    /// <summary>
    /// 显示按钮
    /// </summary>
    void Show();

    /// <summary>
    /// 隐藏按钮
    /// </summary>
    void Hide();

    /// <summary>
    /// 销毁按钮
    /// </summary>
    void Destroy();
}

/// <summary>
/// 用户托管数据
/// </summary>
public struct UserStorageData
{
    public string Key { get; set; }
    public string Value { get; set; }
}