#include "Messages.h"
#include "stdafx.h"

namespace nsc = ClipbordWatcherTypes;
void Messages::WriteOut(nsc::MsgSeverity msgType, const std::string msg){

	std::cout << static_cast<int>(msgType) << ":" << msg.c_str() << std::endl;
}


void Messages::WriteMsg(nsc::MsgSeverity msgType, std::string format, va_list args)
{
	int size = format.length() * 2;
	char* buffer = new char[size];
	int count = vsprintf_s(buffer,sizeof(char)*size, format.c_str(), args);
	std::string message = std::string(buffer, count + 1);
	delete(buffer);
	WriteOut(nsc::MsgSeverity::AppData, message);
}


void Messages::WriteInfo(std::string format, ...)
{
	va_list args;
	va_start(args, format);
	WriteMsg(nsc::MsgSeverity::Info, format);
	va_end(args);
}

void Messages::WriteError(const std::string errFunction,std::string format, ...){
	auto errCode = GetLastError();
	std::ostringstream msgStream;
	msgStream << "ErrCode" << errCode << ":" << errFunction << ":" << format;
	va_list args;
	va_start(args, format);
	WriteMsg(nsc::MsgSeverity::Error, msgStream.str(), args);
	va_end(args);
}

void Messages::WriteError(const std::string errFunction){
	auto errCode = GetLastError();
	std::ostringstream msgStream;
	msgStream << "ErrCode" << errCode << ":" << errFunction << ":";

	WriteOut(nsc::MsgSeverity::Error, msgStream.str());
}

void Messages::WriteDebug(std::string format, ...){
	va_list args;
	va_start(args, format);
	WriteMsg(nsc::MsgSeverity::Debug, format, args);
	va_end(args);
}