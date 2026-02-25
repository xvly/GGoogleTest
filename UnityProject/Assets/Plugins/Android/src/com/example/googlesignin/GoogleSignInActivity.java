package com.example.googlesignin;

import android.content.Intent;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.common.api.ApiException;

// 注意：使用此类需要在 AndroidManifest.xml 中将启动 Activity 替换为
// com.example.googlesignin.GoogleSignInActivity（或将其作为主 Activity 的子类）
public class GoogleSignInActivity extends UnityPlayerActivity {
    private static final int GOOGLE_SIGN_IN_REQUEST = 9001; // 与 Unity 端保持一致
    private static final String UNITY_GAMEOBJECT = "TestLogin"; // Unity 中挂载脚本的 GameObject 名称（请确认）

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == GOOGLE_SIGN_IN_REQUEST) {
            try {
                Task<GoogleSignInAccount> task = GoogleSignIn.getSignedInAccountFromIntent(data);
                GoogleSignInAccount account = task.getResult(ApiException.class);
                if (account != null) {
                    String idToken = account.getIdToken();
                    if (idToken == null) idToken = "";
                    UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", idToken);
                } else {
                    UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
                }
            } catch (ApiException e) {
                UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
            } catch (Exception e) {
                UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
            }
        }
        super.onActivityResult(requestCode, resultCode, data);
    }
}
