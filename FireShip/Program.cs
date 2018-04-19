using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;


/*
    Projekt z przedmiotu  - Komunikacja Człowiek Komputer
    Aplikacja konsolowa
    Autorzy:    Michał Motoszko
                Kacper Szorc
*/

namespace FireShip
{
    struct Position
    {
        public double X { set; get; }
        public double Y { set; get; }

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Position(Position old, double x, double y)
        {
            X = old.X + x;
            Y = old.Y + y;
        }
    }

    struct Velocity
    {
        public double X { set; get; }
        public double Y { set; get; }

        public Velocity(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    class Program
    {
        public static Random rnd = new Random();
        public static Stopwatch stopWatch = new Stopwatch();
        public static Stopwatch gameTime = new Stopwatch();
        public static int framesPerSecondTop = 50;
        public static int frameTime = 1000 / framesPerSecondTop;
        public static int consoleHeight = 80;
        public static int consoleWidth = 80;
        public static int screenHeight = consoleHeight;
        public static int screenWidth = consoleWidth;
        public static Menu menu = null;
        public static List<MovingObject> playerShipList = new List<MovingObject>();
        public static List<MovingObject> enemyShipList = new List<MovingObject>();
        public static List<MovingObject> sceneryObjectList = new List<MovingObject>();
        public static PowerUp powerUp = null;
        public static List<char[]> playerSkins = new List<char[]>();
        public static List<char[]> enemySkins = new List<char[]>();
        public static List<char[]> otherSkins = new List<char[]>();
        public static List<string> highscoresNames = new List<string>();
        public static List<int> highscoresScores = new List<int>();
        public static object drawingLock = new object();
        public static object keyboardLock = new object();
        public static bool newScoreSaved = false;
        public static bool showHighScores = false;
        public static bool playerFire = false;
        public static bool togglePlayerFire = false;
        public static bool playerDead = false;
        public static bool gameStart = false;
        public static bool powerUpAvailable = false;
        public static bool gameRunning = true;
        public static int playerXMovement;
        public static int playerYMovement;
        public static int difficultyTimeIncrement = 15;
        public static int score;
        public static int timeScore;


        // drawing moving object here ---------------------------------------------------------
        static void drawMovingObject(MovingObject mObj)
        {
            double x = mObj.getPosition().X;
            double y = mObj.getPosition().Y;
            double pX = mObj.getPreviousPosition().X;
            double pY = mObj.getPreviousPosition().Y;

            if (x == pX && y == pY)
                return;

            double size = screenWidth * mObj.getSize();
            double XstartingPoint = Math.Round(screenWidth * x - size / 2, MidpointRounding.AwayFromZero);
            double YstartingPoint = Math.Round(screenHeight * y - size / 2, MidpointRounding.AwayFromZero);
            double prevXstartingPoint = Math.Round(screenWidth * pX - size / 2, MidpointRounding.AwayFromZero);
            double prevYstartingPoint = Math.Round(screenHeight * pY - size / 2, MidpointRounding.AwayFromZero);
            int drawingX = (int)XstartingPoint;
            int drawingY = (int)YstartingPoint;
            int previousDrawingX = (int)prevXstartingPoint;
            int previousDrawingY = (int)prevYstartingPoint;

            if (drawingX == previousDrawingX && drawingY == previousDrawingY)
                return;

            if (mObj.GetType() == typeof(Bullet))
            {
                Bullet b = (Bullet)mObj;

                if (drawingX > 0 && drawingY > 0 && drawingX < screenWidth - 1 && drawingY < screenWidth - 1)
                {
                    Console.SetCursorPosition(drawingX, drawingY);
                    Console.Write(b.getShape());
                }

                if (previousDrawingX > 0 && previousDrawingY > 0 && previousDrawingX < screenWidth - 1 && previousDrawingY < screenWidth - 1)
                {
                    Console.SetCursorPosition(previousDrawingX, previousDrawingY);
                    Console.Write(" ");
                }
            }
            else if (mObj.GetType() == typeof(SceneryObject) && mObj.getImage() == null)
            {
                SceneryObject sb = (SceneryObject)mObj;

                if (drawingX > 0 && drawingY > 0 && drawingX < screenWidth - 1 && drawingY < screenWidth - 1)
                {
                    Console.SetCursorPosition(drawingX, drawingY);
                    Console.Write(sb.getShape());
                }

                if (previousDrawingX > 0 && previousDrawingY > 0 && previousDrawingX < screenWidth - 1 && previousDrawingY < screenWidth - 1)
                {
                    Console.SetCursorPosition(previousDrawingX, previousDrawingY);
                    Console.Write(" ");
                }
            }
            else
            {
                EnemyShip ship = null;
                if (mObj.GetType() == typeof(EnemyShip))
                    ship = (EnemyShip)mObj;

                char[] skin = mObj.getImage();
                for (int i = 0; i < (int)size; i++)
                {
                    drawingX = (int)XstartingPoint + i;
                    if (drawingX <= 0 || drawingX >= screenWidth - 1)
                        continue;

                    for (int j = 0; j < (int)size; j++)
                    {
                        drawingY = (int)YstartingPoint + j;
                        if (drawingY <= 0 || drawingY >= screenHeight - 1)
                            continue;

                        Console.SetCursorPosition(drawingX, drawingY);
                        if (ship != null && i == (int)(size / 2) && j == (int)(size / 2))
                            if (ship.getHitpoints() <= 9)
                                Console.Write(ship.getHitpoints());
                            else
                                Console.Write('*');
                        else if (mObj.GetType() == typeof(PowerUp) && i == (int)(size / 2) && j == (int)(size / 2))
                            Console.Write(((PowerUp)mObj).getShape());
                        else
                            Console.Write(skin[j * (int)size + i]);
                    }
                }

                int drawnXDown = (int)Math.Round(screenWidth * x - size / 2, MidpointRounding.AwayFromZero);
                int drawnXUp = (int)Math.Round(screenWidth * x + size / 2, MidpointRounding.AwayFromZero);
                int drawnYDown = (int)Math.Round(screenHeight * y - size / 2, MidpointRounding.AwayFromZero);
                int drawnYUp = (int)Math.Round(screenHeight * y + size / 2, MidpointRounding.AwayFromZero);

                for (int i = 0; i < (int)size; i++)
                {
                    previousDrawingX = (int)prevXstartingPoint + i;
                    if (previousDrawingX <= 0 || previousDrawingX >= screenWidth - 1)
                        continue;

                    for (int j = 0; j < (int)size; j++)
                    {
                        previousDrawingY = (int)prevYstartingPoint + j;
                        if (previousDrawingY <= 0 || previousDrawingY >= screenHeight - 1)
                            continue;

                        if (previousDrawingX < drawnXDown || previousDrawingX >= drawnXUp
                            || previousDrawingY < drawnYDown || previousDrawingY >= drawnYUp)
                        {
                            Console.SetCursorPosition(previousDrawingX, previousDrawingY);
                            Console.Write(" ");
                        }
                    }
                }
            }
        }

        static void drawAllObjects()
        {
            lock (drawingLock)
            {
                Console.ForegroundColor = ConsoleColor.White;

                foreach (MovingObject mObj in sceneryObjectList)
                    drawMovingObject(mObj);
            }

            lock (drawingLock)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;

                foreach (MovingObject mObj in playerShipList)
                    drawMovingObject(mObj);
            }

            lock (drawingLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                foreach (MovingObject mObj in enemyShipList)
                    drawMovingObject(mObj);
            }

            if(powerUp != null)
            {
                lock (drawingLock)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    drawMovingObject(powerUp);
                }
            }
        }

