# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-02-22

### Added

- **Blender settings extraction**: Read output path, format, and frame range directly from `.blend` files
- **Respect file settings**: Use the output name and path defined in each Blender file (no more default overrides)
- **Detailed progress for animations**: Display current frame, total, and percentage (e.g. "Frame 25/250 (10%)")
- **Per-file progress bar**: Visual progress indicator for each animation render in progress
- **Output path display**: Show expected or actual output path under each file in the list
- **Pre-load settings**: Extract settings when files are added to display information before rendering

### Changed

- **User interface**: Hide frame progress for Image renders (single image only)
- **Build scripts**: Fix NETSDK1112 and NU1102 errors during build (restore and clean targeting Windows framework)

### Fixed

- **BlenderSettingsExtractor**: Fix thread-safety issue when drag & dropping files (replaced events with `ReadToEndAsync`)

---

## [1.0.0] - 2025

### Added

- Initial release of AutoShut
- Blender render queue for sequential rendering
- Choice between Image or Animation rendering for each file
- Visual progress indicators
- Optional auto-shutdown of the computer when all renders complete
- Modern dark-themed interface
- Drag & drop support for adding files
- Browse button to select `.blend` files
- Retry button for failed renders
- Remove button to delete files from the list

[1.1.0]: https://github.com/APihery/AutoShut/releases/tag/v1.1
[1.0.0]: https://github.com/APihery/AutoShut/releases/tag/v1.0
