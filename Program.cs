
using Foster.Framework;
using System.Numerics;
using static Foster.Framework.Batcher;

class Program
{
	public static void Main()
	{
		App.Register<Game>();
		App.Run("Ship FireTest", 1280, 720);
	}
}
public class Projectile
{
	public Vector2 position;
	public Vector2 speed;
	public bool visible;
	public int fireLevel;
	public int index;

	public Projectile(Vector2 position, bool visible)
	{
		this.position = position;
		this.visible = visible;
		//defaults
		this.speed = Vector2.Zero;
		fireLevel = 1;
		index = 0;
	}

	public void SpawnProjectile(Vector2 pos, int level, int index)
	{
		this.position = pos;
		this.index = index;
		this.fireLevel = level;
	}
}
class FireLevel
{
	public List<Vector2> positions = new List<Vector2>();
	public List<Vector2> directions = new List<Vector2>();

	public FireLevel(List<Vector2> pos, List<Vector2> dir)
	{
		this.positions = pos;
		this.directions = dir;
	}
}

class Game : Module
{
	private const float Acceleration = 1500;
	private const float Friction = 800;
	private const float MaxSpeed = 800;

	private readonly Batcher batch = new();
	private Texture shiptexture = new Texture(new Image(128, 128, Color.Blue));
	private Texture projectileTexture = new Texture(new Image(128, 128, Color.Blue));
	private SpriteFont font = null!;

	private Vector2 shipPos = new(App.WidthInPixels/2, App.HeightInPixels/2);
	private Vector2 shipSpeed = new();
	private Vector2 textureOffset = new();
	private Vector2 bulletSpawnPoint = new();

	private int fireLevel = 0;
	public float fireRate = 2f;

	private float fireTimer = 0;
	private float fireInterval =0;
	private bool readyToShoot;

	private Projectile[] projectiles = new Projectile[50];
	private int projectileCount = 0;
	private float projectileSpeed;

	
	public override void Startup()
	{
		using var shipImage = new Image(Path.Join("Asset", "ship3.png"));
		shipImage.Premultiply();
		shiptexture = new Texture(shipImage);
		textureOffset = new Vector2(shiptexture.Width, shiptexture.Height) / 2;

		using var projectileImage = new Image(Path.Join("Asset", "Projectile.png"));
		projectileImage.Premultiply();
		projectileTexture = new Texture(projectileImage);

		font = new SpriteFont(Path.Join("Asset", "monogram.ttf"), 32);

		//spawning projectile Pool
		for (int i = 0; i < 50; i++)
		{
			projectiles[i] = new Projectile(Vector2.One * 100, false);
		}

		fireInterval = (float)1 / fireRate;
		projectileSpeed = 600;
		bulletSpawnPoint = new Vector2(0,shiptexture.Height/2);

		fireLevel = 1;
	}
	
	public override void Update()
	{
		App.Title = $"Ship FireTest {App.Width}x{App.Height} : {App.WidthInPixels}x{App.HeightInPixels}";

		ShipInput();
		HandleFiring();
	}

	private void HandleFiring()
	{
		if (Input.Keyboard.Pressed(Keys.Z))
			ModifyFireRate(true);
		if (Input.Keyboard.Pressed(Keys.X))
			ModifyFireRate(false);

		if (Input.Keyboard.Pressed(Keys.Q))
			ModifyFireLevel(true);
		if (Input.Keyboard.Pressed(Keys.E))
			ModifyFireLevel(false);

		if (fireTimer >= fireInterval)
		{
			//shoot
			fireTimer = 0;
			readyToShoot = true;
		}
		else
		{
			fireTimer += Time.Delta;
		}

		if (Input.Keyboard.Pressed(Keys.Space) && readyToShoot)
			SpawnFromPool(fireLevel);

		projectileCount = 0;
		//update projectile position and deactivate them
		foreach (var projectile in projectiles)
		{
			if (!projectile.visible)
				continue;


			if (projectile.position.Y <= 0)
			{
				projectile.visible = false;
			}
			else
			{
				projectileCount++;
				projectile.position += CustomFireLevel(projectile.fireLevel).directions[projectile.index] * Time.Delta;
			}
		}

		//Console.WriteLine("pcount - " + projectileCount+ ", fireRate - "+fireRate+", fireLevel - " + fireLevel);
	}

	private void ShipInput()
	{
		if (Input.Keyboard.Down(Keys.A))
			shipSpeed.X -= Acceleration * Time.Delta;
		if (Input.Keyboard.Down(Keys.D))
			shipSpeed.X += Acceleration * Time.Delta;
		if (Input.Keyboard.Down(Keys.W))
			shipSpeed.Y -= Acceleration * Time.Delta;
		if (Input.Keyboard.Down(Keys.S))
			shipSpeed.Y += Acceleration * Time.Delta;

		if (!Input.Keyboard.Down(Keys.Left, Keys.Right))
			shipSpeed.X = Calc.Approach(shipSpeed.X, 0, Time.Delta * Friction);
		if (!Input.Keyboard.Down(Keys.Up, Keys.Down))
			shipSpeed.Y = Calc.Approach(shipSpeed.Y, 0, Time.Delta * Friction);

		if (Input.Keyboard.Pressed(Keys.F4))
			App.Fullscreen = !App.Fullscreen;



		if (shipSpeed.Length() > MaxSpeed)
			shipSpeed = shipSpeed.Normalized() * MaxSpeed;

		shipPos += shipSpeed * Time.Delta;

		shipPos = Vector2.Clamp(shipPos, Vector2.Zero + textureOffset, new Vector2(App.Width, App.Height) - textureOffset);
	}

