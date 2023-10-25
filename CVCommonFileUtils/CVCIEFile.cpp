#include "CVCIEFile.h"
#include "pch.h"

#include <sys/stat.h> // struct stat
#include<iostream>
#include<fstream>
using namespace std;

char ciehd[] = { 'C', 'V', 'C', 'I', 'E' };
int cur_ver = 1;

EXPORTC MYDLL int STDCALL WriteCVCIE(char* cieFileName, CVCIEFileInfo* fileInfo) {
	ofstream file(cieFileName);
	int ret = -1;
	if (file.good()) {
		file.write(ciehd, 5);//��ʶ
		//�汾
		file.write((char*)&cur_ver, sizeof(cur_ver));
		file.write((char*)&fileInfo->srcFileNameLen, sizeof(fileInfo->srcFileNameLen));//��ʾ��Դͼ
		file.write(fileInfo->srcFileName, fileInfo->srcFileNameLen);//��ʾ��Դͼ
		//����
		file.write((char*)&fileInfo->gain, sizeof(fileInfo->gain));
		//ͨ��
		file.write((char*)&fileInfo->channels, sizeof(fileInfo->channels));
		//�ع�
		for (int i = 0; i < fileInfo->channels; i++) {
			file.write((char*)&fileInfo->exp[i], sizeof(float));
		}
		//��
		file.write((char*)&fileInfo->width, sizeof(fileInfo->width));
		//��
		file.write((char*)&fileInfo->height, sizeof(fileInfo->height));
		//λ
		file.write((char*)&fileInfo->bpp, sizeof(fileInfo->bpp));
		//���ݳ���
		file.write((char*)&fileInfo->dataLen, sizeof(fileInfo->dataLen));
		//����
		file.write(fileInfo->data, fileInfo->dataLen);
		file.flush();
		ret = 0;
	}
	else {
		ret = -1;
	}
	file.close();
	return ret;
}
/*
* ����ֵ:
* 0 : �ɹ�
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
*/
int readHeader(ifstream& file, float* exp, int& width, int& height, int& bpp, int& channels, int& gain, int& dateLen, char* srcFileName, int& srcFileNameLen) {
	char hd[5];
	file.read(hd, 5);
	for (int i = 0; i < 5; i++) {
		if (hd[i] != ciehd[i]) {
			return -1;
		}
	}
	int ver = 0;
	file.read(reinterpret_cast<char*>(&ver), sizeof(int));
	if (ver != cur_ver) {
		return -2;
	}
	//��ʾ��Դͼ
	int len = 0;
	file.read(reinterpret_cast<char*>(&len), sizeof(int));
	if (len > 0 ) {
		if (srcFileNameLen >= len && srcFileName != NULL) { memset(srcFileName, 0, srcFileNameLen); file.read(srcFileName, len); }
		else file.seekg(len, ios::cur);
		srcFileNameLen = len;
	}
	//����
	file.read(reinterpret_cast<char*>(&gain), sizeof(int));
	//ͨ��
	file.read(reinterpret_cast<char*>(&channels), sizeof(int));
	//�ع�
	for (int i = 0; i < channels; i++) {
		file.read(reinterpret_cast<char*>(&exp[i]), sizeof(float));
	}
	//��
	file.read(reinterpret_cast<char*>(&width), sizeof(int));
	//��
	file.read(reinterpret_cast<char*>(&height), sizeof(int));
	//λ
	file.read(reinterpret_cast<char*>(&bpp), sizeof(int));
	//���ݳ���
	file.read(reinterpret_cast<char*>(&dateLen), sizeof(int));
	return 0;
}

/*
* ����ֵ:
* 0 : �ɹ�
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
 -999 : �ļ�������
*/
EXPORTC MYDLL int STDCALL ReadCVCIEHeader(char* cieFileName, CVCIEFileInfo* fileInfo) {
	ifstream file(cieFileName);
	int ret = -999;
	if (file.good()) {
		float* exp = new float[3];
		fileInfo->exp = NULL;
		fileInfo->data = NULL;
		ret = readHeader(file, exp, fileInfo->width, fileInfo->height, fileInfo->bpp, fileInfo->channels, fileInfo->gain, fileInfo->dataLen, fileInfo->srcFileName, fileInfo->srcFileNameLen);
	}
	else {
		ret = -999;
	}
	file.close();
	return ret;
}

EXPORTC MYDLL long STDCALL GetCVCIEFileLength(char* cieFileName) {
	if (cieFileName == NULL) {
		return -1;
	}
	// ����һ���洢�ļ�(��)��Ϣ�Ľṹ�壬�������ļ���С�ʹ���ʱ�䡢����ʱ�䡢�޸�ʱ���
	struct stat statbuf;
	// �ṩ�ļ����ַ���������ļ����Խṹ��
	if (stat(cieFileName, &statbuf) == 0) {
		// ��ȡ�ļ���С
		size_t filesize = statbuf.st_size;
		return filesize;
	}
	else {
		return -1;
	}
}

/*
* ����ֵ:
* 0 : �ɹ�
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
 -3 : ���������Ȳ���
 -999 : �ļ�������
*/
EXPORTC MYDLL int STDCALL ReadCVCIE(char* cieFileName, CVCIEFileInfo* fileInfo) {
	ifstream file(cieFileName);
	int ret = -999;
	if (file.good()) {
		int _dataLen, _srcFileNameLen = fileInfo->srcFileNameLen;
		ret = readHeader(file, fileInfo->exp, fileInfo->width, fileInfo->height, fileInfo->bpp, fileInfo->channels, fileInfo->gain, _dataLen, fileInfo->srcFileName, _srcFileNameLen);
		if (ret == 0) {
			fileInfo->srcFileNameLen = _srcFileNameLen;
			if (fileInfo->dataLen >= _dataLen) {
				file.read(fileInfo->data, _dataLen);
				fileInfo->dataLen = _dataLen;
			}
			else {
				ret = -3;
			}
		}
	}
	else {
		ret = -999;
	}
	file.close();
	return ret;
}