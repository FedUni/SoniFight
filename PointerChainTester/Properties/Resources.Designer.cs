﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace au.edu.federation.PointerChainTester.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("au.edu.federation.PointerChainTester.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Illegal pointer chain - argument exception: .
        /// </summary>
        public static string argumentExceptionString {
            get {
                return ResourceManager.GetString("argumentExceptionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Illegal pointer chain - do not prefix pointer hops with 0x or such and separate each hop with comma..
        /// </summary>
        public static string formatExceptionString {
            get {
                return ResourceManager.GetString("formatExceptionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to INVALID POINTER TRAIL.
        /// </summary>
        public static string invalidPointerString {
            get {
                return ResourceManager.GetString("invalidPointerString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NOT CONNECTED TO PROCESS.
        /// </summary>
        public static string notConnectedString {
            get {
                return ResourceManager.GetString("notConnectedString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Illegal pointer chain - individual pointer hop value exceeds 32-bit limit..
        /// </summary>
        public static string overflowExceptionString {
            get {
                return ResourceManager.GetString("overflowExceptionString", resourceCulture);
            }
        }
    }
}
