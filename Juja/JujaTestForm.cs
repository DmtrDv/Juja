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
        /* 
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
             timer.Interval = 30; // 15 мс ≈ 66 FPS
             timer.Tick += MoveToCursor_Timer_Tick;
             timer.Start();
         }
         private void MoveToCursor_Timer_Tick(object sender, EventArgs e)
         {
             GetCursorPos(out POINT cursorPos);
             int X = cursorPos.X;
             int Y = cursorPos.Y;
             int NewPlusX = 5;
             int NewPlusY = 5;

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

        /*---------------------------------------------------------------------------------------------------------------------*/

        Timer timer = new Timer();
        // сегменты тела Жужи
        List<PointF> jujaSegments = new List<PointF>();

        int segmentsCount = 31; //количество сегментов в Жуже
        int distanceBetweenSegments = 10; // расстояние между сегментами

        //действия Жужи
        public enum Actions
        {
            Rotation,
            Following
        }

        Random random = new Random();












        /*---------------------------------------------------------------------------------------------------------------------*/
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        // Состояния змейки
        private enum SnakeState { Following, Circling }
        private SnakeState currentState = SnakeState.Following;

        // Параметры змейки
        private readonly int maxSegments = 12;               // длина тела
        private readonly int segmentSpacing = 10;            // расстояние между сегментами
        private readonly Point headOffset = new Point(300, 300); // позиция головы в форме

        // Параметры движения
        private const double moveSpeed = 5.0;                // пикселей за тик при следовании
        private const double circleRadius = 80.0;            // радиус кружения
        private const double circleAngularSpeed = 0.05;      // радиан за тик

        private List<Point> snakeHistory = new List<Point>(); // история позиций головы
        private Point headPosition;                           // текущая экранная позиция головы
        private double circleAngle = 0.0;                     // текущий угол кружения (рад)

        private Timer animationTimer;
        private bool legToggle = false;                       // для анимации ног
        private int legAnimationCounter = 0;
        public JujaTestForm()
        {
            InitializeComponent();

            // Настройка прозрачной формы
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.Location = new Point(0, 0);
            this.BackColor = Color.Fuchsia;
            this.TransparencyKey = Color.Fuchsia;
            this.DoubleBuffered = true;

            // Инициализация головы и истории начальной позицией курсора
            GetCursorPos(out POINT startPos);
            headPosition = new Point(startPos.X, startPos.Y);
            snakeHistory.Add(headPosition);

            animationTimer = new Timer();
            animationTimer.Interval = 100;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            GetCursorPos(out POINT cursorPos);
            Point cursor = new Point(cursorPos.X, cursorPos.Y);

            // Вычисляем вектор и расстояние от головы до курсора
            double dx = cursor.X - headPosition.X;
            double dy = cursor.Y - headPosition.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (currentState == SnakeState.Following)
            {
                if (distance <= circleRadius)
                {
                    // Переключаемся в режим кружения, запоминаем начальный угол
                    currentState = SnakeState.Circling;
                    circleAngle = Math.Atan2(dy, dx); // угол от курсора к голове (направление на голову)
                                                      // В кружении голова будет двигаться вокруг курсора против часовой стрелки,
                                                      // поэтому угол должен указывать на текущую позицию головы относительно курсора.
                                                      // Угол atan2(dy, dx) даёт направление от курсора к голове.
                }
                else
                {
                    // Двигаем голову в сторону курсора с ограниченной скоростью
                    double step = Math.Min(moveSpeed, distance);
                    headPosition = new Point(
                        headPosition.X + (int)(dx / distance * step),
                        headPosition.Y + (int)(dy / distance * step)
                    );
                }
            }
            else // SnakeState.Circling
            {
                // Если расстояние значительно отклонилось от радиуса (например, курсор резко дёрнулся),
                // переключаемся обратно в следование.
                if (distance > circleRadius * 1.5)
                {
                    currentState = SnakeState.Following;
                    // Двигаем голову чуть-чуть в сторону курсора на этом же тике
                    double step = Math.Min(moveSpeed, distance);
                    headPosition = new Point(
                        headPosition.X + (int)(dx / distance * step),
                        headPosition.Y + (int)(dy / distance * step)
                    );
                }
                else
                {
                    // Увеличиваем угол (движение против часовой стрелки)
                    circleAngle += circleAngularSpeed;
                    // Нормализуем угол
                    circleAngle %= (2 * Math.PI);

                    // Вычисляем новую позицию головы на окружности вокруг курсора
                    double targetX = cursor.X + Math.Cos(circleAngle) * circleRadius;
                    double targetY = cursor.Y + Math.Sin(circleAngle) * circleRadius;
                    headPosition = new Point((int)targetX, (int)targetY);
                }
            }

            // Обновляем историю позиций головы для рисования тела
            // Добавляем новую точку, только если голова сдвинулась достаточно далеко от последней
            if (snakeHistory.Count == 0 || Distance(snakeHistory[0], headPosition) >= segmentSpacing)
            {
                snakeHistory.Insert(0, headPosition);
            }
            // Ограничиваем длину истории
            while (snakeHistory.Count > maxSegments)
                snakeHistory.RemoveAt(snakeHistory.Count - 1);

            // Перемещаем форму так, чтобы голова отображалась в headOffset
            this.Location = new Point(headPosition.X - headOffset.X, headPosition.Y - headOffset.Y);

            // Анимация ног
            legAnimationCounter++;
            if (legAnimationCounter >= 2) // регулируйте значение по вкусу
            {
                legToggle = !legToggle;
                legAnimationCounter = 0;
            }

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (snakeHistory.Count == 0) return;

            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            using (Font font = new Font("Consolas", 14, FontStyle.Bold))
            using (Brush headBrush = new SolidBrush(Color.DarkGreen))
            using (Brush bodyBrush = new SolidBrush(Color.Olive))
            using (Brush legBrush = new SolidBrush(Color.SaddleBrown))
            {
                // Рисуем голову (она всегда в headOffset относительно формы)
                Point headFormPos = new Point(headOffset.X, headOffset.Y);
                g.DrawString("@", font, headBrush, headFormPos);

                // Рисуем тело
                for (int i = 1; i < snakeHistory.Count; i++)
                {
                    // Вычисляем позицию сегмента относительно формы
                    Point segScreenPos = snakeHistory[i];
                    Point segFormPos = new Point(
                        headOffset.X - (snakeHistory[0].X - segScreenPos.X),
                        headOffset.Y - (snakeHistory[0].Y - segScreenPos.Y)
                    );

                    char bodyChar = (i % 2 == 0) ? '(' : ')';
                    g.DrawString(bodyChar.ToString(), font, bodyBrush, segFormPos);

                    // Ноги для каждого второго сегмента
                    if (i % 2 == 0)
                    {
                        string legs = legToggle ? "/\\" : "\\/";
                        Point legPos = new Point(segFormPos.X - 5, segFormPos.Y + 16);
                        g.DrawString(legs, font, legBrush, legPos);
                    }
                }
            }
        }

        private double Distance(Point p1, Point p2)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