        static void eraseObject(MovingObject mObj)
        {
            double x = mObj.getPosition().X;
            double y = mObj.getPosition().Y;
            double size = screenWidth * mObj.getSize();
            int drawingX = (int)Math.Round(screenWidth * x - size / 2, MidpointRounding.AwayFromZero);
            int drawingY = (int)Math.Round(screenHeight * y - size / 2, MidpointRounding.AwayFromZero);
            double XstartingPoint = Math.Round(screenWidth * x - size / 2, MidpointRounding.AwayFromZero);
            double YstartingPoint = Math.Round(screenHeight * y - size / 2, MidpointRounding.AwayFromZero);

            for (int i = 0; i < (int)size; i++)
            {
                drawingX = (int)XstartingPoint + i;
                if (drawingX <= 0 || drawingX >= screenWidth - 1)
                    continue;

                for (int j = 0; j < (int)size; j++)
                {
                    drawingY = (int)YstartingPoint + j;
                    if (drawingY <= 0 || drawingY >= screenHeight - 1)
                        continue;

                    Console.SetCursorPosition(drawingX, drawingY);
                    Console.Write(' ');
                }
            }
        }

        static void calculateEnemyMovement()
        {
            int iCount = enemyShipList.Count;

            for (int i = 0; i < iCount; i++)
            {
                double x = enemyShipList[i].getPosition().X;
                double y = enemyShipList[i].getPosition().Y;

                enemyShipList[i].setPreviousPosition(x, y);

                double velX = enemyShipList[i].getVelocity().X;
                double velY = enemyShipList[i].getVelocity().Y;

                double newX = x + velX;
                double newY = y + velY;

                if (enemyShipList[i].GetType() == typeof(EnemyShip))
                    if (newX > 1 || newX < 0)
                      enemyShipList[i].setVelocity(0 - velX, velY);

                enemyShipList[i].setPosition(newX, newY);

                if(newY > 1.1 || newY < -0.2)
                {
                    enemyShipList.RemoveAt(i);
                    i--;
                    iCount--;
                }
            }
        }

