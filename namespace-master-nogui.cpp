#include <conio.h>
#include <iostream>
#include <windows.h>
#include <string>
#include <fstream>
#include <shlobj.h>
#include <tchar.h>
#include <shellapi.h>
#include <vector>
#include <time.h>
namespace master {
    void RestartAsAdmin() {
		char path[MAX_PATH];
		GetModuleFileNameA(NULL, path, MAX_PATH);
	
		SHELLEXECUTEINFOA sei = { sizeof(SHELLEXECUTEINFOA) };
		sei.lpVerb = "runas"; // 请求管理员权限
		sei.lpFile = path;    // 当前程序路径
		sei.nShow = SW_NORMAL; // 窗口显示方式
	
		if (!ShellExecuteExA(&sei)) {
			DWORD error = GetLastError(); // 获取错误代码
			if (error == ERROR_CANCELLED) { // 用户取消了管理员权限请求
				std::cerr << "用户取消了管理员权限请求。" << std::endl;
			} else {
				std::cerr << "无法请求管理员权限，错误代码: " << error << std::endl;
			}
		}
	}
    std::string xorEncrypt(const std::string& data, char key) {
    std::string result = data;
    for (auto& c : result) {
        c ^= key;
    }
    return result;
	}
	std::string fileName = "ATC4-HQ.ini"; // 文件名
    bool FileExistsInCurrentDirectory(const std::string& fileName) {
		WIN32_FIND_DATA findFileData;
		HANDLE hFind = FindFirstFile(fileName.c_str(), &findFileData);
		if (hFind == INVALID_HANDLE_VALUE) {
			// 文件未找到
			return false;
		} else {
			// 文件找到，关闭句柄
			FindClose(hFind);
			return true;
		}
	}
    void overwriteSecondLine(const std::string& filePath, const std::string& newContent) {
		std::fstream file(filePath, std::ios::in | std::ios::out); // 打开文件进行读写
		if (!file) {
			std::cerr << "无法打开文件！" << std::endl;
			return;
		}
		// 定位到第二行的起始位置
		std::string line;
		std::getline(file, line); // 跳过第一行
		std::streampos secondLinePos = file.tellg(); // 获取第二行的起始位置
		// 写入新的内容到第二行
		file.seekp(secondLinePos); // 定位到第二行
		file << newContent; // 写入新的内容
		file.close();
	}
    char banbenhao [20] = "1.4.2.1.0";//版本号
    void updateSecondLineInFile(const std::string& filePath, const std::string& newContent , int hang) {
		// 读取文件内容到内存
		std::ifstream inputFile(filePath);
		if (!inputFile) {
			std::cerr << "无法打开文件！" << std::endl;
			return;
		}
		std::vector<std::string> lines;
		std::string line;
		while (std::getline(inputFile, line)) {
			lines.push_back(line);
		}
		inputFile.close();
		// 修改第二行内容
		if (lines.size() >= hang) {
			lines[1] = newContent; // 更新第(=hang)行
		} else {
			// 如果文件少于两行，填充空行到第(=hang)行
			while (lines.size() < hang) {
				lines.push_back("");
			}
			lines[1] = newContent;
		}
		// 写回文件
		std::ofstream outputFile(filePath, std::ios::trunc);
		if (!outputFile) {
			std::cerr << "无法写入文件！" << std::endl;
			return;
		}
		for (const auto& l : lines) {
			outputFile << l << std::endl;
		}
		outputFile.close();
	}
    void qidong (std::wstring command) {
        // std::wstring command = L"Compatibility-mod.exe -114514";
		STARTUPINFOW si = { sizeof(si) };
		PROCESS_INFORMATION pi;
		CreateProcessW(
			NULL,                   // 应用程序名称
			&command[0],            // 命令行
			NULL,                   // 进程安全属性
			NULL,                   // 线程安全属性
			FALSE,                  // 是否继承句柄
			0,                      // 创建标志
			NULL,                   // 环境变量
			NULL,                   // 当前目录
			&si,                    // 启动信息
			&pi                     // 进程信息
		);
	}
}