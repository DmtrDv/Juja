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
    public partial class JujaTestForm : Form
    {
        /*---------------------------------------------------------------------------------------------------------------------*/

        private Timer timer = new Timer();
        private int actionTimer = 100;
        // сегменты тела Жужи
        private List<PointF> jujaSegments = new List<PointF>();

        private int segmentsCount = 41; //количество сегментов в Жуже
        private int distanceBetweenSegments = 8; // расстояние между сегментами

        //действия Жужи
        public enum JujaActions
        {
            Following,
            Roam,
            Runnig,
            Rotation
        }

        private Random randomAction = new Random();

        private JujaActions currentAction = JujaActions.Following; //текущее действие Жужы

        private float speedMove = 5;

        PointF laggingCursor = new PointF(110, 110);
        float animPhase;

        public JujaTestForm()
        {
            InitializeComponent();
            // настройка прозрачной формы
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.Location = new Point(0, 0);
            this.BackColor = Color.Fuchsia;
            this.TransparencyKey = Color.Fuchsia;
            this.DoubleBuffered = true;

            // настройка таймера
            timer.Interval = 20;
            timer.Tick += Update;
            timer.Start();

            // инициализация Жужы
            for (int i = 0; i <= segmentsCount; i++)
            {
                jujaSegments.Add(new PointF(110, 110));
            }
        }

        public void Update(object sender, EventArgs e)
        {
            Point positionOfCursor = Cursor.Position;
            PointF positionHeadJuja = jujaSegments[0];
            double distanceJujaToCursor = Distance(positionOfCursor, positionHeadJuja);

            actionTimer--;
            if (actionTimer <= 0)
            {
                if (distanceJujaToCursor <= 50)
                {
                    currentAction = JujaActions.Rotation;
                    actionTimer = randomAction.Next(50, 200);
                }
                else
                {
                    currentAction = JujaActions.Following;
                    actionTimer = randomAction.Next(50, 200);
                }
            }

            laggingCursor.X += (Cursor.Position.X - laggingCursor.X) * 0.2f;
            laggingCursor.Y += (Cursor.Position.Y - laggingCursor.Y) * 0.2f;

            switch (currentAction)
            {
                case JujaActions.Following:
                    speedMove = 3;
                    float dx = laggingCursor.X - jujaSegments[0].X;
                    float dy = laggingCursor.Y - jujaSegments[0].Y;
                    float distance = Distance(laggingCursor, jujaSegments[0]);

                    jujaSegments[0] = new PointF(jujaSegments[0].X + dx / distance * speedMove, 
                                                jujaSegments[0].Y + dy / distance * speedMove);
                    
                    for (int i = 1; i < jujaSegments.Count; i++)
                    {
                        PointF previousSegmentPos = jujaSegments[i-1];
                        PointF currentSegmentPos = jujaSegments[i];
                        
                        float dxSegments = previousSegmentPos.X - currentSegmentPos.X;
                        float dySegments = previousSegmentPos.Y - currentSegmentPos.Y;
                        float distanceFromSegmentToSegment = Distance(previousSegmentPos, currentSegmentPos);

                        if (distanceFromSegmentToSegment > distanceBetweenSegments)
                        {
                            currentSegmentPos.X = currentSegmentPos.X + (dxSegments - dxSegments / distanceFromSegmentToSegment * distanceBetweenSegments);
                            currentSegmentPos.Y = currentSegmentPos.Y + (dySegments - dySegments / distanceFromSegmentToSegment * distanceBetweenSegments);
                            jujaSegments[i] = currentSegmentPos;
                        }
                    }
                    
                    break;
                case JujaActions.Rotation:
                    speedMove = 1;
                    break;
            }
            animPhase += 0.3f;
            Invalidate();
        }
        public float Distance(PointF a, PointF b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

            using (Font font = new Font("Consolas", 10))
            using (Brush brush = new SolidBrush(Color.Green))
            {
                // отрисовка головы
                PointF head = new PointF(jujaSegments[0].X, jujaSegments[0].Y);
                g.DrawString("@", font, brush, head);

                // отрисовка тела
                for (int i = 1; i < jujaSegments.Count; i++)
                {
                    PointF pointOfSegments = jujaSegments[i];

                    string bodyChar = i % 2 == 0 ? "(" : ")";
                    g.DrawString(bodyChar, font, brush, pointOfSegments);

                    // отрисовка ног
                    if (i % 3 == 0)
                    {
                        PointF previousSegment = jujaSegments[i - 1];
                        float dx = pointOfSegments.X - previousSegment.X;
                        float dy = pointOfSegments.Y - previousSegment.Y;

                        // Единичный вектор вперёд (приближённо)
                        float fx = -dy / distanceBetweenSegments;
                        float fy = dx / distanceBetweenSegments; 

                        // Базовая длина ноги и анимация шага
                        float legBase = 10;
                        float swing = (float)Math.Sin(animPhase + i * 0.5f) * 3f;

                        PointF rightLegPos = new PointF(pointOfSegments.X + fx * legBase + swing, pointOfSegments.Y + fy * legBase);
                        PointF leftLegPos = new PointF(pointOfSegments.X - fx * legBase - swing, pointOfSegments.Y - fy * legBase);

                        g.DrawString("\\", font, brush, rightLegPos);
                        g.DrawString("/", font, brush, leftLegPos);
                    }
                }
            }




            /*---------------------------------------------------------------------------------------------------------------------*/
            /*[DllImport("user32.dll")]
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
            }*/
        }
    }
}
