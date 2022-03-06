using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;
using System;
using System.Linq;
using UnityEngine;

namespace AudioUtilityToolkit.Tests
{
    public class RingBufferTest
    {
        float[] _data;
        float[] _buffer;

        [SetUp]
        public void SetUp()
        {
            _data = new float[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            _buffer = new float[3];
        }

        [Test]
        public void InitialStateTest()
        {
            var ringBuffer = new RingBuffer<float>(10);
            Assert.AreEqual(ringBuffer.Capacity, 16);
            Assert.AreEqual(ringBuffer.FreeCount, 16);
            Assert.AreEqual(ringBuffer.Count, 0);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 0);

            ringBuffer = new RingBuffer<float>(1000);
            Assert.AreEqual(ringBuffer.Capacity, 1024);
            Assert.AreEqual(ringBuffer.FreeCount, 1024);
            Assert.AreEqual(ringBuffer.Count, 0);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 0);
        }

        [Test]
        public void CapacityTest()
        {
            var ringBuffer = new RingBuffer<float>(10);
            Assert.AreEqual(ringBuffer.Capacity, 16);

            ringBuffer = new RingBuffer<float>(100);
            Assert.AreEqual(ringBuffer.Capacity, 128);

            ringBuffer = new RingBuffer<float>(1000);
            Assert.AreEqual(ringBuffer.Capacity, 1024);

            ringBuffer = new RingBuffer<float>(10000);
            Assert.AreEqual(ringBuffer.Capacity, 16384);
        }

