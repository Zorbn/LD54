using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD54;

public class Game1 : Game
{
    private const float MaxFrameTime = 0.1f;

    private static readonly Color BackgroundColor = new(110, 39, 39);

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _atlas;
    private TileMap _tileMap;
    private Camera? _camera;
    private readonly Input _input = new();
    private readonly Player _player = new();
    private bool _onEndScreen;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 500;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.ClientSizeChanged += (_, _) => OnResize();
        Window.AllowUserResizing = true;

        IsFixedTimeStep = false;
    }

    private void OnResize()
    {
        _camera?.Resize(Window.ClientBounds.Width, Window.ClientBounds.Height);
    }

    protected override void Initialize()
    {
        _tileMap = new TileMap();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _atlas = Content.Load<Texture2D>("atlas");
        _camera = new Camera(_spriteBatch, _atlas);
        _tileMap.Generate(_player, _camera);
        OnResize();
    }

    protected override void Update(GameTime gameTime)
    {
        Debug.Assert(_camera is not null);

        _input.Update(Keyboard.GetState());
        if (_input.WasKeyPressed(Keys.Escape)) Exit();

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (deltaTime > MaxFrameTime) return;

        if (_onEndScreen)
        {
            if (_input.WasKeyPressed(Keys.Space))
            {
                _onEndScreen = false;
                _player.Reset();
                _tileMap.Generate(_player, _camera);
            }

            return;
        }

        _player.Update(deltaTime, _input, _tileMap, _camera);
        _tileMap.Update(deltaTime);

        if (_player.WasSpotted || _input.WasKeyPressed(Keys.R))
        {
            _onEndScreen = true;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        Debug.Assert(_camera is not null);

        GraphicsDevice.Clear(BackgroundColor);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, sortMode: SpriteSortMode.FrontToBack);

        _player.Draw(_camera);
        _tileMap.Draw(_camera, _player);

        if (_onEndScreen)
        {
            _camera.Draw(Vector2.Zero, Sprite.EndScreen.Rectangle, 0.99f);
            TextRenderer.Draw(_camera, 107, 22, $"${_player.Money}");
        }
        else
        {
            TextRenderer.Draw(_camera, 1, 4, $"${_player.Money}");
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}