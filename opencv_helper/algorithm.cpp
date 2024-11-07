
#include "pch.h"
#include "algorithm.h"
#include <iostream>  
#include <opencv2/core/core.hpp>  
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include "spdlog/spdlog.h"

#include <vector>
#include <algorithm>
#include <ctime>
using namespace cv;

int drawPoiImage(cv::Mat& img, cv::Mat& dst, int radius, int* points, int pointCount,int thickness)
{
    // ��������ͼ���cv::Mat
    // ����ͼ�񣬻���Բ��
    for (int i = 0; i < pointCount/2; ++i)
    {
        int x = points[i * 2];
        int y = points[i * 2 + 1];
        cv::circle(img, cv::Point(x, y), radius, cv::Scalar(0, 0, 255), thickness);
    }
    dst = img;
    return 0; // �ɹ�
}


int extractChannel(cv::Mat& input, cv::Mat& dst ,int channel)
{
    if (input.empty())
        return -1;
    if (channel < 0 || input.channels() < channel)
        return -2;
    // ���ͨ��

    std::vector<cv::Mat> channels;
    cv::split(input, channels);

    cv::Mat redChannel = channels[channel];

    // ������ͨ��ͼ�� (�Ҷ�ͼ��)
    //cv::Mat grayImage;
    //std::vector<cv::Mat> grayChannels = { redChannel, redChannel, redChannel };
    //cv::merge(grayChannels, grayImage);
	dst = redChannel;
    return 0;
}

int pseudoColor(cv::Mat& image, uint min1, uint max1, cv::ColormapTypes types)
{
    if (image.empty())
        return -1;

    if (image.channels() != 1) {
        cv::cvtColor(image, image, cv::COLOR_BGR2GRAY);
    }
    if (image.depth() == CV_16U) {
        cv::normalize(image, image, 0, 255, cv::NORM_MINMAX, CV_8U);
    }


    // ת��Ϊ8λͼ��
    double minVal, maxVal;
    cv::minMaxLoc(image, &minVal, &maxVal); // �ҵ�ͼ�����С���������ֵ
    image.convertTo(image, CV_8UC1, 255.0 / (maxVal - minVal), -minVal * 255.0 / (maxVal - minVal));

    cv::Mat maskGreaterZero = image == 0; // Change maxVal to your specific threshold

    if (max1 < 255) {
        cv::Mat maskGreater = image > max1; // Change maxVal to your specific threshold
        image.setTo(cv::Scalar(255, 255, 255), maskGreater);
    }
    if (min1 > 0) {
        // Set values less than a threshold to black
        cv::Mat maskLess = image < min1; // Change minVal to your specific threshold
        image.setTo(cv::Scalar(0, 0, 0), maskLess);
    }

    cv::applyColorMap(image, image, types);
    image.setTo(cv::Scalar(0, 0, 0), maskGreaterZero);
    return 0;
}

void AdjustWhiteBalance(const cv::Mat& src, cv::Mat& dst, float redBalance, float greenBalance, float blueBalance) {
    // Split the source image into BGR channels
    std::vector<cv::Mat> channels(3);
    cv::split(src, channels);

    // Apply balance parameters to each channel
    channels[2] *= redBalance;    // Red channel
    channels[1] *= greenBalance;  // Green channel
    channels[0] *= blueBalance;   // Blue channel

    // Merge the channels back into the destination image
    cv::merge(channels, dst);

    // Clip values to the appropriate range
    double maxVal = (src.depth() == CV_8U) ? 255.0 : 65535.0;
    cv::threshold(dst, dst, maxVal, maxVal, cv::THRESH_TRUNC);
}



