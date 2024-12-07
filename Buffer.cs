using PackageTransferSystem;
using System.Collections.Concurrent;

public class Buffer
{
    private BlockingCollection<Package> _queue;
    public string Name { get; private set; }
    public int Capacity { get; }
    public int Threshold { get; }

    // Event triggered when threshold is reached
    public event Action? ThresholdReached;

    public Buffer(int capacity, string name, int threshold = 0)
    {
        Capacity = capacity;
        Threshold = threshold;
        _queue = new BlockingCollection<Package>(new ConcurrentQueue<Package>(), capacity);
        Name = name;
    }

    public bool IsFull => _queue.Count >= Capacity;
    public int Count => _queue.Count;

    public void Enqueue(Package packet)
    {
        if (!_queue.IsAddingCompleted)
        {
            _queue.Add(packet);
            if (Threshold > 0 && _queue.Count >= Threshold)
            {
                ThresholdReached?.Invoke();
            }
        }
        else
        {
            throw new InvalidOperationException("Buffer is no longer accepting items.");
        }
    }

    public Package Dequeue()
    {
        Package packet;
        if (_queue.TryTake(out packet, Timeout.Infinite))
        {
            return packet;
        }
        else
        {
            return null;
        }
    }
}
