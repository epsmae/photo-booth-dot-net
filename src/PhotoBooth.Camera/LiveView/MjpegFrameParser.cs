using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoBooth.Camera.LiveView
{
    /// <summary>
    /// Splits a stream of concatenated JPEG images (as written by "gphoto2 --capture-movie --stdout")
    /// into single frames.
    /// The JPEG marker structure is followed instead of searching for the end marker (FF D9), so
    /// embedded thumbnails (e.g. in an EXIF segment) do not split a frame.
    /// </summary>
    public class MjpegFrameParser
    {
        // protects against unbounded growth if the stream contains garbage
        private const int MaxFrameSize = 16 * 1024 * 1024;

        private enum State
        {
            SeekStartOfImage,
            Marker,
            LengthHigh,
            LengthLow,
            SkipSegment,
            EntropyCodedData
        }

        private readonly Action<byte[]> _onFrame;
        private readonly MemoryStream _frame = new MemoryStream(256 * 1024);

        private State _state = State.SeekStartOfImage;
        private bool _previousWasFf;
        private byte _currentMarker;
        private int _segmentLength;
        private int _remaining;

        public MjpegFrameParser(Action<byte[]> onFrame)
        {
            _onFrame = onFrame;
        }

        public long FrameCount { get; private set; }

        public long DiscardedBytes { get; private set; }

        public async Task ReadAsync(Stream stream, CancellationToken token)
        {
            byte[] buffer = new byte[64 * 1024];
            int read;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
            {
                Process(buffer, 0, read);
            }
        }

        public void Process(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
            {
                Process(buffer[i]);
            }
        }

        private void Process(byte b)
        {
            if (_state != State.SeekStartOfImage && _frame.Length >= MaxFrameSize)
            {
                Reset();
            }

            switch (_state)
            {
                case State.SeekStartOfImage:
                    if (_previousWasFf && b == 0xD8)
                    {
                        // the preceding FF was counted as discarded but belongs to the frame
                        DiscardedBytes--;
                        _frame.SetLength(0);
                        _frame.WriteByte(0xFF);
                        _frame.WriteByte(0xD8);
                        _previousWasFf = false;
                        _state = State.Marker;
                        return;
                    }

                    DiscardedBytes++;
                    _previousWasFf = b == 0xFF;
                    return;

                case State.Marker:
                    Append(b);

                    if (!_previousWasFf)
                    {
                        if (b == 0xFF)
                        {
                            _previousWasFf = true;
                        }
                        else
                        {
                            // invalid structure, start over
                            Reset();
                        }

                        return;
                    }

                    if (b == 0xFF)
                    {
                        // fill byte
                        return;
                    }

                    _previousWasFf = false;
                    HandleMarker(b);
                    return;

                case State.LengthHigh:
                    Append(b);
                    _segmentLength = b << 8;
                    _state = State.LengthLow;
                    return;

                case State.LengthLow:
                    Append(b);
                    _segmentLength |= b;

                    if (_segmentLength < 2)
                    {
                        Reset();
                        return;
                    }

                    _remaining = _segmentLength - 2;
                    _state = _remaining > 0 ? State.SkipSegment : AfterSegment();
                    return;

                case State.SkipSegment:
                    Append(b);
                    _remaining--;

                    if (_remaining == 0)
                    {
                        _state = AfterSegment();
                    }

                    return;

                case State.EntropyCodedData:
                    Append(b);

                    if (!_previousWasFf)
                    {
                        _previousWasFf = b == 0xFF;
                        return;
                    }

                    if (b == 0xFF)
                    {
                        // fill byte
                        return;
                    }

                    _previousWasFf = false;

                    if (b == 0x00 || (b >= 0xD0 && b <= 0xD7))
                    {
                        // stuffed byte or restart marker, still entropy coded data
                        return;
                    }

                    // a real marker: end of image or e.g. the next scan of a progressive JPEG
                    HandleMarker(b);
                    return;
            }
        }

        private void HandleMarker(byte marker)
        {
            if (marker == 0xD9)
            {
                EmitFrame();
                return;
            }

            if (marker == 0xD8)
            {
                // start of a new image before the current one ended (truncated frame), restart
                DiscardedBytes += _frame.Length - 2;
                _frame.SetLength(0);
                _frame.WriteByte(0xFF);
                _frame.WriteByte(0xD8);
                _state = State.Marker;
                return;
            }

            if (marker == 0x01 || (marker >= 0xD0 && marker <= 0xD7))
            {
                // markers without length
                _state = State.Marker;
                return;
            }

            _currentMarker = marker;
            _state = State.LengthHigh;
        }

        private State AfterSegment()
        {
            // the start of scan header is followed by the entropy coded image data
            return _currentMarker == 0xDA ? State.EntropyCodedData : State.Marker;
        }

        private void Append(byte b)
        {
            _frame.WriteByte(b);
        }

        private void EmitFrame()
        {
            byte[] frame = _frame.ToArray();
            StartOver();
            FrameCount++;
            _onFrame(frame);
        }

        private void Reset()
        {
            // the current frame is invalid
            DiscardedBytes += _frame.Length;
            StartOver();
        }

        private void StartOver()
        {
            _frame.SetLength(0);
            _previousWasFf = false;
            _state = State.SeekStartOfImage;
        }
    }
}
