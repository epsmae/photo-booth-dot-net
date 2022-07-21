using System;
using System.Threading.Tasks;

namespace PhotoBooth.Abstraction
{
    public interface IWorkflowController
    {
        /// <summary>
        /// Notifies a state change of the state machine current state
        /// <see cref="State"/>
        /// </summary>
        event EventHandler StateChanged;

        /// <summary>
        /// Notifies a count down change before capture
        /// <see cref="CurrentCountDownStep"/>
        /// </summary>
        event EventHandler CountDownChanged;

        /// <summary>
        /// Notifies a count down change during review
        ///  <see cref="CurrentReviewCountDown"/>
        /// </summary>
        event EventHandler ReviewCountDownChanged;

        /// <summary>
        /// Current state of the state machine
        /// </summary>
        CaptureProcessState State { get; }

        /// <summary>
        /// Current count down step
        /// <see cref="CountDownChanged"/>
        /// </summary>
        int CurrentCountDownStep
        {
            get;
        }

        /// <summary>
        /// Current review count down
        /// <see cref="ReviewCountDownChanged"/>
        /// </summary>
        int CurrentReviewCountDown
        {
            get;
        }

        /// <summary>
        /// Contains the last exception
        /// </summary>
        Exception LastException
        {
            get;
        }

        /// <summary>
        /// File name of the last captured image
        /// </summary>
        string CurrentImageFileName
        {
            get;
        }

        /// <summary>
        /// Image data of the last captured image
        /// The preview size is taken from the configuration
        /// </summary>
        byte[] ImageData
        {
            get;
        }

        /// <summary>
        /// Trigger a capture command
        /// </summary>
        /// <returns></returns>
        Task Capture();

        /// <summary>
        /// Trigger a print command
        /// </summary>
        /// <returns></returns>
        Task Print();

        /// <summary>
        /// Trigger a skip command
        /// </summary>
        /// <returns></returns>
        Task Skip();

        /// <summary>
        /// Trigger a confirm error command
        /// </summary>
        /// <returns></returns>
        Task ConfirmError();

        /// <summary>
        /// Generate a UML Dot Graph state machine diagram
        /// </summary>
        /// <returns></returns>
        string GenerateUmlDiagram();
    }
}
