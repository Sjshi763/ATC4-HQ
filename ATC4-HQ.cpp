#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
#include <string>
#include <fstream>
#include <shlobj.h>
#include <tchar.h>
namespace master {
	std::string fileName = "ATC4-HQ.ini"; // �ļ���
	//const wchar_t* ziti = L"Arial"; //ʹ������
	char ziti[] = "Arial"; //ʹ������
	int a = GetSystemMetrics(SM_CXSCREEN); //�������int a���û���ʾ����
	int b = a /4; //�������int b�ǳ�������ʱ�ĸߺͿ�
	bool FileExistsInCurrentDirectory(const std::string& fileName) {
		WIN32_FIND_DATA findFileData;
		HANDLE hFind = FindFirstFile(fileName.c_str(), &findFileData);
		if (hFind == INVALID_HANDLE_VALUE) {
			// �ļ�δ�ҵ�
			return false;
		} else {
			// �ļ��ҵ����رվ��
			FindClose(hFind);
			return true;
		}
	}
	/*
	bool jianrongmoushi
	if (a  < 540) {
		bool jianrongmoushi = true; //���Ӧ�ÿ��С��540��������Ϊtrue
	} if else  (b > 540) {
		bool jianrongmoushi = false; //���Ӧ�ÿ�ȴ���540��������Ϊfalse
		b = 540; //��Ӧ�ø߶�����Ϊ540
	} else {
		bool jianrongmoushi = false; //���Ӧ�ÿ�ȴ���540��������Ϊfalse
	}
	*/
	void overwriteSecondLine(const std::string& filePath, const std::string& newContent) {
		std::fstream file(filePath, std::ios::in | std::ios::out); // ���ļ����ж�д
		if (!file) {
			std::cerr << "�޷����ļ���" << std::endl;
			return;
		}
		// ��λ���ڶ��е���ʼλ��
		std::string line;
		std::getline(file, line); // ������һ��
		std::streampos secondLinePos = file.tellg(); // ��ȡ�ڶ��е���ʼλ��
		// д���µ����ݵ��ڶ���
		file.seekp(secondLinePos); // ��λ���ڶ���
		file << newContent; // д���µ�����
		file.close();
	}
	// �������Ƿ��ڰ�ť������
    bool c (int x, int y, int btnX, int btnY, int btnWidth, int btnHeight) {
    return x >= btnX && x <= btnX + btnWidth && y >= btnY && y <= btnY + btnHeight;
	}
	bool sa (int x, int y, int btnX1, int btnY, int btnWidth, int btnHeight) {
		return x >= btnX1 && x <= btnX1 + btnWidth && y >= btnY && y <= btnY + btnHeight;
		}
	//�������ڰ�ť�����ڣ����ز���ֵc true�����򷵻�false
	int qidongyouxianniudeX = b / 3 - 50; //a��b��ť��X����
	int qidongyouxianniudeY = b / 3 * 2; //a��b��ť��Y����
	void clearButton(int x, int y, int width, int height) {
		setfillcolor(WHITE); // ���������ɫΪ��ɫ
		solidrectangle(x, y, x + width, y + height); // �ð�ɫ��䰴ť����
	}
	void chongzhipingmu() {
		setfillcolor(WHITE); // ���������ɫΪ��ɫ
		solidrectangle(0, 0, b, b); // �ð�ɫ���������Ļ
		IMAGE img; //����һ��ͼƬ����
		const char * a91 = "ATC4.ico"; //ͼƬ·��
		loadimage(&img , a91 , b , b ,false); //����ͼƬ
		putimage(0,0 , &img ); //����Ļ����ʾͼƬ
	}
	int qidongqitajichangdeX = b / 3 * 2 - 50; //a��b��ť��X����
	int qidongqitajichangdeY = b / 3 * 2; //a��b��ť��Y����
}
int main() {
    // if () {
	using namespace master;
	std::ofstream outFile("ATC4-HQ.ini", std::ios::app); // ���ļ���׷��ģʽ
	if (FileExistsInCurrentDirectory(fileName) == false) {
        std::ofstream outFile("ATC4-HQ.ini"); // �����ļ�
		if (!outFile) {
			return false; // �ļ�����ʧ��
		}
		outFile << "LE �� {" << std::endl;
		outFile << std::endl;
		outFile << "}" << std::endl;
		outFile << "�汾 {" << std::endl;
		outFile << "pre-Aplea 1.4.0.0.0" << std::endl;
		outFile << "}" << std::endl;
    }
	initgraph(b,b ,EX_SHOWCONSOLE); //��ʼ��ͼ�δ���
	SetConsoleOutputCP(936); //���ÿ���̨�������ΪGBK
	chongzhipingmu(); //����
	// ��ťλ�úʹ�С
    int btnX =   qidongyouxianniudeX , btnY =  qidongyouxianniudeY , btnWidth = 100, btnHeight = 50;
	int btnX1 =   qidongqitajichangdeX , btnY1 =  qidongqitajichangdeY + 100 , btnWidth1 = 100, btnHeight1 = 50;
	//���ư汾�������Ͻ�
	settextstyle(20, 0, (ziti));
	outtextxy(10, 10, _T("ATC4-HQ 1.4.0.0.0"));
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
	// a���ư�ť
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, (ziti));
    outtextxy(btnX + 10, btnY + 15, _T("����RJOO"));
	// b���ư�ť
    setfillcolor(LIGHTGRAY);
	btnWidth1 += 50;
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
					const char* world = "�ļ�/RJAA.dll";
					const char* the = "XPACK.dll" ;
					if (CopyFileA(world, the, FALSE)) {
						// �ļ����Ƴɹ�
						std::ifstream inputFile("ATC4-HQ.ini"); // ���ļ�
						if (!inputFile) {
							return false; // �ļ���ʧ��
						}
						std::string line;
						// ��ȡ��һ�У�������
						if (std::getline(inputFile, line)) {
							// ��ȡ�ڶ���
							if (std::getline(inputFile, line)) {
								//�еڶ���
								std::wstring LEdizhi(line.begin(), line.end()); // ���ڶ���ת��Ϊwstring
								std::wstring command = LEdizhi + L"\\LEProc.exe" + L" " + L"-run AXA.exe";
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
								//û�еڶ���
								TCHAR szBuffer[MAX_PATH] = {0}; // ���ѡ���ļ��е�·��
								BROWSEINFO bi;
								ZeroMemory(&bi, sizeof(BROWSEINFO));
								bi.hwndOwner = NULL;
								bi.pszDisplayName = szBuffer;
								bi.lpszTitle = _T("��ѡ��һ���ļ���:"); // �Ի������
								bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE; // �������ļ�ϵͳĿ¼����ʹ���¶Ի�����ʽ
								LPITEMIDLIST idl = SHBrowseForFolder(&bi); // ��ʾѡ���ļ��жԻ���
								if (NULL == idl)
								{
								return false;
								}
								SHGetPathFromIDList(idl, szBuffer); // ��ȡѡ����ļ���·��
								std::string filePath = "ATC4-HQ.ini";
								wchar_t wideBuffer[260];
								mbstowcs(wideBuffer, szBuffer, 260); // �����ֽ��ַ���ת��Ϊ���ַ�
								std::wstring wideStr = wideBuffer;  // ת��Ϊ std::wstring
								std::wstring newContent = wideBuffer;
								// ��������д��ڶ���
							}
						} else {
							return false; // �ļ���ȡʧ��
						}
						inputFile.close(); // �ر��ļ�
					} else {	
						// �ļ�����ʧ��
						return false;
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
					const char* world = "�ļ�/RJOO.dll";
					const char* the = "XPACK.dll" ;
					if (CopyFileA(world, the, FALSE)) {
						// �ļ����Ƴɹ�
						Sleep(1000); // �ȴ�1��
    					std::wstring command = L"B:\\Locale-Emulator-2.5.0.1\\LEProc.exe -run AXA.exe";
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
						// �ļ�����ʧ��
						return false;
					}
					break;
				}
			}
		}
	}
	closegraph();
	return 0;
}