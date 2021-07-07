using System;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using SimplexNoise;

public class Game {
	private readonly Clock clock;
	private readonly RenderWindow window;
	private readonly View view2D;
	private readonly View view3D;

	private readonly uint height;
	private readonly uint width;
	private float deltaTime;
	private float deltaTimeCounter;

	private Vector2f shipPos;
	private Vector2f shipVec;
	private double shipVel;
	private readonly CircleShape shipShape;
	private bool turnLeft;
	private bool turnRight;

	private readonly Sound ambience;
	private readonly SoundBuffer bird;
	private readonly Sound song;
	private readonly SoundBuffer music;
	private readonly short[] copyBuffer;

	public Game() {
		this.clock = new Clock();
		this.deltaTime = 0.0f;
		this.deltaTimeCounter = 0.0f;

		this.width = 1500;
		this.height = 750;

		this.bird = new SoundBuffer("C:\\Users\\Tangent\\Dropbox\\Programming\\RiderProjects\\VoxelSpace\\VoxelSpace\\bird.wav");
		this.ambience = new Sound(this.bird);
		this.ambience.Loop = true;
		this.ambience.Volume = 40f;

		this.music = new SoundBuffer("C:\\Users\\Alfred\\RiderProjects\\VoxelSpace\\VoxelSpace\\song.wav");
		this.song = new Sound(this.music);
		this.song.Loop = true;

		this.copyBuffer = this.music.Samples;

		this.view2D = new View(new FloatRect(0f, 0f, this.width / 2f, this.height));
		this.view2D.Viewport = new FloatRect(0f, 0f, 0.5f, 1f);
		this.view3D = new View(new FloatRect(0f, 0f, this.width / 2f, this.height));
		this.view3D.Viewport = new FloatRect(0.5f, 0f, 1f, 1f);

		ContextSettings contextSettings = new ContextSettings();
		//contextSettings.AntialiasingLevel = 8;

		this.window = new RenderWindow(new VideoMode(this.width, this.height), "Voxel Space", Styles.Default, contextSettings);
		//this.window.SetIcon();
		//this.window.SetFramerateLimit(5000);
		this.window.SetVerticalSyncEnabled(true);
		this.window.SetKeyRepeatEnabled(false);

		this.window.Closed += this.OnClose;
		this.window.Resized += this.OnResize;
		this.window.KeyPressed += this.OnKeyPressed;
		this.window.KeyReleased += this.OnKeyReleased;
		this.window.MouseMoved += this.OnMouseMoved;
		this.window.MouseButtonPressed += this.OnMousePressed;
		this.window.MouseButtonReleased += this.OnMouseReleased;
		this.window.MouseWheelScrolled += this.OnMouseScrolled;

		this.shipPos = new Vector2f(this.width / 4f, this.height / 2f);
		this.shipVec = new Vector2f(0f, -1f);
		this.shipVel = 0f;
		this.shipShape = new CircleShape(16);
		this.shipShape.Position = this.shipPos;
		this.shipShape.Origin = new Vector2f(16, 16);

		this.turnLeft = false;
		this.turnRight = false;
	}

	private void OnMouseScrolled(object? sender, MouseWheelScrollEventArgs e) {
	}

	private void OnMouseReleased(object? sender, MouseButtonEventArgs e) {
	}

	private void OnMousePressed(object? sender, MouseButtonEventArgs e) {
	}

	private void OnMouseMoved(object? sender, MouseMoveEventArgs e) {
	}

	private void OnKeyReleased(object? sender, KeyEventArgs e) {
		if (e.Code == Keyboard.Key.W)
			this.shipVel = 0f;
		else if (e.Code == Keyboard.Key.A)
			this.turnLeft = false;
		else if (e.Code == Keyboard.Key.D)
			this.turnRight = false;
		else if (e.Code == Keyboard.Key.S) this.shipVel = 0f;
	}

	private void OnKeyPressed(object? sender, KeyEventArgs e) {
		if (e.Code == Keyboard.Key.W)
			this.shipVel = 2f;
		else if (e.Code == Keyboard.Key.A)
			this.turnLeft = true;
		else if (e.Code == Keyboard.Key.D)
			this.turnRight = true;
		else if (e.Code == Keyboard.Key.S) this.shipVel = -2f;
	}

	private void OnResize(object? sender, SizeEventArgs e) {
	}

	private void OnClose(object? sender, EventArgs e) {
		this.window.Close();
	}

	public void Start() {
		this.Run();
	}

	public void Stop() {
		this.window.Close();
	}

