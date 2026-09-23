using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new RunnerForm());
	}
}

internal sealed class RunnerForm : Form
{
	private readonly System.Windows.Forms.Timer gameTimer = new System.Windows.Forms.Timer { Interval = 16 };
	private readonly List<Rectangle> obstacles = new List<Rectangle>();
	private readonly List<Point> coins = new List<Point>();
	private readonly Random random = new Random();
	private Rectangle player;
	private float playerVelocityY;
	private int groundY;
	private int worldOffset;
	private int distance;
	private int score;
	private int spawnCounter;
	private bool isJumping;
	private bool gameOver;

	public RunnerForm()
	{
		BackColor = Color.FromArgb(255, 222, 174);
		FormBorderStyle = FormBorderStyle.None;
		KeyPreview = true;
		ShowInTaskbar = true;
		StartPosition = FormStartPosition.CenterScreen;
		DoubleBuffered = true;

		Rectangle screenArea = Screen.PrimaryScreen?.WorkingArea ?? Screen.AllScreens[0].WorkingArea;
		Size = new Size((int)(screenArea.Width * 0.75), (int)(screenArea.Height * 0.75));

		Load += (_, _) => StartGame();
		KeyDown += HandleKeyDown;
		gameTimer.Tick += UpdateGame;
	}

	private void StartGame()
	{
		groundY = ClientSize.Height - 105;
		player = new Rectangle(150, groundY - 58, 48, 58);
		obstacles.Clear();
		coins.Clear();
		playerVelocityY = 0;
		worldOffset = 0;
		distance = 0;
		score = 0;
		spawnCounter = 80;
		isJumping = false;
		gameOver = false;
		gameTimer.Start();
		Invalidate();
	}

	private void HandleKeyDown(object? sender, KeyEventArgs eventArgs)
	{
		if (eventArgs.KeyCode == Keys.Escape)
		{
			Close();
		}
		else if (eventArgs.KeyCode == Keys.R && gameOver)
		{
			StartGame();
		}
		else if ((eventArgs.KeyCode == Keys.Space || eventArgs.KeyCode == Keys.Up) && !isJumping && !gameOver)
		{
			playerVelocityY = -15;
			isJumping = true;
		}
	}

	private void UpdateGame(object? sender, EventArgs eventArgs)
	{
		if (gameOver)
		{
			return;
		}

		const int speed = 7;
		playerVelocityY += 1;
		player.Y += (int)playerVelocityY;
		if (player.Bottom >= groundY)
		{
			player.Y = groundY - player.Height;
			playerVelocityY = 0;
			isJumping = false;
		}

		worldOffset = (worldOffset + speed) % 80;
		distance += speed;
		score = distance / 10;
		spawnCounter--;
		if (spawnCounter <= 0)
		{
			SpawnObjects();
			spawnCounter = random.Next(65, 115);
		}

		for (int index = obstacles.Count - 1; index >= 0; index--)
		{
			Rectangle obstacle = obstacles[index];
			obstacle.X -= speed;
			obstacles[index] = obstacle;
			if (obstacle.Right < 0)
			{
				obstacles.RemoveAt(index);
			}
			else if (player.IntersectsWith(obstacle))
			{
				gameOver = true;
				gameTimer.Stop();
			}
		}

		for (int index = coins.Count - 1; index >= 0; index--)
		{
			Point coin = coins[index];
			coin.X -= speed;
			coins[index] = coin;
			Rectangle coinBounds = new Rectangle(coin.X - 12, coin.Y - 12, 24, 24);
			if (coinBounds.Right < 0)
			{
				coins.RemoveAt(index);
			}
			else if (player.IntersectsWith(coinBounds))
			{
				score += 25;
				coins.RemoveAt(index);
			}
		}

		Invalidate();
	}

	private void SpawnObjects()
	{
		int x = ClientSize.Width + 30;
		int obstacleHeight = random.Next(34, 72);
		obstacles.Add(new Rectangle(x, groundY - obstacleHeight, random.Next(28, 45), obstacleHeight));
		if (random.Next(100) < 65)
		{
			coins.Add(new Point(x + 75, groundY - random.Next(85, 170)));
		}
	}

