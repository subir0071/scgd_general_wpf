#pragma once


#define MYDLL _declspec(dllexport)
#define EXPORTC extern "C"
#define STDCALL __stdcall


struct CVCIEFileInfo
{
    //ͼ����
    int width;
    //ͼ��߶�
    int height;
    //ͼ��λ����8/16/32
    int bpp;
    //ͼ��ͨ����, 1/3
    int channels;
    //����
    int gain;
    //�ع�ʱ��
    float* exp;
    //Դͼ���ļ�
    char* srcFileName;
    //Դͼ���ļ�����������С
    int srcFileNameLen;
    //ͼ������
    char* data;
    //ͼ�����ݴ�С
    int dataLen;
};

/*
* дCVCIE�ļ�
* ����:
* cieFileName ��CVCIE�ļ���
* exp ���ع�ʱ������ָ��
* width ��ͼ����
* height ��ͼ��߶�
* bpp ��ͼ��λ����8/16/32
* channels ��ͼ��ͨ����
* data ��ͼ������
* dataLen ��ͼ�����ݴ�С
* srcFileName ��Դͼ���ļ�
* 
* ����ֵ:
* 0 : д��ɹ�
 -1 : д��ʧ��
*/
EXPORTC MYDLL int STDCALL WriteCVCIE(char* cieFileName, CVCIEFileInfo fileInfo);

/*
* ��ȡCVCIE�ļ�ͷ��Ϣ
* ����ֵ:
* 0 : �ɹ�
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
 -999 : �ļ�������
*/
EXPORTC MYDLL int STDCALL ReadCVCIEHeader(char* cieFileName, CVCIEFileInfo& fileInfo);

/*
* ��ȡCVCIE�ļ���Ϣ
* ����ֵ:
* 0 : �ɹ�
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
 -999 : �ļ�������
*/
EXPORTC MYDLL int STDCALL ReadCVCIE(char* cieFileName, CVCIEFileInfo& fileInfo);

/*
* ��ȡ�ļ���С
* 
* ����ֵ:
* >0 : �ɹ�
* -1 : ʧ��,�ļ�Ϊ�ջ��ļ�������
*/
EXPORTC MYDLL long STDCALL GetCVCIEFileLength(char* cieFileName);

/*
* һ�ζ�ȡCVCIE�ļ�
* ����ֵ:
* 0 : �ɹ�
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
 -3 : ���������Ȳ���
 -999 : �ļ�������
*/
EXPORTC MYDLL int STDCALL ReadCVCIEByOne(char* cieFileName, CVCIEFileInfo& fileInfo);