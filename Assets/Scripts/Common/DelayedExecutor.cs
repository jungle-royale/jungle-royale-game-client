using System;
using System.Threading.Tasks;

public class DelayedExecutor
{
    public static async void ExecuteAfterDelay(int delayMilliseconds, Action action)
    {
        await Task.Delay(delayMilliseconds);
        action?.Invoke();
    }
}