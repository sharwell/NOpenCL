// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL
{
    using System;
    using System.Threading;

    public class ComputeSynchronizationContext : SynchronizationContext
    {
        private EventTask _currentEvent;

        internal EventTask CurrentEvent
        {
            get
            {
                return _currentEvent;
            }

            set
            {
                EventTask previousEvent = _currentEvent;
                _currentEvent = value;
                previousEvent.Event?.Dispose();
            }
        }

        public static Restorer Push()
        {
            SynchronizationContext context = Current;
            var computeContext = new ComputeSynchronizationContext();
            SetSynchronizationContext(computeContext);
            return new Restorer(computeContext, context);
        }

        public struct Restorer : IDisposable
        {
            private readonly ComputeSynchronizationContext _current;
            private readonly SynchronizationContext _context;

            public Restorer(ComputeSynchronizationContext current, SynchronizationContext context)
            {
                _current = current;
                _context = context;
            }

            public void Dispose()
            {
                if (Current == _current)
                {
                    SetSynchronizationContext(_context);
                }
            }
        }
    }
}
