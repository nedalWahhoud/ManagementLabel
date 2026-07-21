using Microsoft.AspNetCore.Components.Server.Circuits;

namespace ManagementLabel.Components.CircuitF
{
    public class MyCircuitHandler : CircuitHandler
    {
        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
