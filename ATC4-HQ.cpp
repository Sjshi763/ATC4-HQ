#include <windows.h>
#include <iostream>
#include <string>
#include <fstream> // 用于文件日志重定向
#include <vector>
#include <cstdlib> // 用于 atexit 函数
#include <filesystem> // 用于路径操作 (C++17)
#include <functional> // 用于 std::function
#include <map>        // 用于命令映射

// 全局日志文件流
static std::ofstream logFile;

// 定义命令处理函数类型：接收一个宽字符串参数
using CommandHandler = std::function<void(const std::wstring& param)>;
// 定义命令映射表：将宽字符串命令映射到对应的处理函数
std::map<std::wstring, CommandHandler> commandHandlers;

// 用于将控制台输出重定向到文件的函数
void RedirectConsoleOutputToFile(const std::string& filename) {
    if (logFile.is_open()) {
        logFile.close();
    }
    logFile.open(filename, std::ios_base::app); // 以追加模式打开

    if (logFile.is_open()) {
        std::cout.rdbuf(logFile.rdbuf()); // 重定向标准输出
        std::cerr.rdbuf(logFile.rdbuf()); // 重定向标准错误
        std::cout << "--- 控制台输出已重定向到 " << filename << " ---" << std::endl;
        std::cout.flush(); // 立即刷新，确保消息写入
    } else {
        std::cerr << "错误：无法打开日志文件 " << filename << std::endl;
    }
}

// 程序退出时自动调用的函数，确保日志文件被正确关闭和刷新
void CloseLogFileOnExit() {
    if (logFile.is_open()) {
        logFile.flush(); // 刷新缓冲区
        logFile.close(); // 关闭文件
    }
}

// 辅助函数：将 std::string (UTF-8) 转换为 std::wstring
std::wstring stringToWstring(const std::string& str) {
    int size_needed = MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), NULL, 0);
    std::wstring wstrTo(size_needed, 0);
    MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), &wstrTo[0], size_needed);
    return wstrTo;
}

// 辅助函数：将 std::wstring 转换为 std::string (UTF-8)，用于日志输出
std::string wstringToString(const std::wstring& wstr) {
    int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
    std::string strTo(size_needed, 0);
    WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], size_needed, NULL, NULL);
    return strTo;
}

