
#include "stdafx.h"


class MsgWindow{
public:
	MsgWindow(Messages* comunicator);
private:
	std::wstring windowClassName;
	HWND messageWindow = NULL;
	Messages* comunicator;
	virtual LRESULT WindProc(UINT uMsg, WPARAM wParam, LPARAM lParam);
public:
	void InitWindowClass(const std::wstring& windClass);
	void CreateMessageWindow();
	void RegisterClipboardListener();
	void RemoveClipboardListener();

	void ChagngeMessageFilter();
	void StartMainLoop();
	
	~MsgWindow();
private:
	static std::map<HWND, MsgWindow*> window_map;
	static LRESULT CALLBACK WindProcGlobal(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
};

