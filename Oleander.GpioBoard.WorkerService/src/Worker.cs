namespace Oleander.GpioBoard.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Board _board = new ();

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime lifetime)
        {
            this._logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            await this._board.StartAsync();

            //stoppingToken.WaitHandle.WaitOne();
            this._logger.LogInformation("Worker started async: {time}", DateTimeOffset.Now);


            await Task.Delay(Timeout.Infinite, stoppingToken);

            await this._board.StopAsync();

            this._logger.LogInformation("Worker stopped async: {time}", DateTimeOffset.Now);


            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    this._logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    await Task.Delay(1000, stoppingToken);
            //}
        }


    }
}