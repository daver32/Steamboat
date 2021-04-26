using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Steamboat.Util
{
    internal static class DiscordClientExtensions
    {
        public static async Task WaitForReadyAsync(this DiscordClient client)
        {
            TaskCompletionSource completionSource = new TaskCompletionSource();
            
            Task OnClientOnReady(DiscordClient sender, ReadyEventArgs args)
            {
                completionSource.SetResult();
                return Task.CompletedTask;
            }

            client.Ready += OnClientOnReady;
            await completionSource.Task;
            client.Ready -= OnClientOnReady;
        }
    }
}