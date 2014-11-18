#include "stdafx.h"
namespace ClipbordWatcherTypes
{
	enum class MsgSeverity{
		Error = 0,
		Warning = 1,
		Info = 2,
		AppData = 3,
	};
}
namespace nsc = ClipbordWatcherTypes;
class Messages{
public:
	void WriteOut(nsc::MsgSeverity msgType, const std::string msg);
	//void WriteMsg(MsgSeverity msgType, std::string format, ...);
	void WriteMsg(nsc::MsgSeverity msgType, std::string format, va_list args);
	void WriteInfo(std::string format, ...);
	void WriteError(const std::string errFunction, std::string format, ...);
	void WriteError(const std::string errFunction);
	void WriteDebug(std::string format, ...);
};

