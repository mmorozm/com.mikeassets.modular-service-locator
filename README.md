# com.mikeassets.modular-service-locator

A lightweight modular service locator / DI container for Unity. You group bindings into **modules** and register or unregister them at runtime. For example, a `CoreModule` lives for the whole app, while a `GamePlayModule` exists only while a level is loaded.

[Changelog](CHANGELOG.md) | [Wiki](https://github.com/mmorozm/com.mikeassets.modular-service-locator/wiki) | [Issues](https://github.com/mmorozm/com.mikeassets.modular-service-locator/issues)

Supports Unity 2021.3 and newer, tested on Unity 6.3 LTS.

## Install

In the Package Manager, choose **+ → Install package from git URL…** and enter:

```
https://github.com/mmorozm/com.mikeassets.modular-service-locator.git
```

To pin a release, append a tag, e.g. `…modular-service-locator.git#1.0.0`. Or add it to `Packages/manifest.json`:

```json
"com.mikeassets.modular-service-locator": "https://github.com/mmorozm/com.mikeassets.modular-service-locator.git#1.0.0"
```

## Usage

```csharp
using MikeAssets.ModularServiceLocator.Runtime;

public sealed class CoreModule : LocatorModule
{
    public override void Load()
    {
        Bind<ISaveService>().ToSingleton<SaveService>();          // one lazy instance
        Bind<IClock>().ToTransient<SystemClock>();                // new instance per resolve
        Bind<ISceneService, IPreloadService>()                    // one object, two contracts
            .ToConstant(new SceneService());
    }
}

var locator = new ServiceLocator();
locator.RegisterModule(new CoreModule());
locator.ResolveSingletons();                  // optional: create singletons eagerly

var save = locator.Get<ISaveService>();       // throws MissingBindingException if unbound
if (locator.TryGet<IAnalytics>(out var analytics)) { /* optional service */ }

locator.UnregisterModule(typeof(CoreModule).ToString());
```

Constructor injection uses the public constructor with the most parameters. Every parameter must be bound, or resolving throws `MissingConstructorParamException`. Dependency cycles throw `CyclicDependencyException`.

### Code stripping (IL2CPP)

Implementations are created through reflection, so with Managed Stripping enabled, mark their constructors with `[UnityEngine.Scripting.Preserve]`. In the Editor, the locator logs a warning once for each type that is missing the attribute.

## Running the tests

The tests ship with the package. To run them from a project that installs the package, add it to `testables` in `Packages/manifest.json`:

```json
"testables": [ "com.mikeassets.modular-service-locator" ]
```
