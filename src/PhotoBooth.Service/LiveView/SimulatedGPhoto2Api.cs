using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using PhotoBooth.Abstraction.LibGPhoto2;

namespace PhotoBooth.Service.LiveView
{
    /// <summary>
    /// Simulated libgphoto2 camera (Nikon D7000 like timing) for development without a camera.
    /// </summary>
    public sealed class SimulatedGPhoto2Api : IGPhoto2Api
    {
        private const int PreviewWidth = 640;
        private const int PreviewHeight = 424;
        private static readonly TimeSpan PreviewInterval = TimeSpan.FromMilliseconds(66);
        private static readonly TimeSpan ConnectDuration = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan CaptureDuration = TimeSpan.FromMilliseconds(600);

        private readonly SimulatedFrameRenderer _previewRenderer = new SimulatedFrameRenderer(PreviewWidth, PreviewHeight);
        private readonly SimulatedFrameRenderer _photoRenderer = new SimulatedFrameRenderer(1600, 1060);
        private readonly Stopwatch _sinceLastPreview = Stopwatch.StartNew();
        private long _frameNumber;
        private int _imageNumber;

        public bool IsConnected { get; private set; }

        public bool InLiveView { get; private set; }

        public void Connect()
        {
            Thread.Sleep(ConnectDuration);
            IsConnected = true;
        }

        public void Disconnect()
        {
            IsConnected = false;
            InLiveView = false;
        }

        public string GetModel()
        {
            EnsureConnected();
            return "Nikon DSC D7000 (simulated)";
        }

        public byte[] CapturePreview()
        {
            EnsureConnected();

            if (!InLiveView)
            {
                // mirror up
                Thread.Sleep(500);
                InLiveView = true;
            }

            TimeSpan wait = PreviewInterval - _sinceLastPreview.Elapsed;

            if (wait > TimeSpan.Zero)
            {
                Thread.Sleep(wait);
            }

            _sinceLastPreview.Restart();
            return _previewRenderer.Render("LIBGPHOTO2 SIMULATOR", ++_frameNumber, 70);
        }

        public CameraFileLocation CaptureImage()
        {
            EnsureConnected();
            Thread.Sleep(CaptureDuration);
            return new CameraFileLocation("/store_00010001/DCIM/100NCD70", $"DSC_{++_imageNumber:D4}.JPG");
        }

        public void DownloadFile(CameraFileLocation location, string targetFile)
        {
            EnsureConnected();
            File.WriteAllBytes(targetFile, _photoRenderer.Render($"CAPTURED {location.Name}", _imageNumber, 90));
        }

        public void SetConfig(string name, string value)
        {
            EnsureConnected();
        }

        public void Dispose()
        {
            _previewRenderer.Dispose();
            _photoRenderer.Dispose();
        }

        private void EnsureConnected()
        {
            if (!IsConnected)
            {
                throw new GPhoto2Exception(GPhoto2Exception.ErrorModelNotFound, "Camera not connected");
            }
        }
    }
}
