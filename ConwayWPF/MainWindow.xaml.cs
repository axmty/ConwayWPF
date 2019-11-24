using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ConwayWPF
{
    public partial class MainWindow : Window
    {
        private static readonly string WindowTitleTemplate = "Conway's Game of Life - Step {0}";
        private static readonly int TickIntervalMilliseconds = 300;
        private static readonly int BoardWidth = 30;
        private static readonly int BoardHeight = 30;
        private static readonly int CellSize = 20;
        private static readonly Brush AliveCellColor = Brushes.Black;
        private static readonly Brush DeadCellColor = Brushes.White;
        private static readonly Brush StrokeColor = Brushes.LightGray;
        private static readonly DoubleCollection StrokeDashArray = new DoubleCollection { 2, 2 };

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Rectangle[,] _universe = new Rectangle[BoardHeight, BoardWidth];

        private bool _running = false;
        private int _step = 1;

        public MainWindow()
        {
            InitializeComponent();

            _timer.Tick += this.Timer_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(TickIntervalMilliseconds);
        }

        private void Start()
        {
            _timer.Start();
            _running = true;
            this.UpdateWindowTitle();
        }

        private void Pause()
        {
            _timer.Stop();
            _running = false;
        }

        private void UpdateWindowTitle()
        {
            this.Title = string.Format(WindowTitleTemplate, _step);
        }

        private void DrawDeadCell(int x, int y)
        {
            var cell = new Rectangle
            {
                Width = CellSize,
                Height = CellSize,
                Fill = DeadCellColor,
                Stroke = StrokeColor,
                StrokeThickness = 1,
                StrokeDashArray = StrokeDashArray
            };
            cell.MouseDown += this.Cell_MouseDown;

            Area.Children.Add(cell);
            _universe[y, x] = cell;

            Canvas.SetLeft(cell, x * CellSize);
            Canvas.SetTop(cell, y * CellSize);
        }

        private void DrawBoard()
        {
            Area.Width = BoardWidth * CellSize;
            Area.Height = BoardHeight * CellSize;

            for (int i = 0; i < BoardHeight; i++)
            {
                for (int j = 0; j < BoardWidth; j++)
                {
                    this.DrawDeadCell(j, i);
                }
            }
        }

        private void NextStep()
        {
            var cellsToChange = new List<Rectangle>();

            for (int i = 0; i < BoardHeight; i++)
            {
                for (int j = 0; j < BoardWidth; j++)
                {
                    var neighboursCount = CountNeighbours(j, i);
                    var livesInNextStep = neighboursCount == 3 || (this.IsAlive(j, i) && neighboursCount == 2);
                    var toUpdate = livesInNextStep != this.IsAlive(j, i);

                    if (toUpdate)
                    {
                        cellsToChange.Add(_universe[i, j]);
                    }
                }
            }

            foreach (var cell in cellsToChange)
            {
                this.ChangeCellState(cell);
            }

            _step += 1;
            this.UpdateWindowTitle();
        }

        private void ChangeCellState(Rectangle cell)
        {
            cell.Fill = this.IsAlive(cell) ? DeadCellColor : AliveCellColor;
        }

        private int CountNeighbours(int x, int y)
        {
            return
                IsInBoardAndAlive(x - 1, y - 1) +
                IsInBoardAndAlive(x - 1, y) +
                IsInBoardAndAlive(x - 1, y + 1) +
                IsInBoardAndAlive(x, y - 1) +
                IsInBoardAndAlive(x, y + 1) +
                IsInBoardAndAlive(x + 1, y - 1) +
                IsInBoardAndAlive(x + 1, y) +
                IsInBoardAndAlive(x + 1, y + 1);
        }

        private bool IsAlive(int x, int y)
        {
            return this.IsAlive(_universe[y, x]);
        }

        private bool IsAlive(Rectangle cell)
        {
            return cell.Fill == AliveCellColor;
        }

        private int IsInBoardAndAlive(int x, int y)
        {
            var isInBoard = x >= 0 && x < BoardWidth && y >= 0 && y < BoardHeight;

            return isInBoard && this.IsAlive(x, y) ? 1 : 0;
        }

        private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_running)
            {
                return;
            }

            this.ChangeCellState((Rectangle)sender);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Space)
            {
                return;
            }

            if (_running)
            {
                this.Pause();
            }
            else
            {
                this.Start();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            this.NextStep();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.DrawBoard();
        }
    }
}
