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
        private const int outPin02 = 02;
        private const int outPin03 = 03;
        private const int outPin04 = 04;
        private const int outPin05 = 05;
        private const int outPin06 = 06;
        private const int outPin07 = 07;
        private const int outPin08 = 08;
        private const int outPin09 = 09;
        private const int outPin10 = 10;
        private const int outPin11 = 11;
        private const int outPin12 = 12;
        private const int outPin13 = 13;
        private const int outPin14 = 14;
        private const int outPin15 = 15;
        private const int outPin16 = 16;
        private const int outPin17 = 17;


        private const int inPin18 = 18;
        private const int inPin19 = 19;
        private const int inPin20 = 20;
        private const int inPin21 = 21;
        private const int inPin22 = 22;
        private const int inPin23 = 23;
        private const int inPin24 = 24;
        private const int inPin25 = 25;
        private const int inPin26 = 26;
        private const int inPin27 = 27;

        private int _currentOutPin = 01;



        private int _inPinCounter;
        private int _lastInPin;

        private const string alert = "ALERT 🚨";
        private const string ready = "READY ✅";

        private Timer? _puseTimer;
        private bool _pulseValue;

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
                    this._controller.OpenPin(outPin02, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin03, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin04, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin05, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin06, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin07, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin08, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin09, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin10, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin11, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin12, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin13, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin14, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin15, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin16, PinMode.Output, PinValue.High);
                    this._controller.OpenPin(outPin17, PinMode.Output, PinValue.High);


                    this._controller.OpenPin(inPin18, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin19, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin20, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin21, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin22, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin23, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin24, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin25, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin26, PinMode.InputPullDown, PinValue.High);
                    this._controller.OpenPin(inPin27, PinMode.InputPullDown, PinValue.High);


                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin18, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin19, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin20, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin21, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin22, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin23, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin24, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin25, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin26, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inPin27, PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);


                    this._puseTimer = new Timer(state =>
                    {
                        //this._controller.Write(outPin02, this._pulseValue ? PinValue.High : PinValue.Low);
                        this._pulseValue = !this._pulseValue;

                        if (this._inPinCounter != 0)
                        {
                            if (this._inPinCounter > 10)
                            {
                                this.Logger.LogInformation("Counter 1: Pin={pin}, Count={count}", inPin18, this._inPinCounter);
                            }

                            this._inPinCounter = 0;
                        }


                        if (this._currentOutPin < 2)
                        {
                            this._controller.Write(17, PinValue.High);
                        }
                        else
                        {
                            this._controller.Write(this._currentOutPin, PinValue.High);
                        }

                        this._currentOutPin++;
                        this._controller.Write(this._currentOutPin, PinValue.Low);

                        if (this._currentOutPin > 16)
                        {
                            this._currentOutPin = 1;
                        }


                    }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

                }
                catch (Exception ex)
                {
                    this.Logger.LogError("Start failed! {ex}", ex.GetAllMessages());
                }

            });

        }

        public async Task StopAsync()
        {

            await Task.Run(() =>
            {
                try
                {
                    this._controller.Dispose();
                }
                catch (Exception ex)
                {
                    this.Logger.LogError("Stop failed! {ex}", ex.GetAllMessages());
                }
            });
        }


        private void OnPinEvent(object sender, PinValueChangedEventArgs args)
        {
            if (args.ChangeType != PinEventTypes.Rising)
            {
                this._inPinCounter++;
            }

            //if (this._inPinCounter > 10)
            //{
            //    return;
            //}

            if (this._lastInPin != args.PinNumber)
            {
                this.Logger.LogInformation("==============================================");
                this._lastInPin = args.PinNumber;
            }

            this.Logger.LogInformation("Pin={pin}, State={state}", args.PinNumber, args.ChangeType is PinEventTypes.Rising ? alert : ready);

            if (args.ChangeType != PinEventTypes.Rising) return;

            //var value = this._controller.Read(outPin02);
            //this._controller.Write(outPin02, value == PinValue.Low ? PinValue.High : PinValue.Low);
        }

    }
}