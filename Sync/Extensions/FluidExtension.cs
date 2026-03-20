using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;

namespace Sync.Extensions
{
    internal static class FluidExtension
    {
        public static TemplateOptions DefaultOptions()
        {
            TemplateOptions options = new TemplateOptions();
            options.MemberAccessStrategy.Register<JObject, object>((source, name) => source[name]);
            options.ValueConverters.Add(x => x is JObject o ? new ObjectValue(o) : null);
            options.ValueConverters.Add(x => x is JValue v ? v.Value : null);

            return options;
        }

        public static bool TryRender(this IFluidTemplate template, JObject context, out string? text, out string? message)
        {
            var variables = new TemplateContext(context, DefaultOptions());

            try
            {
                text = template.Render(variables);
                message = null;
                return true;
            }
            catch (Exception ex)
            {
                text = null;
                message = ex.GetBaseException().Message;
                return false;
            }
        }
        public static bool TryRender(this string template, JObject context, out string? text, out string? message)
        {
            var parser = new FluidParser();
            if (parser.TryParse(template, out var renderer, out var error))
            {
                var variables = new TemplateContext(context, DefaultOptions());

                try
                {
                    text = renderer.Render(variables);
                    message = null;
                    return true;
                }
                catch (Exception ex)
                {
                    text = null;
                    message = ex.GetBaseException().Message;
                    return false;
                }
            }
            else
            {
                text = null;
                message = error;
                return false;
            }
        }
    }
}
