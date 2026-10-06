namespace PhotoBooth.Abstraction.LibGPhoto2
{
    public class CameraFileLocation
    {
        public CameraFileLocation(string folder, string name)
        {
            Folder = folder;
            Name = name;
        }

        public string Folder { get; }

        public string Name { get; }

        public override string ToString()
        {
            return $"{Folder}/{Name}";
        }
    }
}
