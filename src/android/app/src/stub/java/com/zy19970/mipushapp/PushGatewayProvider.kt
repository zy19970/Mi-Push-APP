package com.zy19970.mipushapp

import android.content.Context

object PushGatewayProvider {
    fun create(context: Context): PushGateway = object : PushGateway {
        override val implementationName: String = "stub"
        override fun register(): String = "当前是 stub 构建。放入 MiPush AAR 并构建 mipushDebug 才会连接 Xiaomi Push。"
        override fun currentRegId(): String? = null
    }
}