	private FireLevel CustomFireLevel(int level)
	{
		//custom position and directions from ships texture size for different fire levels

		switch (level)
		{
			case 1:

				FireLevel level_1 = new FireLevel(
					new List<Vector2> { new Vector2(0, -shiptexture.Height / 2) }, //positon
					new List<Vector2> { new Vector2(0, -projectileSpeed) }		 //direction
					);
				return level_1;
			case 2:

				FireLevel level_2 = new FireLevel(
					new List<Vector2> { new Vector2(-shiptexture.Width / 3, -shiptexture.Height / 2),
										new Vector2(shiptexture.Width / 3,-shiptexture.Height / 2 ) },
					new List<Vector2> { new Vector2(0, -projectileSpeed),
										new Vector2(0, -projectileSpeed) }
					);
				return level_2;
			case 3:
				FireLevel level_3 = new FireLevel(
					new List<Vector2> { new Vector2(-shiptexture.Width / 3, -shiptexture.Height / 2),
										new Vector2(0, -shiptexture.Height),
										new Vector2(shiptexture.Width / 3,-shiptexture.Height / 2 ) },
					new List<Vector2> { new Vector2(0, -projectileSpeed),
										new Vector2(0, -projectileSpeed),
										new Vector2(0, -projectileSpeed)}
					);
				return level_3;
			case 4:
				FireLevel level_4 = new FireLevel(
					new List<Vector2> { new Vector2(-shiptexture.Width / 2, -shiptexture.Height / 2),
										new Vector2(-shiptexture.Width / 4, -shiptexture.Height/2),
										new Vector2(shiptexture.Width / 4, -shiptexture.Height /2),
										new Vector2(shiptexture.Width/2,-shiptexture.Height / 2 ) },
					new List<Vector2> { new Vector2(-200, -projectileSpeed),
										new Vector2(-100, -projectileSpeed),
										new Vector2(100, -projectileSpeed),
										new Vector2(200, -projectileSpeed)}
					);
				return level_4;
			case 5:
				FireLevel level_5 = new FireLevel(
					new List<Vector2> { new Vector2(-shiptexture.Width / 2, -shiptexture.Height / 2),
										new Vector2(-shiptexture.Width / 4, -shiptexture.Height/2),
										new Vector2(0, -shiptexture.Height / 2),
										new Vector2(shiptexture.Width / 4, -shiptexture.Height /2),
										new Vector2(shiptexture.Width/2,-shiptexture.Height / 2 ) },
					new List<Vector2> { new Vector2(-200, -projectileSpeed),
										new Vector2(-100, -projectileSpeed),
										new Vector2(0, -projectileSpeed),
										new Vector2(100, -projectileSpeed),
										new Vector2(200, -projectileSpeed)}
					);
				return level_5;
			default:
				return null;

		}
	}
	public override void Render()
	{
		Graphics.Clear(0x44aa77);

		//draw ship
		batch.PushMatrix(
			shipPos,
			Vector2.One,
			new Vector2(shiptexture.Width, shiptexture.Height) / 2, 0
			);
		
		batch.Image(shiptexture, Vector2.Zero, Color.White);
		batch.PopMatrix();

		batch.Text(font, $" FireLevel - {fireLevel}\n FireRate - {fireRate}\n PoolCount - {projectileCount}", new(8, -2), Color.Black);
		batch.Text(font, " Space - shoot \n Q/E - modify firelevel \n Z/X - modify firerate", new(8, App.HeightInPixels - 90), Color.Black);

		//draw projectiles
		foreach (var projectile in projectiles)
		{
			if (!projectile.visible)
				continue;

			batch.PushMatrix(projectile.position,
				Vector2.One,
			new Vector2(projectileTexture.Width, projectileTexture.Height) / 2, 0
			);
			batch.Image(projectileTexture, Vector2.Zero, Color.White);
			batch.PopMatrix();
		}

		batch.Render();
		batch.Clear();
	}

	public void SpawnFromPool(int level)
	{
		int count = 0;
		//spawn bullet
		readyToShoot = false;
		foreach(var projectile in projectiles)
		{
			if(!projectile.visible)
			{
				if (count >= level)
				{
					return;
				}

				projectile.visible = true;
				var offsetPos = shipPos + CustomFireLevel(level).positions[count];
				projectile.SpawnProjectile(offsetPos, level, count);
				count++;
			}
		}
	}
	
	public void ModifyFireRate(bool value)
	{
		fireRate = value ? fireRate+1 : fireRate-1;
		fireRate = Math.Clamp(fireRate, 1, 8);//max fireRate - 8
		fireInterval = (float)1 / fireRate;
	}
	private void ModifyFireLevel(bool value)
	{
		fireLevel = value ? fireLevel + 1 : fireLevel - 1;
		fireLevel = Math.Clamp(fireLevel, 1, 5); //max fireLevel -5
	}
}
