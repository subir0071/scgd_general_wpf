#pragma once
#include <opencv2/opencv.hpp>
#include <iostream>

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
    char Name[6] = { 0x43,0x75,0x73,0x74,0x6f,0x6d };
    int Version; //0
    int Matoffset; //ֱ�Ӷ�ȡMat���ݵ�ƫ���� int ������2G��С�������Ҫ���࣬����Ҫfloat or double d����������ڴ����Ƚ��鷳
}CustomFileHeader;

int WriteFile(std::string path, cv::Mat src, int compression = 1);
cv::Mat ReadFile(std::string FileName);
int GrifToMatGz(std::string path, cv::Mat& src);
void OsWrite(std::string path, cv::Mat src);
void OsWrite1(std::string path, cv::Mat src);

