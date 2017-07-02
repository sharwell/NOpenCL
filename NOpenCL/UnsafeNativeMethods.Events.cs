// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Runtime.InteropServices;
    using NOpenCL.SafeHandles;

    /// <content>
    /// Event objects.
    /// </content>
    internal partial class UnsafeNativeMethods
    {
        [DllImport(ExternDll.OpenCL)]
        private static extern EventSafeHandle clCreateUserEvent(ContextSafeHandle context, out ErrorCode errorCode);

        public static EventSafeHandle CreateUserEvent(ContextSafeHandle context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            ErrorCode errorCode;
            EventSafeHandle handle = clCreateUserEvent(context, out errorCode);
            ErrorHandler.ThrowOnFailure(errorCode);
            return handle;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clSetUserEventStatus(EventSafeHandle @event, ExecutionStatus executionStatus);

        public static void SetUserEventStatus(EventSafeHandle @event, ExecutionStatus executionStatus)
        {
            if (@event == null)
                throw new ArgumentNullException("event");

            ErrorHandler.ThrowOnFailure(clSetUserEventStatus(@event, executionStatus));
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clWaitForEvents(
            uint numEvents,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList);

        public static void WaitForEvents(EventSafeHandle[] eventWaitList)
        {
            if (eventWaitList == null)
                throw new ArgumentNullException("eventWaitList");
            if (eventWaitList.Length == 0)
                throw new ArgumentException();

            ErrorHandler.ThrowOnFailure(clWaitForEvents((uint)eventWaitList.Length, eventWaitList));
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clGetEventInfo(
            EventSafeHandle @event,
            int paramName,
            UIntPtr paramValueSize,
            IntPtr paramValue,
            out UIntPtr paramValueSizeRet);

        public static T GetEventInfo<T>(EventSafeHandle @event, EventParameterInfo<T> parameter)
        {
            int? fixedSize = parameter.ParameterInfo.FixedSize;
#if DEBUG
            bool verifyFixedSize = true;
#else
            bool verifyFixedSize = false;
#endif

            UIntPtr requiredSize;
            if (fixedSize.HasValue && !verifyFixedSize)
                requiredSize = (UIntPtr)fixedSize;
            else
                ErrorHandler.ThrowOnFailure(clGetEventInfo(@event, parameter.ParameterInfo.Name, UIntPtr.Zero, IntPtr.Zero, out requiredSize));

            if (verifyFixedSize && fixedSize.HasValue)
            {
                if (requiredSize.ToUInt64() != (ulong)fixedSize.Value)
                    throw new ArgumentException("The parameter definition includes a fixed size that does not match the required size according to the runtime.");
            }

            IntPtr memory = IntPtr.Zero;
            try
            {
                memory = Marshal.AllocHGlobal((int)requiredSize.ToUInt32());
                UIntPtr actualSize;
                ErrorHandler.ThrowOnFailure(clGetEventInfo(@event, parameter.ParameterInfo.Name, requiredSize, memory, out actualSize));
                return parameter.ParameterInfo.Deserialize(actualSize, memory);
            }
            finally
            {
                Marshal.FreeHGlobal(memory);
            }
        }

        public static class EventInfo
        {
            /// <summary>
            /// Return the command-queue associated with event. For user event objects, <see cref="IntPtr.Zero"/> is returned.
            /// </summary>
            public static readonly EventParameterInfo<IntPtr> CommandQueue = (EventParameterInfo<IntPtr>)new ParameterInfoIntPtr(0x11D0);

            /// <summary>
            /// Return the context associated with event.
            /// </summary>
            public static readonly EventParameterInfo<IntPtr> Context = (EventParameterInfo<IntPtr>)new ParameterInfoIntPtr(0x11D4);

            /// <summary>
            /// Return the command associated with event. Can be one of the following values:
            ///
            /// <list type="bullet">
            /// <item><see cref="NOpenCL.CommandType.NdrangeKernel"/></item>
            /// <item><see cref="NOpenCL.CommandType.Task"/></item>
            /// <item><see cref="NOpenCL.CommandType.NativeKernel"/></item>
            /// <item><see cref="NOpenCL.CommandType.ReadBuffer"/></item>
            /// <item><see cref="NOpenCL.CommandType.WriteBuffer"/></item>
            /// <item><see cref="NOpenCL.CommandType.CopyBuffer"/></item>
            /// <item><see cref="NOpenCL.CommandType.ReadImage"/></item>
            /// <item><see cref="NOpenCL.CommandType.WriteImage"/></item>
            /// <item><see cref="NOpenCL.CommandType.CopyImage"/></item>
            /// <item><see cref="NOpenCL.CommandType.CopyBufferToImage"/></item>
            /// <item><see cref="NOpenCL.CommandType.CopyImageToBuffer"/></item>
            /// <item><see cref="NOpenCL.CommandType.MapBuffer"/></item>
            /// <item><see cref="NOpenCL.CommandType.MapImage"/></item>
            /// <item><see cref="NOpenCL.CommandType.UnmapMemObject"/></item>
            /// <item><see cref="NOpenCL.CommandType.Marker"/></item>
            /// <item><see cref="NOpenCL.CommandType.AcquireGlObjects"/></item>
            /// <item><see cref="NOpenCL.CommandType.ReleaseGlObjects"/></item>
            /// <item><see cref="NOpenCL.CommandType.ReadBufferRect"/></item>
            /// <item><see cref="NOpenCL.CommandType.WriteBufferRect"/></item>
            /// <item><see cref="NOpenCL.CommandType.CopyBufferRect"/></item>
            /// <item><see cref="NOpenCL.CommandType.User"/></item>
            /// <item><see cref="NOpenCL.CommandType.Barrier"/></item>
            /// <item><see cref="NOpenCL.CommandType.MigrateMemObjects"/></item>
            /// <item><see cref="NOpenCL.CommandType.FillBuffer"/></item>
            /// <item><see cref="NOpenCL.CommandType.FillImage"/></item>
            /// <item><see cref="NOpenCL.CommandType.GlFenceSyncObjectKhr"/> (if cl_khr_gl_event is enabled)</item>
            /// <item><see cref="NOpenCL.CommandType.AcquireD3d10ObjectsKhr"/> (if cl_khr_d3d10_sharing is enabled)</item>
            /// <item><see cref="NOpenCL.CommandType.ReleaseD3d10ObjectsKhr"/> (if cl_khr_d3d10_sharing is enabled)</item>
            /// <item><see cref="NOpenCL.CommandType.AcquireDx9MediaSurfacesKhr"/> (if cl_khr_dx9_media_sharing is enabled)</item>
            /// <item><see cref="NOpenCL.CommandType.ReleaseDx9MediaSurfacesKhr"/> (if cl_khr_dx9_media_sharing is enabled)</item>
            /// <item><see cref="NOpenCL.CommandType.AcquireD3d11ObjectsKhr"/> (if cl_khr_d3d11_sharing is enabled)</item>
            /// <item><see cref="NOpenCL.CommandType.ReleaseD3d11ObjectsKhr"/> (if cl_khr_d3d11_sharing is enabled)</item>
            /// </list>
            /// </summary>
            public static readonly EventParameterInfo<uint> CommandType = (EventParameterInfo<uint>)new ParameterInfoUInt32(0x11D1);

            /// <summary>
            /// Return the execution status of the command identified by event. The valid values are:
            ///
            /// <list type="bullet">
            /// <item><see cref="ExecutionStatus.Queued"/> (command has been enqueued in the command-queue),</item>
            /// <item><see cref="ExecutionStatus.Submitted"/> (enqueued command has been submitted by the host to the device associated with the command-queue),</item>
            /// <item><see cref="ExecutionStatus.Running"/> (device is currently executing this command),</item>
            /// <item><see cref="ExecutionStatus.Complete"/> (the command has completed), or</item>
            /// <item>Error code given by a negative integer value. (command was abnormally terminated – this may be caused by a bad memory access etc.) These error codes come from the same set of error codes that are returned from the platform or runtime API calls as return values or errcode_ret values.</item>
            /// </list>
            ///
            /// The error code values are negative, and event state values are positive. The
            /// event state values are ordered from the largest value (<see cref="ExecutionStatus.Queued"/>) for the first
            /// or initial state to the smallest value (<see cref="ExecutionStatus.Complete"/> or negative integer value)
            /// for the last or complete state. The value of <see cref="ExecutionStatus.Complete"/> and <see cref="ErrorCode.Success"/> are the same.
            /// </summary>
            public static readonly EventParameterInfo<uint> CommandExecutionStatus = (EventParameterInfo<uint>)new ParameterInfoUInt32(0x11D3);

            /// <summary>
            /// Return the event reference count. The reference count returned should be
            /// considered immediately stale. It is unsuitable for general use in applications.
            /// This feature is provided for identifying memory leaks.
            /// </summary>
            public static readonly EventParameterInfo<uint> ReferenceCount = (EventParameterInfo<uint>)new ParameterInfoUInt32(0x11D2);
        }

        public sealed class EventParameterInfo<T>
        {
            private readonly ParameterInfo<T> _parameterInfo;

            public EventParameterInfo(ParameterInfo<T> parameterInfo)
            {
                if (parameterInfo == null)
                    throw new ArgumentNullException("parameterInfo");

                _parameterInfo = parameterInfo;
            }

            public static explicit operator EventParameterInfo<T>(ParameterInfo<T> parameterInfo)
            {
                return new EventParameterInfo<T>(parameterInfo);
            }

            public ParameterInfo<T> ParameterInfo
            {
                get
                {
                    return _parameterInfo;
                }
            }
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clSetEventCallback(
            EventSafeHandle @event,
            ExecutionStatus executionCallbackType,
            EventCallback eventNotify,
            IntPtr userData);

        public static void SetEventCallback(EventSafeHandle @event, ExecutionStatus executionCallbackType, EventCallback eventNotify, IntPtr userData)
        {
            ErrorHandler.ThrowOnFailure(clSetEventCallback(@event, executionCallbackType, eventNotify, userData));
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void EventCallback(IntPtr rawEventHandle, ExecutionStatus eventCommandExecutionStatus, IntPtr userData);

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clRetainEvent(EventSafeHandle @event);

        [DllImport(ExternDll.OpenCL)]
        public static extern ErrorCode clReleaseEvent(IntPtr @event);
    }
}
