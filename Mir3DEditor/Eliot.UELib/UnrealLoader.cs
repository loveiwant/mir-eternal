﻿using System;
using System.Collections.Generic;
using System.IO;
using UELib.Decoding;

namespace UELib
{
    /// <summary>
    /// Provides static methods for loading unreal packages.
    /// </summary>
    public static class UnrealLoader
    {
        /// <summary>
        /// Stored packages that were imported by certain objects. Kept here that in case re-use is necessary, that it will be loaded faster.
        /// The packages and the list is closed and cleared by the main package that loaded them with ImportObjects().
        /// In any other case the list needs to be cleared manually.
        /// </summary>
        private static readonly List<UnrealPackage> _CachedPackages = new List<UnrealPackage>();

        private static readonly List<UnrealPackage> _ImportedPackages = new List<UnrealPackage>();

        /// <summary>
        /// Loads the given file specified by PackagePath and
        /// returns the serialized UnrealPackage.
        /// </summary>
        public static UnrealPackage LoadPackage(string name, byte[] buffer)
        {
            var stream = new UPackageStream(name, buffer);
            var package = new UnrealPackage(stream);
            package.Deserialize(stream);
            return package;
        }

        /// <summary>
        /// Loads the given file specified by PackagePath and
        /// returns the serialized UnrealPackage.
        /// </summary>
        public static UnrealPackage LoadPackage(string packagePath, IBufferDecoder decoder,
            FileAccess fileAccess = FileAccess.Read)
        {
            var buffer = File.ReadAllBytes(packagePath);
            var stream = new UPackageStream(packagePath, buffer);
            var package = new UnrealPackage(stream) { Decoder = decoder };
            package.Deserialize(stream);
            return package;
        }

        /// <summary>
        /// Looks if the package is already loaded before by looking into the CachedPackages list first.
        /// If it is not found then it loads the given file specified by PackagePath and returns the serialized UnrealPackage.
        /// </summary>
        public static UnrealPackage LoadCachedPackage(string packagePath, FileAccess fileAccess = FileAccess.Read)
        {
            var package = _CachedPackages.Find(pkg => pkg.PackageName == Path.GetFileNameWithoutExtension(packagePath));
            if (package != null)
                return package;

            var buffer = File.ReadAllBytes(packagePath);
            package = LoadPackage(packagePath, buffer);
            if (package != null)
            {
                _CachedPackages.Add(package);
            }

            return package;
        }

        public static UnrealPackage LoadCachedPackage(string packagePath, byte[] buffer)
        {
            var package = _CachedPackages.Find(pkg => pkg.PackageName == Path.GetFileNameWithoutExtension(packagePath));
            if (package != null)
                return package;

            package = LoadPackage(packagePath, buffer);
            if (package != null)
            {
                _CachedPackages.Add(package);
            }

            return package;
        }

        public static UnrealPackage LoadImportPackage(string packagePath, byte[] buffer)
        {
            var package = _ImportedPackages.Find(pkg => pkg.PackageName == Path.GetFileNameWithoutExtension(packagePath));
            if (package != null)
                return package;

            package = LoadPackage(packagePath, buffer);
            if (package != null)
            {
                package.InitializePackage();
                _ImportedPackages.Add(package);
            }

            return package;
        }

        /// <summary>
        /// Loads the given file specified by PackagePath and
        /// returns the serialized UnrealPackage with deserialized objects.
        /// </summary>
        public static UnrealPackage LoadFullPackage(string packagePath, FileAccess fileAccess = FileAccess.Read)
        {
            var buffer = File.ReadAllBytes(packagePath);
            var package = LoadPackage(packagePath, buffer);
            package?.InitializePackage();

            return package;
        }

        public static UnrealPackage GetFromCache(string name)
        {
            var package = _CachedPackages.Find(pkg => pkg.PackageName == Path.GetFileNameWithoutExtension(name));
            if (package != null)
                return package;
            return null;
        }

        public static List<UnrealPackage> GetImportedPackages()
        {
            return _ImportedPackages;
        }
    }
}