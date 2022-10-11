using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using ShellLibrary.Interop;
using WPFUI.Win32;

namespace BExplorer.Shell.Interop {
	/// <summary>
	/// Provides a set of flags to be used with Microsoft.WindowsAPICodePack.Shell.SearchCondition 
	/// to indicate the operation in Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory methods.
	/// </summary>
	public enum SearchConditionOperation {
		/// <summary>
		/// An implicit comparison between the value of the property and the value of the constant.
		/// </summary>
		Implicit = 0,

		/// <summary>
		/// The value of the property and the value of the constant must be equal.
		/// </summary>
		Equal = 1,

		/// <summary>
		/// The value of the property and the value of the constant must not be equal.
		/// </summary>
		NotEqual = 2,

		/// <summary>
		/// The value of the property must be less than the value of the constant.
		/// </summary>
		LessThan = 3,

		/// <summary>
		/// The value of the property must be greater than the value of the constant.
		/// </summary>
		GreaterThan = 4,

		/// <summary>
		/// The value of the property must be less than or equal to the value of the constant.
		/// </summary>
		LessThanOrEqual = 5,

		/// <summary>
		/// The value of the property must be greater than or equal to the value of the constant.
		/// </summary>
		GreaterThanOrEqual = 6,

		/// <summary>
		/// The value of the property must begin with the value of the constant.
		/// </summary>
		ValueStartsWith = 7,

		/// <summary>
		/// The value of the property must end with the value of the constant.
		/// </summary>
		ValueEndsWith = 8,

		/// <summary>
		/// The value of the property must contain the value of the constant.
		/// </summary>
		ValueContains = 9,

		/// <summary>
		/// The value of the property must not contain the value of the constant.
		/// </summary>
		ValueNotContains = 10,

		/// <summary>
		/// The value of the property must match the value of the constant, where '?' 
		/// matches any single character and '*' matches any sequence of characters.
		/// </summary>
		DosWildcards = 11,

		/// <summary>
		/// The value of the property must contain a word that is the value of the constant.
		/// </summary>
		WordEqual = 12,

		/// <summary>
		/// The value of the property must contain a word that begins with the value of the constant.
		/// </summary>
		WordStartsWith = 13,

		/// <summary>
		/// The application is free to interpret this in any suitable way.
		/// </summary>
		ApplicationSpecific = 14
	}

	/// <summary>
	/// Set of flags to be used with Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory.
	/// </summary>
	public enum SearchConditionType {
		/// <summary>
		/// Indicates that the values of the subterms are combined by "AND".
		/// </summary>
		And = 0,

		/// <summary>
		/// Indicates that the values of the subterms are combined by "OR".
		/// </summary>
		Or = 1,

		/// <summary>
		/// Indicates a "NOT" comparison of subterms.
		/// </summary>
		Not = 2,

		/// <summary>
		/// Indicates that the node is a comparison between a property and a 
		/// constant value using a Microsoft.WindowsAPICodePack.Shell.SearchConditionOperation.
		/// </summary>
		Leaf = 3,
	}

	/// <summary>
	/// Used to describe the view mode.
	/// </summary>
	public enum FolderLogicalViewMode {
		/// <summary>
		/// The view is not specified.
		/// </summary>
		Unspecified = -1,

		/// <summary>
		/// This should have the same affect as Unspecified.
		/// </summary>
		None = 0,

		/// <summary>
		/// The minimum valid enumeration value. Used for validation purposes only.
		/// </summary>
		First = 1,

		/// <summary>
		/// Details view.
		/// </summary>
		Details = 1,

		/// <summary>
		/// Tiles view.
		/// </summary>
		Tiles = 2,

		/// <summary>
		/// Icons view.
		/// </summary>
		Icons = 3,

		/// <summary>
		/// Windows 7 and later. List view.
		/// </summary>
		List = 4,

		/// <summary>
		/// Windows 7 and later. Content view.
		/// </summary>
		Content = 5,

