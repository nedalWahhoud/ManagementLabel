using Microsoft.AspNetCore.Components;

namespace ManagementLabel.Components
{
    public class ProjectComponentBase : ComponentBase
    {
        protected (bool Initialized, bool ParametersSet, bool AfterRender) IsRendered;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}
