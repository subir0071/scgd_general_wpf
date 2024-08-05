﻿using ColorVision.Common.Utilities;
using ColorVision.Net;
using ColorVision.Themes.Controls;
using MQTTMessageLib.FileServer;
using OpenCvSharp.WpfExtensions;
using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorVision.Engine.Media
{
    public static class MediaHelper
    {
        public static bool UpdateWriteableBitmap(this CVCIEFile fileInfo, WriteableBitmap writeableBitmap)
        {
            OpenCvSharp.Mat? src = null;
            OpenCvSharp.Mat? dst = null;
            try
            {
                if (fileInfo.FileExtType == FileExtType.Tif)
                {
                    src = OpenCvSharp.Cv2.ImDecode(fileInfo.data, OpenCvSharp.ImreadModes.Unchanged);
                }
                else if (fileInfo.FileExtType == FileExtType.Raw || fileInfo.FileExtType == FileExtType.Src || fileInfo.FileExtType == FileExtType.CIE)
                {
                    src = OpenCvSharp.Mat.FromPixelData(fileInfo.cols, fileInfo.rows, OpenCvSharp.MatType.MakeType(fileInfo.Depth, fileInfo.channels), fileInfo.data);
                    if (fileInfo.bpp == 32)
                    {
                        OpenCvSharp.Cv2.Normalize(src, src, 0, 255, OpenCvSharp.NormTypes.MinMax);
                        dst = new OpenCvSharp.Mat();
                        src.ConvertTo(dst, OpenCvSharp.MatType.CV_8U);
                    }
                    else
                    {
                        dst = src;
                    }
                }

                if (src == null)
                {
                    return false;
#pragma warning disable CA2201 // 不要引发保留的异常类型
                    throw new Exception("Unsupported file format.");
#pragma warning restore CA2201 // 不要引发保留的异常类型
                }

                if (dst == null)
                {
                    dst = src;
                }

                if (writeableBitmap.PixelWidth != dst.Width ||
                    writeableBitmap.PixelHeight != dst.Height ||
                    writeableBitmap.Format != GetPixelFormat(dst))
                {
                    return false;
                    throw new InvalidOperationException("The existing WriteableBitmap does not match the dimensions or format of the new image.");
                }

                writeableBitmap.Lock();
                unsafe
                {
                    byte* srcPtr = (byte*)dst.Data;
                    byte* dstPtr = (byte*)writeableBitmap.BackBuffer;

                    for (int y = 0; y < dst.Rows; y++)
                    {
                        Buffer.MemoryCopy(srcPtr, dstPtr, writeableBitmap.BackBufferStride, dst.Cols * dst.ElemSize());
                        srcPtr += dst.Step();
                        dstPtr += writeableBitmap.BackBufferStride;
                    }
                }
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, dst.Cols, dst.Rows));
                writeableBitmap.Unlock();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                src?.Dispose();
                dst?.Dispose();
            }
        }


        private static PixelFormat GetPixelFormat(OpenCvSharp.Mat mat)
        {
            switch (mat.Channels())
            {
                case 1:
                    return PixelFormats.Gray8;
                case 3:
                    return PixelFormats.Bgr24;
                case 4:
                    return PixelFormats.Bgra32;
                default:
                    throw new NotSupportedException("Unsupported image format.");
            }
        }

        public static WriteableBitmap? ToWriteableBitmap(this CVCIEFile fileInfo)
        {
            OpenCvSharp.Mat? src = null;
            OpenCvSharp.Mat? dst = null;
            try
            {
                if (fileInfo.FileExtType == FileExtType.Tif)
                {
                    src = OpenCvSharp.Cv2.ImDecode(fileInfo.data, OpenCvSharp.ImreadModes.Unchanged);
                }
                else if (fileInfo.FileExtType == FileExtType.Raw || fileInfo.FileExtType == FileExtType.Src || fileInfo.FileExtType == FileExtType.CIE)
                {
                    src = OpenCvSharp.Mat.FromPixelData(fileInfo.cols, fileInfo.rows, OpenCvSharp.MatType.MakeType(fileInfo.Depth, fileInfo.channels), fileInfo.data);
                    if (fileInfo.bpp == 32)
                    {
                        OpenCvSharp.Cv2.Normalize(src, src, 0, 255, OpenCvSharp.NormTypes.MinMax);
                        dst = new OpenCvSharp.Mat();
                        src.ConvertTo(dst, OpenCvSharp.MatType.CV_8U);
                    }
                    else
                    {
                        dst = src;
                    }
                }

                if (src == null)
                {
#pragma warning disable CA2201 // 不要引发保留的异常类型
                    throw new Exception("Unsupported file format.");
#pragma warning restore CA2201 // 不要引发保留的异常类型
                }

                if (dst == null)
                {
                    dst = src;
                }

                var writeableBitmap = dst.ToWriteableBitmap();
                return writeableBitmap;
            }
            catch (Exception ex)
            {
                MessageBox1.Show(Application.Current.GetActiveWindow(), $"打开文件失败:{ex.Message} ", "ColorVision");
                return null;
            }
            finally
            {
                src?.Dispose();
                dst?.Dispose();
            }
        }

    }
}