		/// <summary>
		/// The maximum valid enumeration value. Used for validation purposes only.
		/// </summary>
		Last = 5
	}

	/*
	/// <summary>
	/// The direction in which the items are sorted.
	/// </summary>
	public enum SortDirection {
		/// <summary>
		/// A default value for sort direction, this value should not be used;
		/// instead use Descending or Ascending.
		/// </summary>
		Default = 0,

		/// <summary>
		/// The items are sorted in descending order. Whether the sort is alphabetical, numerical, 
		/// and so on, is determined by the data type of the column indicated in propkey.
		/// </summary>
		Descending = -1,

		/// <summary>
		/// The items are sorted in ascending order. Whether the sort is alphabetical, numerical, 
		/// and so on, is determined by the data type of the column indicated in propkey.
		/// </summary>
		Ascending = 1,
	}
	*/

	/// <summary>
	/// Provides a set of flags to be used with IQueryParser::SetOption and 
	/// IQueryParser::GetOption to indicate individual options.
	/// </summary>
	public enum StructuredQuerySingleOption {
		/// <summary>
		/// The value should be VT_LPWSTR and the path to a file containing a schema binary.
		/// </summary>
		Schema,

		/// <summary>
		/// The value must be VT_EMPTY (the default) or a VT_UI4 that is an LCID. It is used
		/// as the locale of contents (not keywords) in the query to be searched for, when no
		/// other information is available. The default value is the current keyboard locale.
		/// Retrieving the value always returns a VT_UI4.
		/// </summary>
		Locale,

		/// <summary>
		/// This option is used to override the default word breaker used when identifying keywords
		/// in queries. The default word breaker is chosen according to the language of the keywords
		/// (cf. SQSO_LANGUAGE_KEYWORDS below). When setting this option, the value should be VT_EMPTY
		/// for using the default word breaker, or a VT_UNKNOWN with an object supporting
		/// the IWordBreaker interface. Retrieving the option always returns a VT_UNKNOWN with an object
		/// supporting the IWordBreaker interface.
		/// </summary>
		WordBreaker,

		/// <summary>
		/// The value should be VT_EMPTY or VT_BOOL with VARIANT_TRUE to allow natural query
		/// syntax (the default) or VT_BOOL with VARIANT_FALSE to allow only advanced query syntax.
		/// Retrieving the option always returns a VT_BOOL.
		/// This option is now deprecated, use SQSO_SYNTAX.
		/// </summary>
		NaturalSyntax,

		/// <summary>
		/// The value should be VT_BOOL with VARIANT_TRUE to generate query expressions
		/// as if each word in the query had a star appended to it (unless followed by punctuation
		/// other than a parenthesis), or VT_EMPTY or VT_BOOL with VARIANT_FALSE to
		/// use the words as they are (the default). A word-wheeling application
		/// will generally want to set this option to true.
		/// Retrieving the option always returns a VT_BOOL.
		/// </summary>
		AutomaticWildcard,

		/// <summary>
		/// Reserved. The value should be VT_EMPTY (the default) or VT_I4.
		/// Retrieving the option always returns a VT_I4.
		/// </summary>
		TraceLevel,

		/// <summary>
		/// The value must be a VT_UI4 that is a LANGID. It defaults to the default user UI language.
		/// </summary>
		LanguageKeywords,

		/// <summary>
		/// The value must be a VT_UI4 that is a STRUCTURED_QUERY_SYNTAX value.
		/// It defaults to SQS_NATURAL_QUERY_SYNTAX.
		/// </summary>
		Syntax,

		/// <summary>
		/// The value must be a VT_BLOB that is a copy of a TIME_ZONE_INFORMATION structure.
		/// It defaults to the current time zone.
		/// </summary>
		TimeZone,

		/// <summary>
		/// This setting decides what connector should be assumed between conditions when none is specified.
		/// The value must be a VT_UI4 that is a CONDITION_TYPE. Only CT_AND_CONDITION and CT_OR_CONDITION
		/// are valid. It defaults to CT_AND_CONDITION.
		/// </summary>
		ImplicitConnector,

