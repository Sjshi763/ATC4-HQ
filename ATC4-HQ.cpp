#include <windows.h>
#include <iostream>
#include <string>
#include <fstream> // 用于文件日志重定向
#include <vector>
#include <cstdlib> // 用于 atexit 函数
#include <filesystem> // 用于路径操作 (C++17)
#include <functional> // 用于 std::function
#include <map>        // 用于命令映射
#include <mutex>      // 用于 std::mutex
#include <algorithm>  // 用于 std::max_element, std::remove_if
#include <cstdio>     // 用于 sprintf_s
#include <locale>     // 用于 std::locale
#include <codecvt>    // 用于 std::codecvt_utf8_utf16

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

// --- 游戏数据结构体 ---
struct Game {
    int id;
    std::wstring name;
    std::wstring path;
    std::wstring addedDate;      // ISO 8601 格式的日期字符串

    // 将 Game 对象转换为文本行格式
    std::wstring ToTextLine() const {
        return std::to_wstring(id) + L"|" +
               name + L"|" +
               path + L"|" +
               addedDate;
    }

    // 从文本行解析出 Game 对象
    static Game FromTextLine(const std::wstring& line) {
        Game game;
        size_t pos = 0;
        size_t next_pos;

        // Id
        next_pos = line.find(L'|', pos);
        game.id = std::stoi(line.substr(pos, next_pos - pos));
        pos = next_pos + 1;

        // Name
        next_pos = line.find(L'|', pos);
        game.name = line.substr(pos, next_pos - pos);
        pos = next_pos + 1;

        // Path
        next_pos = line.find(L'|', pos);
        game.path = line.substr(pos, next_pos - pos);
        pos = next_pos + 1;

        // AddedDate
        game.addedDate = line.substr(pos); // 现在直接从 Path 后面开始解析 AddedDate

        return game;
    }
};

// --- 游戏管理器类 ---
class GameManager {
private:
    std::filesystem::path _filePath;
    std::vector<Game> _games;
    std::mutex _mutex; // 用于保护 _games 向量和文件操作的互斥锁

    // 私有方法：加载所有游戏
    void LoadGames() {
        _games.clear();
        if (std::filesystem::exists(_filePath)) {
            // 使用 std::wifstream 和 codecvt_utf8_utf16 确保正确处理 UTF-8 编码的宽字符文件
            std::wifstream inputFile(_filePath, std::ios::in);
            inputFile.imbue(std::locale(inputFile.getloc(), new std::codecvt_utf8_utf16<wchar_t>));
            std::wstring line;
            while (std::getline(inputFile, line)) {
                if (!line.empty()) {
                    try {
                        _games.push_back(Game::FromTextLine(line));
                    } catch (const std::exception& ex) {
                        std::cerr << "错误：解析游戏数据行失败: '" << wstringToString(line) << "' - " << ex.what() << std::endl;
                    }
                }
            }
            inputFile.close();
        }
        std::cout << "已从文件加载 " << _games.size() << " 个游戏。" << std::endl;
    }

    // 私有方法：保存所有游戏
    void SaveGames() {
        // 使用 std::wofstream 和 codecvt_utf8_utf16 确保正确处理 UTF-8 编码的宽字符文件
        std::wofstream outputFile(_filePath, std::ios::out | std::ios::trunc); // 覆盖写入
        outputFile.imbue(std::locale(outputFile.getloc(), new std::codecvt_utf8_utf16<wchar_t>));
        if (outputFile.is_open()) {
            for (const auto& game : _games) {
                outputFile << game.ToTextLine() << std::endl;
            }
            outputFile.close();
            std::cout << "已将 " << _games.size() << " 个游戏保存到文件。" << std::endl;
        } else {
            std::cerr << "错误：无法打开文件进行写入: " << _filePath.string() << std::endl;
        }
    }

public:
    // 将默认文件名改为 "gamesdata.ini"
    GameManager(const std::filesystem::path& exeDirectory, const std::string& fileName = "gamesdata.ini") {
        _filePath = exeDirectory / stringToWstring(fileName);
        std::cout << "游戏数据文件路径: " << _filePath.string() << std::endl;

        // 确保文件存在，如果不存在则创建空文件
        if (!std::filesystem::exists(_filePath)) {
            std::wofstream createFile(_filePath);
            createFile.close();
        }
        LoadGames(); // 初始化时加载现有数据
    }