        [Test]
        public void EnqueueTest01()
        {
            var ringBuffer = new RingBuffer<float>(10);
            var dataSpan = new ReadOnlySpan<float>(_data);

            ringBuffer.Enqueue(dataSpan.Slice(0, 4));

            Assert.AreEqual(ringBuffer.Count, 4);
            Assert.AreEqual(ringBuffer.FreeCount, 12);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 4);
        }

        [Test]
        public void EnqueueTest02()
        {
            var ringBuffer = new RingBuffer<float>(10);
            var dataSpan = new ReadOnlySpan<float>(_data);
            var bufferSpan = new Span<float>(_buffer);

            ringBuffer.Enqueue(dataSpan.Slice(0, 4));

            Assert.AreEqual(ringBuffer.Count, 4);
            Assert.AreEqual(ringBuffer.FreeCount, 12);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 4);

            ringBuffer.Enqueue(dataSpan);

            Assert.AreEqual(ringBuffer.Count, 14);
            Assert.AreEqual(ringBuffer.FreeCount, 2);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 14);

            ringBuffer.Dequeue(bufferSpan);
            ringBuffer.Enqueue(dataSpan);

            Assert.AreEqual(ringBuffer.Count, 16);
            Assert.AreEqual(ringBuffer.FreeCount, 0);
            Assert.AreEqual(ringBuffer.Head, 3);
            Assert.AreEqual(ringBuffer.Tail, 3);
        }

        [Test]
        public void DequeueTest01()
        {
            var ringBuffer = new RingBuffer<float>(10);
            var dataSpan = new ReadOnlySpan<float>(_data);

            // Enqueue
            ringBuffer.Enqueue(dataSpan.Slice(0, 4));

            Assert.AreEqual(ringBuffer.Count, 4);
            Assert.AreEqual(ringBuffer.FreeCount, 12);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 4);

            // Dequeue
            var bufferSpan = new Span<float>(_buffer);
            ringBuffer.Dequeue(bufferSpan);

            Assert.AreEqual(ringBuffer.Count, 1);
            Assert.AreEqual(ringBuffer.FreeCount, 15);
            Assert.AreEqual(ringBuffer.Head, 3);
            Assert.AreEqual(ringBuffer.Tail, 4);

            Assert.AreEqual(bufferSpan[0], _data[0]);
            Assert.AreEqual(bufferSpan[1], _data[1]);
            Assert.AreEqual(bufferSpan[2], _data[2]);
        }

        [Test]
        public void DequeueTest02()
        {
            var ringBuffer = new RingBuffer<float>(10);
            var dataSpan = new ReadOnlySpan<float>(_data);
            var bufferSpan = new Span<float>(_buffer);

            // Enqueue
            ringBuffer.EnqueueDefault(5);

            Assert.AreEqual(ringBuffer.Count, 5);
            Assert.AreEqual(ringBuffer.FreeCount, 11);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 5);
            
            ringBuffer.Enqueue(dataSpan);

            Assert.AreEqual(ringBuffer.Count, 15);
            Assert.AreEqual(ringBuffer.FreeCount, 1);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 15);
            
            // Dequeue
            ringBuffer.Dequeue(bufferSpan);

            Assert.AreEqual(ringBuffer.Count, 12);
            Assert.AreEqual(ringBuffer.FreeCount, 4);
            Assert.AreEqual(ringBuffer.Head, 3);
            Assert.AreEqual(ringBuffer.Tail, 15);

            Assert.AreEqual(bufferSpan[0], 0);
            Assert.AreEqual(bufferSpan[1], 0);
            Assert.AreEqual(bufferSpan[2], 0);

            ringBuffer.Dequeue(bufferSpan);

            Assert.AreEqual(ringBuffer.Count, 9);
            Assert.AreEqual(ringBuffer.FreeCount, 7);
            Assert.AreEqual(ringBuffer.Head, 6);
            Assert.AreEqual(ringBuffer.Tail, 15);

            Assert.AreEqual(bufferSpan[0], 0);
            Assert.AreEqual(bufferSpan[1], 0);
            Assert.AreEqual(bufferSpan[2], _data[0]);

            ringBuffer.Dequeue(bufferSpan);

            Assert.AreEqual(bufferSpan[0], _data[1]);
            Assert.AreEqual(bufferSpan[1], _data[2]);
            Assert.AreEqual(bufferSpan[2], _data[3]);

            ringBuffer.EnqueueDefault(20);

            Assert.AreEqual(ringBuffer.Count, 16);
            Assert.AreEqual(ringBuffer.FreeCount, 0);
            Assert.AreEqual(ringBuffer.Head, 9);
            Assert.AreEqual(ringBuffer.Tail, 9);
        }

        [Test]
        public void EmptyBufferDequeueTest01()
        {
            var ringBuffer = new RingBuffer<float>(10);
            var bufferSpan = new Span<float>(_buffer);

            for (int i = 0; i < _buffer.Length; i++)
            {
                bufferSpan[i] = 10 * i;
            }

            ringBuffer.Dequeue(bufferSpan);

            for (int i = 0; i < bufferSpan.Length; i++)
            {
                Assert.AreEqual(bufferSpan[i], 10 * i);
            }
        }

        [Test]
        public void EmptyBufferDequeueTest02()
        {
            var ringBuffer = new RingBuffer<float>(10);
            var bufferSpan = new Span<float>(_buffer);

            for (int i = 0; i < _buffer.Length; i++)
            {
                bufferSpan[i] = 10 * i;
            }

            ringBuffer.Dequeue(bufferSpan, fillWithDefaultWhenEmpty: true);

            for (int i = 0; i < bufferSpan.Length; i++)
            {
                Assert.AreEqual(bufferSpan[i], 0f);
            }
        }

        [Test]
        public void EnqueueByteDataTest()
        {
            // SetUp
            var reversedDataArray = _data.Reverse().ToArray();
            var reversedDataArrayBytes = new byte[4 * reversedDataArray.Length];
            for (int k = 0; k < reversedDataArray.Length; k++)
            {
                float value = reversedDataArray[k];
                byte[] valueBytes = BitConverter.GetBytes(value);
                reversedDataArrayBytes[4 * k] = valueBytes[0];
                reversedDataArrayBytes[4 * k + 1] = valueBytes[1];
                reversedDataArrayBytes[4 * k + 2] = valueBytes[2];
                reversedDataArrayBytes[4 * k + 3] = valueBytes[3];
            }

            // Enqueue
            var ringBuffer = new RingBuffer<float>(10);
            ringBuffer.Enqueue(reversedDataArrayBytes);

            Assert.AreEqual(ringBuffer.Count, 10);
            Assert.AreEqual(ringBuffer.FreeCount, 6);
            Assert.AreEqual(ringBuffer.Head, 0);
            Assert.AreEqual(ringBuffer.Tail, 10);

            // Dequeue
            var bufferSpan = new Span<float>(_buffer);
            ringBuffer.Dequeue(bufferSpan);

            Assert.AreEqual(ringBuffer.Count, 7);
            Assert.AreEqual(ringBuffer.FreeCount, 9);
            Assert.AreEqual(ringBuffer.Head, 3);
            Assert.AreEqual(ringBuffer.Tail, 10);

            Assert.AreEqual(bufferSpan[0], _data[_data.Length - 1]);
            Assert.AreEqual(bufferSpan[1], _data[_data.Length - 2]);
            Assert.AreEqual(bufferSpan[2], _data[_data.Length - 3]);
        }
    }
}