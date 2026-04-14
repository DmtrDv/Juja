using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Juja
{
    public partial class JujaTestForm: Form
    {
        // Импортируем функцию для получения глобальных координат курсора
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        // Структура для координат
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        // Смещение относительно курсора (чтобы объект не перекрывал курсор)
        //private readonly Point offset = new Point(20, 20);
        Cursor cursor;
        public JujaTestForm()
        {
            InitializeComponent();

            // Настройка формы
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(100, 125); // размер "объекта"
            this.BackColor = Color.Red;      // цвет для наглядности

            // Можно сделать круглую форму через Region (опционально)

            // Таймер для обновления позиции
            Timer timer = new Timer();
            timer.Interval = 15; // 15 мс ≈ 66 FPS
            //timer.Tick += Timer_Tick;
            timer.Tick += MoveToCursor_Timer_Tick;
            timer.Start();
        }
        private void MoveToCursor_Timer_Tick(object sender, EventArgs e)
        {
            GetCursorPos(out POINT cursorPos);
            //this.Location = new Point();
            /*if (this.Location.X <= cursorPos.X - 50 || this.Location.X >= cursorPos.X + 50 && this.Location.Y <= cursorPos.Y - 50 || this.Location.Y >= cursorPos.Y + 50)
            {
                this.Location = this.Location;
            }*/
            /*if (cursorPos.X - 50 <= this.Location.X || cursorPos.X + 50 >= this.Location.X  && cursorPos.Y - 50 <= this.Location.Y || cursorPos.Y + 50 >= this.Location.Y)
            {
                this.Location = this.Location;
            }
            else
            {*/
                int X = cursorPos.X;
                int Y = cursorPos.Y;

            int NewPlusX = 5;//(X - this.Location.X) / 100; 
            int NewPlusY = 5; //(Y - this.Location.Y) / 100; 

              /*  if (NewPlusX < 5)
                {
                    NewPlusX = 5;
                }
                if (NewPlusY < 5)
                {
                    NewPlusY = 5;
                }*/

                if (this.Location.X > cursorPos.X && this.Location.Y > cursorPos.Y)
                {
                    this.Location = new Point(this.Location.X - NewPlusX, this.Location.Y - NewPlusY);
                }
                else if (this.Location.X < cursorPos.X && this.Location.Y < cursorPos.Y)
                {
                    this.Location = new Point(this.Location.X + NewPlusX, this.Location.Y + NewPlusY);
                }
                else if (this.Location.X < cursorPos.X && this.Location.Y > cursorPos.Y)
                {
                    this.Location = new Point(this.Location.X + NewPlusX, this.Location.Y - NewPlusY);
                }
                else
                {
                    this.Location = new Point(this.Location.X - NewPlusX, this.Location.Y + NewPlusY);
                }
            //}


            /*
            // Получаем глобальную позицию курсора
            if (GetCursorPos(out POINT cursorPos))
            {
                // Устанавливаем позицию окна со смещением
                //this.Location = new Point(cursorPos.X + offset.X, cursorPos.Y + offset.Y);
            }*/
        }

        // Чтобы форма не исчезала при Alt+Tab, можно переопределить CreateParams
      /*  protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW – не показывать в Alt+Tab
                return cp;
            }
        }*/
    }
}
