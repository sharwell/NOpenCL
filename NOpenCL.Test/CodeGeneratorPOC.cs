// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NOpenCL.Generator;
    using TODO;
    using Buffer = NOpenCL.Buffer;

    [TestClass]
    public class CodeGeneratorConcept
    {
        [TestMethod]
        public async Task ProofOfConceptAsync()
        {
            var solutionFilePath = @"J:\dev\github\sharwell\NOpenCL\NOpenCL.sln";
            var codeGenerator = await OpenCLCodeGenerator.CreateAsync(solutionFilePath, CancellationToken.None).ConfigureAwait(false);
            await codeGenerator.GenerateCodeForProjectAsync(@"J:\dev\github\sharwell\NOpenCL\NOpenCL.Test\NOpenCL.Test.csproj", CancellationToken.None).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task RunConceptAsync()
        {
            float[] inputData = ReadInput(out int width, out int height);
            uint blend = 1;
            float[] outputData = new float[4 * width * height];

            Platform platform = Platform.GetPlatforms()[0];
            Device device = platform.GetDevices().First();
            using (Context context = Context.Create(device))
            using (var harness = new GodRaysGpu(context))
            using (CommandQueue queue = context.CreateCommandQueue(device))
            using (Buffer inputImage = context.CreateBuffer(MemoryFlags.ReadOnly, System.Buffer.ByteLength(outputData)))
            using (Buffer output = context.CreateBuffer(MemoryFlags.WriteOnly, System.Buffer.ByteLength(outputData)))
            {
                const int BlockDim = 64;
                const int GodRaysBunchSize = 1;

                IntPtr[] globalWorkOffset = null;
                IntPtr[] globalWorkSize = { (IntPtr)((2 * (width + height - 2) / GodRaysBunchSize) + 1) };
                IntPtr[] localWorkSize = { (IntPtr)BlockDim };
                Console.WriteLine($"Original global work size {globalWorkSize[0]}");
                Console.WriteLine($"Original local work size {localWorkSize[0]}");
                globalWorkSize[0] = (IntPtr)(((long)globalWorkSize[0] + (long)(localWorkSize[0] - 1)) & ~(long)(localWorkSize[0] - 1));
                Console.WriteLine($"Corrected global work size {globalWorkSize[0]}");

                var inputHandle = GCHandle.Alloc(inputData, GCHandleType.Pinned);
                var handle = GCHandle.Alloc(outputData, GCHandleType.Pinned);

                try
                {
                    using (ComputeSynchronizationContext.Push())
                    {
                        await queue.WriteBufferAsync(inputImage, 0, sizeof(float) * 4 * width * height, inputHandle.AddrOfPinnedObject());
                        await ConceptImplAsync(harness, queue, globalWorkOffset, globalWorkSize, localWorkSize, handle.AddrOfPinnedObject(), inputImage, output, width, height, blend);
                    }

                    queue.Finish();
                }
                finally
                {
                    handle.Free();
                    inputHandle.Free();
                }

                Console.WriteLine(Path.GetFullPath("GodRaysInput.bmp"));
                SaveImageAsBmp32FC4(inputData, 255.0f, width, height, "GodRaysInput.bmp");
                SaveImageAsBmp32FC4(outputData, 255.0f, width, height, "GodRaysOutput.bmp");
            }
        }

        private static void SaveImageAsBmp32FC4(float[] p_buf, float scale, int width, int height, string fileName)
        {
            int array_pitch = width;
            uint[] outUIntBuf = new uint[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float fTmpFVal = 0.0f;

                    // Ensure that no value is greater than 255.0
                    uint[] uiTmp = new uint[4];
                    fTmpFVal = scale * p_buf[(((y * array_pitch) + x) * 4) + 0];
                    fTmpFVal = Math.Max(0.0f, Math.Min(255.0f, fTmpFVal));
                    uiTmp[0] = (uint)fTmpFVal;

                    fTmpFVal = scale * p_buf[(((y * array_pitch) + x) * 4) + 1];
                    fTmpFVal = Math.Max(0.0f, Math.Min(255.0f, fTmpFVal));
                    uiTmp[1] = (uint)fTmpFVal;

                    fTmpFVal = scale * p_buf[(((y * array_pitch) + x) * 4) + 2];
                    fTmpFVal = Math.Max(0.0f, Math.Min(255.0f, fTmpFVal));
                    uiTmp[2] = (uint)fTmpFVal;

                    fTmpFVal = scale * p_buf[(((y * width) + x) * 4) + 3];
                    fTmpFVal = Math.Max(0.0f, Math.Min(255.0f, fTmpFVal));
                    uiTmp[3] = 1;    // Alfa

                    outUIntBuf[((height - 1 - y) * width) + x] = 0x000000FF & uiTmp[2];
                    outUIntBuf[((height - 1 - y) * width) + x] |= 0x0000FF00 & (uiTmp[1] << 8);
                    outUIntBuf[((height - 1 - y) * width) + x] |= 0x00FF0000 & (uiTmp[0] << 16);
                    outUIntBuf[((height - 1 - y) * width) + x] |= 0xFF000000 & (uiTmp[3] << 24);
                }
            }

            SaveImageAsBmp(outUIntBuf, width, height, fileName);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BitmapFileHeader
        {
            public ushort bfType;
            public uint bfSize;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BitmapInfoHeader
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        private static void SaveImageAsBmp(uint[] outUIntBuf, int width, int height, string fileName)
        {
            using (var writer = new BinaryWriter(File.OpenWrite(fileName)))
            {
                var fileHeader = default(BitmapFileHeader);
                var infoHeader = default(BitmapInfoHeader);

                int alignSize = width * 4;
                alignSize ^= 0x03;
                alignSize++;
                alignSize &= 0x03;

                int rowLength = (width * 4) + alignSize;

                infoHeader.biSize = (uint)Marshal.SizeOf(typeof(BitmapInfoHeader));
                infoHeader.biWidth = width;
                infoHeader.biHeight = height;
                infoHeader.biPlanes = 1;
                infoHeader.biBitCount = 32;
                infoHeader.biCompression = 0U; // BI_RGB;
                infoHeader.biSizeImage = (uint)(rowLength * height);
                infoHeader.biXPelsPerMeter = 0;
                infoHeader.biYPelsPerMeter = 0;
                infoHeader.biClrUsed = 0; // max available
                infoHeader.biClrImportant = 0; // !!!

                fileHeader.bfType = 0x4D42;
                fileHeader.bfSize = (uint)(Marshal.SizeOf(typeof(BitmapFileHeader)) + Marshal.SizeOf(typeof(BitmapInfoHeader)) + (rowLength * height));
                fileHeader.bfOffBits = (uint)(Marshal.SizeOf(typeof(BitmapFileHeader)) + Marshal.SizeOf(typeof(BitmapInfoHeader)));

                unsafe
                {
                    byte[] data = new byte[sizeof(BitmapFileHeader) + sizeof(BitmapInfoHeader)];
                    fixed (byte* ptr = data)
                    {
                        *(BitmapFileHeader*)ptr = fileHeader;
                        *(BitmapInfoHeader*)(ptr + sizeof(BitmapFileHeader)) = infoHeader;
                    }

                    writer.Write(data);

                    fixed (uint* ptr = outUIntBuf)
                    {
                        int* ppix = (int*)ptr;
                        byte[] buffer = new byte[4];
                        fixed (byte* bufferPtr = buffer)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++, ppix++)
                                {
                                    *(int*)bufferPtr = *ppix;
                                    writer.Write(bufferPtr[0]);
                                    writer.Write(bufferPtr[1]);
                                    writer.Write(bufferPtr[2]);
                                    writer.Write(bufferPtr[3]);
                                }
                            }

                            *(int*)bufferPtr = 0;
                            writer.Write(buffer, 0, alignSize);
                        }
                    }
                }
            }
        }

        private float[] ReadInput(out int width, out int height)
        {
            byte[] data = TestResources.GodRays;
            width = BitConverter.ToInt32(data, 0);
            height = BitConverter.ToInt32(data, sizeof(int));

            float[] image = new float[4 * width * height];
            System.Buffer.BlockCopy(data, 2 * sizeof(int), image, 0, System.Buffer.ByteLength(image));
            return image;
        }

        private async EventTask ConceptImplAsync(GodRaysGpu harness, CommandQueue commandQueue, IntPtr[] globalWorkOffset, IntPtr[] globalWorkSize, IntPtr[] localWorkSize, IntPtr outputData, Buffer inputImage, Buffer output, int width, int height, uint blend)
        {
            await harness.GodRays(commandQueue, globalWorkOffset, globalWorkSize, localWorkSize, inputImage, output, width, height, blend);
            await commandQueue.ReadBufferAsync(output, 0, sizeof(float) * 4 * width * height, outputData);
        }
    }
}
