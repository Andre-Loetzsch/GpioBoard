using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;

namespace Oleander.GpioBoard;

internal class InputPin : IDisposable
{
    public event Action<PinValueChangedEventArgs>? ValueChanged;

    private static readonly Dictionary<int, InputPin> inputPins = new();
    private static Timer? timer;

    private bool _lastValue;

    public InputPin(int pinNumber, string? name = null)
    {
        if (pinNumber is < 18 or > 27)
            throw new ArgumentOutOfRangeException(nameof(pinNumber), pinNumber, "The PIN number must be between 18 and 27!");

        this.PinNumber = pinNumber;
        this.Name = name ?? $"Pin {pinNumber}";

        if (inputPins.ContainsKey(this.PinNumber))
            throw new ArgumentException($"Pin {pinNumber} already existed!", nameof(pinNumber));

        inputPins.Add(this.PinNumber, this);

        timer ??= new(_ =>
        {
            foreach (var inputPin in inputPins.Values)
            {
                inputPin.PublishValue();
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
        if (this._lastValue && args.ChangeType == PinEventTypes.Rising) return;

        this.BounceTime = DateTime.Now.Add(this.BounceTimeSpan);
        this._lastValue = args.ChangeType == PinEventTypes.Rising;
    }

    private void PublishValue()
    {
        if (this.Value == this._lastValue) return;
        if ((DateTime.Now - this.BounceTime).TotalMicroseconds < 1) return;

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