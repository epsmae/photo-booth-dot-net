using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PhotoBooth.Camera.LiveView;

namespace PhotoBooth.Camera.Test.LiveView
{
    public class MjpegFrameParserTest
    {
        // real Nikon D7000 JPEG, the EXIF segment contains two embedded thumbnails (each with FF D8 ... FF D9)
        private static byte[] RealFrame
        {
            get
            {
                return File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "FakeGPhoto2", "frame.jpg"));
            }
        }

        // SOI, DQT, SOS with entropy data containing stuffed bytes (FF 00), a restart marker (FF D0) and fill bytes, EOI
        private static readonly byte[] SyntheticFrame =
        {
            0xFF, 0xD8,
            0xFF, 0xDB, 0x00, 0x05, 0xFF, 0xD9, 0x01,
            0xFF, 0xDA, 0x00, 0x04, 0x01, 0x02,
            0x11, 0xFF, 0x00, 0x22, 0xFF, 0xD0, 0x33, 0xFF, 0xFF, 0x00, 0x44,
            0xFF, 0xD9
        };

        [Test]
        public void TestSplitsFramesWithEmbeddedThumbnails()
        {
            byte[] frame = RealFrame;
            List<byte[]> frames = Parse(Concat(new byte[] {0x00, 0xFF, 0x12}, frame, frame, frame), int.MaxValue);

            Assert.AreEqual(3, frames.Count);
            Assert.True(frames.All(f => f.SequenceEqual(frame)));
        }

        [Test]
        public void TestByteByByte()
        {
            byte[] frame = RealFrame;
            List<byte[]> frames = Parse(Concat(frame, frame), 1);

            Assert.AreEqual(2, frames.Count);
            Assert.True(frames.All(f => f.SequenceEqual(frame)));
        }

        [Test]
        public void TestStuffedBytesRestartMarkersAndFillBytes()
        {
            List<byte[]> frames = Parse(Concat(SyntheticFrame, SyntheticFrame), 3);

            Assert.AreEqual(2, frames.Count);
            Assert.True(frames.All(f => f.SequenceEqual(SyntheticFrame)));
        }

        [Test]
        public void TestTruncatedFrameIsDiscarded()
        {
            byte[] frame = RealFrame;
            byte[] truncated = frame.Take(frame.Length / 2).ToArray();

            MjpegFrameParser parser = null;
            List<byte[]> frames = Parse(Concat(truncated, frame), 4096, p => parser = p);

            Assert.AreEqual(1, frames.Count);
            Assert.True(frames[0].SequenceEqual(frame));
            Assert.AreEqual(truncated.Length, parser.DiscardedBytes);
        }

        [Test]
        public async Task TestReadAsync()
        {
            byte[] frame = RealFrame;
            List<byte[]> frames = new List<byte[]>();
            MjpegFrameParser parser = new MjpegFrameParser(frames.Add);

            await parser.ReadAsync(new MemoryStream(Concat(frame, frame)), CancellationToken.None);

            Assert.AreEqual(2, frames.Count);
            Assert.AreEqual(2, parser.FrameCount);
        }

        private static List<byte[]> Parse(byte[] data, int chunkSize, Action<MjpegFrameParser> parserCreated = null)
        {
            List<byte[]> frames = new List<byte[]>();
            MjpegFrameParser parser = new MjpegFrameParser(frames.Add);
            parserCreated?.Invoke(parser);

            for (int offset = 0; offset < data.Length; offset += chunkSize)
            {
                parser.Process(data, offset, Math.Min(chunkSize, data.Length - offset));
            }

            return frames;
        }

        private static byte[] Concat(params byte[][] parts)
        {
            return parts.SelectMany(p => p).ToArray();
        }
    }
}
