using Razor.Templating.Core;

namespace DocumentGeneration.Utilities
{
    public static class UtilitiesExtension
    {
        public static async Task<string> GenerateHtmlContent<T>(T invoice, string viewName)
        {
            return await RazorTemplateEngine.RenderAsync($"Views/{viewName}.cshtml", invoice);
        }
    }
}
