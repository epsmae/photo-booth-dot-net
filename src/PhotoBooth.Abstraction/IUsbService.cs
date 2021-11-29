using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoBooth.Abstraction
{
    public interface IUsbService
    {
        Task<List<string>> ListUsbDevices();
    }
}
