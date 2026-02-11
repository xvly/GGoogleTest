# 谷歌登录完整配置指南

## 第一步：Firebase项目配置

### 1.1 创建/配置Firebase项目
1. 访问 [Firebase控制台](https://console.firebase.google.com/)
2. 创建新项目或选择现有项目
3. 启用 **Authentication** 服务
4. 在 Authentication 中启用 **Google** 登录方式

### 1.2 添加Android应用到Firebase
1. 在Firebase项目中添加Android应用
2. 输入包名（Bundle ID）：例如 `com.example.testgoogle`
3. 下载 `google-services.json` 文件
4. 将文件放在 `Assets/Plugins/Android/` 目录下

### 1.3 添加iOS应用到Firebase
1. 在Firebase项目中添加iOS应用
2. 输入Bundle ID：例如 `com.example.testgoogle`
3. 下载 `GoogleService-Info.plist` 文件
4. 将文件放在 `Assets/` 目录下

---

## 第二步：Android配置

### 2.1 获取SHA-1指纹
```bash
# Windows用户在Unity中
# 1. 打开 Android Build Settings
# 2. 点击 "Create new keystore"
# 3. 或使用现有keystore获取SHA-1
```

1. File > Build Settings > Android
2. 点击 "Player Settings"
3. 在Inspector中找到 "Keystore Manager"
4. 如果没有keystore，创建新的
5. 复制SHA-1指纹

### 2.2 添加SHA-1到Firebase
1. 返回Firebase控制台
2. Project Settings > Your apps
3. 选择Android应用
4. 在 SHA certificate fingerprints 中添加SHA-1
5. 重新下载 `google-services.json` 并替换

### 2.3 配置Android依赖
1. 打开 `Assets/Plugins/Android/AndroidManifest.xml`（如果不存在则创建）
2. 确保包含以下权限：
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

### 2.4 配置build.gradle
创建文件 `Assets/Plugins/Android/mainTemplate.gradle`：
```gradle
dependencies {
    implementation 'com.google.android.gms:play-services-auth:20.7.0'
    implementation 'com.google.firebase:firebase-auth'
    implementation 'com.google.firebase:firebase-core'
}

android {
    compileSdkVersion 33
    targetSdkVersion 33
}
```

---

## 第三步：iOS配置

### 3.1 配置URL Schemes
1. 打开 iOS Build Settings
2. Player Settings > iOS
3. 在 Other Settings 中找到 Custom URL Schemes
4. 添加：`REVERSED_CLIENT_ID`（从GoogleService-Info.plist文件中获取）

### 3.2 配置Info.plist
创建文件 `Assets/Plugins/iOS/GoogleSignIn.plist`：
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>GIDClientID</key>
    <string>YOUR_CLIENT_ID.apps.googleusercontent.com</string>
    <key>GIDServerClientID</key>
    <string>YOUR_SERVER_CLIENT_ID</string>
</dict>
</plist>
```

### 3.3 Podfile配置
创建文件 `Assets/Plugins/iOS/Podfile`：
```ruby
target 'UnityFramework' do
  pod 'GoogleSignIn', '~> 7.0'
  pod 'Firebase/Auth'
  pod 'Firebase/Core'
end
```

---

## 第四步：Unity场景和UI配置

### 4.1 创建Canvas和UI
1. 在Scene中创建 UI > Canvas
2. 在Canvas下创建以下UI元素：

#### 创建文本显示（StatusText）
- Right-click on Canvas > UI > Text (Legacy)
- 命名为 "StatusText"
- 设置：
  - Alignment: Center
  - Text: "未登录"
  - Font Size: 30
  - Color: Black

#### 创建登录按钮（GoogleLoginButton）
- Right-click on Canvas > UI > Button (Legacy)
- 命名为 "LoginButton"
- 设置：
  - Text: "Google Login"
  - Font Size: 20

#### 创建登出按钮（LogoutButton）
- Right-click on Canvas > UI > Button (Legacy)
- 命名为 "LogoutButton"
- 设置：
  - Text: "Logout"
  - Font Size: 20
  - Interactable: 关闭（登出前不可用）

### 4.2 创建Manager对象
1. 创建空GameObject，命名为 "LoginManager"
2. 将TestLogin脚本挂到此GameObject上

### 4.3 在Inspector中关联UI
1. 选择 LoginManager
2. 在Inspector中找到TestLogin脚本组件
3. 关联：
   - Status Text → StatusText
   - Google Login Button → LoginButton
   - Logout Button → LogoutButton

---

## 第五步：Player Settings配置

### 5.1 基础设置
1. File > Build Settings > Player Settings
2. 设置：
   - Company Name: `YourCompany`
   - Product Name: `TestGoogle`
   - Bundle Identifier: `com.example.testgoogle` （必须与Firebase中配置一致）

### 5.2 Android特定设置
- Target API Level: 31 或更高
- Minimum API Level: 21
- Scripting Backend: IL2CPP
- API Compatibility Level: .NET Standard 2.1

### 5.3 iOS特定设置
- Target Minimum iOS Version: 12.0 或更高
- Scripting Backend: IL2CPP

---

## 第六步：测试

### 6.1 编辑器测试
1. 在Editor中运行，会显示"只支持Android和iOS平台"的错误
2. 这是正常的（编辑器不支持原生Google登录）

### 6.2 Android测试
1. File > Build & Run (选择Android设备)
2. 等待构建完成
3. 在设备上点击"Google Login"按钮
4. 选择Google账户登录
5. 验证显示"登录成功"信息

### 6.3 iOS测试
1. File > Build (选择iOS)
2. 打开生成的Xcode项目
3. 配置签名证书
4. 在Xcode中构建并运行
5. 在设备上测试Google登录

---

## 常见问题解决

### 问题1：Firebase依赖关系错误
**解决方法：**
- 确保 google-services.json 在正确位置
- 重新导入Firebase SDK
- Clean Assets/Plugins 文件夹，重新添加

### 问题2：Android登录时显示"未获取到Google账户"
**解决方法：**
- 检查SHA-1指纹是否正确添加到Firebase
- 确保设备上已安装Google Play Services
- 检查Bundle ID是否与Firebase配置一致

### 问题3：iOS登录不工作
**解决方法：**
- 确保 GoogleService-Info.plist 正确
- 检查URL Schemes配置
- 确保有有效的Apple Developer证书
- 检查 Podfile 依赖是否正确

### 问题4：编译时找不到Firebase/Auth
**解决方法：**
- 确保Firebase SDK已正确导入Assets目录
- 重启Unity
- 重新生成Android build

---

## 检查清单

- [ ] Firebase项目已创建
- [ ] Authentication > Google已启用
- [ ] google-services.json已下载到Assets/Plugins/Android/
- [ ] GoogleService-Info.plist已下载到Assets/
- [ ] SHA-1指纹已添加到Firebase
- [ ] Bundle ID在所有地方一致
- [ ] Canvas和UI已创建
- [ ] TestLogin脚本已关联UI组件
- [ ] Player Settings已配置
- [ ] Android依赖已添加
- [ ] iOS配置文件已创建

---

## 文件结构检查
```
Assets/
├── Demo/
│   └── TestLogin.cs
├── Plugins/
│   ├── Android/
│   │   ├── google-services.json
│   │   ├── AndroidManifest.xml
│   │   └── mainTemplate.gradle
│   └── iOS/
│       ├── GoogleSignIn.plist
│       └── Podfile
└── GoogleService-Info.plist
```

---

**完成以上所有步骤后，您的谷歌登录功能应该可以正常工作！**
