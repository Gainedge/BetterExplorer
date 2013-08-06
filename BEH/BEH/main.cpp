#include <stdio.h>
#include <ShObjIdl.h>
#include <vector>
#include <Objbase.h>
#include <string.h>
#include <winnetwk.h>


extern "C"
{
  __declspec(dllexport) int DisplayHelloFromDLL()
  {
	return 1;
  }

  __declspec(dllexport) void GetColumnbyIndex(IShellView* view, bool isAll, int index, PROPERTYKEY &res)
  {
	  IColumnManager* columns;
	  view->QueryInterface(&columns);
	  UINT count;
	  std::vector<PROPERTYKEY> keys;
	  columns->GetColumnCount(isAll?CM_ENUM_ALL:CM_ENUM_VISIBLE, &count);
	  keys.resize(count);
	  columns->GetColumns(isAll?CM_ENUM_ALL:CM_ENUM_VISIBLE,&keys[0],count);
	  res = keys[index];
  }

   __declspec(dllexport) void GetColumnInfobyIndex(IShellView* view,bool isAll, int index, CM_COLUMNINFO &res)
  {
	 
	  IColumnManager* columns;
	  view->QueryInterface(&columns);
	  UINT count;
	  std::vector<PROPERTYKEY> keys;
	  columns->GetColumnCount(isAll?CM_ENUM_ALL:CM_ENUM_VISIBLE, &count);
	  keys.resize(count);
	  columns->GetColumns(isAll?CM_ENUM_ALL:CM_ENUM_VISIBLE,&keys[0],count);
	  CM_COLUMNINFO col = { sizeof(col), CM_MASK_WIDTH | CM_MASK_NAME | CM_MASK_IDEALWIDTH};
	  columns->GetColumnInfo(keys[index], &col);
	  res = col;

  }


   __declspec(dllexport) void GetColumnInfobyPK(IShellView* view,bool isAll, PROPERTYKEY pk, CM_COLUMNINFO &res)
  {
	 
	  IColumnManager* columns;
	  view->QueryInterface(&columns);
	  CM_COLUMNINFO col = { sizeof(col), CM_MASK_WIDTH | CM_MASK_NAME | CM_MASK_IDEALWIDTH };
	  columns->GetColumnInfo(pk, &col);
	  res = col;

  }


  __declspec(dllexport) void SetColumnInShellView(IShellView* view , int count, PROPERTYKEY pk[])
  {
	 
	  IColumnManager* columns;
	  view->QueryInterface(&columns);
	  std::vector<PROPERTYKEY> keys;
	  keys.resize(count);

	  for (int i = 0; i< count ; i++)
	  {
		  keys[i] = pk[i];
	  }
	  columns->SetColumns(&keys[0], (UINT)keys.size());
  }

	__declspec(dllexport) void SetSortColumns(IFolderView2* view , int count, SORTCOLUMN sc[])
  {
	 
	  std::vector<SORTCOLUMN> keys;
	  keys.resize(count);
	  for (int i = 0; i< count ; i++)
	  {
		  keys[i] = sc[i];
	  }
	  view->SetSortColumns(&keys[0],count);
  }

  __declspec(dllexport) void GetSortColumns(IShellView* view , int index, SORTCOLUMN &sc)
  {
	 
	  IFolderView2* view2;
	  view->QueryInterface(&view2);
	  int count;
	  std::vector<SORTCOLUMN> keys;
	  view2->GetSortColumnCount(&count);
	  keys.resize(count);
	  view2->GetSortColumns(&keys[0],count);
	  sc = keys[index];
	  
  }

  __declspec(dllexport) bool IsItemFolder(IFolderView2* view , int index)
  {
	  IShellItem2* item;
	  view->GetItem(index, IID_PPV_ARGS(&item));
	  SFGAOF sfg;
	  item->GetAttributes(SFGAO_FOLDER,&sfg);
	  if (sfg & SFGAO_FOLDER)
		  return true;
	  else
		  return false;

  }
  __declspec(dllexport) PWSTR GetItemName(IFolderView2* view , int index)
  {
	  IShellItem2* item;
	  view->GetItem(index, IID_PPV_ARGS(&item));
	  PWSTR pszFilePath = NULL;
	  item->GetDisplayName(SIGDN_DESKTOPABSOLUTEPARSING,&pszFilePath);
	  return pszFilePath;
	  CoTaskMemFree(pszFilePath);

  }

  __declspec(dllexport) void GetItemLocation(IShellView* view , int index, int &pointx, int &pointy)
  {
	  POINT p;
	  IFolderView* view2;
	  ITEMIDLIST* pidl;
	  view->QueryInterface(IID_PPV_ARGS(&view2));
	  view2->Item(index,&pidl);
	  view2->GetItemPosition(pidl,&p);
	  pointx = p.x;
	  pointy = p.y;
  }
}