	public void Run() {
		Random r = new Random();
		Noise.Seed = r.Next(0, 255);

		float gridCount = 256 + 128;
		float gridWidth = this.width / 2f / gridCount;
		float gridHeight = this.height / gridCount;

		uint lineCount = 128 + 64;
		uint lerpSteps = (uint) gridCount;
		double viewDistance = 256 + 128;
		double FOV = viewDistance * 0.5625;
		double FOVDecrease = FOV / lineCount;
		double viewDistanceDecrease = viewDistance / lineCount;

		Vertex[] textureMap = new Vertex[(uint) (gridCount * gridCount * 4)];
		VertexBuffer tm = new VertexBuffer((uint) (gridCount * gridCount * 4), PrimitiveType.Quads, VertexBuffer.UsageSpecifier.Stream);
		double[] heightMap = new double[(int) (gridCount * gridCount)];

		Vertex[] quadVertices = new Vertex[(int) (lineCount * lerpSteps * 4f)];
		VertexBuffer quadBuffer = new VertexBuffer((uint) (lineCount * lerpSteps * 4f), PrimitiveType.Quads, VertexBuffer.UsageSpecifier.Stream);

		VertexArray skyBox = new VertexArray(PrimitiveType.Quads, 4);
		Vertex v = new Vertex(new Vector2f(0, 0), new Color(Color.Black));

		v.Color = new Color(70, 161, 236, 255);
		v.Position = new Vector2f(0f, 0f);
		skyBox[0] = v;
		v.Position = new Vector2f(this.width / 2f, 0f);
		skyBox[1] = v;
		v.Color = new Color(239, 195, 128, 255);
		v.Position = new Vector2f(this.width / 2f, this.height);
		skyBox[2] = v;
		v.Position = new Vector2f(0f, this.height);
		skyBox[3] = v;

		VertexArray lineVertices = new VertexArray(PrimitiveType.Lines, lineCount * 2);

		// 2D HEIGHT AND COLOR MAP
		uint terrainIndex = 0;
		uint heightIndex = 0;

		for (float i = 0; i < gridCount; i += 1) {
			for (float j = 0; j < gridCount; j += 1) {
				v.Position.X = j * gridWidth;
				v.Position.Y = i * gridHeight;
				double heightMapH = Noise.CalcPixel2D((int) v.Position.X, (int) v.Position.Y, 0.007f);

				if (heightMapH < 64) {
					if (heightMapH < 24)
						v.Color = this.HSLToRGB(215.0, this.Lerp(heightMapH, 0f, 24, 0.55, 1), this.Lerp(heightMapH, 0f, 24, 0.55, 0.45), 255);
					else
						v.Color = this.HSLToRGB(215.0, 1.0, this.Lerp(heightMapH, 0.0, 64.0, 0.55, 0.7), 255);
					heightMapH = 64;
				}
				else if (heightMapH < 96) {
					v.Color = this.HSLToRGB(50.0, 1.0, this.Lerp(heightMapH, 64.0, 86.0, 0.64, 0.80), 255);
				}
				else {
					v.Color = this.HSLToRGB(this.Lerp(heightMapH, 96f, 255f, 115f, 130), this.Lerp(heightMapH, 86.0, 220.0, 1.0, 0.40), this.Lerp(heightMapH, 86.0, 220.0, 0.6, 0.25), 255);
					heightMapH += 20 * r.NextDouble();
					if (r.NextDouble() < 0.1) {
						v.Color = this.HSLToRGB(this.Lerp(heightMapH, 96f, 255f, 0f, 355f), 0.7, 0.57, 255);
						heightMapH += 20 * r.NextDouble();
					}
				}

				heightMap[heightIndex++] = heightMapH;

				textureMap[terrainIndex++] = v;
				v.Position.X += gridWidth;
				textureMap[terrainIndex++] = v;
				v.Position.Y += gridHeight;
				textureMap[terrainIndex++] = v;
				v.Position.X -= gridWidth;
				textureMap[terrainIndex++] = v;
			}
		}

		Shader shader = new Shader(null, null, "C:\\Users\\Alfred\\RiderProjects\\VoxelSpace\\VoxelSpace\\fra.frag");
		Shader shader2 = new Shader("C:\\Users\\Alfred\\RiderProjects\\VoxelSpace\\VoxelSpace\\ver.vert", null, "C:\\Users\\Alfred\\RiderProjects\\VoxelSpace\\VoxelSpace\\fra2.frag");

		RenderStates renderState = new RenderStates(shader);
		renderState.BlendMode = BlendMode.Alpha;
		RenderStates renderState2 = new RenderStates(shader2);
		renderState2.BlendMode = BlendMode.Alpha;

		shader.SetUniform("u_resolution", new Vector2f(this.width, this.height));
		shader2.SetUniform("u_resolution", new Vector2f(this.width, this.height));

		float angle = 0.0f;
		float counter = 0f;
		float targetHeight = 0f;
		float playerHeight = 0f;

		short[] ringBuffer = new short[(int) (0.3f * this.music.SampleRate) * this.music.ChannelCount];
		int ringBufferIndex = 0;

		//this.song.Play();
		this.ambience.Play();

		while (this.window.IsOpen) {
			this.deltaTime = this.clock.Restart().AsSeconds();
			this.deltaTimeCounter += this.deltaTime;
			shader.SetUniform("u_time", this.deltaTimeCounter / 4f);
			shader2.SetUniform("u_time", counter / 32);
			counter++;


			float abc = this.music.SampleRate * this.deltaTime * this.music.ChannelCount;
			float modu2 = this.song.PlayingOffset.AsSeconds() * this.music.SampleRate * this.music.ChannelCount;
			for (int i = 0; i < abc; i++) {
				int modu = this.Mod(ringBufferIndex, ringBuffer.Length - 1);
				short value = this.copyBuffer[this.Mod((int) (modu2 + i), this.copyBuffer.Length - 1)];
				ringBuffer[modu] = value;
				ringBufferIndex++;
			}

			float rms = this.AudioRMS(ringBuffer) * 2;

			//Console.WriteLine(1f / this.deltaTime);

			this.window.DispatchEvents();
			this.window.Clear(Color.Black);

			this.window.SetView(this.view2D);
			tm.Update(textureMap);
			this.window.Draw(tm);

			// 2D SHIP AND FOV LINES
			this.shipPos += this.shipVec * (float) this.shipVel;
			this.shipShape.Position = this.shipPos;
			this.window.Draw(this.shipShape);

			double f = FOV;
			double vd = viewDistance;
			v.Color = Color.White;

			for (uint i = 0; i < lineCount * 2; i += 2) {
				v.Position = this.shipPos + this.shipVec * (float) vd + this.NormalDirection(this.shipVec) * (float) -f;
				lineVertices[i] = v;
				v.Position = this.shipPos + this.shipVec * (float) vd + this.NormalDirection(this.shipVec) * (float) f;
				lineVertices[i + 1] = v;

				vd -= viewDistanceDecrease;
				f -= FOVDecrease;
			}

			this.window.Draw(lineVertices);

			// 3D SKY BOX
			this.window.SetView(this.view3D);
			this.window.Draw(skyBox, renderState);

			// 3D VOXEL SPACE
			double sx = Math.Floor(this.shipPos.X / gridWidth - 1);
			double sy = Math.Floor(this.shipPos.Y / gridHeight - 1);
			if (sx >= 0 && sx < gridCount && sy >= 0 && sy < gridCount)
				targetHeight = (float) heightMap[(int) (sy * gridCount + sx)];
			else
				targetHeight = 64;

			playerHeight += (targetHeight - playerHeight) / 10;
			shader2.SetUniform("player_height", playerHeight);

			uint horizonHeight = (uint) (this.height / 3f);
			float alphaFadeIn = 255f / lineCount * 4;
			uint lineIndex = 0;
			uint voxelIndex = 0;
			for (int i = 0; i < lineCount; i++) {
				Vector2f lineStart = lineVertices[lineIndex++].Position;
				Vector2f lineEnd = lineVertices[lineIndex++].Position;

				for (int j = 0; j < lerpSteps; j++) {
					int xCoord = (int) (Math.Floor(this.Lerp(j, 0, lerpSteps, lineStart.X, lineEnd.X) / gridWidth) - 1);
					int yCoord = (int) (Math.Floor(this.Lerp(j, 0, lerpSteps, lineStart.Y, lineEnd.Y) / gridHeight) - 1);

					if (xCoord >= 0 && xCoord < gridCount && yCoord >= 0 && yCoord < gridCount) {
						double heightM = heightMap[(int) (yCoord * gridCount + xCoord)];
						Color color = textureMap[(uint) (yCoord * gridCount + xCoord) * 4].Color;
						color.A = (byte) Math.Clamp(i * alphaFadeIn, 0, 255);

						v.Position.X = j * (this.width / 2 / lerpSteps);
						v.Position.Y = (float) this.Lerp(heightM, 0, 255, this.height, horizonHeight) + playerHeight;
						v.Color = color;
						quadVertices[voxelIndex++] = v;
						v.Position.X += this.width / 2f / lerpSteps;
						quadVertices[voxelIndex++] = v;
						v.Position.Y = this.height;
						quadVertices[voxelIndex++] = v;
						v.Position.X -= this.width / 2f / lerpSteps;
						quadVertices[voxelIndex++] = v;
					}
					else {
						v.Color = Color.Transparent;
						quadVertices[voxelIndex++] = v;
						v.Position.X += this.width / 2f / lerpSteps;
						quadVertices[voxelIndex++] = v;
						v.Position.Y = this.height;
						quadVertices[voxelIndex++] = v;
						v.Position.X -= this.width / 2f / lerpSteps;
						quadVertices[voxelIndex++] = v;
					}
				}
			}

			shader2.SetUniform("u_audio_average", rms);
			quadBuffer.Update(quadVertices);
			this.window.Draw(quadBuffer, renderState2);
			this.window.Display();

			if (this.turnLeft) {
				this.shipVec = this.RotatePoint(this.shipVec, 0.05f);
				this.NormalizeVector(ref this.shipVec);
				angle -= 0.01f;
				shader.SetUniform("player_angle", angle);
			}
			else if (this.turnRight) {
				this.shipVec = this.RotatePoint(this.shipVec, -0.05f);
				this.NormalizeVector(ref this.shipVec);
				angle += 0.01f;
				shader.SetUniform("player_angle", angle);
			}
		}
	}

