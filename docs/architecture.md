# 架构说明

## 目标

Mi Push APP 的第一原则是：**个人自用场景不引入不必要的公网中间层**。

发送设备直接充当小米文档中的“应用服务器”角色：它持有 AppSecret 和目标设备 RegID，通过 HTTPS 调用 Xiaomi Push API。Android App 只负责向小米注册、获得 RegID 和接收通知。

```text
+----------------------+        HTTPS        +-------------------+
| Trusted sender       | ------------------> | Xiaomi Push Cloud |
| Windows / NAS / CLI  | AppSecret + RegID   +---------+---------+
+----------------------+                                |
                                                        | system push
                                                        v
                                              +---------+---------+
                                              | Xiaomi phone      |
                                              | Android receiver  |
                                              +-------------------+
```

## 为什么不需要自建服务器

单用户/少量设备场景中，自建服务器原本承担的主要职责只是保存 AppSecret、RegID 和转发请求。只要发送端本身是可信任设备，这三个职责可以在本地完成，因此可以省掉域名、TLS 证书、数据库、公网 IP、Docker 和服务器运维。

## 安全模型

1. Android APK 中只存在 AppId/AppKey，不存在 AppSecret。
2. AppSecret 只存在于可信任的发送设备。
3. RegID 可以通过复制或后续二维码方式从手机传给发送设备。
4. Git 仓库不保存真实 AppId/AppKey/AppSecret/RegID。
5. CLI 优先从 `MIPUSH_APP_SECRET` 环境变量读取 AppSecret。

## 局限

该模式不等价于 Bark 的公网 URL 模式。互联网中的任意第三方系统不能在“不持有 AppSecret、也没有中间网关”的情况下安全地直接发送通知。后续如确实需要 GitHub Webhook、云服务回调等，可另加一个可选 Gateway，但它不是本项目第一阶段的必需组件。
