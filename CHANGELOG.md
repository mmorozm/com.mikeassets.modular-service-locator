# Changelog

All notable changes to this package are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project follows [Semantic Versioning](https://semver.org/).

## [1.0.0] - 2026-09-24

Tested against Unity 6.3 LTS; the minimum supported version is Unity 2021.3.

### Added
- `ToSingleton<TImplementation>()` on both binding builders.
- `TryGet<T>(out T)`, `IsBound<T>()` and non-generic `Get(Type)` extension methods on `IResolutionRoot`.
- `MissingBindingException`, thrown when resolving a type that has no binding.
- `MissingConstructorParamException.ParameterName`.
- `BindingRoot.TryGetBinding(Type, out IBinding)`.
- `CHANGELOG.md`, and package metadata (`unity`, `license`, `repository`, documentation and changelog URLs).

### Changed
- **Breaking:** `BindingType` moved from the misspelled `ikeAssets.ModularServiceLocator.Runtime` namespace to `MikeAssets.ModularServiceLocator.Runtime`.
- **Breaking:** binding the same service twice now replaces the earlier binding (last bind wins). Previously the second `Bind<T>()` returned a builder for a binding that was silently discarded.
- **Breaking:** transient and singleton providers now use the public constructor with the **most** parameters, not the fewest.
- `SingletoneBindingProvider` renamed to `SingletonBindingProvider`. The old name remains as an `[Obsolete]` subclass.
- `ToSingletone<T>()` is `[Obsolete]` and forwards to `ToSingleton<T>()`.
- Singleton creation is now thread-safe.
- The missing-`[Preserve]` warning is logged once per type instead of on every resolve, and its wording is fixed.
- Resolving a service now looks up bindings by dictionary instead of copying and scanning the list.

### Fixed
- `ToSingletone`/`ToSingleton` set `BindingType.Constant` instead of `BindingType.Singleton`.
- `Unbind<T>()` and `Unbind<T1, T2>()` did nothing.
- `Get<T>()` on an unbound type threw a `NullReferenceException`.
- A missing constructor dependency threw a bare `InvalidOperationException` from LINQ `First()`; it now throws `MissingConstructorParamException` again, and `Service` is set.
- Cycles that did not include the requested service (A → B → C → B) caused a stack overflow; every cycle is now reported as `CyclicDependencyException`.
- Unregistering a module removed root bindings that had overridden the module's bindings for the same service.

## [0.0.16-preview] - 2024-06-21
### Added
- Editor warning when an injected constructor lacks `[Preserve]` (contributed by @stan-osipov).

## [0.0.15-preview] - 2021-07-06
- Initial public preview.
