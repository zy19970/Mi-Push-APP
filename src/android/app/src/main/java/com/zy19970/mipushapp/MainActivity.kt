package com.zy19970.mipushapp

import android.Manifest
import android.app.Activity
import android.content.ClipData
import android.content.ClipboardManager
import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.TextView
import android.widget.Toast

class MainActivity : Activity() {
    private lateinit var gateway: PushGateway
    private lateinit var statusText: TextView
    private lateinit var regIdText: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        gateway = PushGatewayProvider.create(applicationContext)
        statusText = findViewById(R.id.statusText)
        regIdText = findViewById(R.id.regIdText)

        findViewById<Button>(R.id.registerButton).setOnClickListener {
            val error = gateway.register()
            if (error == null) {
                statusText.text = getString(R.string.registering)
                window.decorView.postDelayed({ refreshUi() }, 1200)
            } else {
                statusText.text = error
            }
        }

        findViewById<Button>(R.id.copyButton).setOnClickListener {
            val regId = effectiveRegId()
            if (regId.isNullOrBlank()) {
                Toast.makeText(this, R.string.no_regid, Toast.LENGTH_SHORT).show()
            } else {
                val clipboard = getSystemService(Context.CLIPBOARD_SERVICE) as ClipboardManager
                clipboard.setPrimaryClip(ClipData.newPlainText("Mi Push RegID", regId))
                Toast.makeText(this, R.string.copied, Toast.LENGTH_SHORT).show()
            }
        }

        requestNotificationPermissionIfNeeded()
        refreshUi()
    }

    override fun onResume() {
        super.onResume()
        if (::gateway.isInitialized) refreshUi()
    }

    private fun effectiveRegId(): String? =
        RegIdStore.get(this) ?: gateway.currentRegId()?.also { RegIdStore.save(this, it) }

    private fun refreshUi() {
        val regId = effectiveRegId()
        statusText.text = if (regId.isNullOrBlank()) {
            getString(R.string.status_not_registered, gateway.implementationName)
        } else {
            getString(R.string.status_registered, gateway.implementationName)
        }
        regIdText.text = regId ?: getString(R.string.regid_placeholder)
        findViewById<Button>(R.id.copyButton).visibility = if (regId.isNullOrBlank()) View.INVISIBLE else View.VISIBLE
    }

    private fun requestNotificationPermissionIfNeeded() {
        if (Build.VERSION.SDK_INT >= 33 &&
            checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED
        ) {
            requestPermissions(arrayOf(Manifest.permission.POST_NOTIFICATIONS), 1001)
        }
    }
}
