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
    char banbenhao [20] = "pre-ahpha 1.5.0.0.0";//版本号
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
int main(int argc , char* argv[]) {
	using namespace master;
	SetConsoleCP(CP_UTF8);         // 设置输入为UTF-8
	SetConsoleOutputCP(CP_UTF8);   // 设置输出为UTF-8
	if (argc < 2) {
        std::cout 
		<< "请选择功能" << std::endl
		<< "生成配置文件 -new-config" << std::endl
		<< "启动游戏 -run" << std::endl
		<< "配置参数 -set-config" << std::endl;
		// << "以管理员权限运行" << std::endl
        return 1;
    }
	if (argv[1] == "-new-config") {
		time_t now = time(0); // 获取当前时间戳
		tm* localtm = localtime(&now); // 转换为本地时间结构体
		char buf[64];
		strftime(buf, sizeof(buf), "%Y-%m-%d %H:%M:%S", localtm); // 格式化时间
		std::string timeStr(buf); // 将时间转换为字符串
		std::string selectedPath = buf ;
		std::string encryptedPath = xorEncrypt(selectedPath, 0x5A); // 0x5A是密钥
		//创建配置文件
		if (FileExistsInCurrentDirectory(fileName) == false) { // 检查文件是否存在
			std::ofstream outFile;
			outFile.open("ATC4-HQ.ini"); // 重新打开文件
			if (!outFile) {
				std::cerr << "文件创建失败！" << std::endl;
				return false;
			}
			outFile << "LE 在 {" << std::endl  //1
			<< std::endl;					   //2
			outFile << "}" << std::endl;       //3
			outFile << "版本 {" << std::endl;  //4
			outFile << banbenhao << std::endl; //5
			outFile << "}" << std::endl        //6
			<< "初めて run {" << std::endl     //7
			<< encryptedPath << std::endl <<   //8
			"}"<< std::endl ;                  //9
			outFile.close();
		} 
	} else if (argv[1] == "-run") {
		if (argc < 3) {
			
		} else if (argc == 2) {
			std::cout << "记得在-run 后面加 -<游戏>" << std::endl;
			return 1;
		}
	} else {
		std::cout << "写错了！" << std::endl;
	}
    // if (!IsUserAnAdmin()) { // 检查是否以管理员身份运行
	// 	int result = MessageBox(
	// 		NULL,                           // 父窗口句柄（NULL 表示没有父窗口）
	// 		"ATC4-HQ需要管理员权限才可以正常使用！！点击是以使用管理员权限重启，或点击否关闭程序",           // 弹窗内容
	// 		"需要管理员权限运行！！",                     // 弹窗标题
	// 		MB_YESNO | MB_ICONINFORMATION      // 弹窗样式（是否按钮 + 信息图标）
	// 	);
	// 	if (result == IDYES) {
	// 		master::RestartAsAdmin(); // 以管理员身份重新启动程序
	// 		return 0; // 退出当前程序
	// 	} else if (result == IDNO) {
	// 		return 0; // 用户选择不重启，退出程序
	// 	}
	// }

	/*
	time_t now = time(0); // 获取当前时间戳
    tm* localtm = localtime(&now); // 转换为本地时间结构体
    char buf[64];
    strftime(buf, sizeof(buf), "%Y-%m-%d %H:%M:%S", localtm); // 格式化时间
	std::string timeStr(buf); // 将时间转换为字符串
	std::string selectedPath = buf ;
	std::string encryptedPath = xorEncrypt(selectedPath, 0x5A); // 0x5A是密钥
	//创建配置文件
	if (FileExistsInCurrentDirectory(fileName) == false) { // 检查文件是否存在
		std::ofstream outFile;
		outFile.open("ATC4-HQ.ini"); // 重新打开文件
		if (!outFile) {
			std::cerr << "文件创建失败！" << std::endl;
			return false;
		}
		outFile << "LE 在 {" << std::endl  //1
		<< std::endl;					   //2
		outFile << "}" << std::endl;       //3
		outFile << "版本 {" << std::endl;  //4
		outFile << banbenhao << std::endl; //5
		outFile << "}" << std::endl        //6
		<< "初めて run {" << std::endl     //7
		<< encryptedPath << std::endl <<   //8
		"}"<< std::endl ;                  //9
		outFile.close();
	} else {
		std::ifstream inputFile("ATC4-HQ.ini"); // 打开文件
		if (!inputFile) {
			std::cerr << "文件打开失败！" << std::endl;
			return false; // 文件打开失败
		}
		std::string line;
		for (
			auto x = 1;
			x <= 5;
			x = x + 1
		) {
			std::getline(inputFile,line);
			if (! (line == banbenhao)) { //检查配置文件版本
				std::vector<std::string> lines;
				std::string line;
				std::ifstream inputFile("ATC4-HQ.ini");
				while (std::getline(inputFile, line)) {
					lines.push_back(line); // 每读一行就加到数组末尾
				}
				// 修改第2行
				inputFile.close();
				lines[4 /*这里是前面的数更改的行数*] = banbenhao; // 更新第二行内容";
				// 写回
				std::ofstream outputFile("ATC4-HQ.ini", std::ios::trunc);
				for (const auto& l : lines) outputFile << l << std::endl;
				outputFile.close();
			}
		}
	}
	system("copy .\\文件\\RJAA.dll .\\ATC4\\XPACK.dll"); // 复制文件
	// 文件复制成功
	std::ifstream inputFile("ATC4-HQ.ini"); // 打开文件);
	if (!inputFile) {
		std::cerr << "文件打开失败！" << std::endl;
		return false; // 文件打开失败
	}
	// 读取文件内容
	std::string line;
	// 读取第一行（跳过）
	if (std::getline(inputFile, line)) {
		// 读取第二行
		if (std::getline(inputFile, line) && !line.empty()) {
			printf("有第二行\n");
			//有第二行
			std::wstring LEdizhi(line.begin(), line.end()); // 将第二行转换为wstring
			std::wstring command = LEdizhi + L"\\LEProc.exe" + L" " + L"-run .\\ATC4\\AXA.exe";
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
		} else {
			printf("没有第二行\n");
			//没有第二行
			TCHAR szBuffer[MAX_PATH] = {0};
			BROWSEINFO bi = { 0 }; // 初始化BROWSEINFO结构
			bi.lpszTitle = _T("请选择一个文件夹:"); // 设置对话框标题
			bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE; // 设置对话框样式
			LPITEMIDLIST idl = SHBrowseForFolder(&bi); // 显示选择文件夹对话框
			if (idl == NULL) {
				std::cerr << "未选择文件夹！" << std::endl;
				return false;
			}
			SHGetPathFromIDList(idl, szBuffer); // 获取选择的文件夹路径
			// 转换为 std::string
			std::string selectedPath;
			#ifdef UNICODE
			std::wstring ws(szBuffer); // 转换为宽字符串
			selectedPath = std::string(ws.begin(), ws.end()); // 转换为多字节字符串
			#else
			selectedPath = std::string(szBuffer); // 直接转换为字符串
			#endif
			updateSecondLineInFile("ATC4-HQ.ini", szBuffer , 2); // 更新第二行内容
			chongzhipingmu(); //清屏
			settextstyle(100 , 0 , (ziti));
			chongzhipingmu(); //清屏
			goto sb ; //跳转到sb标签
		} 
		inputFile.close(); // 关闭文件
	}
	break;
	}
	// a按钮被点击
	system("copy .\\文件\\RJOO.dll .\\ATC4\\XPACK.dll"); // 复制文件
	// 文件复制成功
	std::ifstream inputFile("ATC4-HQ.ini"); // 打开文件);
	if (!inputFile) {
		std::cerr << "文件打开失败！" << std::endl;
		return false; // 文件打开失败
	}
	// 读取文件内容
	std::string line;
	// 读取第一行（跳过）
	if (std::getline(inputFile, line)) {
		// 读取第二行
		if (std::getline(inputFile, line) && !line.empty()) {
			//有第二行
			std::wstring LEdizhi(line.begin(), line.end()); // 将第二行转换为wstring
			std::wstring command = LEdizhi + L"\\LEProc.exe" + L" " + L"-run .\\ATC4\\AXA.exe";
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
		} else {
			printf("没有第二行\n");
			//没有第二行
			TCHAR szBuffer[MAX_PATH] = {0};
			BROWSEINFO bi = { 0 }; // 初始化BROWSEINFO结构
			bi.lpszTitle = _T("请选择一个文件夹:"); // 设置对话框标题
			bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE; // 设置对话框样式
			LPITEMIDLIST idl = SHBrowseForFolder(&bi); // 显示选择文件夹对话框
			if (idl == NULL) {
				std::cerr << "未选择文件夹！" << std::endl;
				return false;
			}
			SHGetPathFromIDList(idl, szBuffer); // 获取选择的文件夹路径
			// 转换为 std::string
			std::string selectedPath;
			#ifdef UNICODE
			std::wstring ws(szBuffer); // 转换为宽字符串
			selectedPath = std::string(ws.begin(), ws.end()); // 转换为多字节字符串
			#else
			selectedPath = std::string(szBuffer); // 直接转换为字符串
			#endif				
			updateSecondLineInFile("ATC4-HQ.ini", szBuffer , 2); // 更新第二行内容
			chongzhipingmu(); //清屏
			settextstyle(100 , 0 , (ziti));
			chongzhipingmu(); //清屏
			goto sb ; //跳转到sb标签
		} 
		inputFile.close(); // 关闭文件
	}
	*/
	return 0;
}