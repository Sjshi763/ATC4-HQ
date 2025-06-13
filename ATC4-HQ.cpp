#include "namespace-master-gui.cpp"
#include "namespace-master-nogui.cpp"
int main() {
	using namespace master;
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
				lines[4 /*这里是前面的数更改的行数*/ /*- 1*/] = banbenhao; // 更新第二行内容";
				// 写回
				std::ofstream outputFile("ATC4-HQ.ini", std::ios::trunc);
				for (const auto& l : lines) outputFile << l << std::endl;
				outputFile.close();
			}
		}
	}
	std::cout << "a" << std::endl;
	if (b < 540) {
		qidong(L"Compatibility-mod.exe 114514");
		return 0; 
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
			}
		}
	}
	closegraph();
	return 0;
}