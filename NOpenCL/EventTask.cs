// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System.Runtime.CompilerServices;
    using NOpenCL.SafeHandles;

    [AsyncMethodBuilder(typeof(EventTaskMethodBuilder))]
    public struct EventTask
    {
        private readonly EventSafeHandle _eventHandle;

        public EventTask(EventSafeHandle eventHandle)
        {
            _eventHandle = eventHandle;
        }

        public EventSafeHandle Event
        {
            get
            {
                return _eventHandle;
            }
        }

        public EventAwaiter GetAwaiter()
        {
            return new EventAwaiter(this);
        }
    }
}
