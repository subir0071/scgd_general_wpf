#include "pch.h"
#include "opencv_media_export.h"
#include "algorithm.h"
#include <opencv2/opencv.hpp>
#include <nlohmann\json.hpp>

using json = nlohmann::json;

std::vector<std::pair<uintptr_t, cv::Mat>> MediaList;

std::mutex mediaListMutex; // ����һ��������

static void MatToHImage(cv::Mat& mat, HImage* outImage)
{
	std::lock_guard<std::mutex> lock(mediaListMutex); // ����


	outImage->rows = mat.rows;
	outImage->cols = mat.cols;
	outImage->channels = mat.channels();
	outImage->pData = mat.data;
	int bitsPerElement = 0;

	switch (mat.depth()) {
	case CV_8U:
	case CV_8S:
		bitsPerElement = 8;
		break;
	case CV_16U:
	case CV_16S:
		bitsPerElement = 16;
		break;
	case CV_32S:
	case CV_32F:
		bitsPerElement = 32;
		break;
	case CV_64F:
		bitsPerElement = 64;
		break;
	default:
		break;
	}
	outImage->depth = bitsPerElement; // ����ÿ����λ��
	outImage->stride = (int)mat.step; // ����ͼ��Ĳ���
	MediaList.push_back(std::make_pair(reinterpret_cast<uintptr_t>(outImage->pData), mat));
}

COLORVISIONCORE_API void M_FreeHImageData(unsigned char* data)
{
	std::lock_guard<std::mutex> lock(mediaListMutex); // ����
	auto it = std::find_if(MediaList.begin(), MediaList.end(),
		[data](const std::pair<int, cv::Mat>& pair) {
			return pair.first == reinterpret_cast<uintptr_t>(data);
		});
	if (it != MediaList.end()) {
		it->second.release();
		// �ӻ������Ƴ�
		MediaList.erase(it);
	}
}


COLORVISIONCORE_API int M_PseudoColor(HImage img, HImage* outImage, uint min, uint max, cv::ColormapTypes types)
{
	cv::Mat mat(img.rows, img.cols, img.type(), img.pData);

	if (mat.empty())
		return -1;

	cv::Mat out = mat.clone();
	if (out.channels() != 1) {
		cv::cvtColor(out, out, cv::COLOR_BGR2GRAY);
	}
	if (out.depth() == CV_16U) {
		cv::normalize(out, out, 0, 255, cv::NORM_MINMAX, CV_8U);
	}
	pseudoColor(out, min, max, types);
	///���ﲻ����Ļ����ֲ��ڴ�������н���֮�����
	MatToHImage(out, outImage);
	return 0;
}

COLORVISIONCORE_API int M_AutoLevelsAdjust(HImage img, HImage* outImage)
{
	cv::Mat mat(img.rows, img.cols, img.type(), img.pData);

	if (mat.empty())
		return -1;
	if (mat.channels() != 3) {
		return -1;
	}
	cv::Mat out = mat.clone();
	if (out.depth() == CV_16U) {
		cv::normalize(out, out, 0, 255, cv::NORM_MINMAX, CV_8U);
	}
	cv::Mat outMat;
	autoLevelsAdjust(out, outMat);
	out.release();
	MatToHImage(outMat, outImage);
	return 0;
}

COLORVISIONCORE_API int M_AutomaticColorAdjustment(HImage img, HImage* outImage)
{
	cv::Mat mat(img.rows, img.cols, img.type(), img.pData);
	if (mat.empty())
		return -1;
	if (mat.channels() != 3) {
		return -1;
	}
	cv::Mat out = mat.clone();
	if (out.depth() == CV_16U) {
		cv::normalize(out, out, 0, 255, cv::NORM_MINMAX, CV_8U);
	}
	automaticColorAdjustment(out);
	MatToHImage(out, outImage);
	return 0;
}

COLORVISIONCORE_API int M_AutomaticToneAdjustment(HImage img, HImage* outImage)
{
	cv::Mat mat(img.rows, img.cols, img.type(), img.pData);
	if (mat.empty())
		return -1;
	if (mat.channels() != 3) {
		return -1;
	}
	cv::Mat out = mat.clone();
	if (mat.depth() == CV_16U) {
		cv::normalize(out, out, 0, 255, cv::NORM_MINMAX, CV_8U);
	}
	automaticToneAdjustment(out, 1);
	MatToHImage(out, outImage);
	return 0;
}

COLORVISIONCORE_API int M_DrawPoiImage(HImage img, HImage* outImage,int radius, int* point , int pointCount, int thickness)
{
	cv::Mat mat(img.rows, img.cols, img.type(), img.pData);
	if (mat.empty())
		return -1;
	if (mat.channels() != 3) {
		if (mat.channels() == 1) {
			// ����ͨ��ͼ��ת��Ϊ��ͨ��
			cv::cvtColor(mat, mat, cv::COLOR_GRAY2BGR);
		}
	}

	cv::Mat out = mat.clone();
	drawPoiImage(out, out, radius, point, pointCount, thickness);
	MatToHImage(out, outImage);
	return 0;
}

int FindClosestFactor(int value, const int* allowedFactors, int size = 13)
{
	int closestFactor = allowedFactors[0];
	for (int i = 1; i < size; ++i)
	{
		if (std::abs(value - allowedFactors[i]) < std::abs(value - closestFactor))
		{
			closestFactor = allowedFactors[i];
		}
	}
	return closestFactor;
}

COLORVISIONCORE_API int M_ConvertImage(HImage img, HImage* outImage)
{
	cv::Mat mat(img.rows, img.cols, img.type(), img.pData);
	if (mat.empty())
		return -1;

	cv::Mat outMat;

	// ����ǲ�ɫͼ��ת��Ϊ�Ҷ�ͼ
	if (mat.channels() == 3 || mat.channels() == 4)  // �ж��Ƿ�Ϊ��ɫͼ��BGR �� BGRA��
	{
		cv::cvtColor(mat, outMat, cv::COLOR_BGR2GRAY); // ת��Ϊ�Ҷ�ͼ
	}
	else
	{
		mat.convertTo(outMat, CV_8U);  // ����Ѿ��ǻҶ�ͼ����ֱ��ת��
	}

	// Ŀ��ֱ�������
	int targetPixels = 512 * 152; // Ŀ�������������Ե�����
	int originalWidth = outMat.cols;
	int originalHeight = outMat.rows;

	// �����ʼ��������
	double initialScaleFactor = std::sqrt((double)originalWidth * originalHeight / targetPixels);

	// ȷ������������ 1��2��4��8 �ȱ���
	int allowedFactors[] = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048 };
	int scaleFactor = FindClosestFactor((int)std::round(initialScaleFactor), allowedFactors);

	// �����µĿ�Ⱥ͸߶�
	int newWidth = originalWidth / scaleFactor;
	int newHeight = originalHeight / scaleFactor;

	// ����ͼ��
	cv::resize(outMat, outMat, cv::Size(newWidth, newHeight));

	MatToHImage(outMat, outImage);
	return 0;
}

COLORVISIONCORE_API int M_ExtractChannel(HImage img, HImage* outImage, int channel)
{
	cv::Mat mat(img.rows, img.cols, img.type(), img.pData);
	if (mat.empty())
		return -1;
	cv::Mat outMat;
	int i = extractChannel(mat, outMat, channel);
	if (i != 0)
		return i;
	MatToHImage(outMat, outImage);
	return 0;
}
