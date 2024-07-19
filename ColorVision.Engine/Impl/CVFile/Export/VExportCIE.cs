﻿using ColorVision.Common.MVVM;
using ColorVision.Net;
using ColorVision.RecentFile;
using MQTTMessageLib.FileServer;
using OpenCvSharp;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace ColorVision.Engine.Impl.CVFile.Export
{
    public class VExportCIE : ViewModelBase
    {

        public static void SaveTo(VExportCIE export, Mat src, string fileName)
        {
            fileName = fileName + export.ExportImageFormat.ToString().ToLower(CultureInfo.CurrentCulture);
            if (export.ExportImageFormat == ImageFormat.Tiff)
            {
                src.SaveImage(fileName, new ImageEncodingParam(ImwriteFlags.TiffCompression, 1));
            }
            else if (export.ExportImageFormat == ImageFormat.Bmp)
            {
                src.SaveImage(fileName);
            }
            else if (export.ExportImageFormat == ImageFormat.Png)
            {
                src.SaveImage(fileName, new ImageEncodingParam(ImwriteFlags.PngCompression, 3));
            }
            else if (export.ExportImageFormat == ImageFormat.Jpeg)
            {
                src.SaveImage(fileName, new ImageEncodingParam(ImwriteFlags.JpegQuality, 95));
            }
            else
            {
                src.SaveImage(fileName + "tiff");
            }
        }

        public static int SaveToTif(VExportCIE export)
        {
            string FileName = export.FilePath;
            string SavePath = export.SavePath;
            string Name = export.Name;

            int index = CVFileUtil.ReadCIEFileHeader(FileName, out CVCIEFile cvcie);
            if (index < 0) return -1;
            cvcie.FileExtType = FileName.Contains(".cvraw") ? FileExtType.Raw : FileName.Contains(".cvsrc") ? FileExtType.Src : FileExtType.CIE;

            Mat src;
            switch (cvcie.FileExtType)
            {
                case FileExtType.Raw:
                    if (export.IsExportSrc)
                    {
                        CVFileUtil.ReadCIEFileData(FileName, ref cvcie, index);
                        src = Mat.FromPixelData(cvcie.cols, cvcie.rows, MatType.MakeType(cvcie.Depth, cvcie.channels), cvcie.data);
                        SaveTo(export, src, SavePath + "\\" + Name + "Src.");
                    }
                    break;
                case FileExtType.Src:
                    if (export.IsExportSrc)
                    {
                        CVFileUtil.ReadCIEFileData(FileName, ref cvcie, index);
                        src = Mat.FromPixelData(cvcie.cols, cvcie.rows, MatType.MakeType(cvcie.Depth, cvcie.channels), cvcie.data);
                        SaveTo(export, src, SavePath + "\\" + Name + "Src.");
                    }
                    break;
                case FileExtType.CIE:

                    if (export.IsExportSrc)
                    {
                        cvcie.srcFileName = Path.Combine(Path.GetDirectoryName(FileName) ?? string.Empty, cvcie.srcFileName);
                        if (File.Exists(cvcie.srcFileName))
                        {
                            if (CVFileUtil.Read(cvcie.srcFileName, out CVCIEFile cvraw))
                            {
                                src = Mat.FromPixelData(cvraw.cols, cvraw.rows, MatType.MakeType(cvraw.Depth, cvraw.channels), cvraw.data);
                                SaveTo(export, src, SavePath + "\\" + Name + "_Src.");
                            }
                        }
                    }
                    if (CVFileUtil.ReadCIEFileData(FileName, ref cvcie, index))
                    {
                        if (cvcie.channels == 1)
                        {
                            if (export.IsExportChannelY)
                            {
                                src = Mat.FromPixelData(cvcie.cols, cvcie.rows, MatType.MakeType(MatType.CV_32F, 1), cvcie.data);
                                SaveTo(export, src, SavePath + "\\" + Name + "_Y.");
                            }
                        }
                        else if (cvcie.channels == 3)
                        {
                            int len = cvcie.cols * cvcie.rows * cvcie.bpp / 8;

                            if (export.IsExportChannelX)
                            {
                                byte[] data = new byte[len];
                                Buffer.BlockCopy(cvcie.data, 0 * len, data, 0, len);
                                src = Mat.FromPixelData(cvcie.cols, cvcie.rows, MatType.MakeType(MatType.CV_32F, 1), data);
                                SaveTo(export, src, SavePath + "\\" + Name + "_X.");
                            }
                            if (export.IsExportChannelY)
                            {
                                byte[] data = new byte[len];
                                Buffer.BlockCopy(cvcie.data, 1 * len, data, 0, len);
                                src = Mat.FromPixelData(cvcie.cols, cvcie.rows, MatType.MakeType(MatType.CV_32F, 1), data);
                                SaveTo(export, src, SavePath + "\\" + Name + "_Y.");

                            }
                            if (export.IsExportChannelZ)
                            {
                                byte[] data = new byte[len];
                                Buffer.BlockCopy(cvcie.data, 2 * len, data, 0, len);
                                src = Mat.FromPixelData(cvcie.cols, cvcie.rows, MatType.MakeType(MatType.CV_32F, 1), data);
                                SaveTo(export, src, SavePath + "\\" + Name + "_Z.");
                            }
                        }
                    }
                    break;
                case FileExtType.Calibration:
                    break;
                case FileExtType.Tif:
                    break;
                default:
                    break;
            }
            return 0;
        }

        public VExportCIE(string filePath)
        {
            FilePath = filePath;
            FileExtType = filePath.Contains(".cvraw") ? FileExtType.Raw : filePath.Contains(".cvsrc") ? FileExtType.Src : FileExtType.CIE;
            if (FileExtType == FileExtType.CIE)
            {
                IsExportChannelX = true;
                IsExportChannelY = true;
                IsExportChannelZ = true;

                if (CVFileUtil.ReadCIEFileHeader(filePath, out CVCIEFile cVCIEFile) > 0 && !string.IsNullOrEmpty(cVCIEFile.srcFileName))
                {
                    if (!File.Exists(cVCIEFile.srcFileName))
                        cVCIEFile.srcFileName = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, Path.GetFileNameWithoutExtension(filePath) + ".cvraw");
                    if (File.Exists(cVCIEFile.srcFileName))
                    {
                        IsCanExportSrc = CVFileUtil.ReadCIEFileHeader(cVCIEFile.srcFileName, out _CVCIEFile) > 0;
                        IsChannelOne = _CVCIEFile.channels == 0;
                    }
                }

            }
            else if (FileExtType == FileExtType.Raw)
            {
                IsCanExportSrc = CVFileUtil.ReadCIEFileHeader(filePath, out _CVCIEFile) > 0;
                IsChannelOne = CVCIEFile.channels == 0;
            }

            if (RecentImage.RecentFiles.Count == 0)
            {
                RecentImage.InsertFile(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            }
            SavePath = RecentImage.RecentFiles[0];
            RecentImageSaveList = new ObservableCollection<string>(RecentImage.RecentFiles);

            Name = Path.GetFileNameWithoutExtension(FilePath);
        }
        public int Rows { get => _CVCIEFile.rows; }
        public int Cols { get => _CVCIEFile.cols; }
        public int Channels { get => _CVCIEFile.channels; }
        public int Gain { get => _CVCIEFile.gain; }
        public float[] Exp { get => _CVCIEFile.exp; }

        public ImageFormat ExportImageFormat { get; set; } = ImageFormat.Tiff;

        public CVCIEFile CVCIEFile { get => _CVCIEFile; }
        private CVCIEFile _CVCIEFile;

        public RecentFileList RecentImage { get; set; } = new RecentFileList() { Persister = new RegistryPersister("Software\\ColorVision\\RecentImageSaveList") };

        public ObservableCollection<string> RecentImageSaveList { get; set; }
        public bool IsCVRaw { get => FileExtType == FileExtType.Raw; }

        public bool IsCVCIE { get => FileExtType == FileExtType.CIE; }

        public bool IsChannelOne { get; set; }


        public string FilePath { get => _FilePath; set { _FilePath = value; NotifyPropertyChanged(); } }
        private string _FilePath;

        public string Name { get => _Name; set { _Name = value; NotifyPropertyChanged(); } }
        private string _Name;

        public string SavePath { get => _SavePath; set { _SavePath = value; NotifyPropertyChanged(); } }
        private string _SavePath;

        public FileExtType FileExtType { get => _FileExtType; set { _FileExtType = value; NotifyPropertyChanged(); } }
        private FileExtType _FileExtType;
        public bool IsExportChannelX { get => _IsExportChannelX; set { _IsExportChannelX = value; NotifyPropertyChanged(); } }
        private bool _IsExportChannelX;

        public bool IsExportChannelY { get => _IsExportChannelY; set { _IsExportChannelY = value; NotifyPropertyChanged(); } }
        private bool _IsExportChannelY;

        public bool IsExportChannelZ { get => _IsExportChannelZ; set { _IsExportChannelZ = value; NotifyPropertyChanged(); } }
        private bool _IsExportChannelZ;

        public bool IsCanExportSrc { get; set; }

        public bool IsExportSrc { get => _IsExportSrc; set { _IsExportSrc = value; NotifyPropertyChanged(); } }
        private bool _IsExportSrc = true;
        public bool IsExportChannelR { get => _IsExportChannelR; set { _IsExportChannelR = value; NotifyPropertyChanged(); } }
        private bool _IsExportChannelR;

        public bool IsExportChannelG { get => _IsExportChannelG; set { _IsExportChannelG = value; NotifyPropertyChanged(); } }
        private bool _IsExportChannelG;

        public bool IsExportChannelB { get => _IsExportChannelB; set { _IsExportChannelB = value; NotifyPropertyChanged(); } }
        private bool _IsExportChannelB;
    }
}