// --- 新功能：解压命令处理函数 ---
// 这个函数包含了之前在 main 中处理解压的所有逻辑
void HandleUnzipCommand(const std::wstring& targetPath) {
    std::cout << "收到 UNZIP 命令，目标路径: \"" << wstringToString(targetPath) << "\"" << std::endl;

    // 获取当前可执行文件的目录，以便找到 minizip.exe 和 zip 文件
    wchar_t path_temp[MAX_PATH];
    GetModuleFileNameW(NULL, path_temp, MAX_PATH);
    std::filesystem::path currentExeDirectory(path_temp);
    currentExeDirectory = currentExeDirectory.parent_path(); // 获取父目录

    std::filesystem::path minizipExePath = currentExeDirectory / L"minizip.exe";
    std::filesystem::path zipFilePath = currentExeDirectory / L"ATC4ALL.zip";

    std::cout << "minizip.exe 路径设置为: " << minizipExePath.string() << std::endl;
    std::cout << "ZIP 文件路径设置为: " << zipFilePath.string() << std::endl;

    // 检查 minizip.exe 是否存在
    if (!std::filesystem::exists(minizipExePath)) {
        std::cerr << "错误：minizip.exe 不存在于路径: " << minizipExePath.string() << std::endl;
        return; // 返回，不执行解压
    }
    // 检查 ATC4ALL.zip 是否存在
    if (!std::filesystem::exists(zipFilePath)) {
        std::cerr << "错误：ATC4ALL.zip 不存在于路径: " << zipFilePath.string() << std::endl;
        return; // 返回，不执行解压
    }

    // 处理目标路径中的空格，如果需要则添加引号
    std::wstring quotedTargetPath = targetPath;
    // 检查路径是否包含空格且未被引号包裹
    if (quotedTargetPath.find(L' ') != std::wstring::npos && quotedTargetPath.front() != L'\"' && quotedTargetPath.back() != L'\"') {
        quotedTargetPath = L"\"" + quotedTargetPath + L"\""; // 添加引号
        std::cout << "目标路径包含空格，已添加引号。引用路径: \"" << wstringToString(quotedTargetPath) << "\"" << std::endl;
    } else {
        std::cout << "目标路径不包含空格或已加引号。路径: \"" << wstringToString(quotedTargetPath) << "\"" << std::endl;
    }

    // 检查并创建目标目录
    std::filesystem::path targetDirPath(targetPath); // 使用原始 targetPath 来检查目录
    // 如果路径被引号包裹，先移除引号以便正确判断目录
    if (targetDirPath.string().front() == '\"' && targetDirPath.string().back() == '\"') {
        targetDirPath = targetDirPath.string().substr(1, targetDirPath.string().length() - 2);
    }
    
    if (!std::filesystem::exists(targetDirPath)) {
        std::cout << "目标目录不存在，尝试创建: " << targetDirPath.string() << std::endl;
        std::error_code ec; // 用于捕获错误信息
        if (std::filesystem::create_directories(targetDirPath, ec)) { // 创建多级目录
            std::cout << "目标目录创建成功。" << std::endl;
        } else {
            std::cerr << "错误：无法创建目标目录 " << targetDirPath.string() << "，错误信息: " << ec.message() << std::endl;
            return; // 返回，不执行解压
        }
    } else if (!std::filesystem::is_directory(targetDirPath)) {
        std::cerr << "错误：目标路径 " << targetDirPath.string() << " 存在但不是一个目录。" << std::endl;
        return; // 返回，不执行解压
    }

    // 构建 minizip.exe 的完整命令行字符串
    std::wstring commandLine =
        minizipExePath.wstring() + L" " +   // minizip.exe 的绝对路径
        L"-x" + L" " +                       // 解压命令
        zipFilePath.wstring() + L" " +       // ZIP 文件的绝对路径
        L"-d" + L" " +                       // 指定目标目录
        quotedTargetPath;                    // 目标路径 (可能带引号)
    
    std::cout << "构建的完整命令行字符串（用于 CreateProcessW）:" << std::endl;
    std::cout << wstringToString(commandLine) << std::endl; // 打印到日志

    STARTUPINFOW si = { sizeof(si) }; // 初始化 STARTUPINFO 结构体
    PROCESS_INFORMATION pi;           // 初始化 PROCESS_INFORMATION 结构体

    // 创建 NUL 设备的句柄，用于重定向子进程的标准输入、输出和错误，使其静默运行
    HANDLE hNul = CreateFileW(L"NUL", GENERIC_WRITE | GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hNul == INVALID_HANDLE_VALUE) {
        std::cerr << "错误：无法打开 NUL 设备句柄，错误码: " << GetLastError() << std::endl;
        // 如果无法重定向到 NUL，退回到原来的隐藏窗口模式
        si.dwFlags = STARTF_USESHOWWINDOW;
        si.wShowWindow = SW_HIDE;
        std::cout << "回退到 STARTF_USESHOWWINDOW/SW_HIDE 模式。" << std::endl;
    } else {
        // 设置 STARTUPINFO 来重定向标准句柄
        si.dwFlags = STARTF_USESTDHANDLES; // 明确指出要使用自定义的标准句柄
        si.hStdInput = hNul; // 将标准输入重定向到 NUL
        si.hStdOutput = hNul; // 将标准输出重定向到 NUL
        si.hStdError = hNul;  // 将标准错误重定向到 NUL
        std::cout << "已配置标准输入、输出和错误重定向到 NUL。" << std::endl;
    }

    std::cout << "尝试启动解压进程..." << std::endl;
    // 创建子进程 (minizip.exe)
    BOOL success = CreateProcessW(
        NULL,               // lpApplicationName (使用 lpCommandLine 来指定完整路径 + 参数)
        &commandLine[0],    // lpCommandLine (此字符串会被原地修改，因此传递非 const)
        NULL,               // lpProcessAttributes
        NULL,               // lpThreadAttributes
        TRUE,               // bInheritHandles: **非常重要：必须为 TRUE，才能继承重定向的句柄**
        CREATE_NO_WINDOW,   // dwCreationFlags: 防止子进程出现新的控制台窗口
        NULL,               // lpEnvironment
        NULL,               // lpCurrentDirectory
        &si,                // lpStartupInfo (包含重定向信息)
        &pi                 // lpProcessInformation (接收进程信息)
    );

    if (success) {
        std::cout << "成功启动解压进程！进程ID: " << pi.dwProcessId << std::endl;
        CloseHandle(pi.hProcess); // 关闭进程句柄
        CloseHandle(pi.hThread);  // 关闭线程句柄
    } else {
        std::cerr << "错误：启动解压进程失败，错误码: " << GetLastError() << std::endl;
        // 额外打印当前工作目录，有时有助于调试文件找不到的问题
        wchar_t currentDir[MAX_PATH];
        GetCurrentDirectoryW(MAX_PATH, currentDir);
        std::wcout << L"当前工作目录: " << currentDir << std::endl; // 这里使用 wcout 因为可能涉及非 ASCII 字符
    }

    // **重要：使用完后关闭 NUL 句柄**
    if (hNul != INVALID_HANDLE_VALUE) {
        CloseHandle(hNul);
        std::cout << "NUL 句柄已关闭。" << std::endl;
    }
}

