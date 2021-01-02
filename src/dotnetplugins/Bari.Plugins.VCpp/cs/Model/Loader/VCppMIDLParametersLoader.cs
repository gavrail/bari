﻿using System;
using System.Collections.Generic;
using Bari.Core.Model;
using Bari.Core.Model.Loader;
using Bari.Core.UI;
using Bari.Plugins.VsCore.Model;
using YamlDotNet.RepresentationModel;

namespace Bari.Plugins.VCpp.Model.Loader
{
    /// <summary>
    /// Loads a MIDL parameter block (<see cref="VCppProjectMIDLParameters"/> from YAML
    /// </summary>
    public class VCppMIDLParametersLoader : YamlProjectParametersLoaderBase<VCppProjectMIDLParameters>
    {
        public VCppMIDLParametersLoader(IUserOutput output) : base(output)
        {
        }

        /// <summary>
        /// Gets the name of the yaml block the loader supports
        /// </summary>
        protected override string BlockName
        {
            get { return "midl"; }
        }

        /// <summary>
        /// Creates a new instance of the parameter model type 
        /// </summary>
        /// <param name="suite">Current suite</param>
        /// <returns>Returns the new instance to be filled with loaded data</returns>
        protected override VCppProjectMIDLParameters CreateNewParameters(Suite suite)
        {
            return new VCppProjectMIDLParameters(suite);
        }

        /// <summary>
        /// Gets the mapping table
        /// 
        /// <para>The table contains the action to be performed for each supported option key</para>
        /// </summary>
        /// <param name="target">Target model object to be filled</param>
        /// <param name="value">Value to be parsed</param>
        /// <param name="parser">Parser to be used</param>
        /// <returns>Returns the mapping</returns>
        protected override Dictionary<string, Action> GetActions(VCppProjectMIDLParameters target, YamlNode value, YamlParser parser)
        {
            return new Dictionary<string, Action>
            {
                {"additional-include-directories", () => target.AdditionalIncludeDirectories = ParseStringArray(parser, value)},
                {"additional-options", () => target.AdditionalOptions = ParseStringArray(parser, value)},
                {"application-configuration-mode", () => target.ApplicationConfigurationMode = ParseBool(parser, value)},
                {"client-stub-file", () => target.ClientStubFile = ParseString(value)},
                {"c-preprocess-options", () => target.CPreprocessOptions = ParseStringArray(parser, value)},
                {"default-char-type", () => target.DefaultCharType = ParseEnum<CharType>(value, "character type")},
                {"dll-data-file-name", () => target.DllDataFileName = ParseString(value)},
                {"enable-error-checks", () => target.EnableErrorChecks = ParseEnum<MidlErrorChecks>(value, "MIDL error checking mode")},
                {"error-check-allocations", () => target.ErrorCheckAllocations = ParseBool(parser, value)},
                {"error-check-bounds", () => target.ErrorCheckAllocations = ParseBool(parser, value)},
                {"error-check-enum-range", () => target.ErrorCheckEnumRange = ParseBool(parser, value)},
                {"error-check-ref-pointers", () => target.ErrorCheckRefPointers = ParseBool(parser, value)},
                {"error-check-stub-data", () => target.ErrorCheckStubData = ParseBool(parser, value)},
                {"generate-client-stub", () => target.GenerateClientStub = ParseBool(parser, value)},
                {"generate-server-stub", () => target.GenerateServerStub = ParseBool(parser, value)},
                {"generate-stubless-proxies", () => target.GenerateStublessProxies = ParseBool(parser, value)},
                {"generate-type-library", () => target.GenerateTypeLibrary = ParseBool(parser, value)},
                {"header-file-name", () => target.HeaderFileName = ParseString(value)},
                {"ignore-standard-include-path", () => target.IgnoreStandardIncludePath = ParseBool(parser, value)},
                {"interface-identifier-file-name", () => target.InterfaceIdentifierFileName = ParseString(value)},
                {"locale-id", () => target.LocaleID = ParseInt32(value)},
                {"mktyplib-compatible", () => target.MkTypLibCompatible = ParseBool(parser, value)},
                {"preprocessor-definitions", () => target.PreprocessorDefinitions = ParseStringArray(parser, value)},
                {"proxy-file-name", () => target.ProxyFileName = ParseString(value)},
                {"server-stub-file", () => target.ServerStubFile = ParseString(value)},
                {"struct-member-alignment", () => target.StructMemberAlignment = ParseInt32(value)},
                {"suppress-compiler-warnings", () => target.SuppressCompilerWarnings = ParseBool(parser, value)},
                {"target-environment", () => target.TargetEnvironment = ParseEnum<MidlTargetEnvironment>(value, "MIDL target environment")},
                {"new-typelib-format", () => target.NewTypeLibFormat = ParseBool(parser, value)},
                {"type-library-name", () => target.TypeLibraryName = ParseString(value)},
                {"component-file-name", () => target.ComponentFileName = ParseString(value)},
                {"undefine-preprocessor-definitions", () => target.UndefinePreprocessorDefinitions = ParseStringArray(parser, value)},
                {"validate-all-parameters", () => target.ValidateAllParameters = ParseBool(parser, value)},
                {"warnings-as-error", () => target.WarningsAsError = ParseBool(parser, value)},
                {"warning-level", () => target.WarningLevel = ParseEnum<WarningLevel>(value, "warning level")}
            };
        }
    }
}