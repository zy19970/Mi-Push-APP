package com.zy19970.mipushapp

import android.app.ActivityManager
import android.app.Application
import android.os.Process

class MiPushApplication : Application() {
    override fun onCreate() {
        super.onCreate()
        if (isMainProcess()) {
            PushGatewayProvider.create(this).register()
        }
    }

    private fun isMainProcess(): Boolean {
        val manager = getSystemService(ACTIVITY_SERVICE) as ActivityManager
        val mainProcess = applicationInfo.processName
        val pid = Process.myPid()
        return manager.runningAppProcesses
            ?.firstOrNull { it.pid == pid }
            ?.processName == mainProcess
    }
}
