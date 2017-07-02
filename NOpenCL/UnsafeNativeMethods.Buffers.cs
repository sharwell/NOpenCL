// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using NOpenCL.SafeHandles;

    /// <content>
    /// Buffer objects.
    /// </content>
    internal partial class UnsafeNativeMethods
    {
        /// <summary>
        /// Creates a buffer object.
        /// http://www.khronos.org/registry/cl/sdk/1.2/docs/man/xhtml/clCreateBuffer.html
        /// http://www.khronos.org/registry/cl/specs/opencl-1.2.pdf#page=66
        /// </summary>
        /// <param name="context">A valid OpenCL context used to create the buffer object.</param>
        /// <param name="flags">A bit-field that is used to specify allocation and usage information such as the memory arena that should be used to allocate the buffer object and how it will be used. If value specified for flags is <see cref="MemoryFlags.None"/>, the default is used which is <see cref="MemoryFlags.ReadWrite"/>.</param>
        /// <param name="size">The size in bytes of the buffer memory object to be allocated.</param>
        /// <param name="hostPointer">A pointer to the buffer data that may already be allocated by the application. The size of the buffer that <paramref name="hostPointer"/> points to must be ≥ <paramref name="size"/> bytes.</param>
        /// <param name="errorCode">Returns an appropriate error code.</param>
        /// <returns>
        /// Returns a valid non-zero buffer object and <paramref name="errorCode"/> is
        /// set to <see cref="ErrorCode.Success"/> if the buffer object is created
        /// successfully. Otherwise, it returns an invalid handle with one of the
        /// following error values returned in <paramref name="errorCode"/>:
        ///
        /// <list type="bullet">
        /// <item><see cref="ErrorCode.InvalidContext"/> if <paramref name="context"/> is not a valid context.</item>
        /// <item><see cref="ErrorCode.InvalidValue"/> if values specified in <paramref name="flags"/> are not valid.</item>
        /// <item><see cref="ErrorCode.InvalidBufferSize"/> if <paramref name="size"/> is <see cref="IntPtr.Zero"/>. Implementations may return <see cref="ErrorCode.InvalidBufferSize"/> if <paramref name="size"/> is greater than the <see cref="DeviceInfo.MaxMemoryAllocationSize"/> value <see cref="GetDeviceInfo"/> for all devices in <paramref name="context"/>.</item>
        /// <item><see cref="ErrorCode.InvalidHostPtr"/> if <paramref name="hostPointer"/> is <see cref="IntPtr.Zero"/> and <see cref="MemoryFlags.UseHostPointer"/> or <see cref="MemoryFlags.CopyHostPointer"/> are set in <paramref name="flags"/> or if <paramref name="hostPointer"/> is not <see cref="IntPtr.Zero"/> but <see cref="MemoryFlags.CopyHostPointer"/> or <see cref="MemoryFlags.UseHostPointer"/> are not set in <paramref name="flags"/>.</item>
        /// <item><see cref="ErrorCode.MemObjectAllocationFailure"/> if there is a failure to allocate memory for buffer object.</item>
        /// <item><see cref="ErrorCode.OutOfResources"/> if there is a failure to allocate resources required by the OpenCL implementation on the device.</item>
        /// <item><see cref="ErrorCode.OutOfHostMemory"/> if there is a failure to allocate resources required by the OpenCL implementation on the host.</item>
        /// </list>
        /// </returns>
        [DllImport(ExternDll.OpenCL)]
        private static extern BufferSafeHandle clCreateBuffer(
            ContextSafeHandle context,
            MemoryFlags flags,
            IntPtr size,
            IntPtr hostPointer,
            out ErrorCode errorCode);

        public static BufferSafeHandle CreateBuffer(ContextSafeHandle context, MemoryFlags flags, IntPtr size, IntPtr hostPointer)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            ErrorCode errorCode;
            BufferSafeHandle handle = clCreateBuffer(context, flags, size, hostPointer, out errorCode);
            ErrorHandler.ThrowOnFailure(errorCode);
            return handle;
        }

        /// <summary>
        /// Creates a new buffer object (referred to as a sub-buffer object) from an existing buffer object.
        /// http://www.khronos.org/registry/cl/sdk/1.2/docs/man/xhtml/clCreateSubBuffer.html
        /// http://www.khronos.org/registry/cl/specs/opencl-1.2.pdf#page=69
        /// </summary>
        /// <param name="buffer">A valid object and cannot be a sub-buffer object.</param>
        /// <param name="flags">A bit-field that is used to specify allocation and usage information about the sub-buffer memory object being created.</param>
        /// <param name="mustBeRegion">Must be <see cref="BufferCreateType.Region"/>.</param>
        /// <param name="regionInfo">A <see cref="BufferRegion"/> instance defining the region in
        /// <paramref name="buffer"/> for which to create a sub-buffer.</param>
        /// <param name="errorCode">Returns an appropriate error code.</param>
        /// <returns>
        /// Returns a valid non-zero buffer object and <paramref name="errorCode"/> is set
        /// to <see cref="ErrorCode.Success"/> if the buffer object is created successfully.
        /// Otherwise, it returns an invalid handle with one of the following errors in
        /// <paramref name="errorCode"/>:
        ///
        /// <list type="bullet">
        /// <item><see cref="ErrorCode.InvalidMemObject"/> if <paramref name="buffer"/> is not a valid buffer object or is a sub-buffer object.</item>
        /// <item><see cref="ErrorCode.InvalidValue"/> if <paramref name="buffer"/> was created with <see cref="MemoryFlags.WriteOnly"/> and <paramref name="flags"/> specifies <see cref="MemoryFlags.ReadWrite"/> or <see cref="MemoryFlags.ReadOnly"/>, or if <paramref name="buffer"/> was created with <see cref="MemoryFlags.ReadOnly"/> and <paramref name="flags"/> specifies <see cref="MemoryFlags.ReadWrite"/> or <see cref="MemoryFlags.WriteOnly"/>, or if <paramref name="flags"/> specifies <see cref="MemoryFlags.UseHostPointer"/> or <see cref="MemoryFlags.AllocateHostPointer"/> or <see cref="MemoryFlags.CopyHostPointer"/>.</item>
        /// <item><see cref="ErrorCode.InvalidValue"/> if <paramref name="buffer"/> was created with <see cref="MemoryFlags.HostWriteOnly"/> and <paramref name="flags"/> specifies <see cref="MemoryFlags.HostReadOnly"/> or if <paramref name="buffer"/> was created with <see cref="MemoryFlags.HostReadOnly"/> and <paramref name="flags"/> specifies <see cref="MemoryFlags.HostWriteOnly"/>, or if <paramref name="buffer"/> was created with <see cref="MemoryFlags.HostNoAccess"/> and <paramref name="flags"/> specifies <see cref="MemoryFlags.HostReadOnly"/> or <see cref="MemoryFlags.HostWriteOnly"/>.</item>
        /// <item><see cref="ErrorCode.InvalidValue"/> if value specified in <paramref name="mustBeRegion"/> is not valid.</item>
        /// <item><see cref="ErrorCode.InvalidValue"/> if value(s) specified in <paramref name="regionInfo"/> (for a given <paramref name="mustBeRegion"/>) is not valid.</item>
        /// <item><see cref="ErrorCode.InvalidBufferSize"/> if <see cref="BufferRegion.Size"/> is <see cref="IntPtr.Zero"/>.</item>
        /// <item><see cref="ErrorCode.MemObjectAllocationFailure"/> if there is a failure to allocate memory for sub-buffer object.</item>
        /// <item><see cref="ErrorCode.OutOfResources"/> if there is a failure to allocate resources required by the OpenCL implementation on the device.</item>
        /// <item><see cref="ErrorCode.OutOfHostMemory"/> if there is a failure to allocate resources required by the OpenCL implementation on the host.</item>
        /// </list>
        /// </returns>
        [DllImport(ExternDll.OpenCL)]
        private static extern BufferSafeHandle clCreateSubBuffer(
            BufferSafeHandle buffer,
            MemoryFlags flags,
            BufferCreateType mustBeRegion,
            [In] ref BufferRegion regionInfo,
            out ErrorCode errorCode);

        public static BufferSafeHandle CreateSubBuffer(BufferSafeHandle buffer, MemoryFlags flags, BufferRegion regionInfo)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            ErrorCode errorCode;
            BufferSafeHandle handle = clCreateSubBuffer(buffer, flags, BufferCreateType.Region, ref regionInfo, out errorCode);
            ErrorHandler.ThrowOnFailure(errorCode);
            return handle;
        }

        /// <summary>
        /// Enqueue commands to read from a buffer object to host memory.
        /// http://www.khronos.org/registry/cl/sdk/1.2/docs/man/xhtml/clEnqueueReadBuffer.html
        /// http://www.khronos.org/registry/cl/specs/opencl-1.2.pdf#page=72
        /// </summary>
        /// <remarks>
        /// Calling <see cref="clEnqueueReadBuffer"/> to read a region of the buffer object with
        /// the <paramref name="destination"/> argument value set to <em>hostPointer</em> + <paramref name="offset"/>,
        /// where <em>hostPointer</em> is a pointer to the memory region specified when the buffer
        /// object being read is created with <see cref="MemoryFlags.UseHostPointer"/>, must meet
        /// the following requirements in order to avoid undefined behavior:
        ///
        /// <list type="bullet">
        /// <item>All commands that use this buffer object or a memory object (buffer or image) created from this buffer object have finished execution before the read command begins execution.</item>
        /// <item>The buffer object or memory objects created from this buffer object are not mapped.</item>
        /// <item>The buffer object or memory objects created from this buffer object are not used by any command-queue until the read command has finished execution.</item>
        /// </list>
        /// </remarks>
        /// <param name="commandQueue">Refers to the command-queue in which the read command will be queued. <paramref name="commandQueue"/> and <paramref name="buffer"/> must be created with the same OpenCL context.</param>
        /// <param name="buffer">Refers to a valid buffer object.</param>
        /// <param name="blockingRead">
        /// Indicates if the read operations are blocking or non-blocking.
        ///
        /// <para>If <paramref name="blockingRead"/> is <c>true</c> i.e. the read command is blocking, <see cref="clEnqueueReadBuffer"/> does not return until the buffer data has been read and copied into memory pointed to by <paramref name="destination"/>.</para>
        ///
        /// <para>If <paramref name="blockingRead"/> is <c>false</c> i.e. the read command is non-blocking, <see cref="clEnqueueReadBuffer"/> queues a non-blocking read command and returns. The contents of the buffer that <paramref name="destination"/> points to cannot be used until the read command has completed. The <paramref name="event"/> argument returns an event object which can be used to query the execution status of the read command. When the read command has completed, the contents of the buffer that <paramref name="destination"/> points to can be used by the application.</para>
        /// </param>
        /// <param name="offset">The offset in bytes in the buffer object to read from.</param>
        /// <param name="size">The size in bytes of data being read.</param>
        /// <param name="destination">The pointer to buffer in host memory where data is to be read into.</param>
        /// <param name="numEventsInWaitList">The number of events in <paramref name="eventWaitList"/>.</param>
        /// <param name="eventWaitList">The events that need to complete before this particular command can be executed.
        /// If <paramref name="eventWaitList"/> is <see langword="null"/>, then this particular command does not wait on
        /// any event to complete. If <paramref name="eventWaitList"/> is <see langword="null"/>,
        /// <paramref name="numEventsInWaitList"/> must be 0. If <paramref name="eventWaitList"/> is not
        /// <see langword="null"/>, the list of events pointed to by <paramref name="eventWaitList"/> must be valid and
        /// <paramref name="numEventsInWaitList"/> must be greater than 0.</param>
        /// <param name="event">Returns an event object that identifies this particular read command and can be used to query or queue a wait for this particular command to complete.</param>
        /// <returns>
        /// <see cref="clEnqueueReadBuffer"/> returns <see cref="ErrorCode.Success"/> if the function
        /// is executed successfully. Otherwise, it returns one of the following errors:
        ///
        /// <list type="bullet">
        /// <item><see cref="ErrorCode.InvalidCommandQueue"/> if <paramref name="commandQueue"/> is not a valid command-queue.</item>
        /// <item><see cref="ErrorCode.InvalidContext"/> if the context associated with <paramref name="commandQueue"/> and <paramref name="buffer"/> are not the same or if the context associated with <paramref name="commandQueue"/> and events in <paramref name="eventWaitList"/> are not the same.</item>
        /// <item><see cref="ErrorCode.InvalidMemObject"/> if <paramref name="buffer"/> is not a valid buffer object.</item>
        /// <item><see cref="ErrorCode.InvalidValue"/> if the region being read specified by (<paramref name="offset"/>, <paramref name="size"/>) is out of bounds or if <paramref name="destination"/> is <see cref="IntPtr.Zero"/> or if <paramref name="size"/> is 0.</item>
        /// <item><see cref="ErrorCode.InvalidEventWaitList"/> if <paramref name="eventWaitList"/> is <c>null</c> and <paramref name="numEventsInWaitList"/> greater than 0, or <paramref name="eventWaitList"/> is not <c>null</c> and <paramref name="numEventsInWaitList"/> is 0, or if event objects in <paramref name="eventWaitList"/> are not valid events.</item>
        /// <item><see cref="ErrorCode.MisalignedSubBufferOffset"/> if <paramref name="buffer"/> is a sub-buffer object and <paramref name="offset"/> specified when the sub-buffer object is created is not aligned to <see cref="DeviceInfo.MemoryBaseAddressAlignment"/> value for device associated with <paramref name="commandQueue"/>.</item>
        /// <item><see cref="ErrorCode.ExecStatusErrorForEventsInWaitList"/> if the read and write operations are blocking and the <see cref="EventInfo.CommandExecutionStatus"/> of any of the events in <paramref name="eventWaitList"/> is a negative integer value.</item>
        /// <item><see cref="ErrorCode.MemObjectAllocationFailure"/> if there is a failure to allocate memory for data store associated with <paramref name="buffer"/>.</item>
        /// <item><see cref="ErrorCode.InvalidOperation"/> if <see cref="clEnqueueReadBuffer"/> is called on <paramref name="buffer"/> which has been created with <see cref="MemoryFlags.HostWriteOnly"/> or <see cref="MemoryFlags.HostNoAccess"/>.</item>
        /// <item><see cref="ErrorCode.OutOfResources"/> if there is a failure to allocate resources required by the OpenCL implementation on the device.</item>
        /// <item><see cref="ErrorCode.OutOfHostMemory"/> if there is a failure to allocate resources required by the OpenCL implementation on the host.</item>
        /// </list>
        /// </returns>
        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueReadBuffer(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blockingRead,
            IntPtr offset,
            IntPtr size,
            IntPtr destination,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueReadBuffer(CommandQueueSafeHandle commandQueue, BufferSafeHandle buffer, bool blocking, IntPtr offset, IntPtr size, IntPtr destination, EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (destination == IntPtr.Zero)
                throw new ArgumentNullException("destination");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueReadBuffer(commandQueue, buffer, blocking, offset, size, destination, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueWriteBuffer(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blockingWrite,
            IntPtr offset,
            IntPtr size,
            IntPtr source,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueWriteBuffer(CommandQueueSafeHandle commandQueue, BufferSafeHandle buffer, bool blocking, IntPtr offset, IntPtr size, IntPtr source, EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException(nameof(commandQueue));
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (source == IntPtr.Zero)
                throw new ArgumentNullException(nameof(source));

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueWriteBuffer(commandQueue, buffer, blocking, offset, size, source, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        public static Task WriteBufferAsync(CommandQueueSafeHandle commandQueue, BufferSafeHandle buffer, IntPtr offset, IntPtr size, IntPtr source)
        {
            if (commandQueue == null)
                throw new ArgumentNullException(nameof(commandQueue));
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (source == IntPtr.Zero)
                throw new ArgumentNullException(nameof(source));

            var result = new TaskCompletionSource<VoidResult>();
            bool blocking = false;
            EventSafeHandle handle = EnqueueWriteBuffer(commandQueue, buffer, blocking, offset, size, source, new EventSafeHandle[0]);
            EventCallback eventNotify =
                (eventHandle, status, userData) =>
                {
                    try
                    {
                        ErrorHandler.ThrowOnFailure((ErrorCode)status);
                        result.SetResult(default(VoidResult));
                    }
                    catch (Exception ex)
                    {
                        result.SetException(ex);
                    }
                    finally
                    {
                        handle.Dispose();
                    }
                };

            SetEventCallback(handle, ExecutionStatus.Complete, eventNotify, IntPtr.Zero);
            return result.Task;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueReadBufferRect(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blockingRead,
            [In] ref BufferCoordinates bufferOrigin,
            [In] ref BufferCoordinates hostOrigin,
            [In] ref BufferSize region,
            IntPtr bufferRowPitch,
            IntPtr bufferSlicePitch,
            IntPtr hostRowPitch,
            IntPtr hostSlicePitch,
            IntPtr destination,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueReadBufferRect(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            bool blocking,
            ref BufferCoordinates bufferOrigin,
            ref BufferCoordinates hostOrigin,
            ref BufferSize region,
            IntPtr bufferRowPitch,
            IntPtr bufferSlicePitch,
            IntPtr hostRowPitch,
            IntPtr hostSlicePitch,
            IntPtr destination,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (destination == IntPtr.Zero)
                throw new ArgumentNullException("destination");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueReadBufferRect(commandQueue, buffer, blocking, ref bufferOrigin, ref hostOrigin, ref region, bufferRowPitch, bufferSlicePitch, hostRowPitch, hostSlicePitch, destination, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueWriteBufferRect(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blockingWrite,
            [In] ref BufferCoordinates bufferOrigin,
            [In] ref BufferCoordinates hostOrigin,
            [In] ref BufferSize region,
            IntPtr bufferRowPitch,
            IntPtr bufferSlicePitch,
            IntPtr hostRowPitch,
            IntPtr hostSlicePitch,
            IntPtr source,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueWriteBufferRect(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            bool blocking,
            ref BufferCoordinates bufferOrigin,
            ref BufferCoordinates hostOrigin,
            ref BufferSize region,
            IntPtr bufferRowPitch,
            IntPtr bufferSlicePitch,
            IntPtr hostRowPitch,
            IntPtr hostSlicePitch,
            IntPtr source,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (source == IntPtr.Zero)
                throw new ArgumentNullException("source");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueWriteBufferRect(commandQueue, buffer, blocking, ref bufferOrigin, ref hostOrigin, ref region, bufferRowPitch, bufferSlicePitch, hostRowPitch, hostSlicePitch, source, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueFillBuffer(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            IntPtr pattern,
            IntPtr patternSize,
            IntPtr offset,
            IntPtr size,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueFillBuffer(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            IntPtr pattern,
            IntPtr patternSize,
            IntPtr offset,
            IntPtr size,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (pattern == IntPtr.Zero)
                throw new ArgumentNullException("pattern");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueFillBuffer(commandQueue, buffer, pattern, patternSize, offset, size, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueCopyBuffer(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle sourceBuffer,
            BufferSafeHandle destinationBuffer,
            IntPtr sourceOffset,
            IntPtr destinationOffset,
            IntPtr size,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueCopyBuffer(CommandQueueSafeHandle commandQueue, BufferSafeHandle sourceBuffer, BufferSafeHandle destinationBuffer, IntPtr sourceOffset, IntPtr destinationOffset, IntPtr size, EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (sourceBuffer == null)
                throw new ArgumentNullException("sourceBuffer");
            if (destinationBuffer == null)
                throw new ArgumentNullException("destinationBuffer");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueCopyBuffer(commandQueue, sourceBuffer, destinationBuffer, sourceOffset, destinationOffset, size, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueCopyBufferRect(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle sourceBuffer,
            BufferSafeHandle destinationBuffer,
            [In] ref BufferCoordinates sourceOrigin,
            [In] ref BufferCoordinates destinationOrigin,
            [In] ref BufferSize region,
            IntPtr sourceRowPitch,
            IntPtr sourceSlicePitch,
            IntPtr destinationRowPitch,
            IntPtr destinationSlicePitch,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueCopyBufferRect(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle sourceBuffer,
            BufferSafeHandle destinationBuffer,
            [In] ref BufferCoordinates sourceOrigin,
            [In] ref BufferCoordinates destinationOrigin,
            [In] ref BufferSize region,
            IntPtr sourceRowPitch,
            IntPtr sourceSlicePitch,
            IntPtr destinationRowPitch,
            IntPtr destinationSlicePitch,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (sourceBuffer == null)
                throw new ArgumentNullException("sourceBuffer");
            if (destinationBuffer == null)
                throw new ArgumentNullException("destinationBuffer");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueCopyBufferRect(commandQueue, sourceBuffer, destinationBuffer, ref sourceOrigin, ref destinationOrigin, ref region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern IntPtr clEnqueueMapBuffer(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blockingMap,
            MapFlags mapFlags,
            IntPtr offset,
            IntPtr size,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event,
            out ErrorCode errorCode);

        public static EventSafeHandle EnqueueMapBuffer(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle buffer,
            bool blocking,
            MapFlags mapFlags,
            IntPtr offset,
            IntPtr size,
            out IntPtr mappedPointer,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            EventSafeHandle result;
            ErrorCode errorCode;
            mappedPointer = clEnqueueMapBuffer(commandQueue, buffer, blocking, mapFlags, offset, size, GetNumItems(eventWaitList), GetItems(eventWaitList), out result, out errorCode);
            ErrorHandler.ThrowOnFailure(errorCode);
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ImageSafeHandle clCreateImage(
            ContextSafeHandle context,
            MemoryFlags flags,
            [In] ref ImageFormat imageFormat,
            [In] ref ImageDescriptor imageDescriptor,
            IntPtr hostAddress,
            out ErrorCode errorCode);

        public static ImageSafeHandle CreateImage(ContextSafeHandle context, MemoryFlags flags, ref ImageFormat imageFormat, ref ImageDescriptor imageDescriptor, IntPtr hostAddress)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            ErrorCode errorCode;
            ImageSafeHandle result = clCreateImage(context, flags, ref imageFormat, ref imageDescriptor, hostAddress, out errorCode);
            ErrorHandler.ThrowOnFailure(errorCode);
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clGetSupportedImageFormats(
            ContextSafeHandle context,
            MemoryFlags flags,
            MemObjectType imageType,
            uint numEntries,
            [Out, MarshalAs(UnmanagedType.LPArray)] ImageFormat[] imageFormats,
            out uint numImageFormats);

        public static ImageFormat[] GetSupportedImageFormats(
            ContextSafeHandle context,
            MemoryFlags flags,
            MemObjectType imageType)
        {
            uint required;
            ErrorHandler.ThrowOnFailure(clGetSupportedImageFormats(context, flags, imageType, 0, null, out required));

            ImageFormat[] result = new ImageFormat[required];
            if (required > 0)
            {
                uint actual;
                ErrorHandler.ThrowOnFailure(clGetSupportedImageFormats(context, flags, imageType, required, result, out actual));
                if (actual != required)
                    Array.Resize(ref result, (int)actual);
            }

            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueReadImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [MarshalAs(UnmanagedType.Bool)] bool blockingRead,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            IntPtr rowPitch,
            IntPtr slicePitch,
            IntPtr destination,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueReadImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            bool blocking,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            IntPtr rowPitch,
            IntPtr slicePitch,
            IntPtr destination,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (image == null)
                throw new ArgumentNullException("image");
            if (destination == IntPtr.Zero)
                throw new ArgumentNullException("destination");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueReadImage(commandQueue, image, blocking, ref origin, ref region, rowPitch, slicePitch, destination, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueWriteImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [MarshalAs(UnmanagedType.Bool)] bool blockingWrite,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            IntPtr inputRowPtch,
            IntPtr inputSlicePitch,
            IntPtr source,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueWriteImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            bool blocking,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            IntPtr inputRowPitch,
            IntPtr inputSlicePitch,
            IntPtr source,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (image == null)
                throw new ArgumentNullException("image");
            if (source == IntPtr.Zero)
                throw new ArgumentNullException("source");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueWriteImage(commandQueue, image, blocking, ref origin, ref region, inputRowPitch, inputSlicePitch, source, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueFillImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [In, MarshalAs(UnmanagedType.LPArray)] float[] fillColor,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueFillImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [In] float[] fillColor,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (image == null)
                throw new ArgumentNullException("image");
            if (fillColor == null)
                throw new ArgumentNullException("fillColor");
            if (fillColor.Length != 4)
                throw new ArgumentException();

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueFillImage(commandQueue, image, fillColor, ref origin, ref region, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueFillImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [In, MarshalAs(UnmanagedType.LPArray)] int[] fillColor,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueFillImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [In] int[] fillColor,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (image == null)
                throw new ArgumentNullException("image");
            if (fillColor == null)
                throw new ArgumentNullException("fillColor");
            if (fillColor.Length != 4)
                throw new ArgumentException();

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueFillImage(commandQueue, image, fillColor, ref origin, ref region, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueFillImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [In, MarshalAs(UnmanagedType.LPArray)] uint[] fillColor,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueFillImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [In] uint[] fillColor,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (image == null)
                throw new ArgumentNullException("image");
            if (fillColor == null)
                throw new ArgumentNullException("fillColor");
            if (fillColor.Length != 4)
                throw new ArgumentException();

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueFillImage(commandQueue, image, fillColor, ref origin, ref region, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueCopyImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle sourceImage,
            ImageSafeHandle destinationImage,
            [In] ref BufferCoordinates sourceOrigin,
            [In] ref BufferCoordinates destinationOrigin,
            [In] ref BufferSize region,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueCopyImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle sourceImage,
            ImageSafeHandle destinationImage,
            [In] ref BufferCoordinates sourceOrigin,
            [In] ref BufferCoordinates destinationOrigin,
            [In] ref BufferSize region,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (sourceImage == null)
                throw new ArgumentNullException("sourceImage");
            if (destinationImage == null)
                throw new ArgumentNullException("destinationImage");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueCopyImage(commandQueue, sourceImage, destinationImage, ref sourceOrigin, ref destinationOrigin, ref region, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueCopyImageToBuffer(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle sourceImage,
            BufferSafeHandle destinationBuffer,
            [In] ref BufferCoordinates sourceOrigin,
            [In] ref BufferSize region,
            IntPtr destinationOffset,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueCopyImageToBuffer(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle sourceImage,
            BufferSafeHandle destinationBuffer,
            [In] ref BufferCoordinates sourceOrigin,
            [In] ref BufferSize region,
            IntPtr destinationOffset,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (sourceImage == null)
                throw new ArgumentNullException("sourceImage");
            if (destinationBuffer == null)
                throw new ArgumentNullException("destinationBuffer");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueCopyImageToBuffer(commandQueue, sourceImage, destinationBuffer, ref sourceOrigin, ref region, destinationOffset, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueCopyBufferToImage(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle sourceBuffer,
            ImageSafeHandle destinationImage,
            IntPtr sourceOffset,
            [In] ref BufferCoordinates destinationOrigin,
            [In] ref BufferSize region,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueCopyBufferToImage(
            CommandQueueSafeHandle commandQueue,
            BufferSafeHandle sourceBuffer,
            ImageSafeHandle destinationImage,
            IntPtr sourceOffset,
            [In] ref BufferCoordinates destinationOrigin,
            [In] ref BufferSize region,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (sourceBuffer == null)
                throw new ArgumentNullException("sourceBuffer");
            if (destinationImage == null)
                throw new ArgumentNullException("destinationImage");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueCopyBufferToImage(commandQueue, sourceBuffer, destinationImage, sourceOffset, ref destinationOrigin, ref region, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern IntPtr clEnqueueMapImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            [MarshalAs(UnmanagedType.Bool)] bool blockingMap,
            MapFlags mapFlags,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            out IntPtr imageRowPitch,
            out IntPtr imageSlicePitch,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event,
            out ErrorCode errorCode);

        public static EventSafeHandle EnqueueMapImage(
            CommandQueueSafeHandle commandQueue,
            ImageSafeHandle image,
            bool blocking,
            MapFlags mapFlags,
            [In] ref BufferCoordinates origin,
            [In] ref BufferSize region,
            out IntPtr imageRowPitch,
            out IntPtr imageSlicePitch,
            out IntPtr mappedPointer,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (image == null)
                throw new ArgumentNullException("image");

            EventSafeHandle result;
            ErrorCode errorCode;
            mappedPointer = clEnqueueMapImage(commandQueue, image, blocking, mapFlags, ref origin, ref region, out imageRowPitch, out imageSlicePitch, GetNumItems(eventWaitList), GetItems(eventWaitList), out result, out errorCode);
            ErrorHandler.ThrowOnFailure(errorCode);
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueUnmapMemObject(
            CommandQueueSafeHandle commandQueue,
            MemObjectSafeHandle memObject,
            IntPtr mappedPointer,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueUnmapMemObject(
            CommandQueueSafeHandle commandQueue,
            MemObjectSafeHandle memObject,
            IntPtr mappedPointer,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (memObject == null)
                throw new ArgumentNullException("memObject");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueUnmapMemObject(commandQueue, memObject, mappedPointer, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueMigrateMemObjects(
            CommandQueueSafeHandle commandQueue,
            uint numMemObjects,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] MemObjectSafeHandle[] memObjects,
            MigrationFlags flags,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        public static EventSafeHandle EnqueueMigrateMemObjects(
            CommandQueueSafeHandle commandQueue,
            MemObjectSafeHandle[] memObjects,
            MigrationFlags flags,
            EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");
            if (memObjects == null)
                throw new ArgumentNullException("memObjects");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueMigrateMemObjects(commandQueue, (uint)memObjects.Length, memObjects, flags, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clGetMemObjectInfo(
            MemObjectSafeHandle memObject,
            int paramName,
            UIntPtr paramValueSize,
            IntPtr paramValue,
            out UIntPtr paramValueSizeRet);

        public static T GetMemObjectInfo<T>(MemObjectSafeHandle memObject, MemObjectParameterInfo<T> parameter)
        {
            if (memObject == null)
                throw new ArgumentNullException("memObject");
            if (parameter == null)
                throw new ArgumentNullException("parameter");

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
                ErrorHandler.ThrowOnFailure(clGetMemObjectInfo(memObject, parameter.ParameterInfo.Name, UIntPtr.Zero, IntPtr.Zero, out requiredSize));

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
                ErrorHandler.ThrowOnFailure(clGetMemObjectInfo(memObject, parameter.ParameterInfo.Name, requiredSize, memory, out actualSize));
                return parameter.ParameterInfo.Deserialize(actualSize, memory);
            }
            finally
            {
                Marshal.FreeHGlobal(memory);
            }
        }

        public static class MemObjectInfo
        {
            /// <summary>
            /// Returns one of the following values:
            ///
            /// <list type="bullet">
            /// <item><see cref="MemObjectType.Buffer"/> if the memory object was created with <see cref="clCreateBuffer"/> or <see cref="clCreateSubBuffer"/>.</item>
            /// <item><see cref="ImageDescriptor.Type"/> value if the memory object was created with <see cref="clCreateImage"/>.</item>
            /// </list>
            /// </summary>
            public static readonly MemObjectParameterInfo<uint> Type =
                (MemObjectParameterInfo<uint>)new ParameterInfoUInt32(0x1100);

            /// <summary>
            /// Returns the <em>flags</em> argument value specified when the memory object was
            /// created with <see cref="clCreateBuffer"/>, <see cref="clCreateSubBuffer"/>, or
            /// <see cref="clCreateImage"/>. If the memory object is a sub-buffer the memory
            /// access qualifiers inherited from parent buffer are also returned.
            /// </summary>
            public static readonly MemObjectParameterInfo<ulong> Flags =
                (MemObjectParameterInfo<ulong>)new ParameterInfoUInt64(0x1101);

            /// <summary>
            /// Return actual size of the data store associated with the memory object in bytes.
            /// </summary>
            public static readonly MemObjectParameterInfo<UIntPtr> Size =
                (MemObjectParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1102);

            /// <summary>
            /// If the memory object was created with <see cref="clCreateBuffer"/> or <see cref="clCreateImage"/>
            /// and <see cref="MemoryFlags.UseHostPointer"/> is specified in <em>memoryFlags</em>, return the
            /// <em>hostPointer</em> argument value specified when the memory object was created. Otherwise
            /// <see cref="IntPtr.Zero"/> is returned.
            ///
            /// <para>If the memory object was created with <see cref="clCreateSubBuffer"/>, return the
            /// <em>hostPointer</em> + <em>origin</em> value specified when the memory object was created.
            /// <em>hostPointer</em> is the argument value specified to <see cref="clCreateBuffer"/> and
            /// <see cref="MemoryFlags.UseHostPointer"/> is specified in <em>memoryFlags</em> for memory
            /// object from which this memory object was created. Otherwise <see cref="IntPtr.Zero"/> is
            /// returned.</para>
            /// </summary>
            public static readonly MemObjectParameterInfo<IntPtr> HostAddress =
                (MemObjectParameterInfo<IntPtr>)new ParameterInfoIntPtr(0x1103);

            /// <summary>
            /// Map count. The map count returned should be considered immediately stale. It is unsuitable
            /// for general use in applications. This feature is provided for debugging.
            /// </summary>
            public static readonly MemObjectParameterInfo<uint> MapCount =
                (MemObjectParameterInfo<uint>)new ParameterInfoUInt32(0x1104);

            /// <summary>
            /// Return the reference count of the memory object. The reference count returned should be
            /// considered immediately stale. It is unsuitable for general use in applications. This
            /// feature is provided for identifying memory leaks.
            /// </summary>
            public static readonly MemObjectParameterInfo<uint> ReferenceCount =
                (MemObjectParameterInfo<uint>)new ParameterInfoUInt32(0x1105);

            /// <summary>
            /// Return the context specified when the memory object was created. If the memory object was
            /// created using <see cref="clCreateSubBuffer"/>, the context associated with the memory
            /// object specified as the buffer argument to <see cref="clCreateSubBuffer"/> is returned.
            /// </summary>
            public static readonly MemObjectParameterInfo<IntPtr> Context =
                (MemObjectParameterInfo<IntPtr>)new ParameterInfoIntPtr(0x1106);

            /// <summary>
            /// Return the memory object from which this memory object was created.
            ///
            /// <para>This returns the memory object specified as the <em>buffer</em> argument to <see cref="clCreateSubBuffer"/>.</para>
            ///
            /// <para>Otherwise <see cref="IntPtr.Zero"/> is returned.</para>
            /// </summary>
            public static readonly MemObjectParameterInfo<IntPtr> AssociatedMemObject =
                (MemObjectParameterInfo<IntPtr>)new ParameterInfoIntPtr(0x1107);

            /// <summary>
            /// Return offset if the memory object is a sub-buffer object created using
            /// <see cref="clCreateSubBuffer"/>. This returns <see cref="UIntPtr.Zero"/>
            /// if the memory object is not a sub-buffer object.
            /// </summary>
            public static readonly MemObjectParameterInfo<UIntPtr> Offset =
                (MemObjectParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1108);
        }

        public sealed class MemObjectParameterInfo<T>
        {
            private readonly ParameterInfo<T> _parameterInfo;

            public MemObjectParameterInfo(ParameterInfo<T> parameterInfo)
            {
                if (parameterInfo == null)
                    throw new ArgumentNullException("parameterInfo");

                _parameterInfo = parameterInfo;
            }

            public static explicit operator MemObjectParameterInfo<T>(ParameterInfo<T> parameterInfo)
            {
                return new MemObjectParameterInfo<T>(parameterInfo);
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
        private static extern ErrorCode clGetImageInfo(
            ImageSafeHandle image,
            int paramName,
            UIntPtr paramValueSize,
            IntPtr paramValue,
            out UIntPtr paramValueSizeRet);

        public static T GetImageInfo<T>(ImageSafeHandle image, ImageParameterInfo<T> parameter)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (parameter == null)
                throw new ArgumentNullException("parameter");

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
                ErrorHandler.ThrowOnFailure(clGetImageInfo(image, parameter.ParameterInfo.Name, UIntPtr.Zero, IntPtr.Zero, out requiredSize));

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
                ErrorHandler.ThrowOnFailure(clGetImageInfo(image, parameter.ParameterInfo.Name, requiredSize, memory, out actualSize));
                return parameter.ParameterInfo.Deserialize(actualSize, memory);
            }
            finally
            {
                Marshal.FreeHGlobal(memory);
            }
        }

        public static class ImageInfo
        {
            /// <summary>
            /// Return <see cref="ImageFormat"/> descriptor specified when the image was created with <see cref="clCreateImage"/>.
            /// </summary>
            public static ImageParameterInfo<IntPtr[]> Format { get; } =
                (ImageParameterInfo<IntPtr[]>)new ParameterInfoIntPtrArray(0x1110);

            /// <summary>
            /// Return size of each element of the image memory object. An element is made up of <em>n</em> channels.
            /// The value of <em>n</em> is given in <see cref="ImageFormat.ChannelOrder"/>.
            /// </summary>
            public static ImageParameterInfo<UIntPtr> ElementSize { get; } =
                (ImageParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1111);

            /// <summary>
            /// Return size in bytes of a row of elements of the image object given by the image.
            /// </summary>
            public static ImageParameterInfo<UIntPtr> RowPitch { get; } =
                (ImageParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1112);

            /// <summary>
            /// Return calculated slice pitch in bytes of a 2D slice for the 3D image object or
            /// size of each image in a 1D or 2D image array given by the image. For a 1D image,
            /// 1D image buffer and 2D image object return <see cref="UIntPtr.Zero"/>.
            /// </summary>
            public static ImageParameterInfo<UIntPtr> SlicePitch { get; } =
                (ImageParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1113);

            /// <summary>
            /// Return the width of the image in pixels.
            /// </summary>
            public static ImageParameterInfo<UIntPtr> Width { get; } =
                (ImageParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1114);

            /// <summary>
            /// Return the height of the image in pixels. For a 1D image, 1D image buffer and 1D
            /// image array object, this returns <see cref="UIntPtr.Zero"/>.
            /// </summary>
            public static ImageParameterInfo<UIntPtr> Height { get; } =
                (ImageParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1115);

            /// <summary>
            /// Return the depth of the the image in pixels. For a 1D image, 1D image buffer, 2D
            /// image or 1D and 2D image array object, this returns <see cref="UIntPtr.Zero"/>.
            /// </summary>
            public static ImageParameterInfo<UIntPtr> Depth { get; } =
                (ImageParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1116);

            /// <summary>
            /// Return number of images in the image array. If the image is not an image array,
            /// <see cref="UIntPtr.Zero"/> is returned.
            /// </summary>
            public static ImageParameterInfo<UIntPtr> ArraySize { get; } =
                (ImageParameterInfo<UIntPtr>)new ParameterInfoUIntPtr(0x1117);

            /// <summary>
            /// Return the buffer object associated with the image.
            /// </summary>
            public static ImageParameterInfo<IntPtr> Buffer { get; } =
                (ImageParameterInfo<IntPtr>)new ParameterInfoIntPtr(0x1118);

            /// <summary>
            /// Return the <see cref="ImageDescriptor.NumMipLevels"/> associated with the image.
            /// </summary>
            public static ImageParameterInfo<uint> NumMipLevels { get; } =
                (ImageParameterInfo<uint>)new ParameterInfoUInt32(0x1119);

            /// <summary>
            /// Return the <see cref="ImageDescriptor.NumSamples"/> associated with the image.
            /// </summary>
            public static ImageParameterInfo<uint> NumSamples { get; } =
                (ImageParameterInfo<uint>)new ParameterInfoUInt32(0x111A);
        }

        public sealed class ImageParameterInfo<T>
        {
            private readonly ParameterInfo<T> _parameterInfo;

            public ImageParameterInfo(ParameterInfo<T> parameterInfo)
            {
                if (parameterInfo == null)
                    throw new ArgumentNullException("parameterInfo");

                _parameterInfo = parameterInfo;
            }

            public static explicit operator ImageParameterInfo<T>(ParameterInfo<T> parameterInfo)
            {
                return new ImageParameterInfo<T>(parameterInfo);
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
        private static extern ErrorCode clRetainMemObject(MemObjectSafeHandle memObject);

        [DllImport(ExternDll.OpenCL)]
        public static extern ErrorCode clReleaseMemObject(IntPtr memObject);

        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clSetMemObjectDestructorCallback(MemObjectSafeHandle memObject, MemObjectDestructorCallback notify, IntPtr userData);

        internal static void SetMemObjectDestructorCallback(MemObjectSafeHandle memObject, MemObjectDestructorCallback notify, IntPtr userData)
        {
            ErrorHandler.ThrowOnFailure(clSetMemObjectDestructorCallback(memObject, notify, userData));
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MemObjectDestructorCallback(IntPtr memObject, IntPtr userData);
    }
}
