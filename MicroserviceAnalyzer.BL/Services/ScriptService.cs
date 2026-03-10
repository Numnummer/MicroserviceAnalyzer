using System.Diagnostics;
using System.Text.RegularExpressions;
using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.Helpers;

namespace MicroserviceAnalyzer.BL.Services;

/// <summary>
/// Тут работа со скриптом.
/// Скрипт - sh-скрипт (шаблон), предназначенный для построения
/// предварительного микросервиса.
/// </summary>
public class ScriptService : IScriptService
{
    public async Task RunScriptAsync(string script, string workingDirectory)
    {
        script = RemoveIgnoringScript(script);
        var tempScriptPath = Path.GetTempFileName();
        Directory.CreateDirectory(workingDirectory);

        try
        {
            await File.WriteAllTextAsync(tempScriptPath, script);

            await Process.Start("chmod", $"755 \"{tempScriptPath}\"").WaitForExitAsync();

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{tempScriptPath}\"",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Script execution failed: {error}");
            }

            Console.WriteLine($"Output: {output}");
        }
        finally
        {
            if (File.Exists(tempScriptPath))
            {
                File.Delete(tempScriptPath);
            }
        }
    }
    private string RemoveIgnoringScript(string script)
    {
        string pattern = @"#ignore_for_temp_start.*?#ignore_for_temp_end";
        
        while (Regex.IsMatch(script, pattern, RegexOptions.Singleline))
        {
            script = Regex.Replace(script, pattern, "", RegexOptions.Singleline);
        }
        
        return script;
    }

}