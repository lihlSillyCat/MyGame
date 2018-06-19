#include <jni.h>
#include <errno.h>

#include <string.h>
#include <unistd.h>
#include <sys/resource.h>

#include <android/log.h>

#if __DEBUG__
#define LOGD(...) ((void)__android_log_print(ANDROID_LOG_DEBUG, "Unity", __VA_ARGS__))
#else
#define LOGD(...) {}
#endif

#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "Unity", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "Unity", __VA_ARGS__))
#define LOGE(...) ((void)__android_log_print(ANDROID_LOG_ERROR, "Unity", __VA_ARGS__))

#ifndef UNZ_MAXFILENAMEINZIP
#define UNZ_MAXFILENAMEINZIP (256)
#endif

#define SAFE_DELETE(p) do { if (p) { delete (p); (p) = nullptr; } } while(0)