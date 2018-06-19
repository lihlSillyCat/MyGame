#include "AssetManager.h"
#include "ZipFile.h"

AAssetManager* mgr = nullptr;
JNIEnv* jni_env = nullptr;
ZipFile* obbfile = nullptr;

static const std::string assetPrefix = std::string("assets/");

void SetupAssetManager(jobject assetManager, const char* dataPath)
{
    LOGD("JNI SetupAssetManager");
    std::string assetsPath(dataPath);
    if (assetsPath.find("/obb/") != std::string::npos)
    {
        obbfile = new ZipFile(assetsPath, assetPrefix);
        LOGD("Obb file is loaded: %s", assetsPath.c_str());
    }

    mgr = AAssetManager_fromJava(jni_env, assetManager);
    if (mgr == nullptr)
    {
        LOGD("AAssetManager is null");
        return;
    }
}

JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM *vm, void *reserved)
{
    LOGD("JNI OnLoad");
    vm->AttachCurrentThread(&jni_env, 0);
    return JNI_VERSION_1_6;
}

JNIEXPORT void JNICALL JNI_OnUnload(JavaVM *jvm, void *reserved)
{
    LOGD("JNI OnUnLoad");
    if (obbfile)
    {
        delete obbfile;
        obbfile = nullptr;
    }
}

int GetStreamAssetLength(const char* fileName)
{
    if (obbfile)
    {
        std::string assetPath = assetPrefix + fileName;
        LOGD("GetStreamAssetContent: %s", assetPath.c_str());
        return obbfile->getFileLength(assetPath);
    }
    
    if (mgr == nullptr)
    {
        LOGD("manager is null");
        return 0;
    }

    AAsset* asset = AAssetManager_open(mgr, fileName, AASSET_MODE_UNKNOWN);
    if (asset == nullptr)
    {
        LOGD("asset is null");
        return 0;
    }

    return AAsset_getLength(asset);
}

int GetStreamAssetContent(const char* fileName, unsigned char* data, size_t count)
{
    if (obbfile)
    {
        std::string assetPath = assetPrefix + fileName;
        LOGD("GetStreamAssetContent: %s", assetPath.c_str());
        return obbfile->getFileData(assetPath, data, count);
    }

    if (mgr == nullptr)
    {
        LOGD("manager is null");
        return -1;
    }

    AAsset* asset = AAssetManager_open(mgr, fileName, AASSET_MODE_UNKNOWN);
    if (asset == nullptr)
    {
        LOGD("asset is null");
        return -1;
    }

    int ret = AAsset_read(asset, data, count);

    if (ret <= 0)
    {
        LOGD("read asset error: %s", fileName);
        data = nullptr;
    }

    AAsset_close(asset);
    return ret;
}
