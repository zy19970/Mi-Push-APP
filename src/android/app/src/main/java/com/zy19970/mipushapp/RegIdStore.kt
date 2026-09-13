package com.zy19970.mipushapp

import android.content.Context

object RegIdStore {
    private const val PREFS = "mipush"
    private const val KEY_REG_ID = "reg_id"

    fun save(context: Context, regId: String) {
        context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .edit()
            .putString(KEY_REG_ID, regId)
            .apply()
    }

    fun get(context: Context): String? =
        context.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
            .getString(KEY_REG_ID, null)
            ?.takeIf { it.isNotBlank() }
}
