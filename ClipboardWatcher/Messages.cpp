#include "Messages.h"
#include "stdafx.h"

void Messages::WriteOut(std::string msgType, const std::string msg){

	std::cout << msgType.c_str() << ":" << msg.c_str() << std::endl;
}


void Messages::WriteMsg(MsgSeverity msgType, std::string format, va_list args)
{
	int size = format.length() * 2;
	char* buffer = new char[size];
	int count = vsprintf_s(buffer,sizeof(char)*size, format.c_str(), args);
	std::string message = std::string(buffer, count + 1);
	delete(buffer);
	std::string str_msgType;
	switch (msgType){
	case MsgSeverity::AppData:
		str_msgType.assign(MsgSeverity_AppData);
		break;
	case MsgSeverity::Error:
		str_msgType.assign(MsgSeverity_Error);
		break;
	case MsgSeverity::Info:
		str_msgType.assign(MsgSeverity_Info);
		break;
	case MsgSeverity::Warning:
		str_msgType.assign(MsgSeverity_Warning);
		break;
	case MsgSeverity::Debug:
		str_msgType.assign(MsgSeverity_Debug);
		break;
	default:
		str_msgType.assign("Unknown");
		str_msgType.push_back(static_cast<char>(static_cast<int>(msgType)+48));//convert to acsii number char
	}
	WriteOut(str_msgType, message);
}


void Messages::WriteInfo(std::string format, ...)
{
	va_list args;
	va_start(args, format);
	WriteMsg(MsgSeverity::Info, format, args);
	va_end(args);
}

void Messages::WriteError(const std::string errFunction, std::string format, ...){
	auto errCode = GetLastError();
	std::ostringstream msgStream;
	msgStream << "ErrCode" << errCode << ":" << errFunction << ":" << format;
	va_list args;
	va_start(args, format);
	WriteMsg(MsgSeverity::Error, msgStream.str(), args);
	va_end(args);
}

void Messages::WriteData(std::string data){
	WriteData(data, "");
}
void Messages::WriteData(std::string data,std::string format, ...){
	std::ostringstream msgStream;
	msgStream << data << "|" << format;
	va_list args;
	va_start(args, format);
	WriteMsg(MsgSeverity::AppData, msgStream.str(), args);
	va_end(args);
}

void Messages::WriteError(const std::string errFunction){
	auto errCode = GetLastError();
	std::ostringstream msgStream;
	msgStream << "ErrCode" << errCode << ":" << errFunction << ":";
	va_list args = va_list();
	WriteMsg(MsgSeverity::Error, msgStream.str(), args);
}

void Messages::WriteDebug(std::string format, ...){
	va_list args;
	va_start(args, format);
	WriteMsg(MsgSeverity::Debug, format, args);
	va_end(args);
}