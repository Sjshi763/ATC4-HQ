#这个程序的目标是启动！游戏时让玩家选择启动方式，然后加载游戏。
import tkinter as tk
import subprocess

# 创建主窗口
root = tk.Tk()
root.title("游戏启动器的启动器")

# 设置窗口大小
root.geometry("400x300")

# 创建一个函数
def B():
    subprocess.Popen(["others-airport-start.exe"]) 

# 创建一个函数
def A():
    subprocess.Popen(["RJOO-start.exe"]) 

# 创建一个标签
label = tk.Label(root, text="选择你的机场！", font=("Arial", 24))
label.pack(pady=20)

# 创建一个按钮
button = tk.Button(root, text="其他的", font=("Arial", 14),command=B)
button.pack(pady=10, side=tk.LEFT)

#创建另一个按钮
button = tk.Button(root, text="RJOO", font=("Arial", 14),command=A)
button.pack(pady=10, side=tk.RIGHT)


# 运行主循环
root.mainloop()