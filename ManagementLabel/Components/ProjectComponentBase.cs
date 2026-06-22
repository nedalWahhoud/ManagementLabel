using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace ManagementLabel.Components
{
    public class ProjectComponentBase : ComponentBase
    {
        protected bool IsArabic =>
           CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";
        protected (bool Initialized, bool ParametersSet, bool AfterRender) IsRendered;
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}