		/// <summary>
		/// This setting decides whether there are special requirements on the case of connector keywords (such
		/// as AND or OR). The value must be a VT_UI4 that is a CASE_REQUIREMENT value.
		/// It defaults to CASE_REQUIREMENT_UPPER_IF_AQS.
		/// </summary>
		ConnectorCase,

	}

	/// <summary>
	/// Provides a set of flags to be used with IQueryParser::SetMultiOption 
	/// to indicate individual options.
	/// </summary>
	public enum StructuredQueryMultipleOption {
		/// <summary>
		/// The key should be property name P. The value should be a
		/// VT_UNKNOWN with an IEnumVARIANT which has two values: a VT_BSTR that is another
		/// property name Q and a VT_I4 that is a CONDITION_OPERATION cop. A predicate with
		/// property name P, some operation and a value V will then be replaced by a predicate
		/// with property name Q, operation cop and value V before further processing happens.
		/// </summary>
		VirtualProperty,

		/// <summary>
		/// The key should be a value type name V. The value should be a
		/// VT_LPWSTR with a property name P. A predicate with no property name and a value of type
		/// V (or any subtype of V) will then use property P.
		/// </summary>
		DefaultProperty,

		/// <summary>
		/// The key should be a value type name V. The value should be a
		/// VT_UNKNOWN with a IConditionGenerator G. The GenerateForLeaf method of
		/// G will then be applied to any predicate with value type V and if it returns a query
		/// expression, that will be used. If it returns NULL, normal processing will be used
		/// instead.
		/// </summary>
		GeneratorForType,

		/// <summary>
		/// The key should be a property name P. The value should be a VT_VECTOR|VT_LPWSTR,
		/// where each string is a property name. The count must be at least one. This "map" will be
		/// added to those of the loaded schema and used during resolution. A second call with the
		/// same key will replace the current map. If the value is VT_NULL, the map will be removed.
		/// </summary>
		MapProperty,
	}

	/// <summary>
	/// Used by IQueryParserManager::SetOption to set parsing options. 
	/// This can be used to specify schemas and localization options.
	/// </summary>
	public enum QueryParserManagerOption {
		/// <summary>
		/// A VT_LPWSTR containing the name of the file that contains the schema binary. 
		/// The default value is StructuredQuerySchema.bin for the SystemIndex catalog 
		/// and StructuredQuerySchemaTrivial.bin for the trivial catalog.
		/// </summary>
		SchemaBinaryName = 0,

		/// <summary>
		/// Either a VT_BOOL or a VT_LPWSTR. If the value is a VT_BOOL and is FALSE, 
		/// a pre-localized schema will not be used. If the value is a VT_BOOL and is TRUE, 
		/// IQueryParserManager will use the pre-localized schema binary in 
		/// "%ALLUSERSPROFILE%\Microsoft\Windows". If the value is a VT_LPWSTR, the value should 
		/// contain the full path of the folder in which the pre-localized schema binary can be found. 
		/// The default value is VT_BOOL with TRUE.
		/// </summary>
		PreLocalizedSchemaBinaryPath = 1,

		/// <summary>
		/// A VT_LPWSTR containing the full path to the folder that contains the 
		/// unlocalized schema binary. The default value is "%SYSTEMROOT%\System32".
		/// </summary>
		UnlocalizedSchemaBinaryPath = 2,

		/// <summary>
		/// A VT_LPWSTR containing the full path to the folder that contains the 
		/// localized schema binary that can be read and written to as needed. 
		/// The default value is "%LOCALAPPDATA%\Microsoft\Windows".
		/// </summary>
		LocalizedSchemaBinaryPath = 3,

		/// <summary>
		/// A VT_BOOL. If TRUE, then the paths for pre-localized and localized binaries 
		/// have "\(LCID)" appended to them, where language code identifier (LCID) is 
		/// the decimal locale ID for the localized language. The default is TRUE.
		/// </summary>
		AppendLCIDToLocalizedPath = 4,

