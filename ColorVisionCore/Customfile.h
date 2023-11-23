#pragma once

#ifdef COLORVISIONCORE_EXPORTS
#define OPENCV_API __declspec(dllexport)
#else
#define OPENCV_API __declspec(dllimport)
#endif

struct HImage
{
    int rows;
    int cols;
    int type;
    char* pData;
};

typedef struct CustomMatFile
{
    int rows;
    int cols;
    int type;
    int compression; //0,��ѹ��; 1,Zlib; 2,gz
    long long srcLen; //Mat.data �ĳ���
    long long destLen; //��ѹ��ʱ��destLen =0;
}CustomMatFile;

typedef struct  CustomFileHeader
{
    char Name[6] = { 0x43,0x75,0x73,0x74,0x6f,0x6d }; //Custom
    int Version; //0
    int Matoffset; //ֱ�Ӷ�ȡMat���ݵ�ƫ���� int ������2G��С�������Ҫ���࣬����Ҫfloat or double d����������ڴ����Ƚ��鷳
}CustomFileHeader;





extern "C" OPENCV_API int CVWrite(char* path, HImage src, int compression = 0);
extern "C" OPENCV_API int CVRead(char* FileName, HImage src);



