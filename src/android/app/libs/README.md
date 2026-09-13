# Mi Push Android SDK

从小米开发者平台下载最新版 Android AAR SDK，并将其重命名为：

```text
MiPush_SDK_Client.aar
```

放在本目录中。`.gitignore` 会忽略 AAR 文件，避免把第三方二进制 SDK 提交到仓库。

没有 AAR 时仍可构建 `stubDebug`：

```bash
gradle :app:assembleStubDebug
```

放入 AAR 并设置 `MIPUSH_APP_ID`、`MIPUSH_APP_KEY` 后，可构建：

```bash
gradle :app:assembleMipushDebug
```
