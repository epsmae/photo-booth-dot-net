namespace PhotoBooth.Abstraction.LiveView
{
    public class LiveViewFrame
    {
        public LiveViewFrame(long id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        public long Id { get; }

        public byte[] Data { get; }
    }
}
