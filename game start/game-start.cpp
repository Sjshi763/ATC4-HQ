#include <graphics.h>
#include <conio.h>
#include <iostream>
#include <windows.h>
namespace master {
	int a = GetSystemMetrics(SM_CXSCREEN); //这个变量int a是用户显示屏高
	int b = GetSystemMetrics(SM_CYSCREEN); //这个变量int b是用户显示屏宽
}
int main() {
	using namespace master;
	IMAGE img;
	char g[MAX_PATH]; //定义一个字符数组g，大小为MAX_PATH用来获取当前工作目录
	GetCurrentDirectoryA(MAX_PATH, g); //获取当前工作目录
	std::cout << "当前工作目录为" << g << std::endl; //输出当前工作目录
	loadimage(&img, "ATC4.ico"); //加载图片
	int c = img.getwidth(); //获取图片宽度
	int d = img.getheight(); //获取图片高度
	if (c == 0 && d == 0)  /*判断图片是否加载成功*/ { 
		std::string f = std::string(g) + "/ATC4.ico"; //图片路径
		loadimage(&img, f.c_str()); //加载图片
		int c = img.getwidth(); //获取图片宽度
		int d = img.getheight(); //获取图片高度
		int e = c/d ;//获取图片宽高比
	}
	else {
		int e = c/d ;//获取图片宽高比
	}
	std::cout << "宽高比为" << e << std::endl; //输出图片宽高比
	Sleep(10000); //延时1秒
	// initgraph(,);
	// putimage(b, 0, "/ATC4.ico"); //在屏幕上显示图片
	// closegraph();
	return 0;
}