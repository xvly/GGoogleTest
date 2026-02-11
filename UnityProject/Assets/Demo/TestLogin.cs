using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;

public class TestLogin : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;
    
    // UI组件
    public Text statusText;
    public Button googleLoginButton;
    public Button logoutButton;

    void Start()
    {
        // 初始化Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                user = auth.CurrentUser;
                UpdateUI();
                Debug.Log("Firebase初始化成功");
            }
            else
            {
                Debug.LogError("无法解决Firebase依赖关系: " + dependencyStatus);
            }
        });

        // 绑定按钮事件
        if (googleLoginButton != null)
            googleLoginButton.onClick.AddListener(SignInWithGoogle);
        
        if (logoutButton != null)
            logoutButton.onClick.AddListener(SignOut);
    }

    // 谷歌登录
    public void SignInWithGoogle()
    {
        Debug.Log("开始谷歌登录...");
        
        if (statusText != null)
            statusText.text = "谷歌登录中...";

        // 创建谷歌登录凭证
        // 需要在Android或iOS平台上获取ID Token
        #if UNITY_ANDROID
            GetGoogleTokenAndroid();
        #elif UNITY_IOS
            GetGoogleTokenIOS();
        #else
            Debug.LogError("只支持Android和iOS平台");
        #endif
    }

    // Android平台获取谷歌Token
    private void GetGoogleTokenAndroid()
    {
        try
        {
            var googleSignInClient = new AndroidJavaClass("com.google.android.gms.auth.api.signin.GoogleSignIn");
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            
            var account = googleSignInClient.CallStatic<AndroidJavaObject>("getLastSignedInAccount", activity);
            
            if (account != null)
            {
                string idToken = account.Call<string>("getIdToken");
                SignInWithCredential(idToken);
            }
            else
            {
                Debug.LogError("未获取到Google账户");
                if (statusText != null)
                    statusText.text = "未获取到Google账户";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("获取Google Token失败: " + ex.Message);
            if (statusText != null)
                statusText.text = "登录失败: " + ex.Message;
        }
    }

    // iOS平台获取谷歌Token
    private void GetGoogleTokenIOS()
    {
        Debug.Log("iOS平台需要集成Google Sign-In SDK");
        if (statusText != null)
            statusText.text = "iOS平台谷歌登录功能需要额外配置";
    }

    // 使用凭证登录Firebase
    private async void SignInWithCredential(string idToken)
    {
        try
        {
            var credential = GoogleAuthProvider.GetCredential(idToken, null);
            var result = await auth.SignInWithCredentialAsync(credential);
            
            user = result;
            Debug.Log("谷歌登录成功! 用户ID: " + user.UserId);
            if (statusText != null)
                statusText.text = "登录成功! 欢迎 " + user.DisplayName;
            
            UpdateUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("登录失败: " + ex.Message);
            if (statusText != null)
                statusText.text = "登录失败: " + ex.Message;
        }
    }

    // 登出
    public void SignOut()
    {
        auth.SignOut();
        user = null;
        Debug.Log("已登出");
        
        if (statusText != null)
            statusText.text = "已登出";
        
        UpdateUI();
    }

    // 更新UI显示
    private void UpdateUI()
    {
        if (user != null)
        {
            if (googleLoginButton != null)
                googleLoginButton.interactable = false;
            if (logoutButton != null)
                logoutButton.interactable = true;
            if (statusText != null)
                statusText.text = "已登录: " + user.Email;
        }
        else
        {
            if (googleLoginButton != null)
                googleLoginButton.interactable = true;
            if (logoutButton != null)
                logoutButton.interactable = false;
            if (statusText != null)
                statusText.text = "未登录";
        }
    }

    void Update()
    {
        
    }
}
