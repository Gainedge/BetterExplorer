//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

#include <Windows.h>
#include <ShObjIdl.h>
#include <Shlobj.h>
#include <UIAutomationCore.h>
#include "CComPtr.h"
#include "..\MinHook\MinHook.h"

// Hook declaration macro
#define DECLARE_HOOK(id, ret, name, params)                                         \
    typedef ret (WINAPI *__TYPE__##name)params; /* Function pointer type        */  \
    ret WINAPI Detour##name params;             /* Detour function              */  \
    __TYPE__##name fp##name = NULL;             /* Pointer to original function */  \
    const int hook##name = id;                  /* Hook ID                      */ 

// Hook creation macros
#define CREATE_HOOK(address, name) {                                                            \
    MH_STATUS ret = MH_CreateHook(address, &Detour##name, reinterpret_cast<void**>(&fp##name)); \
    if(ret == MH_OK) ret = MH_EnableHook(address);                                              \
    callbacks.fpHookResult(hook##name, ret);                                                    \
}
#define CREATE_COM_HOOK(punk, idx, name) \
    CREATE_HOOK((*(void***)((IUnknown*)(punk)))[idx], name)

// A few undocumented interfaces and classes, of which we only really need the IIDs.
MIDL_INTERFACE("0B907F92-1B63-40C6-AA54-0D3117F03578") IListControlHost     : public IUnknown {};
MIDL_INTERFACE("66A9CB08-4802-11d2-A561-00A0C92DBFE8") ITravelLog           : public IUnknown {};
MIDL_INTERFACE("3050F679-98B5-11CF-BB82-00AA00BDCE0B") ITravelLogEx         : public IUnknown {};
MIDL_INTERFACE("7EBFDD87-AD18-11d3-A4C5-00C04F72D6B8") ITravelLogEntry      : public IUnknown {};
MIDL_INTERFACE("489E9453-869B-4BCC-A1C7-48B5285FD9D8") ICommonExplorerHost  : public IUnknown {};
MIDL_INTERFACE("A86304A7-17CA-4595-99AB-523043A9C4AC") IExplorerFactory 	: public IUnknown {};
MIDL_INTERFACE("E93D4057-B9A2-42A5-8AF8-E5BBF177D365") IShellNavigationBand : public IUnknown {};
MIDL_INTERFACE("596742A5-1393-4E13-8765-AE1DF71ACAFB") CBreadcrumbBar {};
MIDL_INTERFACE("93A56381-E0CD-485A-B60E-67819E12F81B") CExplorerFactoryServer {};

// Win7's IShellBrowserService interface, of which we only need one function.
MIDL_INTERFACE("DFBC7E30-F9E5-455F-88F8-FA98C1E494CA")
IShellBrowserService_7 : public IUnknown {
public:
    virtual HRESULT STDMETHODCALLTYPE Unused0() = 0;
    virtual HRESULT STDMETHODCALLTYPE GetTravelLog(ITravelLog** ppTravelLog) = 0;
};

// Vista's IShellBrowserService.
MIDL_INTERFACE("42DAD0E2-9B43-4E7A-B9D4-E6D1FF85D173")
IShellBrowserService_Vista : public IUnknown {
public:
    virtual HRESULT STDMETHODCALLTYPE Unused0() = 0;
    virtual HRESULT STDMETHODCALLTYPE Unused1() = 0;
    virtual HRESULT STDMETHODCALLTYPE Unused2() = 0;
    virtual HRESULT STDMETHODCALLTYPE Unused3() = 0;
    virtual HRESULT STDMETHODCALLTYPE GetTravelLog(ITravelLog** ppTravelLog) = 0;
};

// Hooks
DECLARE_HOOK( 0, HRESULT, CoCreateInstance, (REFCLSID rclsid, LPUNKNOWN pUnkOuter, DWORD dwClsContext, REFIID riid, LPVOID *ppv))
DECLARE_HOOK( 1, HRESULT, RegisterDragDrop, (HWND hwnd, LPDROPTARGET pDropTarget))
DECLARE_HOOK( 2, HRESULT, SHCreateShellFolderView, (const SFV_CREATE* pcsfv, IShellView** ppsv))
DECLARE_HOOK( 3, HRESULT, BrowseObject, (IShellBrowser* _this, PCUIDLIST_RELATIVE pidl, UINT wFlags))
DECLARE_HOOK( 4, HRESULT, CreateViewWindow3, (IShellView3* _this, IShellBrowser* psbOwner, IShellView* psvPrev, SV3CVW3_FLAGS dwViewFlags, FOLDERFLAGS dwMask, FOLDERFLAGS dwFlags, FOLDERVIEWMODE fvMode, const SHELLVIEWID* pvid, const RECT* prcView, HWND* phwndView))
DECLARE_HOOK( 5, HRESULT, MessageSFVCB, (IShellFolderViewCB* _this, UINT uMsg, WPARAM wParam, LPARAM lParam))
DECLARE_HOOK( 6, LRESULT, UiaReturnRawElementProvider, (HWND hwnd, WPARAM wParam, LPARAM lParam, IRawElementProviderSimple* el))
DECLARE_HOOK( 7, HRESULT, QueryInterface, (IRawElementProviderSimple* _this, REFIID riid, void** ppvObject))
DECLARE_HOOK( 8, HRESULT, TravelToEntry, (ITravelLogEx* _this, IUnknown* punk, ITravelLogEntry* ptle))
DECLARE_HOOK( 9, HRESULT, OnActivateSelection, (IListControlHost* _this, DWORD dwModifierKeys))
DECLARE_HOOK(10, HRESULT, SetNavigationState, (IShellNavigationBand* _this, unsigned long state))
DECLARE_HOOK(11, HRESULT, ShowWindow_Vista, (IExplorerFactory* _this, PCIDLIST_ABSOLUTE pidl, DWORD flags, DWORD mystery1, DWORD mystery2, POINT pt))
DECLARE_HOOK(11, HRESULT, ShowWindow_7, (ICommonExplorerHost* _this, PCIDLIST_ABSOLUTE pidl, DWORD flags, POINT pt, DWORD mystery))
DECLARE_HOOK(12, HRESULT, UpdateWindowList, (/*IShellBrowserService*/ IUnknown* _this))
DECLARE_HOOK(13, HRESULT, DeleteItem, (IFileOperation *pThis, IUnknown *psiItem))
DECLARE_HOOK(14, HRESULT, RenameItem, (IFileOperation *pThis, IUnknown *psiItem, LPCWSTR pszNewName, IFileOperationProgressSink *pfopsItem))
DECLARE_HOOK(15, HRESULT, MoveItem, (IFileOperation *pThis, IUnknown *psiItem, IShellItem *psiDestinationFolder))
DECLARE_HOOK(16, HRESULT, CopyItem, (IFileOperation *pThis, IUnknown *psiItem, IShellItem *psiDestinationFolder, IUnknown *sinc))

// Messages
unsigned int WM_REGISTERDRAGDROP;
unsigned int WM_NEWTREECONTROL;
unsigned int WM_BROWSEOBJECT;
unsigned int WM_HEADERINALLVIEWS;
unsigned int WM_LISTREFRESHED;
unsigned int WM_ISITEMSVIEW;
unsigned int WM_ACTIVATESEL;
unsigned int WM_BREADCRUMBDPA;
unsigned int WM_CHECKPULSE;
unsigned int WM_FILEOPERATION;

// Callback struct
struct CallbackStruct {
    void (*fpHookResult)(int hookId, int retcode);
    bool (*fpNewWindow)(LPCITEMIDLIST pIDL);
	bool (*fpCopyItem)(IUnknown* ifrom, IShellItem* ito);
	bool (*fpMoveItem)(IUnknown* ifrom, IShellItem* ito);
	bool (*fpRenameItem)(IUnknown* ifrom, LPCWSTR pszNewName);
	bool (*fpDeleteItem)(IUnknown *ifrom);
};
CallbackStruct callbacks;

// Other stuff
HMODULE hModAutomation = NULL;
FARPROC fpRealRREP = NULL;
FARPROC fpRealCI = NULL;

extern "C" __declspec(dllexport) int Initialize(CallbackStruct* cb);
extern "C" __declspec(dllexport) int Dispose();
extern "C" __declspec(dllexport) int InitShellBrowserHook(IShellBrowser* psb);

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
        case DLL_PROCESS_DETACH:
            return Dispose();

        case DLL_PROCESS_ATTACH:
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
            break;
    }
    return true;
}