int main() {
    // 注册 CloseLogFileOnExit 函数，以便在程序终止时调用
    atexit(CloseLogFileOnExit);

    // 获取当前可执行文件的完整路径和目录
    wchar_t path[MAX_PATH];
    GetModuleFileNameW(NULL, path, MAX_PATH);
    std::filesystem::path exePath(path);
    std::filesystem::path exeDirectory = exePath.parent_path();
    
    // 构建日志文件的完整路径
    std::string logFileName = "pipe_server_log.txt";
    std::filesystem::path logFilePath = exeDirectory / logFileName;
    std::string logFilePathStr = logFilePath.string();

    // 将控制台输出重定向到指定的日志文件
    RedirectConsoleOutputToFile(logFilePathStr);

    // 在每次程序启动时，如果日志文件非空，则添加一个空行，以便区分不同的运行日志
    if (logFile.is_open() && logFile.tellp() > 0) {
        logFile << std::endl;
    }

    // 设置控制台代码页以支持 UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    std::cout << "--- 程序启动 ---" << std::endl; // 记录程序启动
    std::cout << "日志文件路径: " << logFilePathStr << std::endl; // 记录日志文件实际路径

    // ⭐️ 注册命令处理函数
    // 将 "UNZIP" 命令映射到 HandleUnzipCommand 函数
    commandHandlers[L"UNZIP"] = HandleUnzipCommand;
    // 如果未来有其他命令，可以在这里继续注册，例如：
    // commandHandlers[L"LOG_MESSAGE"] = [](const std::wstring& message) {
    //     std::cout << "收到日志消息: " << wstringToString(message) << std::endl;
    // };

    const char* pipeName = "\\\\.\\pipe\\ATC4Pipe";
    std::cout << "尝试创建命名管道: " << pipeName << std::endl;

    HANDLE hPipe = CreateNamedPipeA(
        pipeName,
        PIPE_ACCESS_INBOUND, // 管道方向：入站 (客户端写入，服务器读取)
        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT, // 字节类型管道，字节读取模式，阻塞模式
        1, // 最大实例数 (只有一个服务器实例)
        256, 256, // 输出缓冲区大小，输入缓冲区大小
        0, // 默认超时
        NULL // 默认安全属性
    );

    if (hPipe == INVALID_HANDLE_VALUE) {
        std::cerr << "错误：创建管道失败，错误码: " << GetLastError() << std::endl;
        return 1;
    }
    std::cout << "管道创建成功。句柄: " << hPipe << std::endl;

    std::cout << "等待客户端连接..." << std::endl;
    BOOL connected = ConnectNamedPipe(hPipe, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);
    if (!connected) {
        std::cerr << "错误：客户端连接失败，错误码: " << GetLastError() << std::endl;
        CloseHandle(hPipe);
        return 1;
    }
    std::cout << "客户端已连接！" << std::endl;

    char buffer[256] = {0}; // 用于存储从管道读取的数据的缓冲区
    DWORD bytesRead = 0;   // 实际读取的字节数

    std::cout << "尝试从管道读取数据..." << std::endl;
    if (ReadFile(hPipe, buffer, sizeof(buffer), &bytesRead, NULL)) {
        std::string fullMessage(buffer, bytesRead); // 获取完整的 IPC 消息
        std::cout << "成功从管道读取到 " << bytesRead << " 字节。" << std::endl;
        std::cout << "收到完整消息: \"" << fullMessage << "\"" << std::endl;

        // ⭐️ 新增：解析命令和参数
        size_t firstSpace = fullMessage.find(' '); // 查找第一个空格
        std::string commandStr;
        std::string paramStr;

        if (firstSpace != std::string::npos) {
            commandStr = fullMessage.substr(0, firstSpace); // 空格前是命令
            paramStr = fullMessage.substr(firstSpace + 1);  // 空格后是参数
        } else {
            // 如果没有空格，则整个消息都被视为命令（没有参数）
            commandStr = fullMessage;
            paramStr = ""; // 参数为空
        }

        // 将解析出的命令和参数转换为宽字符串
        std::wstring commandWstr = stringToWstring(commandStr);
        std::wstring paramWstr = stringToWstring(paramStr);

        std::cout << "解析出命令: \"" << commandStr << "\"" << std::endl;
        std::cout << "解析出参数: \"" << paramStr << "\"" << std::endl;

        // 根据解析出的命令，在命令映射表中查找并调用对应的处理函数
        auto it = commandHandlers.find(commandWstr);
        if (it != commandHandlers.end()) {
            it->second(paramWstr); // 调用对应的处理函数，传入参数
        } else {
            std::cerr << "错误：收到未知命令: \"" << commandStr << "\"" << std::endl;
        }

    } else {
        std::cerr << "错误：从管道读取失败，错误码: " << GetLastError() << std::endl;
    }

    CloseHandle(hPipe); // 关闭管道句柄
    std::cout << "管道已关闭，程序退出。" << std::endl;
    return 0;
}