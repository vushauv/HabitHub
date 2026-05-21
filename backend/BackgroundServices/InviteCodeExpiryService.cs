using backend.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace backend.BackgroundServices
{
    public class InviteCodeExpiryService(IServiceScopeFactory scopeFactory, ILogger<InviteCodeExpiryService> logger): BackgroundService
    {
        private static readonly TimeSpan Time = TimeSpan.FromMinutes(5);

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("InviteCodeExpiryService started, interval {IntervalMinutes}m", Time.TotalMinutes);

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
                    logger.LogError(ex, "InviteCodeExpiryService cycle failed");
                }

                await Task.Delay(Time, cancellationToken);
            }

            logger.LogInformation("InviteCodeExpiryService stopped");
        }
    }
}
