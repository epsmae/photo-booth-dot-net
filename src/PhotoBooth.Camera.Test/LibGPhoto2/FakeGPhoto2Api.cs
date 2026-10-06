using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using PhotoBooth.Abstraction.LibGPhoto2;

namespace PhotoBooth.Camera.Test.LibGPhoto2
{
    internal class FakeGPhoto2Api : IGPhoto2Api
    {
        private readonly ConcurrentQueue<(string Call, int ThreadId)> _calls = new ConcurrentQueue<(string, int)>();
        private int _previewCount;

        public bool IsConnected { get; private set; }

        public Func<Exception> ConnectFailure { get; set; }

        /// <summary>
        /// Called with the preview number (1 based), return an exception to fail this preview.
        /// </summary>
        public Func<int, Exception> PreviewFailure { get; set; }

        public int PreviewCount
        {
            get
            {
                return Volatile.Read(ref _previewCount);
            }
        }

        public IList<string> Calls
        {
            get
            {
                return _calls.Select(c => c.Call).ToList();
            }
        }

        public IList<int> ThreadIds
        {
            get
            {
                return _calls.Select(c => c.ThreadId).Distinct().ToList();
            }
        }

        public void Connect()
        {
            Record("Connect");
            Exception failure = ConnectFailure?.Invoke();

            if (failure != null)
            {
                throw failure;
            }

            IsConnected = true;
        }

        public void Disconnect()
        {
            Record("Disconnect");
            IsConnected = false;
        }

        public string GetModel()
        {
            Record("GetModel");
            return "Fake Camera";
        }

        public byte[] CapturePreview()
        {
            int number = Interlocked.Increment(ref _previewCount);
            Record("CapturePreview");
            Thread.Sleep(5);

            Exception failure = PreviewFailure?.Invoke(number);

            if (failure != null)
            {
                throw failure;
            }

            return new byte[] {0xFF, 0xD8, (byte) number, 0xFF, 0xD9};
        }

        public CameraFileLocation CaptureImage()
        {
            Record("CaptureImage");
            Thread.Sleep(20);
            return new CameraFileLocation("/DCIM/100NCD70", "DSC_0001.JPG");
        }

        public void DownloadFile(CameraFileLocation location, string targetFile)
        {
            Record("DownloadFile");
            File.WriteAllBytes(targetFile, new byte[] {0xFF, 0xD8, 0xFF, 0xD9});
        }

        public void SetConfig(string name, string value)
        {
            Record($"SetConfig({name}={value})");
        }

        public void Dispose()
        {
            Record("Dispose");
        }

        private void Record(string call)
        {
            _calls.Enqueue((call, Environment.CurrentManagedThreadId));
        }
    }
}
