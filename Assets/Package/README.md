# Addressable Resources
##### Unity Package: `io.finalclick.addressableresources`


## **Drop-in Addressables support for `Resources.Load`.**  

This package overrides Unity's internal `ResourcesAPI` to allow transparent loading of Addressables from folders named `AddressableResources`.


## 🔧 How It Works

Whenever you call `Resources.Load`, this package will:

1. **First attempt** to load the asset using the Addressables system from folders named `AddressableResources/`
2. **Fallback** to Unity's built-in `Resources` loading if the asset is not marked or found as an Addressable

✅ Works in both **runtime** and **editor** code  
✅ Zero changes to existing `Resources.Load` code needed

---

## 📦 Setup Instructions

To start using `AddressableResources` instead of `Resources`:

1. **Rename** any folder named `Resources/` to `AddressableResources/`
2. **Mark** the assets inside as **Addressable**

That’s it! Your calls to `Resources.Load("MyAsset")` will now resolve from the Addressables system if possible.

---

## 📁 Example

### Old Structure:
```
Assets/
└── Resources/
    └── MyAsset.prefab
```

### New Structure:
```
Assets/
└── AddressableResources/
    └── MyAsset.prefab   <-- Marked as Addressable
```

---

## ⚠️ Notes

- Folders must be explicitly named `AddressableResources/` to be picked up by this override.
- Assets must be marked as Addressable
- Compatible with both runtime and editor utility scripts.
