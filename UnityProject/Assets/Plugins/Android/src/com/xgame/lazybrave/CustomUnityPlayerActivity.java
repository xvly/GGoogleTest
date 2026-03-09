package com.xgame.lazybrave;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayerActivity;
import com.unity3d.player.UnityPlayer;

import com.google.android.gms.auth.api.signin.GoogleSignIn;
import com.google.android.gms.auth.api.signin.GoogleSignInAccount;
import com.google.android.gms.common.api.ApiException;

public class CustomUnityPlayerActivity extends UnityPlayerActivity {
    private static final int GOOGLE_SIGN_IN_REQUEST = 9001;
    private static final String UNITY_GAMEOBJECT = "GoogleSignInListener";
    private static final String TAG = "CustomUnityPlayerActivity";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == GOOGLE_SIGN_IN_REQUEST) {
            try {
                com.google.android.gms.tasks.Task<GoogleSignInAccount> task = GoogleSignIn.getSignedInAccountFromIntent(data);
                GoogleSignInAccount account = task.getResult(ApiException.class);
                String idToken = account != null ? account.getIdToken() : "";
                if (idToken == null) idToken = "";
                Log.d(TAG, "Google Sign-In success, sending token to Unity (len=" + idToken.length() + ")");
                UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", idToken);
            } catch (ApiException e) {
                Log.w(TAG, "Google sign in failed", e);
                UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
            } catch (Exception e) {
                Log.e(TAG, "Unexpected exception handling sign-in result", e);
                UnityPlayer.UnitySendMessage(UNITY_GAMEOBJECT, "OnGoogleSignInResult", "");
            }
        }
    }
}