        static void calculatePlayerMovement()
        {
            PlayerShip mObj = (PlayerShip)playerShipList[0];

            if (playerXMovement == -1)
                mObj.moveLeft();
            if (playerXMovement == 1)
                mObj.moveRight();
            if (playerYMovement == -1)
                mObj.moveUp();
            if (playerYMovement == 1)
                mObj.moveDown();

            double x = mObj.getPosition().X;
            double y = mObj.getPosition().Y;
            double xVel = mObj.getVelocity().X;
            double yVel = mObj.getVelocity().Y;
            double speed = mObj.getSpeed();

            mObj.setPreviousPosition(x, y);

            double newX = x + xVel;
            double newY = y + yVel;

            mObj.setVelocity(xVel * 0.9, yVel * 0.9);

            if (xVel < 0.002 && xVel > -0.002)
                xVel = 0;
            if (yVel < 0.002 && yVel > -0.002)
                yVel = 0;

            if (newX < 0)
                newX = 0;
            else if (newX > 1)
                newX = 1;
            if (newY < 0)
                newY = 0;
            else if (newY > 1)
                newY = 1;

            mObj.setPosition(newX, newY);

            for(int i = 1; i < playerShipList.Count; i++)
            {
                x = playerShipList[i].getPosition().X;
                y = playerShipList[i].getPosition().Y;

                playerShipList[i].setPreviousPosition(x, y);

                double velX = playerShipList[i].getVelocity().X;
                double velY = playerShipList[i].getVelocity().Y;

                newX = x + velX;
                newY = y + velY;

                playerShipList[i].setPosition(newX, newY);
            }

            if (powerUp != null)
            {
                x = powerUp.getPosition().X;
                y = powerUp.getPosition().Y;

                powerUp.setPreviousPosition(x, y);

                double velX = powerUp.getVelocity().X;
                double velY = powerUp.getVelocity().Y;

                newX = x + velX;
                newY = y + velY;

                powerUp.setPosition(newX, newY);
            }
        }

        static void calculateSceneryMovement()
        {
            int iCount = sceneryObjectList.Count;

            for (int i = 0; i < iCount; i++ )
            {
                double x = sceneryObjectList[i].getPosition().X;
                double y = sceneryObjectList[i].getPosition().Y;

                sceneryObjectList[i].setPreviousPosition(x, y);

                double velX = sceneryObjectList[i].getVelocity().X;
                double velY = sceneryObjectList[i].getVelocity().Y;

                double newX = x + velX;
                double newY = y + velY;

                sceneryObjectList[i].setPosition(newX, newY);

                if (newY > 1.1)
                {
                    sceneryObjectList.RemoveAt(i);
                    i--;
                    iCount--;
                }
            }
        }
       
