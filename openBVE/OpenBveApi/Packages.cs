using System;

namespace OpenBveApi.Packages
{
    /// <summary>Defines an OpenBVE Package</summary>
    /// This is an arbritrarily formatted archive containing the following files:
    /// package.xml - Package descriptor
    /// package.png - Package image
    /// package.rtf - Rich-text package description
    public class Package
    {
        /// <summary>The package version</summary>
        public Version PackageVersion;
        /// <summary>The package name</summary>
        public string Name = "";
        /// <summary>The package author</summary>
        public string Author = "";
        /// <summary>The package website</summary>
        public string Website = "";
        /// <summary>The GUID for this package</summary>
        public string GUID = "";
        /// <summary>Stores the package type- 0 for routes and 1 for trains</summary>
        public int PackageType;

        /*
         * These values are used by dependancies
         * They need to live in the base Package class to save creating another.....
         */
        /// <summary>The minimum package version</summary>
        public Version MinimumVersion;
        /// <summary>The maximum package version</summary>
        public Version MaximumVersion;
    }
}
