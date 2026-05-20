namespace MessageProcessingSystem.Consumer;

internal class Program
{
	static async Task Main(string[] args)
	{
		Console.WriteLine("Consumer started");

		while (true)
		{
			Console.WriteLine($"Waiting for messages at {DateTime.UtcNow}");

			await Task.Delay(2000);
		}
	}
}