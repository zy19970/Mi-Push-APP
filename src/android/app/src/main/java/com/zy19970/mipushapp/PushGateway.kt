package com.zy19970.mipushapp

interface PushGateway {
    val implementationName: String
    fun register(): String?
    fun currentRegId(): String?
}
