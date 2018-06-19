#include "ZipFile.h"

#include <zlib.h>
#include <assert.h>
#include <unordered_map>
#include "unzip/unzip.h"

static const std::string emptyFilename("");

struct ZipEntryInfo
{
    unz_file_pos pos;
    uLong uncompressed_size;
};

class ZipFilePrivate
{
public:
    unzFile zipFile;

    // std::unordered_map is faster if available on the platform
    typedef std::unordered_map<std::string, struct ZipEntryInfo> FileListContainer;
    FileListContainer fileList;
};

ZipFile::ZipFile()
    : _data(new ZipFilePrivate)
{
    _data->zipFile = nullptr;
}

ZipFile::ZipFile(const std::string &zipFile, const std::string &filter)
    : _data(new ZipFilePrivate)
{
    _data->zipFile = unzOpen(zipFile.c_str());
    setFilter(filter);
}

ZipFile::~ZipFile()
{
    if (_data && _data->zipFile)
    {
        unzClose(_data->zipFile);
    }

    SAFE_DELETE(_data);
}

bool ZipFile::setFilter(const std::string &filter)
{
    if (!_data)
    {
        LOGE("setFilter error: _data is null");
        return false;
    }
    if (!_data->zipFile)
    {
        LOGE("setFilter error: zipFile is null");
        return false;
    }

    // clear existing file list
    _data->fileList.clear();

    // UNZ_MAXFILENAMEINZIP + 1 - it is done so in unzLocateFile
    char szCurrentFileName[UNZ_MAXFILENAMEINZIP + 1];
    unz_file_info64 fileInfo;

    // go through all files and store position information about the required files
    int err = unzGoToFirstFile64(_data->zipFile, &fileInfo,
        szCurrentFileName, sizeof(szCurrentFileName) - 1);
    while (err == UNZ_OK)
    {
        unz_file_pos posInfo;
        int posErr = unzGetFilePos(_data->zipFile, &posInfo);
        if (posErr == UNZ_OK)
        {
            std::string currentFileName = szCurrentFileName;
            // cache info about filtered files only (like 'assets/')
            if (filter.empty()
                || currentFileName.substr(0, filter.length()) == filter)
            {
                ZipEntryInfo entry;
                entry.pos = posInfo;
                entry.uncompressed_size = (uLong)fileInfo.uncompressed_size;
                _data->fileList[currentFileName] = entry;
            }
        }
        // next file - also get the information about it
        err = unzGoToNextFile64(_data->zipFile, &fileInfo,
            szCurrentFileName, sizeof(szCurrentFileName) - 1);
    }
    
    return true;
}

bool ZipFile::fileExists(const std::string &fileName) const
{
    bool ret = false;
    if (!_data)
    {
        return ret;
    }

    ret = _data->fileList.find(fileName) != _data->fileList.end();

    return ret;
}

size_t ZipFile::getFileLength(const std::string &fileName) const
{
    if (!_data)
    {
        LOGE("ZipFile::getFileLength error, _data is null");
        return 0;
    }

    auto fileIter = _data->fileList.find(fileName);
    if (fileIter != _data->fileList.end())
    {
        LOGE("ZipFile::getFileLength: %ld", fileIter->second.uncompressed_size);
        return fileIter->second.uncompressed_size;
    }

    LOGE("ZipFile::getFileLength error, can not find file: %s", fileName.c_str());
    return 0;
}

int ZipFile::getFileData(const std::string &fileName, unsigned char* buffer, size_t count)
{
    if (!_data->zipFile)
    {
        return -1;
    }
    if (fileName.empty())
    {
        return -1;
    }

    ZipFilePrivate::FileListContainer::const_iterator it = _data->fileList.find(fileName);
    if (it == _data->fileList.end())
    {
        return -1;
    }

    ZipEntryInfo fileInfo = it->second;

    int nRet = unzGoToFilePos(_data->zipFile, &fileInfo.pos);
    if (UNZ_OK != nRet)
    {
        return -1;
    }

    nRet = unzOpenCurrentFile(_data->zipFile);
    if (UNZ_OK != nRet)
    {
        return -1;
    }

    int nSize = unzReadCurrentFile(_data->zipFile, buffer, count);
    assert(nSize == 0 || nSize == (int)fileInfo.uncompressed_size);
    unzCloseCurrentFile(_data->zipFile);

    return nSize;
}
