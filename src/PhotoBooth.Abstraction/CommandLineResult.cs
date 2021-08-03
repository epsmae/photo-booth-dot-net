namespace PhotoBooth.Abstraction
{
    public class CommandLineResult
    {
        public int ExitCode
        {
            get;
            set;
        }

        public string StandardError
        {
            get;
            set;
        }

        public string StandardOutput
        {
            get;
            set;
        }
    }
}
