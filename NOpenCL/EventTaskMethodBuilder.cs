// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public struct EventTaskMethodBuilder
    {
        private ComputeSynchronizationContext.Restorer _restorer;
        private EventTask? _task;

        public EventTask Task
        {
            get
            {
                if (SynchronizationContext.Current is ComputeSynchronizationContext context)
                {
                    return context.CurrentEvent;
                }

                return _task ?? throw new NotImplementedException();
            }
        }

        public static EventTaskMethodBuilder Create()
        {
            var result = default(EventTaskMethodBuilder);
            result._restorer = ComputeSynchronizationContext.Push();
            return result;
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            throw new NotImplementedException();
        }

        public void SetException(Exception exception)
        {
            _restorer.Dispose();
            throw new NotImplementedException(exception.Message, exception);
        }

        public void SetResult()
        {
            if (SynchronizationContext.Current is ComputeSynchronizationContext context)
            {
                _task = context.CurrentEvent;
                _restorer.Dispose();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            throw new NotImplementedException();
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            throw new NotImplementedException();
        }
    }
}
