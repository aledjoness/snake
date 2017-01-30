using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Snake
{
    public partial class SnakeGame : Form
    {
        private readonly int blockJump = 20;
        enum Direction { none, up, down, left, right };
        public enum SnakeSpeed { none, slow, normal, fast };
        private Direction headDirection;
        private int snakeLength;
        private bool gameOver;
        private List<Point> snakePositions;
        private List<Point> obstaclePositions;
        private bool haveFood;
        private Point foodLocation;
        private List<int> yRowStartValue;
        private SnakePart _head;
        private SnakeSpeed snakeSpeed;
        private object _lock = new object();
        private Random _rnd;

        public SnakeGame()
        {
            // Grid stretches from (0, 24) [top left] to (660, 664) [bottom right], middle is (320, 344)            
            InitializeComponent();
            Focus();
            snakePositions = new List<Point>();
            obstaclePositions = new List<Point>();
            yRowStartValue = new List<int>();
            populateRowStartCoords();
            createInitialSnake();
            haveFood = false;
            _rnd = new Random();
            placeFood();
            normalToolStripMenuItem.Checked = true;
            snakeSpeed = SnakeSpeed.normal;
            gameOver = false;
            Thread snakeTimeControl = new Thread(timeControl);
            snakeTimeControl.IsBackground = true;
            snakeTimeControl.Start();
        }

        public SnakeGame(SnakeSpeed speed, bool placingObstacles)
        {
            InitializeComponent();
            Focus();
            snakePositions = new List<Point>();
            obstaclePositions = new List<Point>();
            yRowStartValue = new List<int>();
            populateRowStartCoords();
            createInitialSnake();
            haveFood = false;
            _rnd = new Random();
            placeFood();
            switch (speed)
            {
                case SnakeSpeed.slow:
                    slowToolStripMenuItem.Checked = true;
                    break;
                case SnakeSpeed.normal:
                    normalToolStripMenuItem.Checked = true;
                    break;
                case SnakeSpeed.fast:
                    fastToolStripMenuItem.Checked = true;
                    break;
            }
            snakeSpeed = speed;

            if (placingObstacles)
                obstaclesToolStripMenuItem.Checked = true;
            gameOver = false;
            Thread snakeTimeControl = new Thread(timeControl);
            snakeTimeControl.IsBackground = true;
            snakeTimeControl.Start();
        }

        private void createInitialSnake()
        {
            SnakePart head = new SnakePart();
            head.Location = new Point(320, 344);
            head.BackColor = Color.Red;
            head.Size = new Size(20, 20);
            head.FlatStyle = FlatStyle.Flat;
            head.Enabled = false;
            head.ImageAlign = ContentAlignment.MiddleLeft;
            snakePositions.Add(head.Location);
            _head = head;
            Controls.Add(_head);
            SnakePart body0 = new SnakePart();
            body0.Location = new Point(300, 344);
            body0.BackColor = Color.Yellow;
            body0.Size = new Size(20, 20);
            body0.FlatStyle = FlatStyle.Flat;
            body0.Enabled = false;
            body0.Name = "body0";
            body0.ImageAlign = ContentAlignment.MiddleLeft;
            snakePositions.Add(body0.Location);
            Controls.Add(body0);
            SnakePart body1 = new SnakePart();
            body1.Location = new Point(280, 344);
            body1.BackColor = Color.Yellow;
            body1.Size = new Size(20, 20);
            body1.FlatStyle = FlatStyle.Flat;
            body1.Enabled = false;
            body1.Name = "body1";
            body1.ImageAlign = ContentAlignment.MiddleLeft;
            snakePositions.Add(body1.Location);
            Controls.Add(body1);
            headDirection = Direction.right;
            snakeLength = 3;
        }

        private void populateRowStartCoords()
        {
            int yChange;
            int prevY = 24;
            for (int i = 0; i < 33; i++)
            {
                if (i == 0)
                    yChange = 0;
                else
                    yChange = 20;
                int yToAdd = prevY + yChange;
                yRowStartValue.Add(yToAdd);
                prevY = yToAdd;
            }
        }

        private void timeControl(object state)
        {
            while (!gameOver)
            {
                switch (snakeSpeed)
                {
                    case SnakeSpeed.slow:
                        Thread.Sleep(150);
                        break;
                    case SnakeSpeed.normal:
                        Thread.Sleep(100);
                        break;
                    case SnakeSpeed.fast:
                        Thread.Sleep(70);
                        break;
                }
                Direction instance;
                lock (_lock)
                {
                    instance = headDirection;
                }
                lock (_lock)
                {
                    moveSnake(instance);
                }
                // Check if we need to place food
                if (!haveFood)
                {
                    placeFood();
                }
                if (obstaclesToolStripMenuItem.Checked)
                {
                    if (_rnd.Next(1, 100) == 1)
                    {
                        placeObstacle();
                    }
                }
            }
            makeSnakeDead();
            showGameOverLabel();
        }

        delegate void WindowActionCallBack();

        delegate void WindowActionCallBackWithParams(Direction dir);

        private void moveSnake(Direction headDirectionInstance)
        {
            Point pointToMoveTo;
            switch (headDirectionInstance)
            {
                case Direction.up:
                    pointToMoveTo = new Point(_head.Location.X, _head.Location.Y - blockJump);
                    if (_head.Location.Y - blockJump < 24 || snakePositions.Contains(pointToMoveTo) || obstaclePositions.Contains(pointToMoveTo))
                    {
                        gameOver = true;
                    }
                    else
                    {
                        if (InvokeRequired)
                        {
                            WindowActionCallBackWithParams d = new WindowActionCallBackWithParams(moveSnake);
                            Invoke(d, new object[] { headDirectionInstance });
                        }
                        else
                        {
                            Point oldheadLocation = _head.Location;
                            _head.Location = new Point(_head.Location.X, _head.Location.Y - blockJump);
                            snakePositions[0] = _head.Location;
                            if (pointToMoveTo == foodLocation)
                                moveSnakeBody(oldheadLocation, true);
                            else
                                moveSnakeBody(oldheadLocation, false);
                        }
                    }
                    break;
                case Direction.down:
                    pointToMoveTo = new Point(_head.Location.X, _head.Location.Y + blockJump);
                    if (_head.Location.Y + blockJump > 664 || snakePositions.Contains(pointToMoveTo) || obstaclePositions.Contains(pointToMoveTo))
                    {
                        gameOver = true;
                    }
                    else
                    {
                        if (InvokeRequired)
                        {
                            WindowActionCallBackWithParams d = new WindowActionCallBackWithParams(moveSnake);
                            Invoke(d, new object[] { headDirectionInstance });
                        }
                        else
                        {
                            Point oldheadLocation = _head.Location;
                            _head.Location = new Point(_head.Location.X, _head.Location.Y + blockJump);
                            snakePositions[0] = _head.Location;
                            if (pointToMoveTo == foodLocation)
                                moveSnakeBody(oldheadLocation, true);
                            else
                                moveSnakeBody(oldheadLocation, false);
                        }
                    }
                    break;
                case Direction.left:
                    pointToMoveTo = new Point(_head.Location.X - blockJump, _head.Location.Y);
                    if (_head.Location.X - blockJump < 0 || snakePositions.Contains(pointToMoveTo) || obstaclePositions.Contains(pointToMoveTo))
                    {
                        gameOver = true;
                    }
                    else
                    {
                        if (InvokeRequired)
                        {
                            WindowActionCallBackWithParams d = new WindowActionCallBackWithParams(moveSnake);
                            Invoke(d, new object[] { headDirectionInstance });
                        }
                        else
                        {
                            Point oldheadLocation = _head.Location;
                            _head.Location = new Point(_head.Location.X - blockJump, _head.Location.Y);
                            snakePositions[0] = _head.Location;
                            if (pointToMoveTo == foodLocation)
                                moveSnakeBody(oldheadLocation, true);
                            else
                                moveSnakeBody(oldheadLocation, false);
                        }
                    }
                    break;
                case Direction.right:
                    pointToMoveTo = new Point(_head.Location.X + blockJump, _head.Location.Y);
                    if (_head.Location.X + blockJump > 660 || snakePositions.Contains(pointToMoveTo) || obstaclePositions.Contains(pointToMoveTo))
                    {
                        gameOver = true;
                    }
                    else
                    {
                        if (InvokeRequired)
                        {
                            WindowActionCallBackWithParams d = new WindowActionCallBackWithParams(moveSnake);
                            Invoke(d, new object[] { headDirectionInstance });
                        }
                        else
                        {
                            Point oldheadLocation = _head.Location;
                            _head.Location = new Point(_head.Location.X + blockJump, _head.Location.Y);
                            snakePositions[0] = _head.Location;
                            if (pointToMoveTo == foodLocation)
                                moveSnakeBody(oldheadLocation, true);
                            else
                                moveSnakeBody(oldheadLocation, false);
                        }
                    }
                    break;
            }
        }

        private void moveSnakeBody(Point oldHeadLoc, bool addBodyPartToTheEnd)
        {
            string buttonToFind = "body0";
            Point oldBodyLocation = new Point();
            Point newBodyLocation = new Point();
            Control[] controls = Controls.Find(buttonToFind, true);
            if (controls.Length > 0)
            {
                Button b = controls[0] as Button;
                oldBodyLocation = b.Location;
                b.Location = oldHeadLoc;
                snakePositions[1] = b.Location;
            }

            for (int i = 2; i < snakeLength; i++)
            {
                buttonToFind = "body" + (i - 1).ToString();
                controls = Controls.Find(buttonToFind, true);
                if (controls.Length > 0)
                {
                    if (addBodyPartToTheEnd && i == snakeLength - 1)
                    {
                        newBodyLocation = snakePositions[i];
                    }
                    Button b = controls[0] as Button;
                    b.Location = oldBodyLocation;
                    oldBodyLocation = snakePositions[i];
                    snakePositions[i] = b.Location;
                }
            }

            if (addBodyPartToTheEnd)
            {
                SnakePart _body = new SnakePart();
                _body.Location = newBodyLocation;
                _body.BackColor = Color.Yellow;
                _body.Size = new Size(20, 20);
                _body.FlatStyle = FlatStyle.Flat;
                _body.Enabled = false;
                _body.Name = "body" + (snakeLength - 1).ToString();
                _body.ImageAlign = ContentAlignment.MiddleLeft;
                snakePositions.Add(_body.Location);
                snakeLength++;
                Controls.Add(_body);

                controls = Controls.Find("food", true);
                if (controls.Length > 0)
                {
                    Button b = controls[0] as Button;
                    b.Dispose();
                    haveFood = false;
                }
            }
        }

        private void placeFood()
        {
            // Logic here is that we place another snakebody bit, given a list of valid X and Y points to choose from,
            // checking with snakePositions and obstaclePositions to prevent a clash, then placing the food.
            // Food is placed like a normal snakebody, but then it is resized and location altered to reflect this
            // There's 33 * 34 (1122) different grid squares to choose from
            if (InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(placeFood);
                Invoke(d, new object[] { });
            }
            else
            {
                bool validPlacement = false;
                int xCoord = 0, yCoord = 0;

                while (!validPlacement)
                {
                    Random rnd = new Random();
                    int gridSquareToPlace = rnd.Next(0, 1122);

                    int row = gridSquareToPlace / 34;

                    yCoord = yRowStartValue[row];
                    xCoord = 0;
                    for (int i = row * 34; i < gridSquareToPlace; i++)
                    {
                        xCoord += 20;
                    }
                    Point pointToPlaceFoodAt = new Point(xCoord, yCoord);
                    if (!snakePositions.Contains(pointToPlaceFoodAt) || !obstaclePositions.Contains(pointToPlaceFoodAt))
                    {
                        validPlacement = true;
                        foodLocation = pointToPlaceFoodAt;
                    }
                }
                // Translate to a point 
                Button food = new Button();
                int scaledXCoord = xCoord + 5;
                int scaledYCoord = yCoord + 5;
                food.Location = new Point(scaledXCoord, scaledYCoord);
                food.Size = new Size(10, 10);
                food.BackColor = Color.DeepSkyBlue;
                food.Enabled = false;
                food.FlatStyle = FlatStyle.Popup;
                food.Name = "food";
                Controls.Add(food);
                haveFood = true;
            }
        }

        private void placeObstacle()
        {
            // Logic here is that we place another snakebody bit, given a list of valid X and Y points to choose from,
            // checking with snakePositions to prevent a clash, then placing the food.
            // Food is placed like a normal snakebody, but then it is resized and location altered to reflect this
            // There's 33 * 34 (1122) different grid squares to choose from
            if (InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(placeObstacle);
                Invoke(d, new object[] { });
            }
            else
            {
                bool validPlacement = false;
                int xCoord = 0, yCoord = 0;

                while (!validPlacement)
                {
                    Random rnd = new Random();
                    int gridSquareToPlace = rnd.Next(0, 1122);

                    int row = gridSquareToPlace / 34;

                    yCoord = yRowStartValue[row];
                    xCoord = 0;
                    for (int i = row * 34; i < gridSquareToPlace; i++)
                    {
                        xCoord += 20;
                    }
                    Point pointToPlaceObstacleAt = new Point(xCoord, yCoord);
                    if (!snakePositions.Contains(pointToPlaceObstacleAt) || pointToPlaceObstacleAt != foodLocation)
                    {
                        validPlacement = true;
                        obstaclePositions.Add(pointToPlaceObstacleAt);
                    }
                }
                // Translate to a point 
                Button obstacle = new Button();
                int scaledXCoord = xCoord + 1;
                int scaledYCoord = yCoord + 1;
                obstacle.Location = new Point(scaledXCoord, scaledYCoord);
                obstacle.Size = new Size(18, 18);
                obstacle.BackColor = Color.LightGray;
                obstacle.Enabled = false;
                obstacle.FlatStyle = FlatStyle.Flat;
                obstacle.Name = "obstacle" + (obstaclePositions.Count - 1).ToString();
                Controls.Add(obstacle);
                haveFood = true;
            }
        }

        private void makeSnakeDead()
        {
            if (InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(makeSnakeDead);
                Invoke(d, new object[] { });
            }
            else
            {
                _head.BackColor = Color.Gray;
                for (int i = 0; i < snakeLength; i++)
                {
                    string buttonName = "body" + i;
                    Control[] controls = Controls.Find(buttonName, true);
                    if (controls.Length > 0)
                    {
                        Button b = controls[0] as Button;
                        b.BackColor = Color.Gray;
                    }
                }
            }
        }

        private void showGameOverLabel()
        {
            if (InvokeRequired)
            {
                WindowActionCallBack d = new WindowActionCallBack(showGameOverLabel);
                Invoke(d, new object[] { });
            }
            else
            {
                Label l = new Label();
                l.Font = new Font("Microsoft Sans Serif", 22, FontStyle.Bold);
                l.Text = "Game Over\nScore: " + snakeLength;
                l.Location = new Point(252, 326);
                l.BackColor = Color.White;
                l.AutoSize = true;
                l.BringToFront();
                Controls.Add(l);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                if (headDirection == Direction.left || headDirection == Direction.right)
                {
                    lock (_lock)
                    {
                        headDirection = Direction.up;
                    }
                }
                return true;
            }
            if (keyData == Keys.Down)
            {
                if (headDirection == Direction.left || headDirection == Direction.right)
                {
                    lock (_lock)
                    {
                        headDirection = Direction.down;
                    }
                }
                return true;
            }
            if (keyData == Keys.Left)
            {
                if (headDirection == Direction.up || headDirection == Direction.down)
                {
                    lock (_lock)
                    {
                        headDirection = Direction.left;
                    }
                }
                return true;
            }
            if (keyData == Keys.Right)
            {
                if (headDirection == Direction.up || headDirection == Direction.down)
                {
                    lock (_lock)
                    {
                        headDirection = Direction.right;
                    }
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SnakeGame_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                SnakeGame sg = new SnakeGame(snakeSpeed, obstaclesToolStripMenuItem.Checked);
                sg.FormClosed += (s, args) => Close();
                Hide();
                sg.Show();
            }
            if (e.Control && e.KeyCode == Keys.O)
            {
                obstaclesToolStripMenuItem.Checked = true;
            }
            if (e.Control && e.KeyCode == Keys.S)
            {
                normalToolStripMenuItem.Checked = false;
                fastToolStripMenuItem.Checked = false;
                snakeSpeed = SnakeSpeed.slow;
                slowToolStripMenuItem.Checked = true;
            }
            if (e.Control && e.KeyCode == Keys.R)
            {
                slowToolStripMenuItem.Checked = false;
                fastToolStripMenuItem.Checked = false;
                snakeSpeed = SnakeSpeed.normal;
                normalToolStripMenuItem.Checked = true;
            }
            if (e.Control && e.KeyCode == Keys.F)
            {
                slowToolStripMenuItem.Checked = false;
                normalToolStripMenuItem.Checked = false;
                snakeSpeed = SnakeSpeed.fast;
                fastToolStripMenuItem.Checked = true;
            }
            if (e.KeyCode == Keys.Escape)
            {
                if (Application.MessageLoop)
                {
                    Application.Exit();
                }
                else
                {
                    gameOver = true;
                    Environment.Exit(1);
                }
            }
        }

        private void button_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SnakeGame sg = new SnakeGame(snakeSpeed, obstaclesToolStripMenuItem.Checked);
            sg.FormClosed += (s, args) => Close();
            Hide();
            sg.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                gameOver = true;
                Environment.Exit(1);
            }
        }

        private void slowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            normalToolStripMenuItem.Checked = false;
            fastToolStripMenuItem.Checked = false;
            snakeSpeed = SnakeSpeed.slow;
            slowToolStripMenuItem.Checked = true;
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            slowToolStripMenuItem.Checked = false;
            fastToolStripMenuItem.Checked = false;
            snakeSpeed = SnakeSpeed.normal;
            normalToolStripMenuItem.Checked = true;
        }

        private void fastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            slowToolStripMenuItem.Checked = false;
            normalToolStripMenuItem.Checked = false;
            snakeSpeed = SnakeSpeed.fast;
            fastToolStripMenuItem.Checked = true;
        }
    }
}