int Initialize(CallbackStruct* cb) {

    volatile static long initialized;
    if(InterlockedIncrement(&initialized) != 1) {
        // Return if another thread has beaten us here.
        initialized = 1;
        return MH_OK;
    }

    // Initialize MinHook.
    MH_STATUS ret = MH_Initialize();
    if(ret != MH_OK && ret != MH_ERROR_ALREADY_INITIALIZED) return ret;

    // Store the callback struct
    callbacks = *cb;

    // Register the messages.
    WM_REGISTERDRAGDROP = RegisterWindowMessageA("BE_RegisterDragDrop");
    WM_NEWTREECONTROL   = RegisterWindowMessageA("BE_NewTreeControl");
    WM_BROWSEOBJECT     = RegisterWindowMessageA("BE_BrowseObject");
    WM_HEADERINALLVIEWS = RegisterWindowMessageA("BE_HeaderInAllViews");
    WM_LISTREFRESHED    = RegisterWindowMessageA("BE_ListRefreshed");
    WM_ISITEMSVIEW      = RegisterWindowMessageA("BE_IsItemsView");
    WM_ACTIVATESEL      = RegisterWindowMessageA("BE_ActivateSelection");
    WM_BREADCRUMBDPA    = RegisterWindowMessageA("BE_BreadcrumbDPA");
    WM_CHECKPULSE       = RegisterWindowMessageA("BE_CheckPulse");
	WM_FILEOPERATION	= RegisterWindowMessageA("BE_FileOperation");

    // Create and enable the CoCreateInstance, RegisterDragDrop, and SHCreateShellFolderView hooks.
    CREATE_HOOK(&CoCreateInstance, CoCreateInstance);
    //CREATE_HOOK(&RegisterDragDrop, RegisterDragDrop);
    //CREATE_HOOK(&SHCreateShellFolderView, SHCreateShellFolderView);


    // Create an instance of the breadcrumb bar so we can hook it.
    //CComPtr<IShellNavigationBand> psnb;
    //if(psnb.Create(__uuidof(CBreadcrumbBar), CLSCTX_INPROC_SERVER)) {
    //    CREATE_COM_HOOK(psnb, 4, SetNavigationState)
    //}


    // Create an instance of CExplorerFactoryServer so we can hook it.
    // The interface in question is different on Vista and 7.
    //CComPtr<IUnknown> punk;
    //if(punk.Create(__uuidof(CExplorerFactoryServer), CLSCTX_INPROC_SERVER)) {
    //    CComPtr<ICommonExplorerHost> pceh;
    //    CComPtr<IExplorerFactory> pef;
    //    if(pceh.QueryFrom(punk)) {
    //        CREATE_COM_HOOK(pceh, 3, ShowWindow_7)
    //    }
    //    else if(pef.QueryFrom(punk)) {
    //        CREATE_COM_HOOK(pef, 3, ShowWindow_Vista)
    //    }
    //}
    return MH_OK;
}