	public double Lerp(double x, double x0, double x1, double y0, double y1) {
		return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
	}

	public float AudioRMS(short[] samples) {
		float sum = 0f;
		for (int i = 0; i < samples.Length; i++) {
			sum += Math.Abs(samples[i]);
		}

		return sum / samples.Length / short.MaxValue;
	}

	public float PointLength(Vector2f point) {
		return (float) Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
	}

	public void NormalizeVector(ref Vector2f point) {
		float length = this.PointLength(point);
		point.X /= length;
		point.Y /= length;
	}

	public float Angle(Vector2f v, bool radians) {
		float num = (float) Math.Atan2(v.Y, v.X);
		if (!radians)
			num *= 57.29578f;
		return num;
	}

	public Vector2f NormalDirection(Vector2f point) {
		this.NormalizeVector(ref point);
		return new Vector2f(-point.Y, point.X);
	}

	public Vector2f RotatePoint(Vector2f point, double radians) {
		float x = point.X;
		float y = point.Y;
		double qx = Math.Cos(radians) * x + Math.Sin(radians) * y;
		double qy = -Math.Sin(radians) * x + Math.Cos(radians) * y;

		return new Vector2f((float) qx, (float) qy);
	}

	public Vector2f RotateAroundPoint(Vector2f point, Vector2f origin, double radians) {
		float x = point.X;
		float y = point.Y;
		float ox = origin.X;
		float oy = origin.Y;
		double qx = ox + Math.Cos(radians) * (x - ox) + Math.Sin(radians) * (y - oy);
		double qy = oy + -Math.Sin(radians) * (x - ox) + Math.Cos(radians) * (y - oy);

		return new Vector2f((float) qx, (float) qy);
	}

