#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN); //�������int a���û���ʾ����
	int b = a /4; //�������int b�ǳ�������ʱ�ĸߺͿ�
	// �������Ƿ��ڰ�ť������
    bool c (int x, int y, int btnX, int btnY, int btnWidth, int btnHeight) {
    return x >= btnX && x <= btnX + btnWidth && y >= btnY && y <= btnY + btnHeight;
	}
	//�������ڰ�ť�����ڣ����ز���ֵc true�����򷵻�false
	int qidongyouxianniudeX = b / 3 - 50; //��ť��X����
	int qidongyouxianniudeY = b / 3 * 2; //��ť��Y����
	void clearButton(int x, int y, int width, int height) {
		setfillcolor(WHITE); // ���������ɫΪ��ɫ
		solidrectangle(x, y, x + width, y + height); // �ð�ɫ��䰴ť����
	}
	void chongzhipingmu() {
		setfillcolor(WHITE); // ���������ɫΪ��ɫ
		solidrectangle(0, 0, b, b); // �ð�ɫ���������Ļ
		IMAGE img; //����һ��ͼƬ����
		loadimage(&img , "ATC4.ico" , b , b ,false); //����ͼƬ
		putimage(0,0 , &img ); //����Ļ����ʾͼƬ
	}
}
int main() {
	using namespace master;
	SetConsoleOutputCP(936); //���ÿ���̨�������ΪGBK
	initgraph(b,b); //��ʼ��ͼ�δ���
	chongzhipingmu(); //����
	// ��ťλ�úʹ�С
    int btnX =   qidongyouxianniudeX , btnY =  qidongyouxianniudeY , btnWidth = 100, btnHeight = 50;
	// ���ư�ť
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, _T("����"));
    outtextxy(btnX + 10, btnY + 15, _T("������Ϸ"));
	while ( bool x = true)
	{
		std::cout << "�����ť��������Ϸ" << std::endl;
		while (true) {
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
	}
	std::cout << "b" << std::endl;
	closegraph();
	return 0;
}