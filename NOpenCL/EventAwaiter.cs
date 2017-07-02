// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public struct EventAwaiter : ICriticalNotifyCompletion
    {
        private readonly EventTask _eventTask;

        public EventAwaiter(EventTask eventTask)
        {
            _eventTask = eventTask;
        }

        public bool IsCompleted
        {
            get
            {
                if (SynchronizationContext.Current is ComputeSynchronizationContext)
                    return true;

                var executionStatus = (ExecutionStatus)UnsafeNativeMethods.GetEventInfo(_eventTask.Event, UnsafeNativeMethods.EventInfo.CommandExecutionStatus);
                return executionStatus < 0 || executionStatus == ExecutionStatus.Complete;
            }
        }

        public void GetResult()
        {
            if (SynchronizationContext.Current is ComputeSynchronizationContext context)
            {
                context.CurrentEvent = _eventTask;
                return;
            }

            if (_eventTask.Event != null)
            {
                UnsafeNativeMethods.WaitForEvents(new[] { _eventTask.Event });
                _eventTask.Event.Dispose();
            }
        }

        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            UnsafeNativeMethods.SetEventCallback(
                _eventTask.Event,
                ExecutionStatus.Complete,
                (eventHandle, executionStatus, userData) => Task.Factory.StartNew(continuation, CancellationToken.None, TaskCreationOptions.HideScheduler, TaskScheduler.Default),
                userData: IntPtr.Zero);
        }
    }
}
