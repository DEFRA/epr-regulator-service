﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPR.RegulatorService.Frontend.Web.Resources.Views.RemoveApprovedUser {
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Confirm_en {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Confirm_en() {
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EPR.RegulatorService.Frontend.Web.Resources.Views.RemoveApprovedUser.Confirm.en", typeof(Confirm_en).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to You have selected {0} to be removed from {1} account.
        /// </summary>
        internal static string Page_Title {
            get {
                return ResourceManager.GetString("Page.Title", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The organisation will not be able to submit packaging data until it has a new approved person..
        /// </summary>
        internal static string Warning_Submit_Packaging {
            get {
                return ResourceManager.GetString("Warning.Submit.Packaging", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Any delegated persons linked to the organisation will receive an email telling them they are basic users and unable to submit data..
        /// </summary>
        internal static string Warning_Users {
            get {
                return ResourceManager.GetString("Warning.Users", resourceCulture);
            }
        }
    }
}
