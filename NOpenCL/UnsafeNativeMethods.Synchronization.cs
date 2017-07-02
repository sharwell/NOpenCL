// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Runtime.InteropServices;
    using NOpenCL.SafeHandles;

    /// <content>
    /// Markers, barriers, and waiting.
    /// </content>
    internal partial class UnsafeNativeMethods
    {
        /// <summary>
        /// Enqueues a marker command which waits for either a list of events to complete, or all previously enqueued commands to complete.
        /// http://www.khronos.org/registry/cl/sdk/1.2/docs/man/xhtml/clEnqueueMarkerWithWaitList.html
        /// http://www.khronos.org/registry/cl/specs/opencl-1.2.pdf#page=186
        /// </summary>
        /// <remarks>
        /// Enqueues a marker command which waits for either a list of events to complete,
        /// or if the list is empty it waits for all commands previously enqueued in
        /// <paramref name="commandQueue"/> to complete before it completes. This command
        /// returns an event which can be waited on, i.e. this event can be waited on to
        /// ensure that all events either in the <paramref name="eventWaitList"/> or all
        /// previously enqueued commands, queued before this command to
        /// <paramref name="commandQueue"/>, have completed.
        /// </remarks>
        /// <param name="commandQueue">A valid command-queue.</param>
        /// <param name="numEventsInWaitList">The number of events in <paramref name="eventWaitList"/>.</param>
        /// <param name="eventWaitList">The events that need to complete before this particular command can be executed.
        /// If <paramref name="eventWaitList"/> is <see langword="null"/>, then this particular command does not wait on
        /// any event to complete. If <paramref name="eventWaitList"/> is <see langword="null"/>,
        /// <paramref name="numEventsInWaitList"/> must be 0. If <paramref name="eventWaitList"/> is not
        /// <see langword="null"/>, the list of events pointed to by <paramref name="eventWaitList"/> must be valid and
        /// <paramref name="numEventsInWaitList"/> must be greater than 0.</param>
        /// <param name="event">Returns an event object that identifies this particular command.</param>
        /// <returns>
        /// Returns <see cref="ErrorCode.Success"/> if the function executed successfully,
        /// or one of the errors below:
        ///
        /// <list type="bullet">
        /// <item><see cref="ErrorCode.InvalidCommandQueue"/> if <paramref name="commandQueue"/> is not a valid command-queue.</item>
        /// <item><see cref="ErrorCode.InvalidEventWaitList"/> if <paramref name="eventWaitList"/> is <c>null</c> and <paramref name="numEventsInWaitList"/> &gt; 0, or <paramref name="eventWaitList"/> is not <c>null</c> and <paramref name="numEventsInWaitList"/> is 0, or if event objects in <paramref name="eventWaitList"/> are not valid events.</item>
        /// <item><see cref="ErrorCode.OutOfResources"/> if there is a failure to allocate resources required by the OpenCL implementation on the device.</item>
        /// <item><see cref="ErrorCode.OutOfHostMemory"/> if there is a failure to allocate resources required by the OpenCL implementation on the host.</item>
        /// </list>
        /// </returns>
        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueMarkerWithWaitList(
            CommandQueueSafeHandle commandQueue,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        internal static EventSafeHandle EnqueueMarkerWithWaitList(CommandQueueSafeHandle commandQueue, EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueMarkerWithWaitList(commandQueue, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }

        internal static EventTask WhenAllAsync(CommandQueueSafeHandle commandQueue, EventTask[] events)
        {
            EventSafeHandle[] eventWaitList = events != null ? Array.ConvertAll(events, eventTask => eventTask.Event) : null;
            return new EventTask(EnqueueMarkerWithWaitList(commandQueue, eventWaitList));
        }

        internal static EventTask WhenAllAsync(CommandQueueSafeHandle commandQueue)
        {
            return WhenAllAsync(commandQueue, null);
        }

        /// <summary>
        /// A synchronization point that enqueues a barrier operation.
        /// http://www.khronos.org/registry/cl/sdk/1.2/docs/man/xhtml/clEnqueueBarrierWithWaitList.html
        /// http://www.khronos.org/registry/cl/specs/opencl-1.2.pdf#page=187
        /// </summary>
        /// <remarks>
        /// Enqueues a barrier command which waits for either a list of events to complete,
        /// or if the list is empty it waits for all commands previously enqueued in
        /// <paramref name="commandQueue"/> to complete before it completes. This command
        /// blocks command execution, that is, any following commands enqueued after it do
        /// not execute until it completes. This command returns an event which can be
        /// waited on, i.e. this event can be waited on to ensure that all events either
        /// in the <paramref name="eventWaitList"/> or all previously enqueued commands,
        /// queued before this command to <paramref name="commandQueue"/>, have completed.
        /// </remarks>
        /// <param name="commandQueue">A valid command-queue.</param>
        /// <param name="numEventsInWaitList">The number of events in <paramref name="eventWaitList"/>.</param>
        /// <param name="eventWaitList">The events that need to complete before this particular command can be executed.
        /// If <paramref name="eventWaitList"/> is <see langword="null"/>, then this particular command does not wait on
        /// any event to complete. If <paramref name="eventWaitList"/> is <see langword="null"/>,
        /// <paramref name="numEventsInWaitList"/> must be 0. If <paramref name="eventWaitList"/> is not
        /// <see langword="null"/>, the list of events pointed to by <paramref name="eventWaitList"/> must be valid and
        /// <paramref name="numEventsInWaitList"/> must be greater than 0.</param>
        /// <param name="event">Returns an event object that identifies this particular command.</param>
        /// <returns>
        /// Returns <see cref="ErrorCode.Success"/> if the function executed successfully,
        /// or one of the errors below:
        ///
        /// <list type="bullet">
        /// <item><see cref="ErrorCode.InvalidCommandQueue"/> if <paramref name="commandQueue"/> is not a valid command-queue.</item>
        /// <item><see cref="ErrorCode.InvalidEventWaitList"/> if <paramref name="eventWaitList"/> is <c>null</c> and <paramref name="numEventsInWaitList"/> &gt; 0, or <paramref name="eventWaitList"/> is not <c>null</c> and <paramref name="numEventsInWaitList"/> is 0, or if event objects in <paramref name="eventWaitList"/> are not valid events.</item>
        /// <item><see cref="ErrorCode.OutOfResources"/> if there is a failure to allocate resources required by the OpenCL implementation on the device.</item>
        /// <item><see cref="ErrorCode.OutOfHostMemory"/> if there is a failure to allocate resources required by the OpenCL implementation on the host.</item>
        /// </list>
        /// </returns>
        [DllImport(ExternDll.OpenCL)]
        private static extern ErrorCode clEnqueueBarrierWithWaitList(
            CommandQueueSafeHandle commandQueue,
            uint numEventsInWaitList,
            [In, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(SafeHandleArrayMarshaler))] EventSafeHandle[] eventWaitList,
            out EventSafeHandle @event);

        internal static EventSafeHandle EnqueueBarrierWithWaitList(CommandQueueSafeHandle commandQueue, EventSafeHandle[] eventWaitList)
        {
            if (commandQueue == null)
                throw new ArgumentNullException("commandQueue");

            EventSafeHandle result;
            ErrorHandler.ThrowOnFailure(clEnqueueBarrierWithWaitList(commandQueue, GetNumItems(eventWaitList), GetItems(eventWaitList), out result));
            return result;
        }
    }
}
