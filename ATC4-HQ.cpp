#include "namespace-master-gui.cpp"
#include "namespace-master-nogui.cpp"
int main() {
	using namespace master;
    // if (!IsUserAnAdmin()) { // ����Ƿ��Թ���Ա�������
	// 	int result = MessageBox(
	// 		NULL,                           // �����ھ����NULL ��ʾû�и����ڣ�
	// 		"ATC4-HQ��Ҫ����ԱȨ�޲ſ�������ʹ�ã����������ʹ�ù���ԱȨ��������������رճ���",           // ��������
	// 		"��Ҫ����ԱȨ�����У���",                     // ��������
	// 		MB_YESNO | MB_ICONINFORMATION      // ������ʽ���Ƿ�ť + ��Ϣͼ�꣩
	// 	);
	// 	if (result == IDYES) {
	// 		master::RestartAsAdmin(); // �Թ���Ա���������������
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
				lines[4 /*������ǰ��������ĵ�����*/ /*- 1*/] = banbenhao; // ���µڶ�������";
				// д��
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
	// ���ư�ť
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, (ziti));
    outtextxy(btnX + 10, btnY + 15, _T("������Ϸ"));
	while (true)
	{
		// ��������
		if (MouseHit()) {
			MOUSEMSG msg = GetMouseMsg();
			if (msg.uMsg == WM_LBUTTONDOWN) {
				if (c(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
					// ��ť�����
					chongzhipingmu(); //����
					break;
				}
			}
		}
	}
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
	}
	closegraph();
	return 0;
}