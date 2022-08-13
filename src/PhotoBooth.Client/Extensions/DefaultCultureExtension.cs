using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace PhotoBooth.Client.Extensions
{
    public static class DefaultCultureExtension
    {
        public static async Task SetDefaultCulture(this WebAssemblyHost host, List<string> supportedCultures, string defaultCulture)
        {
            IJSRuntime jsInterop = host.Services.GetRequiredService<IJSRuntime>();
            string result = await jsInterop.InvokeAsync<string>("getCulture");
            CultureInfo culture;
            if (result != null)
            {
                culture = new CultureInfo(result);
            }
            else
            {
                string browserCulture = await jsInterop.InvokeAsync<string>("getBrowserCulture");

                if (! string.IsNullOrEmpty(browserCulture) && TryGetExactLocale(supportedCultures, browserCulture, out string foundExactLocale))
                {
                    culture = new CultureInfo(foundExactLocale);
                }
                // e.g. de
                else if (!string.IsNullOrEmpty(browserCulture) && TryGetLocale(supportedCultures, browserCulture, out string foundLocale))
                {
                    culture = new CultureInfo(foundLocale);
                }
                else
                {
                    culture = new CultureInfo(defaultCulture);
                }

                await jsInterop.InvokeVoidAsync("setCulture", culture.Name);
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        private static bool TryGetExactLocale(List<string> supportedCultures, string localeName, out string foundLocale)
        {
            foundLocale = supportedCultures.FirstOrDefault(l => l.ToLower() == localeName.ToLower());
            return foundLocale != null;
        }

        private static bool TryGetLocale(List<string> supportedCultures, string localeName, out string foundLocale)
        {
            foundLocale = supportedCultures.FirstOrDefault(l => localeName.ToLower().StartsWith(l.ToLower()));
            return foundLocale != null;
        }
    }
}