	protected override void OnPaint(PaintEventArgs eventArgs)
	{
		base.OnPaint(eventArgs);
		Graphics graphics = eventArgs.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;

		using (Brush skyBrush = new SolidBrush(Color.FromArgb(255, 224, 177)))
		using (Brush cloudBrush = new SolidBrush(Color.FromArgb(255, 244, 215)))
		using (Brush hillBrush = new SolidBrush(Color.FromArgb(206, 167, 109)))
		using (Brush groundBrush = new SolidBrush(Color.FromArgb(95, 164, 91)))
		{
			graphics.FillRectangle(skyBrush, ClientRectangle);
			graphics.FillEllipse(cloudBrush, 90, 105, 150, 35);
			graphics.FillEllipse(cloudBrush, ClientSize.Width - 270, 155, 190, 40);
			graphics.FillEllipse(hillBrush, -120, groundY - 100, 430, 160);
			graphics.FillEllipse(hillBrush, ClientSize.Width - 350, groundY - 125, 500, 185);
			graphics.FillRectangle(groundBrush, 0, groundY, ClientSize.Width, ClientSize.Height - groundY);
		}

		using (Pen trackPen = new Pen(Color.FromArgb(62, 126, 70), 4))
		{
			for (int x = -worldOffset; x < ClientSize.Width; x += 80)
			{
				graphics.DrawLine(trackPen, x, groundY + 24, x + 34, groundY + 24);
			}
		}

		foreach (Point coin in coins)
		{
			using (Brush coinBrush = new SolidBrush(Color.FromArgb(255, 190, 45)))
			using (Pen coinPen = new Pen(Color.FromArgb(224, 130, 18), 3))
			{
				graphics.FillEllipse(coinBrush, coin.X - 12, coin.Y - 12, 24, 24);
				graphics.DrawEllipse(coinPen, coin.X - 12, coin.Y - 12, 24, 24);
			}
		}

		foreach (Rectangle obstacle in obstacles)
		{
			using (Brush obstacleBrush = new SolidBrush(Color.FromArgb(177, 86, 63)))
			using (Pen obstaclePen = new Pen(Color.FromArgb(112, 56, 48), 3))
			{
				graphics.FillRectangle(obstacleBrush, obstacle);
				graphics.DrawRectangle(obstaclePen, obstacle);
			}
		}

		DrawPlayer(graphics);
		using (Font hudFont = new Font("Segoe UI", 16, FontStyle.Bold))
		using (Brush hudBrush = new SolidBrush(Color.FromArgb(75, 58, 43)))
		{
			graphics.DrawString("SCORE  " + score, hudFont, hudBrush, 28, 24);
			graphics.DrawString("SPACE / UP  JUMP", new Font("Segoe UI", 10, FontStyle.Bold), hudBrush, 30, 57);
		}

		if (gameOver)
		{
			using (Brush panelBrush = new SolidBrush(Color.FromArgb(220, 255, 248, 231)))
			using (Font titleFont = new Font("Segoe UI", 30, FontStyle.Bold))
			using (Font textFont = new Font("Segoe UI", 14, FontStyle.Regular))
			{
				Rectangle panel = new Rectangle(ClientSize.Width / 2 - 190, ClientSize.Height / 2 - 115, 380, 230);
				graphics.FillRectangle(panelBrush, panel);
				graphics.DrawString("RUN OVER", titleFont, Brushes.Brown, panel.X + 82, panel.Y + 35);
				graphics.DrawString("Score: " + score, textFont, Brushes.Brown, panel.X + 145, panel.Y + 100);
				graphics.DrawString("Press R to run again", textFont, Brushes.Brown, panel.X + 96, panel.Y + 145);
			}
		}
	}

	private void DrawPlayer(Graphics graphics)
	{
		using (Brush bodyBrush = new SolidBrush(Color.FromArgb(241, 103, 75)))
		using (Brush faceBrush = new SolidBrush(Color.FromArgb(255, 187, 113)))
		using (Pen outlinePen = new Pen(Color.FromArgb(105, 55, 49), 3))
		{
			graphics.FillEllipse(bodyBrush, player.X, player.Y + 10, player.Width, player.Height - 10);
			graphics.FillEllipse(faceBrush, player.X + 7, player.Y, player.Width - 14, 39);
			graphics.DrawEllipse(outlinePen, player.X + 7, player.Y, player.Width - 14, 39);
			graphics.FillEllipse(Brushes.Black, player.X + 17, player.Y + 14, 5, 7);
			graphics.FillEllipse(Brushes.Black, player.X + 31, player.Y + 14, 5, 7);
		}
	}
}