	public Color HSLToRGB(double h, double s, double l, byte a) {
		byte r = 0;
		byte g = 0;
		byte b = 0;

		if (s == 0) {
			r = g = b = (byte) (l * 255.0);
		}
		else {
			float v1, v2;
			float hue = (float) ((float) h / 360.0);

			v2 = (float) (l < 0.5 ? l * (1.0 + s) : l + s - l * s);
			v1 = (float) (2.0 * l - v2);

			r = (byte) (255 * this.HueToRGB(v1, v2, (float) (hue + 1.0f / 3.0)));
			g = (byte) (255 * this.HueToRGB(v1, v2, hue));
			b = (byte) (255 * this.HueToRGB(v1, v2, (float) (hue - 1.0f / 3.0)));
		}

		return new Color(r, g, b, a);
	}

	private float HueToRGB(float v1, float v2, float vH) {
		if (vH < 0)
			vH += 1;

		if (vH > 1)
			vH -= 1;

		if (6 * vH < 1)
			return v1 + (v2 - v1) * 6 * vH;

		if (2 * vH < 1)
			return v2;

		if (3 * vH < 2)
			return v1 + (v2 - v1) * (2.0f / 3 - vH) * 6;

		return v1;
	}

	public int Mod(int x, int m) {
		int r = x % m;
		return r < 0 ? r + m : r;
	}
}