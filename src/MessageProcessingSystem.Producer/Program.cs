namespace MessageProcessingSystem.Producer;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Producer started");

        while (true)
        {
            Console.WriteLine($"Producing message at {DateTime.UtcNow}");

            await Task.Delay(2000);
        }
    }
}