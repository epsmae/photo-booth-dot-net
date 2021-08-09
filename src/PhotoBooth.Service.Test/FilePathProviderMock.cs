using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service.Test
{
    public class FilePathProviderMock
    {
        private readonly Mock<IFilePathProvider> _mock;

        private string _executionDirectory;

        public FilePathProviderMock()
        {
            _mock = new Mock<IFilePathProvider>();
            _mock.Setup(m => m.ExecutionDirectory).Returns(()=> _executionDirectory);
        }

        public string ExecutionDirectory
        {
            get
            {
                return _executionDirectory;
            }
            set
            {
                _executionDirectory = value;
            }
        }

        public IFilePathProvider Object
        {
            get
            {
                return _mock.Object;
            }
        }
    }
}
