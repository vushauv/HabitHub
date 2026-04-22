using backend.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace backend.BackgroundServices
{
    public class InviteCodeExpiryService(IServiceScopeFactory scopeFactory): BackgroundService
    {
        private static readonly TimeSpan Time = TimeSpan.FromMinutes(5);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    IInviteCodeRepository inviteCodeRepository = scope.ServiceProvider.GetRequiredService<IInviteCodeRepository>();

                    await inviteCodeRepository.ExpirePastDueInviteCodesAsync();
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"InviteCodeExpiryService error: {ex.Message}");
                }

                await Task.Delay(Time, cancellationToken);
            }
        }
    }
}
