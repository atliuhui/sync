# Sync

A Windows command-line tool for **scanning** and **syncing** media files (photos, videos, audio, documents) from any portable device exposed via MTP/PTP — phones, cameras, tablets, etc.

Files are organized into a local gallery using configurable folder templates derived from each file's date metadata, with a CSV index tracking what has already been transferred so re-runs are incremental.

## Features

- Enumerate connected MTP/PTP devices and pick one by friendly name
- Two actions: **Scan** (dry run, build/update index) and **Sync** (copy new files into the gallery)
- Date extraction from filenames via [Grok](https://github.com/Marusyk/grok.net) patterns (configurable)
- Output folder layout driven by [Fluid](https://github.com/sebastienros/fluid) Liquid templates
- File/directory ignore rules by prefix/suffix
- Per-extension category mapping (audio / video / image / document / ignore)
- Incremental tracking via a per-device CSV index

## Requirements

- Windows (x64 or ARM64)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (target `net10.0-windows`)
- A portable device connected and visible in Windows Explorer (MTP)

## Usage

```text
sync Device --action <None|Scan|Sync> --name <device> [--root <path>]
```

### Options

| Option | Alias | Description |
| --- | --- | --- |
| `--action` | `-a` | Action to perform: `None` (list devices), `Scan` (build index only), `Sync` (copy new files) |
| `--name` | `-n` | Friendly name of the target device (e.g. `"Apple iPhone"`, `"Redmi 5 Plus"`) |
| `--root` | `-r` | Root directory on the device to traverse. Empty = device default |

### Examples

List connected devices:

```powershell
sync Device -a None
```

Scan a device (no files copied, only the index is updated):

```powershell
sync Device -a Scan -n "Apple iPhone"
```

Sync new files from a specific folder on the device:

```powershell
sync Device -a Sync -n "Apple iPhone"
```

```powershell
sync Device -a Sync -n "Redmi 5 Plus" -r "Internal storage/DCIM"
```

## Configuration

All behavior is driven by [Sync/appsettings.json](Sync/appsettings.json). Key sections:

- `Storage.Caching.Path` — local cache root (per-device subfolder + `tracking.csv` index)
- `Storage.Gallery.Path` — destination root for synced files
- `Storage.Gallery.Subfolder.Name.Template` — Fluid template for the per-file subfolder, e.g.
  `{{Date|date:'%Y'}}/{{Date|date:'%Y-%m-%d'}}`
- `Ignore.File` / `Ignore.Directory` — prefix/suffix filters
- `Mapping.CategoryProvider` — extension → category (`audio`, `video`, `image`, `document`, `ignore`)
- `Grok.Cores` / `Grok.Patterns.Dates` — Grok patterns used to extract a date from filenames
