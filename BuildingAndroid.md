# Building DroidBVE (Android / mobile port)

This fork tracks the effort to port OpenBVE to smartphones. The desktop build is
unchanged: see [Building.md](Building.md) for the original instructions.

## Current status

| Component | State |
|---|---|
| Portable simulation core (`source/Mobile/*`, net8.0) | **Compiles and runs** - route + train loading and physics verified by the headless smoke test |
| Headless smoke test (`source/Mobile/OpenBve.Headless`) | **Passing** on .NET 8 (Windows; no UI, GDI or GL required) |
| Android app skeleton (`source/OpenBVE.Android`) | Skeleton - boots the core with touch controls; **not yet buildable** without the Android workload, and rendering/audio are TODO |
| OpenGL renderer on Android | TODO |
| Audio backend on Android | TODO |
| Menus / package UI on Android | TODO |

## Repository layout

* `source/` - the original desktop projects (net48, untouched).
* `source/Mobile/` - SDK-style projects that compile the **same sources** for
  `net8.0` (the runtime family behind `net8.0-android` / `net8.0-ios`).
  * `DroidBVE.Mobile.sln` - solution containing all mobile projects.
  * `Mobile.Compat/` - tiny stand-ins for the WinForms types the shared sources
    use (`MessageBox`, `Application.StartupPath`, `IWin32Window`).
  * `*.Mobile/` - one project per core component
    (OpenBveApi, Formats.OpenBve, Formats.Msts, LibRender2, SoundManager,
    RouteManager2, TrainManager, Route.CsvRw, Train.OpenBve).
  * `OpenBve.Headless/` - console smoke test which loads a bundled test route
    and train and runs the physics for 20 simulated seconds.
* `source/OpenBVE.Android/` - the Android application skeleton.

### Source code conventions for shared files

The desktop build does not define any custom symbols; the mobile builds define
`MOBILE` (see `source/Mobile/Directory.Build.props`). Shared sources guard
desktop-only code with `#if !MOBILE` and mobile replacements with `#if MOBILE`.
Please keep shared files compiling for both.

Temporary compromises in the mobile build (all marked with `TEMPORARY` in the
project files):

* `System.Drawing.Common` is referenced for GDI bitmap handling. It compiles
  for net8.0 but throws at runtime on Android; the texture/font/route-map code
  must move to ImageSharp or SkiaSharp.
* `CS-Script.lib` (train scripts) and `OpenTK-OpenBVE` are consumed through
  NuGet compatibility shims (net45/net20 assemblies). Script execution and the
  GL binding story on Android are open items (see Roadmap).

## Building and running the headless smoke test

Only the .NET 8 SDK is required:

```bash
cd source/Mobile
dotnet build DroidBVE.Mobile.sln
dotnet run --project OpenBve.Headless
```

Expected output ends with `HEADLESS SMOKE TEST PASSED`, after loading the test
CSV route (station parse, track data) and the test `train.dat` (2 cars) and
accelerating the train to ~9 km/h in 20 simulated seconds. Exit code is 0 on
success, so it can be used in CI.

## Building the Android app

1. Install the Android workload and an Android SDK (API 34):

   ```bash
   dotnet workload install android
   ```

2. Build / deploy:

   ```bash
   cd source/OpenBVE.Android
   dotnet build
   dotnet run --project .   # deploys to a connected device or emulator
   ```

   Alternatively open `source/Mobile/DroidBVE.Mobile.sln` plus
   `source/OpenBVE.Android/OpenBVE.Android.csproj` in your IDE.

The app currently boots the bundled test route and train and shows live
speed/position with throttle and brake sliders (the same verified flow as the
headless test). Route content is unpacked from APK assets into app storage on
first start.

## Roadmap (roughly in order)

1. **Rendering**: host `LibRender2` in an OpenGL ES context. OpenBVE uses
   OpenTK's OpenGL 1.x/2.x fixed-function API; on Android this needs either an
   OpenTK build with Android support or a GLES translation layer
   (e.g. gl4es-style) since GLES drops immediate mode. This is the largest
   remaining piece of work.
2. **Audio**: replace the OpenAL backend in `SoundManager` with an Android
   backend (AAudio via OpenSL ES or `SoundPool`/`AudioTrack`).
3. **Touch UI**: on-screen controls (reverser, power/brake, horn, doors,
   ATS/ATC), station announcements, and a route/train picker replacing the
   WinForms menus.
4. **Content**: install packages / user content from shared storage or SAF
   (Storage Access Framework); the package database lives under the app files
   directory today.
5. **Images**: swap `System.Drawing.Common` for ImageSharp/SkiaSharp
   (texture loading, fonts, route map, text overlays).
6. **Scripts**: decide the policy for CS-Script based train/animated scripts on
   Android (dynamic compilation is unavailable on iOS and restricted on
   Android; options: precompiled script cache or an interpreter).
7. **Input plugins**: the Win32 input device plugin API is desktop-only; touch
   and gamepad input need a small mobile input abstraction.
8. **Performance**: object/texture budgets for mobile GPUs, loading screen,
   battery-friendly frame pacing.

## Known test data quirks

* `assets/Compatibility/Misc.xml` references `null.csv` without its `Misc/`
  subfolder, producing two harmless "file was not found" warnings during route
  loading. This is inherited from upstream OpenBVE.
