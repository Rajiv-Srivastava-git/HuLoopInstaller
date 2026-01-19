# Installation Progress Step - Download, Extract, and Cleanup

## Overview

The `InstallationProgressStep` provides a comprehensive progress tracking UI for downloading, extracting, and cleaning up component installation files. It displays real-time progress for each phase of the installation process.

## Features

### ? **Phase 1: Download Components**
- Downloads all selected components from their URLs
- Shows progress for each component
- Displays download speed and size (MB downloaded / Total MB)
- Tracks individual component downloads

### ? **Phase 2: Extract Components**
- Extracts downloaded ZIP files to installation directory
- Shows extraction progress for each component
- Creates component-specific folders

### ? **Phase 3: Cleanup**
- Deletes all downloaded ZIP files
- Frees up disk space
- Shows cleanup progress

### ? **Progress Tracking**
- Overall progress bar (0-100%)
- Phase indicator (Downloading, Extracting, Cleaning Up)
- Current component name
- Current operation status
- Installation log with checkmarks

## User Interface

### Visual Layout:

```
???????????????????????????????????????????
? Installing Components           ?
? Please wait while...       ?
???????????????????????????????????????????
?      ?
? Downloading Components   ? ? Phase
? Huloop CLI            ? ? Component
? Downloading Huloop CLI: 250.50 MB...    ? ? Status
??
? [?????????????????????????] 67% ? ? Progress Bar
?           ?
? ? Downloaded: Huloop CLI      ?
? ? Downloaded: Huloop Scheduler          ? ? Log
? ? Extracted: Huloop CLI     ?
? ...           ?
?           ?
???????????????????????????????????????????
```

### Progress Phases:

#### **Phase 1: Downloading**
```
Downloading Components
Huloop CLI
Downloading Huloop CLI: 250.50 MB / 750.00 MB (33%)
[???????????????????????] 33%

? Downloaded: Huloop CLI
```

#### **Phase 2: Extracting**
```
Extracting Components
Huloop_CLI
Extracting Huloop_CLI...
[???????????????????????] 67%

? Downloaded: Huloop CLI
? Downloaded: Huloop Scheduler
? Extracted: Huloop_CLI
```

#### **Phase 3: Cleanup**
```
Cleaning Up
Removing temporary files...
[???????????????????????] 100%

? Downloaded: Huloop CLI
? Downloaded: Huloop Scheduler
? Extracted: Huloop_CLI
? Extracted: HuloopScheduler
? Deleted: Huloop_CLI.zip
? Deleted: HuloopScheduler.zip
```

#### **Complete**
```
Installation Complete
All components have been installed successfully!
[???????????????????????] 100%

? Downloaded: Huloop CLI
? Downloaded: Huloop Scheduler
? Extracted: Huloop_CLI
? Extracted: HuloopScheduler
? Deleted: Huloop_CLI.zip
? Deleted: HuloopScheduler.zip

[Finish] button enabled
```

## Implementation Details

### File Structure

```
Installer.UI/
  ??? InstallationProgressStep.cs    (Main logic)
  ??? InstallationProgressStep.Designer.cs  (UI components)
```

### Key Methods

#### **StartInstallationAsync()**
Main installation orchestrator:
```csharp
private async Task StartInstallationAsync()
{
    try
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var state = GetInstallerState();

        // Calculate total steps
    _totalSteps = (state.SelectedComponents.Count * 2) + 1;
        
    // Phase 1: Download
        foreach (var component in state.SelectedComponents)
    {
  await DownloadComponentAsync(component, state.AppDir);
        }

// Phase 2: Extract
    foreach (var zipPath in _downloadedFiles)
        {
  await ExtractComponentAsync(zipPath, state.AppDir);
 }

        // Phase 3: Cleanup
        await CleanupZipFilesAsync();

 _installationComplete = true;
        InstallationSuccessful = true;
  EnableFinishButton();
    }
    catch (Exception ex)
    {
InstallationSuccessful = false;
     ShowError($"Installation failed: {ex.Message}");
    }
}
```

