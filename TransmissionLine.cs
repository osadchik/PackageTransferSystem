using PackageTransferSystem;
using PackageTransferSystem.Extensions;

public class TransmissionLine
{
    public string Name { get; }
    public double MinTransmissionTime { get; set; }
    public double MaxTransmissionTime { get; set; }

    private SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

    public TransmissionLine(string name, double minTime, double maxTime)
    {
        Name = name;
        MinTransmissionTime = minTime;
        MaxTransmissionTime = maxTime;
    }

    public async Task TransmitAsync(Package package, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            Console.Out.WriteLineWithUniqueColor(package.Id.ToString(), $"{package.Id}. Started package transmission on line {Name}.");

            double transmissionTime = transmissionTime = GetTransmissionTime();
            await Task.Delay(TimeSpan.FromMilliseconds(transmissionTime));

            Console.Out.WriteLineWithUniqueColor(package.Id.ToString(), $"{package.Id}. Succesfully transmitted in {transmissionTime}ms using the line {Name}.");
        }
        finally
        {
            _semaphore.Release();
        }    
    }

    private double GetTransmissionTime()
    {
        if (MinTransmissionTime == MaxTransmissionTime)
            return MinTransmissionTime;
        else
        {
            Random rng = new Random();
            return MinTransmissionTime + rng.NextDouble() * (MaxTransmissionTime - MinTransmissionTime);
        }
    }
}
