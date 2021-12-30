using System.Collections.Generic;
using System.Threading.Tasks;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class UsbServiceStub : IUsbService
    {
        public  Task<List<string>> ListUsbDevices()
        {
            return Task.FromResult(new List<string>
            {
                "Bus 001 DeviceX 002: ID eeee:0021 Camera",
                "Bus 002 DeviceX 003: ID eeee:0023 Printer"
            });
        }
    }
}
