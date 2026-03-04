package com.xgame.googlesignin;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.common.api.ApiException;

// 注意：使用此类需要在 AndroidManifest.xml 中将启动 Activity 替换为
// com.xgame.googlesignin.GoogleSignInActivity（或将其作为主 Activity 的子类）
public class GoogleSignInActivity extends UnityPlayerActivity {
    private static final int GOOGLE_SIGN_IN_REQUEST = 9001; // 与 Unity 端保持一致
    private static final String UNITY_GAMEOBJECT = "TestLogin"; // Unity 中挂载脚本的 GameObject 名称（请确认）

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        Log.d("GoogleSignInActivity", "[google] onActivityResult: requestCode=" + requestCode + ", resultCode=" + resultCode);   
        if (requestCode == GOOGLE_SIGN_IN_REQUEST) {
            try {
                Log.d("GoogleSignInActivity", "[google] onActivityResult 1");
                Task<GoogleSignInAccount> task = GoogleSignIn.getSignedInAccountFromIntent(data);
                Log.d("GoogleSignInActivity", "[google] onActivityResult: task=" + task.toString());    
                GoogleSignInAccount account = task.getResult(ApiException.class);
                Log.d("GoogleSignInActivity", "[google] onActivityResult: account=" + account);
                if (account != null) {
                    String idToken = account.getIdToken();
                    Log.d("GoogleSignInActivity", "[google] onActivityResult: account=" + account.getEmail() + ", idToken=" + idToken); 
                    if (idToken == null) idToken = "";
                    UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", idToken);
                    Log.d("GoogleSignInActivity", "[google] onActivityResult: idToken=" + idToken);
                } else {
                    UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
                    Log.e("GoogleSignInActivity", "[google] onActivityResult: account is null");
                }
            } catch (ApiException e) {
                UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
                Log.e("GoogleSignInActivity", "[google] onActivityResult: ApiException=" + e.toString());
            } catch (Exception e) {
                UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
                Log.e("GoogleSignInActivity", "[google] onActivityResult: error=" + e.toString());

            }
        }
        super.onActivityResult(requestCode, resultCode, data);
    }
}
