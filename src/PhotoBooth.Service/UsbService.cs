using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using PhotoBooth.Abstraction;

namespace PhotoBooth.Service
{
    public class UsbService : IUsbService
    {
        private readonly ILogger<UsbService> _logger;
        
        public UsbService(ILogger<UsbService> logger)
        {
            _logger = logger;
        }
        
        public async Task<List<string>> ListUsbDevices()
        {
            _logger.LogInformation($"Fetch USB devices --> lsusb");

            BufferedCommandResult result = await Cli.Wrap("lsusb")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            List<string> items = new List<string>();
            foreach (string device in result.StandardOutput.Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            {
                items.Add(device);
            }

            return items;
        }
    }
}
