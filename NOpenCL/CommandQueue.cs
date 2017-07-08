// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using NOpenCL.SafeHandles;

    public sealed class CommandQueue : IDisposable
    {
        private readonly CommandQueueSafeHandle _handle;
        private readonly Context _context;
        private readonly Device _device;
        private bool _disposed;

        internal CommandQueue(CommandQueueSafeHandle handle, Context context, Device device)
        {
            if (handle == null)
                throw new ArgumentNullException(nameof(handle));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            _handle = handle;
            _context = context;
            _device = device;
        }

        /// <summary>
        /// Gets the <see cref="Context"/> associated with this command queue.
        /// </summary>
        /// <exception cref="ObjectDisposedException">if this command queue has been disposed.</exception>
        public Context Context
        {
            get
            {
                ThrowIfDisposed();
                return _context;
            }
        }

        /// <summary>
        /// Gets the <see cref="Device"/> associated with this command queue.
        /// </summary>
        /// <exception cref="ObjectDisposedException">if this command queue has been disposed.</exception>
        public Device Device
        {
            get
            {
                ThrowIfDisposed();
                return _device;
            }
        }

        /// <summary>
        /// Get the command queue reference count.
        /// </summary>
        /// <remarks>
        /// The returned reference count should be considered immediately stale. It is unsuitable
        /// for general use in applications. This feature is provided for identifying memory leaks.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">if this command queue has been disposed.</exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public uint ReferenceCount
        {
            get
            {
                return UnsafeNativeMethods.GetCommandQueueInfo(_handle, UnsafeNativeMethods.CommandQueueInfo.ReferenceCount);
            }
        }

        /// <summary>
        /// Get the properties for the command queue. These properties are specified by the
        /// <em>properties</em> argument in <see cref="Context.CreateCommandQueue(Device, CommandQueueProperties)"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException">if this command queue has been disposed.</exception>
        public CommandQueueProperties Properties
        {
            get
            {
                return (CommandQueueProperties)UnsafeNativeMethods.GetCommandQueueInfo(_handle, UnsafeNativeMethods.CommandQueueInfo.Properties);
            }
        }

        /// <summary>
        /// Get the underlying handle for this command queue.
        /// </summary>
        /// <exception cref="ObjectDisposedException">if this command queue has been disposed.</exception>
        internal CommandQueueSafeHandle Handle
        {
            get
            {
                ThrowIfDisposed();
                return _handle;
            }
        }

        public Event EnqueueReadBuffer(Buffer buffer, bool blocking, long offset, long size, IntPtr destination, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueReadBuffer(Handle, buffer.Handle, blocking, (IntPtr)offset, (IntPtr)size, destination, eventHandles);
            return new Event(handle);
        }

        public EventTask ReadBufferAsync(Buffer buffer, long offset, long size, IntPtr destination)
        {
            EventTask marker;
            bool disposeMarker;
            if (SynchronizationContext.Current is ComputeSynchronizationContext context)
            {
                marker = context.CurrentEvent;
                disposeMarker = false;
            }
            else
            {
                // No synchronization information is available, so enqueue a marker and read the buffer after that
                marker = WhenAllAsync();
                disposeMarker = true;
            }

            try
            {
                return UnsafeNativeMethods.ReadBufferAsync(Handle, buffer.Handle, (IntPtr)offset, (IntPtr)size, destination, marker);
            }
            finally
            {
                if (disposeMarker)
                    marker.Event?.Dispose();
            }
        }

        public Event EnqueueReadBufferRect(Buffer buffer, bool blocking, BufferCoordinates bufferOrigin, BufferCoordinates hostOrigin, BufferSize region, long bufferRowPitch, long bufferSlicePitch, long hostRowPitch, long hostSlicePitch, IntPtr destination, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueReadBufferRect(Handle, buffer.Handle, blocking, ref bufferOrigin, ref hostOrigin, ref region, (IntPtr)bufferRowPitch, (IntPtr)bufferSlicePitch, (IntPtr)hostRowPitch, (IntPtr)hostSlicePitch, destination, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueWriteBuffer(Buffer buffer, bool blocking, long offset, long size, IntPtr source, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueWriteBuffer(Handle, buffer.Handle, blocking, (IntPtr)offset, (IntPtr)size, source, eventHandles);
            return new Event(handle);
        }

        public EventTask WriteBufferAsync(Buffer buffer, long offset, long size, IntPtr source)
        {
            EventTask marker;
            bool disposeMarker;
            if (SynchronizationContext.Current is ComputeSynchronizationContext context)
            {
                marker = context.CurrentEvent;
                disposeMarker = false;
            }
            else
            {
                // No synchronization information is available, so enqueue a marker and read the buffer after that
                marker = WhenAllAsync();
                disposeMarker = true;
            }

            try
            {
                return UnsafeNativeMethods.WriteBufferAsync(Handle, buffer.Handle, (IntPtr)offset, (IntPtr)size, source, marker);
            }
            finally
            {
                if (disposeMarker)
                    marker.Event?.Dispose();
            }
        }

        public Event EnqueueWriteBufferRect(Buffer buffer, bool blocking, BufferCoordinates bufferOrigin, BufferCoordinates hostOrigin, BufferSize region, long bufferRowPitch, long bufferSlicePitch, long hostRowPitch, long hostSlicePitch, IntPtr source, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueWriteBufferRect(Handle, buffer.Handle, blocking, ref bufferOrigin, ref hostOrigin, ref region, (IntPtr)bufferRowPitch, (IntPtr)bufferSlicePitch, (IntPtr)hostRowPitch, (IntPtr)hostSlicePitch, source, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueCopyBuffer(Buffer source, Buffer destination, long sourceOffset, long destinationOffset, long size, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueCopyBuffer(Handle, source.Handle, destination.Handle, (IntPtr)sourceOffset, (IntPtr)destinationOffset, (IntPtr)size, eventHandles);
            return new Event(handle);
        }

        public EventTask CopyBufferAsync(Buffer source, Buffer destination, long sourceOffset, long destinationOffset, long size)
        {
            EventTask marker;
            bool disposeMarker;
            if (SynchronizationContext.Current is ComputeSynchronizationContext context)
            {
                marker = context.CurrentEvent;
                disposeMarker = false;
            }
            else
            {
                // No synchronization information is available, so enqueue a marker and read the buffer after that
                marker = WhenAllAsync();
                disposeMarker = true;
            }

            try
            {
                return UnsafeNativeMethods.CopyBufferAsync(Handle, source.Handle, destination.Handle, (IntPtr)sourceOffset, (IntPtr)destinationOffset, (IntPtr)size, marker);
            }
            finally
            {
                if (disposeMarker)
                    marker.Event?.Dispose();
            }
        }

        public Event EnqueueCopyBufferRect(Buffer source, Buffer destination, BufferCoordinates sourceOrigin, BufferCoordinates destinationOrigin, BufferSize region, long sourceRowPitch, long sourceSlicePitch, long destinationRowPitch, long destinationSlicePitch, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueCopyBufferRect(Handle, source.Handle, destination.Handle, ref sourceOrigin, ref destinationOrigin, ref region, (IntPtr)sourceRowPitch, (IntPtr)sourceSlicePitch, (IntPtr)destinationRowPitch, (IntPtr)destinationSlicePitch, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueMapBuffer(Buffer buffer, bool blocking, MapFlags mapFlags, long offset, long size, out IntPtr mappedPointer, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueMapBuffer(Handle, buffer.Handle, blocking, mapFlags, (IntPtr)offset, (IntPtr)size, out mappedPointer, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueReadImage(Image image, bool blocking, BufferCoordinates origin, BufferSize region, long rowPitch, long slicePitch, IntPtr destination, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueReadImage(Handle, image.Handle, blocking, ref origin, ref region, (IntPtr)rowPitch, (IntPtr)slicePitch, destination, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueWriteImage(Image image, bool blocking, BufferCoordinates origin, BufferSize region, long inputRowPitch, long inputSlicePitch, IntPtr source, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueWriteImage(Handle, image.Handle, blocking, ref origin, ref region, (IntPtr)inputRowPitch, (IntPtr)inputSlicePitch, source, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueFillImage(Image image, float[] fillColor, BufferCoordinates origin, BufferSize region, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueFillImage(Handle, image.Handle, fillColor, ref origin, ref region, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueFillImage(Image image, int[] fillColor, BufferCoordinates origin, BufferSize region, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueFillImage(Handle, image.Handle, fillColor, ref origin, ref region, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueFillImage(Image image, uint[] fillColor, BufferCoordinates origin, BufferSize region, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueFillImage(Handle, image.Handle, fillColor, ref origin, ref region, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueCopyImage(Image sourceImage, Image destinationImage, BufferCoordinates sourceOrigin, BufferCoordinates destinationOrigin, BufferSize region, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueCopyImage(Handle, sourceImage.Handle, destinationImage.Handle, ref sourceOrigin, ref destinationOrigin, ref region, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueCopyImageToBuffer(Image sourceImage, Buffer destinationBuffer, BufferCoordinates sourceOrigin, BufferSize region, long destinationOffset, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueCopyImageToBuffer(Handle, sourceImage.Handle, destinationBuffer.Handle, ref sourceOrigin, ref region, (IntPtr)destinationOffset, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueCopyBufferToImage(Buffer sourceBuffer, Image destinationImage, long sourceOffset, BufferCoordinates destinationOrigin, BufferSize region, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueCopyBufferToImage(Handle, sourceBuffer.Handle, destinationImage.Handle, (IntPtr)sourceOffset, ref destinationOrigin, ref region, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueMapImage(Image image, bool blocking, MapFlags mapFlags, BufferCoordinates origin, BufferSize region, out long rowPitch, out long slicePitch, out IntPtr mappedPointer, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            IntPtr imageRowPitch;
            IntPtr imageSlicePitch;
            EventSafeHandle handle = UnsafeNativeMethods.EnqueueMapImage(Handle, image.Handle, blocking, mapFlags, ref origin, ref region, out imageRowPitch, out imageSlicePitch, out mappedPointer, eventHandles);
            rowPitch = imageRowPitch.ToInt64();
            slicePitch = imageSlicePitch.ToInt64();
            return new Event(handle);
        }

        public Event EnqueueUnmapMemObject(MemObject memObject, IntPtr mappedPointer, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueUnmapMemObject(Handle, memObject.BaseHandle, mappedPointer, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueMigrateMemObjects(MemObject[] memObjects, MigrationFlags flags, params Event[] eventWaitList)
        {
            MemObjectSafeHandle[] memHandles = null;
            if (memObjects != null)
                memHandles = Array.ConvertAll(memObjects, mem => mem.BaseHandle);

            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueMigrateMemObjects(Handle, memHandles, flags, eventHandles);
            return new Event(handle);
        }

        public Event EnqueueNDRangeKernel(Kernel kernel, IntPtr globalWorkSize, IntPtr localWorkSize, params Event[] eventWaitList)
        {
            return EnqueueNDRangeKernel(kernel, null, new[] { globalWorkSize }, new[] { localWorkSize }, eventWaitList);
        }

        public Event EnqueueNDRangeKernel(Kernel kernel, IntPtr globalWorkOffset, IntPtr globalWorkSize, IntPtr localWorkSize, params Event[] eventWaitList)
        {
            return EnqueueNDRangeKernel(kernel, new[] { globalWorkOffset }, new[] { globalWorkSize }, new[] { localWorkSize }, eventWaitList);
        }

        public Event EnqueueNDRangeKernel(Kernel kernel, IntPtr[] globalWorkSize, IntPtr[] localWorkSize, params Event[] eventWaitList)
        {
            return EnqueueNDRangeKernel(kernel, null, globalWorkSize, localWorkSize, eventWaitList);
        }

        public Event EnqueueNDRangeKernel(Kernel kernel, IntPtr[] globalWorkOffset, IntPtr[] globalWorkSize, IntPtr[] localWorkSize, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueNDRangeKernel(Handle, kernel.Handle, globalWorkOffset, globalWorkSize, localWorkSize, eventHandles);
            return new Event(handle);
        }

        /// <summary>
        /// Enqueues a command to execute a <see cref="Kernel"/> on this command queue's <see cref="Device"/>.
        /// </summary>
        /// <remarks>
        /// The kernel is executed using a single work-item.
        ///
        /// <para><see cref="EnqueueTask"/> is equivalent to calling
        /// <see cref="EnqueueNDRangeKernel(Kernel, IntPtr, IntPtr, Event[])"/> with
        /// <em>globalWorkSize</em> set to 1, and <em>localWorkSize</em> set to 1.</para>
        /// </remarks>
        /// <param name="kernel">A valid <see cref="Kernel"/> object.</param>
        /// <param name="eventWaitList">The events that need to be complete before this
        /// command is executed. If the list is null or empty, this command does not
        /// wait on any event to complete.</param>
        /// <returns>Returns an event object that identifies this particular kernel execution instance.</returns>
        public Event EnqueueTask(Kernel kernel, params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueTask(Handle, kernel.Handle, eventHandles);
            return new Event(handle);
        }

        /// <summary>
        /// Enqueues a marker command which waits for either a list of events to complete,
        /// or all previously enqueued commands to complete.
        /// </summary>
        /// <remarks>
        /// Enqueues a marker command which waits for all events in
        /// <paramref name="eventWaitList"/> to complete, or if
        /// <paramref name="eventWaitList"/> is empty it waits for all previously enqueued
        /// commands to complete before it completes. This command returns an event which
        /// can be waited on, i.e. this event can be waited on to ensure that all events
        /// either in <paramref name="eventWaitList"/> or all previously enqueued commands,
        /// queued before this command, have completed.
        /// </remarks>
        /// <param name="eventWaitList">The events that need to be complete before this
        /// command is executed. If the list is null or empty, this command waits until
        /// all previous enqueued commands have completed.</param>
        /// <returns>Returns an event object that identifies this particular command.</returns>
        public Event EnqueueMarker(params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueMarkerWithWaitList(Handle, eventHandles);
            return new Event(handle);
        }

        public EventTask WhenAllAsync()
        {
            return UnsafeNativeMethods.WhenAllAsync(Handle);
        }

        /// <summary>
        /// A synchronization point that enqueues a barrier operation.
        /// </summary>
        /// <remarks>
        /// Enqueues a barrier command which waits for all events in
        /// <paramref name="eventWaitList"/> to complete, or if
        /// <paramref name="eventWaitList"/> is empty it waits for all previously enqueued
        /// commands to complete before it completes. This command blocks command execution,
        /// that is, any commands enqueued after it do not execute until it completes. This
        /// command returns an event which can be waited on, i.e. this event can be waited
        /// on to ensure that all events either in the <paramref name="eventWaitList"/>
        /// or all previously enqueued commands, queued before this command, have completed.
        /// </remarks>
        /// <param name="eventWaitList">The events that need to be complete before this
        /// command is executed. If the list is null or empty, this command waits until
        /// all previous enqueued commands have completed.</param>
        /// <returns>Returns an event object that identifies this particular command.</returns>
        public Event EnqueueBarrier(params Event[] eventWaitList)
        {
            EventSafeHandle[] eventHandles = null;
            if (eventWaitList != null)
                eventHandles = Array.ConvertAll(eventWaitList, @event => @event.Handle);

            EventSafeHandle handle = UnsafeNativeMethods.EnqueueBarrierWithWaitList(Handle, eventHandles);
            return new Event(handle);
        }

        /// <summary>
        /// Blocks until all previously queued OpenCL commands in the command-queue are
        /// issued to the associated device and have completed.
        /// </summary>
        /// <remarks>
        /// <see cref="Finish"/> does not return until all previously queued commands in
        /// the command queue have been processed and completed. <see cref="Finish"/> is
        /// also a synchronization point.
        /// </remarks>
        public void Finish()
        {
            ThrowIfDisposed();
            UnsafeNativeMethods.Finish(Handle);
        }

        /// <summary>
        /// Issues all previously queued OpenCL commands in the command-queue to the device
        /// associated with the command-queue.
        /// </summary>
        /// <remarks>
        /// <see cref="Flush"/> only guarantees that all queued commands will eventually be
        /// submitted to the appropriate device. There is no guarantee that they will be
        /// complete after <see cref="Flush"/> returns.
        ///
        /// <para>Any blocking commands queued in a command-queue and <see cref="Dispose"/>
        /// perform an implicit flush of the command-queue. These blocking commands are
        /// <see cref="EnqueueReadBuffer"/>, <see cref="EnqueueReadBufferRect"/>, or
        /// <see cref="EnqueueReadImage"/> with <c>blocking</c> set to <c>true</c>;
        /// <see cref="EnqueueWriteBuffer"/>, <see cref="EnqueueWriteBufferRect"/>, or
        /// <see cref="EnqueueWriteImage"/> with <c>blocking_write</c> set to <c>true</c>;
        /// <see cref="EnqueueMapBuffer"/> or <see cref="EnqueueMapImage"/> with
        /// <c>blocking_map</c> set to <c>true</c>; or <see cref="Event.WaitAll"/>.</para>
        ///
        /// <para>To use event objects that refer to commands enqueued in a command-queue
        /// as event objects to wait on by commands enqueued in a different command-queue,
        /// the application must call a <see cref="Flush"/> or any blocking commands that
        /// perform an implicit flush of the command-queue where the commands that refer
        /// to these event objects are enqueued.</para>
        /// </remarks>
        public void Flush()
        {
            ThrowIfDisposed();
            UnsafeNativeMethods.Flush(Handle);
        }

        public void Dispose()
        {
            _handle.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this command queue has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">if this command queue has been disposed.</exception>
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
