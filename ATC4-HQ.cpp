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
int main() {
	using namespace master;
    // if (!IsUserAnAdmin()) { // ����Ƿ��Թ���Ա��������
	// 	int result = MessageBox(
	// 		NULL,                           // �����ھ����NULL ��ʾû�и����ڣ�
	// 		"ATC4-HQ��Ҫ����ԱȨ�޲ſ�������ʹ�ã����������ʹ�ù���ԱȨ��������������رճ���",           // ��������
	// 		"��Ҫ����ԱȨ�����У���",                     // ��������
	// 		MB_YESNO | MB_ICONINFORMATION      // ������ʽ���Ƿ�ť + ��Ϣͼ�꣩
	// 	);
	// 	if (result == IDYES) {
	// 		master::RestartAsAdmin(); // �Թ���Ա����������������
	// 		return 0; // �˳���ǰ����
	// 	} else if (result == IDNO) {
	// 		return 0; // �û�ѡ���������˳�����
	// 	}
	// }
	time_t now = time(0); // ��ȡ��ǰʱ���
    tm* localtm = localtime(&now); // ת��Ϊ����ʱ��ṹ��
    char buf[64];
    strftime(buf, sizeof(buf), "%Y-%m-%d %H:%M:%S", localtm); // ��ʽ��ʱ��
	std::string timeStr(buf); // ��ʱ��ת��Ϊ�ַ���
	std::string selectedPath = buf ;
	std::string encryptedPath = xorEncrypt(selectedPath, 0x5A); // 0x5A����Կ
	//���������ļ�
	if (FileExistsInCurrentDirectory(fileName) == false) { // ����ļ��Ƿ����
		std::ofstream outFile;
		outFile.open("ATC4-HQ.ini"); // ���´��ļ�
		if (!outFile) {
			std::cerr << "�ļ�����ʧ�ܣ�" << std::endl;
			return false;
		}
		outFile << "LE �� {" << std::endl  //1
		<< std::endl;					   //2
		outFile << "}" << std::endl;       //3
		outFile << "�汾 {" << std::endl;  //4
		outFile << banbenhao << std::endl; //5
		outFile << "}" << std::endl        //6
		<< "����� run {" << std::endl     //7
		<< encryptedPath << std::endl <<   //8
		"}"<< std::endl ;                  //9
		outFile.close();
	} else {
		std::ifstream inputFile("ATC4-HQ.ini"); // ���ļ�
		if (!inputFile) {
			std::cerr << "�ļ���ʧ�ܣ�" << std::endl;
			return false; // �ļ���ʧ��
		}
		std::string line;
		for (
			auto x = 1;
			x <= 5;
			x = x + 1
		) {
			std::getline(inputFile,line);
			if (! (line == banbenhao)) { //��������ļ��汾
				std::vector<std::string> lines;
				std::string line;
				std::ifstream inputFile("ATC4-HQ.ini");
				while (std::getline(inputFile, line)) {
					lines.push_back(line); // ÿ��һ�оͼӵ�����ĩβ
				}
				// �޸ĵ�2��
				inputFile.close();
				lines[4 /*������ǰ��������ĵ�����*/] = banbenhao; // ���µڶ�������";
				// д��
				std::ofstream outputFile("ATC4-HQ.ini", std::ios::trunc);
				for (const auto& l : lines) outputFile << l << std::endl;
				outputFile.close();
			}
		}
	}
	std::cout << "a" << std::endl;
	if (a < 540) {
		qidong(L"Compatibility-mod.exe 114514");
		return 0; 
	} else if (b < 540) {
		b = 540; // ���bС��540����b����Ϊ540
	}
	initgraph(b,b ); //��ʼ��ͼ�δ���
	//byd�´δ��������ɾ��������һ�еġ�EX_SHOWCONSOLE��
	SetConsoleOutputCP(936); //���ÿ���̨�������ΪGBK
	chongzhipingmu(); //����
	// ��ťλ�úʹ�С
    int btnX =   qidongyouxianniudeX , btnY =  qidongyouxianniudeY , btnWidth = 100, btnHeight = 50;
	int btnX1 =   qidongqitajichangdeX , btnY1 =  qidongqitajichangdeY + 100 , btnWidth1 = 100, btnHeight1 = 50;
	//���ư汾�������Ͻ�
	settextstyle(20, 0, (ziti));
	outtextxy(10, 10, (banbenhao));
	// ���ư�ť A
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, (ziti));
    outtextxy(btnX + 10, btnY + 15, _T("������Ϸ"));
	// ���ư�ť B
    setfillcolor(LIGHTGRAY);
	btnWidth1 = 150;
    solidrectangle(btnX1, btnY, btnX1 + btnWidth1, btnY + btnHeight1);
    settextstyle(20, 0, (ziti));
    outtextxy(btnX1 + 10, btnY + 15, _T("��װATC4"));
	while (true) {
		// ��������
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (c(msg.x, msg.y, btnX1, btnY, btnWidth, btnHeight)) {
					chongzhipingmu(); //����
					// a���ư�ť
					setfillcolor(LIGHTGRAY);
					solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
					settextstyle(20, 0, (ziti));
					outtextxy(btnX + 10, btnY + 15, _T(""));
				}
			}
		}	
		// ��������
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (c(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
					// ��ť�����
					chongzhipingmu(); //����
					sb :
					// a���ư�ť
					setfillcolor(LIGHTGRAY);
					solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
					settextstyle(20, 0, (ziti));
					outtextxy(btnX + 10, btnY + 15, _T("����RJOO"));
					// b���ư�ť
					setfillcolor(LIGHTGRAY);
					btnWidth1 = 150;
					solidrectangle(btnX1, btnY, btnX1 + btnWidth1, btnY + btnHeight1);
					settextstyle(20, 0, (ziti));
					outtextxy(btnX1 + 10, btnY + 15, _T("������������"));
					while (true) {
						// ��������
						if (MouseHit()) {
							MOUSEMSG msg = GetMouseMsg();
							if (msg.uMsg == WM_LBUTTONDOWN) {
								if (c(msg.x, msg.y, btnX1, btnY, btnWidth, btnHeight)) {
									// b��ť�����
									system("copy .\\�ļ�\\RJAA.dll .\\ATC4\\XPACK.dll"); // �����ļ�
									// �ļ����Ƴɹ�
									std::ifstream inputFile("ATC4-HQ.ini"); // ���ļ�);
									if (!inputFile) {
										std::cerr << "�ļ���ʧ�ܣ�" << std::endl;
										return false; // �ļ���ʧ��
									}
									// ��ȡ�ļ�����
									std::string line;
									// ��ȡ��һ�У�������
									if (std::getline(inputFile, line)) {
										// ��ȡ�ڶ���
										if (std::getline(inputFile, line) && !line.empty()) {
											printf("�еڶ���\n");
											//�еڶ���
											std::wstring LEdizhi(line.begin(), line.end()); // ���ڶ���ת��Ϊwstring
											std::wstring command = LEdizhi + L"\\LEProc.exe" + L" " + L"-run .\\ATC4\\AXA.exe";
											STARTUPINFOW si = { sizeof(si) };
											PROCESS_INFORMATION pi;
											CreateProcessW(
												NULL,                   // Ӧ�ó�������
												&command[0],            // ������
												NULL,                   // ���̰�ȫ����
												NULL,                   // �̰߳�ȫ����
												FALSE,                  // �Ƿ�̳о��
												0,                      // ������־
												NULL,                   // ��������
												NULL,                   // ��ǰĿ¼
												&si,                    // ������Ϣ
												&pi                     // ������Ϣ
											);
										} else {
											printf("û�еڶ���\n");
											//û�еڶ���
											TCHAR szBuffer[MAX_PATH] = {0};
											BROWSEINFO bi = { 0 }; // ��ʼ��BROWSEINFO�ṹ
											bi.lpszTitle = _T("��ѡ��һ���ļ���:"); // ���öԻ������
											bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE; // ���öԻ�����ʽ
											LPITEMIDLIST idl = SHBrowseForFolder(&bi); // ��ʾѡ���ļ��жԻ���
											if (idl == NULL) {
												std::cerr << "δѡ���ļ��У�" << std::endl;
												return false;
											}
											SHGetPathFromIDList(idl, szBuffer); // ��ȡѡ����ļ���·��
											// ת��Ϊ std::string
											std::string selectedPath;
											#ifdef UNICODE
											std::wstring ws(szBuffer); // ת��Ϊ���ַ���
											selectedPath = std::string(ws.begin(), ws.end()); // ת��Ϊ���ֽ��ַ���
											#else
											selectedPath = std::string(szBuffer); // ֱ��ת��Ϊ�ַ���
											#endif
											updateSecondLineInFile("ATC4-HQ.ini", szBuffer , 2); // ���µڶ�������
											chongzhipingmu(); //����
											settextstyle(100 , 0 , (ziti));
											chongzhipingmu(); //����
											goto sb ; //��ת��sb��ǩ
										} 
										inputFile.close(); // �ر��ļ�
									}
									break;
								}
							}
						}
						if (MouseHit()) {
							MOUSEMSG msg = GetMouseMsg();
							if (msg.uMsg == WM_LBUTTONDOWN) {
								if (sa(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
									// a��ť�����
									system("copy .\\�ļ�\\RJOO.dll .\\ATC4\\XPACK.dll"); // �����ļ�
									// �ļ����Ƴɹ�
									std::ifstream inputFile("ATC4-HQ.ini"); // ���ļ�);
									if (!inputFile) {
										std::cerr << "�ļ���ʧ�ܣ�" << std::endl;
										return false; // �ļ���ʧ��
									}
									// ��ȡ�ļ�����
									std::string line;
									// ��ȡ��һ�У�������
									if (std::getline(inputFile, line)) {
										// ��ȡ�ڶ���
										if (std::getline(inputFile, line) && !line.empty()) {
											//�еڶ���
											std::wstring LEdizhi(line.begin(), line.end()); // ���ڶ���ת��Ϊwstring
											std::wstring command = LEdizhi + L"\\LEProc.exe" + L" " + L"-run .\\ATC4\\AXA.exe";
											STARTUPINFOW si = { sizeof(si) };
											PROCESS_INFORMATION pi;
											CreateProcessW(
												NULL,                   // Ӧ�ó�������
												&command[0],            // ������
												NULL,                   // ���̰�ȫ����
												NULL,                   // �̰߳�ȫ����
												FALSE,                  // �Ƿ�̳о��
												0,                      // ������־
												NULL,                   // ��������
												NULL,                   // ��ǰĿ¼
												&si,                    // ������Ϣ
												&pi                     // ������Ϣ
											);
										} else {
											printf("û�еڶ���\n");
											//û�еڶ���
											TCHAR szBuffer[MAX_PATH] = {0};
											BROWSEINFO bi = { 0 }; // ��ʼ��BROWSEINFO�ṹ
											bi.lpszTitle = _T("��ѡ��һ���ļ���:"); // ���öԻ������
											bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE; // ���öԻ�����ʽ
											LPITEMIDLIST idl = SHBrowseForFolder(&bi); // ��ʾѡ���ļ��жԻ���
											if (idl == NULL) {
												std::cerr << "δѡ���ļ��У�" << std::endl;
												return false;
											}
											SHGetPathFromIDList(idl, szBuffer); // ��ȡѡ����ļ���·��
											// ת��Ϊ std::string
											std::string selectedPath;
											#ifdef UNICODE
											std::wstring ws(szBuffer); // ת��Ϊ���ַ���
											selectedPath = std::string(ws.begin(), ws.end()); // ת��Ϊ���ֽ��ַ���
											#else
											selectedPath = std::string(szBuffer); // ֱ��ת��Ϊ�ַ���
											#endif				
											updateSecondLineInFile("ATC4-HQ.ini", szBuffer , 2); // ���µڶ�������
											chongzhipingmu(); //����
											settextstyle(100 , 0 , (ziti));
											chongzhipingmu(); //����
											goto sb ; //��ת��sb��ǩ
										} 
										inputFile.close(); // �ر��ļ�
									}
									break;
								}
							}
						}
						break;
					}
				}
			}
		}
	}
	closegraph();
	return 0;
}