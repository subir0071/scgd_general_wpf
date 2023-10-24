#include "CVCIEFile.h"
#include "pch.h"
#include<iostream>
#include<fstream>
using namespace std;

char ciehd[] = { 'C', 'V', 'C', 'I', 'E' };
int cur_ver = 1;

EXPORTC MYDLL int STDCALL WriteCVCIE(char* cieFileName, float* exp, int width, int height, int bpp, int channels, const char* data, int dateLen, char* srcFileName) {
	ofstream file(cieFileName);
	file.write(ciehd, 5);//��ʶ
	//�汾
	file.write((char*)&cur_ver, sizeof(cur_ver));
	int len = strlen(srcFileName);
	file.write((char*)&len, sizeof(len));//��ʾ��Դͼ
	file.write(srcFileName, len);//��ʾ��Դͼ
	//ͨ��
	file.write((char*)&channels, sizeof(channels));
	//�ع�
	for (int i = 0; i < channels; i++) {
		file.write((char*)&exp[i], sizeof(float));
	}
	//��
	file.write((char*)&width, sizeof(width));
	//��
	file.write((char*)&height, sizeof(height));
	//λ
	file.write((char*)&bpp, sizeof(bpp));
	//���ݳ���
	file.write((char*)&dateLen, sizeof(dateLen));
	//����
	file.write(data, dateLen);
	file.close();
	return 0;
}

int readHeader(ifstream& file, float* exp, int& width, int& height, int& bpp, int& channels, int& dateLen, char* srcFileName, int& srcFileNameLen) {
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
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
*/
EXPORTC MYDLL int STDCALL ReadCVCIEHeader(char* cieFileName, int& width, int& height, int& bpp, int& channels, int& dataLen, int& srcFileNameLen) {
	ifstream file(cieFileName);
	float* exp = new float[3];
	int ret = readHeader(file, exp, width, height, bpp, channels, dataLen, NULL, srcFileNameLen);
	file.close();
	return ret;
}


/*
* ����ֵ:
 -1 : �ļ�ͷ�Ƿ�
 -2 : �ļ��汾�Ƿ�
 -3 : ���������Ȳ���
*/
EXPORTC MYDLL int STDCALL ReadCVCIE(char* cieFileName, float* exp, char* data, int dataLen, char* srcFileName, int srcFileNameLen) {
	ifstream file(cieFileName);
	int width, height, bpp, channels, _dataLen, _srcFileNameLen = srcFileNameLen;
	int ret = readHeader(file, exp, width, height, bpp, channels, _dataLen, srcFileName, _srcFileNameLen);
	if (ret == 0) {
		if (dataLen >= _dataLen) {
			file.read(data, _dataLen);
		}
		else {
			ret = -3;
		}
	}

	file.close();
	return ret;
}