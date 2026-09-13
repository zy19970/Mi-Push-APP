package com.zy19970.mipushapp

import android.content.Context
import com.xiaomi.mipush.sdk.MiPushClient

object PushGatewayProvider {
    fun create(context: Context): PushGateway = MiPushGateway(context.applicationContext)
}

private class MiPushGateway(private val context: Context) : PushGateway {
    override val implementationName: String = "Xiaomi Push"

    override fun register(): String? {
        if (BuildConfig.MIPUSH_APP_ID.isBlank() || BuildConfig.MIPUSH_APP_KEY.isBlank()) {
            return "缺少 MIPUSH_APP_ID / MIPUSH_APP_KEY。"
        }
        return try {
            MiPushClient.registerPush(context, BuildConfig.MIPUSH_APP_ID, BuildConfig.MIPUSH_APP_KEY)
            null
        } catch (t: Throwable) {
            "注册失败：${t.message ?: t.javaClass.simpleName}"
        }
    }

    override fun currentRegId(): String? =
        runCatching { MiPushClient.getRegId(context) }
            .getOrNull()
            ?.takeIf { it.isNotBlank() }
}
