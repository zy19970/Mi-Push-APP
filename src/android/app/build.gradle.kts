plugins {
    id("com.android.application")
}

fun credential(name: String): String =
    providers.gradleProperty(name)
        .orElse(providers.environmentVariable(name))
        .getOrElse("")

fun quoted(value: String): String = "\"" + value.replace("\\", "\\\\").replace("\"", "\\\"") + "\""

val configuredPackageName = credential("MIPUSH_PACKAGE_NAME").ifBlank { "com.zy19970.mipushapp" }
val miPushAar = file("libs/MiPush_SDK_Client.aar")

android {
    namespace = "com.zy19970.mipushapp"
    compileSdk = 36

    defaultConfig {
        applicationId = configuredPackageName
        minSdk = 23
        targetSdk = 36
        versionCode = 1
        versionName = "0.1.0"
    }

    buildFeatures {
        buildConfig = true
    }

    flavorDimensions += "push"
    productFlavors {
        create("stub") {
            dimension = "push"
            applicationIdSuffix = ".stub"
            versionNameSuffix = "-stub"
        }
        create("mipush") {
            dimension = "push"
            buildConfigField("String", "MIPUSH_APP_ID", quoted(credential("MIPUSH_APP_ID")))
            buildConfigField("String", "MIPUSH_APP_KEY", quoted(credential("MIPUSH_APP_KEY")))
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles("proguard-rules.pro")
        }
    }
}

dependencies {
    if (miPushAar.exists()) {
        add("mipushImplementation", files(miPushAar))
    } else {
        logger.lifecycle("Mi Push SDK AAR not found: ${miPushAar.absolutePath}. Stub builds remain available; mipush builds require the AAR.")
    }
}
