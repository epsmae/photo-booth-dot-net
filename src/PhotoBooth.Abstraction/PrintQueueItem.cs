namespace PhotoBooth.Abstraction
{
    public class PrintQueueItem
    {
        public string Name
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
