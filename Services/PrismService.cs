using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using NLog;
using Prism.Helpers;
using ILogger = NLog.ILogger;

namespace Prism.Services;

public sealed partial class PrismService(ILogger? logger)
{
    private const int DwmwaBorderColor = 34;
    private int _delay = 2000;

    private readonly Dictionary<IntPtr, uint?> _originalBorderColors = [];
    private readonly Dictionary<IntPtr, uint> _desiredBorderColors = [];
    private readonly HashSet<IntPtr> _modifiedWindows = [];

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref uint pvAttribute,
        uint cbAttribute);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out uint pvAttribute,
        uint cbAttribute);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        logger = LogManager.GetCurrentClassLogger();
        var configFilePath = ConfigHelper.GetConfigPath();
        if (File.Exists(configFilePath))
        {
            var configJson = await File.ReadAllTextAsync(configFilePath, stoppingToken);
            dynamic? config = JsonConvert.DeserializeObject(configJson);
            if (config != null && config?.Delay != null)
            {
                _delay = (int)config?.Delay;
                logger.Info($"Configured delay: {_delay}ms");
            }
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    var configJson = await File.ReadAllTextAsync(configFilePath, stoppingToken);
                    dynamic? config = JsonConvert.DeserializeObject(configJson);
                    if (config == null)
                    {
                        logger.Fatal("Configuration file is empty or contains invalid JSON.");
                        await Task.Delay(_delay, stoppingToken);
                        continue;
                    }

                    if (config.Processes == null)
                    {
                        logger.Fatal("No 'Processes' property found in the configuration file.");
                        await Task.Delay(_delay, stoppingToken);
                        continue;
                    }

                    List<dynamic> processes = config.Processes.ToObject<List<dynamic>>();
                    var currentProcesses = Process.GetProcesses();

                    EnumWindows((hWnd, lParam) =>
                    {
                        GetWindowThreadProcessId(hWnd, out var processId);
                        var process = currentProcesses.FirstOrDefault(p => p.Id == processId);
                        if (process == null) return true;
                        foreach (var processConfig in processes)
                        {
                            string processName = processConfig.ProcessName;
                            string hexColor = processConfig.HexColor;
                            var newColor = HexColorToUint(hexColor);
                            _desiredBorderColors[hWnd] = newColor;

                            if (!process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (!_modifiedWindows.Contains(hWnd))
                            {
                                if (!_originalBorderColors.ContainsKey(hWnd))
                                {
                                    if (DwmGetWindowAttribute(hWnd, DwmwaBorderColor, out var originalColor,
                                            sizeof(uint)) == 0)
                                    {
                                        _originalBorderColors[hWnd] = originalColor;
                                    }
                                    else
                                    {
                                        _originalBorderColors[hWnd] = null;
                                    }
                                }

                                var result = DwmSetWindowAttribute(hWnd, DwmwaBorderColor, ref newColor, sizeof(uint));
                                if (result == 0)
                                {
                                    _modifiedWindows.Add(hWnd);
                                    _desiredBorderColors[hWnd] = newColor;
                                }
                                else
                                {
                                    logger.Error(
                                        $"Failed to set border color for window handle {hWnd} of {processName}, DwmSetWindowAttribute returned {result}");
                                }
                            }
                            else
                            {
                                var desiredColor = _desiredBorderColors[hWnd];
                                _ = DwmSetWindowAttribute(hWnd, DwmwaBorderColor, ref desiredColor,
                                    sizeof(uint));
                            }
                        }

                        return true;
                    }, IntPtr.Zero);

                    var existingWindowHandles =
                        new HashSet<IntPtr>(currentProcesses.SelectMany(p => GetProcessWindows(p.Id)));
                    var removedWindows = _modifiedWindows.Where(hWnd => !existingWindowHandles.Contains(hWnd)).ToList();
                    foreach (var hWnd in removedWindows)
                    {
                        _desiredBorderColors.Remove(hWnd);
                        _modifiedWindows.Remove(hWnd);
                        _originalBorderColors.Remove(hWnd);
                    }
                }
                else
                {
                    logger.Warn($"Configuration file '{configFilePath}' not found.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while executing the application.");
            }

            await Task.Delay(_delay, stoppingToken);
        }

        Cleanup();
    }


    private void Cleanup()
    {
        logger?.Info("Cleaning up border colors.");
        foreach (var kvp in _originalBorderColors)
        {
            if (!kvp.Value.HasValue) continue;
            var originalColor = kvp.Value.Value;
            var result = DwmSetWindowAttribute(kvp.Key, DwmwaBorderColor, ref originalColor, sizeof(uint));
            if (result == 0)
                logger?.Info($"Restored border color for window handle {kvp.Key}.");
            else
                logger?.Error(
                    $"Failed to restore border color for window handle {kvp.Key}, DwmSetWindowAttribute returned {result}");
        }
    }

    private static uint HexColorToUint(string hexColor)
    {
        var color = ColorTranslator.FromHtml(hexColor);
        if (color.IsEmpty) return 0;
        return (uint)(color.R | (color.G << 8) | (color.B << 16));
    }

    private static IEnumerable<IntPtr> GetProcessWindows(int processId)
    {
        var windowHandles = new List<IntPtr>();

        EnumWindows((hWnd, lParam) =>
        {
            _ = GetWindowThreadProcessId(hWnd, out var windowProcessId);
            if (windowProcessId == processId) windowHandles.Add(hWnd);
            return true;
        }, IntPtr.Zero);

        return windowHandles;
    }
}