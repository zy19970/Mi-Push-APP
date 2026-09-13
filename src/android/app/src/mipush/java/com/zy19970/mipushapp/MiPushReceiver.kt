package com.zy19970.mipushapp

import android.content.Context
import com.xiaomi.mipush.sdk.MiPushCommandMessage
import com.xiaomi.mipush.sdk.MiPushMessage
import com.xiaomi.mipush.sdk.PushMessageReceiver

class MiPushReceiver : PushMessageReceiver() {
    override fun onReceiveRegisterResult(context: Context, message: MiPushCommandMessage) {
        message.commandArguments
            ?.firstOrNull()
            ?.takeIf { it.isNotBlank() }
            ?.let { RegIdStore.save(context, it) }
    }

    override fun onNotificationMessageArrived(context: Context, message: MiPushMessage) {
        // Notification display is handled by Xiaomi Push. Local history will be added later.
    }

    override fun onNotificationMessageClicked(context: Context, message: MiPushMessage) {
        // Deep-link handling will be added in the next milestone.
    }
}