        static void checkCollisions()
        {
            int iCount = playerShipList.Count;
            int jCount = enemyShipList.Count;

            if (powerUp != null)
                if (powerUp.getPosition().Y > 1)
                    powerUp = null;

            for (int i = 0; i < iCount; i++)
            {
                if (powerUp != null && i == 0)
                {
                    double size1 = playerShipList[i].getSize();
                    double size2 = powerUp.getSize();
                    double pos1X = playerShipList[i].getPosition().X;
                    double pos1Y = playerShipList[i].getPosition().Y;
                    double pos2X = powerUp.getPosition().X;
                    double pos2Y = powerUp.getPosition().Y;

                    if ((Math.Abs(pos1X - pos2X) < size1 - 0.0125 || Math.Abs(pos1X - pos2X) < size2 - 0.0125) &&
                           (Math.Abs(pos1Y - pos2Y) < size1 - 0.025 || Math.Abs(pos1Y - pos2Y) < size2 - 0.025))
                    {
                        ((PlayerShip)playerShipList[i]).upgrade(powerUp.getShape());
                        eraseObject(powerUp);
                        powerUp = null;
                    }
                }

                for(int j = 0; j < jCount; j++)
                {
                    double size1 = playerShipList[i].getSize();
                    double size2 = enemyShipList[j].getSize();
                    double pos1X = playerShipList[i].getPosition().X;
                    double pos1Y = playerShipList[i].getPosition().Y;
                    double pos2X = enemyShipList[j].getPosition().X;
                    double pos2Y = enemyShipList[j].getPosition().Y;

                    if ((Math.Abs(pos1X - pos2X) < size1 - 0.0125 || Math.Abs(pos1X - pos2X) < size2 - 0.0125) &&
                        (Math.Abs(pos1Y - pos2Y) < size1 - 0.025 || Math.Abs(pos1Y - pos2Y) < size2 - 0.025))
                    {
                        if (i == 0)
                        {
                            playerDead = true;
                        }
                        else
                        {
                            if (enemyShipList[j].GetType() == typeof(EnemyShip) && playerShipList[i].GetType() == typeof(Bullet))
                                if (((EnemyShip)enemyShipList[j]).takeDamage(((Bullet)playerShipList[i]).getDamage()) <= 0)
                                {
                                    score += 5;
                                    eraseObject(enemyShipList[j]);
                                    enemyShipList.RemoveAt(j);
                                    j--;
                                    jCount--;
                                }

                            eraseObject(playerShipList[i]);
                            playerShipList.RemoveAt(i);
                            i--;
                            iCount--;
                        }
                    }
                }
            }
        }
         
        static void initializeGame()
        {
            char[] newPlayerSkin =
            {
                ' ', ' ', 'O', ' ', ' ',
                ' ', 'O', 'O', 'O', ' ',
                ' ', 'O', 'O', 'O', ' ',
                'O', 'O', 'O', 'O', 'O',
                ' ', 'O', 'O', 'O', ' '
            };
            playerSkins.Add(newPlayerSkin);

            char[] newEnemySkin1 =
            {
                ' ', 'X', 'X', 'X', ' ',
                'X', 'X', ' ', 'X', 'X',
                'X', ' ', 'X', ' ', 'X',
                'X', 'X', ' ', 'X', 'X',
                ' ', 'X', ' ', 'X', ' '
            };
            enemySkins.Add(newEnemySkin1);

            char[] newEnemySkin2 =
            {
                ' ', 'X', ' ',
                'X', 'X', 'X',
                'X', ' ', 'X'
            };
            enemySkins.Add(newEnemySkin2);

            char[] newSkin =
            {
                '╔', '═', '╗',
                '║', ' ', '║',
                '╚', '═', '╝'
            };
            otherSkins.Add(newSkin);


            lock (drawingLock)
            {
                foreach (MovingObject mObj in enemyShipList)
                {
                    eraseObject(mObj);
                }

                foreach (MovingObject mObj in playerShipList)
                {
                    eraseObject(mObj);
                }

                enemyShipList.Clear();
                playerShipList.Clear();
            }

            gameStart = false;
            playerDead = false;
            newScoreSaved = false;
            playerXMovement = 0;
            playerYMovement = 0;
            score = 0;

            playerShipList.Add(new PlayerShip(5, playerSkins[0]));
            drawMovingObject(playerShipList[0]);
        }

        static Task keyboardListener()
        {
            ConsoleKeyInfo cki;
            PlayerShip ship = (PlayerShip)playerShipList[0];

            while(true)
            {
                while (Console.KeyAvailable == false)
                    Thread.Sleep(10);

                lock(keyboardLock)
                {
                    cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.LeftArrow || cki.Key == ConsoleKey.A)
                    {
                        if (playerXMovement > -1)
                            playerXMovement--;
                    }
                    if (cki.Key == ConsoleKey.RightArrow || cki.Key == ConsoleKey.D)
                    {
                        if (playerXMovement < 1)
                            playerXMovement++;
                    }
                    if (cki.Key == ConsoleKey.UpArrow || cki.Key == ConsoleKey.W)
                    {
                        if (playerYMovement > -1 && gameStart)
                            playerYMovement--;

                        if (!gameStart)
                            menu.selectUp();
                    }
                    if (cki.Key == ConsoleKey.DownArrow || cki.Key == ConsoleKey.S)
                    {
                        if (playerYMovement < 1 && gameStart)
                            playerYMovement++;

                        if (!gameStart)
                            menu.selectDown();
                    }
                    if (cki.Key == ConsoleKey.Spacebar || cki.Key == ConsoleKey.Enter)
                    {
                        if (togglePlayerFire)
                            togglePlayerFire = false;
                        else
                            togglePlayerFire = true;

                        if (!gameStart)
                            menu.select();
                    }
                    if (cki.Key == ConsoleKey.Escape)
                    {
                        playerDead = false;

                        if (gameStart)
                            gameStart = false;
                        else
                            gameRunning = false;
                    }
                }
            }
        }

