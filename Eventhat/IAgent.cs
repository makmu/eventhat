namespace Eventhat;

public interface IAgent
{
    public Task StartAsync();

    public void Stop();
}