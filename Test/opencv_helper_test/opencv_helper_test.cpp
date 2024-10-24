﻿// OpenCVHelper_test.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//
#include <chrono>
#include <iostream>
#include <opencv.hpp>
#include <stack>


void LampBeadDetection(cv::Mat image)
{
    cv::Mat image8bit;
    image.convertTo(image8bit, CV_8UC3, 255.0 / 65535.0);

    cv::Mat binary;
    cv::threshold(gray, binary, 20, 255, cv::THRESH_BINARY);

    // 定义结构元素
    cv::Mat binary;
    cv::threshold(gray, binary, 20, 255, cv::THRESH_BINARY);


    cv::Mat gray;
    cv::cvtColor(image8bit, gray, cv::COLOR_BGR2GRAY);

    // 二值化

    // 定义结构元素
    cv::Mat binary;
    cv::threshold(gray, binary, 20, 255, cv::THRESH_BINARY);

    // 腐蚀操作
    cv::erode(binary, binary, cv::getStructuringElement(cv::MORPH_ELLIPSE, cv::Size(2, 2)));

    cv::dilate(binary, binary, cv::getStructuringElement(cv::MORPH_ELLIPSE, cv::Size(4, 4)));
    cv::erode(binary, binary, cv::getStructuringElement(cv::MORPH_ELLIPSE, cv::Size(2, 2)));


    // 检测轮廓
    std::vector<std::vector<cv::Point>> contours;
    cv::findContours(binary, contours, cv::RETR_EXTERNAL, cv::CHAIN_APPROX_SIMPLE);

    std::vector<cv::Point> centers;

    // 遍历轮廓
    for (const auto& contour : contours) {
        // 计算轮廓的边界框
        cv::Rect boundingBox = cv::boundingRect(contour);

        // 根据灯珠的已知大小过滤
        if (boundingBox.width > 2 && boundingBox.width < 20 &&
            boundingBox.height > 2 && boundingBox.height < 20) {

            // 计算中心点
            int cx = boundingBox.x + boundingBox.width / 2;
            int cy = boundingBox.y + boundingBox.height / 2;
            centers.push_back(cv::Point(cx, cy));
        }
        else {
            int coutns = centers.size();
        }
    }

    //总亮点
    int coutns = centers.size();

    // 计算中心点的凸包
    std::vector<cv::Point> hull;
    if (!centers.empty()) {
        cv::convexHull(centers, hull);
    }

    //绘制中心点
    for (const auto& center : centers) {
        cv::circle(image8bit, center, 4, cv::Scalar(255), -1);
    }

    // 绘制凸包
    if (!hull.empty()) {
        for (size_t i = 0; i < hull.size(); ++i) {
            cv::line(image8bit, hull[i], hull[(i + 1) % hull.size()], cv::Scalar(255), 2);
        }
    }

    // 计算凸包的面积
    double area = cv::contourArea(hull);
    std::cout << "Convex Hull Area: " << area << std::endl;

    // 计算凸包的边界矩形
    cv::Rect boundingRect = cv::boundingRect(hull);
    double width = boundingRect.width;
    double height = boundingRect.height;

    double singlewith = width / 850;
    double singleheight = height / 650;

    // 创建一个掩码，初始为全零
    cv::Mat mask = cv::Mat::zeros(image.size(), CV_8UC1);

    // 在掩码上绘制凸包区域
    std::vector<std::vector<cv::Point>> hulls = { hull };
    cv::fillPoly(mask, hulls, cv::Scalar(255));

    // 遍历图像的所有点
    for (int y = 0; y < binary.rows; ++y) {
        uchar* maskRow = mask.ptr<uchar>(y);
        uchar* imgRow = binary.ptr<uchar>(y);
        for (int x = 0; x < binary.cols; ++x) {
            // 如果掩码中该点不在凸包内，则设置为255
            if (maskRow[x] == 0) {
                imgRow[x] = 255;
            }
        }
    }

    //缺少的点
    int black = rows * cols - centers.size();
    std::cout << black << std::endl;


    std::vector<std::vector<cv::Point>> ledMatrix1;
    std::vector<cv::Point> currentRow;

    cv::dilate(binary, binary, cv::getStructuringElement(cv::MORPH_RECT, cv::Size(12, 12)));

    binary = 255 - binary;
    std::vector<cv::Point> blackcenters;

    std::vector<std::vector<cv::Point>> contourless;
    cv::findContours(binary, contourless, cv::RETR_EXTERNAL, cv::CHAIN_APPROX_SIMPLE);

    // 遍历轮廓
    for (const auto& contour : contourless) {
        // 计算轮廓的边界框
        cv::Rect boundingBox = cv::boundingRect(contour);

        // 根据灯珠的已知大小过滤
        if (boundingBox.width > 2 && boundingBox.width < 20 &&
            boundingBox.height > 2 && boundingBox.height < 20) {

            // 计算中心点
            int cx = boundingBox.x + boundingBox.width / 2;
            int cy = boundingBox.y + boundingBox.height / 2;
            blackcenters.push_back(cv::Point(cx, cy));
        }
        else
        {
            // 计算凸包的边界矩形
            cv::Rect boundingRect = cv::boundingRect(contour);

            // 点的间隔和起始偏移
            int offset = 4;

            // 存储在凸包内的点
            std::vector<cv::Point> pointsInsideHull;

            // 在边界矩形内迭代
            for (double y = boundingRect.y + offset; y < boundingRect.y + boundingRect.height; y += singlewith) {
                for (double x = boundingRect.x + offset; x < boundingRect.x + boundingRect.width; x += singleheight) {
                    cv::Point p(x, y);
                    // 检查点是否在凸包内
                    if (cv::pointPolygonTest(contour, p, false) >= 0) {
                        blackcenters.push_back(p);
                    }
                }
            }

        }
    }

    //缺少的点
    std::cout << blackcenters.size() << std::endl;
    for (const auto& contour : blackcenters)
    {
        std::cout << contour << std::endl;
        cv::circle(image8bit, contour, 5, cv::Scalar(0, 0, 255), 1);
    }
    std::cout << blackcenters.size() << std::endl;

}

int main()
{
    int rows = 650;
    int cols = 850;

	std::chrono::steady_clock::time_point start, end;
	std::chrono::microseconds duration;

	cv::Mat image = cv::imread("C:\\Users\\17917\\Documents\\WXWork\\1688854819471931\\Cache\\File\\2024-10\\ledTest-q.tif",cv::ImreadModes::IMREAD_UNCHANGED);

	if (image.empty()) {
		std::cerr << "无法读取图像文件！" << std::endl;
		return -1;
	}
    start = std::chrono::high_resolution_clock::now();


	end = std::chrono::high_resolution_clock::now();
	duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start);
	std::cout << ": " << duration.count() / 1000.0 << " 毫秒" << std::endl;

    cv::imwrite("reee.tif", image8bit);


	//cv::imshow("tif读", image);
	cv::waitKey(0);
}