		/// <summary>
		/// A VT_UNKNOWN with an object supporting ISchemaLocalizerSupport. 
		/// This object will be used instead of the default localizer support object.
		/// </summary>
		LocalizerSupport = 5
	}

	// Summary:
	//     Provides the managed definition of the IPersistStream interface, with functionality
	//     from IPersist.
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("00000109-0000-0000-C000-000000000046")]
	internal interface IPersistStream {
		// Summary:
		//     Retrieves the class identifier (CLSID) of an object.
		//
		// Parameters:
		//   pClassID:
		//     When this method returns, contains a reference to the CLSID. This parameter
		//     is passed uninitialized.
		[PreserveSig]
		void GetClassID(out Guid pClassID);
		//
		// Summary:
		//     Checks an object for changes since it was last saved to its current file.
		//
		// Returns:
		//     S_OK if the file has changed since it was last saved; S_FALSE if the file
		//     has not changed since it was last saved.
		[PreserveSig]
		HResult IsDirty();

		[PreserveSig]
		HResult Load([In, MarshalAs(UnmanagedType.Interface)] IStream stm);

		[PreserveSig]
		HResult Save([In, MarshalAs(UnmanagedType.Interface)] IStream stm, bool fRemember);

		[PreserveSig]
		HResult GetSizeMax(out ulong cbSize);
	}

	[ComImport(),
	Guid(InterfaceGuids.ICondition),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ICondition : IPersistStream {
		// Summary:
		//     Retrieves the class identifier (CLSID) of an object.
		//
		// Parameters:
		//   pClassID:
		//     When this method returns, contains a reference to the CLSID. This parameter
		//     is passed uninitialized.
		[PreserveSig]
		new void GetClassID(out Guid pClassID);
		//
		// Summary:
		//     Checks an object for changes since it was last saved to its current file.
		//
		// Returns:
		//     S_OK if the file has changed since it was last saved; S_FALSE if the file
		//     has not changed since it was last saved.
		[PreserveSig]
		new HResult IsDirty();

		[PreserveSig]
		new HResult Load([In, MarshalAs(UnmanagedType.Interface)] IStream stm);

		[PreserveSig]
		new HResult Save([In, MarshalAs(UnmanagedType.Interface)] IStream stm, bool fRemember);

		[PreserveSig]
		new HResult GetSizeMax(out ulong cbSize);

		// For any node, return what kind of node it is.
		[PreserveSig]
		HResult GetConditionType([Out()] out SearchConditionType pNodeType);

		// riid must be IID_IEnumUnknown, IID_IEnumVARIANT or IID_IObjectArray, or in the case of a negation node IID_ICondition.
		// If this is a leaf node, E_FAIL will be returned.
		// If this is a negation node, then if riid is IID_ICondition, *ppv will be set to a single ICondition, otherwise an enumeration of one.
		// If this is a conjunction or a disjunction, *ppv will be set to an enumeration of the subconditions.
		[PreserveSig]
		HResult GetSubConditions([In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

		// If this is not a leaf node, E_FAIL will be returned.
		// Retrieve the property name, operation and value from the leaf node.
		// Any one of ppszPropertyName, pcop and ppropvar may be NULL.
		[PreserveSig]
		HResult GetComparisonInfo(
				[Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszPropertyName,
				[Out] out SearchConditionOperation pcop,
				[Out] PropVariant ppropvar);

		// If this is not a leaf node, E_FAIL will be returned.
		// *ppszValueTypeName will be set to the semantic type of the value, or to NULL if this is not meaningful.
		[PreserveSig]
		HResult GetValueType([Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszValueTypeName);

		// If this is not a leaf node, E_FAIL will be returned.
		// If the value of the leaf node is VT_EMPTY, *ppszNormalization will be set to an empty string.
		// If the value is a string (VT_LPWSTR, VT_BSTR or VT_LPSTR), then *ppszNormalization will be set to a
		// character-normalized form of the value.
		// Otherwise, *ppszNormalization will be set to some (character-normalized) string representation of the value.
		[PreserveSig]
		HResult GetValueNormalization([Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszNormalization);

		// Return information about what parts of the input produced the property, the operation and the value.
		// Any one of ppPropertyTerm, ppOperationTerm and ppValueTerm may be NULL.
		// For a leaf node returned by the parser, the position information of each IRichChunk identifies the tokens that
		// contributed the property/operation/value, the string value is the corresponding part of the input string, and
		// the PROPVARIANT is VT_EMPTY.
		[PreserveSig]
		HResult GetInputTerms([Out] out IRichChunk ppPropertyTerm, [Out] out IRichChunk ppOperationTerm, [Out] out IRichChunk ppValueTerm);

		// Make a deep copy of this ICondition.
		[PreserveSig]
		HResult Clone([Out()] out ICondition ppc);
	}

	[ComImport,
		Guid(InterfaceGuids.IRichChunk),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IRichChunk {
		// The position *pFirstPos is zero-based.
		// Any one of pFirstPos, pLength, ppsz and pValue may be NULL.
		[PreserveSig]
		HResult GetData(/*[out, annotation("__out_opt")] ULONG* pFirstPos, [out, annotation("__out_opt")] ULONG* pLength, [out, annotation("__deref_opt_out_opt")] LPWSTR* ppsz, [out, annotation("__out_opt")] PROPVARIANT* pValue*/);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(InterfaceGuids.IEnumUnknown)]
	internal interface IEnumUnknown {
		[PreserveSig]
		HResult Next(UInt32 requestedNumber, ref IntPtr buffer, ref UInt32 fetchedNumber);
		[PreserveSig]
		HResult Skip(UInt32 number);
		[PreserveSig]
		HResult Reset();
		[PreserveSig]
		HResult Clone(out IEnumUnknown result);
	}


	[ComImport,
	Guid(InterfaceGuids.IConditionFactory),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IConditionFactory {
		[PreserveSig]
		HResult MakeNot([In] ICondition pcSub, [In] bool fSimplify, [Out] out ICondition ppcResult);

		[PreserveSig]
		HResult MakeAndOr([In] SearchConditionType ct, [In] IEnumUnknown peuSubs, [In] bool fSimplify, [Out] out ICondition ppcResult);

		[PreserveSig]
		HResult MakeLeaf(
				[In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
				[In] SearchConditionOperation cop,
				[In, MarshalAs(UnmanagedType.LPWStr)] string pszValueType,
				[In] PropVariant ppropvar,
				IRichChunk richChunk1,
				IRichChunk richChunk2,
				IRichChunk richChunk3,
				[In] bool fExpand,
				[Out] out ICondition ppcResult);

		[PreserveSig]
		HResult Resolve(/*[In] ICondition pc, [In] STRUCTURED_QUERY_RESOLVE_OPTION sqro, [In] ref SYSTEMTIME pstReferenceTime, [Out] out ICondition ppcResolved*/);

	}

	[ComImport,
	Guid(InterfaceGuids.ISearchFolderItemFactory),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISearchFolderItemFactory {
		[PreserveSig]
		HResult SetDisplayName([In, MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName);

		[PreserveSig]
		HResult SetFolderTypeID([In] Guid ftid);

		[PreserveSig]
		HResult SetFolderLogicalViewMode([In] FolderLogicalViewMode flvm);

		[PreserveSig]
		HResult SetIconSize([In] int iIconSize);

		[PreserveSig]
		HResult SetVisibleColumns([In] uint cVisibleColumns, [In, MarshalAs(UnmanagedType.LPArray)] PROPERTYKEY[] rgKey);

		[PreserveSig]
		HResult SetSortColumns([In] uint cSortColumns, [In] IntPtr rgSortColumns);

		[PreserveSig]
		HResult SetGroupColumn([In] ref PROPERTYKEY keyGroup);

		[PreserveSig]
		HResult SetStacks([In] uint cStackKeys, [In, MarshalAs(UnmanagedType.LPArray)] PROPERTYKEY[] rgStackKeys);

		[PreserveSig]
		HResult SetScope([In, MarshalAs(UnmanagedType.Interface)] IShellItemArray ppv);

		[PreserveSig]
		HResult SetCondition([In] ICondition pCondition);

		[PreserveSig]
		int GetShellItem(ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

		[PreserveSig]
		HResult GetIDList([Out] IntPtr ppidl);
	}

	
	[ComImport,
	Guid(InterfaceGuids.ISearchFolderItemFactory),
	CoClass(typeof(SearchFolderItemFactoryCoClass))]
	internal interface INativeSearchFolderItemFactory : ISearchFolderItemFactory { //TODO: Remove INativeSearchFolderItemFactory if it will never be used
	}
	

	[ComImport,
	ClassInterface(ClassInterfaceType.None),
	TypeLibType(TypeLibTypeFlags.FCanCreate),
	Guid(InterfaceGuids.SearchFolderItemFactory)]
	internal class SearchFolderItemFactoryCoClass {
	}

	[ComImport,
	Guid(InterfaceGuids.IQuerySolution),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IQuerySolution : IConditionFactory {
		[PreserveSig]
		new HResult MakeNot([In] ICondition pcSub, [In] bool fSimplify, [Out] out ICondition ppcResult);

		[PreserveSig]
		new HResult MakeAndOr([In] SearchConditionType ct, [In] IEnumUnknown peuSubs, [In] bool fSimplify, [Out] out ICondition ppcResult);

		[PreserveSig]
		new HResult MakeLeaf(
				[In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName,
				[In] SearchConditionOperation cop,
				[In, MarshalAs(UnmanagedType.LPWStr)] string pszValueType,
				[In] PropVariant ppropvar,
				IRichChunk richChunk1,
				IRichChunk richChunk2,
				IRichChunk richChunk3,
				[In] bool fExpand,
				[Out] out ICondition ppcResult);

		[PreserveSig]
		new HResult Resolve(/*[In] ICondition pc, [In] int sqro, [In] ref SYSTEMTIME pstReferenceTime, [Out] out ICondition ppcResolved*/);

		// Retrieve the condition tree and the "main type" of the solution.
		// ppQueryNode and ppMainType may be NULL.
		[PreserveSig]
		HResult GetQuery([Out, MarshalAs(UnmanagedType.Interface)] out ICondition ppQueryNode, [Out, MarshalAs(UnmanagedType.Interface)] out IEntity ppMainType);

		// Identify parts of the input string not accounted for.
		// Each parse error is represented by an IRichChunk where the position information
		// reflect token counts, the string is NULL and the value is a VT_I4
		// where lVal is from the ParseErrorType enumeration. The valid
		// values for riid are IID_IEnumUnknown and IID_IEnumVARIANT.
		[PreserveSig]
		HResult GetErrors([In] ref Guid riid, [Out] out /* void** */ IntPtr ppParseErrors);

		// Report the query string, how it was tokenized and what LCID and word breaker were used (for recognizing keywords).
		// ppszInputString, ppTokens, pLocale and ppWordBreaker may be NULL.
		[PreserveSig]
		HResult GetLexicalData([MarshalAs(UnmanagedType.LPWStr)] out string ppszInputString, [Out] /* ITokenCollection** */ out IntPtr ppTokens, [Out] out uint plcid, [Out] /* IUnknown** */ out IntPtr ppWordBreaker);
	}

	[ComImport,
	Guid(InterfaceGuids.IQueryParser),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IQueryParser {
		// Parse parses an input string, producing a query solution.
		// pCustomProperties should be an enumeration of IRichChunk objects, one for each custom property
		// the application has recognized. pCustomProperties may be NULL, equivalent to an empty enumeration.
		// For each IRichChunk, the position information identifies the character span of the custom property,
		// the string value should be the name of an actual property, and the PROPVARIANT is completely ignored.
		[PreserveSig]
		HResult Parse([In, MarshalAs(UnmanagedType.LPWStr)] string pszInputString, [In] IEnumUnknown pCustomProperties, [Out] out IQuerySolution ppSolution);

		// Set a single option. See STRUCTURED_QUERY_SINGLE_OPTION above.
		[PreserveSig]
		HResult SetOption([In] StructuredQuerySingleOption option, [In] PropVariant pOptionValue);

		[PreserveSig]
		HResult GetOption([In] StructuredQuerySingleOption option, [Out] PropVariant pOptionValue);

		// Set a multi option. See STRUCTURED_QUERY_MULTIOPTION above.
		[PreserveSig]
		HResult SetMultiOption([In] StructuredQueryMultipleOption option, [In, MarshalAs(UnmanagedType.LPWStr)] string pszOptionKey, [In] PropVariant pOptionValue);

		// Get a schema provider for browsing the currently loaded schema.
		[PreserveSig]
		HResult GetSchemaProvider([Out] out /*ISchemaProvider*/ IntPtr ppSchemaProvider);

		// Restate a condition as a query string according to the currently selected syntax.
		// The parameter fUseEnglish is reserved for future use; must be FALSE.
		[PreserveSig]
		HResult RestateToString([In] ICondition pCondition, [In] bool fUseEnglish, [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszQueryString);

		// Parse a condition for a given property. It can be anything that would go after 'PROPERTY:' in an AQS expession.
		[PreserveSig]
		HResult ParsePropertyValue([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName, [In, MarshalAs(UnmanagedType.LPWStr)] string pszInputString, [Out] out IQuerySolution ppSolution);

		// Restate a condition for a given property. If the condition contains a leaf with any other property name, or no property name at all,
		// E_INVALIDARG will be returned.
		[PreserveSig]
		HResult RestatePropertyValueToString([In] ICondition pCondition, [In] bool fUseEnglish, [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszPropertyName, [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszQueryString);
	}

	[ComImport,
	Guid(InterfaceGuids.IQueryParserManager),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IQueryParserManager {
		// Create a query parser loaded with the schema for a certain catalog localize to a certain language, and initialized with
		// standard defaults. One valid value for riid is IID_IQueryParser.
		[PreserveSig]
		HResult CreateLoadedParser([In, MarshalAs(UnmanagedType.LPWStr)] string pszCatalog, [In] ushort langidForKeywords, [In] ref Guid riid, [Out] out IQueryParser ppQueryParser);

		// In addition to setting AQS/NQS and automatic wildcard for the given query parser, this sets up standard named entity handlers and
		// sets the keyboard locale as locale for word breaking.
		[PreserveSig]
		HResult InitializeOptions([In] bool fUnderstandNQS, [In] bool fAutoWildCard, [In] IQueryParser pQueryParser);

		// Change one of the settings for the query parser manager, such as the name of the schema binary, or the location of the localized and unlocalized
		// schema binaries. By default, the settings point to the schema binaries used by Windows Shell.
		[PreserveSig]
		HResult SetOption([In] QueryParserManagerOption option, [In] PropVariant pOptionValue);

	}

	/*
	[ComImport,
	Guid(InterfaceGuids.IQueryParserManager),
	CoClass(typeof(QueryParserManagerCoClass))]
	internal interface INativeQueryParserManager : IQueryParserManager {
	}
	*/

	[ComImport,
	ClassInterface(ClassInterfaceType.None),
	TypeLibType(TypeLibTypeFlags.FCanCreate),
	Guid(InterfaceGuids.QueryParserManager)]
	internal class QueryParserManagerCoClass {
	}

	[ComImport,
	Guid("24264891-E80B-4fd3-B7CE-4FF2FAE8931F"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEntity {
	}
}
