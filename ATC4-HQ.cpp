#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN); //这个变量int a是用户显示屏高
	int b = a /4; //这个变量int b是程序运行时的高和宽
	// 检查鼠标是否在按钮区域内
    bool c (int x, int y, int btnX, int btnY, int btnWidth, int btnHeight) {
    return x >= btnX && x <= btnX + btnWidth && y >= btnY && y <= btnY + btnHeight;
	}
	//如果鼠标在按钮区域内，返回布尔值c true，否则返回false
	int qidongyouxianniudeX = b / 3 - 50; //按钮的X坐标
	int qidongyouxianniudeY = b / 3 * 2; //按钮的Y坐标
	void clearButton(int x, int y, int width, int height) {
		setfillcolor(WHITE); // 设置填充颜色为白色
		solidrectangle(x, y, x + width, y + height); // 用白色填充按钮区域
	}
	void chongzhipingmu() {
		setfillcolor(WHITE); // 设置填充颜色为白色
		solidrectangle(0, 0, b, b); // 用白色填充整个屏幕
		IMAGE img; //定义一个图片对象
		loadimage(&img , "ATC4.ico" , b , b ,false); //加载图片
		putimage(0,0 , &img ); //在屏幕上显示图片
	}
}
int main() {
	using namespace master;
	SetConsoleOutputCP(936); //设置控制台输出编码为GBK
	initgraph(b,b); //初始化图形窗口
	chongzhipingmu(); //清屏
	// 按钮位置和大小
    int btnX =   qidongyouxianniudeX , btnY =  qidongyouxianniudeY , btnWidth = 100, btnHeight = 50;
	// 绘制按钮
    setfillcolor(LIGHTGRAY);
    solidrectangle(btnX, btnY, btnX + btnWidth, btnY + btnHeight);
    settextstyle(20, 0, _T("仿宋"));
    outtextxy(btnX + 10, btnY + 15, _T("启动游戏"));
	while ( bool x = true)
	{
		std::cout << "点击按钮以启动游戏" << std::endl;
		while (true) {
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
	}
	std::cout << "b" << std::endl;
	closegraph();
	return 0;
}