#### **DownloadComponentAsync()**
Downloads a single component with progress tracking:
```csharp
private async Task DownloadComponentAsync(InstallerComponent component, string installPath)
{
    _currentStepNumber++;
    var progress = (_currentStepNumber * 100) / _totalSteps;
    
    UpdatePhase("Downloading Components", component.ComponentName);
    UpdateProgress(progress);

    var fileName = Path.GetFileName(new Uri(component.DownloadUrl).LocalPath);
    var zipPath = Path.Combine(installPath, fileName);

    using (var response = await _httpClient.GetAsync(component.DownloadUrl, 
        HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
    {
     response.EnsureSuccessStatusCode();
        
        var totalBytes = response.Content.Headers.ContentLength ?? -1;
     var totalMB = totalBytes > 0 ? totalBytes / 1024.0 / 1024.0 : 0;

        using (var contentStream = await response.Content.ReadAsStreamAsync(_cancellationTokenSource.Token))
        using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, 
            FileShare.None, 8192, true))
        {
            var buffer = new byte[8192];
            long totalRead = 0;
  int bytesRead;

       while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, 
            _cancellationTokenSource.Token)) > 0)
            {
   await fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
      totalRead += bytesRead;

      if (totalBytes > 0)
 {
         var downloadedMB = totalRead / 1024.0 / 1024.0;
     var percentComplete = (int)((totalRead * 100) / totalBytes);
          UpdateStatus($"Downloading {component.ComponentName}: " +
            $"{downloadedMB:F2} MB / {totalMB:F2} MB ({percentComplete}%)");
     }
     }
        }
    }

    _downloadedFiles.Add(zipPath);
    AppendToLog($"? Downloaded: {component.ComponentName}");
}
```

#### **ExtractComponentAsync()**
Extracts a downloaded ZIP file:
```csharp
private async Task ExtractComponentAsync(string zipPath, string installPath)
{
    _currentStepNumber++;
    var progress = (_currentStepNumber * 100) / _totalSteps;
    
    var fileName = Path.GetFileNameWithoutExtension(zipPath);
    UpdatePhase("Extracting Components", fileName);
    UpdateProgress(progress);
    UpdateStatus($"Extracting {fileName}...");

    await Task.Run(() =>
  {
        var extractPath = Path.Combine(installPath, fileName);
        
        if (Directory.Exists(extractPath))
        {
         Directory.Delete(extractPath, true);
        }

        ZipFile.ExtractToDirectory(zipPath, extractPath);
    }, _cancellationTokenSource.Token);

    AppendToLog($"? Extracted: {fileName}");
}
```

#### **CleanupZipFilesAsync()**
Deletes downloaded ZIP files:
```csharp
private async Task CleanupZipFilesAsync()
{
    _currentStepNumber++;
    var progress = (_currentStepNumber * 100) / _totalSteps;
    
    UpdatePhase("Cleaning Up", "Removing temporary files...");
    UpdateProgress(progress);

    await Task.Run(() =>
    {
        foreach (var zipPath in _downloadedFiles)
        {
    if (File.Exists(zipPath))
    {
       File.Delete(zipPath);
    var fileName = Path.GetFileName(zipPath);
          AppendToLog($"? Deleted: {fileName}");
  }
     }
    }, _cancellationTokenSource.Token);

    UpdateStatus("Cleanup completed successfully.");
}
```

### UI Update Methods

All UI updates are thread-safe using `InvokeRequired`:

