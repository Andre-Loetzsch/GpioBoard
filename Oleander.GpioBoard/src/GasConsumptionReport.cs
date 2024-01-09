using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Oleander.Extensions.Logging.Abstractions;

namespace Oleander.GpioBoard;

internal class GasConsumptionReport
{
    private readonly ILogger _logger;
    private readonly string _reportingDir;
    private int _reportedHour = DateTime.Now.Hour;
    private int _reportedDay = DateTime.Now.Day;
    private int _reportedMonth = DateTime.Now.Month;
    private int _reportedYear = DateTime.Now.Year;
    private bool _underGasMeterIncreased;

    private DateTime _mainGasMeterIncreasedTime = DateTime.Now;
    private DateTime _underGasMeterIncreasedTime = DateTime.Now;

    public GasConsumptionReport(ILogger logger, string reportingDir)
    {
        this._logger = logger;
        this._reportingDir = reportingDir;
        if (!Directory.Exists(this._reportingDir)) Directory.CreateDirectory(this._reportingDir);

        var meterReadingFileName = Path.Combine(this._reportingDir, "meterReading");
        if (!File.Exists(meterReadingFileName)) File.WriteAllLines(meterReadingFileName, new[] { "3451985", "63414412" });

        var fileContent = File.ReadAllLines(meterReadingFileName);

        this.MainMeterReading = fileContent.Length > 0 && long.TryParse(fileContent[0], CultureInfo.InvariantCulture, out var value) ? value : 0;
        this.UnderMeterReading = fileContent.Length > 1 && long.TryParse(fileContent[1], CultureInfo.InvariantCulture, out value) ? value : 0;
    }

    public long MainMeterReading { get; internal set; }
    public long UnderMeterReading { get; internal set; }

    public long IncreaseMainGasMeter()
    {
        this.MainMeterReading += 10;

        try
        {
            this.ReportConsumption(true);
        }
        catch (Exception ex)
        {
            this._logger.LogError("Exc: {exception}", ex.GetAllMessages());
        }

        return this.MainMeterReading;
    }

    public long IncreaseUnderGasMeter()
    {
        this.UnderMeterReading += 10;

        try
        {
            this.ReportConsumption(false);
        }
        catch (Exception ex)
        {
            this._logger.LogError("Exc: {exception}", ex.GetAllMessages());
        }

        return this.UnderMeterReading;
    }

