#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
#include <string>
#include <fstream>
#include <shlobj.h>
#include <tchar.h>
#include <shellapi.h>
#include <vector>
namespace master {
	char ats [100] = {0};
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
	std::string fileName = "ATC4-HQ.ini"; // 文件名
	//const wchar_t* ziti = L"Arial"; //使用字体
	char ziti[] = "Arial"; //使用字体
	int a = GetSystemMetrics(SM_CXSCREEN); //这个变量int a是用户显示屏高
	int b = a /4; //这个变量int b是程序运行时的高和宽
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
	/*
	bool jianrongmoushi
	if (a  < 540) {
		bool jianrongmoushi = true; //如果应用宽度小于540，则设置为true
	} if else  (b > 540) {
		bool jianrongmoushi = false; //如果应用宽度大于540，则设置为false
		b = 540; //将应用高度设置为540
	} else {
		bool jianrongmoushi = false; //如果应用宽度大于540，则设置为false
	}
	*/
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
	// 检查鼠标是否在按钮区域内
    bool c (int x, int y, int btnX, int btnY, int btnWidth, int btnHeight) {
    return x >= btnX && x <= btnX + btnWidth && y >= btnY && y <= btnY + btnHeight;
	}
	bool sa (int x, int y, int btnX1, int btnY, int btnWidth, int btnHeight) {
		return x >= btnX1 && x <= btnX1 + btnWidth && y >= btnY && y <= btnY + btnHeight;
		}
	//如果鼠标在按钮区域内，返回布尔值c true，否则返回false
	int qidongyouxianniudeX = b / 3 - 50; //a和b按钮的X坐标
	int qidongyouxianniudeY = b / 3 * 2; //a和b按钮的Y坐标
	void clearButton(int x, int y, int width, int height) {
		setfillcolor(WHITE); // 设置填充颜色为白色
		solidrectangle(x, y, x + width, y + height); // 用白色填充按钮区域
	}
	void chongzhipingmu() {
		setfillcolor(WHITE); // 设置填充颜色为白色
		solidrectangle(0, 0, b, b); // 用白色填充整个屏幕
		IMAGE img; //定义一个图片对象
		const char * a91 = "ATC4.ico"; //图片路径
		loadimage(&img , a91 , b , b ,false); //加载图片
		putimage(0,0 , &img ); //在屏幕上显示图片
	}
	int qidongqitajichangdeX = b / 3 * 2 - 50; //a和b按钮的X坐标
	int qidongqitajichangdeY = b / 3 * 2; //a和b按钮的Y坐标
	char banbenhao [20] = "pre-ahpha 1.4.1.0.0";//版本号
	void updateSecondLineInFile(const std::string& filePath, const std::string& newContent) {
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
    if (lines.size() >= 2) {
        lines[1] = newContent; // 更新第二行
    } else {
        // 如果文件少于两行，填充空行到第二行
        while (lines.size() < 2) {
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
}
int main() {
	mainstart :
	using namespace master;
	b = 540 ;
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
	if (FileExistsInCurrentDirectory(fileName) == false) { // 检查文件是否存在
		std::ofstream outFile;
		outFile.open("ATC4-HQ.ini"); // 重新打开文件
		if (!outFile) {
			std::cerr << "文件创建失败！" << std::endl;
			return false;
		}
		outFile << "LE 在 {" << std::endl;
		if (ats [0] != '\0' ) {
			outFile << ats <<std::endl;
		} else {
			outFile << std::endl;
		}
		outFile << "}" << std::endl;
		outFile << "版本 {" << std::endl;
		outFile << banbenhao << std::endl;
		outFile << "}" << std::endl
		<< "初めて run {" << std::endl
		<< std::endl <<
		"}"<< std::endl ;
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
				strncpy(ats, line.c_str(), sizeof(ats) - 1); // 拷贝内容，防止越界
				inputFile.close();
				system("del ATC4-HQ.ini");
			}
		}
		goto mainstart;
	}
	initgraph(b,b ); //初始化图形窗口
	//byd下次打包别忘了删掉上面那一行的“EX_SHOWCONSOLE”
	SetConsoleOutputCP(936); //设置控制台输出编码为GBK
	chongzhipingmu(); //清屏
	// 按钮位置和大小
    int btnX =   qidongyouxianniudeX , btnY =  qidongyouxianniudeY , btnWidth = 100, btnHeight = 50;
	int btnX1 =   qidongqitajichangdeX , btnY1 =  qidongqitajichangdeY + 100 , btnWidth1 = 100, btnHeight1 = 50;
	//绘制版本号在左上角
	settextstyle(20, 0, (ziti));
	outtextxy(10, 10, (banbenhao));
	// 绘制按钮
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, (ziti));
    outtextxy(btnX + 10, btnY + 15, _T("启动游戏"));
	while (true)
	{
		// 检查鼠标点击
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (c(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
					// 按钮被点击
					chongzhipingmu(); //清屏
					break;
				}
			}
		}
	}
	sb :
	// a绘制按钮
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, (ziti));
    outtextxy(btnX + 10, btnY + 15, _T("启动RJOO"));
	// b绘制按钮
    setfillcolor(LIGHTGRAY);
	btnWidth1 = 150;
    solidrectangle(btnX1, btnY, btnX1 + btnWidth1, btnY + btnHeight1);
    settextstyle(20, 0, (ziti));
    outtextxy(btnX1 + 10, btnY + 15, _T("启动其他机场"));
	while (true) {
		// 检查鼠标点击
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (c(msg.x, msg.y, btnX1, btnY, btnWidth, btnHeight)) {
					// b按钮被点击
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
							updateSecondLineInFile("ATC4-HQ.ini", szBuffer); // 更新第二行内容
							chongzhipingmu(); //清屏
							settextstyle(100 , 0 , (ziti));
							chongzhipingmu(); //清屏
							goto sb ; //跳转到sb标签
						} 
						inputFile.close(); // 关闭文件
					}
					break;
				}
			}
		}
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (sa(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
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
							updateSecondLineInFile("ATC4-HQ.ini", szBuffer); // 更新第二行内容
							chongzhipingmu(); //清屏
							settextstyle(100 , 0 , (ziti));
							chongzhipingmu(); //清屏
							goto sb ; //跳转到sb标签
						} 
						inputFile.close(); // 关闭文件
					}
					break;
				}
			}
		}
	}
	closegraph();
	return 0;
}