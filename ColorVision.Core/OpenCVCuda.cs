﻿#pragma warning disable CA1401,CA1051,CA2101,CA1707
using System.Runtime.InteropServices;

namespace ColorVision
{
    public static class OpenCVCuda
    {
        private const string LibPath = "libs\\opencv_cuda.dll";


        [DllImport(LibPath)]
        public static extern int CM_Fusion(string fusionjson, out HImage hImage);

    }
}