    // 添加新游戏
    void AddGame(Game& newGame) {
        std::lock_guard<std::mutex> lock(_mutex); // 锁定
        int nextId = 1;
        if (!_games.empty()) {
            // 查找当前最大ID并加1
            nextId = (*std::max_element(_games.begin(), _games.end(), 
                                       [](const Game& a, const Game& b) { return a.id < b.id; })).id + 1;
        }
        newGame.id = nextId;
        
        // 获取当前UTC时间并格式化为 ISO 8601 字符串
        SYSTEMTIME st;
        GetSystemTime(&st); // 获取UTC时间
        char buffer[256];
        sprintf_s(buffer, sizeof(buffer), "%04d-%02d-%02dT%02d:%02d:%02dZ", 
                  st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond);
        newGame.addedDate = stringToWstring(buffer);

        _games.push_back(newGame);
        SaveGames();
        std::cout << "游戏 '" << wstringToString(newGame.name) << "' (ID: " << newGame.id << ") 已添加。" << std::endl;
    }

    // 更新游戏
    void UpdateGame(const Game& updatedGame) {
        std::lock_guard<std::mutex> lock(_mutex); // 锁定
        auto it = std::find_if(_games.begin(), _games.end(), 
                               [&](const Game& g) { return g.id == updatedGame.id; });
        if (it != _games.end()) {
            it->name = updatedGame.name;
            it->path = updatedGame.path;
            // addedDate 通常不更新
            SaveGames();
            std::cout << "游戏 '" << wstringToString(updatedGame.name) << "' (ID: " << updatedGame.id << ") 已更新。" << std::endl;
        } else {
            std::cerr << "错误：无法更新游戏。未找到 ID 为 " << updatedGame.id << " 的游戏。" << std::endl;
        }
    }

    // 删除游戏
    void DeleteGame(int id) {
        std::lock_guard<std::mutex> lock(_mutex); // 锁定
        auto initialSize = _games.size();
        _games.erase(std::remove_if(_games.begin(), _games.end(), 
                                    [&](const Game& g) { return g.id == id; }),
                     _games.end());
        if (_games.size() < initialSize) {
            SaveGames();
            std::cout << "游戏 (ID: " << id << ") 已删除。" << std::endl;
        } else {
            std::cerr << "错误：无法删除游戏。未找到 ID 为 " << id << " 的游戏。" << std::endl;
        }
    }

    // 获取所有游戏 (返回副本以避免外部直接修改)
    std::vector<Game> GetAllGames() {
        std::lock_guard<std::mutex> lock(_mutex); // 锁定
        return _games;
    }
};

// 全局游戏管理器实例
GameManager* g_gameManager = nullptr;

// --- IPC 命令处理函数 ---

