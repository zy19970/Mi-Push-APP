# Mi Push APP

一个面向 Xiaomi HyperOS / MIUI 的个人通知工具套件。目标是在**不自建公网服务器**的前提下，让受信任的本地设备直接调用 Xiaomi Push API，将通知推送到自己的小米手机。

## 第一阶段架构

```text
Windows / NAS / CLI
        |
        | HTTPS + AppSecret + RegID
        v
Xiaomi Push API
        |
        v
Xiaomi HyperOS / MIUI 手机
```

仓库当前包含：

- `src/android`：Android 接收端。负责注册 Mi Push、获取/展示 RegID、复制 RegID。
- `src/cli/MiPush.Cli`：.NET 10 命令行发送端。直接调用 Xiaomi Push REST API，不经过自建服务器。
- `docs`：架构、安全边界与小米开发者平台配置说明。

## 安全边界

`AppSecret` **不能放进 Android APK，也不能提交到 GitHub**。Android 端只使用 AppId/AppKey 注册并获得 RegID；AppSecret 只保留在可信任的发送设备上。CLI 默认从环境变量 `MIPUSH_APP_SECRET` 读取 AppSecret，也支持发送时临时通过 `--app-secret` 传入。

## 快速开始

### 1. 小米开发者平台

创建 Android 应用并启用 Xiaomi Push，得到 AppId、AppKey、AppSecret。Android 客户端 SDK AAR 需要从小米开发者平台下载，重命名为 `MiPush_SDK_Client.aar` 后放到：

```text
src/android/app/libs/MiPush_SDK_Client.aar
```

### 2. Android 接收端

在 `src/android` 下设置 Gradle 属性或环境变量：

```text
MIPUSH_APP_ID=你的AppId
MIPUSH_APP_KEY=你的AppKey
```

构建真实 Mi Push 版本：

```bash
gradle :app:assembleMipushDebug
```

仓库还提供不依赖小米 AAR 的 `stub` flavor，用于 CI 和界面开发：

```bash
gradle :app:assembleStubDebug
```

安装 `mipushDebug` 后点击“注册/刷新”，注册成功的 RegID 会显示在主界面。

### 3. .NET 10 CLI

```bash
cd src/cli/MiPush.Cli
dotnet build

dotnet run -- config init --package com.zy19970.mipushapp
dotnet run -- device add phone "你的RegID"
```

在本机设置 AppSecret：

PowerShell：

```powershell
$env:MIPUSH_APP_SECRET = "你的AppSecret"
```

发送通知：

```bash
dotnet run -- send phone "任务完成" "模型转换已经完成"
```

默认调用中国大陆正式环境 `https://api.xmpush.xiaomi.com/v3/message/regid`。如使用非中国大陆 Xiaomi Push，可通过 CLI 配置 `--api-base` 指定对应区域的 API Base URL。

## 2026 年 Xiaomi Push 适配

CLI 已预留 `channel_id`、`template_id`、`template_param` 参数，以适配小米当前的通知模板/私信消息体系。具体配置见 `docs/xiaomi-setup.md`。

## 当前里程碑

- [x] 无自建服务器的总体架构
- [x] Android Mi Push 注册骨架
- [x] RegID 本地展示/复制
- [x] .NET 10 直连发送 CLI
- [x] 多设备 RegID 本地配置
- [x] Channel / Template 参数预留
- [ ] Android 二维码展示 RegID
- [ ] Windows GUI Sender
- [ ] Windows Credential Manager 安全保存 AppSecret
- [ ] 通知点击参数与本地消息历史
- [ ] GitHub Actions / PowerShell 一行调用封装