void autoLevelsAdjust(cv::Mat& src, cv::Mat& dst)
{
    CV_Assert(!src.empty() && src.channels() == 3);
    spdlog::info("AutoLevelsAdjust");

    //ͳ�ƻҶ�ֱ��ͼ
    int BHist[256] = { 0 };    //B����
    int GHist[256] = { 0 };    //G����
    int RHist[256] = { 0 };    //R����
    cv::MatIterator_<Vec3b> its, ends;
    for (its = src.begin<Vec3b>(), ends = src.end<Vec3b>(); its != ends; its++)
    {
        BHist[(*its)[0]]++;
        GHist[(*its)[1]]++;
        RHist[(*its)[2]]++;
    }

    //����LowCut��HighCut
    float LowCut = 0.4;
    float HighCut = 0.4;

    //����LowCut��HighCut����ÿ��ͨ�����ֵ��Сֵ
    int BMax = 0, BMin = 0;
    int GMax = 0, GMin = 0;
    int RMax = 0, RMin = 0;

    int TotalPixels = src.cols * src.rows;
    float LowTh = LowCut * 0.01 * TotalPixels;
    float HighTh = HighCut * 0.01 * TotalPixels;

    //Bͨ��������С���ֵ
    int sumTempB = 0;
    for (int i = 0; i < 256; i++)
    {
        sumTempB += BHist[i];
        if (sumTempB >= LowTh)
        {
            BMin = i;
            break;
        }
    }
    sumTempB = 0;
    for (int i = 255; i >= 0; i--)
    {
        sumTempB += BHist[i];
        if (sumTempB >= HighTh)
        {
            BMax = i;
            break;
        }
    }

    //Gͨ��������С���ֵ
    int sumTempG = 0;
    for (int i = 0; i < 256; i++)
    {
        sumTempG += GHist[i];
        if (sumTempG >= LowTh)
        {
            GMin = i;
            break;
        }
    }
    sumTempG = 0;
    for (int i = 255; i >= 0; i--)
    {
        sumTempG += GHist[i];
        if (sumTempG >= HighTh)
        {
            GMax = i;
            break;
        }
    }

    //Rͨ��������С���ֵ
    int sumTempR = 0;
    for (int i = 0; i < 256; i++)
    {
        sumTempR += RHist[i];
        if (sumTempR >= LowTh)
        {
            RMin = i;
            break;
        }
    }
    sumTempR = 0;
    for (int i = 255; i >= 0; i--)
    {
        sumTempR += RHist[i];
        if (sumTempR >= HighTh)
        {
            RMax = i;
            break;
        }
    }

    //��ÿ��ͨ�������ֶ����Բ��ұ�
    //B�������ұ�
    int BTable[256] = { 0 };
    for (int i = 0; i < 256; i++)
    {
        if (i <= BMin)
            BTable[i] = 0;
        else if (i > BMin && i < BMax)
            BTable[i] = cvRound((float)(i - BMin) / (BMax - BMin) * 255);
        else
            BTable[i] = 255;
    }

    //G�������ұ�
    int GTable[256] = { 0 };
    for (int i = 0; i < 256; i++)
    {
        if (i <= GMin)
            GTable[i] = 0;
        else if (i > GMin && i < GMax)
            GTable[i] = cvRound((float)(i - GMin) / (GMax - GMin) * 255);
        else
            GTable[i] = 255;
    }

    //R�������ұ�
    int RTable[256] = { 0 };
    for (int i = 0; i < 256; i++)
    {
        if (i <= RMin)
            RTable[i] = 0;
        else if (i > RMin && i < RMax)
            RTable[i] = cvRound((float)(i - RMin) / (RMax - RMin) * 255);
        else
            RTable[i] = 255;
    }

    //��ÿ��ͨ������Ӧ�Ĳ��ұ���зֶ���������
    cv::Mat dst_ = src.clone();
    cv::MatIterator_<Vec3b> itd, endd;
    for (itd = dst_.begin<Vec3b>(), endd = dst_.end<Vec3b>(); itd != endd; itd++)
    {
        (*itd)[0] = BTable[(*itd)[0]];
        (*itd)[1] = GTable[(*itd)[1]];
        (*itd)[2] = RTable[(*itd)[2]];
    }
    dst = dst_;
}



void automaticColorAdjustment(cv::Mat& image) {
    cv::Mat lab_image;
    cv::cvtColor(image, lab_image, cv::COLOR_BGR2Lab);

    std::vector<cv::Mat> lab_planes(3);
    cv::split(lab_image, lab_planes);

    double avg_a = cv::mean(lab_planes[1])[0];
    double avg_b = cv::mean(lab_planes[2])[0];

    lab_planes[1] = lab_planes[1] - ((avg_a - 128) * (lab_planes[0] / 255.0) * 1.1);
    lab_planes[2] = lab_planes[2] - ((avg_b - 128) * (lab_planes[0] / 255.0) * 1.1);

    cv::merge(lab_planes, lab_image);
    cv::cvtColor(lab_image, image, cv::COLOR_Lab2BGR);
}


void automaticToneAdjustment(cv::Mat& image, double clip_hist_percent) {
    cv::Mat gray;
    cv::cvtColor(image, gray, cv::COLOR_BGR2GRAY);

    int hist_size = 256;
    float range[] = { 0, 256 };
    const float* hist_range = { range };
    cv::Mat hist;

    cv::calcHist(&gray, 1, 0, cv::Mat(), hist, 1, &hist_size, &hist_range);

    std::vector<float> accumulator(hist_size);
    accumulator[0] = hist.at<float>(0);
    for (int i = 1; i < hist_size; i++) {
        accumulator[i] = accumulator[i - 1] + hist.at<float>(i);
    }

    float max_value = accumulator.back();
    clip_hist_percent *= (max_value / 100.0);
    clip_hist_percent /= 2.0;

    int min_gray = 0;
    while (accumulator[min_gray] < clip_hist_percent) {
        min_gray++;
    }

    int max_gray = hist_size - 1;
    while (accumulator[max_gray] >= (max_value - clip_hist_percent)) {
        max_gray--;
    }

    double alpha = 255.0 / (max_gray - min_gray);
    double beta = -min_gray * alpha;

    image.convertTo(image, -1, alpha, beta);
}

