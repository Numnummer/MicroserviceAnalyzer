using System.Diagnostics;
using MicroserviceAnalyzer.BL.Abstractions.Services;
using MicroserviceAnalyzer.BL.Helpers;

namespace MicroserviceAnalyzer.BL.Services;

/// <summary>
/// Тут работа со скриптом.
/// Скрипт - sh-скрипт (шаблон), предназначенный для построения
/// предварительного микросервиса.
/// </summary>
public class ScriptService:IScriptService
{
    public async Task RunScriptAsync(string script, string workingDirectory)
    {
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

}