// 解压命令处理函数
void HandleUnzipCommand(const std::wstring& targetPath) {
    std::cout << "收到 UNZIP 命令，目标路径: \"" << wstringToString(targetPath) << "\"" << std::endl;

    wchar_t path_temp[MAX_PATH];
    GetModuleFileNameW(NULL, path_temp, MAX_PATH);
    std::filesystem::path currentExeDirectory(path_temp);
    currentExeDirectory = currentExeDirectory.parent_path();

    std::filesystem::path minizipExePath = currentExeDirectory / L"minizip.exe";
    std::filesystem::path zipFilePath = currentExeDirectory / L"ATC4ALL.zip";

    std::cout << "minizip.exe 路径设置为: " << minizipExePath.string() << std::endl;
    std::cout << "ZIP 文件路径设置为: " << zipFilePath.string() << std::endl;

    if (!std::filesystem::exists(minizipExePath)) {
        std::cerr << "错误：minizip.exe 不存在于路径: " << minizipExePath.string() << std::endl;
        return;
    }
    if (!std::filesystem::exists(zipFilePath)) {
        std::cerr << "错误：ATC4ALL.zip 不存在于路径: " << zipFilePath.string() << std::endl;
        return;
    }

    std::wstring quotedTargetPath = targetPath;
    if (quotedTargetPath.find(L' ') != std::wstring::npos && quotedTargetPath.front() != L'\"' && quotedTargetPath.back() != L'\"') {
        quotedTargetPath = L"\"" + quotedTargetPath + L"\"";
        std::cout << "目标路径包含空格，已添加引号。引用路径: \"" << wstringToString(quotedTargetPath) << "\"" << std::endl;
    } else {
        std::cout << "目标路径不包含空格或已加引号。路径: \"" << wstringToString(quotedTargetPath) << "\"" << std::endl;
    }

    std::filesystem::path targetDirPath(targetPath);
    if (targetDirPath.string().front() == '\"' && targetDirPath.string().back() == '\"') {
        targetDirPath = targetDirPath.string().substr(1, targetDirPath.string().length() - 2);
    }
    
    if (!std::filesystem::exists(targetDirPath)) {
        std::cout << "目标目录不存在，尝试创建: " << targetDirPath.string() << std::endl;
        std::error_code ec;
        if (std::filesystem::create_directories(targetDirPath, ec)) {
            std::cout << "目标目录创建成功。" << std::endl;
        } else {
            std::cerr << "错误：无法创建目标目录 " << targetDirPath.string() << "，错误信息: " << ec.message() << std::endl;
            return;
        }
    } else if (!std::filesystem::is_directory(targetDirPath)) {
        std::cerr << "错误：目标路径 " << targetDirPath.string() << " 存在但不是一个目录。" << std::endl;
        return;
    }

    std::wstring commandLine =
        minizipExePath.wstring() + L" " +
        L"-x" + L" " +
        zipFilePath.wstring() + L" " +
        L"-d" + L" " +
        quotedTargetPath;
    
    std::cout << "构建的完整命令行字符串（用于 CreateProcessW）:" << std::endl;
    std::cout << wstringToString(commandLine) << std::endl;

    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi;

    HANDLE hNul = CreateFileW(L"NUL", GENERIC_WRITE | GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    if (hNul == INVALID_HANDLE_VALUE) {
        std::cerr << "错误：无法打开 NUL 设备句柄，错误码: " << GetLastError() << std::endl;
        si.dwFlags = STARTF_USESHOWWINDOW;
        si.wShowWindow = SW_HIDE;
        std::cout << "回退到 STARTF_USESHOWWINDOW/SW_HIDE 模式。" << std::endl;
    } else {
        si.dwFlags = STARTF_USESTDHANDLES;
        si.hStdInput = hNul;
        si.hStdOutput = hNul;
        si.hStdError = hNul;
        std::cout << "已配置标准输入、输出和错误重定向到 NUL。" << std::endl;
    }

    std::cout << "尝试启动解压进程..." << std::endl;
    BOOL success = CreateProcessW(
        NULL,
        &commandLine[0],
        NULL,
        NULL,
        TRUE,
        CREATE_NO_WINDOW,
        NULL,
        NULL,
        &si,
        &pi
    );

    if (success) {
        std::cout << "成功启动解压进程！进程ID: " << pi.dwProcessId << std::endl;
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    } else {
        std::cerr << "错误：启动解压进程失败，错误码: " << GetLastError() << std::endl;
        wchar_t currentDir[MAX_PATH];
        GetCurrentDirectoryW(MAX_PATH, currentDir);
        std::wcout << L"当前工作目录: " << currentDir << std::endl;
    }

    if (hNul != INVALID_HANDLE_VALUE) {
        CloseHandle(hNul);
        std::cout << "NUL 句柄已关闭。" << std::endl;
    }
}

// ADD_GAME 命令处理函数
// 参数格式: <Name>|<Path>
void HandleAddGameCommand(const std::wstring& param) {
    if (!g_gameManager) {
        std::cerr << "错误：游戏管理器未初始化。" << std::endl;
        return;
    }

    std::wcout << L"收到 ADD_GAME 命令，参数: \"" << param << L"\"" << std::endl;
    try {
        Game newGame;
        size_t pos = 0;
        size_t next_pos;

        // Name
        next_pos = param.find(L'|', pos);
        // 如果没有找到第二个 '|'，说明只有 Name 和 Path
        if (next_pos == std::wstring::npos) { 
            std::cerr << "错误：ADD_GAME 格式不正确，缺少路径。期望格式: <Name>|<Path>" << std::endl;
            return; // 提前返回，避免后续错误
        }
        newGame.name = param.substr(pos, next_pos - pos);
        pos = next_pos + 1;

        // Path
        newGame.path = param.substr(pos); // Path 是剩余部分

        g_gameManager->AddGame(newGame);
    } catch (const std::exception& ex) {
        std::cerr << "错误：处理 ADD_GAME 命令失败: " << ex.what() << std::endl;
    }
}

// GET_GAMES 命令处理函数
// 目前只将游戏列表打印到服务端日志
void HandleGetGamesCommand(const std::wstring& param) {
    if (!g_gameManager) {
        std::cerr << "错误：游戏管理器未初始化。" << std::endl;
        return;
    }
    std::cout << "收到 GET_GAMES 命令。" << std::endl;
    auto games = g_gameManager->GetAllGames();
    if (games.empty()) {
        std::cout << "当前没有游戏记录。" << std::endl;
    } else {
        std::cout << "当前游戏列表：" << std::endl;
        for (const auto& game : games) {
            std::wcout << L"  ID: " << game.id 
                       << L", 名称: \"" << game.name 
                       << L"\", 路径: \"" << game.path 
                       << L"\", 添加日期: " << game.addedDate << std::endl;
        }
    }
    // TODO: 如果需要将游戏列表返回给客户端，需要实现双向IPC
}

// UPDATE_GAME 命令处理函数
// 参数格式: <Id>|<Name>|<Path>
void HandleUpdateGameCommand(const std::wstring& param) {
    if (!g_gameManager) {
        std::cerr << "错误：游戏管理器未初始化。" << std::endl;
        return;
    }
    std::wcout << L"收到 UPDATE_GAME 命令，参数: \"" << param << L"\"" << std::endl;
    try {
        Game updatedGame;
        size_t pos = 0;
        size_t next_pos;

        // Id
        next_pos = param.find(L'|', pos);
        if (next_pos == std::wstring::npos) throw std::runtime_error("Invalid UPDATE_GAME format: missing Name.");
        updatedGame.id = std::stoi(param.substr(pos, next_pos - pos));
        pos = next_pos + 1;

        // Name
        next_pos = param.find(L'|', pos);
        // 如果没有找到第二个 '|'，说明只有 Name 和 Path
        if (next_pos == std::wstring::npos) { 
            std::cerr << "错误：UPDATE_GAME 格式不正确，缺少路径。期望格式: <Id>|<Name>|<Path>" << std::endl;
            return; // 提前返回，避免后续错误
        }
        updatedGame.name = param.substr(pos, next_pos - pos);
        pos = next_pos + 1;

        // Path
        updatedGame.path = param.substr(pos); // Path 是剩余部分

        g_gameManager->UpdateGame(updatedGame);
    } catch (const std::exception& ex) {
        std::cerr << "错误：处理 UPDATE_GAME 命令失败: " << ex.what() << std::endl;
    }
}

// DELETE_GAME 命令处理函数
// 参数格式: <Id>
void HandleDeleteGameCommand(const std::wstring& param) {
    if (!g_gameManager) {
        std::cerr << "错误：游戏管理器未初始化。" << std::endl;
        return;
    }
    std::wcout << L"收到 DELETE_GAME 命令，参数: \"" << param << L"\"" << std::endl;
    try {
        int idToDelete = std::stoi(param);
        g_gameManager->DeleteGame(idToDelete);
    } catch (const std::exception& ex) {
        std::cerr << "错误：处理 DELETE_GAME 命令失败: " << ex.what() << std::endl;
    }
}

int main() {
    atexit(CloseLogFileOnExit);

    wchar_t path_temp[MAX_PATH];
    GetModuleFileNameW(NULL, path_temp, MAX_PATH);
    std::filesystem::path exePath(path_temp);
    std::filesystem::path exeDirectory = exePath.parent_path();
    
    std::string logFileName = "pipe_server_log.txt";
    std::filesystem::path logFilePath = exeDirectory / logFileName;
    std::string logFilePathStr = logFilePath.string();

    RedirectConsoleOutputToFile(logFilePathStr);

    if (logFile.is_open() && logFile.tellp() > 0) {
        logFile << std::endl;
    }

    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    std::cout << "--- 程序启动 ---" << std::endl;
    std::cout << "日志文件路径: " << logFilePathStr << std::endl;

    // 初始化全局游戏管理器实例
    g_gameManager = new GameManager(exeDirectory);

    // 注册命令处理函数
    commandHandlers[L"UNZIP"] = HandleUnzipCommand;
    commandHandlers[L"ADD_GAME"] = HandleAddGameCommand;
    commandHandlers[L"GET_GAMES"] = HandleGetGamesCommand;
    commandHandlers[L"UPDATE_GAME"] = HandleUpdateGameCommand;
    commandHandlers[L"DELETE_GAME"] = HandleDeleteGameCommand;

    const char* pipeName = "\\\\.\\pipe\\ATC4Pipe";
    std::cout << "尝试创建命名管道: " << pipeName << std::endl;

    HANDLE hPipe = CreateNamedPipeA(
        pipeName,
        PIPE_ACCESS_INBOUND,
        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
        1,
        512, 512, // 增加缓冲区大小以适应更长的游戏数据
        0,
        NULL
    );

    if (hPipe == INVALID_HANDLE_VALUE) {
        std::cerr << "错误：创建管道失败，错误码: " << GetLastError() << std::endl;
        // 释放游戏管理器内存
        if (g_gameManager) {
            delete g_gameManager;
            g_gameManager = nullptr;
        }
        return 1;
    }
    std::cout << "管道创建成功。句柄: " << hPipe << std::endl;

    // 主循环，持续监听客户端连接
    while (true) {
        std::cout << "等待客户端连接..." << std::endl;
        BOOL connected = ConnectNamedPipe(hPipe, NULL) ? TRUE : (GetLastError() == ERROR_PIPE_CONNECTED);
        if (!connected) {
            std::cerr << "错误：客户端连接失败，错误码: " << GetLastError() << std::endl;
            // 尝试断开并重新连接，或者直接退出
            break; // 简单起见，这里选择退出循环
        }
        std::cout << "客户端已连接！" << std::endl;

        char buffer[512] = {0}; // 增加缓冲区大小以适应更长的游戏数据
        DWORD bytesRead = 0;

        std::cout << "尝试从管道读取数据..." << std::endl;
        if (ReadFile(hPipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL)) { // 留一个字节给null终止符
            std::string fullMessage(buffer, bytesRead);
            std::cout << "成功从管道读取到 " << bytesRead << " 字节。" << std::endl;
            std::cout << "收到完整消息: \"" << fullMessage << "\"" << std::endl;

            size_t firstSpace = fullMessage.find(' ');
            std::string commandStr;
            std::string paramStr;

            if (firstSpace != std::string::npos) {
                commandStr = fullMessage.substr(0, firstSpace);
                paramStr = fullMessage.substr(firstSpace + 1);
            } else {
                commandStr = fullMessage;
                paramStr = "";
            }

            std::wstring commandWstr = stringToWstring(commandStr);
            std::wstring paramWstr = stringToWstring(paramStr);

            std::cout << "解析出命令: \"" << commandStr << "\"" << std::endl;
            // ⭐️ 修复：将 std::wstring 转换为 std::string 再输出到 std::cout
            std::cout << "解析出参数: \"" << wstringToString(paramWstr) << "\"" << std::endl; 

            auto it = commandHandlers.find(commandWstr);
            if (it != commandHandlers.end()) {
                it->second(paramWstr);
            } else {
                std::cerr << "错误：收到未知命令: \"" << commandStr << "\"" << std::endl;
            }
        } else {
            std::cerr << "错误：从管道读取失败，错误码: " << GetLastError() << std::endl;
        }

        // 断开连接，准备接受下一个客户端
        DisconnectNamedPipe(hPipe);
        std::cout << "客户端已断开连接。" << std::endl;
    }

    CloseHandle(hPipe);
    std::cout << "管道已关闭，程序退出。" << std::endl;
    
    // 释放游戏管理器内存
    if (g_gameManager) {
        delete g_gameManager;
        g_gameManager = nullptr;
    }

    return 0;
}