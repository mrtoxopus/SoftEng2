using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using SqlCommand = System.Data.SqlClient.SqlCommand;
using SqlConnection = System.Data.SqlClient.SqlConnection;

namespace SoftEng2
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private bool moveLeft, moveRight;
        private readonly DispatcherTimer gameTimer = new();
        private readonly List<Rectangle> itemsToRemove = new();
        private const int PlayerSpeed = 10;
        private const int BulletSpeed = 20;

        private readonly Random rand = new();
        private int enemySpawnLimit = 50;
        private int score = 0;
        private int damage = 0;
        private int enemySpawnCounter = 100;
        private string player;
        private const int EnemySpeed = 10;
        private const int EnemyPassesDamage = 10;
        private const int EnemyCrashesDamage = 5;

        public GameWindow()
        {
            InitializeComponent();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameEngine;
            gameTimer.Start();

            MyCanvas.Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }

            if (e.Key == Key.Right)
            {
                moveRight = true;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }

            if (e.Key == Key.Right)
            {
                moveRight = false;
            }

            if (e.Key == Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "Bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };
                Canvas.SetTop(newBullet, Canvas.GetTop(Player) - newBullet.Height);
                Canvas.SetLeft(newBullet, Canvas.GetLeft(Player) + Player.Width / 2);
                MyCanvas.Children.Add(newBullet);
            }
        }
        private void GameEngine(object sender, EventArgs e)
        {
            if (moveLeft && Canvas.GetLeft(Player) > 0)
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - PlayerSpeed);

            if (moveRight && Canvas.GetLeft(Player) + Player.Width < Application.Current.MainWindow.Width)
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + PlayerSpeed);

            foreach (Rectangle x in MyCanvas.Children.OfType<Rectangle>())
            {
             Rect playerHitBox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);   
             if ((string)x.Tag == "Bullet")
             {
                 Canvas.SetTop(x, Canvas.GetTop(x) - BulletSpeed);
                 Rect bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                 if (Canvas.GetTop(x) < 10)
                     itemsToRemove.Add(x);
                 foreach (Rectangle y in MyCanvas.Children.OfType<Rectangle>())
                 {
                     if ((string)y.Tag == "Enemy")
                     {
                         Rect enemy = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);

                         if (bullet.IntersectsWith(enemy))
                         {
                             itemsToRemove.Add(x);
                             itemsToRemove.Add(y);
                             score++;
                         }
                     }
                 }
             }

             if ((string)x.Tag == "Enemy")
             {
                 Canvas.SetTop(x, Canvas.GetTop(x) + EnemySpeed);
                 Rect enemy = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                 if (Canvas.GetTop(x) + 50 > 900)
                 {
                     itemsToRemove.Add(x);
                     damage += EnemyPassesDamage;
                 }

                 if (playerHitBox.IntersectsWith(enemy))
                 {
                     damage += EnemyCrashesDamage;
                     itemsToRemove.Add(x);
                 }
             }
            }
            foreach (Rectangle r in itemsToRemove)
            {
                MyCanvas.Children.Remove(r);
            }
            
            enemySpawnCounter--;

            if (enemySpawnCounter < 0)
            {
                MakeEnemies();
                enemySpawnCounter = enemySpawnLimit;
            }

            LabelScore.Content = "Score: " + score;
            LabelDamage.Content = "Damage: " + damage;

            if (score > 5)
            {
                enemySpawnLimit = 20;
            }

            if (damage > 99)
            {
               gameTimer.Stop();
               LabelDamage.Content = "Damage: 100";
               LabelDamage.Foreground = Brushes.Red;
               MessageBox.Show("You have destroyed " + score + "ships", "Game Over");
            }
        }

        /*private void MakeEnemies()
        {
            ImageBrush enemySprite = new ImageBrush();
            int enemySpriteCounter = rand.Next(1, 3);
     

            switch (enemySpriteCounter)
            {
                case 1:
 
                    break;
                case 2:
                    
                    break;
                case 3:
                    
                    break;
                default:
                    enemySprite.ImageSource = null;
                    enemySprite.Fill = Brushes.YellowGreen;
                    break;
            }
            

            Rectangle newEnemy = new Rectangle
            {
                Tag = "Enemy",
                Height = 40,
                Width = 50,
                Fill = enemySprite
            };

            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, rand.Next(30, 430));
            MyCanvas.Children.Add(newEnemy);

            GC.Collect();
        }
           */
        private void MakeEnemies()
        {
            int enemySpriteCounter = rand.Next(1, 4); // Change the range to include all cases

            switch (enemySpriteCounter)
            {
                case 1:
                    // Load an image for enemy type 1
                    LoadEnemyImage("pack://application:,,,/Images/B.png");
                    break;
                case 2:
                    // Load an image for enemy type 2
                    LoadEnemyImage("pack://application:,,,/Images/EnemyType2.png");
                    break;
                case 3:
                    // Load an image for enemy type 3
                    LoadEnemyImage("pack://application:,,,/Images/EnemyType3.png");
                    break; 
                default:
                    // Create a yellow rectangle for the default case
                    CreateDefaultEnemy();
                    break;
            }
        }

        private void LoadEnemyImage(string imagePath)
        {
            ImageBrush enemySprite = new ImageBrush();
            enemySprite.ImageSource = new BitmapImage(new Uri(imagePath));

            Rectangle newEnemy = new Rectangle
            {
                Tag = "Enemy",
                Height = 40,
                Width = 50,
                Fill = enemySprite
            };

            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, rand.Next(30, 430));
            MyCanvas.Children.Add(newEnemy);

            GC.Collect();
        }

        private void CreateDefaultEnemy()
        {
            Rectangle newEnemy = new Rectangle
            {
                Tag = "Enemy",
                Height = 40,
                Width = 50,
                Fill = Brushes.Yellow // Create a yellow rectangle
            };

            Canvas.SetTop(newEnemy, -100);
            Canvas.SetLeft(newEnemy, rand.Next(30, 430));
            MyCanvas.Children.Add(newEnemy);

            GC.Collect();
        }




        private void AddHighscoreToDatabase(int highscore)
        {
            string connectionString =
                "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\dtoxo\\1coderen\\Software Engineering 1\\SoftEng2\\Data\\GameDatabase.mdf\";Integrated Security=True";

            string query = "INSERT INTO [Highscores] ([Highscore],[Player],[Date]) VALUES ('" + highscore + "','" + player + "', '" + DateTime.Today + "')";

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(query, connection);
            try
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.Connection = connection;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception)
            {
                connection.Close();
            }
        }

    }
}
