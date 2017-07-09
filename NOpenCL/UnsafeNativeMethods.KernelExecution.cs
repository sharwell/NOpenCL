// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Runtime.InteropServices;
    using NOpenCL.SafeHandles;

    /// <content>
    /// Executing kernels.
    /// </content>
    internal partial class UnsafeNativeMethods
    {
        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueNDRangeKernel(
            CommandQueueSafeHandle commandQueue,
            KernelSafeHandle kernel,
            uint workDim,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] globalWorkOffset,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] globalWorkSize,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] localWorkSize,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueNDRangeKernel(
            CommandQueueSafeHandle commandQueue,
            KernelSafeHandle kernel,
            IntPtr[] globalWorkOffset,
            IntPtr[] globalWorkSize,
            IntPtr[] localWorkSize,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException(nameof(commandQueue));
            if (kernel == null)
                throw new ArgumentNullException(nameof(kernel));
            if (globalWorkSize == null)
                throw new ArgumentNullException(nameof(globalWorkSize));

            uint workDim = (uint)globalWorkSize.Length;
            if (globalWorkOffset != null && globalWorkOffset.Length != workDim)
                throw new ArgumentException();
            if (localWorkSize != null && localWorkSize.Length != workDim)
                throw new ArgumentException();

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueNDRangeKernel(commandQueue, kernel, (uint)workDim, globalWorkOffset, globalWorkSize, localWorkSize, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        public static EventTask NDRangeKernelAsync(
            CommandQueueSafeHandle commandQueue,
            KernelSafeHandle kernel,
            IntPtr[] globalWorkOffset,
            IntPtr[] globalWorkSize,
            IntPtr[] localWorkSize,
            EventTask waitEvent)
        {
            var eventWaitList = waitEvent.Event != null ? new[] { waitEvent.Event } : null;
            return new EventTask(EnqueueNDRangeKernel(commandQueue, kernel, globalWorkOffset, globalWorkSize, localWorkSize, eventWaitList));
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueTask(
            CommandQueueSafeHandle commandQueue,
            KernelSafeHandle kernel,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueTask(
            CommandQueueSafeHandle commandQueue,
            KernelSafeHandle kernel,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException(nameof(commandQueue));
            if (kernel == null)
                throw new ArgumentNullException(nameof(kernel));

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueTask(commandQueue, kernel, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueNativeKernel(
            CommandQueueSafeHandle commandQueue,
            NativeKernelFunction userFunction,
            IntPtr args,
            IntPtr cbArgs,
            uint numMemObjects,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] MemObjectSafeHandle[] memList,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] argsMemLoc,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void NativeKernelFunction(IntPtr args);
    }
}
