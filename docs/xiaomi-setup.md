# Xiaomi Push 接入说明

## 1. 开发者平台准备

在小米开发者平台创建 Android 应用、启用推送服务，并获得：

- AppId：Android 注册使用。
- AppKey：Android 注册使用。
- AppSecret：服务端/可信任发送端调用 REST API 使用，禁止放入 APK。

本仓库第一阶段以中国大陆版 Xiaomi Push SDK/API 为主。

## 2. Android SDK

从小米开发者平台下载最新版 Android AAR SDK，将文件重命名为：

```text
MiPush_SDK_Client.aar
```

放到：

```text
src/android/app/libs/MiPush_SDK_Client.aar
```

真实构建使用 `mipush` flavor；`stub` flavor 不依赖该 AAR，只用于 CI/界面开发。

## 3. Android 凭据

不要把 AppId/AppKey 写进仓库。可使用环境变量：

```text
MIPUSH_APP_ID=...
MIPUSH_APP_KEY=...
```

或者在用户级 Gradle 配置中设置同名属性。

## 4. REST API

中国大陆正式环境默认使用：

```text
POST https://api.xmpush.xiaomi.com/v3/message/regid
Authorization: key=<APP_SECRET>
Content-Type: application/x-www-form-urlencoded
```

基础字段包括：

```text
restricted_package_name
registration_id
title
description
```

本项目 CLI 还支持：

```text
extra.channel_id
extra.template_id
extra.template_param
```

用于适配小米当前通知模板体系。

## 5. 模板消息

如果你的小米 Push 应用已经启用/要求私信消息模板，请先在开发者平台创建 Channel 与 Template，再在 CLI 配置默认值：

```bash
dotnet run -- config init \
  --package com.example.app \
  --channel-id YOUR_CHANNEL_ID \
  --template-id YOUR_TEMPLATE_ID
```

发送时可以覆盖默认值：

```bash
dotnet run -- send phone "标题" "正文" \
  --channel-id YOUR_CHANNEL_ID \
  --template-id YOUR_TEMPLATE_ID \
  --template-param-json '{"name":"value"}'
```

模板正文/参数必须与小米开发者平台中配置的模板一致。

## 6. 非中国大陆区域

不同区域使用不同 API 域名，Android 国际版 SDK 还要求在注册前设置 Region。本仓库暂不自动推断区域；CLI 可以用 `--api-base` 自定义服务地址。Android 国际版将在后续独立 flavor 中处理，避免把中国大陆与全球 SDK 逻辑混在一起。