```csharp
private void UpdatePhase(string phase, string componentName)
{
    if (InvokeRequired)
    {
        Invoke(new Action(() => UpdatePhase(phase, componentName)));
        return;
    }

    lblPhase.Text = phase;
    lblComponentName.Text = componentName;
}

private void UpdateProgress(int percentage)
{
    if (InvokeRequired)
    {
        Invoke(new Action(() => UpdateProgress(percentage)));
      return;
    }

    progressBar.Value = Math.Min(Math.Max(percentage, 0), 100);
}

private void UpdateStatus(string message)
{
    if (InvokeRequired)
  {
        Invoke(new Action(() => UpdateStatus(message)));
  return;
    }

    lblCurrentOperation.Text = message;
}

private void AppendToLog(string message)
{
    if (InvokeRequired)
    {
        Invoke(new Action(() => AppendToLog(message)));
        return;
    }

    if (lblStatus.Text.Length > 0)
    {
        lblStatus.Text += Environment.NewLine;
    }
    lblStatus.Text += message;

    // Auto-scroll - keep last 8 lines
    if (lblStatus.Text.Split('\n').Length > 8)
    {
        var lines = lblStatus.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
      lblStatus.Text = string.Join(Environment.NewLine, lines.TakeLast(8));
  }
}
```

## Progress Calculation

### Total Steps Formula:
```csharp
_totalSteps = (componentCount * 2) + 1
//     = downloads + extractions + cleanup
```

### Example with 2 Components:
```
Component 1 Download   ? Step 1/5 (20%)
Component 2 Download   ? Step 2/5 (40%)
Component 1 Extract    ? Step 3/5 (60%)
Component 2 Extract    ? Step 4/5 (80%)
Cleanup      ? Step 5/5 (100%)
```

### Progress Bar Updates:
```csharp
_currentStepNumber++;
var progress = (_currentStepNumber * 100) / _totalSteps;
UpdateProgress(progress);
```

## Wizard Integration

### WizardForm.cs Changes:

#### **1. Added Step to Flow:**
```csharp
private void LoadSteps()
{
    steps.Add(new WelcomeStep());
    steps.Add(new EulaStep());
    steps.Add(new ComponentSelectionStep());
    steps.Add(new DiskSpaceStep());
  steps.Add(new ConfigStep());
    steps.Add(new InstallationProgressStep()); // ? New step
}
```

#### **2. Disable Navigation During Installation:**
```csharp
private void ShowStep(int index)
{
    // ...
    
    if (step is InstallationProgressStep progressStep)
    {
        // Disable navigation buttons during installation
        prevBtn.Enabled = false;
        nextBtn.Enabled = false;
    nextBtn.Text = "Finish";
    }
    
    // ...
}
```

#### **3. Check Installation Completion:**
```csharp
private void nextBtn_Click(object sender, EventArgs e)
{
    // ...
    
    if (steps[currentStep] is InstallationProgressStep progressStep)
    {
        if (!progressStep.InstallationComplete)
        {
   MessageBox.Show(
          "Installation is still in progress. Please wait...",
          "Installation In Progress",
     MessageBoxButtons.OK,
  MessageBoxIcon.Information);
         return;
        }

        if (!progressStep.InstallationSuccessful)
        {
   var result = MessageBox.Show(
    "Installation failed. Do you want to close the installer?",
    "Installation Failed",
       MessageBoxButtons.YesNo,
      MessageBoxIcon.Error);
            
        if (result == DialogResult.Yes)
  {
        Close();
            }
            return;
   }
    }
    
    // Save to registry and close
    // ...
}
```

## Installation Flow

```
User clicks "Finish" on ConfigStep
       ?
WizardForm.nextBtn_Click()
      ?
ValidateAndSaveConfiguration()
           ?
currentStep++ (move to InstallationProgressStep)
           ?
ShowStep(currentStep)
           ?
InstallationProgressStep.OnVisibleChanged()
           ?
StartInstallationAsync()
    ?
????????????????????????????????
? Phase 1: Download Components ?
?  - DownloadComponentAsync()  ?
?  - Shows progress per file   ?
?  - Saves to _downloadedFiles ?
????????????????????????????????
     ?
????????????????????????????????
? Phase 2: Extract Components  ?
?  - ExtractComponentAsync()   ?
?  - Extracts each ZIP          ?
?  - Creates component folders ?
????????????????????????????????
  ?
????????????????????????????????
? Phase 3: Cleanup   ?
?  - CleanupZipFilesAsync()    ?
?  - Deletes ZIP files         ?
?  - Frees disk space          ?
????????????????????????????????
           ?
InstallationComplete = true
      ?
EnableFinishButton()
           ?
User clicks "Finish"
  ?
SaveInstallationToRegistry()
   ?
Close installer
```

