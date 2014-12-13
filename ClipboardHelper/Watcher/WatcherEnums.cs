

//TEnums shoud be same as in the ClipbordWatcherTypes.h file
namespace ClipboardHelper.Watcher
{
    enum MsgSeverity{
	Error = 0,
	Warning = 1,
	Info = 2,
	SendData = 3,
	PostData = 4,
	Debug=5,
}
	public enum MsgType{
	WindowHandle=0,
	CopyData=1,
	ClipboardUpdate=2,
	DestroyClipboard=3,
	RenderFormat=4,
    RenderAllFormats = 5,
}

}

