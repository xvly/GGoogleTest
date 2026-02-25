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
    public Text tokenText;
    public Button googleLoginButton;
    public Button logoutButton;
    public Button refreshTokenButton;
    // 在 Inspector 中填入在 Google Cloud / Firebase 控制台得到的 Web client ID
    public string webClientId = "1:376231049174:android:4be9be362cb5c78e27d677";

    // 请求码，用于 startActivityForResult
    private const int GOOGLE_SIGN_IN_REQUEST = 9001;

    void Start()
    {
        Debug.Log("开始初始化Firebase...");
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
        
        if (refreshTokenButton != null)
            refreshTokenButton.onClick.AddListener(RefreshFirebaseToken);
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
                Debug.Log("未获取到Google账户，尝试启动Google登录界面");
                if (statusText != null)
                    statusText.text = "未获取到Google账户，启动登录界面...";
                StartGoogleSignInIntentAndroid(activity);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("获取Google Token失败: " + ex.Message);
            if (statusText != null)
                statusText.text = "登录失败: " + ex.Message;
        }
    }

    // 启动 Google Sign-In 的原生 Intent（会弹出 Google 登录界面），需要在 Android 原生层把结果通过
    // UnityPlayer.UnitySendMessage(...) 回传给 Unity（见 OnGoogleSignInResult）
    private void StartGoogleSignInIntentAndroid(AndroidJavaObject activity)
    {
        try
        {
            if (string.IsNullOrEmpty(webClientId))
            {
                Debug.LogError("请在 Inspector 中填入 webClientId（Web 客户端 ID）");
                if (statusText != null)
                    statusText.text = "缺少 webClientId，请配置后重试";
                return;
            }

            AndroidJavaObject defaultSignInObj = null;
            AndroidJavaObject gsoBuilder = null;
            try
            {
                var gsoClass = new AndroidJavaClass("com.google.android.gms.auth.api.signin.GoogleSignInOptions");
                // DEFAULT_SIGN_IN is a GoogleSignInOptions object in some Play Services versions
                try
                {
                    defaultSignInObj = gsoClass.GetStatic<AndroidJavaObject>("DEFAULT_SIGN_IN");
                }
                catch (System.Exception)
                {
                    Debug.LogWarning("无法通过反射获取 DEFAULT_SIGN_IN 对象，尝试使用无参 Builder 构造。");
                }

                if (defaultSignInObj != null)
                {
                    gsoBuilder = new AndroidJavaObject("com.google.android.gms.auth.api.signin.GoogleSignInOptions$Builder", defaultSignInObj);
                }
                else
                {
                    // 有些版本可能没有 DEFAULT_SIGN_IN 对象或对应构造，尝试无参构造再配置
                    gsoBuilder = new AndroidJavaObject("com.google.android.gms.auth.api.signin.GoogleSignInOptions$Builder");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("构造 GoogleSignInOptions.Builder 失败: " + ex.Message);
                if (statusText != null)
                    statusText.text = "启动登录失败: " + ex.Message;
                return;
            }
            gsoBuilder = gsoBuilder.Call<AndroidJavaObject>("requestIdToken", webClientId);
            gsoBuilder = gsoBuilder.Call<AndroidJavaObject>("requestEmail");
            var gso = gsoBuilder.Call<AndroidJavaObject>("build");

            var googleSignInClass = new AndroidJavaClass("com.google.android.gms.auth.api.signin.GoogleSignIn");
            var googleSignInClient = googleSignInClass.CallStatic<AndroidJavaObject>("getClient", activity, gso);
            var signInIntent = googleSignInClient.Call<AndroidJavaObject>("getSignInIntent");
            activity.Call("startActivityForResult", signInIntent, GOOGLE_SIGN_IN_REQUEST);

            Debug.Log("已启动 Google Sign-In 界面，等待用户操作...");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("启动 Google Sign-In 失败: " + ex.Message);
            if (statusText != null)
                statusText.text = "启动登录失败: " + ex.Message;
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
            
            // 登录成功后获取 Firebase 用户的 ID Token（如需强制刷新请传 true）
            GetFirebaseIdToken(false);

            UpdateUI();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("登录失败: " + ex.Message);
            if (statusText != null)
                statusText.text = "登录失败: " + ex.Message;
        }
    }

    // 调用 FirebaseUser.TokenAsync 获取 Firebase ID token（对应 Android 的 getIdToken(boolean)）
    // forceRefresh: true 表示强制从服务器刷新 token，false 表示使用本地缓存（若有效）
    public async void GetFirebaseIdToken(bool forceRefresh)
    {
        try
        {
            if (auth == null || auth.CurrentUser == null)
            {
                Debug.LogWarning("未登录或 Firebase 未初始化，无法获取 Firebase ID token");
                if (statusText != null)
                    statusText.text = "未登录，无法获取 Firebase token";
                return;
            }

            var current = auth.CurrentUser;
            // TokenAsync 对应底层的 getIdToken(boolean)
            var token = await current.TokenAsync(forceRefresh);
            Debug.Log("获取到 Firebase ID token（长度）: " + (token != null ? token.Length.ToString() : "null"));
            if (statusText != null)
                statusText.text = "获取到 Firebase ID token";

            // 如果需要把 token 传给原生或其他模块，可在这里处理
            webClientId = token; // 仅示例，实际使用中请妥善管理 token  
            if (tokenText != null)
                tokenText.text = "Firebase ID Token: " + token.Substring(0, Math.Min(20, token.Length)) + "...";
        }
        catch (System.Exception ex)
        {
            Debug.LogError("获取 Firebase ID token 失败: " + ex.Message);
            if (statusText != null)
                statusText.text = "获取 token 失败: " + ex.Message;
        }
    }

    // 供 Android 原生层在 onActivityResult 成功解析到 idToken 后调用：
    // UnityPlayer.UnitySendMessage("<GameObjectName>", "OnGoogleSignInResult", idToken);
    public void OnGoogleSignInResult(string idToken)
    {
        if (!string.IsNullOrEmpty(idToken))
        {
            Debug.Log("收到 idToken，继续用 Firebase 登录");
            SignInWithCredential(idToken);
        }
        else
        {
            Debug.LogError("Google 登录未返回 idToken");
            if (statusText != null)
                statusText.text = "Google 登录未返回 idToken";
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
            if (refreshTokenButton != null)
                refreshTokenButton.interactable = true;
            if (statusText != null)
                statusText.text = "已登录: " + user.Email;
        }
        else
        {
            if (googleLoginButton != null)
                googleLoginButton.interactable = true;
            if (logoutButton != null)
                logoutButton.interactable = false;
            if (refreshTokenButton != null)
                refreshTokenButton.interactable = false;
            if (statusText != null)
                statusText.text = "未登录";
        }
    }

    // UI 按钮点击：强制刷新 Firebase ID token
    public void RefreshFirebaseToken()
    {
        Debug.Log("用户点击刷新 Token 按钮");
        if (statusText != null)
            statusText.text = "正在刷新 Firebase ID token...";
        GetFirebaseIdToken(true); // 传 true 强制从服务器刷新
    }

    void Update()
    {
        
    }
}
