using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.Abstractions;

namespace Oleander.GpioBoard;

internal class InputPin : IDisposable
{
    private readonly ILogger _logger;
    public event Action<PinValueChangedEventArgs>? ValueChanged;

    private static readonly Dictionary<int, InputPin> inputPins = new();
    private static Timer? timer;

    private readonly List<int> _pinChanges = new();
    private bool _lastValue;

    public InputPin(ILogger logger, int pinNumber, string? name = null)
    {
        if (pinNumber is < 18 or > 27)
            throw new ArgumentOutOfRangeException(nameof(pinNumber), pinNumber, "The PIN number must be between 18 and 27!");
        this._logger = logger;

        this.PinNumber = pinNumber;
        this.Name = name ?? $"Pin {pinNumber}";

        if (inputPins.ContainsKey(this.PinNumber))
            throw new ArgumentException($"Pin {pinNumber} already existed!", nameof(pinNumber));

        inputPins.Add(this.PinNumber, this);

        timer ??= new(_ =>
        {
            foreach (var inputPin in inputPins.Values)
            {
                try
                {
                    inputPin.PublishValue();
                }
                catch (Exception ex)
                {
                    this._logger.LogError("PublishValue caused an exception! {ex}", ex.GetAllMessages());
                }
            }

        }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

    }

    public DateTime BounceTime { get; private set; } = DateTime.Now;

    public TimeSpan BounceTimeSpan { get; set; } = TimeSpan.FromMicroseconds(200);

    public int PinNumber { get; }

    public bool Value { get; private set; }

    public string Name { get; }

    public void CallbackForPinValueChangedEvent(object sender, PinValueChangedEventArgs args)
    {
        if (this.PinNumber != args.PinNumber) return;

        lock (this._pinChanges)
        {
            this._pinChanges.Add(args.ChangeType == PinEventTypes.Rising ? 1 : 0);
        }

        if (this._lastValue && args.ChangeType == PinEventTypes.Rising) return;

        //this._logger.LogInformation("CallbackForPinValueChangedEvent: {PinNumber}", this.PinNumber);

        this.BounceTime = DateTime.Now.Add(this.BounceTimeSpan);
        this._lastValue = args.ChangeType == PinEventTypes.Rising;
    }

    private void PublishValue()
    {
        if (this.Value == this._lastValue) return;
        if ((DateTime.Now - this.BounceTime).TotalMicroseconds < 1) return;

        lock (this._pinChanges)
        {
            var pinChangedList = this._pinChanges.ToList();
            this._logger.LogInformation("Pin={pin}, PublishValue={value} -> {lastValue} {pinChanges}", this.PinNumber,
                this.Value, this._lastValue, string.Join(":", pinChangedList));

            this._pinChanges.Clear();
        }

        this.Value = this._lastValue;
        this.ValueChanged?.Invoke(new(this.Value ? PinEventTypes.Rising : PinEventTypes.Falling, this.PinNumber));
    }

    #region IDisposable

    private void ReleaseThis()
    {
        inputPins.Remove(this.PinNumber);

        if (inputPins.Count > 0) return;
        timer?.Dispose();
    }

    public void Dispose()
    {
        this.ReleaseThis();
        GC.SuppressFinalize(this);
    }

    ~InputPin()
    {
        this.ReleaseThis();
    }

    #endregion
} 