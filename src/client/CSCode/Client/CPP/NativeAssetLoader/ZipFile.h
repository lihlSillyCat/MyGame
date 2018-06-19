#pragma once
#include <string>

#ifndef _unz64_H
typedef struct unz_file_info_s unz_file_info;
#endif

class ZipFilePrivate;

class ZipFile
{
public:
    ZipFile(const std::string &zipFile, const std::string &filter = std::string());
    virtual ~ZipFile();

    bool setFilter(const std::string &filter);

    bool fileExists(const std::string &fileName) const;

    size_t getFileLength(const std::string &fileName) const;

    int getFileData(const std::string &fileName, unsigned char* buffer, size_t count);

private:
    ZipFile();

    /** Internal data like zip file pointer / file list array and so on */
    ZipFilePrivate *_data;
};