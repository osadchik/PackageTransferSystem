class Program
{
    static async Task Main(string[] args)
    {
        Simulation simulation = new Simulation();
        await simulation.RunAsync();

        Console.ReadLine();
    }
}
