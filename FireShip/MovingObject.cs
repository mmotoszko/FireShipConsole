using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FireShip
{
    class MovingObject
    {
        protected char[] image;
        protected Position position;
        protected Position previousPosition;
        protected Velocity velocity;
        protected double maxVelocity;
        protected double size;

        public char[] getImage()
        {
            return image;
        }

        public Position getPosition()
        {
            return position;
        }

        public void setPosition(double x, double y)
        {
            position.X = x;
            position.Y = y;
        }

        public Position getPreviousPosition()
        {
            return previousPosition;
        }

        public void setPreviousPosition(double x, double y)
        {
            previousPosition.X = x;
            previousPosition.Y = y;
        }

        public Velocity getVelocity()
        {
            return velocity;
        }

        public void setVelocity(double x, double y)
        {
            velocity.X = x;
            velocity.Y = y;
        }

        public double getSize()
        {
            return size;
        }

        public void setSize(double newSize)
        {
            size = newSize;
        }

        public double getMaxVelocity()
        {
            return maxVelocity;
        }

        public void moveUp(double moveVelocity)
        {
            velocity.Y -= moveVelocity;
            if (velocity.Y > maxVelocity)
                velocity.Y = maxVelocity;
        }

        public void moveDown(double moveVelocity)
        {
            velocity.Y += moveVelocity;
            if (velocity.Y < 0 - maxVelocity)
                velocity.Y = 0 - maxVelocity;
        }

        public void moveLeft(double moveVelocity)
        {
            velocity.X -= moveVelocity;
            if (velocity.X > maxVelocity)
                velocity.X = maxVelocity;
        }

        public void moveRight(double moveVelocity)
        {
            velocity.X += moveVelocity;
            if (velocity.X < 0 - maxVelocity)
                velocity.X = 0 - maxVelocity;
        }
    }

    class EnemyShip : MovingObject
    {
        int hitPoints;

        public EnemyShip(int enemyLDifficultyifeIncrementInSeconds, int enemySize, char[] img)
        {
            double tempPosX = Program.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = 0;
            velocity.X = 0;
            velocity.Y = 0.001 + 0.0006 * (int)Program.gameTime.ElapsedMilliseconds / (enemyLDifficultyifeIncrementInSeconds * 1000);
            maxVelocity = 0.01;

            hitPoints = 1 + (int)Program.gameTime.ElapsedMilliseconds / (enemyLDifficultyifeIncrementInSeconds * 2000);
            size = 0.0125 * enemySize;
            image = img;
        }

        public void fire() { Bullet.Fire(this); }
        public int getHitpoints() { return hitPoints; }

        public int takeDamage(int damage)
        {
            hitPoints -= damage;
            return hitPoints;
        }
    }

    class PlayerShip : MovingObject
    {
        int power;
        double fireRate;
        Stopwatch shotCooldown;
        double speed;
        int multipleBullets;
        double moveVelocity;

        public PlayerShip(int playerSize, char[] img)
        {
            position.X = 0.5 - 0.0125;
            position.Y = 0.8;
            previousPosition.X = -1;
            previousPosition.Y = -1;
            velocity.X = 0;
            velocity.Y = 0;
            maxVelocity = 0.005;
            moveVelocity = 0.002;
            size = 0.0125 * playerSize;
            image = img;

            power = 1;
            fireRate = 1;
            shotCooldown = new Stopwatch();
            shotCooldown.Start();
            speed = 1;
            multipleBullets = 1;
        }

        public int getPower() { return power; }
        public double getFirerate() { return fireRate; }
        public double getSpeed() { return speed; }
        public int getMultipleBullets() { return multipleBullets; }

        public void addPower() { power++; }
        public void addFirerate() { fireRate++; }
        public void addSpeed() { speed++; }
        public void addMultipleBullets() { multipleBullets++; }

        public void moveUp()
        {
            velocity.Y -= moveVelocity * speed / 2;
            if (velocity.Y < 0 - maxVelocity * speed)
                velocity.Y = 0 - maxVelocity * speed;
        }

        public void moveDown()
        {
            velocity.Y += moveVelocity * speed / 2;
            if (velocity.Y > maxVelocity * speed)
                velocity.Y = maxVelocity * speed;
        }

        public void moveLeft()
        {
            velocity.X -= moveVelocity * speed / 2;
            if (velocity.X < 0 - maxVelocity * speed)
                velocity.X = 0 - maxVelocity * speed;
        }

        public void moveRight()
        {
            velocity.X += moveVelocity * speed / 2;
            if (velocity.X > maxVelocity * speed)
                velocity.X = maxVelocity * speed;
        }

        public void fire()
        {
            if (shotCooldown.ElapsedMilliseconds >= 1000 / fireRate)
            {
                shotCooldown.Restart();
                Bullet.Fire(this);
            }
        }

        public void upgrade(char c)
        {
            switch (c)
            {
                case 'P':
                    power++;
                    break;
                case 'F':
                    fireRate += 0.3;
                    break;
                case 'S':
                    speed += 0.2;
                    break;
                case 'M':
                    multipleBullets++;
                    break;
                default: break;
            }
        }
    }

    class Bullet : MovingObject
    {
        protected int damage;
        protected char type;

        private Bullet(Position position, Velocity velocity, int damage, char type)
        {
            this.position.X = position.X;
            this.position.Y = position.Y;
            previousPosition.X = -1;
            previousPosition.Y = -1;
            this.velocity.X = velocity.X;
            this.velocity.Y = velocity.Y;
            maxVelocity = 0.04;
            size = 0.0125;
            this.damage = damage;
            this.type = type;
        }

        public int getDamage() { return damage; }
        public char getShape() { return type; }

        static public void Fire(MovingObject ship)
        {
            if (ship.GetType() == typeof(PlayerShip))
            {
                PlayerShip pShip = (PlayerShip)ship;
                Position bulletPosition = new Position(ship.getPosition(), 0, 0 - (ship.getSize() + 0.0125) / 2);
                int bulletCount = pShip.getMultipleBullets();

                if (bulletCount >= 1)
                    Program.playerShipList.Add(new Bullet(bulletPosition, new Velocity(0, -0.01), pShip.getPower(), '|'));
                if (bulletCount >= 2)
                    Program.playerShipList.Add(new Bullet(bulletPosition, new Velocity(-0.007, -0.007), pShip.getPower(), '\\'));
                if (bulletCount >= 3)
                    Program.playerShipList.Add(new Bullet(bulletPosition, new Velocity(0.007, -0.007), pShip.getPower(), '/'));
                if (bulletCount >= 4)
                    Program.playerShipList.Add(new Bullet(bulletPosition, new Velocity(0.01, 0), pShip.getPower(), '¯'));
                if (bulletCount >= 5)
                    Program.playerShipList.Add(new Bullet(bulletPosition, new Velocity(-0.01, 0), pShip.getPower(), '¯'));
            }
            else
            {
                EnemyShip pShip = (EnemyShip)ship;
                Position bulletPosition = new Position(ship.getPosition(), 0, (ship.getSize() + 0.0125) / 2);
                Program.enemyShipList.Add(new Bullet(bulletPosition, new Velocity(0, 0.01), 1, '|'));
            }
        }


    }

    class SceneryObject : MovingObject
    {
        protected char shape;

        public SceneryObject()
        {
            double tempPosX = Program.rnd.NextDouble();
            if (tempPosX < 0.02)
                tempPosX += 0.02;
            else
            if (tempPosX > 0.98)
                tempPosX -= 0.02;

            position.X = tempPosX;
            position.Y = 0;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = 0;
            velocity.X = 0;
            maxVelocity = 0.01;
            size = 0.0125;

            double randomShape = Program.rnd.NextDouble();
            if (randomShape < 0.8)
            {
                shape = '.';
                velocity.Y = 0.0005;
            }
            else
            if (randomShape < 0.9)
            {
                shape = '■';
                velocity.Y = 0.002;
            }
        }

        public SceneryObject(char[] img)
        {
            double tempPosX = Program.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = 0;
            velocity.X = 0;
            velocity.Y = 0.0003;
            maxVelocity = 0.01;

            shape = ' ';
            size = 0.0125 * img.Count();
            image = img;
        }

        public char getShape() { return shape; }
    }

    class PowerUp : MovingObject
    {
        protected char shape;

        public PowerUp(char[] image, PlayerShip ship)
        {
            double tempPosX = Program.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = 0;
            velocity.X = 0;
            velocity.Y = 0.002;
            maxVelocity = 0.01;
            size = 0.0125 * 3;
            this.image = image;

            double value = Program.rnd.NextDouble();

            if (value < 0.2)
                shape = 'S';
            else if (value < 0.5)
            {
                if (ship.getMultipleBullets() < 5)
                    shape = 'M';
                else
                    shape = 'P';
            }
            else if (value < 0.8)
                shape = 'F';
            else if (value <= 1)
                shape = 'P';
        }

        public PowerUp(char shape, char[] image)
        {
            double tempPosX = Program.rnd.NextDouble();
            if (tempPosX < 0.1)
                tempPosX += 0.1;
            else
            if (tempPosX > 0.9)
                tempPosX -= 0.1;

            position.X = tempPosX;
            position.Y = 0;
            previousPosition.X = tempPosX - 1;
            previousPosition.Y = 0;
            velocity.X = 0;
            velocity.Y = 0.002;
            maxVelocity = 0.01;
            size = 0.0125 * 3;
            this.shape = shape;
            this.image = image;
        }

        public char getShape() { return shape; }
    }

    class Menu
    {
        private static Menu instance;
        protected List<string> menuList = new List<string>();
        protected Position position;
        protected List<string> titleImage = new List<string>();
        protected int selection;
        protected int previousSelection;
        protected bool selectionChanged;

        private Menu()
        {
            menuList.Add("S t a r t");
            menuList.Add("H i g h s c o r e s");
            menuList.Add("E x i t");

            position.X = Program.screenWidth / 2;
            position.Y = Program.screenHeight / 2;
            selection = 0;
            selectionChanged = false;

            titleImage.Add(" ________ ___  ________  _______   ________  ___  ___  ___  ________   ");
            titleImage.Add("|\\  _____\\\\  \\|\\   __  \\|\\  ___ \\ |\\   ____\\|\\  \\|\\  \\|\\  \\|\\   __  \\  ");
            titleImage.Add("\\ \\  \\__/\\ \\  \\ \\  \\|\\  \\ \\   __/|\\ \\  \\___|\\ \\  \\\\\\  \\ \\  \\ \\  \\|\\  \\ ");
            titleImage.Add(" \\ \\   __\\\\ \\  \\ \\   _  _\\ \\  \\_|/_\\ \\_____  \\ \\   __  \\ \\  \\ \\   ____\\");
            titleImage.Add("  \\ \\  \\_| \\ \\  \\ \\  \\\\  \\\\ \\  \\_|\\ \\|____|\\  \\ \\  \\ \\  \\ \\  \\ \\  \\___|");
            titleImage.Add("   \\ \\__\\   \\ \\__\\ \\__\\\\ _\\\\ \\_______\\____\\_\\  \\ \\__\\ \\__\\ \\__\\ \\__\\   ");
            titleImage.Add("    \\|__|    \\|__|\\|__|\\|__|\\|_______|\\_________\\|__|\\|__|\\|__|\\|__|   ");
            titleImage.Add("                                     \\|_________|                      ");


        }

        public static Menu Create()
        {
            if (instance == null)
                instance = new Menu();

            return instance;
        }

        public int getOptionsCount() { return menuList.Count; }
        public int getTitleHeight() { return titleImage.Count; }

        public void selectUp()
        {
            previousSelection = selection;

            selection--;
            if (selection < 0)
                selection = menuList.Count - 1;

            selectionChanged = true;
        }

        public void selectDown()
        {
            previousSelection = selection;

            selection++;
            selection = selection % menuList.Count;

            selectionChanged = true;
        }

        public void select()
        {
            if(Program.showHighScores)
            {
                Program.showHighScores = false;
                eraseHighScores();
                return;
            }

            switch (selection)
            {
                case 0:
                    Program.gameStart = true;
                    eraseTitle();
                    eraseMenu();
                    Program.gameTime.Restart();
                    break;
                case 1:
                    Program.showHighScores = true;
                    eraseMenu();
                    break;
                case 2:
                    Program.gameRunning = false;
                    break;
                default: break;
            }
        }

        public void drawTitle()
        {
            int titleWidth = titleImage[0].Count();
            int titleHeight = titleImage.Count;

            lock (Program.drawingLock)
            {
                Console.ForegroundColor = ConsoleColor.Gray;

                for (int i = 0; i < titleHeight; i++)
                {
                    Console.SetCursorPosition((Program.screenWidth - titleWidth) / 2, i + (Program.screenHeight - titleHeight) / 7);
                    Console.Write(titleImage[i]);
                }
            }
        }

        public void drawMenu()
        {
            lock (Program.drawingLock)
            {
                for (int i = 0; i < menuList.Count; i++)
                {
                    if (i == selection)
                    {
                        Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 5, i * 3 + (int)(Program.screenHeight / 2.5));
                        Console.Write(">");
                        Console.SetCursorPosition((Program.screenWidth - 20) / 2 + (19 - menuList[i].Count()) / 2, i * 3 + (int)(Program.screenHeight / 2.5));
                        Console.Write(menuList[i]);
                        Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 23, i * 3 + (int)(Program.screenHeight / 2.5));
                        Console.Write("<");
                    }
                    else
                    {
                        if (selectionChanged)
                        {
                            Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 5, previousSelection * 3 + (int)(Program.screenHeight / 2.5));
                            Console.Write(" ");
                            Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 23, previousSelection * 3 + (int)(Program.screenHeight / 2.5));
                            Console.Write(" ");
                            selectionChanged = false;
                        }
                        Console.SetCursorPosition((Program.screenWidth - 20) / 2 + (19 - menuList[i].Count()) / 2, i * 3 + (int)(Program.screenHeight / 2.5));
                        Console.Write(menuList[i]);
                    }
                }
            }
        }

        public void eraseTitle()
        {
            lock (Program.drawingLock)
            {
                int titleWidth = titleImage[0].Count();
                int titleHeight = titleImage.Count;

                Console.ForegroundColor = ConsoleColor.Gray;

                for (int i = 0; i < titleHeight; i++)
                {
                    Console.SetCursorPosition((Program.screenWidth - titleWidth) / 2, i + (Program.screenHeight - titleHeight) / 7);
                    for (int j = 0; j < titleWidth; j++)
                        Console.Write(" ");
                }
            }
        }

        public void eraseMenu()
        {
            lock (Program.drawingLock)
            {
                for (int i = 0; i < menuList.Count; i++)
                {
                    Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 5, i * 3 + (int)(Program.screenHeight / 2.5));
                    for (int j = 0; j < 29; j++)
                        Console.Write(" ");
                }
            }
        }

        public void drawGameOver(int score)
        {
            lock (Program.drawingLock)
            {
                Console.SetCursorPosition(Program.screenWidth / 2 - 17 / 2, (int)(Program.screenHeight / 2.5));
                Console.Write("G a m e   o v e r");
                Console.SetCursorPosition(Program.screenWidth / 2 - ( 8 + score.ToString().Count()) / 2, 3 + (int)(Program.screenHeight / 2.5));
                Console.Write("Score:  " + score);
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 3, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write(">");
                Console.SetCursorPosition(Program.screenWidth / 2 - 15 / 2, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write("C o n t i n u e");
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 23, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write("<");
            }
        }

        public void drawNewHighscore()
        {
            lock (Program.drawingLock)
            {
                Console.SetCursorPosition(Program.screenWidth / 2 - 12 / 2, 5 + (int)(Program.screenHeight / 2.5));
                Console.Write("New highscore!");
                Console.SetCursorPosition(Program.screenWidth / 2 - 8 / 2, 7 + (int)(Program.screenHeight / 2.5));
                Console.Write("Your name:");
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 3, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write(">");
                Console.SetCursorPosition(Program.screenWidth / 2 - 15 / 2, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write("C o n t i n u e");
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 23, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write("<");
            }
        }

        public void eraseGameOver()
        {
            lock (Program.drawingLock)
            {
                Console.SetCursorPosition(Program.screenWidth / 2 - 17 / 2, (int)(Program.screenHeight / 2.5));
                Console.Write("                 ");
                Console.SetCursorPosition(Program.screenWidth / 2 - 12 / 2, 3 + (int)(Program.screenHeight / 2.5));
                Console.Write("         ");
                Console.SetCursorPosition(Program.screenWidth / 2 - 12 / 2, 5 + (int)(Program.screenHeight / 2.5));
                Console.Write("              ");
                Console.SetCursorPosition(Program.screenWidth / 2 - 8 / 2, 7 + (int)(Program.screenHeight / 2.5));
                Console.Write("          ");
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 3, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write(" ");
                Console.SetCursorPosition(Program.screenWidth / 2 - 15 / 2, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write("               ");
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 23, 17 + (int)(Program.screenHeight / 2.5));
                Console.Write(" ");
            }
        }

        public void drawHighscores(List<string> names, List<int> scores)
        {
            lock (Program.drawingLock)
            {
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 1, (Program.screenHeight / 3) - 1);
                Console.Write("H i g h s c o r e s");

                int i;

                for (i = 0; i < scores.Count; i++)
                {
                    Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 2, 2 + i * 2 + (Program.screenHeight / 3));
                    Console.Write("{0}. ", i + 1);
                    Console.Write(names[i]);
                    Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 23 - scores[i].ToString().Count(), 2 + i * 2 + (Program.screenHeight / 3));
                    Console.Write(scores[i]);
                }


                Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 3, 6 + 9 * 2 + (Program.screenHeight / 3));
                Console.Write(">");
                Console.SetCursorPosition(Program.screenWidth / 2 - 15 / 2, 6 + 9 * 2 + (Program.screenHeight / 3));
                Console.Write("C o n t i n u e");
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 23, 6 + 9 * 2 + (Program.screenHeight / 3));
                Console.Write("<");

            }
        }

        public void eraseHighScores()
        {
            lock (Program.drawingLock)
            {
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 + 1, (Program.screenHeight / 3) - 1);
                Console.Write("                   ");

                int i;

                for (i = 0; i < 10; i++)
                {
                    Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 5, 2 + i * 2 + (Program.screenHeight / 3));
                    Console.Write("                              ");
                }
                
                Console.SetCursorPosition((Program.screenWidth - 20) / 2 - 4, 6 + 9 * 2 + (Program.screenHeight / 3));
                Console.Write("                              ");
            }
        }
    }
}
