using System.Text.RegularExpressions;

namespace TaskRunner.Domain
{
    public class JsFile : IJsFile
    {
        public int Id { get; set; }
        public string JavascriptCode { get; set; }
        public string JavascriptCodeIdentifier { get; set; }

        public void SetJavascriptCodeIdentifier()
        {
            if (!string.IsNullOrEmpty(JavascriptCodeIdentifier)) 
            {
                throw new Exception("JavascriptCodeIdentifier already set");
            }

            JavascriptCodeIdentifier = (new DateTime()).ToShortDateString() + RandomString(16);

            static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                var random = new Random();

                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            }
        }

        public bool IsValidJavascriptCode()
        {
            var javascriptCodeLength = JavascriptCode.Length;

            if (string.IsNullOrEmpty(JavascriptCodeIdentifier) ||
                !ValidateModules() ||
                !ValidateAllEvals())
            {
                return false;
            }

            return true;

            bool ValidateModules()
            {
                var matches = Regex.Matches(JavascriptCode, @"require\((.*?)\)");
                
                var list = new List<string>();

                foreach (Match match in matches)
                {
                    var item = match.Value
                        .Replace("require(", "")
                        .Replace("\'", "")
                        .Replace("\"", "");
                    item = item.Remove(item.Length -1);
                    list.Add(item);
                }

                if (list.Count == 0)
                {
                    return true;
                }

                var forbiddenModules = new List<string>() {
                      "_http_agent", "_http_client", "_http_common",
                      "_http_incoming", "_http_outgoing", "_http_server",
                      "_stream_duplex", "_stream_passthrough", "_stream_readable",
                      "_stream_transform", "_stream_wrap", "_stream_writable",
                      "_tls_common", "_tls_wrap", "assert",
                      "assert/strict", "async_hooks", "buffer",
                      "child_process", "cluster", "console",
                      "constants", "crypto", "dgram",
                      "diagnostics_channel", "dns", "dns/promises",
                      "domain", "events", "fs",
                      "fs/promises", "http", "http2",
                      "https", "inspector", "module",
                      "net", "os", "path",
                      "path/posix", "path/win32", "perf_hooks",
                      "process", "punycode", "querystring",
                      "readline", "repl", "stream",
                      "stream/consumers", "stream/promises", "stream/web",
                      "string_decoder", "sys", "timers",
                      "timers/promises", "tls", "trace_events",
                      "tty", "url", "util",
                      "util/types", "v8", "vm",
                      "worker_threads", "zlib"
                    };

                return !list.Any(i => forbiddenModules.Exists(m => i == m));
            }

            bool ValidateAllEvals()
            {
                var matches = Regex.Matches(JavascriptCode, @"(.?)eval\((.*?)\)");

                var list = new List<string>();

                foreach (Match match in matches)
                {
                    list.Add(match.Value);
                }

                if (list.Count == 0)
                {
                    return true;
                }

                var validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                return !list.Any(i => i.IndexOf("eval(") == 0 || !validChars.ToList().Exists(v => v == i[0]));
            }
        }
    }
}