        static string getPlayerName()
        {
            lock(keyboardLock)
            {
                ConsoleKeyInfo cki;
                string name = "";

                lock (drawingLock)
                {
                    Console.SetCursorPosition(screenWidth / 2 - 12 / 2 + name.Count(), 10 + (int)(screenHeight / 2.5));
                    Console.Write("_");
                }

                while (true)
                {
                    while (Console.KeyAvailable == false)
                        Thread.Sleep(10);

                    Console.SetCursorPosition(screenWidth / 2 - 12 / 2 + name.Count(), 10 + (int)(screenHeight / 2.5));
                    if (name.Count() < 14)
                        cki = Console.ReadKey();
                    else
                        cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Spacebar || cki.Key == ConsoleKey.Enter)
                        return name;
                    else if (cki.Key != ConsoleKey.Backspace && name.Count() < 14)
                    {
                        name += cki.KeyChar;

                        lock (drawingLock)
                        {
                            Console.SetCursorPosition(screenWidth / 2 - 12 / 2 + name.Count(), 10 + (int)(screenHeight / 2.5));
                            if (name.Count() < 14)
                                Console.Write("_");
                        }
                    }
                    else if (cki.Key == ConsoleKey.Backspace && name.Count() > 0)
                    {
                        lock (drawingLock)
                        {
                            Console.SetCursorPosition(screenWidth / 2 - 12 / 2 + name.Count() - 1, 10 + (int)(screenHeight / 2.5));
                            Console.Write("_ ");
                            name = name.Remove(name.Count() - 1);
                        }
                    }
                }
            }
        }

        static void readHighscores(List<string> listNames, List<int> listScores)
        {
            string[]  highscores;

            if (File.Exists("highscores.txt"))
                highscores = File.ReadAllLines("highscores.txt");
            else
            {
                var fileHighscores = File.Create("highscores.txt");
                fileHighscores.Close();
                return;
            }

            for(int i = 0; i < highscores.Count(); i++)
            {
                int tempInt;
                string[] tempString = highscores[i].Split(null);
                int.TryParse(tempString[0], out tempInt);
                listScores.Add(tempInt);
                listNames.Add(tempString[1]);
            }
        }

        static void saveHighscores(List<string> listNames, List<int> listScores)
        {
            string[] highscores = new string[listNames.Count];

            for (int i = 0; i < listNames.Count(); i++)
            {
                string tempString = listScores[i] + " " + listNames[i];
                highscores[i] = tempString;
            }

            var fileHighscores = File.Create("highscores.txt");
            fileHighscores.Close();
            File.WriteAllLines("highscores.txt", highscores);
        }

        static bool checkHighscores(int value)
        {
            if (highscoresScores.Count < 9)
                return true;

            for(int i = 0; i < highscoresScores.Count(); i++)
            {
                if (value > highscoresScores[i])
                    return true;
            }

            return false;
        }

        static void addHighscore(int value, string name)
        {
            for (int i = 0; i < highscoresScores.Count(); i++)
            {
                if (value > highscoresScores[i])
                {
                    highscoresScores.Insert(i, value);
                    highscoresNames.Insert(i, name);

                    if (highscoresScores.Count > 9)
                    {
                        highscoresScores.RemoveAt(9);
                        highscoresNames.RemoveAt(9);
                    }

                    return;
                }
            }

            highscoresScores.Add(value);
            highscoresNames.Add(name);
        }

        static void Main(string[] args)
        {
            Console.BufferHeight = consoleHeight;
            Console.BufferWidth = consoleWidth;
            Console.WindowHeight = consoleHeight;
            Console.WindowWidth = consoleWidth;
            Console.CursorVisible = false;
            Console.Title = "Fireship";
            menu = Menu.Create();
            readHighscores(highscoresNames, highscoresScores);
            int sleepTime = 0;
            int maxEnemiesCount = 0;
            int frameCounter = 0;

            initializeGame();
            Task.Run(keyboardListener);

            while (gameRunning)
            {
                stopWatch.Start();
                frameCounter++;
                
                drawAllObjects();
                Console.ForegroundColor = ConsoleColor.Gray;

                if (frameCounter == 10)
                    sceneryObjectList.Add(new SceneryObject());

                if(!gameStart)
                {
                    if (!showHighScores)
                    {
                        menu.drawTitle();
                        menu.drawMenu();
                    }
                    else
                    {
                        menu.drawTitle();
                        menu.drawHighscores(highscoresNames, highscoresScores);
                    }

                    Thread.Sleep(10);
                }
                else
                {
                    if (!playerDead)
                    {
                        // in-game logic - random enemy movement etc. --------------------
                        if (frameCounter == 10)
                        {
                            if (maxEnemiesCount < 6)
                                maxEnemiesCount = 1 + (int)(gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 3000));

                            for (int i = 0; i < maxEnemiesCount; i++)
                                enemyShipList.Add(new EnemyShip(difficultyTimeIncrement, 3, enemySkins[1]));
                        }

                        if (frameCounter == 20 && enemyShipList.Count != 0)
                        {
                            double maxMoveSpeed = 0;

                            if (enemyShipList[0] != null)
                                maxMoveSpeed = enemyShipList[0].getMaxVelocity();

                            for (int i = 0; i < enemyShipList.Count; i++)
                            {
                                if (enemyShipList[i].GetType() == typeof(PlayerShip) || enemyShipList[i].GetType() == typeof(Bullet))
                                    continue;

                                if (enemyShipList[i].getVelocity().X > 0)
                                    enemyShipList[i].moveLeft(rnd.NextDouble() % (maxMoveSpeed / 4));
                                else
                                    enemyShipList[i].moveRight(rnd.NextDouble() % (maxMoveSpeed / 4));

                                if (rnd.NextDouble() < (double)gameTime.ElapsedMilliseconds / (difficultyTimeIncrement * 1000) / 100)
                                    ((EnemyShip)enemyShipList[i]).fire();
                            }
                        }

                        if(frameCounter == 25)
                        {
                            if (powerUp == null)
                            {
                                if ((double)gameTime.ElapsedMilliseconds % (difficultyTimeIncrement * 1000) > difficultyTimeIncrement * 800)
                                    powerUpAvailable = true;

                                if (powerUpAvailable)
                                {
                                    powerUp = new PowerUp(otherSkins[0], (PlayerShip)playerShipList[0]);
                                    powerUpAvailable = false;
                                }
                            }
                        }

                        if (togglePlayerFire == true)
                        {
                            ((PlayerShip)playerShipList[0]).fire();
                        }


                    
                        checkCollisions();
                        calculateEnemyMovement();

                        timeScore = (int)gameTime.ElapsedMilliseconds / 1000;

                        lock (drawingLock)
                        {
                            Console.SetCursorPosition(1, consoleHeight - 2);
                            Console.Write("Score: " + (score + timeScore));
                        }


                        // ------------------------------------------
                    }
                    else
                    {
                        menu.drawGameOver(score + timeScore);
                        if (checkHighscores(score + timeScore))
                        {
                            menu.drawNewHighscore();
                            if (!newScoreSaved)
                            {
                                string newName = getPlayerName();

                                lock (drawingLock)
                                {
                                    Console.SetCursorPosition(screenWidth / 2 - 12 / 2, 10 + (int)(screenHeight / 2.5));
                                    Console.Write("                 ");
                                }

                                addHighscore(score + timeScore, newName);
                                saveHighscores(highscoresNames, highscoresScores);
                                newScoreSaved = true;
                            }
                        }
                        else
                            Console.ReadKey(true);
                        
                        initializeGame();
                        menu.eraseGameOver();

                        lock (drawingLock)
                        {
                            Console.SetCursorPosition(1, consoleHeight - 2);
                            Console.Write("                 ");
                        }
                    }

                }

                if(!playerDead)
                    calculatePlayerMovement();
                calculateSceneryMovement();

                sleepTime = frameTime - (int)stopWatch.ElapsedMilliseconds;
                if(sleepTime > 0)
                    Thread.Sleep(sleepTime);

                if (frameCounter == framesPerSecondTop)
                {
                    frameCounter = 0;

                    lock(drawingLock)
                    {
                        Console.SetCursorPosition(1, 1);
                        Console.Write("FPS: " + (1000 / stopWatch.ElapsedMilliseconds + 1) + " ");
                    }
                }

                stopWatch.Reset();
            }
        }
    }
}
