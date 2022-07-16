using System;

namespace PhotoBooth.Abstraction
{
    public interface IGpioInterface
    {
        event EventHandler<bool> PrimaryButtonChanged;

        event EventHandler<bool> SecondaryButtonChanged;
        
        void StartBlinkingPrimaryButton();

        void StopBlinkingPrimaryButton();

        void StartBlinkingSecondaryButton();

        void StopBlinkingSecondaryButton();
    }
}
