using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.Abstractions;
using System.Device.Gpio;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;
using System.IO;

namespace Oleander.GpioBoard;
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


    private readonly InputPin _inPin18;
    private readonly InputPin _inPin19;
    private readonly InputPin _inPin20;
    private readonly InputPin _inPin21; //Klo Taster
    private readonly InputPin _inPin22;
    private readonly InputPin _inPin23;
    private readonly InputPin _inPin24;
    private readonly InputPin _inPin25;
    private readonly InputPin _inPin26;
    private readonly InputPin _inPin27;


    private readonly Dictionary<int, InputPin> _inputPins = new();

    private string[] _inpstatus = new string[10];

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

        this._inPin18 = new InputPin(this.Logger, 18);
        this._inPin19 = new InputPin(this.Logger, 19); // rot
        this._inPin20 = new InputPin(this.Logger, 20); // ws
        this._inPin21 = new InputPin(this.Logger, 21); // ge
        this._inPin22 = new InputPin(this.Logger, 22); // ws
        this._inPin23 = new InputPin(this.Logger, 23); // gn
        this._inPin24 = new InputPin(this.Logger, 24); // ws
        this._inPin25 = new InputPin(this.Logger, 25); // br
        this._inPin26 = new InputPin(this.Logger, 26); // rt
        this._inPin27 = new InputPin(this.Logger, 27); // sw

    }

    internal readonly ILogger Logger;

    public async Task StartAsync()
    {
        await Task.Run(() =>
        {
            try
            {

                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RawInputs"), "");

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


                this._inputPins.Add(this._inPin18.PinNumber, this._inPin18);
                this._inputPins.Add(this._inPin19.PinNumber, this._inPin19);
                this._inputPins.Add(this._inPin20.PinNumber, this._inPin20);
                this._inputPins.Add(this._inPin21.PinNumber, this._inPin21);
                this._inputPins.Add(this._inPin22.PinNumber, this._inPin22);
                this._inputPins.Add(this._inPin23.PinNumber, this._inPin23);
                this._inputPins.Add(this._inPin24.PinNumber, this._inPin24);
                this._inputPins.Add(this._inPin25.PinNumber, this._inPin25);
                this._inputPins.Add(this._inPin26.PinNumber, this._inPin26);
                this._inputPins.Add(this._inPin27.PinNumber, this._inPin27);


                // LS WC
                this._inPin20.ValueChanged += args =>
                {
                    // Dusche ein?
                    if (this._controller.Read(this._inPin27.PinNumber) == PinValue.High) return;
                    // WC-Tür offen?
                    if (this._controller.Read(this._inPin23.PinNumber) == PinValue.Low) return;

                    // LK WC
                    //this._controller.Write(outPin06, args.ChangeType == PinEventTypes.Rising ? PinValue.High : PinValue.Low);

                    // LK Boden
                    //this._controller.Write(outPin07, args.ChangeType == PinEventTypes.Rising ? PinValue.High : PinValue.Low);


                    // Lüfter ein
                    if (args.ChangeType == PinEventTypes.Rising)
                    {
                        // Lüfter ein
                        this._controller.Write(outPin03, PinValue.Low);

                        // LK Boden / Keller
                        this._controller.Write(outPin10, PinValue.Low);

                        // WC-Beckern
                        this._controller.Write(outPin11,  PinValue.High);

                        // LK WC Zu
                        this._controller.Write(outPin06, PinValue.Low);
                    }
                    else
                    {
                        // LK WC ??
                        //this._controller.Write(outPin06, PinValue.High);
                    }

                    //this._controller.Write(outPin04, args.ChangeType == PinEventTypes.Rising ? PinValue.High : PinValue.Low);

                };

                // Klo-Tür
                this._inPin23.ValueChanged += args =>
                {
                    // Lüfter aus
                    this._controller.Write(outPin03, PinValue.High);
                };

                // Dusche
                this._inPin27.ValueChanged += args =>
                {
                    // LS WC
                    if (this._controller.Read(this._inPin20.PinNumber) == PinValue.High) return;

                    // Lüfter
                    this._controller.Write(outPin03, args.ChangeType == PinEventTypes.Rising ? PinValue.Low : PinValue.High);

                    if (args.ChangeType == PinEventTypes.Rising)
                    {

                        // LK Boden / Keller
                        this._controller.Write(outPin10, PinValue.High);

                        // WC-Beckern
                        this._controller.Write(outPin11, PinValue.Low);

                        // LK WC zu
                        this._controller.Write(outPin06, PinValue.Low);
                    }
                };






                // Taster
                this._inPin21.ValueChanged += args =>
                {
                    // Trafo 26V~
                    this._controller.Write(outPin02, args.ChangeType == PinEventTypes.Rising ? PinValue.High : PinValue.Low);

                    //// Lüfter
                    //this._controller.Write(outPin03, args.ChangeType == PinEventTypes.Rising ? PinValue.Low : PinValue.High);
                    //this._controller.Write(outPin04, args.ChangeType == PinEventTypes.Rising ? PinValue.High : PinValue.Low);
                };






                foreach (var inputPins in this._inputPins.Values)
                {
                    this._inpstatus[inputPins.PinNumber - 18] = string.Empty;

                    inputPins.ValueChanged += args =>
                    {
                        this._inpstatus[args.PinNumber - 18] = $"{DateTime.Now:yyyy.MM.dd HH:mm:ss fff} {args.PinNumber}:{(args.ChangeType is PinEventTypes.Rising ? "1" : "0")}";
                        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Inputs"), string.Join(Environment.NewLine, this._inpstatus));

                        var logMsg = this._lastInPin != args.PinNumber ? string.Concat("==============================================", Environment.NewLine) : string.Empty;
                        logMsg = string.Concat(logMsg, $"Pin={args.PinNumber}, State={(args.ChangeType is PinEventTypes.Rising ? alert : ready)}");
                        this.Logger.LogInformation(logMsg);

                        this._lastInPin = args.PinNumber;
                    };

                    this._controller.OpenPin(inputPins.PinNumber, PinMode.InputPullDown, PinValue.Low);
                    this._controller.RegisterCallbackForPinValueChangedEvent(inputPins.PinNumber,
                            PinEventTypes.Falling | PinEventTypes.Rising, this.OnPinEvent);
                    
                }


                //this._puseTimer = new Timer(state =>
                //{
                //    try
                //    {
                //        this._pulseValue = !this._pulseValue;

                //        if (this._inPinCounter != 0)
                //        {
                //            if (this._inPinCounter > 10)
                //            {
                //                this.Logger.LogInformation("inPinCounter={count}", this._inPinCounter);
                //            }

                //            this._inPinCounter = 0;
                //        }

                //        if (this._currentOutPin < 2)
                //        {
                //            this._controller.Write(17, PinValue.High);
                //        }
                //        else
                //        {
                //            this._controller.Write(this._currentOutPin, PinValue.High);
                //        }

                //        this._currentOutPin++;
                //        this._controller.Write(this._currentOutPin, PinValue.Low);

                //        if (this._currentOutPin > 16)
                //        {
                //            this._currentOutPin = 1;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        this.Logger.LogError("useTimer caused an exception! {ex}", ex.GetAllMessages());
                //    }


                //}, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

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
        if (args.ChangeType == PinEventTypes.Rising) this._inPinCounter++;


        if (!this._inputPins.TryGetValue(args.PinNumber, out var inputPin))
        {
            this.Logger.LogWarning("Pin {pin} does not exist! State={state}", args.PinNumber, args.ChangeType is PinEventTypes.Rising ? alert : ready);
            return;
        }

        //this.Logger.LogInformation("Pin {pin}, State={state}", args.PinNumber, args.ChangeType is PinEventTypes.Rising ? alert : ready);

        try
        {
            inputPin.CallbackForPinValueChangedEvent(sender, args);

            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RawInputs"),
                $"{DateTime.Now:yyyy.MM.dd HH:mm:ss fff}:{args.PinNumber}:{args.ChangeType}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            this.Logger.LogError("OnPinEvent caused an exception! {ex}", ex.GetAllMessages());
        }
    }
}