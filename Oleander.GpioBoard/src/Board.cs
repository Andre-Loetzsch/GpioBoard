using System;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.Abstractions;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Oleander.GpioBoard
{
    // https://learn.microsoft.com/de-de/dotnet/iot/tutorials/gpio-input?source=recommendations
    // https://learn.microsoft.com/de-de/dotnet/iot/debugging?tabs=self-contained&pivots=vscode

    public class Board
    {
        private const int pin = 21;
        private const string alert = "ALERT 🚨";
        private const string ready = "READY ✅";

        private readonly GpioController _controller = new();

        public Board()
        {
            this.Logger = LoggerFactory.CreateLogger<Board>();
        }

        internal readonly ILogger Logger;


        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    this._controller.OpenPin(pin, PinMode.InputPullUp);
                    this.Logger.LogInformation("Initial status: Pin={pin}, State={state}", pin, (this._controller.Read(pin) == PinValue.High ? alert : ready));
                    this._controller.RegisterCallbackForPinValueChangedEvent(pin, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError("Start failed! {ex}",  ex.GetAllMessages());
                }
             
            });

        }


        public async Task StopAsync()
        {

            await Task.Run(() =>
            {
                try
                {
                    this._controller.UnregisterCallbackForPinValueChangedEvent(pin, this.OnPinEvent);
                    this._controller.ClosePin(pin);

                }
                catch (Exception ex)
                {
                    this.Logger.LogError("Start failed! {ex}", ex.GetAllMessages());
                }
            });
        }


        private void OnPinEvent(object sender, PinValueChangedEventArgs args)
        {
            this.Logger.LogInformation("Pin={pin}, State={state}", args.PinNumber, args.ChangeType is PinEventTypes.Rising ? alert : ready);
        }

    }
}