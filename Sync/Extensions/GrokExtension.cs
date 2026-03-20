using GrokNet;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Text;

namespace Sync.Extensions
{
    internal static class GrokExtension
    {
        static IEnumerable<string> GrokCores { get; set; }
        static IEnumerable<GrokPatternDate> GrokDates { get; set; }

        public static void Init(IConfiguration configuration)
        {
            GrokCores = new List<string>();
            GrokDates = new List<GrokPatternDate>();
            configuration.GetRequiredSection("Grok:Cores").Bind(GrokCores);
            configuration.GetRequiredSection("Grok:Patterns:Dates").Bind(GrokDates);

            var grok_cores = Encoding.UTF8.GetBytes(string.Join("\n", GrokCores));
            var index = 0;
            foreach (var item in GrokDates)
            {
                item.Name = $"DP{++index:D2}";
                item.Grok = new Grok(item.Pattern, new MemoryStream(grok_cores));
            }
        }

        public static DateTime? FormatDate(string text)
        {
            foreach (var pattern in GrokDates)
            {
                var result = pattern.Grok.Parse(text);
                var value = result.FirstOrDefault(item => item.Key == "date");
                if (value != null)
                {
                    if (DateTime.TryParseExact(value.Value.ToString(), pattern.Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out var date))
                    {
                        return date;
                    }
                }
            }

            return default;
        }
    }

    public class GrokPatternDate
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string Format { get; set; }
        public IEnumerable<string> Samples { get; set; }
        public Grok Grok { get; set; }
    }
}
