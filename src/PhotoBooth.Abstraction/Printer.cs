namespace PhotoBooth.Abstraction
{
    public class Printer
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