int Dispose() {
    // Uninitialize MinHook.
    MH_Uninitialize();

    // Free the Automation library
    if(hModAutomation != NULL) {
        FreeLibrary(hModAutomation);
    }

    return S_OK;
}

PVOID GetInterfaceMethod(PVOID intf, DWORD methodIndex)
{

	#if defined(_WIN64)
		return *(PVOID*)(*(DWORD_PTR*)intf + methodIndex*8);
	#elif defined(_WIN32)
		return *(PVOID*)(*(DWORD_PTR*)intf + methodIndex*4);
	#endif
}

//////////////////////////////
// Detour Functions
//////////////////////////////

// The purpose of this hook is to intercept the creation of the NameSpaceTreeControl object, and 
// send a reference to the control to QTTabBar.  We can use this reference to hit test the
// control, which is how opening new tabs from middle-click works.
HRESULT WINAPI DetourCoCreateInstance(REFCLSID rclsid, LPUNKNOWN pUnkOuter, DWORD dwClsContext, REFIID riid, LPVOID *ppv) {
    HRESULT ret = fpCoCreateInstance(rclsid, pUnkOuter, dwClsContext, riid, ppv);
    if(SUCCEEDED(ret)) {
		if (IsEqualIID(rclsid, CLSID_NamespaceTreeControl)){
			PostThreadMessage(GetCurrentThreadId(), WM_NEWTREECONTROL, (WPARAM)(*ppv), NULL);
		}
		if (IsEqualIID(rclsid, CLSID_FileOperation)){
			//PostThreadMessage(GetCurrentThreadId(), WM_FILEOPERATION, (WPARAM)(*ppv), NULL);
			//CREATE_COM_HOOK(GetInterfaceMethod(*ppv, 12),13,CopyItem);
			CREATE_HOOK(GetInterfaceMethod(*ppv, 17),CopyItem);
			CREATE_HOOK(GetInterfaceMethod(*ppv, 15),MoveItem);
			CREATE_HOOK(GetInterfaceMethod(*ppv, 12),RenameItem);
			CREATE_HOOK(GetInterfaceMethod(*ppv, 19),DeleteItem);
		}
    }  
    return ret;
}



HRESULT WINAPI DetourRenameItem(IFileOperation *pThis, IUnknown *psiItem, LPCWSTR pszNewName, IFileOperationProgressSink *pfopsItem){
	return callbacks.fpRenameItem(psiItem, pszNewName)? S_OK : fpRenameItem(pThis,psiItem,pszNewName,pfopsItem);
}

HRESULT WINAPI DetourDeleteItem(IFileOperation *pThis, IUnknown *psiItem){
	return callbacks.fpDeleteItem(psiItem) ? S_OK : fpDeleteItem(pThis,psiItem);
}

HRESULT WINAPI DetourCopyItem(IFileOperation *pThis, IUnknown *psiItem, IShellItem *psiDestinationFolder, IUnknown *sinc){
	callbacks.fpCopyItem(psiItem, psiDestinationFolder);
	return S_OK;
}
HRESULT WINAPI DetourMoveItem(IFileOperation *pThis, IUnknown *psiItem, IShellItem *psiDestinationFolder){
	return callbacks.fpMoveItem(psiItem, psiDestinationFolder) ? S_OK : fpMoveItem(pThis,psiItem,psiDestinationFolder);
}

