// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL.Test
{
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Buffer = NOpenCL.Buffer;

    [TestClass]
    public class TestBuffers
    {
        [TestMethod]
        public void TestBufferDestroyedEvent()
        {
            bool destroyed = false;

            Platform platform = Platform.GetPlatforms()[0];
            using (Context context = Context.Create(platform.GetDevices()))
            {
                using (Buffer buffer = context.CreateBuffer(MemoryFlags.AllocateHostPointer, 1024))
                {
                    buffer.Destroyed += (sender, e) => destroyed = true;
                    Assert.IsFalse(destroyed);
                }
            }

            Assert.IsTrue(destroyed);
        }

        [TestMethod]
        public async Task TestBufferCopyAsync()
        {
            Platform platform = Platform.GetPlatforms()[0];
            Device device = platform.GetDevices().First();
            using (Context context = Context.Create(device))
            using (CommandQueue queue = context.CreateCommandQueue(device))
            using (Buffer sourceBuffer = context.CreateBuffer(MemoryFlags.AllocateHostPointer, 1024))
            using (Buffer targetBuffer = context.CreateBuffer(MemoryFlags.AllocateHostPointer, 1024))
            {
                using (ComputeSynchronizationContext.Push())
                {
                    await queue.CopyBufferAsync(sourceBuffer, targetBuffer, 0, 0, 1024);
                }

                queue.Finish();
            }
        }

        [TestMethod]
        public async Task TestEventTaskFastAsync()
        {
            await TestEventTaskAsync(true, 100000);
        }

        [TestMethod]
        public async Task TestEventTaskSlowAsync()
        {
            await TestEventTaskAsync(false, 100000);
        }

        private async Task TestEventTaskAsync(bool testFast, int count)
        {
            int[] referenceData = new int[1024];
            for (int i = 0; i < referenceData.Length; i++)
                referenceData[i] = i;

            Platform platform = Platform.GetPlatforms()[0];
            Device device = platform.GetDevices().First();
            using (Context context = Context.Create(device))
            using (CommandQueue queue = context.CreateCommandQueue(device))
            using (Buffer sourceBuffer = context.CreateBuffer(MemoryFlags.None, System.Buffer.ByteLength(referenceData)))
            using (Buffer targetBuffer = context.CreateBuffer(MemoryFlags.None, System.Buffer.ByteLength(referenceData)))
            {
                int[] outputData = new int[referenceData.Length];

                GCHandle pinnedReferenceData = GCHandle.Alloc(referenceData, GCHandleType.Pinned);
                GCHandle pinnedOutputData = GCHandle.Alloc(outputData, GCHandleType.Pinned);

                if (testFast)
                    await TestFastImplAsync();
                else
                    await TestSlowImplAsync();

                Assert.IsNotInstanceOfType(SynchronizationContext.Current, typeof(ComputeSynchronizationContext));
                queue.Finish();

                pinnedOutputData.Free();
                pinnedReferenceData.Free();

                for (int i = 0; i < referenceData.Length; i++)
                {
                    Assert.AreEqual(referenceData[i], outputData[i]);
                }

                // Local functions
                async EventTask TestFastImplAsync()
                {
                    for (int i = 0; i < count; i++)
                    {
                        Assert.IsInstanceOfType(SynchronizationContext.Current, typeof(ComputeSynchronizationContext));
                        await queue.WriteBufferAsync(sourceBuffer, 0, System.Buffer.ByteLength(referenceData), pinnedReferenceData.AddrOfPinnedObject());
                        await queue.CopyBufferAsync(sourceBuffer, targetBuffer, 0, 0, System.Buffer.ByteLength(referenceData));
                        await queue.ReadBufferAsync(targetBuffer, 0, System.Buffer.ByteLength(referenceData), pinnedOutputData.AddrOfPinnedObject());
                    }
                }

                async Task TestSlowImplAsync()
                {
                    for (int i = 0; i < count; i++)
                    {
                        await queue.WriteBufferAsync(sourceBuffer, 0, System.Buffer.ByteLength(referenceData), pinnedReferenceData.AddrOfPinnedObject());
                        await queue.CopyBufferAsync(sourceBuffer, targetBuffer, 0, 0, System.Buffer.ByteLength(referenceData));
                        await queue.ReadBufferAsync(targetBuffer, 0, System.Buffer.ByteLength(referenceData), pinnedOutputData.AddrOfPinnedObject());
                    }
                }
            }
        }
    }
}
