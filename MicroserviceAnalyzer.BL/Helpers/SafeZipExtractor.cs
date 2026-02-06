using System.IO.Compression;

namespace MicroserviceAnalyzer.BL.Helpers;

public static class SafeZipExtractor
{
    public static int ExtractZipSafely(string zipPath, string extractPath, bool overwrite = true)
    {
        var extractedCount = 0;
        var skippedCount = 0;
        
        try
        {
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;
                            
                        if (IsHiddenFileOrDirectory(entry.FullName))
                        {
                            Console.WriteLine($"Пропущен скрытый файл: {entry.FullName}");
                            skippedCount++;
                            continue;
                        }
                        
                        var destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                        
                        // Проверяем безопасность пути (защита от ZipSlip атаки)
                        if (!destinationPath.StartsWith(extractPath, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Опасный путь: {entry.FullName}");
                            skippedCount++;
                            continue;
                        }
                        
                        var directoryPath = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        
                        ExtractEntryWithRetry(entry, destinationPath, overwrite);
                        extractedCount++;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Нет прав доступа к файлу {entry.FullName}: {ex.Message}");
                        skippedCount++;
                    }
                    catch (PathTooLongException ex)
                    {
                        Console.WriteLine($"Слишком длинный путь: {entry.FullName}: {ex.Message}");
                        skippedCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке {entry.FullName}: {ex.Message}");
                        skippedCount++;
                    }
                }
            }
            
            Console.WriteLine($"Распаковка завершена. Успешно: {extractedCount}, Пропущено: {skippedCount}");
            return extractedCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка открытия архива: {ex.Message}");
            return -1;
        }
    }
    
    private static bool IsHiddenFileOrDirectory(string path)
    {
        var pathParts = path.Split('/', '\\');
        
        foreach (var part in pathParts)
        {
            if (string.IsNullOrEmpty(part))
                continue;
                
            // Проверяем, является ли часть пути скрытой
            // 1. Начинается с точки (Unix/Linux скрытые файлы)
            // 2. Начинается с ~$ (временные файлы Office)
            // 3. Содержит определённые паттерны
            if (part.StartsWith(".") || 
                part.StartsWith("~$") ||
                part.Equals("__MACOSX", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase) ||
                part.Equals(".DS_Store", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            if (part.StartsWith("_") && part.Contains("hidden"))
                return true;
        }
        
        return false;
    }
    
    private static void ExtractEntryWithRetry(ZipArchiveEntry entry, string destinationPath, bool overwrite)
    {
        var maxRetries = 3;
        var delayMs = 100;
        
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (File.Exists(destinationPath) && !overwrite)
                    return;
                    
                entry.ExtractToFile(destinationPath, overwrite);
                Console.WriteLine($"Успешно: {entry.FullName}");
                return;
            }
            catch (IOException ex) when (attempt < maxRetries)
            {
                // Если ошибка связана с блокировкой файла, ждем и повторяем
                Console.WriteLine($"Попытка {attempt}/{maxRetries} для {entry.FullName}: {ex.Message}");
                Thread.Sleep(delayMs * attempt);
            }
        }
        
        Console.WriteLine($"Не удалось извлечь {entry.FullName} после {maxRetries} попыток");
    }
}