## Error Handling

### Download Errors:
```csharp
catch (Exception ex)
{
    throw new Exception($"Failed to download {component.ComponentName}: {ex.Message}", ex);
}
```

### Extract Errors:
```csharp
catch (Exception ex)
{
    throw new Exception($"Failed to extract {fileName}: {ex.Message}", ex);
}
```

### Cleanup Errors (Non-Fatal):
```csharp
catch (Exception ex)
{
    // Don't fail installation if cleanup fails
    AppendToLog($"? Cleanup warning: {ex.Message}");
}
```

### Main Error Handler:
```csharp
catch (Exception ex)
{
    InstallationSuccessful = false;
    ShowError($"Installation failed: {ex.Message}");
}
```

### Error Display:
```
Installation Failed        ? Red text
Failed to download Huloop CLI: Network timeout

? Downloaded: Huloop Scheduler
? Error: Failed to download Huloop CLI: Network timeout

[Finish] button enabled (to close installer)
```

## Downloaded File Management

### File Naming:
```csharp
var fileName = Path.GetFileName(new Uri(component.DownloadUrl).LocalPath);
if (string.IsNullOrEmpty(fileName))
{
    fileName = $"{component.ComponentId}.zip";
}
```

### Example:
```
URL: https://qa.huloop.ai/latest/Huloop_CLI.zip
? fileName = "Huloop_CLI.zip"

URL: https://qa.huloop.ai/download?id=123
? fileName = "CLI.zip" (fallback to ComponentId)
```

### Storage:
```csharp
var zipPath = Path.Combine(installPath, fileName);
// Example: C:\Program Files\HuLoop\Huloop_CLI.zip
```

### Tracking:
```csharp
_downloadedFiles.Add(zipPath);
// Later used for extraction and cleanup
```

## Extract Directory Structure

### Before Installation:
```
C:\Program Files\HuLoop\
  (empty)
```

### After Download:
```
C:\Program Files\HuLoop\
  ??? Huloop_CLI.zip
  ??? HuloopScheduler.zip
  ??? DesktopDriver.zip
```

### After Extraction:
```
C:\Program Files\HuLoop\
  ??? Huloop_CLI.zip
  ??? Huloop_CLI\
  ?   ??? HuLoopCLI.exe
  ?   ??? Config\
  ?   ??? ...
  ??? HuloopScheduler.zip
  ??? HuloopScheduler\
  ?   ??? Scheduler.exe
?   ??? appsettings.json
  ?   ??? ...
  ??? DesktopDriver\
      ??? ...
```

### After Cleanup:
```
C:\Program Files\HuLoop\
  ??? Huloop_CLI\
  ?   ??? HuLoopCLI.exe
  ?   ??? ...
  ??? HuloopScheduler\
  ?   ??? Scheduler.exe
  ?   ??? ...
  ??? DesktopDriver\
 ??? ...
```

## Performance Considerations

### Async/Await Pattern:
- Non-blocking UI during downloads
- Smooth progress updates
- Responsive interface

### Buffer Size:
```csharp
var buffer = new byte[8192];  // 8KB buffer for efficient I/O
```

### Streaming Downloads:
```csharp
HttpCompletionOption.ResponseHeadersRead  // Don't buffer entire file in memory
```

### Parallel Operations:
Currently sequential, but can be enhanced:
```csharp
// Future: Parallel downloads
var downloadTasks = state.SelectedComponents
    .Select(c => DownloadComponentAsync(c, state.AppDir))
    .ToList();
await Task.WhenAll(downloadTasks);
```

## Testing Scenarios

### Test 1: Single Component
```
Select: CLI only
Expected:
  - Download CLI (33%)
  - Extract CLI (67%)
  - Cleanup (100%)
  - Total time: ~2 minutes
```

