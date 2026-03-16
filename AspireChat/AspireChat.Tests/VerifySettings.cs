using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using VerifyTests;

namespace AspireChat.Tests;

public static class VerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.ScrubInlineGuids();
        VerifierSettings.AddScrubber(builder =>
        {
            var text = builder.ToString();
            text = Regex.Replace(text, @"itest-[a-f0-9]{32}", "itest-<run-prefix>");
            text = Regex.Replace(text, @"https?://127\.0\.0\.1:\d+/devstoreaccount1/images/[^\s,}\]]+", "https://blob-host/devstoreaccount1/images/<blob-name>");
            text = Regex.Replace(text, @"https?://localhost:\d+/devstoreaccount1/images/[^\s,}\]]+", "https://blob-host/devstoreaccount1/images/<blob-name>");
            builder.Clear();
            builder.Append(text);
        });
    }
}
