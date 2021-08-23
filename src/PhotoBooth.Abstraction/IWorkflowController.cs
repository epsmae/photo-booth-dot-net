using System;
using System.Threading.Tasks;

namespace PhotoBooth.Abstraction
{
    public interface IWorkflowController
    {
        CaptureProcessState State { get; }

        Task Capture();

        Task Print();
        
        int CurrentCountDownStep { get; }

        Exception LastException { get; }

        string CurrentImageFileName { get; }

        byte[] ImageData { get; }
        int CurrentReviewCountDown { get; }

        event EventHandler StateChanged;

        event EventHandler CountDownChanged;

        Task ConfirmError();
        
        event EventHandler ReviewCountDownChanged;
        string GenerateUmlDiagram();
    }
}
