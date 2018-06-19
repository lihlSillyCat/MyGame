#pragma once

#include <jni.h>
#include <android/asset_manager_jni.h>
#include <android/asset_manager.h>

#ifdef __cplusplus
extern "C"
{
#endif
    void SetupAssetManager(jobject assetManager, const char* dataPath);

    JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM *vm, void *reserved);

    JNIEXPORT void JNICALL JNI_OnUnload(JavaVM *jvm, void *reserved);

    int GetStreamAssetLength(const char* fileName);
    int GetStreamAssetContent(const char* fileName, unsigned char* data, size_t count);
#ifdef __cplusplus
}
#endif