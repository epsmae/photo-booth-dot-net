namespace PhotoBooth.Abstraction
{
    public class CameraInfo
    {
        public string CameraModel
        {
            get;
            set;
        }
        public string Port
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{CameraModel}, {Port}";
        }
    }
}
