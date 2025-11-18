
# Change Log
All notable changes to this project will be documented in this file.

## [1.3.0] - 2024-01-05

### Added
- **Experimental Audio Stream**: Streaming haptic to standard audio devices
- **Wifi**: Can detect and connect to Skinetic device over Wifi4
- **Documentation**: Generation of online API documentation on unitouch.actronika.com

## [1.2.1] - 2023-10-09

### Added
- **HSD mk.II USB**: Detection and connection to HSD mk.II devices

## [1.2.0] - 2023-06-21

### Added
- **Global Boost**: The boost increase the overall intensity of all haptic effects.
- **Pattern and Effect Boost**: Each instance has its own boost value using the default Pattern boost, which can be overridden on play. 
- **Transformation**: Shape-based patterns can be transformed on play by passing three additional parameters: height translation, heading rotation and tilting rotation.
- **Spatial Inversion and Addition**: An effect can be triggered with right/left, up/down and front/back inversion or addition.
- **Effect Accumulation strategy**: A fallback-pattern feature to alleviate haptic confusion.

### Changed
- **Stop Fadeout**: Remove any duration limitation to allow longer fadeout.
- Number of simultaneous playing simultaneous samples increased to 10: compatible with the Skinetic firmware 1.1.3 or higher.

### Fixed
- Boost are now scaled by the volume envelop of the samples

## [1.1.2] - 2023-05-23

### Changed
- Documentation: Unity Project configuration for Android build

## [1.1.1] - 2023-02-20

### Changed
- Minor Fix

## [1.1.0] - 2023-02-16

### Added
- Bluetooth support

### Changed
- Log formatting