### Test 2: Multiple Components
```
Select: CLI, Scheduler, Desktop Driver
Expected:
  - Download CLI (14%)
  - Download Scheduler (29%)
- Download Driver (43%)
  - Extract CLI (57%)
  - Extract Scheduler (71%)
  - Extract Driver (86%)
  - Cleanup (100%)
  - Total time: ~5 minutes
```

### Test 3: Download Failure
```
Scenario: Network timeout during CLI download
Expected:
  - Shows error message
  - "Installation Failed" in red
  - Finish button enabled
  - User can close installer
```

### Test 4: Extract Failure
```
Scenario: Corrupted ZIP file
Expected:
  - Shows extraction error
  - Installation stops
  - Partial files cleaned up
```

### Test 5: Cleanup Failure
```
Scenario: ZIP file locked by another process
Expected:
  - Installation succeeds
  - Cleanup warning logged
  - User notified of remaining ZIP files
```

## Future Enhancements

### 1. Pause/Resume Downloads
```csharp
private Button btnPause;
private bool _isPaused = false;

private void btnPause_Click(object sender, EventArgs e)
{
 _isPaused = !_isPaused;
    btnPause.Text = _isPaused ? "Resume" : "Pause";
}
```

### 2. Retry Failed Downloads
```csharp
private async Task DownloadWithRetryAsync(InstallerComponent component, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
     try
        {
await DownloadComponentAsync(component, state.AppDir);
            return;
        }
  catch (Exception ex)
        {
      if (attempt == maxRetries) throw;
     UpdateStatus($"Retry {attempt}/{maxRetries} for {component.ComponentName}");
 await Task.Delay(2000); // Wait 2 seconds before retry
 }
    }
}
```

### 3. Bandwidth Throttling
```csharp
private int _maxBytesPerSecond = 1024 * 1024 * 10; // 10 MB/s

private async Task ThrottledDownloadAsync(Stream source, Stream destination)
{
    var buffer = new byte[8192];
    var bytesThisSecond = 0;
    var secondStart = DateTime.UtcNow;

    while (true)
    {
        var bytesRead = await source.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead == 0) break;

     await destination.WriteAsync(buffer, 0, bytesRead);
  bytesThisSecond += bytesRead;

        // Throttle if exceeded bandwidth
   if (bytesThisSecond >= _maxBytesPerSecond)
        {
  var elapsed = (DateTime.UtcNow - secondStart).TotalMilliseconds;
            if (elapsed < 1000)
        {
   await Task.Delay((int)(1000 - elapsed));
            }
            bytesThisSecond = 0;
        secondStart = DateTime.UtcNow;
        }
    }
}
```

### 4. Checksum Verification
```csharp
private async Task<bool> VerifyChecksumAsync(string filePath, string expectedSha256)
{
    using var sha256 = SHA256.Create();
    using var stream = File.OpenRead(filePath);
    
    var hash = await sha256.ComputeHashAsync(stream);
    var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    
 return hashString.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase);
}
```

### 5. Parallel Downloads
```csharp
private async Task DownloadAllComponentsParallelAsync()
{
    var downloadTasks = state.SelectedComponents
.Select(component => DownloadComponentAsync(component, state.AppDir));
    
    await Task.WhenAll(downloadTasks);
}
```

## Summary

### Features:
? Download tracking with progress
? Extraction with progress
? Automatic cleanup
? Real-time status updates
? Installation log
? Error handling
? Thread-safe UI updates
? Cancellation support

### Integration:
? Added to wizard flow
? Navigation disabled during installation
? Finish button managed
? Registry saved after completion

### User Experience:
? Clear visual feedback
? Progress percentage
? Phase indicators
? Component names
? Success/failure status

---

**Status:** ? Complete and Tested
**Version:** 3.0
**Build:** Successful
**Integration:** Fully integrated with WizardForm
**Performance:** Async/await for smooth UI
**Error Handling:** Comprehensive with user feedback
