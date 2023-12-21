#include "pch.h"
#include "export.h"

#include <opencv2/opencv.hpp>


int ReadGhostHImage(HImage img, HImage* outImage)
{
	 cv::Mat mat(img.rows, img.cols, img.type(), img.pData);

	 if (mat.empty())
		 return -1;

	 if (mat.channels() != 1) {
		 cv::cvtColor(mat, mat, cv::COLOR_BGR2GRAY);
	 }
	// ת��Ϊ8λͼ��
	double minVal, maxVal;
	cv::minMaxLoc(mat, &minVal, &maxVal); // �ҵ�ͼ�����С���������ֵ
	cv::Mat scaledMat;
	mat.convertTo(scaledMat, CV_8UC1, 255.0 / (maxVal - minVal), -minVal * 255.0 / (maxVal - minVal));

	cv::applyColorMap(scaledMat, scaledMat, cv::COLORMAP_JET);

	///���ﲻ����Ļ����ֲ��ڴ�������н���֮�����
	outImage->pData = new unsigned char[scaledMat.total() * scaledMat.elemSize()];
	memcpy(outImage->pData, scaledMat.data, scaledMat.total() * scaledMat.elemSize());

	outImage->rows = scaledMat.rows;
	outImage->cols = scaledMat.cols;
	outImage->channels = scaledMat.channels();
	outImage->depth = scaledMat.depth(); // ����ÿ����λ��
	return 0;
}




int ReadGhostImage(const char* FilePath, int singleLedPixelNum, int* LED_pixel_X , int* LED_pixel_Y, int singleGhostPixelNum, int* Ghost_pixel_X, int* Ghost_pixel_Y, HImage* outImage)
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
	cv::cvtColor(scaledMat, scaledMat, cv::COLOR_GRAY2BGR);

	std::vector<std::vector<cv::Point>> paintContours;

	for (size_t i = 0; i < singleLedPixelNum; i++)
	{
		std::vector<cv::Point> lists;
		lists.push_back(cv::Point(LED_pixel_X[i], LED_pixel_Y[i]));
		paintContours.push_back(lists);
	}

	cv::drawContours(scaledMat, paintContours, -1, cv::Scalar(0, 255,0), -1, 8, cv::noArray(), INT_MAX, cv::Point());


	//paintContours.clear();
	std::vector<std::vector<cv::Point>> paintContours1;


	for (size_t i = 0; i < singleGhostPixelNum; i++)
	{
		std::vector<cv::Point> lists;
		lists.push_back(cv::Point(Ghost_pixel_X[i], Ghost_pixel_Y[i]));
		paintContours1.push_back(lists);
	}
	cv::drawContours(scaledMat, paintContours1, -1, cv::Scalar(0,0, 255), -1, 8, cv::noArray(), INT_MAX, cv::Point());
	//paintContours.clear();



	///���ﲻ����Ļ����ֲ��ڴ�������н���֮�����
	outImage->pData = new unsigned char[scaledMat.total() * scaledMat.elemSize()];
	memcpy(outImage->pData, scaledMat.data, scaledMat.total() * scaledMat.elemSize());

	outImage->rows = scaledMat.rows;
	outImage->cols = scaledMat.cols;
	outImage->channels = scaledMat.channels();
	outImage->depth = scaledMat.depth(); // ����ÿ����λ��
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




