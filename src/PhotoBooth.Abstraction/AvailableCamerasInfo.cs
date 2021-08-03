using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoBooth.Abstraction
{
    public class AvailableCamerasInfo
    {
        public AvailableCamerasInfo()
        {
            AvailableCameras = new List<CameraInfo>();
        }

        public IList<CameraInfo> AvailableCameras { get; set; } 
    }
}
