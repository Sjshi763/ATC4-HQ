#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN); //�������int a���û���ʾ����
	int b = a /4;
	// �������Ƿ��ڰ�ť������
    bool c (int x, int y, int btnX, int btnY, int btnWidth, int btnHeight) {
    return x >= btnX && x <= btnX + btnWidth && y >= btnY && y <= btnY + btnHeight;
	}
	//�������ڰ�ť�����ڣ����ز���ֵc true�����򷵻�false
}

int main() {
	using namespace master;
	SetConsoleOutputCP(936); //���ÿ���̨�������ΪGBK
	initgraph(b,b);
	IMAGE img; //����һ��ͼƬ����
	loadimage(&img , "ATC4.ico" , b , b ,false); //����ͼƬ
	putimage(0,0 , &img ); //����Ļ����ʾͼƬ
	// ��ťλ�úʹ�С
    int btnX = 200, btnY = 200, btnWidth = 100, btnHeight = 50;
	// ���ư�ť
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, _T("����"));
    outtextxy(btnX + 10, btnY + 15, _T("����"));
	while ( bool x = true)
	{
		
		while (true) {
			// ��������
			if (MouseHit()) {
				MOUSEMSG msg = GetMouseMsg();
				if (msg.uMsg == WM_LBUTTONDOWN) {
					if (c(msg.x, msg.y, btnX, btnY, btnWidth, btnHeight)) {
						// ��ť�����
						outtextxy(10, 10, _T("��ť������ˣ�"));
						break;
					}
				}
			}
		}
	}
	closegraph();
	return 0;
}