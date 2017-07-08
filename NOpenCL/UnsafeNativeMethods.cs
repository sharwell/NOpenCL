// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Runtime.InteropServices;
    using NOpenCL.SafeHandles;

    internal static partial class UnsafeNativeMethods
    {
        /// <summary>
        /// Helper method to ensure the proper value is passed for the <em>num&lt;items&gt;</em>
        /// argument of several API methods.
        /// </summary>
        /// <param name="items">The list of items, or <c>null</c> if no items are specified.</param>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <returns>The length of <paramref name="items"/>, or 0 if <paramref name="items"/> is <c>null</c>.</returns>
        private static uint GetNumItems<T>(T[] items)
        {
            if (items == null)
                return 0;

            return (uint)items.Length;
        }

        /// <summary>
        /// Helper method to ensure the proper value is passed for the <em>&lt;items&gt;</em>
        /// argument of several API methods.
        /// </summary>
        /// <remarks>
        /// This method allows the user to pass a non-null but empty list of items to the
        /// managed API wrapper methods, which is not allowed by the underlying OpenCL API.
        /// In this case, the list is treated as though the user passed <c>null</c> instead.
        /// </remarks>
        /// <param name="items">The list of items, or <c>null</c> if no items are specified.</param>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <returns><paramref name="items"/> if it contains at least one item, otherwise <c>null</c>.</returns>
        private static T[] GetItems<T>(T[] items)
        {
            if (items == null || items.Length == 0)
                return null;

            return items;
        }

        private struct VoidResult
        {
        }

        public abstract class ParameterInfo<T>
        {
            private readonly int _name;

            protected ParameterInfo(int name)
            {
                _name = name;
            }

            public int Name
            {
                get
                {
                    return _name;
                }
            }

            public virtual int? FixedSize
            {
                get
                {
                    return null;
                }
            }

            public abstract T Deserialize(UIntPtr memorySize, IntPtr memory);
        }

        public sealed class ParameterInfoString : ParameterInfo<string>
        {
            public ParameterInfoString(int name)
                : base(name)
            {
            }

            public override string Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                return Marshal.PtrToStringAnsi(memory, (int)memorySize.ToUInt32() - 1);
            }
        }

        public sealed class ParameterInfoBoolean : ParameterInfo<bool>
        {
            public ParameterInfoBoolean(int name)
                : base(name)
            {
            }

            public override int? FixedSize
            {
                get
                {
                    return sizeof(int);
                }
            }

            public override bool Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                if ((int)memorySize.ToUInt32() != FixedSize)
                    throw new InvalidOperationException();

                return Marshal.ReadInt32(memory) != 0;
            }
        }

        public sealed class ParameterInfoInt32 : ParameterInfo<int>
        {
            public ParameterInfoInt32(int name)
                : base(name)
            {
            }

            public override int? FixedSize
            {
                get
                {
                    return sizeof(int);
                }
            }

            public override int Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                if ((int)memorySize.ToUInt32() != FixedSize)
                    throw new InvalidOperationException();

                return Marshal.ReadInt32(memory);
            }
        }

        public sealed class ParameterInfoInt64 : ParameterInfo<long>
        {
            public ParameterInfoInt64(int name)
                : base(name)
            {
            }

            public override int? FixedSize
            {
                get
                {
                    return sizeof(long);
                }
            }

            public override long Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                if ((int)memorySize.ToUInt32() != FixedSize)
                    throw new InvalidOperationException();

                return Marshal.ReadInt64(memory);
            }
        }

        public sealed class ParameterInfoIntPtr : ParameterInfo<IntPtr>
        {
            public ParameterInfoIntPtr(int name)
                : base(name)
            {
            }

            public override int? FixedSize
            {
                get
                {
                    return IntPtr.Size;
                }
            }

            public override IntPtr Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                if ((int)memorySize.ToUInt32() != FixedSize)
                    throw new InvalidOperationException();

                return Marshal.ReadIntPtr(memory);
            }
        }

        public sealed class ParameterInfoIntPtrArray : ParameterInfo<IntPtr[]>
        {
            public ParameterInfoIntPtrArray(int name)
                : base(name)
            {
            }

            public override IntPtr[] Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                IntPtr[] array = new IntPtr[(int)((long)memorySize.ToUInt64() / IntPtr.Size)];
                Marshal.Copy(memory, array, 0, array.Length);
                return array;
            }
        }

        public sealed class ParameterInfoUInt32 : ParameterInfo<uint>
        {
            public ParameterInfoUInt32(int name)
                : base(name)
            {
            }

            public override int? FixedSize
            {
                get
                {
                    return sizeof(uint);
                }
            }

            public override uint Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                if ((int)memorySize.ToUInt32() != FixedSize)
                    throw new InvalidOperationException();

                return (uint)Marshal.ReadInt32(memory);
            }
        }

        public sealed class ParameterInfoUInt64 : ParameterInfo<ulong>
        {
            public ParameterInfoUInt64(int name)
                : base(name)
            {
            }

            public override int? FixedSize
            {
                get
                {
                    return sizeof(ulong);
                }
            }

            public override ulong Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                if ((int)memorySize.ToUInt32() != FixedSize)
                    throw new InvalidOperationException();

                return (ulong)Marshal.ReadInt64(memory);
            }
        }

        public sealed class ParameterInfoUIntPtr : ParameterInfo<UIntPtr>
        {
            public ParameterInfoUIntPtr(int name)
                : base(name)
            {
            }

            public override int? FixedSize
            {
                get
                {
                    return UIntPtr.Size;
                }
            }

            public override UIntPtr Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                if ((int)memorySize.ToUInt32() != FixedSize)
                    throw new InvalidOperationException();

                return (UIntPtr)(ulong)(long)Marshal.ReadIntPtr(memory);
            }
        }

        public sealed class ParameterInfoUIntPtrArray : ParameterInfo<UIntPtr[]>
        {
            public ParameterInfoUIntPtrArray(int name)
                : base(name)
            {
            }

            public override UIntPtr[] Deserialize(UIntPtr memorySize, IntPtr memory)
            {
                IntPtr[] array = new IntPtr[(int)((long)memorySize.ToUInt64() / IntPtr.Size)];
                Marshal.Copy(memory, array, 0, array.Length);
                UIntPtr[] result = new UIntPtr[array.Length];
                System.Buffer.BlockCopy(array, 0, result, 0, array.Length * IntPtr.Size);
                return result;
            }
        }

        public enum ErrorCode
        {
            Success = 0,

            DeviceNotFound = -1,
            DeviceNotAvailable = -2,
            CompilerNotAvailable = -3,
            MemObjectAllocationFailure = -4,
            OutOfResources = -5,
            OutOfHostMemory = -6,
            ProfilingInfoNotAvailable = -7,
            MemCopyOverlap = -8,
            ImageFormatMismatch = -9,
            ImageFormatNotSupported = -10,
            BuildProgramFailure = -11,
            MapFailure = -12,
            MisalignedSubBufferOffset = -13,
            ExecStatusErrorForEventsInWaitList = -14,
            CompileProgramFailure = -15,
            LinkerNotAvailable = -16,
            LinkProgramFailure = -17,
            DevicePartitionFailed = -18,
            KernelArgInfoNotAvailable = -19,

            InvalidValue = -30,
            InvalidDeviceType = -31,
            InvalidPlatform = -32,
            InvalidDevice = -33,
            InvalidContext = -34,
            InvalidQueueProperties = -35,
            InvalidCommandQueue = -36,
            InvalidHostPtr = -37,
            InvalidMemObject = -38,
            InvalidImageFormatDescriptor = -39,
            InvalidImageSize = -40,
            InvalidSampler = -41,
            InvalidBinary = -42,
            InvalidBuildOptions = -43,
            InvalidProgram = -44,
            InvalidProgramExecutable = -45,
            InvalidKernelName = -46,
            InvalidKernelDefinition = -47,
            InvalidKernel = -48,
            InvalidArgIndex = -49,
            InvalidArgValue = -50,
            InvalidArgSize = -51,
            InvalidKernelArgs = -52,
            InvalidWorkDimension = -53,
            InvalidWorkGroupSize = -54,
            InvalidWorkItemSize = -55,
            InvalidGlobalOffset = -56,
            InvalidEventWaitList = -57,
            InvalidEvent = -58,
            InvalidOperation = -59,
            InvalidGlObject = -60,
            InvalidBufferSize = -61,
            InvalidMipLevel = -62,
            InvalidGlobalWorkSize = -63,
            InvalidProperty = -64,
            InvalidImageDescriptor = -65,
            InvalidCompilerOptions = -66,
            InvalidLinkerOptions = -67,
            InvalidDevicePartitionCount = -68,
        }
    }
}
