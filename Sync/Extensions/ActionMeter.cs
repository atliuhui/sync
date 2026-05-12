using System.Diagnostics;

namespace Sync.Extensions
{
    internal class ActionMeter : IDisposable
    {
        readonly string name;
        readonly Stopwatch stopwatch;
        readonly long heapsize;
        bool disposed;

        public ActionMeter(string name)
        {
            this.name = name;
            this.stopwatch = Stopwatch.StartNew();
            // https://learn.microsoft.com/zh-cn/dotnet/api/system.gc.gettotalmemory
            this.heapsize = GC.GetTotalMemory(forceFullCollection: true);
        }
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            this.disposed = true;

            this.stopwatch.Stop();
            var current = GC.GetTotalMemory(forceFullCollection: true);

            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now:s}, {this.name} completed in {this.stopwatch.Elapsed}, used {FormatSize(current - this.heapsize)} / {FormatSize(GC.GetTotalAllocatedBytes(precise: true))} mb.");
        }

        static decimal FormatSize(long value)
        {
            return Math.Round((decimal)value / 1024 / 1024, 2);
        }
        public string ConsoleFormat(string text)
        {
            var width = 0;
            try { width = Console.WindowWidth; } catch { /* redirected output */ }

            var name_format = $"{this.name} ";
            var time_format = $" ({(int)this.stopwatch.Elapsed.TotalSeconds} s)";
            var text_width = width - name_format.Length - time_format.Length;
            if (text_width <= 0)
            {
                return $"{name_format}{text}{time_format}";
            }
            var cut = CutFormat(text, text_width, out var pad);
            if (pad < 0)
            {
                pad = 0;
            }
            return $"{name_format}{cut.PadLeft(pad)}{time_format}";
        }
        public void ConsoleClear()
        {
            var width = 0;
            try { width = Console.WindowWidth; } catch { /* redirected output */ }
            if (width <= 1)
            {
                return;
            }
            var blank = new string(' ', width - 1);
            Console.Write('\r' + blank + '\n' + blank + '\r');
            try { Console.SetCursorPosition(0, Math.Max(0, Console.CursorTop - 1)); }
            catch { /* redirected output */ }
        }
        static string CutFormat(string text, int width, out int pad)
        {
            pad = width;
            var actual = 0;
            var index = 0;
            for (var i = text.Length - 1; i >= 0; i--)
            {
                if (IsWideChar(text.ElementAt(i)))
                {
                    actual = actual + 2;
                    pad = pad - 1;
                }
                else
                {
                    actual = actual + 1;
                }
                if (actual <= width)
                {
                    index = i;
                }
                else
                {
                    break;
                }
            }

            return text.Substring(index);
        }
        static bool IsWideChar(char ch)
        {
            // CJK Unified Ideographs
            if (ch >= '\u4E00' && ch <= '\u9FFF') return true;

            // CJK Symbols and Punctuation
            if (ch >= '\u3000' && ch <= '\u303F') return true;

            // Hiragana / Katakana
            if (ch >= '\u3040' && ch <= '\u30FF') return true;

            // Fullwidth Forms
            if (ch >= '\uFF01' && ch <= '\uFF60') return true;
            if (ch >= '\uFFE0' && ch <= '\uFFE6') return true;

            // Hangul Syllables
            if (ch >= '\uAC00' && ch <= '\uD7AF') return true;

            return false;
        }
    }
}
