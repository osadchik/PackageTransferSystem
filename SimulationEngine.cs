using PackageTransferSystem;
using PackageTransferSystem.Extensions;
using System.Diagnostics;

public class Simulation
{
    private Buffer _bufferA;
    private Buffer _bufferB;
    private Buffer _bufferC;
    private TransmissionLine _lineAB1;
    private TransmissionLine _lineAB2;
    private TransmissionLine _lineBC1;
    private TransmissionLine _lineBC2;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private Random _random = new Random();

    private Stopwatch timer = new Stopwatch();

    private bool _backupActivated = false;
    private long _backupTime = 0;

    public Simulation()
    {
        _bufferA = new Buffer(capacity: 20, name: "A");
        _bufferB = new Buffer(capacity: 25, threshold: 20, name: "B");
        _bufferB.ThresholdReached += ActivateBackupEquipment;
        _bufferC = new Buffer(capacity: 100000, name: "C");

        _lineAB1 = new TransmissionLine("AB1", 20, 20);
        _lineAB2 = new TransmissionLine("AB2", 15, 25);
        _lineBC1 = new TransmissionLine("BC1", 22, 28);
        _lineBC2 = new TransmissionLine("BC2", 25, 25);
    }

    public async Task RunAsync()
    {
        try
        {
            timer.Start();

            // Start up a UI thread for cancellation.
            var simulationTask = Task.Run(() =>
            {
                if (Console.ReadKey(true).KeyChar == 'c')
                    _cts.Cancel();
            });

            var producerTask = Task.Run(() =>
            {
                GeneratePacketsAsync(_cts.Token);
            });

            var AB1_TransmissionTask = Task.Run(async () =>
            {
                await TransmitPackage(_bufferA, _bufferB, _cts.Token, _lineAB1);
            });

            var AB2_TransmissionTask = Task.Run(async () =>
            {
                await TransmitPackage(_bufferA, _bufferB, _cts.Token, _lineAB2);
            });

            var BC1_TransmissionTask = Task.Run(async () =>
            {
                await TransmitPackage(_bufferB, _bufferC, _cts.Token, _lineBC1);
            });

            var BC2_TransmissionTask = Task.Run(async () =>
            {
                await TransmitPackage(_bufferB, _bufferC, _cts.Token, _lineBC2);
            });

            // Wait for the simulation to complete

            await Task.WhenAll(producerTask, AB1_TransmissionTask, AB2_TransmissionTask, BC1_TransmissionTask, BC2_TransmissionTask);
        }
        catch (TaskCanceledException)
        {
            // Keep it just to avoid crashes.
        }
        finally
        {
            // Stop the watch
            timer.Stop();

            // Output statistics ???
            Console.WriteLine($"Backup equipment activated on {_backupTime} ms.");
            Console.WriteLine($"Simulation was running for: {timer.ElapsedMilliseconds} ms.");
            Console.Write($"Buffer C count: {_bufferC.Count}");
        }
    }

    private async Task GeneratePacketsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Calculate the next delay: between 5 ms and 15 ms
            double delayMs = 5 + _random.NextDouble() * 10;

            // Asynchronously wait for the next interval
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs), cancellationToken);

            // Create a new package instance
            Package newPackage = new Package();

            // Handle the newly created package (e.g., enqueue it somewhere, process it, etc.)
            // For demonstration, we'll just print the package Id and creation time:
            Console.Out.WriteLineWithUniqueColor(newPackage.Id.ToString(), $"{newPackage.Id}. Package created in {delayMs}ms.");

            try
            {
                _bufferA.Enqueue(newPackage);
            }
            catch
            {
                Console.WriteLine("Buffer A overflowed.");
                continue; // Skip to the next iteration
            }
        }
    }

    private async Task TransmitPackage(Buffer from, Buffer to, CancellationToken cancellationToken, TransmissionLine transmissionLine = null)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var package = from.Dequeue();

            if (package == null)
            {
                continue;
            }

            Console.Out.WriteLineWithUniqueColor(package.Id.ToString(), $"{package.Id}. Successfully dequeued from {from.Name}. Current queue count: {from.Count}");

            // Determine the next line based on package's source line
            TransmissionLine nextLine = DetermineNextLine(package, transmissionLine);

            package.SourceLine = transmissionLine.Name;

            await nextLine.TransmitAsync(package, _cts.Token);
            to.Enqueue(package);
        }
    }

    private TransmissionLine DetermineNextLine(Package package, TransmissionLine defaultLine)
    {
        return (package?.SourceLine) switch
        {
            "AB1" => _lineBC1,
            "AB2" => _lineBC2,
            _ => defaultLine,
        };
    }

    private void ActivateBackupEquipment()
    {
        if (!_backupActivated)
        {
            _backupActivated = true;
            _backupTime = timer.ElapsedMilliseconds;

            // Reduce transmission times for BC1 and BC2 to 15 ms
            _lineBC1.MinTransmissionTime = 15;
            _lineBC1.MaxTransmissionTime = 15;
            _lineBC2.MinTransmissionTime = 15;
            _lineBC2.MaxTransmissionTime = 15;
        }
    }
}