    private void ReportConsumption(bool mainGasMeter)
    {
        var fileName = Path.Combine(this._reportingDir, "setup");

        if (File.Exists(fileName))
        {
            var fileContent = File.ReadAllLines(fileName);

            if (fileContent.Length > 0 && long.TryParse(fileContent[0], CultureInfo.InvariantCulture, out var value))
            {
                this.MainMeterReading = value;
            }

            if (fileContent.Length > 1 && long.TryParse(fileContent[1], CultureInfo.InvariantCulture, out value))
            {
                this.UnderMeterReading = value;
            }

            if (mainGasMeter)
            {
                this.MainMeterReading++;
            }
            else
            {
                this.UnderMeterReading++;
            }

            File.Delete(fileName);
        }

        fileName = Path.Combine(this._reportingDir, "meterReading");

        File.WriteAllLines(fileName, new[]
        {
            this.MainMeterReading.ToString(CultureInfo.InvariantCulture),
            this.UnderMeterReading.ToString(CultureInfo.InvariantCulture)
        });

        fileName = Path.Combine(this._reportingDir, "cache");

        var reportCounters = new long[8];


        if (File.Exists(fileName))
        {
            var fileContent = File.ReadAllLines(fileName);
            reportCounters[0] = fileContent.Length > 0 && long.TryParse(fileContent[0], out var value) ? value : 0;
            reportCounters[1] = fileContent.Length > 1 && long.TryParse(fileContent[1], CultureInfo.InvariantCulture, out value) ? value : 0;
            reportCounters[2] = fileContent.Length > 2 && long.TryParse(fileContent[2], CultureInfo.InvariantCulture, out value) ? value : 0;
            reportCounters[3] = fileContent.Length > 3 && long.TryParse(fileContent[3], CultureInfo.InvariantCulture, out value) ? value : 0;
            reportCounters[4] = fileContent.Length > 4 && long.TryParse(fileContent[4], CultureInfo.InvariantCulture, out value) ? value : 0;
            reportCounters[5] = fileContent.Length > 5 && long.TryParse(fileContent[5], CultureInfo.InvariantCulture, out value) ? value : 0;
            reportCounters[6] = fileContent.Length > 6 && long.TryParse(fileContent[6], CultureInfo.InvariantCulture, out value) ? value : 0;
            reportCounters[7] = fileContent.Length > 7 && long.TryParse(fileContent[7], CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        if (this._reportedHour != DateTime.Now.Hour)
        {
            this._reportedHour = DateTime.Now.Hour;
            reportCounters[0] = 0;
            reportCounters[1] = 0;
        }

        if (this._reportedDay != DateTime.Now.Day)
        {
            this._reportedDay = DateTime.Now.Day;
            reportCounters[2] = 0;
            reportCounters[3] = 0;
        }

        if (this._reportedMonth != DateTime.Now.Month)
        {
            this._reportedMonth = DateTime.Now.Month;
            reportCounters[4] = 0;
            reportCounters[5] = 0;
        }

        if (this._reportedYear != DateTime.Now.Year)
        {
            this._reportedYear = DateTime.Now.Year;
            reportCounters[6] = 0;
            reportCounters[7] = 0;
        }

        var diffMainTime = TimeSpan.Zero;
        var diffUnderTime = TimeSpan.Zero;

        if (mainGasMeter)
        {
            if (this._underGasMeterIncreased)
            {
                this._underGasMeterIncreased = false;
            }
            else
            {
                reportCounters[0] += 10;
                reportCounters[2] += 10;
                reportCounters[4] += 10;
                reportCounters[6] += 10;
                diffMainTime = DateTime.Now - this._mainGasMeterIncreasedTime;
                this._mainGasMeterIncreasedTime = DateTime.Now;
            }
        }
        else
        {
            this._underGasMeterIncreased = true;

            reportCounters[1] += 10;
            reportCounters[3] += 10;
            reportCounters[5] += 10;
            reportCounters[7] += 10;
            diffUnderTime = DateTime.Now - this._underGasMeterIncreasedTime;
            this._underGasMeterIncreasedTime = DateTime.Now;
        }

        File.WriteAllLines(fileName, reportCounters.Select(x => x.ToString(CultureInfo.InvariantCulture)));


        fileName = Path.Combine(this._reportingDir, $"hour_{DateTime.Now:yyyyMMdd}");
        var fileContent1 = File.Exists(fileName) ? File.ReadAllLines(fileName).ToList() : new List<string>();

        while (fileContent1.Count < 25)
        {
            fileContent1.Add("");
        }

        fileContent1[DateTime.Now.Hour] = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{reportCounters[0],10}{reportCounters[1],10}";
        File.WriteAllLines(fileName, fileContent1);

        fileName = Path.Combine(this._reportingDir, $"day_{DateTime.Now:yyyy}");

        fileContent1 = File.Exists(fileName) ? File.ReadAllLines(fileName).ToList() : new List<string>();

        while (fileContent1.Count < 366)
        {
            fileContent1.Add("");
        }

        fileContent1[DateTime.Now.DayOfYear] = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{reportCounters[2],10}{reportCounters[3],10}";
        File.WriteAllLines(fileName, fileContent1);


        fileName = Path.Combine(this._reportingDir, $"month_{DateTime.Now:yyyy}");

        fileContent1 = File.Exists(fileName) ? File.ReadAllLines(fileName).ToList() : new List<string>();

        while (fileContent1.Count < 13)
        {
            fileContent1.Add("");
        }

        fileContent1[DateTime.Now.Month] = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{reportCounters[4],10}{reportCounters[5],10}";
        File.WriteAllLines(fileName, fileContent1);

        fileName = Path.Combine(this._reportingDir, "year");

        fileContent1 = File.Exists(fileName) ? File.ReadAllLines(fileName).ToList() : new List<string>();

        while (fileContent1.Count < DateTime.Now.Year - 2022)
        {
            fileContent1.Add("");
        }

        fileContent1[DateTime.Now.Year - 2023] = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{reportCounters[6],10}{reportCounters[7],10}";
        File.WriteAllLines(fileName, fileContent1);


        fileName = Path.Combine(this._reportingDir, "report.log");

        File.AppendAllLines(fileName, new[]
        {
            "",
            $"{DateTime.Now:yyyy.MM.dd HH:mm:ss}{this.MainMeterReading.ToString(CultureInfo.InvariantCulture),15}{this.UnderMeterReading.ToString(CultureInfo.InvariantCulture),15}",
            $"Elapsed time           {diffMainTime:dd\\.hh\\:mm\\:ss}    {diffUnderTime:dd\\.hh\\:mm\\:ss}",
            $"{DateTime.Now:HH}                 {reportCounters[0].ToString(CultureInfo.InvariantCulture),15}{reportCounters[1].ToString(CultureInfo.InvariantCulture),15}",
            $"{DateTime.Now:dd}                 {reportCounters[2].ToString(CultureInfo.InvariantCulture),15}{reportCounters[3].ToString(CultureInfo.InvariantCulture),15}",
            $"{DateTime.Now:MM}                 {reportCounters[4].ToString(CultureInfo.InvariantCulture),15}{reportCounters[5].ToString(CultureInfo.InvariantCulture),15}",
            $"{DateTime.Now:yyyy}               {reportCounters[6].ToString(CultureInfo.InvariantCulture),15}{reportCounters[7].ToString(CultureInfo.InvariantCulture),15}"
        });

    }
}