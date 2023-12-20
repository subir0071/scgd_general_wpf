#include "pch.h"
#include "export.h"

#include <opencv2/opencv.hpp>


int ReadGhostImage(const char* FilePath, unsigned char** data, int* rows, int* cols, int* channels)
{
	cv::Mat mat = cv::imread(FilePath, cv::ImreadModes::IMREAD_UNCHANGED);
	if (mat.empty())
		return -1;

	// ȷ��ͼ����CV_32FC1����
	if (mat.type() != CV_32FC1) {
		return -2; // ����������������ת��ͼ������
	}

	// ת��Ϊ8λͼ��
	double minVal, maxVal;
	cv::minMaxLoc(mat, &minVal, &maxVal); // �ҵ�ͼ�����С���������ֵ
	cv::Mat scaledMat;
	mat.convertTo(scaledMat, CV_8UC1, 255.0 / (maxVal - minVal), -minVal * 255.0 / (maxVal - minVal));

	// Ϊͼ�����ݷ����ڴ�
	*data = new unsigned char[scaledMat.total() * scaledMat.elemSize()];
	memcpy(*data, scaledMat.data, scaledMat.total() * scaledMat.elemSize());

	*rows = scaledMat.rows;
	*cols = scaledMat.cols;
	*channels = scaledMat.channels();
	return 0;

}



double CalArtculation(int nw,int nh,char* data) {
	cv::Mat img =cv::Mat(nh, nw, CV_8UC1, data);
	//cv::Mat gray;

	//cv::cvtColor(img, gray, cv::ColorConversionCodes::COLOR_RGB2GRAY);
	cv::Mat TempMen;
	cv::Mat TempStd;
	cv::meanStdDev(img, TempMen, TempStd);

	double value = TempStd.at<double>(0, 0);
	return value;
}

double CalArtculationROI(int nw, int nh, char* data, int x, int y, int width, int height) {
	cv::Mat img = cv::Mat(nh, nw, CV_8UC1, data);
	cv::Mat m_roi = img(cv::Rect(x, y, width, height));

	//cv::Mat gray;

	//cv::cvtColor(m_roi, gray, cv::ColorConversionCodes::COLOR_RGB2GRAY);
	cv::Mat TempMen;
	cv::Mat TempStd;
	cv::meanStdDev(m_roi, TempMen, TempStd);

	double value = TempStd.at<double>(0, 0);
	return value;
}




